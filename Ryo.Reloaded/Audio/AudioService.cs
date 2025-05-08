using System.IO.Hashing;
using Ryo.Definitions.Structs;
using Ryo.Interfaces;
using Ryo.Reloaded.Audio.Services;
using Ryo.Reloaded.CRI.CriAtomEx;
using SharedScans.Interfaces;
using System.Runtime.InteropServices;
using static Ryo.Definitions.Functions.CriAtomExFunctions;

namespace Ryo.Reloaded.Audio;

internal unsafe class AudioService
{
    private readonly ICriAtomEx criAtomEx;
    private readonly CriAtomRegistry criAtomRegistry;
    private readonly AudioRegistry audioRegistry;

    private readonly RyoService ryo;
    private readonly VirtualCueService virtualCue;

    private readonly HookContainer<criAtomExPlayer_SetCueName> setCueName;
    private readonly HookContainer<criAtomExPlayer_SetCueId> setCueId;
    private readonly HookContainer<criAtomExPlayer_SetFile> setFile;
    private readonly HookContainer<criAtomExPlayer_SetWaveId> setWaveId;
    private readonly HookContainer<criAtomExPlayer_SetData> setData;

    private bool devMode;

    public AudioService(
        string game,
        ISharedScans scans,
        ICriAtomEx criAtomEx,
        CriAtomRegistry criAtomRegistry,
        AudioRegistry audioRegistry)
    {
        this.criAtomEx = criAtomEx;
        this.criAtomRegistry = criAtomRegistry;
        this.audioRegistry = audioRegistry;

        this.ryo = new(criAtomEx, criAtomRegistry, scans);
        this.virtualCue = new(game, scans, criAtomRegistry, audioRegistry);

        GameDefaults.ConfigureCriAtom(game, criAtomEx);

        this.setCueName = scans.CreateHook<criAtomExPlayer_SetCueName>(this.CriAtomExPlayer_SetCueName, Mod.NAME);
        this.setCueId = scans.CreateHook<criAtomExPlayer_SetCueId>(this.CriAtomExPlayer_SetCueId, Mod.NAME);
        this.setFile = scans.CreateHook<criAtomExPlayer_SetFile>(this.CriAtomExPlayer_SetFile, Mod.NAME);
        this.setWaveId = scans.CreateHook<criAtomExPlayer_SetWaveId>(this.CriAtomExPlayer_SetWaveId, Mod.NAME);
        
        // Still WIP.
#if DEBUG
        this.setData = scans.CreateHook<criAtomExPlayer_SetData>(this.CriAtomExPlayer_SetData, Mod.NAME);
#endif
    }

    public void SetDevMode(bool devMode)
        => this.devMode = devMode;

    private bool SetCue(nint playerHn, nint acbHn, Cue cue)
    {
        var player = this.criAtomRegistry.GetPlayerByHn(playerHn)!;
        var acbName = this.criAtomRegistry.GetAcbByHn(acbHn)?.Name ?? "Unknown";

        if (this.devMode)
        {
            var cueIdString = cue.Id == -1 ? "(N/A)" : cue.Id.ToString();
            var cueNameString = cue.Name ?? "(N/A)";

            Log.Information($"{nameof(SetCue)} || Player: {player.Id} || ACB: {acbName} || Cue: {cueIdString} / {cueNameString}");
            if (cue.Categories?.Length > 0)
            {
                Log.Debug($"Categories: {string.Join(", ", cue.Categories.Select(x => x.ToString()))}");
            }
        }

        if (cue.Id != -1 && this.audioRegistry.TryGetCueContainer(cue.Id.ToString(), acbName, out var idContainer))
        {
            this.ryo.SetAudio(player, idContainer, idContainer.CategoryIds ?? cue.Categories);
            return true;
        }
        else if (cue.Name != null && this.audioRegistry.TryGetCueContainer(cue.Name, acbName, out var nameContainer))
        {
            this.ryo.SetAudio(player, nameContainer, nameContainer.CategoryIds ?? cue.Categories);
            return true;
        }
        else
        {
            this.ryo.ResetCustomVolumes(player, cue.Categories ?? []);
            return false;
        }
    }

    private void CriAtomExPlayer_SetCueName(nint playerHn, nint acbHn, byte* cueNameStr)
    {
        var cueInfo = (CriAtomExCueInfoTag*)Marshal.AllocHGlobal(sizeof(CriAtomExCueInfoTag));

        var cueName = Marshal.PtrToStringAnsi((nint)cueNameStr)!;
        var cueId = -1;
        int[]? categories = null;
        
        if (this.criAtomEx.Acb_GetCueInfoByName(acbHn, (nint)cueNameStr, cueInfo))
        {
            cueId = cueInfo->id;
            categories = cueInfo->GetCategories();
        }

        var cue = new Cue() { Name = cueName, Id = cueId, Categories = categories };
        if (this.SetCue(playerHn, acbHn, cue) == false)
        {
            this.setCueName.Hook!.OriginalFunction(playerHn, acbHn, cueNameStr);
        }

        Marshal.FreeHGlobal((nint)cueInfo);
    }

    private void CriAtomExPlayer_SetCueId(nint playerHn, nint acbHn, int cueId)
    {
        var cueInfo = (CriAtomExCueInfoTag*)Marshal.AllocHGlobal(sizeof(CriAtomExCueInfoTag));
        string? cueName = null;
        int[]? categories = null;

        if (this.criAtomEx.Acb_GetCueInfoById(acbHn, cueId, cueInfo))
        {
            cueName = Marshal.PtrToStringAnsi(cueInfo->name)!;
            categories = cueInfo->GetCategories();
        }

        var cue = new Cue() { Name = cueName, Id = cueId, Categories = categories };
        if (this.SetCue(playerHn, acbHn, cue) == false)
        {
            this.setCueId.Hook!.OriginalFunction(playerHn, acbHn, cueId);
        }

        Marshal.FreeHGlobal((nint)cueInfo);
    }

    private void CriAtomExPlayer_SetFile(nint playerHn, nint criBinderHn, byte* path)
    {
        var filePath = Marshal.PtrToStringAnsi((nint)path);
        var player = this.criAtomRegistry.GetPlayerByHn(playerHn)!;

        if (this.devMode)
        {
            Log.Information($"{nameof(criAtomExPlayer_SetFile)} || Player: {player.Id} || {filePath}");
        }

        if (filePath != null && this.audioRegistry.TryGetFileContainer(filePath, out var fileContainer))
        {
            this.ryo.SetAudio(player, fileContainer, fileContainer.CategoryIds);
        }
        else
        {
            this.setFile.Hook!.OriginalFunction(playerHn, criBinderHn, path);
        }
    }

    private void CriAtomExPlayer_SetWaveId(nint playerHn, nint awbHn, int waveId)
    {
        var awbPath = this.criAtomRegistry.GetAwbByHn(awbHn)?.Path ?? "Unknown";
        var player = this.criAtomRegistry.GetPlayerByHn(playerHn)!;

        if (this.devMode)
        {
            Log.Information($"{nameof(criAtomExPlayer_SetWaveId)} || Player: {player.Id} || AWB: {awbPath} || Wave ID: {waveId}");
        }

        if (awbPath != null && this.audioRegistry.TryGetFileContainer($"{awbPath.Trim('/')}/{waveId}.wave", out var fileContainer))
        {
            this.ryo.SetAudio(player, fileContainer, fileContainer.CategoryIds);
        }
        else
        {
            this.setWaveId.Hook!.OriginalFunction(playerHn, awbHn, waveId);
        }
    }

    private readonly Dictionary<AudioData, string> audioHashes = [];
    private void CriAtomExPlayer_SetData(nint playerHn, byte* buffer, int size)
    {
        var player = this.criAtomRegistry.GetPlayerByHn(playerHn)!;
        
        var audioData = new AudioData((nint)buffer, size);
        if (!this.audioHashes.TryGetValue(audioData, out var hash))
        {
            var hashBytes = XxHash3.Hash(new ReadOnlySpan<byte>(buffer, size));
            hash = Convert.ToHexString(hashBytes);
            this.audioHashes[audioData] = hash;
        }
        
        if (this.devMode) Log.Information($"{nameof(criAtomExPlayer_SetData)} || Player: {player.Id} || Hash: {hash}");

        if (this.audioRegistry.TryGetDataContainer(hash, out var dataContainer))
        {
            this.ryo.SetAudio(player, dataContainer, dataContainer.CategoryIds);
            this.setData.Hook!.OriginalFunction(playerHn, (byte*)0, 0);
        }
        else
        {
            this.setData.Hook!.OriginalFunction(playerHn, buffer, size);
        }
    }

    private class Cue
    {
        public int Id { get; init; } = -1;

        public string? Name { get; init; }

        public int[]? Categories { get; init; }
    }

    private readonly struct AudioData : IEquatable<AudioData>
    {
        private readonly nint address;
        private readonly int length;
        
        public AudioData(nint address, int length) 
        {
            this.address = address;
            this.length = length;
        }
        
        public bool Equals(AudioData other) => address == other.address && length == other.length;

        public override int GetHashCode() => HashCode.Combine(address, length);
    }
}
