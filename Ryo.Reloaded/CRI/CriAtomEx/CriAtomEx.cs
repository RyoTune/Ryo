using System.Runtime.InteropServices;
using Ryo.Interfaces;
using Ryo.Definitions.Structs;
using Ryo.Definitions.Classes;
using static Ryo.Definitions.Functions.CriAtomExFunctions;
using Ryo.Definitions.Enums;
using SharedScans.Interfaces;

namespace Ryo.Reloaded.CRI.CriAtomEx;

internal unsafe class CriAtomEx : ICriAtomEx
{
    private readonly string game;
    private readonly CriAtomExPatterns patterns;

    private readonly Dictionary<int, CriAtomExPlayerConfigTag> playerConfigs = new();
    private readonly List<PlayerConfig> players = new();

    private bool devMode;

    // Hooks.
    private readonly HookContainer<criAtomExPlayer_Create> create;
    private readonly HookContainer<criAtomExAcb_LoadAcbFile> loadAcbFile;
    private readonly HookContainer<criAtomExAcb_LoadAcbData> loadAcbData;
    private readonly HookContainer<criAtomAwb_LoadToc> loadToc;

    // Wrappers.
    private readonly WrapperContainer<criAtomExPlayer_SetCueId> setCueId;
    private readonly WrapperContainer<criAtomExPlayer_SetCueName> setCueName;
    private readonly WrapperContainer<criAtomExPlayer_SetFile> setFile;
    private readonly WrapperContainer<criAtomExPlayer_Start> start;
    private readonly WrapperContainer<criAtomExPlayer_SetWaveId> setWaveId;
    private readonly WrapperContainer<criAtomExPlayer_SetData> setData;
    private readonly WrapperContainer<criAtomConfig_GetCategoryIndexById> getCategoryIndex;
    private readonly WrapperContainer<criAtomExAcb_GetCueInfoById> getCueInfoById;
    private readonly WrapperContainer<criAtomExAcb_GetCueInfoByName> getCueInfoByName;
    private readonly WrapperContainer<criAtomExPlayer_SetStartTime> setStartTime;
    private readonly WrapperContainer<criAtomExPlayback_GetTimeSyncedWithAudioMicro> getTimeSyncedWithAudioMicro;
    private readonly WrapperContainer<criAtomExPlayer_GetStatus> getStatus;
    private readonly WrapperContainer<criAtomExPlayer_ResetParameters> resetParameters;
    private readonly WrapperContainer<criAtomExPlayer_GetNumPlayedSamples> getNumPlayedSamples;
    private readonly WrapperContainer<criAtomExCategory_SetVolumeById> setVolumeById;
    private readonly WrapperContainer<criAtomExCategory_GetVolume> getCategoryVolume;
    private readonly WrapperContainer<criAtomExCategory_GetVolumeById> getVolumeById;
    private readonly WrapperContainer<criAtomExCategory_SetVolume> setVolumeByIndex;
    private readonly WrapperContainer<criAtomExPlayer_SetFormat> setFormat;
    private readonly WrapperContainer<criAtomExPlayer_SetSamplingRate> setSamplingRate;
    private readonly WrapperContainer<criAtomExPlayer_SetNumChannels> setNumChannels;
    private readonly WrapperContainer<criAtomExPlayer_SetVolume> setVolume;
    private readonly WrapperContainer<criAtomExPlayer_SetCategoryById> setCategoryById;
    private readonly WrapperContainer<criAtomExPlayer_GetLastPlaybackId> getLastPlaybackId;
    private readonly WrapperContainer<criAtomExPlayer_SetCategoryByName> setCategoryByName;
    private readonly WrapperContainer<criAtomExPlayer_GetCategoryInfo> getCategoryInfo;
    private readonly WrapperContainer<criAtomExPlayer_UpdateAll> updateAll;
    private readonly WrapperContainer<criAtomExPlayer_LimitLoopCount> limitLoopCount;
    private readonly WrapperContainer<criAtomExPlayer_Stop> stop;
    private readonly WrapperContainer<criAtomExPlayer_SetAisacControlByName> setAisacControlByName;
    private readonly WrapperContainer<criAtomExAcf_GetCategoryInfoByIndex> acfGetCategoryInfoById;

    public CriAtomEx(string game, ISharedScans scans)
    {
        this.game = game;
        this.patterns = CriAtomExGames.GetGamePatterns(game);

        scans.AddScan<criAtomExPlayer_SetCueId>(this.patterns.criAtomExPlayer_SetCueId);
        this.setCueId = scans.CreateWrapper<criAtomExPlayer_SetCueId>(Mod.NAME);

        scans.AddScan<criAtomExPlayer_SetCueName>(this.patterns.criAtomExPlayer_SetCueName);
        this.setCueName = scans.CreateWrapper<criAtomExPlayer_SetCueName>(Mod.NAME);

        scans.AddScan<criAtomExPlayer_Start>(this.patterns.criAtomExPlayer_Start);
        this.start = scans.CreateWrapper<criAtomExPlayer_Start>(Mod.NAME);

        scans.AddScan<criAtomExPlayer_SetFile>(this.patterns.criAtomExPlayer_SetFile);
        this.setFile = scans.CreateWrapper<criAtomExPlayer_SetFile>(Mod.NAME);

        scans.AddScan<criAtomExPlayer_SetWaveId>(this.patterns.criAtomExPlayer_SetWaveId);
        this.setWaveId = scans.CreateWrapper<criAtomExPlayer_SetWaveId>(Mod.NAME);

        scans.AddScan<criAtomExPlayer_SetData>(this.patterns.criAtomExPlayer_SetData);
        this.setData = scans.CreateWrapper<criAtomExPlayer_SetData>(Mod.NAME);

        scans.AddScan<criAtomExAcb_GetCueInfoById>(this.patterns.criAtomExAcb_GetCueInfoById);
        this.getCueInfoById = scans.CreateWrapper<criAtomExAcb_GetCueInfoById>(Mod.NAME);

        scans.AddScan<criAtomExAcb_GetCueInfoByName>(this.patterns.criAtomExAcb_GetCueInfoByName);
        this.getCueInfoByName = scans.CreateWrapper<criAtomExAcb_GetCueInfoByName>(Mod.NAME);

        scans.AddScan<criAtomConfig_GetCategoryIndexById>(this.patterns.criAtomConfig_GetCategoryIndexById);
        this.getCategoryIndex = scans.CreateWrapper<criAtomConfig_GetCategoryIndexById>(Mod.NAME);

        scans.AddScan<criAtomExPlayer_SetStartTime>(this.patterns.criAtomExPlayer_SetStartTime);
        this.setStartTime = scans.CreateWrapper<criAtomExPlayer_SetStartTime>(Mod.NAME);

        scans.AddScan<criAtomExPlayback_GetTimeSyncedWithAudioMicro>(this.patterns.criAtomExPlayback_GetTimeSyncedWithAudioMicro);
        this.getTimeSyncedWithAudioMicro = scans.CreateWrapper<criAtomExPlayback_GetTimeSyncedWithAudioMicro>(Mod.NAME);

        scans.AddScan<criAtomExPlayer_GetStatus>(this.patterns.criAtomExPlayer_GetStatus);
        this.getStatus = scans.CreateWrapper<criAtomExPlayer_GetStatus>(Mod.NAME);

        scans.AddScan<criAtomExPlayer_ResetParameters>(this.patterns.criAtomExPlayer_ResetParameters);
        this.resetParameters = scans.CreateWrapper<criAtomExPlayer_ResetParameters>(Mod.NAME);

        scans.AddScan<criAtomExAcf_GetCategoryInfoByIndex>(this.patterns.criAtomExAcf_GetCategoryInfoByIndex);
        this.acfGetCategoryInfoById = scans.CreateWrapper<criAtomExAcf_GetCategoryInfoByIndex>(Mod.NAME);

        scans.AddScan<criAtomExCategory_GetVolume>(this.patterns.criAtomExCategory_GetVolume);
        this.getCategoryVolume = scans.CreateWrapper<criAtomExCategory_GetVolume>(Mod.NAME);
        
        scans.AddScan<criAtomExCategory_SetVolumeById>(this.patterns.criAtomExCategory_SetVolumeById);
        this.setVolumeById = scans.CreateWrapper<criAtomExCategory_SetVolumeById>(Mod.NAME);

        scans.AddScan<criAtomExCategory_SetVolume>(this.patterns.criAtomExCategory_SetVolume);
        this.setVolumeByIndex = scans.CreateWrapper<criAtomExCategory_SetVolume>(Mod.NAME);

        scans.AddScan<criAtomExPlayer_Create>(this.patterns.criAtomExPlayer_Create);
        this.create = scans.CreateHook<criAtomExPlayer_Create>(this.Player_Create, Mod.NAME);

        scans.AddScan<criAtomExAcb_LoadAcbFile>(this.patterns.criAtomExAcb_LoadAcbFile);
        this.loadAcbFile = scans.CreateHook<criAtomExAcb_LoadAcbFile>(this.Acb_LoadAcbFile, Mod.NAME);

        scans.AddScan<criAtomAwb_LoadToc>(this.patterns.criAtomAwb_LoadToc);
        this.loadToc = scans.CreateHook<criAtomAwb_LoadToc>(this.Awb_LoadToc, Mod.NAME);

        scans.AddScan<criAtomExAcb_LoadAcbData>(this.patterns.criAtomExAcb_LoadAcbData);
        this.loadAcbData = scans.CreateHook<criAtomExAcb_LoadAcbData>(this.Acb_LoadAcbData, Mod.NAME);

        scans.AddScan<criAtomExCategory_GetVolumeById>(this.patterns.criAtomExCategory_GetVolumeById);
        this.getVolumeById = scans.CreateWrapper<criAtomExCategory_GetVolumeById>(Mod.NAME);

        scans.AddScan<criAtomExPlayer_GetNumPlayedSamples>(this.patterns.criAtomExPlayer_GetNumPlayedSamples);
        this.getNumPlayedSamples = scans.CreateWrapper<criAtomExPlayer_GetNumPlayedSamples>(Mod.NAME);

        scans.AddScan<criAtomExPlayer_SetFormat>(this.patterns.criAtomExPlayer_SetFormat);
        this.setFormat = scans.CreateWrapper<criAtomExPlayer_SetFormat>(Mod.NAME);

        scans.AddScan<criAtomExPlayer_SetSamplingRate>(this.patterns.criAtomExPlayer_SetSamplingRate);
        this.setSamplingRate = scans.CreateWrapper<criAtomExPlayer_SetSamplingRate>(Mod.NAME);

        scans.AddScan<criAtomExPlayer_SetNumChannels>(this.patterns.criAtomExPlayer_SetNumChannels);
        this.setNumChannels = scans.CreateWrapper<criAtomExPlayer_SetNumChannels>(Mod.NAME);

        scans.AddScan<criAtomExPlayer_SetVolume>(this.patterns.criAtomExPlayer_SetVolume);
        this.setVolume = scans.CreateWrapper<criAtomExPlayer_SetVolume>(Mod.NAME);

        scans.AddScan<criAtomExPlayer_SetCategoryById>(this.patterns.criAtomExPlayer_SetCategoryById);
        this.setCategoryById = scans.CreateWrapper<criAtomExPlayer_SetCategoryById>(Mod.NAME);

        scans.AddScan<criAtomExPlayer_GetLastPlaybackId>(this.patterns.criAtomExPlayer_GetLastPlaybackId);
        this.getLastPlaybackId = scans.CreateWrapper<criAtomExPlayer_GetLastPlaybackId>(Mod.NAME);

        scans.AddScan<criAtomExPlayer_SetCategoryByName>(this.patterns.criAtomExPlayer_SetCategoryByName);
        this.setCategoryByName = scans.CreateWrapper<criAtomExPlayer_SetCategoryByName>(Mod.NAME);

        scans.AddScan<criAtomExPlayer_GetCategoryInfo>(this.patterns.criAtomExPlayer_GetCategoryInfo);
        this.getCategoryInfo = scans.CreateWrapper<criAtomExPlayer_GetCategoryInfo>(Mod.NAME);

        scans.AddScan<criAtomExPlayer_UpdateAll>(this.patterns.criAtomExPlayer_UpdateAll);
        this.updateAll = scans.CreateWrapper<criAtomExPlayer_UpdateAll>(Mod.NAME);

        scans.AddScan<criAtomExPlayer_LimitLoopCount>(this.patterns.criAtomExPlayer_LimitLoopCount);
        this.limitLoopCount = scans.CreateWrapper<criAtomExPlayer_LimitLoopCount>(Mod.NAME);

        scans.AddScan<criAtomExPlayer_Stop>(this.patterns.criAtomExPlayer_Stop);
        this.stop = scans.CreateWrapper<criAtomExPlayer_Stop>(Mod.NAME);

        scans.AddScan<criAtomExPlayer_SetAisacControlByName>(this.patterns.criAtomExPlayer_SetAisacControlByName);
        this.setAisacControlByName = scans.CreateWrapper<criAtomExPlayer_SetAisacControlByName>(Mod.NAME);
    }

    public void SetDevMode(bool devMode) => this.devMode = devMode;

    public void Player_ResetParameters(nint playerHn) => this.resetParameters.Wrapper(playerHn);

    public bool Acb_GetCueInfoById(nint acbHn, int id, CriAtomExCueInfoTag* info) => this.getCueInfoById.Wrapper(acbHn, id, info);

    public bool Acb_GetCueInfoByName(nint acbHn, nint name, CriAtomExCueInfoTag* info) => this.getCueInfoByName.Wrapper(acbHn, name, info);

    public bool Acf_GetCategoryInfoById(ushort id, CriAtomExCategoryInfoTag* info) => this.acfGetCategoryInfoById.Wrapper(id, info);

    public nint Acb_LoadAcbData(nint acbData, int acbDataSize, nint awbBinder, nint awbPath, void* work, int workSize)
    {
        var acbHn = (AcbHn*)this.loadAcbData.Hook!.OriginalFunction(acbData, acbDataSize, awbBinder, awbPath, work, workSize);
        CriAtomRegistry.RegisterAcb(acbHn);
        return (nint)acbHn;
    }

    public PlayerConfig? GetPlayerByAcbPath(string acbPath)
    {
        var player = this.players.FirstOrDefault(x => x.Acb.AcbPath == acbPath);
        if (player != null)
        {
            Log.Debug($"PlayerHn: {player.PlayerHn} || ACB Path: {player.Acb.AcbPath} || ID: {this.players.IndexOf(player)}");
        }

        return player;
    }

    public PlayerConfig? GetPlayerByHn(nint playerHn)
        => this.players.FirstOrDefault(x => x.PlayerHn == playerHn);

    public PlayerConfig? GetPlayerById(int playerId)
        => this.players.FirstOrDefault(x => x.Id == playerId);

    public CriAtomExPlayerStatusTag Player_GetStatus(nint playerHn) => this.getStatus.Wrapper(playerHn);

    public void Player_LimitLoopCount(nint playerHn, int count) => this.limitLoopCount.Wrapper(playerHn, count);

    public void Player_Stop(nint playerHn) => this.stop.Wrapper(playerHn);

    public void Player_SetAisacControlByName(nint playerHn, byte* controlName, float controlValue)  => this.setAisacControlByName.Wrapper(playerHn, controlName, controlValue);

    public void Player_SetCueId(nint playerHn, nint acbHn, int cueId)  => this.setCueId.Wrapper(playerHn, acbHn, cueId);

    public void Player_SetCueName(nint playerHn, nint acbHn, byte* cueName)  => this.setCueName.Wrapper(playerHn, acbHn, cueName);

    public void SetPlayerConfigById(int id, CriAtomExPlayerConfigTag config) => this.playerConfigs[id] = config;

    public int Playback_GetTimeSyncedWithAudio(uint playbackId) => throw new NotImplementedException();

    public uint Player_Start(nint playerHn) => this.start.Wrapper(playerHn);

    public void Player_SetStartTime(nint playerHn, int currentBgmTime) => this.setStartTime.Wrapper(playerHn, currentBgmTime);

    public void Player_SetFile(nint playerHn, nint criBinderHn, byte* path) => this.setFile.Wrapper(playerHn, criBinderHn, path);

    public void Player_SetFormat(nint playerHn, CriAtomFormat format) => this.setFormat.Wrapper(playerHn, format);

    public void Player_SetNumChannels(nint playerHn, int numChannels) => this.setNumChannels.Wrapper(playerHn, numChannels);

    public void Player_SetCategoryById(nint playerHn, uint id) => this.setCategoryById.Wrapper(playerHn, id);

    public void Player_SetSamplingRate(nint playerHn, int samplingRate) => this.setSamplingRate.Wrapper(playerHn, samplingRate);

    public uint Player_GetLastPlaybackId(nint playerHn) => this.getLastPlaybackId.Wrapper(playerHn);

    public void Player_SetCategoryByName(nint playerHn, byte* name) => this.setCategoryByName.Wrapper(playerHn, name);

    public bool Player_GetCategoryInfo(nint playerHn, ushort index, CriAtomExCategoryInfoTag* info) => this.getCategoryInfo.Wrapper(playerHn, index, info);

    public int Playback_GetTimeSyncedWithAudioMicro(uint playbackId) => this.getTimeSyncedWithAudioMicro.Wrapper(playbackId);

    public float Category_GetVolumeById(uint id)
    {
        // Use existing CriAtom function.
        if (this.getVolumeById.Wrapper != null)
        {
            return this.getVolumeById.Wrapper(id);
        }

        // Reimplement function if missing (like in P3R).
        return this.Category_GetVolume(this.Config_GetCategoryIndexById(id));
    }

    public void Player_SetVolume(nint playerHn, float volume)
        => this.setVolume.Wrapper(playerHn, volume);

    public void Category_SetVolumeById(uint id, float volume)
    {
        // Use existing CriAtom function.
        if (this.setVolumeById.Wrapper != null)
        {
            this.setVolumeById.Wrapper(id, volume);
            return;
        }

        // Reimplement function if missing (like in STMV).
        var catIndex = this.Config_GetCategoryIndexById(id);
        this.setVolumeByIndex.Wrapper(catIndex, volume);
    }

    public ushort Config_GetCategoryIndexById(uint id) => this.getCategoryIndex.Wrapper(id);

    public float Category_GetVolume(ushort index) => this.getCategoryVolume.Wrapper(index);

    public void Player_SetData(nint playerHn, byte* buffer, int size)
        => this.setData.Wrapper(playerHn, buffer, size);

    public void Player_UpdateAll(nint playerHn)
        => this.updateAll.Wrapper(playerHn);

    public nint Player_Create(CriAtomExPlayerConfigTag* config, void* work, int workSize)
    {
        var playerId = this.players.Count;
        //Log.Verbose($"{nameof(criAtomExPlayer_Create)} || Config: {(nint)config:X} || Work: {(nint)work:X} || WorkSize: {workSize}");

        CriAtomExPlayerConfigTag* currentConfigPtr;
        if (this.playerConfigs.TryGetValue(playerId, out var newConfig))
        {
            currentConfigPtr = (CriAtomExPlayerConfigTag*)Marshal.AllocHGlobal(sizeof(CriAtomExPlayerConfigTag));
            Marshal.StructureToPtr(newConfig, (nint)currentConfigPtr, false);
            Log.Information($"Using custom player config for: {playerId}");
        }
        else
        {
            currentConfigPtr = config;
        }

        var playerHn = this.create.Hook!.OriginalFunction(currentConfigPtr, work, workSize);
        this.players.Add(new(playerId, playerHn));
        CriAtomRegistry.RegisterPlayer(playerHn);

        return playerHn;
    }

    private nint Acb_LoadAcbFile(nint acbBinder, byte* acbPathStr, nint awbBinder, byte* awbPathStr, void* work, int workSize)
    {
        var acbHn = (AcbHn*)this.loadAcbFile.Hook!.OriginalFunction(acbBinder, acbPathStr, awbBinder, awbPathStr, work, workSize);
        //Log.Debug($"{nameof(criAtomExAcb_LoadAcbFile)} || Path: {acbPath} || Hn: {(nint)acbHn:X}");
        CriAtomRegistry.RegisterAcb(acbHn);

        return (nint)acbHn;
    }

    private nint Awb_LoadToc(nint binder, nint path, void* work, int workSize)
    {
        var awbHn = this.loadToc.Hook!.OriginalFunction(binder, path, work, workSize);
        var awbPath = Marshal.PtrToStringAnsi(path)!;
        CriAtomRegistry.RegisterAwb(awbPath, awbHn);
        return awbHn;
    }
}