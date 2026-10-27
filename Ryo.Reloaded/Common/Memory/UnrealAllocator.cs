using Reloaded.Hooks.Definitions;

namespace Ryo.Reloaded.Common.Memory;

public class UnrealAllocator : IRyoAllocator
{
    public const string UnrealIniId = "unreal";
    public const int DefaultAlignment = 0;

    private IFMalloc? _fmalloc;
    
    public UnrealAllocator()
        => Project.Scans.AddScanHook("GMalloc", (result, hooks)
            => _fmalloc = new FMallocBinned2(result, hooks));

    public nint Malloc(int size) => _fmalloc?.Malloc(size) ?? throw new();

    public void Free(nint pointer) => _fmalloc?.Free(pointer);
}

// Runtime/Core/HAL/MallocBinned2
// used in Persona 3 Reload
internal class FMallocBinned2 : IFMalloc
{
    private IFunctionPtr<IFMalloc.FMemoryMalloc>? _malloc;
    private uint _mallocOffset;
    
    private IFunctionPtr<IFMalloc.FMemoryRealloc>? _realloc;
    private uint _reallocOffset;
    
    private IFunctionPtr<IFMalloc.FMemoryFree>? _free;
    private uint _freeOffset;
    
    private IFunctionPtr<IFMalloc.FMemoryGetAllocSize>? _getAllocSize;
    private uint _getAllocSizeOffset;
    
    private IFunctionPtr<IFMalloc.FMemoryQuantizeSize>? _quantizeSize;
    private uint _quantizeSizeOffset;
    
    private readonly nint _ptr;
    private readonly IReloadedHooks _hooks;
    
    public FMallocBinned2(nint ptr, IReloadedHooks hooks)
    {
        _ptr = ptr;
        _hooks = hooks;
        
        Project.Inis.UsingSetting<uint>("unreal", nameof(Malloc), nameof(FMallocBinned2), value => _mallocOffset = value);
        Project.Inis.UsingSetting<uint>(UnrealAllocator.UnrealIniId, nameof(Realloc), nameof(FMallocBinned2), value => _reallocOffset = value);
        Project.Inis.UsingSetting<uint>(UnrealAllocator.UnrealIniId, nameof(Free), nameof(FMallocBinned2), value => _freeOffset = value);
        Project.Inis.UsingSetting<uint>(UnrealAllocator.UnrealIniId, nameof(GetAllocSize), nameof(FMallocBinned2), value => _getAllocSizeOffset = value);
        Project.Inis.UsingSetting<uint>(UnrealAllocator.UnrealIniId, nameof(QuantizeSize), nameof(FMallocBinned2), value => _quantizeSizeOffset = value);
    }

    public unsafe void Free(nint original)
    {
        if (_free == null)
            _free = _hooks.CreateFunctionPtr<IFMalloc.FMemoryFree>(**(nuint**)_ptr + _freeOffset);
        _free.GetDelegate()(*(nint*)_ptr, original);
    }

    public unsafe bool GetAllocSize(nint ptr, ref nint size)
    {
        if (_getAllocSize == null)
            _getAllocSize = _hooks.CreateFunctionPtr<IFMalloc.FMemoryGetAllocSize>(**(nuint**)_ptr + _getAllocSizeOffset);
        return _getAllocSize.GetDelegate()(*(nint*)_ptr, ptr, ref size) != 0;
    }

    public unsafe nint Malloc(nint size, int alignment = UnrealAllocator.DefaultAlignment)
    {
        if (_malloc == null)
            _malloc = _hooks.CreateFunctionPtr<IFMalloc.FMemoryMalloc>(**(nuint**)_ptr + _mallocOffset);
        var value = _malloc.GetDelegate()(*(nint*)_ptr, size, alignment);
        return value;
    }

    public unsafe nint Realloc(nint ptr, nint size, int alignment = UnrealAllocator.DefaultAlignment)
    {
        if (_realloc == null)
            _realloc = _hooks.CreateFunctionPtr<IFMalloc.FMemoryRealloc>(**(nuint**)_ptr + _reallocOffset);
        return _realloc.GetDelegate()(*(nint*)_ptr, ptr, size, alignment);
    }

    public unsafe nint QuantizeSize(nint count, int alignment = UnrealAllocator.DefaultAlignment)
    {
        if (_quantizeSize == null)
            _quantizeSize = _hooks.CreateFunctionPtr<IFMalloc.FMemoryQuantizeSize>(**(nuint**)_ptr + _quantizeSizeOffset);
        return _quantizeSize.GetDelegate()(*(nint*)_ptr, count, alignment);
    }
}

internal interface IFMalloc
{
    // Memory allocation stubs
    public nint Malloc(nint size, int alignment = UnrealAllocator.DefaultAlignment);
    public nint Realloc(nint ptr, nint size, int alignment = UnrealAllocator.DefaultAlignment);
    public void Free(nint original);
    public bool GetAllocSize(nint ptr, ref nint size);
    public nint QuantizeSize(nint count, int alignment = UnrealAllocator.DefaultAlignment);

    internal delegate nint FMemoryMalloc(nint self, nint size, int alignment = UnrealAllocator.DefaultAlignment);
    internal delegate nint FMemoryRealloc(nint self, nint ptr, nint size, int alignment = UnrealAllocator.DefaultAlignment);
    internal delegate void FMemoryFree(nint self, nint ptr);
    internal delegate char FMemoryGetAllocSize(nint self, nint ptr, ref nint size);
    internal delegate nint FMemoryQuantizeSize(nint self, nint count, int alignment = UnrealAllocator.DefaultAlignment);
}