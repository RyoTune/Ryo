namespace Ryo.Reloaded.Common.Memory;

public interface IRyoAllocator
{
    nint Malloc(int size);

    void Free(nint pointer);
}