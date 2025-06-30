using Ryo.Definitions.Structs;
using Ryo.Interfaces;
using SharedScans.Interfaces;
using System.Runtime.InteropServices;
using static Ryo.Definitions.Functions.CriAtomExFunctions;

namespace Ryo.Reloaded.Audio.Services;

internal unsafe class VirtualCueService
{
    private readonly Func<AcbCueInfo, string>? linkCueCb;
    private readonly ICriAtomRegistry criAtomRegistry;
    private readonly AudioRegistry audioRegistry;

    private readonly HookContainer<criAtomExAcb_GetCueInfoById> getCueInfoById;
    private readonly HookContainer<criAtomExAcb_GetCueInfoByName> getCueInfoByName;

    public VirtualCueService(
        string game,
        ISharedScans scans,
        ICriAtomRegistry criAtomRegistry,
        AudioRegistry audioRegistry)
    {
        this.linkCueCb = GameDefaults.GetLinkCueCb(game);
        this.criAtomRegistry = criAtomRegistry;
        this.audioRegistry = audioRegistry;

        this.getCueInfoById = scans.CreateHook<criAtomExAcb_GetCueInfoById>(this.CriAtomExAcb_GetCueInfoById, Mod.NAME);
        this.getCueInfoByName = scans.CreateHook<criAtomExAcb_GetCueInfoByName>(this.CriAtomExAcb_GetCueInfoByName, Mod.NAME);
    }

    private bool GetCueInfoByRyo(string acb, string cue, CriAtomExCueInfoTag* info)
    {
        // Ryo has audio registered for a "new" cue.
        if (this.audioRegistry.TryGetCueContainer(cue, acb, out _))
        {
            // Might be better to use GetCueInfoByIndex with index 0 and use that
            // cue info as a shell.
            _ = int.TryParse(cue, out int id);
            info->id = id;
            info->name = cue.AsPointerAnsi(true);
            info->categories[0] = -1;
            Log.Debug($"{nameof(GetCueInfoByRyo)} || Virtual Cue: {cue} / {acb}");
            return true;
        }

        return false;
    }

    private bool CriAtomExAcb_GetCueInfoByName(nint acbHn, nint nameStr, CriAtomExCueInfoTag* info)
    {
        var acb = this.criAtomRegistry.GetAcbByHn(acbHn)!;
        var name = Marshal.PtrToStringAnsi(nameStr)!;

        // Use the linked cue to fetch categories.
        // This is to handle the scenario where game uses link cues but the link cues
        // themselves has no/missing categories compared to the actual cue played.
        nint? linkedCueStr = null;
        if (this.linkCueCb != null)
        {
            var linkedCueName = this.linkCueCb(new(acb.Name, name));
            if (name != linkedCueName)
            {
                Log.Debug($"LinkCueCb: {name} -> {linkedCueName}");
                linkedCueStr = linkedCueName.AsPointerAnsi(true);
            }
        }

        // Cue by name exists in original ACB.
        if (this.getCueInfoByName.Hook?.OriginalFunction(acbHn, linkedCueStr ?? nameStr, info) == true)
        {
            return true;
        }

        if (acb != null)
        {
            return this.GetCueInfoByRyo(acb.Name, name, info);
        }

        return false;
    }

    private bool CriAtomExAcb_GetCueInfoById(nint acbHn, int id, CriAtomExCueInfoTag* info)
    {
        // Cue by ID exists in original ACB.
        if (this.getCueInfoById.Hook?.OriginalFunction(acbHn, id, info) == true)
        {
            return true;
        }

        var acb = this.criAtomRegistry.GetAcbByHn(acbHn);
        if (acb != null)
        {
            return this.GetCueInfoByRyo(acb.Name, id.ToString(), info);
        }

        return false;
    }
}
