using Reloaded.Hooks.Definitions;
using Ryo.Definitions.Structs;
using Ryo.Reloaded.Audio;
using static Ryo.Definitions.Functions.CriUnrealFunctions;

namespace Ryo.Reloaded.CRI.CriUnreal;

internal unsafe class CriUnreal
{
    private readonly string game;
    private readonly CriUnrealPatterns patterns;
    private IHook<USoundAtomCueSheet_AsyncLoadCueSheetTask> asyncLoadCueSheetTaskHook;
    private IHook<USoundAtomCueSheet_AsyncLoadCueSheetTask_SMTV> asyncLoadCueSheetTaskHook_SMTV;
    private bool devMode;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public CriUnreal(string game)
    {
        this.game = game;
        this.patterns = CriUnrealGames.GetGamePatterns(game);

        //ScanHooks.Add(
        //    nameof(USoundAtomCueSheet_AsyncLoadCueSheetTask),
        //    this.patterns.USoundAtomCueSheet_AsyncLoadCueSheetTask,
        //    (hooks, result) => this.asyncLoadCueSheetTaskHook = hooks.CreateHook<USoundAtomCueSheet_AsyncLoadCueSheetTask>(this.USoundAtomCueSheet_AsyncLoadCueSheetTask, result).Activate());

        //ScanHooks.Add(
        //    nameof(USoundAtomCueSheet_AsyncLoadCueSheetTask_SMTV),
        //    this.patterns.USoundAtomCueSheet_AsyncLoadCueSheetTask_SMTV,
        //    (hooks, result) => this.asyncLoadCueSheetTaskHook_SMTV = hooks.CreateHook<USoundAtomCueSheet_AsyncLoadCueSheetTask_SMTV>(this.AsyncLoadCueSheetTask_SMTV, result).Activate());
    }

    public void SetDevMode(bool devMode)
    {
        this.devMode = devMode;
    }

    private unsafe void USoundAtomCueSheet_AsyncLoadCueSheetTask(LoadTaskParameter* taskParams)
    {
        this.asyncLoadCueSheetTaskHook.OriginalFunction(taskParams);

        var acbName = taskParams->CueSheet->CueSheetName.GetString()!;
        var acbHn = *taskParams->CueSheet->_AcbHn;

        //AcbRegistry.Register(acbName, acbHn);
    }

    private void AsyncLoadCueSheetTask_SMTV(USoundAtomCueSheetSMTV* cueSheet)
    {
        this.asyncLoadCueSheetTaskHook_SMTV.OriginalFunction(cueSheet);
        //AcbRegistry.Register(cueSheet->CueSheetName.GetString()!, cueSheet->_acbHn);
    }
}
