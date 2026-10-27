using System.Runtime.InteropServices;

namespace Ryo.Reloaded.Common.Memory;

public class DefaultAllocator : IRyoAllocator
{
    private readonly Dictionary<nint, byte> _blocks = [];
    
    public nint Malloc(int size)
    {
        var block = Marshal.AllocHGlobal(size);
        _blocks.Add(block, 0);
        return block;
    }

    public void Free(nint pointer)
    {
        if (_blocks.Remove(pointer, out _)) Marshal.FreeHGlobal(pointer);
    }
}