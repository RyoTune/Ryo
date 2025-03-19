using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
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

    private IFunction<criAtomExPlayer_Create>? create;
    private IFunction<criAtomExPlayer_GetNumPlayedSamples>? getNumPlayedSamples;
    private IFunction<criAtomExAcb_LoadAcbFile>? loadAcbFile;
    private IFunction<criAtomExPlayer_SetFormat>? setFormat;
    private IFunction<criAtomExPlayer_SetSamplingRate>? setSamplingRate;
    private IFunction<criAtomExPlayer_SetNumChannels>? setNumChannels;
    private IFunction<criAtomExPlayer_SetVolume>? setVolume;
    private IFunction<criAtomExPlayer_SetCategoryById>? setCategoryById;
    private IFunction<criAtomExPlayer_GetLastPlaybackId>? getLastPlaybackId;
    private IFunction<criAtomExPlayer_SetCategoryByName>? setCategoryByName;
    private IFunction<criAtomExPlayer_GetCategoryInfo>? getCategoryInfo;
    private IFunction<criAtomExPlayer_UpdateAll>? updateAll;
    private IFunction<criAtomExPlayer_LimitLoopCount>? limitLoopCount;
    private IFunction<criAtomExPlayer_Stop>? stop;
    private IFunction<criAtomExPlayer_SetAisacControlByName>? setAisacControlByName;

    private bool devMode;
    private IHook<criAtomExPlayer_Create>? createHook;
    private IHook<criAtomExAcb_LoadAcbFile>? loadAcbFileHook;
    private IHook<criAtomExAcb_LoadAcbData>? loadAcbDataHook;
    private IHook<criAtomAwb_LoadToc>? loadToc;
    private IHook<criAtomExAcf_GetCategoryInfoByIndex>? acfGetCategoryInfoByIdHook;

    // Private.
    private criAtomExCategory_SetVolumeById? setVolumeById;
    private criAtomExCategory_GetVolume? getCategoryVolume;
    private criAtomExCategory_GetVolumeById? getVolumeById;
    private criAtomExCategory_SetVolume? setVolumeByIndex;

    // Shared.
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

    public CriAtomEx(string game, ISharedScans scans, IReloadedHooks hooks)
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

        ScanHooks.Add(
            nameof(criAtomExAcf_GetCategoryInfoByIndex),
            this.patterns.criAtomExAcf_GetCategoryInfoByIndex,
            (hooks, result) =>
            {
                this.acfGetCategoryInfoByIdHook = hooks.CreateHook<criAtomExAcf_GetCategoryInfoByIndex>(this.Acf_GetCategoryInfoById, 0x140643f6c).Activate();
            });

        ScanHooks.Add(
            nameof(criAtomExCategory_GetVolume),
            this.patterns.criAtomExCategory_GetVolume,
            (hooks, result) => this.getCategoryVolume = hooks.CreateWrapper<criAtomExCategory_GetVolume>(result, out _));

        ScanHooks.Add(
            nameof(criAtomExCategory_SetVolumeById),
            this.patterns.criAtomExCategory_SetVolumeById,
            (hooks, result) => this.setVolumeById = hooks.CreateWrapper<criAtomExCategory_SetVolumeById>(result, out _));

        ScanHooks.Add(
            nameof(criAtomExCategory_SetVolume),
            this.patterns.criAtomExCategory_SetVolume,
            (hooks, result) => this.setVolumeByIndex = hooks.CreateWrapper<criAtomExCategory_SetVolume>(result, out _));

        scans.AddScan<criAtomExPlayer_Create>(this.patterns.criAtomExPlayer_Create);
        scans.CreateListener<criAtomExPlayer_Create>(result =>
        {
            this.create = hooks.CreateFunction<criAtomExPlayer_Create>(result);
            this.createHook = this.create.Hook(this.Player_Create).Activate();
        });

        scans.AddScan<criAtomExAcb_LoadAcbFile>(this.patterns.criAtomExAcb_LoadAcbFile);
        scans.CreateListener<criAtomExAcb_LoadAcbFile>(result =>
        {
            this.loadAcbFile = hooks.CreateFunction<criAtomExAcb_LoadAcbFile>(result);
            this.loadAcbFileHook = this.loadAcbFile.Hook(this.Acb_LoadAcbFile).Activate();
        });

        scans.AddScan<criAtomAwb_LoadToc>(this.patterns.criAtomAwb_LoadToc);
        scans.CreateListener<criAtomAwb_LoadToc>(result =>
        {
            this.loadToc = hooks.CreateHook<criAtomAwb_LoadToc>(this.Awb_LoadToc, result).Activate();
        });

        scans.AddScan<criAtomExAcb_LoadAcbData>(this.patterns.criAtomExAcb_LoadAcbData);
        scans.CreateListener<criAtomExAcb_LoadAcbData>(result =>
        {
            this.loadAcbDataHook = hooks.CreateHook<criAtomExAcb_LoadAcbData>(this.Acb_LoadAcbData, result).Activate();
        });

        ScanHooks.Add(
            nameof(criAtomExPlayer_GetNumPlayedSamples),
            this.patterns.criAtomExPlayer_GetNumPlayedSamples,
            (hooks, result) => this.getNumPlayedSamples = hooks.CreateFunction<criAtomExPlayer_GetNumPlayedSamples>(result));

        ScanHooks.Add(
            nameof(criAtomExPlayer_SetFormat),
            this.patterns.criAtomExPlayer_SetFormat,
            (hooks, result) => this.setFormat = hooks.CreateFunction<criAtomExPlayer_SetFormat>(result));

        ScanHooks.Add(
            nameof(criAtomExPlayer_SetSamplingRate),
            this.patterns.criAtomExPlayer_SetSamplingRate,
            (hooks, result) => this.setSamplingRate = hooks.CreateFunction<criAtomExPlayer_SetSamplingRate>(result));

        ScanHooks.Add(
            nameof(criAtomExPlayer_SetNumChannels),
            this.patterns.criAtomExPlayer_SetNumChannels,
            (hooks, result) => this.setNumChannels = hooks.CreateFunction<criAtomExPlayer_SetNumChannels>(result));

        ScanHooks.Add(
            nameof(criAtomExCategory_GetVolumeById),
            this.patterns.criAtomExCategory_GetVolumeById,
            (hooks, result) => this.getVolumeById = hooks.CreateWrapper<criAtomExCategory_GetVolumeById>(result, out _));

        ScanHooks.Add(
            nameof(criAtomExPlayer_SetVolume),
            this.patterns.criAtomExPlayer_SetVolume,
            (hooks, result) => this.setVolume = hooks.CreateFunction<criAtomExPlayer_SetVolume>(result));

        ScanHooks.Add(
            nameof(criAtomExPlayer_SetCategoryById),
            this.patterns.criAtomExPlayer_SetCategoryById,
            (hooks, result) => this.setCategoryById = hooks.CreateFunction<criAtomExPlayer_SetCategoryById>(result));

        ScanHooks.Add(
            nameof(criAtomExPlayer_GetLastPlaybackId),
            this.patterns.criAtomExPlayer_GetLastPlaybackId,
            (hooks, result) => this.getLastPlaybackId = hooks.CreateFunction<criAtomExPlayer_GetLastPlaybackId>(result));

        ScanHooks.Add(
            nameof(criAtomExPlayer_SetCategoryByName),
            this.patterns.criAtomExPlayer_SetCategoryByName,
            (hooks, result) => this.setCategoryByName = hooks.CreateFunction<criAtomExPlayer_SetCategoryByName>(result));

        ScanHooks.Add(
            nameof(criAtomExPlayer_GetCategoryInfo),
            this.patterns.criAtomExPlayer_GetCategoryInfo,
            (hooks, result) => this.getCategoryInfo = hooks.CreateFunction<criAtomExPlayer_GetCategoryInfo>(result));

        ScanHooks.Add(
            nameof(criAtomExPlayer_UpdateAll),
            this.patterns.criAtomExPlayer_UpdateAll,
            (hooks, result) => this.updateAll = hooks.CreateFunction<criAtomExPlayer_UpdateAll>(result));

        ScanHooks.Add(
            nameof(criAtomExPlayer_LimitLoopCount),
            this.patterns.criAtomExPlayer_LimitLoopCount,
            (hooks, result) => this.limitLoopCount = hooks.CreateFunction<criAtomExPlayer_LimitLoopCount>(result));

        ScanHooks.Add(
            nameof(criAtomExPlayer_Stop),
            this.patterns.criAtomExPlayer_Stop,
            (hooks, result) => this.stop = hooks.CreateFunction<criAtomExPlayer_Stop>(result));

        ScanHooks.Add(
            nameof(criAtomExPlayer_SetAisacControlByName),
            this.patterns.criAtomExPlayer_SetAisacControlByName,
            (hooks, result) => this.setAisacControlByName = hooks.CreateFunction<criAtomExPlayer_SetAisacControlByName>(result));
    }

    public void SetDevMode(bool devMode) => this.devMode = devMode;

    public void Player_ResetParameters(nint playerHn) => this.resetParameters.Wrapper(playerHn);

    public bool Acb_GetCueInfoById(nint acbHn, int id, CriAtomExCueInfoTag* info) => this.getCueInfoById.Wrapper(acbHn, id, info);

    public bool Acb_GetCueInfoByName(nint acbHn, nint name, CriAtomExCueInfoTag* info) => this.getCueInfoByName.Wrapper(acbHn, name, info);

    public bool Acf_GetCategoryInfoById(ushort id, CriAtomExCategoryInfoTag* info)
    {
        var result = this.acfGetCategoryInfoByIdHook!.OriginalFunction(id, info);
        return result;
    }

    public nint Acb_LoadAcbData(nint acbData, int acbDataSize, nint awbBinder, nint awbPath, void* work, int workSize)
    {
        var acbHn = (AcbHn*)this.loadAcbDataHook!.OriginalFunction(acbData, acbDataSize, awbBinder, awbPath, work, workSize);
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

    public void Player_LimitLoopCount(nint playerHn, int count) => this.limitLoopCount!.GetWrapper()(playerHn, count);

    public void Player_Stop(nint playerHn) => this.stop!.GetWrapper()(playerHn);

    public void Player_SetAisacControlByName(nint playerHn, byte* controlName, float controlValue)  => this.setAisacControlByName!.GetWrapper()(playerHn, controlName, controlValue);

    public void Player_SetCueId(nint playerHn, nint acbHn, int cueId)  => this.setCueId.Wrapper(playerHn, acbHn, cueId);

    public void Player_SetCueName(nint playerHn, nint acbHn, byte* cueName)  => this.setCueName.Wrapper(playerHn, acbHn, cueName);

    public void SetPlayerConfigById(int id, CriAtomExPlayerConfigTag config) => this.playerConfigs[id] = config;

    public int Playback_GetTimeSyncedWithAudio(uint playbackId) => throw new NotImplementedException();

    public uint Player_Start(nint playerHn) => this.start.Wrapper(playerHn);

    public void Player_SetStartTime(nint playerHn, int currentBgmTime) => this.setStartTime.Wrapper(playerHn, currentBgmTime);

    public void Player_SetFile(nint playerHn, nint criBinderHn, byte* path) => this.setFile.Wrapper(playerHn, criBinderHn, path);

    public void Player_SetFormat(nint playerHn, CriAtomFormat format) => this.setFormat!.GetWrapper()(playerHn, format);

    public void Player_SetNumChannels(nint playerHn, int numChannels) => this.setNumChannels!.GetWrapper()(playerHn, numChannels);

    public void Player_SetCategoryById(nint playerHn, uint id) => this.setCategoryById!.GetWrapper()(playerHn, id);

    public void Player_SetSamplingRate(nint playerHn, int samplingRate) => this.setSamplingRate!.GetWrapper()(playerHn, samplingRate);

    public uint Player_GetLastPlaybackId(nint playerHn) => this.getLastPlaybackId!.GetWrapper()(playerHn);

    public void Player_SetCategoryByName(nint playerHn, byte* name) => this.setCategoryByName!.GetWrapper()(playerHn, name);

    public bool Player_GetCategoryInfo(nint playerHn, ushort index, CriAtomExCategoryInfoTag* info) => this.getCategoryInfo!.GetWrapper()(playerHn, index, info);

    public int Playback_GetTimeSyncedWithAudioMicro(uint playbackId) => this.getTimeSyncedWithAudioMicro.Wrapper(playbackId);

    public float Category_GetVolumeById(uint id)
    {
        // Use existing CriAtom function.
        if (this.getVolumeById != null)
        {
            return this.getVolumeById(id);
        }

        // Reimplement function if missing (like in P3R).
        return this.Category_GetVolume(this.Config_GetCategoryIndexById(id));
    }

    public void Player_SetVolume(nint playerHn, float volume)
        => this.setVolume!.GetWrapper()(playerHn, volume);

    public void Category_SetVolumeById(uint id, float volume)
    {
        // Use existing CriAtom function.
        if (this.setVolumeById != null)
        {
            this.setVolumeById(id, volume);
            return;
        }

        // Reimplement function if missing (like in STMV).
        var catIndex = this.Config_GetCategoryIndexById(id);
        this.setVolumeByIndex!(catIndex, volume);
    }

    public ushort Config_GetCategoryIndexById(uint id) => this.getCategoryIndex.Wrapper(id);

    public float Category_GetVolume(ushort index) => this.getCategoryVolume!(index);

    public void Player_SetData(nint playerHn, byte* buffer, int size)
        => this.setData.Wrapper(playerHn, buffer, size);

    public void Player_UpdateAll(nint playerHn)
        => this.updateAll!.GetWrapper()(playerHn);

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

        var playerHn = this.createHook!.OriginalFunction(currentConfigPtr, work, workSize);
        this.players.Add(new(playerId, playerHn));
        CriAtomRegistry.RegisterPlayer(playerHn);

        return playerHn;
    }

    private nint Acb_LoadAcbFile(nint acbBinder, byte* acbPathStr, nint awbBinder, byte* awbPathStr, void* work, int workSize)
    {
        var acbHn = (AcbHn*)this.loadAcbFileHook!.OriginalFunction(acbBinder, acbPathStr, awbBinder, awbPathStr, work, workSize);
        //Log.Debug($"{nameof(criAtomExAcb_LoadAcbFile)} || Path: {acbPath} || Hn: {(nint)acbHn:X}");
        CriAtomRegistry.RegisterAcb(acbHn);

        return (nint)acbHn;
    }

    private nint Awb_LoadToc(nint binder, nint path, void* work, int workSize)
    {
        var awbHn = this.loadToc!.OriginalFunction(binder, path, work, workSize);
        var awbPath = Marshal.PtrToStringAnsi(path)!;
        CriAtomRegistry.RegisterAwb(awbPath, awbHn);
        return awbHn;
    }
}