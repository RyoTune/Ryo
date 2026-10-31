using System.Buffers.Binary;
using System.Text;

namespace Ryo.Reloaded.CRI.CriAtom.Models;

public sealed record HcaInfo(byte Channels, uint SampleRate, uint SampleCount, uint? LoopStart, uint? LoopEnd)
{
    public uint DurationMilliseconds => checked((uint)((ulong)SampleCount * 1000 / SampleRate));

    public void Validate()
    {
        if (Channels is 0 or > 16 || SampleRate == 0 || SampleCount == 0)
            throw new InvalidDataException("Invalid HCA channel count, rate or sample count.");
        if (LoopStart.HasValue != LoopEnd.HasValue ||
            (LoopStart.HasValue && (LoopStart >= LoopEnd || LoopEnd > SampleCount)))
            throw new InvalidDataException("Invalid HCA loop range.");
    }

    public static HcaInfo Read(string path) => Read(File.ReadAllBytes(path));

    public static HcaInfo Read(byte[] b)
    {
        string Tag(int p) => Encoding.ASCII.GetString(b.AsSpan(p, 4).ToArray().Select(x => (byte)(x & 0x7f)).ToArray());
        if (b.Length < 8 || Tag(0) != "HCA\0") throw new InvalidDataException("Missing HCA header.");
        int length = BinaryPrimitives.ReadUInt16BigEndian(b.AsSpan(6));
        if (length < 26 || length > b.Length) throw new InvalidDataException("Truncated HCA header.");
        ushort crc = 0;
        foreach (byte value in b.AsSpan(0, length - 2))
        {
            crc ^= (ushort)(value << 8);
            for (int bit = 0; bit < 8; bit++)
                crc = unchecked((ushort)((crc << 1) ^ ((crc & 0x8000) != 0 ? 0x8005 : 0)));
        }

        if (crc != BinaryPrimitives.ReadUInt16BigEndian(b.AsSpan(length - 2)))
            throw new InvalidDataException("HCA header checksum mismatch.");
        byte channels = 0;
        uint rate = 0, frames = 0;
        ushort delay = 0, padding = 0;
        uint? loopFirst = null, loopLast = null;
        ushort pre = 0, post = 0, frameSize = 0;
        bool fmt = false, codec = false;
        int p = 8;
        while (p < length - 2)
        {
            if (p + 4 > length - 2) throw new InvalidDataException("Truncated HCA chunk tag.");
            string tag = Tag(p);
            int size = tag switch
            {
                "fmt\0" => 16, "comp" => 16, "dec\0" => 12, "loop" => 16,
                "ath\0" or "ciph" => 6, "rva\0" or "vbr\0" => 8,
                "comm" when p + 5 <= length - 2 => 5 + b[p + 4],
                "pad\0" => length - 2 - p,
                _ => throw new NotSupportedException($"Unknown HCA chunk '{tag}' at {p}.")
            };
            if (p + size > length - 2) throw new InvalidDataException("Truncated HCA chunk.");
            var chunk = b.AsSpan(p, size);
            if (tag == "fmt\0")
            {
                if (fmt) throw new InvalidDataException("Duplicate HCA fmt chunk.");
                fmt = true;
                channels = chunk[4];
                rate = (uint)(chunk[5] << 16 | chunk[6] << 8 | chunk[7]);
                frames = BinaryPrimitives.ReadUInt32BigEndian(chunk[8..]);
                delay = BinaryPrimitives.ReadUInt16BigEndian(chunk[12..]);
                padding = BinaryPrimitives.ReadUInt16BigEndian(chunk[14..]);
            }
            else if (tag is "comp" or "dec\0")
            {
                if (codec) throw new InvalidDataException("Duplicate HCA codec chunk.");
                codec = true;
                frameSize = BinaryPrimitives.ReadUInt16BigEndian(chunk[4..]);
            }
            else if (tag == "loop")
            {
                if (loopFirst.HasValue) throw new InvalidDataException("Duplicate HCA loop chunk.");
                loopFirst = BinaryPrimitives.ReadUInt32BigEndian(chunk[4..]);
                loopLast = BinaryPrimitives.ReadUInt32BigEndian(chunk[8..]);
                pre = BinaryPrimitives.ReadUInt16BigEndian(chunk[12..]);
                post = BinaryPrimitives.ReadUInt16BigEndian(chunk[14..]);
            }

            p += size;
        }

        if (!fmt || !codec || frameSize == 0 || frames == 0 ||
            (ulong)length + (ulong)frames * frameSize > (ulong)b.Length)
            throw new InvalidDataException("Incomplete HCA audio payload or required header chunks.");
        long samples = (long)frames * 1024 - delay - padding;
        if (samples <= 0) throw new InvalidDataException("Invalid HCA sample count.");
        if (loopFirst.HasValue && (loopFirst > loopLast || loopLast >= frames || pre >= 1024 || post >= 1024))
            throw new InvalidDataException("Invalid HCA loop frames.");
        var info = new HcaInfo(channels, rate, checked((uint)samples),
            loopFirst.HasValue ? checked((uint)((long)loopFirst.Value * 1024 + pre - delay)) : null,
            loopLast.HasValue ? checked((uint)(((long)loopLast.Value + 1) * 1024 - post - delay)) : null);
        info.Validate();
        return info;
    }
}