using System.Buffers.Binary;

namespace Ryo.Reloaded.CRI.CriAtom.Models;

public readonly record struct RelatedCue(uint Id, string Name);

public sealed partial class AcbBank
{
    /// <summary>
    /// Returns distinct linked descendants in this bank, excluding the requested cue.
    /// Traverses synth/sequence graphs and nested links, safely handling cycles.
    /// Cross-bank or unresolved links and unsupported graph types throw instead of returning partial results.
    /// Names are taken from the first CueNameTable entry for each linked cue row.
    /// </summary>
    public IReadOnlyList<RelatedCue> GetRelatedCues(uint cueId)
        => GetRelatedCuesCore(Cue(cueId));

    /// <inheritdoc cref="GetRelatedCues(uint)"/>
    public IReadOnlyList<RelatedCue> GetRelatedCues(string cueName)
    {
        ArgumentNullException.ThrowIfNull(cueName);
        var cue = Cues().SingleOrDefault(c => c.Name == cueName)
            ?? throw new ArgumentException($"Cue name '{cueName}' not found.", nameof(cueName));
        return GetRelatedCuesCore(cue);
    }

    private IReadOnlyList<RelatedCue> GetRelatedCuesCore(CueHandle root)
    {
        var cues = Cues();
        var cueTable = Table("CueTable");
        var result = new List<RelatedCue>();
        var found = new HashSet<ushort> { root.CueRow };
        var visited = new HashSet<(ushort Type, ushort Row)>();
        var pending = new Stack<(ushort Type, ushort Row)>();

        ushort U16(byte[] bytes, int offset)
        {
            if (offset < 0 || offset > bytes.Length - 2)
                throw new InvalidDataException("Truncated cue graph reference.");
            return BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset, 2));
        }

        void PushCue(CueHandle cue) => pending.Push((
            checked((ushort)cueTable.Number(cue.CueRow, "ReferenceType")),
            checked((ushort)cueTable.Number(cue.CueRow, "ReferenceIndex"))));

        void CheckRow(UtfTable table, ushort row)
        {
            if (row >= table.Rows.Count)
                throw new InvalidDataException($"Cue graph reference outside {table.Name}: {row}.");
        }

        string ReadString(uint index)
        {
            var strings = Table("StringValueTable");
            if (index >= strings.Rows.Count)
                throw new InvalidDataException($"Linked cue string index out of range: {index}.");
            return strings.Text((int)index, "StringValue");
        }

        PushCue(root);
        while (pending.TryPop(out var reference))
        {
            if (!visited.Add(reference)) continue;
            var (type, row) = reference;
            switch (type)
            {
                case 0: // No reference.
                case 1: // Waveform leaf, not a link to another cue.
                    break;
                case 2:
                {
                    var synths = Table("SynthTable");
                    CheckRow(synths, row);
                    byte[] items = synths.Data(row, "ReferenceItems");
                    if (items.Length % 4 != 0)
                        throw new InvalidDataException("Truncated synth reference list.");
                    for (int i = items.Length - 4; i >= 0; i -= 4)
                        pending.Push((U16(items, i), U16(items, i + 2)));
                    break;
                }
                case 3:
                {
                    var sequences = Table("SequenceTable");
                    CheckRow(sequences, row);
                    byte[] trackIndices = sequences.Data(row, "TrackIndex");
                    int count = checked((int)sequences.Number(row, "NumTracks"));
                    if ((long)count * 2 > trackIndices.Length)
                        throw new InvalidDataException("Truncated sequence track list.");
                    var tracks = Table("TrackTable");
                    for (int i = count - 1; i >= 0; i--)
                    {
                        ushort track = U16(trackIndices, i * 2);
                        CheckRow(tracks, track);
                        ushort evt = checked((ushort)tracks.Number(track, "EventIndex"));
                        if (evt == ushort.MaxValue) continue;
                        var events = Table("TrackEventTable");
                        CheckRow(events, evt);
                        var commands = AcbCommands.Decode(events.Data(evt, "Command"));
                        for (int c = commands.Count - 1; c >= 0; c--)
                        {
                            var command = commands[c];
                            // These commands begin with a reference type/index pair.
                            if (command.Opcode is 2000 or 2002 or 2003)
                                pending.Push((U16(command.Parameters, 0), U16(command.Parameters, 2)));
                        }
                    }
                    break;
                }
                case 5:
                {
                    var links = Table("OutsideLinkTable");
                    CheckRow(links, row);
                    uint bankIndex = links.Number(row, "AcbNameStringIndex");
                    string targetBank = bankIndex == ushort.MaxValue ? "" :
                        ReadString(bankIndex);
                    if (targetBank.Length != 0 && targetBank != Name)
                        throw new NotSupportedException($"Linked cue targets another ACB: '{targetBank}'.");
                    uint id = links.Number(row, "Id");
                    CueHandle? child;
                    if (id != uint.MaxValue)
                        child = cues.FirstOrDefault(c => c.Id == id);
                    else
                    {
                        uint nameIndex = links.Number(row, "StringIndex");
                        string name = ReadString(nameIndex);
                        child = cues.FirstOrDefault(c => c.Name == name);
                    }
                    if (child is null)
                        throw new InvalidDataException($"Unresolved linked cue in OutsideLinkTable row {row}.");
                    if (found.Add(child.CueRow))
                    {
                        result.Add(new(child.Id, child.Name));
                        PushCue(child);
                    }
                    break;
                }
                default:
                    break;
                    throw new NotSupportedException($"Related-cue traversal does not support reference type {type}.");
            }
        }
        return result.AsReadOnly();
    }
}
