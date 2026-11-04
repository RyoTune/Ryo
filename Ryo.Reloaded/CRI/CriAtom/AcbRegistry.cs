// ReSharper disable InconsistentNaming

using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Ryo.Reloaded.Audio;
using Ryo.Reloaded.Audio.Models;
using Ryo.Reloaded.Common.Memory;
using Ryo.Reloaded.CRI.CriAtom.Models;

namespace Ryo.Reloaded.CRI.CriAtom;

internal unsafe class AcbRegistry
{
    private delegate nint criAtomExAcb_LoadAcbData(void* acbData, int acbDataSize, nint awbBinder, nint awbPath, nint work, int workSize);
    private readonly SHFunction<criAtomExAcb_LoadAcbData> _criAtomExAcb_LoadAcbData;
    
    private delegate void criAtomConfig_Register(void* acfData, int acfDataSize);
    private readonly SHFunction<criAtomConfig_Register> _criAtomConfig_Register;
    
    private readonly AudioRegistry _audioRegistry;
    private readonly IRyoAllocator _allocator;
    private readonly ConcurrentDictionary<string, AcbEntry> _acbs = [];
    private readonly ConcurrentDictionary<nint, string> _acbHnToName = [];
    private AcfCatalog? _acf;

    public AcbRegistry(AudioRegistry audioRegistry, IRyoAllocator allocator)
    {
        _audioRegistry = audioRegistry;
        _allocator = allocator;

        _criAtomExAcb_LoadAcbData = new(criAtomExAcb_LoadAcbDataImpl);
        _criAtomConfig_Register = new(criAtomConfig_RegisterImpl);
    }
    
    public Cue GetCue(string acbName, int cueId) => _acbs[acbName].GetCue(cueId);

    public Cue GetCue(string acbName, string cueName) => _acbs[acbName].GetCue(cueName);

    public Cue GetCue(CriAtomExAcbHn* acbHn, int cueId) => GetCue(_acbHnToName[(nint)acbHn], cueId);

    public Cue GetCue(CriAtomExAcbHn* acbHn, string cueName) => GetCue(_acbHnToName[(nint)acbHn], cueName);

    private nint criAtomExAcb_LoadAcbDataImpl(void* acbData, int acbDataSize, nint awbBinder, nint awbPath, nint work, int workSize)
    {
        var acbSpan = new ReadOnlySpan<byte>(acbData, acbDataSize);
        var acb = new AcbBank(acbSpan, _acf);
        Log.Debug($"{nameof(criAtomExAcb_LoadAcbData)} || Loading ACB: {acb.Name}");
        
        var registeredCues = _audioRegistry.GetRegisteredCuesForAcb(acb.Name);

        var rebuildRequired = false;
        foreach (var cue in _audioRegistry.GetRegisteredCuesForAcb(acb.Name))
        {
            var isCueId = int.TryParse(cue.Identifier, out var cueId);
            var resolvedVolume = ResolveCueVolume(cue.Config);
            
            // Edit existing cue data, currently just volume.
            if ((isCueId && acb.TryGetCueHandle((uint)cueId, out var handle))
                || acb.TryGetCueHandle(cue.Identifier, out handle))
            {
                if (resolvedVolume != null)
                {
                    acb.SetCueVolume(handle.Id, resolvedVolume.Value);
                    Log.Debug($"Cue Edit || ACB: {acb.Name}, Cue: {cue.Identifier}, Volume: {resolvedVolume}");
                    rebuildRequired = true;
                }
                
                continue;
            }

            // TODO: Currently new cues by name is not supported.
            if (!isCueId) continue;

            CueHandle? newCue = null;
            if (cue.Config.TemplateCueId != null)
            {
                newCue = acb.AddCue((uint)cueId, (uint)cue.Config.TemplateCueId.Value);
                Log.Debug($"Cue Added || ACB: {acb.Name}, Cue: {cue.Identifier}, Template Cue ID: {cue.Config.TemplateCueId.Value}");
            }
            else if (cue.Config.TemplateCueName != null)
            {
                newCue = acb.AddCue((uint)cueId, cue.Config.TemplateCueName);
                Log.Debug($"Cue Added || ACB: {acb.Name}, Cue: {cue.Identifier}, Template Cue: {cue.Config.TemplateCueName}");
            }
            else
            {
                var categoryIds = cue.Config.CategoryIds?.Select(x => (uint)x).ToList() ?? [];
                newCue = acb.AddCue((uint)cueId,
                    new CueOptions
                    {
                        CategoryIds = categoryIds,
                    });
                
                Log.Debug($"Cue Added || ACB: {acb.Name}, Cue: {cue.Identifier}, Volume: {resolvedVolume}, Categories: {string.Join(", ", categoryIds)}");
            }
            
            acb.SetCueVolume(newCue.Id, resolvedVolume ?? 1.0f);
            rebuildRequired = true;
        }

        _acbs[acb.Name] = new(acb);

        // TODO: Support custom ACBs.
        // Probably need to hook criAtomExAcb_LoadAcbFile, then redirect to here with our ACB data.
        nint acbHn = 0;
        if (rebuildRequired || Mod.Config.ForceRebuildAcbs)
        {
            var rebuiltAcb = acb.ToBytes();
            var rebuiltAcbBuffer = _allocator.Malloc(rebuiltAcb.Length);
            Marshal.Copy(rebuiltAcb, 0, rebuiltAcbBuffer, rebuiltAcb.Length);
        
            acbHn = _criAtomExAcb_LoadAcbData.Hook.OriginalFunction((void*)rebuiltAcbBuffer, rebuiltAcb.Length, awbBinder, awbPath, work, workSize);
            if (acbHn != 0)
            {
                Log.Debug($"Rebuilt ACB: {acb.Name}");
                
                // TODO: Free rebuilt ACB memory blocks when ACBs are released in criAtomExAcb_Release.
                // In CriWare UE, USoundAtomCueSheet stores and frees the original ACB block, but will not
                // clean up our newly created one.
                //_allocator.Free(acbData);
            }
            else
            {
                Log.Error($"Failed to rebuild ACB: {acb.Name}. Loading original ACB.");
                _allocator.Free(rebuiltAcbBuffer);
                
                acbHn = _criAtomExAcb_LoadAcbData.Hook.OriginalFunction(acbData, acbDataSize, awbBinder, awbPath, work, workSize);
            }
        }
        else
        {
            acbHn = _criAtomExAcb_LoadAcbData.Hook.OriginalFunction(acbData, acbDataSize, awbBinder, awbPath, work, workSize);
        }
        
        _acbHnToName[acbHn] = acb.Name;
        return acbHn;
    }
    
    private void criAtomConfig_RegisterImpl(void* acfData, int acfDataSize)
    {
        var acfSpan = new ReadOnlySpan<byte>(acfData, acfDataSize);
        _acf = new(acfSpan);
        
        Log.Debug($"{nameof(criAtomConfig_Register)} || Loading ACF: {_acf.Name}");
        _criAtomConfig_Register.Hook!.OriginalFunction(acfData, acfDataSize);
    }

    private static float? ResolveCueVolume(CueConfig cue)
    {
        if (cue.Volume != null) return cue.Volume;
        if (cue.ClassicVolume?.Volume != null)
        {
            if (cue.ClassicVolume.ShouldBoost)
            {
                if (cue.ClassicVolume.Volume.Value >= 1.0f) return cue.ClassicVolume.Volume.Value;
                return (float?)(cue.ClassicVolume.Volume.Value + Mod.Config.RyoClassicVolumeBoost);
            }

            return cue.ClassicVolume.Volume.Value;
        }
        
        return null;
    }
    
    private class AcbEntry
    {
        private readonly Dictionary<int, Cue> _idToCue = [];
        private readonly Dictionary<string, int> _nameToId = [];
        
        public AcbEntry(AcbBank acb)
        {
            foreach (var cue in acb.Cues())
            {
                _nameToId[cue.Name] = (int)cue.Id;
                _idToCue[(int)cue.Id] = new(acb.Name, (int)cue.Id, cue.Name);
            }
        }
        
        public Cue GetCue(int id) => _idToCue[id];
        
        public Cue GetCue(string name) => _idToCue[_nameToId[name]];
    }
}
