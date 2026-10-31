using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Ryo.Reloaded.CRI.CriAtom.Models;

public enum CriAtomExCueSelectionType : int
{
    CueId = 1,
    CueName = 2,
    CueIndex = 3,
}

/// <summary>SDK CriAtomExCueLinkType; a 32-bit enum, not CriBool.</summary>
public enum CriAtomExCueLinkType : int
{
    Static = 0,
    Dynamic = 1,
}

/// <summary>Native x64 cue selection copied from playback context + 0x98.</summary>
[StructLayout(LayoutKind.Explicit, Size = 0x18)]
public readonly unsafe struct CriAtomExSourceInfo
{
    [FieldOffset(0x00)]
    private readonly CriAtomExCueSelectionType _type;

    [FieldOffset(0x08)]
    public readonly CriAtomExAcbHn* AcbHandle;

    [FieldOffset(0x10)]
    private readonly int _cueId;

    [FieldOffset(0x10)]
    private readonly nint _cueName;

    [FieldOffset(0x10)]
    private readonly int _cueIndex;

    public CriAtomExCueSelectionType Type => _type;

    public bool TryGetCueId(out int cueId)
    {
        cueId = default;
        if (_type != CriAtomExCueSelectionType.CueId) return false;
        cueId = _cueId;
        return true;
    }

    /// <summary>Borrowed native pointer; do not free or retain beyond its owning ACB's lifetime.</summary>
    public bool TryGetCueNamePointer(out nint cueName)
    {
        cueName = default;
        if (_type != CriAtomExCueSelectionType.CueName || _cueName == 0) return false;
        cueName = _cueName;
        return true;
    }
    
    public bool TryGetCueName([NotNullWhen(true)] out string? cueName)
    {
        cueName = null;
        if (!TryGetCueNamePointer(out nint pointer)) return false;
        cueName = Marshal.PtrToStringUTF8(pointer);
        return cueName is not null;
    }

    public bool TryGetCueIndex(out int cueIndex)
    {
        cueIndex = default;
        if (_type != CriAtomExCueSelectionType.CueIndex) return false;
        cueIndex = _cueIndex;
        return true;
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct CriAtomExAcbHn;

/// <summary>
/// CueLink notification layout verified against P3R.
/// The info pointer is stack-backed and valid only during the callback. Copy the values
/// needed for ancestry tracking; copied ACB/name pointers still have native lifetimes.
/// Playback IDs include context generations and are not context pointers.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x50)]
public readonly unsafe struct CriAtomExCueLinkInfo
{
    [FieldOffset(0x00)]
    public readonly nint Player;

    [FieldOffset(0x08)]
    public readonly uint ParentPlaybackId;

    [FieldOffset(0x10)]
    public readonly CriAtomExSourceInfo Parent;

    [FieldOffset(0x28)]
    public readonly uint ChildPlaybackId;

    [FieldOffset(0x30)]
    public readonly CriAtomExSourceInfo Child;

    // SDK field: link_type (CRIATOMEX_CUELINK_TYPE_STATIC / DYNAMIC).
    [FieldOffset(0x48)]
    public readonly CriAtomExCueLinkType LinkType;

    /// <summary>True for reference type 5; false for other child graph references.</summary>
    public bool IsExplicitCueLink => LinkType == CriAtomExCueLinkType.Dynamic;
}
