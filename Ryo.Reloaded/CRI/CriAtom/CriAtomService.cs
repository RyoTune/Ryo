// ReSharper disable InconsistentNaming

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Reloaded.Hooks.Definitions;
using Ryo.Reloaded.Audio;
using Ryo.Reloaded.Audio.Models.Containers;
using Ryo.Reloaded.Common.Memory;
using Ryo.Reloaded.CRI.CriAtom.Models;

namespace Ryo.Reloaded.CRI.CriAtom;

internal unsafe class CriAtomService
{
    public delegate uint CRIATOM_BuildCueWaveformSource(PlaybackContext* pPlaybackContext, WaveformSourceEvent* pWaveformSourceEvent);
    private readonly SHFunction<CRIATOM_BuildCueWaveformSource> _CRIATOM_BuildCueWaveformSource;
    
    private delegate void CriAtomExCueLinkCallback(nint userData, CriAtomExCueLinkInfo* info);
    private IReverseWrapper<CriAtomExCueLinkCallback>? _CRIATOM_CueLinkCallback;
    
    private delegate uint CRIATOM_GetPlaybackIdFromContext(PlaybackContext* pPlaybackContext);
    private readonly SHFunction<CRIATOM_GetPlaybackIdFromContext> _CRIATOM_GetPlaybackIdFromContext;
    
    private record struct LinkCue(Cue ParentCue, uint ParentPlaybackId);
    private readonly ConcurrentDictionary<uint, LinkCue> _linkCuePlaybacks = [];
    
    private readonly AudioRegistry _audioRegistry;
    private readonly AcbRegistry _acbRegistry;
    
    public CriAtomService(AudioRegistry audioRegistry, IRyoAllocator allocator)
    {
        _audioRegistry = audioRegistry;
        _acbRegistry = new(audioRegistry, allocator);
        
        _CRIATOM_BuildCueWaveformSource = new(CRIATOM_BuildCueWaveformSourceImpl);
        _CRIERR_DispatchDiagnostic = new(CRIERR_DispatchDiagnosticImpl);
        _CRIATOM_AddFileSourceEntry = new(CRIATOM_AddFileSourceEntryImpl);
        _CRIATOM_AddAwbSourceEntry = new(CRIATOM_AddAwbSourceEntryImpl);
        _CRIATOM_GetPlaybackIdFromContext = new();
        
        Project.Scans.AddScanHook($"{nameof(CriAtomExCueLinkCallback)}_PTR", (result, hooks) =>
        {
            _CRIATOM_CueLinkCallback = hooks.CreateReverseWrapper<CriAtomExCueLinkCallback>(CriAtomExCueLinkCallbackImpl);
            var address = (nint*)result;
            if (*address == 0) *address = _CRIATOM_CueLinkCallback.NativeFunctionPtr;
            else Log.Error($"{nameof(CriAtomExCueLinkCallback)} already exists. Please report this game to the Ryo GitHub or GameBanana page.");
        });
    }

    private void CriAtomExCueLinkCallbackImpl(nint userData, CriAtomExCueLinkInfo* info)
    {
        var parentCue = ResolveSourceCue(&info->Parent);
        _linkCuePlaybacks[info->ChildPlaybackId] =  new(parentCue, info->ParentPlaybackId);
    }

    private Cue ResolveSourceCue(CriAtomExSourceInfo* info)
    {
        var cue = Cue.None;
        if (info->TryGetCueId(out var id)) cue = _acbRegistry.GetCue(info->AcbHandle, id);
        else if (info->TryGetCueName(out var name)) cue = _acbRegistry.GetCue(info->AcbHandle, name);
        else Log.Error($"Failed to resolve source cue. Type: {info->Type}");

        return cue;
    }
    
    private Cue ResolvePlaybackCue(PlaybackContext* playback)
    {
        var cue = Cue.None;
        if (playback->TryGetCueId(out var id)) cue = _acbRegistry.GetCue(playback->AcbHandle, id);
        else if (playback->TryGetCueName(out var name)) cue = _acbRegistry.GetCue(playback->AcbHandle, name);
        else Log.Error($"Failed to resolve playback cue. Type: {playback->SelectionType}");

        return cue;
    }

    private List<Cue> ResolvePlaybackCueIdentities(PlaybackContext* playback)
    {
        var identities = new List<Cue>();
        var playbackCue = ResolvePlaybackCue(playback);
        identities.Add(playbackCue);
        
        var currentPlaybackId = _CRIATOM_GetPlaybackIdFromContext.Wrapper(playback);
        while (_linkCuePlaybacks.TryRemove(currentPlaybackId, out var linkCue))
        {
            currentPlaybackId = linkCue.ParentPlaybackId;
            identities.Insert(0, linkCue.ParentCue);
        }
        
        return identities;
    }

    private bool TryGetCueContainerByAnyIdentity(IReadOnlyList<Cue> identities, [NotNullWhen(true)] out AudioContainer? container)
    {
        container = null;
        foreach (var identity in identities)
        {
            if (_audioRegistry.TryGetCueContainer(identity.Name, identity.Acb, out container)) break;
            if (_audioRegistry.TryGetCueContainer(identity.Id.ToString(), identity.Acb, out container)) break;
        }
        
        return container != null;
    }

    [ThreadStatic]
    private static RedirectScope? _redirect;
    private uint CRIATOM_BuildCueWaveformSourceImpl(PlaybackContext* pPlaybackContext, WaveformSourceEvent* pWaveformSourceEvent)
    {
        var playbackCueIdentities = ResolvePlaybackCueIdentities(pPlaybackContext);
        
        var playingLog = $"BuildCue || {string.Join(" -> ", playbackCueIdentities.Select(x => $"ACB: {x.Acb}, Cue: {x.Id} / {x.Name}"))}";
        if (Mod.Config.DevMode)
            Log.Information(playingLog);
        else
            Log.Debug(playingLog);

        var prevRedirect = _redirect;
        if (TryGetCueContainerByAnyIdentity(playbackCueIdentities, out var container))
            _redirect = new(pPlaybackContext->SourceConfig, pWaveformSourceEvent->StreamWaveId, container.Name, container.GetAudio().FilePath);
        
        try
        {
            return _CRIATOM_BuildCueWaveformSource.Hook!.OriginalFunction(pPlaybackContext, pWaveformSourceEvent);
        }
        finally
        {
            _redirect = prevRedirect;
        }
    }
    
    private delegate int CRIATOM_AddAwbSourceEntry(nint pSourceConfig, nint pAwbHandle, uint nWaveId);
    private readonly SHFunction<CRIATOM_AddAwbSourceEntry> _CRIATOM_AddAwbSourceEntry;
    private int CRIATOM_AddAwbSourceEntryImpl(nint pSourceConfig, nint pAwbHandle, uint nWaveId)
    {
        if (pSourceConfig == _redirect?.SourceConfig && nWaveId == _redirect.WaveId)
        {
            Log.Debug($"{nameof(CRIATOM_AddAwbSourceEntry)} || '{_redirect.RedirectedCue}' redirected to file.\nFile: {_redirect.ReplacementPath}");
            return _CRIATOM_AddFileSourceEntry.Wrapper(pSourceConfig, 0, _redirect.ReplacementPath.AsPointer(Encoding.UTF8, true));
        }

        return _CRIATOM_AddAwbSourceEntry.Hook!.OriginalFunction(pSourceConfig, pAwbHandle, nWaveId);
    }
    
    private delegate void CRIERR_DispatchDiagnostic(int severity, nint message, nint argument1, nint argument2, nint additionalData);
    private readonly SHFunction<CRIERR_DispatchDiagnostic> _CRIERR_DispatchDiagnostic;
    private void CRIERR_DispatchDiagnosticImpl(int severity, nint message, nint argument1, nint argument2, nint additionalData)
    {
        if (Mod.Config.CriLogsEnabled) Log.Warning($"(CRI) {Marshal.PtrToStringUTF8(message)}");
        _CRIERR_DispatchDiagnostic.Hook!.OriginalFunction(severity, message, argument1, argument2, additionalData);
    }

    private delegate int CRIATOM_AddFileSourceEntry(nint pSourceConfig, nint pBinder, nint pszPath);
    private readonly SHFunction<CRIATOM_AddFileSourceEntry> _CRIATOM_AddFileSourceEntry;

    private int CRIATOM_AddFileSourceEntryImpl(nint pSourceConfig, nint pBinder, nint pszPath)
    {
        return _CRIATOM_AddFileSourceEntry.Hook!.OriginalFunction(pSourceConfig, pBinder, pszPath);
    }
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct WaveformSourceEvent
{
    [FieldOffset(0x10)]
    private byte* _waveformInfo;

    public readonly ushort MemoryWaveId =>
        *(ushort*)_waveformInfo;

    public readonly ushort StreamWaveId =>
        *(ushort*)(_waveformInfo + 0x2E);

    public readonly byte StreamingMode =>
        _waveformInfo[0x08];
}

public sealed record RedirectScope(nint SourceConfig, uint WaveId, string RedirectedCue, string ReplacementPath);