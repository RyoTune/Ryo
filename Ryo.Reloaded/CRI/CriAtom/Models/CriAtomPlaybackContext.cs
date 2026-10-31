using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Ryo.Reloaded.CRI.CriAtom.Models;

public enum SourceSelectionType : uint
{
    CueId = 1,
    CueName = 2,
    CueIndex = 3,
}

/// <summary>
/// Partial, borrowed view of the P5R x64 playback context passed to
/// CRIATOM_BuildCueWaveformSource.
/// This describes only the verified selection fields, not the full allocation.
/// Do not allocate or replace a native playback context using this struct.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly unsafe struct PlaybackContext
{
    [FieldOffset(0x98)]
    private readonly SourceSelectionType _selectionType;

    [FieldOffset(0xA0)]
    public readonly CriAtomExAcbHn* AcbHandle;

    // Native union: uint cue ID, char* cue name, or ushort cue index.
    // Narrow reads intentionally ignore the unused upper bytes.
    [FieldOffset(0xA8)]
    private readonly int _cueId;

    [FieldOffset(0xA8)]
    private readonly nint _cueName;

    [FieldOffset(0xA8)]
    private readonly ushort _cueIndex;
    
    [FieldOffset(0x70)]
    private readonly nint _exPlayer;

    public SourceSelectionType SelectionType => _selectionType;

    public bool TryGetCueId(out int cueId)
    {
        cueId = default;
        if (_selectionType != SourceSelectionType.CueId)
            return false;

        cueId = _cueId;
        return true;
    }

    /// <summary>The returned pointer is borrowed and must not be freed.</summary>
    public bool TryGetCueNamePointer(out nint cueName)
    {
        cueName = default;
        if (_selectionType != SourceSelectionType.CueName || _cueName == 0)
            return false;

        cueName = _cueName;
        return true;
    }

    /// <summary>Copies the native UTF-8 name while its pointer is valid.</summary>
    public bool TryGetCueName([NotNullWhen(true)] out string? cueName)
    {
        cueName = null;
        if (!TryGetCueNamePointer(out nint pointer))
            return false;

        cueName = Marshal.PtrToStringUTF8(pointer);
        return cueName is not null;
    }

    public bool TryGetCueIndex(out ushort cueIndex)
    {
        cueIndex = default;
        if (_selectionType != SourceSelectionType.CueIndex)
            return false;

        cueIndex = _cueIndex;
        return true;
    }
    
    public nint SourceConfig => _exPlayer == 0 ? 0 : Marshal.ReadIntPtr(_exPlayer, 0xC0);
}
