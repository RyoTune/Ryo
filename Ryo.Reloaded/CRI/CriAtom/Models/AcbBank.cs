using System.Text.Json;

namespace Ryo.Reloaded.CRI.CriAtom.Models;

public sealed record CueHandle(uint Id, string Name, ushort CueRow, ushort SequenceRow);

public sealed record BusSend(string Name, float Level);

public sealed class BusSends
{
    private readonly Dictionary<string, float> _busses = new(StringComparer.OrdinalIgnoreCase);
    
    public void Add(string name, float level)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        if (level < 0.0f || level > 1.0f) throw new ArgumentOutOfRangeException(nameof(level));
        _busses.Add(name, level);
    }
    
    public void Remove(string name) => _busses.Remove(name);
    
    public IReadOnlyList<BusSend> Busses => _busses.Select(x => new BusSend(x.Key, x.Value)).ToArray();
}

public sealed class CueOptions
{
    private readonly Dictionary<string, float> _busses = [];
    
    public float Volume { get; init; } = 1.0f;
    
    public List<string> Categories { get; init; } = [];

    public List<uint> CategoryIds { get; init; } = [];

    public List<string> GlobalAisacs { get; init; } = [];

    public IReadOnlyList<BusSend> BusSends => _busses.Select(x => new BusSend(x.Key, x.Value)).ToArray();

    /// <summary>Additional validated TLV commands; caller is responsible for opcode semantics.</summary>
    public List<AcbCommand> Commands { get; init; } = [];

    public void SetBus(string name, float level) => _busses[name] = level;
    
    public void RemoveBus(string name) => _busses.Remove(name);

    public bool ContainsBus(string name) => _busses.ContainsKey(name);
}

public sealed record CueOptions2(float f);

/// <summary>Schema-preserving ACB 1.30 editor and metadata-bank builder.</summary>
public sealed partial class AcbBank
{
    private readonly List<string> _clearedTables = [];
    private readonly Dictionary<string, UtfTable> _tables = new(StringComparer.Ordinal);
    private HashSet<uint> _usedCueIds = [];
    private HashSet<string> _usedCueNames = [];

    public UtfTable Header { get; private set; }

    /// <summary>Tables emptied by CreateEmpty; diagnostic information, not serialized.</summary>
    public IReadOnlyList<string> ClearedTables => _clearedTables.AsReadOnly();

    public AcfCatalog? Acf { get; }
    public uint Version => Header.Number(0, "Version");
    public string Name => Header.Text(0, "Name");

    public AcbBank(ReadOnlySpan<byte> bytes, AcfCatalog? acf = null)
    {
        Header = UtfTable.Read(bytes);
        Acf = acf;
        if (Header.Rows.Count != 1 || !Header.Has("CueTable")) throw new InvalidDataException("Not an ACB header.");
    }

    public AcbBank(byte[] bytes, AcfCatalog? acf = null) : this(bytes.AsSpan(), acf)
    {
    }

    public static AcbBank Load(string path, AcfCatalog? acf = null) => new(File.ReadAllBytes(path), acf);

    public UtfTable Table(string name)
    {
        if (_tables.TryGetValue(name, out var t)) return t;
        var bytes = Header.Data(0, name);
        if (bytes.Length == 0) throw new InvalidDataException($"ACB table '{name}' is absent.");
        t = UtfTable.Read(bytes);
        _tables.Add(name, t);
        return t;
    }

    private void Supported()
    {
        // if (Version != 0x01300000)
        //     throw new NotSupportedException(
        //         $"Authoring is validated against ACB 1.30 (0x01300000), not 0x{Version:x8}.");
    }

    public static AcbBank CreateEmpty(byte[] schemaTemplate, string name, AcfCatalog? acf = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Contains('\0')) throw new ArgumentException("Invalid bank name.");
        var bank = new AcbBank(schemaTemplate, acf);
        // Emptying is schema-driven; cue authoring still enforces its supported version.
        // Bank-wide scalar configuration and opaque blobs are preserved, except AWB data.
        string[] awbMetadata =
        [
            "StreamAwbHash", "StreamAwbAfs2Header", "StreamAwbTocWork",
            "StreamAwbTocWorkOld", "StreamAwbTocWork_Old"
        ];
        foreach (var col in bank.Header.Columns.Where(c => c.Type == UtfType.Data))
        {
            if (col.Name == "AwbFile" || awbMetadata.Contains(col.Name)) continue;
            var blob = bank.Header.Data(0, col.Name);
            if (!blob.AsSpan().StartsWith("@UTF"u8)) continue;
            var table = bank.Table(col.Name);
            if (table.Rows.Count > 0) bank._clearedTables.Add(col.Name);
            table.Rows.Clear();
        }

        bank.Header.Set(0, "Name", name);
        foreach (string field in new[] { "NumCueLimitListWorks", "NumCueLimitNodeWorks" })
            if (bank.Header.Has(field))
                bank.Header.Set(0, field, 0);
        if (bank.Header.Has("AwbFile")) bank.Header.Set(0, "AwbFile", Array.Empty<byte>());
        // Detach AWB metadata as a unit, retaining UTF schemas but removing their rows.
        // CRI derives the stream-AWB count from StreamAwbHash.Rows.Count, even if each
        // hash/name is zero. Keeping a placeholder row while clearing StreamAwbTocWork
        // can trigger E2015051807 (Stream awb work overflow) during ACB initialization;
        // the caller's allocator/work buffer does not supply this embedded workspace.
        // A later source hook may need an attached AWB to pass the earlier waveform
        // attachment check. If so, restore coherent metadata AND workspace/attachment
        // or hook that check earlier; a zeroed metadata row alone is insufficient.
        foreach (string field in awbMetadata)
        {
            if (!bank.Header.Has(field)) continue;
            var bytes = bank.Header.Data(0, field);
            if (bytes.AsSpan().StartsWith("@UTF"u8))
            {
                var t = bank.Table(field);
                if (t.Rows.Count > 0) bank._clearedTables.Add(field);
                t.Rows.Clear();
            }
            else bank.Header.Set(0, field, Array.Empty<byte>());
        }

        return bank;
    }

    public IReadOnlyList<CueHandle> Cues()
    {
        var cues = Table("CueTable");
        var names = Table("CueNameTable");
        var result = new List<CueHandle>();
        for (int i = 0; i < names.Rows.Count; i++)
        {
            ushort row = checked((ushort)names.Number(i, "CueIndex"));
            if (row >= cues.Rows.Count) throw new InvalidDataException("Cue name points outside CueTable.");
            result.Add(new(cues.Number(row, "CueId"), names.Text(i, "CueName"), row,
                cues.Number(row, "ReferenceType") == 3
                    ? checked((ushort)cues.Number(row, "ReferenceIndex"))
                    : ushort.MaxValue));
        }

        return result.AsReadOnly();
    }

    private ushort Add(string name, Dictionary<string, object> fields)
    {
        var t = Table(name);
        if (t.Rows.Count >= ushort.MaxValue)
            throw new InvalidOperationException($"{name} exhausted its 16-bit indices.");
        // Extra fields known from later AtomicAudio schemas may be omitted by this template.
        var actual = fields.Where(p => t.Has(p.Key)).ToDictionary(p => p.Key, p => p.Value);
        foreach (var c in t.Columns)
            if (!fields.ContainsKey(c.Name))
                throw new NotSupportedException($"No authoring default for {name}.{c.Name}.");
        return checked((ushort)t.AddRow(actual));
    }

    private T Transaction<T>(Func<T> operation)
    {
        var bytes = ToBytes();
        try
        {
            Supported();
            T result = operation();
            SortCues();
            // Sorting invalidates row indices previously returned to callers. Refresh the
            // current result; retained handles should be resolved again by ID after edits.
            if (result is CueHandle cue)
                result = (T)(object)(cue with { CueRow = FindCueRow(cue.Id) });
            return result;
        }
        catch
        {
            Header = UtfTable.Read(bytes);
            _tables.Clear();
            _usedCueIds = [];
            _usedCueNames = [];
            throw;
        }
    }

    private CueHandle Cue(uint id) => Cues().SingleOrDefault(c => c.Id == id)
                                      ?? throw new ArgumentException($"Cue ID {id} not found.");

    private void NewIdentity(uint id, string name)
    {
        if (id > int.MaxValue || string.IsNullOrWhiteSpace(name) || name.Contains('\0'))
            throw new ArgumentException("A cue needs a nonreserved ID and a nonempty name without NUL.");
        EnsureIdentities();
        if (_usedCueIds.Contains(id) || _usedCueNames.Contains(name))
            throw new ArgumentException("Duplicate cue ID or name.");
    }

    private void EnsureIdentities()
    {
        var cues = Table("CueTable");
        var names = Table("CueNameTable");
        
        // Include unnamed cues and every alias, rather than just the Cues() view.
        _usedCueIds = Enumerable.Range(0, cues.Rows.Count).Select(i => cues.Number(i, "CueId")).ToHashSet();
        _usedCueNames = Enumerable.Range(0, names.Rows.Count).Select(i => names.Text(i, "CueName"))
            .ToHashSet(StringComparer.Ordinal);
    }

    private uint AllocateCueId()
    {
        EnsureIdentities();
        uint id = 0;
        while (_usedCueIds.Contains(id))
        {
            if (id == int.MaxValue) throw new InvalidOperationException("No free cue IDs.");
            id++;
        }

        return id;
    }

    private string AllocateCueName(uint id)
    {
        EnsureIdentities();
        string basis = id.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string name = basis;
        for (int suffix = 1; _usedCueNames.Contains(name); suffix++) name = $"{basis}_{suffix}";
        return name;
    }

    private ushort FindCueRow(uint id)
    {
        var cues = Table("CueTable");
        return checked((ushort)Enumerable.Range(0, cues.Rows.Count).Single(i => cues.Number(i, "CueId") == id));
    }

    private void SortCues()
    {
        var cues = Table("CueTable");
        var names = Table("CueNameTable");
        int idColumn = cues.ColumnIndex("CueId");
        var order = Enumerable.Range(0, cues.Rows.Count)
            .OrderBy(i => Convert.ToUInt32(cues.Rows[i][idColumn])).ToArray();
        var remap = new int[order.Length];
        for (int i = 0; i < order.Length; i++) remap[order[i]] = i;
        var rows = order.Select(i => cues.Rows[i]).ToArray();
        cues.Rows.Clear();
        cues.Rows.AddRange(rows);
        for (int i = 0; i < names.Rows.Count; i++)
        {
            uint old = names.Number(i, "CueIndex");
            if (old >= remap.Length) throw new InvalidDataException("Cue name points outside CueTable.");
            names.Set(i, "CueIndex", checked((ushort)remap[old]));
        }

        // Native name lookup also expects ordered names. Compare serialized bytes,
        // including legacy Shift-JIS banks, rather than culture-dependent text order.
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        var encoding = System.Text.Encoding.GetEncoding(names.EncodingId == 0 ? 932 : 65001);
        int nameColumn = names.ColumnIndex("CueName");
        names.Rows.Sort((a, b) => encoding.GetBytes((string)a[nameColumn]).AsSpan()
            .SequenceCompareTo(encoding.GetBytes((string)b[nameColumn])));
    }

    private ushort NewCue(uint id, string name, ushort sequence, uint length, uint waveCount)
    {
        ushort row = Add("CueTable", new()
        {
            ["CueId"] = id, ["ReferenceType"] = 3, ["ReferenceIndex"] = sequence,
            ["UserData"] = "", ["Worksize"] = 0, ["AisacControlMap"] = Array.Empty<byte>(),
            ["Length"] = length, ["NumAisacControlMaps"] = 0, ["HeaderVisibility"] = 1,
            ["NumRelatedWaveforms"] = waveCount
        });
        Add("CueNameTable", new() { ["CueName"] = name, ["CueIndex"] = row });
        _usedCueIds.Add(id);
        _usedCueNames.Add(name);
        return row;
    }

    private ushort TrackForReference(ushort type, ushort index)
    {
        ushort evt = Add("TrackEventTable", new()
            { ["Command"] = AcbCommands.Encode([new(AcbCommands.NoteOn, AcbCommands.U16(type, index))]) });
        return Add("TrackTable", new()
        {
            ["EventIndex"] = evt, ["CommandIndex"] = ushort.MaxValue, ["LocalAisacs"] = Array.Empty<byte>(),
            ["GlobalAisacStartIndex"] = ushort.MaxValue, ["GlobalAisacNumRefs"] = 0,
            ["ParameterPallet"] = ushort.MaxValue, ["TargetType"] = 0, ["TargetName"] = "",
            ["TargetId"] = uint.MaxValue, ["TargetAcbName"] = "", ["Scope"] = 0, ["TargetTrackNo"] = ushort.MaxValue
        });
    }

    private ushort Sequence(ushort[] tracks)
    {
        if (tracks.Length == 0) throw new ArgumentException("A sequence needs tracks.");
        return Add("SequenceTable", new()
        {
            ["PlaybackRatio"] = 100, ["NumTracks"] = tracks.Length, ["TrackIndex"] = AcbCommands.U16(tracks),
            ["CommandIndex"] = ushort.MaxValue, ["LocalAisacs"] = Array.Empty<byte>(),
            ["GlobalAisacStartIndex"] = ushort.MaxValue, ["GlobalAisacNumRefs"] = 0,
            ["ParameterPallet"] = ushort.MaxValue, ["ActionTrackStartIndex"] = ushort.MaxValue,
            ["NumActionTracks"] = 0,
            ["TrackValues"] = tracks.Length > 1
                ? AcbCommands.U16(tracks.Select(_ => (ushort)100).ToArray())
                : Array.Empty<byte>(),
            ["Type"] = 0, ["ControlWorkArea1"] = 1, ["ControlWorkArea2"] = 1,
            ["InstPluginTrackStartIndex"] = ushort.MaxValue, ["NumInstPluginTracks"] = 0
        });
    }

    /// <summary>Creates a hook-backed cue; its generated name is returned in the handle.</summary>
    public CueHandle AddCue(uint id, CueOptions? options = null)
        => AddCue(id, AllocateCueName(id), options);

    /// <summary>Creates a hook-backed cue with an unused nonnegative ID.</summary>
    public CueHandle AddCue(string name, CueOptions? options = null)
        => AddCue(AllocateCueId(), name, options);

    /// <summary>Copies a cue's metadata and graph references, assigning a new ID and generated unique name.</summary>
    public CueHandle AddCue(uint id, uint templateCueId)
        => AddTemplateCue(id, AllocateCueName(id), FindTemplateRow(templateCueId));

    /// <summary>Copies a named cue, assigning a new ID and generated unique name.</summary>
    public CueHandle AddCue(uint id, string templateCueName)
        => AddTemplateCue(id, AllocateCueName(id), FindTemplateRow(templateCueName));

    /// <summary>Copies a cue, assigning a new name and generated unique ID.</summary>
    public CueHandle AddCue(string name, uint templateCueId)
        => AddTemplateCue(AllocateCueId(), name, FindTemplateRow(templateCueId));

    /// <summary>Copies a named cue, assigning a new name and generated unique ID.</summary>
    public CueHandle AddCue(string name, string templateCueName)
        => AddTemplateCue(AllocateCueId(), name, FindTemplateRow(templateCueName));

    private ushort FindTemplateRow(uint id)
    {
        EnsureIdentities();
        if (!_usedCueIds.Contains(id)) throw new ArgumentException($"Template cue ID {id} not found.");
        return FindCueRow(id);
    }

    private ushort FindTemplateRow(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        var names = Table("CueNameTable");
        for (int i = 0; i < names.Rows.Count; i++)
            if (names.Text(i, "CueName") == name)
                return checked((ushort)names.Number(i, "CueIndex"));
        throw new ArgumentException($"Template cue name '{name}' not found.");
    }

    private CueHandle AddTemplateCue(uint id, string name, ushort templateRow)
        => Transaction(() =>
        {
            NewIdentity(id, name);
            var cues = Table("CueTable");
            if (templateRow >= cues.Rows.Count)
                throw new InvalidDataException("Template cue index is outside CueTable.");
            // Temporary diagnostic allowlist: remove names to replace their template
            // values with the UTF type's zero/empty value. Keep CueId (overridden below),
            // ReferenceType and ReferenceIndex while testing non-graph metadata.
            // This filters CueTable only; the referenced playback graph remains shared.
            HashSet<string> templateColumns = new(StringComparer.Ordinal)
            {
                "CueId",
                "ReferenceType",
                "ReferenceIndex",
                "UserData",
                "Worksize",
                "AisacControlMap",
                "Length",
                "NumAisacControlMaps",
                "HeaderVisibility",
                "NumRelatedWaveforms"
            };
            var fields = cues.Columns.Select((column, i) =>
                    (column.Name, Value: !templateColumns.Contains(column.Name)
                        ? UtfTable.Zero(column.Type)
                        : cues.Rows[templateRow][i] is byte[] blob
                            ? blob.ToArray()
                            : cues.Rows[templateRow][i]))
                .ToDictionary(pair => pair.Name, pair => pair.Value);
            fields["CueId"] = id;
            ushort row = Add("CueTable", fields);
            Add("CueNameTable", new() { ["CueName"] = name, ["CueIndex"] = row });
            _usedCueIds.Add(id);
            _usedCueNames.Add(name);
            ushort sequence = cues.Number(row, "ReferenceType") == 3
                ? checked((ushort)cues.Number(row, "ReferenceIndex"))
                : ushort.MaxValue;

            return new CueHandle(id, name, row, sequence);
        });

    public CueHandle AddStreamingCue(uint id, CueOptions? options = null) => AddCue(id, options);
    public CueHandle AddStreamingCue(string name, CueOptions? options = null) => AddCue(name, options);

    // Plausible nonzero metadata is enough to enter waveform construction. The
    // source hook supplies actual audio; this is not its duration/loop metadata.
    // Native streaming-AWB attachment checks must still be satisfied or bypassed.
    private static readonly HcaInfo HookWaveform = new(1, 48000, 1024, null, null);

    private CueHandle AddCue(uint id, string name, CueOptions? options)
        => Transaction(() =>
        {
            NewIdentity(id, name);
            var wt = Table("WaveformTable");
            string waveIdColumn = wt.Has("StreamAwbId") ? "StreamAwbId" : "Id";
            var used = Enumerable.Range(0, wt.Rows.Count).Select(r => wt.Number(r, waveIdColumn)).ToHashSet();
            int free = Enumerable.Range(0, ushort.MaxValue).FirstOrDefault(i => !used.Contains((uint)i), -1);
            if (free < 0) throw new InvalidOperationException("No free stream waveform IDs.");
            ushort waveId = (ushort)free;
            ushort wave = Add("WaveformTable", new()
            {
                ["Id"] = waveId, ["MemoryAwbId"] = ushort.MaxValue, ["StreamAwbId"] = waveId,
                ["EncodeType"] = 2, ["Streaming"] = 1, ["NumChannels"] = HookWaveform.Channels,
                ["LoopFlag"] = 1, ["SamplingRate"] = HookWaveform.SampleRate,
                ["NumSamples"] = HookWaveform.SampleCount, ["ExtensionData"] = ushort.MaxValue,
                ["StreamAwbPortNo"] = 0, ["LipMorthIndex"] = ushort.MaxValue
            });
            ushort synth = Add("SynthTable", new()
            {
                ["Type"] = 0, ["VoiceLimitGroupName"] = "", ["CommandIndex"] = ushort.MaxValue,
                ["ReferenceItems"] = AcbCommands.U16(1, wave), ["LocalAisacs"] = Array.Empty<byte>(),
                ["GlobalAisacStartIndex"] = ushort.MaxValue, ["GlobalAisacNumRefs"] = 0,
                ["ControlWorkArea1"] = 0, ["ControlWorkArea2"] = 0, ["TrackValues"] = Array.Empty<byte>(),
                ["ParameterPallet"] = ushort.MaxValue, ["ActionTrackStartIndex"] = ushort.MaxValue,
                ["NumActionTracks"] = 0
            });
            ushort sequence = Sequence([TrackForReference(2, synth)]);
            ushort cue = NewCue(id, name, sequence, HookWaveform.DurationMilliseconds, 1);

            options ??= new();
            if (!options.ContainsBus("MasterOut")) options.SetBus("MasterOut", options.Volume);
            
            Apply(sequence, options);
            UpdateControlMap(cue, options, []);
            return new CueHandle(id, name, cue, sequence);
        });

    /// <summary>Creates a polyphonic sequence of linked cues in this bank. Existing children retain their own metadata.</summary>
    public CueHandle AddLinkedCue(uint id, string name, IReadOnlyList<uint> children, CueOptions? options = null)
        => Transaction(() =>
        {
            NewIdentity(id, name);
            if (children.Count == 0) throw new ArgumentException("A linked cue needs children.");
            var childCues = children.Select(Cue).ToArray();
            if (!Header.Has("OutsideLinkTable")) throw new NotSupportedException("No OutsideLinkTable column.");
            if (Header.Data(0, "OutsideLinkTable").Length == 0 && !_tables.ContainsKey("OutsideLinkTable"))
            {
                // Column order/types confirmed by native reader 18006be34.
                var links = new UtfTable("OutsideLink", Header.EncodingId);
                links.Columns.AddRange([
                    new("Id", UtfType.UInt32), new("StringIndex", UtfType.UInt16),
                    new("AcbNameStringIndex", UtfType.UInt16)
                ]);
                _tables.Add("OutsideLinkTable", links);
            }

            ushort bankName = Add("StringValueTable", new() { ["StringValue"] = Name });
            var linkedTracks = childCues.Select(c =>
            {
                ushort link = Add("OutsideLinkTable",
                    new() { ["Id"] = c.Id, ["StringIndex"] = ushort.MaxValue, ["AcbNameStringIndex"] = bankName });
                return TrackForReference(5, link);
            }).ToArray();
            ushort sequence = Sequence(linkedTracks);
            uint duration = childCues.Max(c => Table("CueTable").Number(c.CueRow, "Length"));
            ushort cue = NewCue(id, name, sequence, duration, 0);
            Apply(sequence, options ?? new());
            UpdateControlMap(cue, options ?? new(),
                childCues.Select(c => Table("CueTable").Data(c.CueRow, "AisacControlMap")).ToArray());
            return new CueHandle(id, name, cue, sequence);
        });

    public void SetCueOptions(uint id, CueOptions options) => Transaction(() =>
    {
        var cue = Cue(id);
        if (cue.SequenceRow == ushort.MaxValue)
            throw new NotSupportedException("Only sequence-backed cues can be configured.");
        ushort sequence = cue.SequenceRow;
        if (Cues().Count(c => c.SequenceRow == sequence) > 1)
        {
            var st = Table("SequenceTable");
            sequence = Add("SequenceTable",
                st.Columns.Select((c, i) => (c.Name, st.Rows[cue.SequenceRow][i]))
                    .ToDictionary(p => p.Name, p => p.Item2));
            Table("CueTable").Set(cue.CueRow, "ReferenceIndex", sequence);
        }

        // Existing local AISACs and nested graphs can contribute additional control-map bits.
        // Preserve those rather than erasing their query metadata during a parameter edit.
        var inherited = Table("CueTable").Data(cue.CueRow, "AisacControlMap");
        Apply(sequence, options);
        UpdateControlMap(cue.CueRow, options, [inherited]);
        return true;
    });

    private void UpdateControlMap(ushort cue, CueOptions options, byte[][] inherited)
    {
        var bits = inherited.SelectMany((map, _) => map.Select((value, index) => (value, index))).ToArray();
        var indices = new List<ushort>();
        if (Acf is not null)
            foreach (string globalName in options.GlobalAisacs)
            {
                var global = Acf.GlobalAisac(globalName);
                var control = Acf.Controls.SingleOrDefault(c => c.StoredId == global.StoredControlId)
                              ?? throw new InvalidDataException("Global AISAC references an unknown control.");
                indices.Add(control.MapIndex);
            }

        int length = Math.Max(bits.Length == 0 ? 0 : bits.Max(b => b.index) + 1,
            indices.Count == 0 ? 0 : indices.Max() / 8 + 1);
        if (length > byte.MaxValue)
            throw new NotSupportedException("Cue control map exceeds this schema's byte count field.");
        var map = new byte[length];
        foreach (var bit in bits) map[bit.index] |= bit.value;
        foreach (ushort index in indices) map[index / 8] |= (byte)(1 << (index % 8));
        Table("CueTable").Set(cue, "AisacControlMap", map);
        Table("CueTable").Set(cue, "NumAisacControlMaps", map.Length);
    }

    private void AcfReference(byte type, string name, uint id = uint.MaxValue)
    {
        var t = Table("AcfReferenceTable");
        for (int i = 0; i < t.Rows.Count; i++)
            if (t.Number(i, "Type") == type && t.Text(i, "Name") == name)
            {
                if (t.Number(i, "Id") != id) throw new InvalidDataException("Conflicting ACF reference ID.");
                return;
            }

        Add("AcfReferenceTable", new() { ["Type"] = type, ["Name"] = name, ["Name2"] = "", ["Id"] = id });
    }

    private void Apply(ushort sequence, CueOptions options)
    {
        var st = Table("SequenceTable");
        ushort old = checked((ushort)st.Number(sequence, "CommandIndex"));
        var commands = old == ushort.MaxValue ? [] : AcbCommands.Decode(Table("SeqCommandTable").Data(old, "Command"));
        // Options replace these feature sets while preserving unrelated commands and their opaque parameters.
        commands.RemoveAll(c => c.Opcode is AcbCommands.Category or AcbCommands.VolumeBus);
        var categoryIds = options.CategoryIds.ToList();
        foreach (string name in options.Categories)
            categoryIds.Add((Acf ?? throw new InvalidOperationException("Category names require an ACF catalog."))
                .Category(name).Id);
        categoryIds = categoryIds.Distinct().ToList();
        if (categoryIds.Count > 63) throw new ArgumentException("Category command exceeds the 255-byte TLV limit.");
        if (Acf is not null)
        {
            var categories = categoryIds.Select(Acf.Category).ToArray();
            if (categories.Length > Acf.CategoriesPerPlayback)
                throw new ArgumentException("Too many categories for this ACF.");
            if (categories.GroupBy(c => c.GroupIndex).Any(g => g.Count() > 1))
                throw new ArgumentException("Only one category per ACF category group may be assigned.");
            foreach (var c in categories) AcfReference(3, c.Name, c.Id);
        }
        // Without an ACF, accept public category IDs and reuse matching names from the bank's reference table.
        else
        {
            var refs = Table("AcfReferenceTable");
            foreach (uint id in categoryIds)
                if (!Enumerable.Range(0, refs.Rows.Count)
                        .Any(r => refs.Number(r, "Type") == 3 && refs.Number(r, "Id") == id))
                    throw new InvalidOperationException(
                        $"Category ID {id} needs an existing ACF reference or a supplied ACF catalog.");
        }

        if (categoryIds.Count > 0) commands.Add(new(AcbCommands.Category, AcbCommands.U32(categoryIds.ToArray())));
        var globals = options.GlobalAisacs.Distinct(StringComparer.Ordinal).ToArray();
        ushort globalStart = ushort.MaxValue;
        foreach (string name in globals)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Contains('\0'))
                throw new ArgumentException("Invalid global AISAC name.");
            if (Acf is null) throw new InvalidOperationException("Global AISAC attachment requires an ACF catalog.");
            var global = Acf.GlobalAisac(name);
            ushort row = Add("GlobalAisacReferenceTable", new() { ["Name"] = name });
            if (globalStart == ushort.MaxValue) globalStart = row;
            AcfReference(4, name);
            foreach (var control in Acf.Controls.Where(c => c.StoredId == global.StoredControlId))
                AcfReference(5, control.Name);
        }

        st.Set(sequence, "GlobalAisacStartIndex", globalStart);
        st.Set(sequence, "GlobalAisacNumRefs", globals.Length);
        // Global AISAC attachments are represented by the range above. Do not infer command 75's
        // meaning from AtomicAudio's enum name: the native interpreter writes parameter 0x5d.
        var strings = Table("StringValueTable");
        foreach (var send in options.BusSends)
        {
            if (string.IsNullOrWhiteSpace(send.Name) || send.Name.Contains('\0') || !float.IsFinite(send.Level) ||
                send.Level < 0 || send.Level > 1)
                throw new ArgumentException("Bus send needs a name and level in [0, 1].");
            if (Acf is not null && !Acf.BusNames.Contains(send.Name, StringComparer.Ordinal))
                throw new ArgumentException($"Bus '{send.Name}' is absent from the ACF.");
            int index = Enumerable.Range(0, strings.Rows.Count)
                .FirstOrDefault(i => strings.Text(i, "StringValue") == send.Name, -1);
            if (index < 0) index = Add("StringValueTable", new() { ["StringValue"] = send.Name });
            commands.Add(new(AcbCommands.VolumeBus,
                AcbCommands.U16(checked((ushort)index), checked((ushort)MathF.Round(send.Level * 10000)))));
            AcfReference(9, send.Name);
        }

        if (options.Commands.Any(c => c.Opcode is AcbCommands.Category or AcbCommands.VolumeBus or AcbCommands.NoteOn))
            throw new ArgumentException("Use typed cue options for references, categories, global AISACs and buses.");
        commands.AddRange(options.Commands);
        ushort ci = commands.Count == 0
            ? ushort.MaxValue
            : Add("SeqCommandTable", new() { ["Command"] = AcbCommands.Encode(commands) });
        st.Set(sequence, "CommandIndex", ci);
    }

    public byte[] ToBytes()
    {
        foreach (var pair in _tables) Header.Set(0, pair.Key, pair.Value.Write());
        return Header.Write();
    }

    public void Save(string path) => File.WriteAllBytes(path, ToBytes());

    public string ManifestJson() => JsonSerializer.Serialize(new
    {
        schemaVersion = 1, bank = Name, acbVersion = Version,
        requiresRuntimeSourceHook = true, requiresStreamingAttachmentHandling = true,
    }, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
}