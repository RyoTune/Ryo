using System.Buffers.Binary;
using System.Text;

namespace Ryo.Reloaded.CRI.CriAtom.Models;

public enum UtfType : byte
{
    UInt8,
    Int8,
    UInt16,
    Int16,
    UInt32,
    Int32,
    UInt64,
    Int64,
    Single,
    Double,
    String,
    Data,
    Guid
}

public enum UtfStorage : byte
{
    Zero = 0x10,
    Constant = 0x30,
    Row = 0x50
}

public sealed record UtfColumn(string Name, UtfType Type, UtfStorage Storage = UtfStorage.Row, object? Constant = null);

/// <summary>Big-endian CRI @UTF table. Nested data is deliberately kept opaque until requested.</summary>
public sealed class UtfTable(string name, byte encoding = 1)
{
    public string Name { get; set; } = name;
    public byte EncodingId { get; } = encoding;
    public List<UtfColumn> Columns { get; } = [];
    public List<object[]> Rows { get; } = [];

    public int ColumnIndex(string name) => Columns.FindIndex(c => c.Name == name) is var i && i >= 0
        ? i
        : throw new InvalidDataException($"Table '{Name}' has no column '{name}'.");

    public bool Has(string name) => Columns.Any(c => c.Name == name);
    public object Get(int row, string name) => Rows[row][ColumnIndex(name)];
    public uint Number(int row, string name) => Convert.ToUInt32(Get(row, name));
    public string Text(int row, string name) => (string)Get(row, name);
    public byte[] Data(int row, string name) => (byte[])Get(row, name);

    public void Set(int row, string name, object value)
    {
        int i = ColumnIndex(name);
        Normalize(Columns[i].Type, value); // Validate before changing storage or rows.
        Columns[i] = Columns[i] with { Storage = UtfStorage.Row, Constant = null };
        Rows[row][i] = value;
    }

    public int AddRow(IReadOnlyDictionary<string, object> values)
    {
        foreach (var key in values.Keys) ColumnIndex(key); // No silently ignored fields.
        var row = Columns.Select(c => values.TryGetValue(c.Name, out var v) ? v :
            c.Storage == UtfStorage.Constant ? c.Constant! : Zero(c.Type)).ToArray();
        for (int i = 0; i < Columns.Count; i++) Normalize(Columns[i].Type, row[i]);
        for (int i = 0; i < Columns.Count; i++)
        {
            var c = Columns[i];
            object prior = c.Storage == UtfStorage.Constant ? c.Constant! : Zero(c.Type);
            if (c.Storage != UtfStorage.Row && !Equal(prior, row[i]))
                Columns[i] = c with { Storage = UtfStorage.Row, Constant = null };
        }

        Rows.Add(row);
        return Rows.Count - 1;
    }

    public static bool Equal(object a, object b) => a is byte[] x && b is byte[] y
        ? x.SequenceEqual(y)
        : a is float or double || b is float or double
            ? Convert.ToDouble(a).Equals(Convert.ToDouble(b))
            : a is IConvertible && b is IConvertible && a is not string && b is not string
                ? Convert.ToDecimal(a) == Convert.ToDecimal(b)
                : Equals(a, b);

    public static object Zero(UtfType type) => type switch
    {
        UtfType.String => "", UtfType.Data => Array.Empty<byte>(), UtfType.Guid => new byte[16], _ => 0
    };

    private static int Width(UtfType t) => t switch
    {
        UtfType.UInt8 or UtfType.Int8 => 1, UtfType.UInt16 or UtfType.Int16 => 2,
        UtfType.UInt32 or UtfType.Int32 or UtfType.Single or UtfType.String => 4,
        UtfType.UInt64 or UtfType.Int64 or UtfType.Double or UtfType.Data => 8,
        UtfType.Guid => 16, _ => throw new InvalidDataException("Unknown UTF type.")
    };

    private static Encoding TextEncoding(byte id)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return id switch
        {
            0 => Encoding.GetEncoding(932, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback),
            1 => new UTF8Encoding(false, true), _ => throw new NotSupportedException($"UTF encoding {id}.")
        };
    }

    private static object Normalize(UtfType type, object value) => type switch
    {
        UtfType.UInt8 => Convert.ToByte(value), UtfType.Int8 => Convert.ToSByte(value),
        UtfType.UInt16 => Convert.ToUInt16(value), UtfType.Int16 => Convert.ToInt16(value),
        UtfType.UInt32 => Convert.ToUInt32(value), UtfType.Int32 => Convert.ToInt32(value),
        UtfType.UInt64 => Convert.ToUInt64(value), UtfType.Int64 => Convert.ToInt64(value),
        UtfType.Single => Convert.ToSingle(value), UtfType.Double => Convert.ToDouble(value),
        UtfType.String when value is string s && !s.Contains('\0') => s,
        UtfType.Data when value is byte[] => value,
        UtfType.Guid when value is byte[] { Length: 16 } => value,
        _ => throw new InvalidDataException($"Invalid value for {type}.")
    };

    public static UtfTable Read(byte[] input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Read((ReadOnlySpan<byte>)input);
    }

    /// <summary>Parses directly from the input span. Returned strings and blobs own their data.</summary>
    public static UtfTable Read(ReadOnlySpan<byte> input)
    {
        if (input.Length < 32 || !input[..4].SequenceEqual("@UTF"u8))
            throw new InvalidDataException("Missing @UTF header.");
        int end = checked((int)BinaryPrimitives.ReadUInt32BigEndian(input[4..]) + 8);
        if (end > input.Length || end < 32) throw new InvalidDataException("Invalid UTF table length.");
        var b = input[..end];
        if (b[8] != 0) throw new NotSupportedException("Unsupported UTF header variant.");
        int rowStart = BinaryPrimitives.ReadUInt16BigEndian(b.Slice(10)) + 8;
        int strings = checked((int)BinaryPrimitives.ReadUInt32BigEndian(b.Slice(12)) + 8);
        int data = checked((int)BinaryPrimitives.ReadUInt32BigEndian(b.Slice(16)) + 8);
        int ncols = BinaryPrimitives.ReadUInt16BigEndian(b.Slice(24));
        int stride = BinaryPrimitives.ReadUInt16BigEndian(b.Slice(26));
        int nrows = checked((int)BinaryPrimitives.ReadUInt32BigEndian(b.Slice(28)));
        if (rowStart < 32 || strings < rowStart || data < strings || data > end ||
            (long)rowStart + (long)nrows * stride > strings)
            throw new InvalidDataException("Invalid UTF section boundaries.");
        var enc = TextEncoding(b[9]);

        static string Str(ReadOnlySpan<byte> b, int strings, int data, Encoding enc, uint offset)
        {
            long p = (long)strings + offset;
            if (p < strings || p >= data) throw new InvalidDataException("Invalid UTF string offset.");
            int z = b.Slice((int)p, data - (int)p).IndexOf((byte)0);
            if (z < 0) throw new InvalidDataException("Unterminated UTF string.");
            return enc.GetString(b.Slice((int)p, z));
        }

        static object Value(ReadOnlySpan<byte> b, int strings, int data, int end, Encoding enc,
            UtfType type, ref int p, int bound)
        {
            int w = Width(type);
            if (p < 0 || p > bound - w) throw new InvalidDataException("Truncated UTF value.");
            var s = b.Slice(p, w);
            p += w;
            switch (type)
            {
                case UtfType.UInt8: return s[0];
                case UtfType.Int8: return unchecked((sbyte)s[0]);
                case UtfType.UInt16: return BinaryPrimitives.ReadUInt16BigEndian(s);
                case UtfType.Int16: return BinaryPrimitives.ReadInt16BigEndian(s);
                case UtfType.UInt32: return BinaryPrimitives.ReadUInt32BigEndian(s);
                case UtfType.Int32: return BinaryPrimitives.ReadInt32BigEndian(s);
                case UtfType.UInt64: return BinaryPrimitives.ReadUInt64BigEndian(s);
                case UtfType.Int64: return BinaryPrimitives.ReadInt64BigEndian(s);
                case UtfType.Single: return BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32BigEndian(s));
                case UtfType.Double: return BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64BigEndian(s));
                case UtfType.String: return Str(b, strings, data, enc, BinaryPrimitives.ReadUInt32BigEndian(s));
                case UtfType.Guid: return s.ToArray();
                case UtfType.Data:
                    uint offset = BinaryPrimitives.ReadUInt32BigEndian(s);
                    uint length = BinaryPrimitives.ReadUInt32BigEndian(s[4..]);
                    if (length == 0) return Array.Empty<byte>();
                    long at = (long)data + offset;
                    if (at > end || length > end - at) throw new InvalidDataException("Invalid UTF blob range.");
                    return b.Slice((int)at, (int)length).ToArray();
                default: throw new InvalidDataException("Unknown UTF type.");
            }
        }

        var table = new UtfTable(Str(b, strings, data, enc, BinaryPrimitives.ReadUInt32BigEndian(b.Slice(20))), b[9]);
        int cursor = 32;
        for (int i = 0; i < ncols; i++)
        {
            if (cursor >= rowStart) throw new InvalidDataException("Truncated UTF column definition.");
            byte flags = b[cursor++];
            var type = (UtfType)(flags & 15);
            var storage = (UtfStorage)(flags & 0xf0);
            Width(type);
            if (storage is not (UtfStorage.Zero or UtfStorage.Constant or UtfStorage.Row))
                throw new NotSupportedException($"UTF column flags 0x{flags:x2}.");
            string name = (string)Value(b, strings, data, end, enc, UtfType.String, ref cursor, rowStart);
            if (table.Has(name)) throw new InvalidDataException($"Duplicate UTF column '{name}'.");
            object? constant = storage == UtfStorage.Constant ? Value(b, strings, data, end, enc, type, ref cursor, rowStart) : null;
            table.Columns.Add(new(name, type, storage, constant));
        }

        int schemaStride = table.Columns.Where(c => c.Storage == UtfStorage.Row).Sum(c => Width(c.Type));
        if (schemaStride != stride)
        {
            // Some ACB headers (including P3R) retain a stale, shorter RowLength.
            // Recover only a single row whose schema ends exactly at the strings section.
            if (nrows == 1 && stride < schemaStride && strings - rowStart == schemaStride)
                stride = schemaStride;
            else
                throw new InvalidDataException("UTF row size does not match column definitions.");
        }
        for (int r = 0; r < nrows; r++)
        {
            cursor = checked(rowStart + r * stride);
            var row = new object[ncols];
            for (int c = 0; c < ncols; c++)
            {
                var col = table.Columns[c];
                row[c] = col.Storage switch
                {
                    UtfStorage.Row => Value(b, strings, data, end, enc, col.Type, ref cursor, rowStart + (r + 1) * stride),
                    UtfStorage.Constant => col.Constant!, _ => Zero(col.Type)
                };
            }

            table.Rows.Add(row);
        }

        return table;
    }

    public byte[] Write()
    {
        var enc = TextEncoding(EncodingId);
        if (Columns.Select(c => c.Name).Distinct().Count() != Columns.Count)
            throw new InvalidDataException("Duplicate UTF columns.");
        foreach (var row in Rows)
        {
            if (row.Length != Columns.Count) throw new InvalidDataException("Wrong UTF row width.");
            for (int i = 0; i < row.Length; i++)
            {
                Normalize(Columns[i].Type, row[i]);
                var c = Columns[i];
                if (c.Storage != UtfStorage.Row &&
                    !Equal(row[i], c.Storage == UtfStorage.Constant ? c.Constant! : Zero(c.Type)))
                    throw new InvalidDataException(
                        $"Column '{c.Name}' changed without promoting its storage; use Set().");
            }
        }

        using var strings = new MemoryStream();
        using var data = new MemoryStream();

        uint AddString(string s)
        {
            Normalize(UtfType.String, s);
            uint offset = checked((uint)strings.Position);
            strings.Write(enc.GetBytes(s));
            strings.WriteByte(0);
            return offset;
        }

        uint nameOffset = AddString(Name);

        void WriteValue(BinaryWriter w, UtfType t, object value)
        {
            object v = Normalize(t, value);
            Span<byte> s = stackalloc byte[16];
            switch (t)
            {
                case UtfType.UInt8: s[0] = (byte)v; break;
                case UtfType.Int8: s[0] = unchecked((byte)(sbyte)v); break;
                case UtfType.UInt16: BinaryPrimitives.WriteUInt16BigEndian(s, (ushort)v); break;
                case UtfType.Int16: BinaryPrimitives.WriteInt16BigEndian(s, (short)v); break;
                case UtfType.UInt32: BinaryPrimitives.WriteUInt32BigEndian(s, (uint)v); break;
                case UtfType.Int32: BinaryPrimitives.WriteInt32BigEndian(s, (int)v); break;
                case UtfType.UInt64: BinaryPrimitives.WriteUInt64BigEndian(s, (ulong)v); break;
                case UtfType.Int64: BinaryPrimitives.WriteInt64BigEndian(s, (long)v); break;
                case UtfType.Single:
                    BinaryPrimitives.WriteInt32BigEndian(s, BitConverter.SingleToInt32Bits((float)v)); break;
                case UtfType.Double:
                    BinaryPrimitives.WriteInt64BigEndian(s, BitConverter.DoubleToInt64Bits((double)v)); break;
                case UtfType.String: BinaryPrimitives.WriteUInt32BigEndian(s, AddString((string)v)); break;
                case UtfType.Guid: ((byte[])v).CopyTo(s); break;
                case UtfType.Data:
                    var blob = (byte[])v;
                    // CRI-authored ACB blobs occupy zero-padded 32-byte slots.
                    // Some native readers inspect a full header even for a legacy
                    // 16-byte StreamAwbHash. Packing its following table immediately
                    // after it makes those bytes look like a nonzero stream-slot
                    // count and can cause E2015051807 during an unchanged round-trip.
                    BinaryPrimitives.WriteUInt32BigEndian(s, blob.Length == 0 ? 0 : checked((uint)data.Position));
                    BinaryPrimitives.WriteUInt32BigEndian(s[4..], checked((uint)blob.Length));
                    data.Write(blob);
                    while (data.Position % 32 != 0) data.WriteByte(0);
                    break;
            }

            w.Write(s[..Width(t)]);
        }

        using var body = new MemoryStream();
        using var writer = new BinaryWriter(body);
        body.SetLength(32);
        body.Position = 32;
        foreach (var col in Columns)
        {
            writer.Write((byte)((byte)col.Storage | (byte)col.Type));
            WriteValue(writer, UtfType.String, col.Name);
            if (col.Storage == UtfStorage.Constant) WriteValue(writer, col.Type, col.Constant!);
        }

        ushort rowOffset = checked((ushort)(body.Position - 8));
        ushort rowSize = checked((ushort)Columns.Where(c => c.Storage == UtfStorage.Row).Sum(c => Width(c.Type)));
        foreach (var row in Rows)
            for (int c = 0; c < Columns.Count; c++)
                if (Columns[c].Storage == UtfStorage.Row)
                    WriteValue(writer, Columns[c].Type, row[c]);
        uint stringsOffset = checked((uint)(body.Position - 8));
        body.Write(strings.ToArray());
        // Align the section too: aligned relative blob offsets alone are not
        // sufficient for native workspace structures embedded in the ACB.
        while (body.Position % 32 != 0) body.WriteByte(0);
        uint dataOffset = checked((uint)(body.Position - 8));
        body.Write(data.ToArray());
        while (body.Length % 4 != 0) body.WriteByte(0);
        byte[] result = body.ToArray();
        var h = result.AsSpan();
        "@UTF"u8.CopyTo(h);
        BinaryPrimitives.WriteUInt32BigEndian(h[4..], checked((uint)result.Length - 8));
        h[8] = 0;
        h[9] = EncodingId;
        BinaryPrimitives.WriteUInt16BigEndian(h[10..], rowOffset);
        BinaryPrimitives.WriteUInt32BigEndian(h[12..], stringsOffset);
        BinaryPrimitives.WriteUInt32BigEndian(h[16..], dataOffset);
        BinaryPrimitives.WriteUInt32BigEndian(h[20..], nameOffset);
        BinaryPrimitives.WriteUInt16BigEndian(h[24..], checked((ushort)Columns.Count));
        BinaryPrimitives.WriteUInt16BigEndian(h[26..], rowSize);
        BinaryPrimitives.WriteUInt32BigEndian(h[28..], checked((uint)Rows.Count));
        return result;
    }
}
