using System.Buffers.Binary;

namespace Ryo.Reloaded.CRI.CriAtom.Models;

public sealed record AcbCommand(ushort Opcode, byte[] Parameters);
public static class AcbCommands
{
    public const ushort Category = 65, VolumeBus = 111, NoteOn = 2000;
    public static byte[] U16(params ushort[] values)
    {
        byte[] b = new byte[checked(values.Length * 2)];
        for (int i = 0; i < values.Length; i++) BinaryPrimitives.WriteUInt16BigEndian(b.AsSpan(i * 2), values[i]);
        return b;
    }
    public static byte[] U32(params uint[] values)
    {
        byte[] b = new byte[checked(values.Length * 4)];
        for (int i = 0; i < values.Length; i++) BinaryPrimitives.WriteUInt32BigEndian(b.AsSpan(i * 4), values[i]);
        return b;
    }
    public static byte[] Encode(IEnumerable<AcbCommand> commands, bool terminate = true)
    {
        using var s = new MemoryStream();
        foreach (var cmd in commands)
        {
            if (cmd.Opcode == 0 || cmd.Parameters.Length > 255) throw new ArgumentException("Invalid ACB command.");
            s.Write(U16(cmd.Opcode)); s.WriteByte((byte)cmd.Parameters.Length); s.Write(cmd.Parameters);
        }
        if (terminate) s.Write(new byte[3]);
        return s.ToArray();
    }
    public static List<AcbCommand> Decode(byte[] bytes)
    {
        var result = new List<AcbCommand>(); int p = 0;
        while (p < bytes.Length)
        {
            if (bytes.Length - p < 3) throw new InvalidDataException("Truncated ACB command header.");
            ushort op = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(p)); int count = bytes[p + 2]; p += 3;
            if (count > bytes.Length - p) throw new InvalidDataException("Truncated ACB command payload.");
            if (op == 0)
            {
                if (count != 0 || bytes.AsSpan(p).ContainsAnyExcept((byte)0))
                    throw new InvalidDataException("Unexpected data after command terminator.");
                break;
            }
            result.Add(new(op, bytes.AsSpan(p, count).ToArray())); p += count;
        }
        return result;
    }
}
