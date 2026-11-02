// ReSharper disable InconsistentNaming

using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Ryo.Reloaded.Audio;
using Ryo.Reloaded.Common.Memory;
using Ryo.Reloaded.CRI.CriAtom.Models;

namespace Ryo.Reloaded.CRI.CriAtom;

internal unsafe class AcbRegistry
{
    private delegate nint criAtomExAcb_LoadAcbData(nint acbData, int acbDataSize, nint awbBinder, nint awbPath, nint work, int workSize);
    private readonly SHFunction<criAtomExAcb_LoadAcbData> _criAtomExAcb_LoadAcbData;
    
    private readonly AudioRegistry _audioRegistry;
    private readonly IRyoAllocator _allocator;
    private readonly ConcurrentDictionary<string, AcbEntry> _acbs = [];
    private readonly ConcurrentDictionary<nint, string> _acbHnToName = [];

    public AcbRegistry(AudioRegistry audioRegistry, IRyoAllocator allocator)
    {
        _audioRegistry = audioRegistry;
        _allocator = allocator;

        _criAtomExAcb_LoadAcbData = new(criAtomExAcb_LoadAcbDataImpl);
    }
    
    public Cue GetCue(string acbName, int cueId) => _acbs[acbName].GetCue(cueId);

    public Cue GetCue(string acbName, string cueName) => _acbs[acbName].GetCue(cueName);

    public Cue GetCue(CriAtomExAcbHn* acbHn, int cueId) => GetCue(_acbHnToName[(nint)acbHn], cueId);

    public Cue GetCue(CriAtomExAcbHn* acbHn, string cueName) => GetCue(_acbHnToName[(nint)acbHn], cueName);

    private nint criAtomExAcb_LoadAcbDataImpl(nint acbData, int acbDataSize, nint awbBinder, nint awbPath, nint work, int workSize)
    {
        var acbSpan = new ReadOnlySpan<byte>((void*)acbData, acbDataSize);
        var acb = new AcbBank(acbSpan);
        Log.Debug($"{nameof(criAtomExAcb_LoadAcbData)} || Loading ACB: {acb.Name}");
        
        var registeredCues = _audioRegistry.GetRegisteredCuesForAcb(acb.Name);

        var existingCueIds = new HashSet<int>();
        var existingCueNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var cue in acb.Cues())
        {
            existingCueIds.Add((int)cue.Id);
            existingCueNames.Add(cue.Name);
        }
        
        //var existingCueIds = acb.Cues().Select(x => (int)x.Id).ToHashSet();

        var rebuildRequired = false;
        foreach (var cue in _audioRegistry.GetRegisteredCuesForAcb(acb.Name))
        {
            if (existingCueNames.Contains(cue.Identifier))
            {
                // TODO: Possibly support creating cues by cue name,
                // but auto-assigning cue IDs could be problematic.
                // Would have to first add all new cues using IDs, then
                // add the cues using name.
                continue;
            }
            
            if (int.TryParse(cue.Identifier, out var newCueId) && !existingCueIds.Contains(newCueId))
            {
                rebuildRequired = true;
                if (cue.Config.TemplateCueId != null)
                {
                    acb.AddCue((uint)newCueId, (uint)cue.Config.TemplateCueId.Value);
                    Log.Debug($"Cue Added || ACB: {acb.Name}, Cue: {cue.Identifier}, Template Cue ID: {cue.Config.TemplateCueId.Value}");
                    continue;
                }

                if (cue.Config.TemplateCueName != null)
                {
                    acb.AddCue((uint)newCueId, cue.Config.TemplateCueName);
                    Log.Debug($"Cue Added || ACB: {acb.Name}, Cue: {cue.Identifier}, Template Cue: {cue.Config.TemplateCueName}");
                    continue;
                }
                
                var volume = cue.Config.Volume ?? 1.0f;
                var categoryIds = cue.Config.CategoryIds?.Select(x => (uint)x).ToList() ?? [];
                acb.AddCue((uint)newCueId,
                    new CueOptions
                    {
                        Volume = volume,
                        CategoryIds = categoryIds,
                    });
                
                Log.Debug($"Cue Added || ACB: {acb.Name}, Cue: {cue.Identifier}, Volume: {volume}, Categories: {string.Join(", ", categoryIds)}");
            }
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
        
            acbHn = _criAtomExAcb_LoadAcbData.Hook.OriginalFunction(rebuiltAcbBuffer, rebuiltAcb.Length, awbBinder, awbPath, work, workSize);
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
