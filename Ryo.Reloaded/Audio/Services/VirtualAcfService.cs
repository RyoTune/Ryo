using Ryo.Definitions.Structs;
using SharedScans.Interfaces;
using static Ryo.Definitions.Functions.CriAtomExFunctions;

namespace Ryo.Reloaded.Audio.Services;

internal unsafe class VirtualAcfService
{
    private readonly HookContainer<criAtomConfig_GetCategoryIndexById> getCategoryIndexById;
    private readonly HookContainer<criAtomExAcf_GetCategoryInfoByIndex> getCategoryInfoByIndex;

    public VirtualAcfService(ISharedScans scans)
    {
        this.getCategoryIndexById = scans.CreateHook<criAtomConfig_GetCategoryIndexById>(this.CriAtomExAcf_GetCategoryIndexById, Mod.NAME);
        this.getCategoryInfoByIndex = scans.CreateHook<criAtomExAcf_GetCategoryInfoByIndex>(this.CriAtomExAcf_GetCategoryInfoByIndex, Mod.NAME);
    }

    private bool CriAtomExAcf_GetCategoryInfoByIndex(ushort index, CriAtomExCategoryInfoTag* info)
    {
        var result = this.getCategoryInfoByIndex.Hook!.OriginalFunction(index, info);
        if (result)
        {
            return result;
        }

        if (index == 69)
        {
            var fakeCat = new CriAtomExCategoryInfoTag
            {
                group_no = 0,
                id = 69,
                name = "69420lmao".AsPointerAnsi(true),
                num_cue_limits = uint.MaxValue,
                volume = 0.05f
            };

            *info = fakeCat;

            Log.Information("Using Fake Category for Index 69");
            return true;
        }

        return false;
    }

    private ushort CriAtomExAcf_GetCategoryIndexById(uint id)
    {
        var result = this.getCategoryIndexById.Hook!.OriginalFunction(id);
        if (result == ushort.MaxValue)
        {
            if (id == 69)
            {
                ushort fakeCatIndex = 69;
                Log.Information($"Fake Category Index: {fakeCatIndex}");
                return fakeCatIndex;
            }

            Log.Information($"Invalid Category ID: {id}");
        }

        return result;
    }
}
