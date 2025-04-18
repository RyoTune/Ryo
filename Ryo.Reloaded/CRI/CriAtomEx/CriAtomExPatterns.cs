// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
namespace Ryo.Reloaded.CRI.CriAtomEx;

internal class CriAtomExPatterns
{
    public string? criAtomExPlayer_GetNumPlayedSamples { get; init; }
    public string? criAtomExAcb_LoadAcbFile { get; init; }
    public string? criAtomExPlayer_SetCueId { get; init; }
    public string? criAtomExPlayer_Start { get; init; }
    public string? criAtomExPlayer_SetFile { get; init; }
    public string? criAtomExPlayer_SetFormat { get; init; }
    public string? criAtomExPlayer_SetSamplingRate { get; init; }
    public string? criAtomExPlayer_SetNumChannels { get; init; }
    public string? criAtomExCategory_GetVolumeById { get; init; }
    public string? criAtomExPlayer_SetVolume { get; init; }
    public string? criAtomExCategory_SetVolumeById { get; init; }
    public string? criAtomExPlayer_SetCategoryById { get; init; }
    public string? criAtomExPlayer_SetStartTime { get; init; }
    public string? criAtomExPlayback_GetTimeSyncedWithAudio { get; init; }
    public string? criAtomExPlayer_Create { get; init; }
    public string? criAtomExPlayer_GetLastPlaybackId { get; init; }
    public string? criAtomExPlayer_SetCategoryByName { get; init; }
    public string? criAtomExPlayer_GetCategoryInfo { get; init; }
    public string? criAtomExPlayer_SetData { get; init; }
    public string? criAtomExPlayer_SetCueName { get; init; }
    public string? criAtomExPlayer_UpdateAll { get; init; }
    public string? criAtomExPlayer_LimitLoopCount { get; init; }
    public string? criAtomExPlayer_GetStatus { get; init; }
    public string? criAtomExPlayer_Stop { get; init; }
    public string? criAtomExPlayer_SetAisacControlByName { get; init; }
    public string? criAtomExCategory_GetVolume { get; init; }
    public string? criAtomConfig_GetCategoryIndexById { get; init; }
    public string? criAtomExCategory_SetVolume { get; init; }
    public string? criAtomExAcb_ExistsId { get; init; }
    public string? criAtomExAcb_LoadAcbData { get; init; }
    public string? criAtomExPlayer_SetWaveId { get; init; }
    public string? criAtomExAcb_GetCueInfoByName { get; init; }
    public string? criAtomExAcb_ExistsName { get; init; }
    public string? criAtomExAcb_GetCueInfoById { get; init; }
    public string? criAtomAwb_LoadToc { get; init; }
    public string? criAtomExAcf_GetCategoryInfoByIndex { get; init; }
    public string? criAtomExPlayer_SetSyncPlaybackId { get; init; }
    public string? criAtomExPlayback_GetTimeSyncedWithAudioMicro { get; init; }
    public string? criAtomExPlayer_ResetParameters { get; init; }
    
    public static CriAtomExPatterns GetGamePatterns(string game)
        => game.ToLowerInvariant() switch
        {
            "p3r" => new()
            {
                criAtomExPlayer_Create = "48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 55 41 54 41 55 41 56 41 57 48 8B EC 48 83 EC 40 45 33 ED 41 8B F8",
                criAtomExPlayer_Start = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 44 8D 41 ?? 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 83 C8 FF",
                criAtomExPlayer_SetData = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 44 89 C6",
                criAtomExPlayer_SetFormat = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 48 8D 15",
                criAtomExPlayer_SetNumChannels = "48 89 5C 24 ?? 57 48 83 EC 20 89 D7 48 89 CB 48 85 C9 74 ?? 8D 42",
                criAtomExPlayer_SetSamplingRate = "48 89 5C 24 ?? 57 48 83 EC 20 89 D7 48 89 CB 48 85 C9 74 ?? 85 D2",
                criAtomExPlayer_SetCategoryById = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 50 48 8B F9 8B F2",
                criAtomExPlayer_SetCueName = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F8 48 8B EA 48 8B D9",
                criAtomExPlayer_SetVolume = "48 85 C9 75 ?? 44 8D 41 ?? 48 8D 15 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8B 89 ?? ?? ?? ?? 33 D2",
                criAtomExCategory_SetVolumeById = "40 53 48 83 EC 30 8B D9 0F 29 74 24",
                criAtomExPlayer_UpdateAll = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 44 8D 41 ?? 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? EB",
                criAtomExPlayer_LimitLoopCount = "44 8B C2 48 85 C9 74 ?? 83 FA FD",
                criAtomExPlayer_GetStatus = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 44 8D 41 ?? 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 8D 47",
                criAtomExPlayer_Stop = "40 53 48 83 EC 20 48 8B D9 48 85 C9 75 ?? 44 8D 41 ?? 48 8D 15 ?? ?? ?? ?? 48 83 C4 20 5B E9 ?? ?? ?? ?? E8 ?? ?? ?? ?? 85 C0 74 ?? 83 F8 03 75 ?? 48 8B 8B ?? ?? ?? ?? E8 ?? ?? ?? ?? 83 63 ?? 00 83 A3 ?? ?? ?? ?? 00 C6 83 ?? ?? ?? ?? 00 48 83 C4 20 5B C3 E8 ?? ?? ?? ?? 48 8B CB",
                criAtomExPlayer_SetAisacControlByName = "48 89 5C 24 ?? 57 48 83 EC 30 48 8B F9 0F 29 74 24 ?? 33 C9",
                criAtomExCategory_GetVolume = "40 53 48 83 EC 20 83 64 24 ?? 00",
                criAtomConfig_GetCategoryIndexById = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B F9 BE FF FF 00 00",

                criAtomExAcb_LoadAcbData = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 50 33 F6",

                criAtomExAcb_GetCueInfoByName = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F0 48 8B FA 48 8B D9 48 85 D2 75",
                criAtomExAcb_GetCueInfoById = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F8 8B F2 48 8B D9 4D 85 C0",
            },
            "metaphor" => new()
            {
                criAtomExPlayer_Create = "48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 55 41 54 41 55 41 56 41 57 48 8B EC 48 83 EC 40 45 33 E4",
                criAtomExPlayer_Start = "48 89 5C 24 ?? 57 48 83 EC 20 48 89 CF 48 85 C9 75 ?? 44 8D 41",
                criAtomExPlayer_SetFormat = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 48 8D 15",
                criAtomExPlayer_SetNumChannels = "48 89 5C 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 85 C9 74 ?? 8D 42",
                criAtomExPlayer_SetSamplingRate = "48 89 5C 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 85 C9 74 ?? 85 D2",
                criAtomExPlayer_SetCategoryById = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 50 48 8B F9 8B F2",

                criAtomExPlayer_SetVolume = "40 53 48 83 EC 30 0F 29 74 24 ?? 0F 28 F1 48 8B D9 48 85 C9 75 ?? 48 8D 15 ?? ?? ?? ?? 41 B8 FE FF FF FF 33 C9 E8 ?? ?? ?? ?? EB ?? 0F 57 C0 F3 0F 5A C6 E8 ?? ?? ?? ?? 85 C0 74 ?? 48 8D 15 ?? ?? ?? ?? EB ?? F3 0F 11 74 24 ?? 33 D2",
                criAtomExPlayer_ResetParameters = "40 53 48 83 EC 20 48 8B D9 48 85 C9 75 ?? 44 8D 41 ?? 48 8D 15 ?? ?? ?? ?? 48 83 C4 20 5B E9 ?? ?? ?? ?? 48 8B 89 ?? ?? ?? ?? E8 ?? ?? ?? ?? 83 A3 ?? ?? ?? ?? 00",

                criAtomExPlayer_SetFile = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F0 48 8B EA 48 8B F9 48 85 C9", // ???
                criAtomExPlayer_SetCueId = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 63 F8 48 89 D6",
                criAtomExPlayer_SetWaveId = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 41 8B F0 48 8B EA 48 8B F9 48 85 C9 74 ?? 48 85 D2 74 ?? 41 81 F8 FF FF 00 00 77 ?? E8 ?? ?? ?? ?? 48 8B CF 8B D8 E8 ?? ?? ?? ?? C7 87 ?? ?? ?? ?? 07 00 00 00",

                criAtomExAcb_LoadAcbData = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 50 33 F6",
                criAtomAwb_LoadToc = "40 53 48 83 EC 20 83 25 ?? ?? ?? ?? 00",

                criAtomExCategory_GetVolumeById = "40 53 48 83 EC 20 8B D9 33 C9 E8 ?? ?? ?? ?? 85 C0 75 ?? 48 8D 15 ?? ?? ?? ?? 33 C9 E8 ?? ?? ?? ?? F3 0F 10 05",
                criAtomExCategory_SetVolumeById = "40 53 48 83 EC 30 8B D9 0F 29 74 24 ?? 33 C9",

                criAtomExAcb_GetCueInfoById = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 4C 89 C7 89 D6",
            },
            "Jujutsu Kaisen CC" => new()
            {
                criAtomExPlayer_Create = "48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 55 41 54 41 55 41 56 41 57 48 8B EC 48 83 EC 40 45 33 E4",
                //criAtomExPlayer_Start = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 44 8D 41 ?? 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 83 C8 FF",
                criAtomExPlayer_SetData = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 41 8B F0 48 8B EA",
                criAtomExPlayer_SetFormat = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 48 8D 15",
                criAtomExPlayer_SetNumChannels = "48 89 5C 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 85 C9 74 ?? 8D 42",
                criAtomExPlayer_SetSamplingRate = "48 89 5C 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 85 C9 74 ?? 85 D2",

                criAtomExPlayer_SetCategoryById = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 50 48 8B F9 8B F2",
                criAtomExPlayer_SetCueName = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F8 48 8B EA 48 8B D9 48 85 C9",

                criAtomExCategory_GetVolume = "40 53 48 83 EC 20 83 64 24 ?? 00",
                criAtomExCategory_SetVolume = "40 53 48 83 EC 30 0F B7 D9",
                criAtomConfig_GetCategoryIndexById = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B F9 BE FF FF 00 00",

                criAtomExAcb_LoadAcbData = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 50 33 F6",

                criAtomExAcb_GetCueInfoByName = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F0 48 8B FA 48 8B D9 48 85 D2 75",
                //criAtomExAcb_GetCueInfoById = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F8 8B F2 48 8B D9 4D 85 C0",
            },
            "SMT5V-Win64-Shipping" => new()
            {
                criAtomExPlayer_Create = "48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 55 41 54 41 55 41 56 41 57 48 8B EC 48 83 EC 40 45 33 ED 41 8B F8",
                criAtomExPlayer_Start = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 44 8D 41 ?? 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 83 C8 FF",
                criAtomExPlayer_SetData = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 44 89 C6",
                criAtomExPlayer_SetFormat = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 48 8D 15",
                criAtomExPlayer_SetNumChannels = "48 89 5C 24 ?? 57 48 83 EC 20 89 D7 48 89 CB 48 85 C9 74 ?? 8D 42",
                criAtomExPlayer_SetSamplingRate = "48 89 5C 24 ?? 57 48 83 EC 20 89 D7 48 89 CB 48 85 C9 74 ?? 85 D2",
                criAtomExPlayer_SetCueName = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F8 48 8B EA 48 8B D9 48 85 C9",
                criAtomExPlayer_SetCategoryById = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 50 48 8B F9 8B F2",

                criAtomExAcb_LoadAcbData = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 50 33 F6",

                criAtomExCategory_GetVolumeById = "40 53 48 83 EC 20 8B D9 33 C9 E8 ?? ?? ?? ?? 85 C0 75 ?? 48 8D 15 ?? ?? ?? ?? 33 C9 E8 ?? ?? ?? ?? F3 0F 10 05",
                criAtomExCategory_GetVolume = "40 53 48 83 EC 20 83 64 24 ?? 00",
                criAtomConfig_GetCategoryIndexById = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B F9 BE FF FF 00 00",
                criAtomExCategory_SetVolume = "40 53 48 83 EC 30 0F B7 D9",

                criAtomExAcb_GetCueInfoByName = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F0 48 8B FA 48 8B D9 48 85 D2 75 ??",
                criAtomExAcb_GetCueInfoById = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F8 8B F2 48 8B D9 4D 85 C0",
            },
            "p5r" => new()
            {
                criAtomExPlayer_Create = "48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 55 41 54 41 55 41 56 41 57 48 8B EC 48 83 EC 40 45 33 ED",
                criAtomExPlayer_Start = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 44 8D 41 ?? 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 83 C8 FF EB ?? E8 ?? ?? ?? ?? 33 D2",
                criAtomExPlayer_SetFormat = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 48 8D 15",
                criAtomExPlayer_SetNumChannels = "48 89 5C 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 85 C9 74 ?? 8D 42",
                criAtomExPlayer_SetSamplingRate = "48 89 5C 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 85 C9 74 ?? 85 D2",
                criAtomExPlayer_SetCategoryById = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 50 48 8B F9 8B F2",
                criAtomExPlayer_SetVolume = "48 85 C9 75 ?? 44 8D 41 ?? 48 8D 15 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8B 89 ?? ?? ?? ?? 33 D2",
                criAtomExPlayer_ResetParameters = "40 53 48 83 EC 20 48 8B D9 48 85 C9 75 ?? 44 8D 41 ?? 48 8D 15 ?? ?? ?? ?? 48 83 C4 20 5B E9 ?? ?? ?? ?? 48 8B 89 ?? ?? ?? ?? E8 ?? ?? ?? ?? 83 A3 ?? ?? ?? ?? 00",

                criAtomExPlayer_SetFile = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F0 48 8B EA 48 8B F9",
                criAtomExPlayer_SetCueId = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 63 F8 48 8B F2 48 8B D9",
                criAtomExPlayer_SetWaveId = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 41 8B F0 48 8B EA 48 8B F9 48 85 C9 74 ?? 48 85 D2 74 ?? 41 81 F8 FF FF 00 00 77 ?? E8 ?? ?? ?? ?? 48 8B CF 8B D8 E8 ?? ?? ?? ?? C7 87 ?? ?? ?? ?? 07 00 00 00",

                //criAtomExAcb_LoadAcbFile = "48 8B C4 48 89 58 ?? 48 89 68 ?? 48 89 70 ?? 48 89 78 ?? 41 54 41 56 41 57 48 83 EC 40 49 8B E9",
                criAtomExAcb_LoadAcbData = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 50 33 F6 4C 63 F2",
                criAtomAwb_LoadToc = "40 53 48 83 EC 20 83 25 ?? ?? ?? ?? 00",

                criAtomExCategory_GetVolumeById = "40 53 48 83 EC 20 8B D9 33 C9 E8 ?? ?? ?? ?? 85 C0 75 ?? 48 8D 15 ?? ?? ?? ?? 33 C9 E8 ?? ?? ?? ?? F3 0F 10 05",
                criAtomExCategory_SetVolumeById = "40 53 48 83 EC 30 8B D9 0F 29 74 24 ?? 33 C9",

                criAtomExAcb_GetCueInfoById = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F8 8B F2 48 8B D9 4D 85 C0 75 ?? 48 8D 15 ?? ?? ?? ?? 41 B8 FE FF FF FF 33 C9 E8 ?? ?? ?? ?? 33 C0",
            },
            "p4g" => new()
            {
                criAtomExPlayer_Create = "40 55 53 56 57 41 54 41 55 41 56 41 57 48 8D 6C 24 ?? 48 81 EC C8 00 00 00 45 8B F8",
                criAtomExPlayer_Start = "48 89 E0 48 89 58 ?? 48 89 68 ?? 48 89 70 ?? 48 89 78 ?? 41 56 48 83 EC 60",
                criAtomExPlayer_SetFormat = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 48 8D 15",
                criAtomExPlayer_SetNumChannels = "48 89 5C 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 85 C9 74 ?? 8D 42",
                criAtomExPlayer_SetSamplingRate = "48 89 5C 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 85 C9 74 ?? 85 D2",
                criAtomExPlayer_SetCategoryById = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 50 48 8B F9 8B F2",
                criAtomExPlayer_SetCategoryByName = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 50 48 8B F9 48 8B F2",
                criAtomExPlayer_SetVolume = "48 85 C9 75 ?? 44 8D 41 ?? 48 8D 15 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8B 89 ?? ?? ?? ?? 0F 28 D1 33 D2",
                criAtomExPlayer_ResetParameters = "40 53 48 83 EC 20 48 8B D9 48 85 C9 75 ?? 44 8D 41 ?? 48 8D 15 ?? ?? ?? ?? 48 83 C4 20 5B E9 ?? ?? ?? ?? 48 8B 89 ?? ?? ?? ?? E8 ?? ?? ?? ?? 83 A3 ?? ?? ?? ?? 00",

                criAtomExPlayer_SetCueId = "48 8B C4 48 89 58 ?? 48 89 68 ?? 48 89 70 ?? 48 89 78 ?? 41 54 41 56 41 57 48 81 EC 80 00 00 00 4D 63 F0",
                criAtomExPlayer_SetFile = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F0 48 8B EA 48 8B F9", // Uses same pattern as P5R but function is "simpler".
                criAtomExPlayer_SetData = "48 8B C4 48 89 58 ?? 48 89 68 ?? 48 89 70 ?? 48 89 78 ?? 41 54 41 56 41 57 48 81 EC 80 00 00 00 45 8B F0",
                criAtomExPlayer_SetWaveId = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 56 41 57 48 81 EC 80 00 00 00 45 8B F0 4C 8B FA 48 8B E9 E8 ?? ?? ?? ?? 48 8B F0 E8 ?? ?? ?? ?? B9 4D 00 00 00",

                // Pattern to actual function that handles loading ACB data, used by both loading functions (ID/non-ID).
                // FUN_14061d260
                criAtomExAcb_LoadAcbData = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 50 33 F6",
                criAtomAwb_LoadToc = "40 53 48 83 EC 20 83 25 ?? ?? ?? ?? 00",

                criAtomExCategory_GetVolumeById = "40 53 48 83 EC 20 8B D9 33 C9 E8 ?? ?? ?? ?? 85 C0 75 ?? 48 8D 15 ?? ?? ?? ?? 33 C9 E8 ?? ?? ?? ?? F3 0F 10 05",
                criAtomExCategory_SetVolumeById = "40 53 48 83 EC 30 8B D9 0F 29 74 24 ?? 33 C9",

                criAtomExAcb_GetCueInfoById = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F8 8B F2 48 8B D9 4D 85 C0 75 ?? 48 8D 15 ?? ?? ?? ?? 41 B8 FE FF FF FF 33 C9 E8 ?? ?? ?? ?? 33 C0",
                
                //criAtomExAcf_GetCategoryInfoByIndex = "48 89 5C 24 ?? 48 89 74 24 ?? 55 57 41 55 41 56 41 57 48 8D 6C 24",
                criAtomConfig_GetCategoryIndexById = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B F9 BE FF FF 00 00",

                criAtomExPlayer_SetSyncPlaybackId = "48 89 5C 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 85 C9 75 ?? 44 8D 41 ?? 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? EB ?? 8B CF",
                criAtomExPlayer_SetStartTime = "48 85 C9 74 ?? 48 85 D2 78 ?? B8 FF FF FF FF 48 3B D0 48 0F 4C C2",
                criAtomExPlayer_GetStatus = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 44 8D 41 ?? 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 8D 47",
                criAtomExPlayback_GetTimeSyncedWithAudioMicro = "48 89 5C 24 ?? 48 89 6C 24 ?? 56 57 41 56 48 83 EC 20 44 8B F1",
            },
            "p3p" => new()
            {
                criAtomExPlayer_Create = "48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 55 41 54 41 55 41 56 41 57 48 8B EC 48 83 EC 40 45 33 E4",
                criAtomExPlayer_Start = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 44 8D 41 ?? 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 83 C8 FF EB ?? E8 ?? ?? ?? ?? 33 D2",
                criAtomExPlayer_SetFormat = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 48 8D 15",
                criAtomExPlayer_SetNumChannels = "48 89 5C 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 85 C9 74 ?? 8D 42",
                criAtomExPlayer_SetSamplingRate = "48 89 5C 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 85 C9 74 ?? 85 D2",

                // P3P contains both SetFile and SetCueName which match each other's patterns.
                // The last 4 bytes reference the error message "E2010021535", it might break if there's an update.
                criAtomExPlayer_SetFile = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F8 48 8B EA 48 8B D9 48 85 C9 74 ?? 4D 85 C0 74 ?? E8 ?? ?? ?? ?? 4C 8B C7 48 8B D5 48 8B CB 8B F0 E8 ?? ?? ?? ?? 85 C0 75 ?? 48 8B CB E8 ?? ?? ?? ?? 85 F6 74 ?? E8 ?? ?? ?? ?? EB ?? 41 B8 FE FF FF FF 48 8D 15 50 d2 23 00",
                criAtomExPlayer_SetCueName = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F8 48 8B EA 48 8B D9 48 85 C9 74 ?? 4D 85 C0 74 ?? E8 ?? ?? ?? ?? 4C 8B C7 48 8B D5 48 8B CB 8B F0 E8 ?? ?? ?? ?? 85 C0 75 ?? 48 8B CB E8 ?? ?? ?? ?? 85 F6 74 ?? E8 ?? ?? ?? ?? EB ?? 41 B8 FE FF FF FF 48 8D 15",
                criAtomExPlayer_SetWaveId = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 41 8B F0 48 8B EA 48 8B F9 48 85 C9 74 ?? 48 85 D2 74 ?? 41 81 F8 FF FF 00 00 77 ?? E8 ?? ?? ?? ?? 48 8B CF 8B D8 E8 ?? ?? ?? ?? C7 87 ?? ?? ?? ?? 07 00 00 00",
                criAtomExPlayer_SetData = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 41 8B F0 48 8B EA 48 8B F9 48 85 C9 74 ?? 48 85 D2 74 ?? 45 85 C0",

                criAtomExPlayer_SetCategoryById = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 50 48 8B F9 8B F2",

                // Function used by LoadAcbData and LoadAcbDataById.
                criAtomExAcb_LoadAcbData = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 50 33 F6",

                criAtomExCategory_GetVolumeById = "40 53 48 83 EC 20 8B D9 33 C9 E8 ?? ?? ?? ?? 85 C0 75 ?? 48 8D 15 ?? ?? ?? ?? 33 C9 E8 ?? ?? ?? ?? F3 0F 10 05",
                criAtomExCategory_SetVolumeById = "40 53 48 83 EC 30 8B D9 0F 29 74 24 ?? 33 C9",
            },
            "likeadragon8" => new()
            {
                criAtomExPlayer_Create = "48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 55 41 54 41 55 41 56 41 57 48 8B EC 48 83 EC 40 45 33 E4",
                criAtomExPlayer_Start = "48 89 5C 24 ?? 57 48 83 EC 20 48 89 CF 48 85 C9 75 ?? 8B 15 ?? ?? ?? ??", // denuvo moment
                criAtomExPlayer_SetFormat = "48 89 5C 24 ?? 57 48 83 EC 20 48 89 CF 48 85 C9 75 ?? 48 8D 15 ?? ?? ?? ??",
                criAtomExPlayer_SetNumChannels = "48 89 5C 24 ?? 57 48 83 EC 20 89 D7 48 89 CB 48 85 C9 74 ?? 8D 42 ??",
                criAtomExPlayer_SetSamplingRate = "48 89 5C 24 ?? 57 48 83 EC 20 89 D7 48 89 CB 48 85 C9 74 ?? 85 D2",
                criAtomExPlayer_SetFile = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F0 48 8B EA 48 8B F9 48 85 C9",
                criAtomExPlayer_SetCueId = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 63 F8",
                criAtomExPlayer_SetCategoryById = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 50 48 89 CF",
                criAtomExAcb_LoadAcbFile = "48 8B C4 48 89 58 ?? 48 89 68 ?? 48 89 70 ?? 48 89 78 ?? 41 54 41 56 41 57 48 83 EC 40 49 8B E9",
                criAtomExAcb_LoadAcbData = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 50 33 F6",
                criAtomExPlayer_SetCueName = "48 8B C4 48 89 58 ?? 48 89 68 ?? 48 89 70 ?? 57 48 83 EC 20 33 ED",

                criAtomExCategory_GetVolumeById = "40 53 48 83 EC 20 83 64 24 ?? 00 0F B7 D9",
                criAtomExCategory_SetVolumeById = "40 53 48 83 EC 30 8B D9 0F 29 74 24 ?? 33 C9",

                criAtomExAcb_GetCueInfoById = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 4C 89 C7 89 D6",
                criAtomExPlayer_SetData = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 41 8B F0 48 8B EA 48 8B F9 48 85 C9 74 ?? 48 85 D2 74 ?? 45 85 C0",
            },
            "likeadragonpirates" => new()
            {
                criAtomExPlayer_Create = "48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 55 41 54 41 55 41 56 41 57 48 8B EC 48 83 EC 40 45 33 E4",
                criAtomExPlayer_Start = "48 89 5C 24 ?? 57 48 83 EC 20 48 89 CF 48 85 C9 75 ?? 44 8D 41 ?? 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 83 C8 FF EB ?? E8 ?? ?? ?? ?? 31 D2",
                criAtomExPlayer_SetFormat = "48 89 5C 24 ?? 57 48 83 EC 20 48 89 CF 48 85 C9 75 ?? 48 8D 15 ?? ?? ?? ??",
                criAtomExPlayer_SetNumChannels = "48 89 5C 24 ?? 57 48 83 EC 20 89 D7 48 89 CB 48 85 C9 74 ?? 8D 42 ??",
                criAtomExPlayer_SetSamplingRate = "48 89 5C 24 ?? 57 48 83 EC 20 89 D7 48 89 CB 48 85 C9 74 ?? 85 D2",
                criAtomExPlayer_SetFile = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F0 48 8B EA 48 8B F9 48 85 C9",
                criAtomExPlayer_SetCueId = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 63 F8",
                criAtomExPlayer_SetCategoryById = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 50 48 89 CB 89 D7",
                criAtomExAcb_LoadAcbFile = "48 8B C4 48 89 58 ?? 48 89 68 ?? 48 89 70 ?? 48 89 78 ?? 41 54 41 56 41 57 48 83 EC 40 49 8B E9",
                criAtomExAcb_LoadAcbData = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 50 33 F6",
                criAtomExPlayer_SetCueName = "48 89 5C 24 ?? 48 89 6C 24 ?? 56 57 41 56 48 83 EC 20 45 33 F6 49 8B F0",

                criAtomExCategory_GetVolumeById = "40 53 48 83 EC 20 83 64 24 ?? 00 48 0F BF D9",
                criAtomExCategory_SetVolumeById = "40 53 48 83 EC 30 8B D9 0F 29 74 24 ?? 33 C9", 

                criAtomExAcb_GetCueInfoById = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 4C 89 C7 89 D6",
            },
            "likeadragongaiden" => new()
            {
                criAtomExPlayer_Create = "48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 55 41 54 41 55 41 56 41 57 48 8B EC 48 83 EC 40 45 33 E4",
                criAtomExPlayer_Start = "48 89 5C 24 ?? 57 48 83 EC 20 48 89 CF 48 85 C9 75 ?? 44 8D 41 ?? 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 83 C8 FF EB ?? E8 ?? ?? ?? ?? 31 D2",
                criAtomExPlayer_SetFormat = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 48 8D 15 ?? ?? ?? ??",
                criAtomExPlayer_SetNumChannels = "48 89 5C 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 85 C9 74 ?? 8D 42 ??",
                criAtomExPlayer_SetSamplingRate = "48 89 5C 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 85 C9 74 ?? 85 D2",
                criAtomExPlayer_SetFile = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F0 48 8B EA 48 8B F9 48 85 C9",
                criAtomExPlayer_SetCueId = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 63 F8",
                criAtomExPlayer_SetCategoryById = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 50 48 89 CF",
                criAtomExAcb_LoadAcbFile = "48 8B C4 48 89 58 ?? 48 89 68 ?? 48 89 70 ?? 48 89 78 ?? 41 54 41 56 41 57 48 83 EC 40 49 8B E9",
                criAtomExAcb_LoadAcbData = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 50 33 F6",
                criAtomExPlayer_SetCueName = "48 8B C4 48 89 58 ?? 48 89 68 ?? 48 89 70 ?? 57 48 83 EC 20 33 ED",

                criAtomExCategory_GetVolumeById = "40 53 48 83 EC 20 83 64 24 ?? 00 0F B7 D9",
                criAtomExCategory_SetVolumeById = "40 53 48 83 EC 30 8B D9 0F 29 74 24 ?? 33 C9",

                criAtomExAcb_GetCueInfoById = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 4C 89 C7 89 D6",
            },
            "LostJudgment" => new()
            {
                criAtomExPlayer_Create = "48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 55 41 54 41 55 41 56 41 57 48 8B EC 48 83 EC 40 45 33 ED",
                criAtomExPlayer_Start = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 44 8D 41 ?? 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 83 C8 FF EB ?? E8 ?? ?? ?? ?? 33 D2",
                criAtomExPlayer_SetFormat = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 48 8D 15 ?? ?? ?? ??",
                criAtomExPlayer_SetNumChannels = "48 89 5C 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 85 C9 74 ?? 8D 42 ??",
                criAtomExPlayer_SetSamplingRate = "48 89 5C 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 85 C9 74 ?? 85 D2",
                criAtomExPlayer_SetFile = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F0 48 8B EA 48 8B F9 48 85 C9",
                criAtomExPlayer_SetCueId = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 63 F8",
                criAtomExPlayer_SetCategoryById = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 50 48 8B F9 8B F2",
                criAtomExAcb_LoadAcbFile = "48 8B C4 48 89 58 ?? 48 89 68 ?? 48 89 70 ?? 48 89 78 ?? 41 54 41 56 41 57 48 83 EC 40 49 8B E9",
                criAtomExAcb_LoadAcbData = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 50 33 F6",
                criAtomExPlayer_SetCueName = "48 8B C4 48 89 58 ?? 48 89 68 ?? 48 89 70 ?? 57 48 83 EC 20 33 ED",

                criAtomExCategory_GetVolumeById = "40 53 48 83 EC 20 83 64 24 ?? 00 0F B7 D9",
                criAtomExCategory_SetVolumeById = "40 53 48 83 EC 30 8B D9 0F 29 74 24 ?? 33 C9",

                criAtomExAcb_GetCueInfoById = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 4C 89 C7 89 D6",
            },
            "RainCodePlus-Win64-Shipping" => new()
            {
                criAtomExPlayer_Create = "48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 55 41 54 41 55 41 56 41 57 48 8B EC 48 83 EC 40 45 33 E4 41 8B F8",
                criAtomExPlayer_Start = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 44 8D 41 ?? 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 83 C8 FF",
                criAtomExPlayer_SetData = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 41 8B F0 48 8B EA",
                criAtomExPlayer_SetFormat = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 48 8D 15 ?? ?? ?? ??",
                criAtomExPlayer_SetNumChannels = "48 89 5C 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 85 C9 74 ?? 8D 42 ??",
                criAtomExPlayer_SetSamplingRate = "48 89 5C 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 85 C9 74 ?? 85 D2",
                criAtomExPlayer_SetCueName = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F8 48 8B EA 48 8B D9 48 85 C9",
                criAtomExPlayer_SetCategoryById = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 50 48 8B F9 8B F2",

                criAtomExAcb_LoadAcbData = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 50 33 F6",

                criAtomExCategory_GetVolumeById = "40 53 48 83 EC 20 8B D9 33 C9 E8 ?? ?? ?? ?? 85 C0",
                criAtomExCategory_GetVolume = "40 53 48 83 EC 20 0F B7 D9 33 C9 E8 ?? ?? ?? ?? 33 C9 85 C0 75 ?? 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 33 C0 48 83 C4 20 5B C3 66 85 DB 79 ??",
                criAtomConfig_GetCategoryIndexById = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B F9 BE FF FF 00 00",
                criAtomExCategory_SetVolume = "40 53 48 83 EC 30 0F B7 D9",
                criAtomExPlayer_GetStatus = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 44 8D 41 ?? 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 8D 47 ??",

                criAtomExAcb_GetCueInfoByName = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F0 48 8B FA 48 8B D9 48 85 D2 75 ??",
                criAtomExAcb_GetCueInfoById = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F8 8B F2 48 8B D9 4D 85 C0",
            },
            "SparkingZERO-Win64-Shipping" => new()
            {
                criAtomExPlayer_Create = "48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 55 41 54 41 55 41 56 41 57 48 8B EC 48 83 EC 40 45 33 E4",
                criAtomExPlayer_Start = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 44 8D 41 ?? 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 83 C8 FF",
                criAtomExPlayer_SetData = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 41 8B F0 48 8B EA",
                criAtomExPlayer_SetFormat = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 85 C9 75 ?? 48 8D 15 ?? ?? ?? ?? 41 B8 FE FF FF FF",
                criAtomExPlayer_SetNumChannels = "48 89 5C 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 85 C9 74 ?? 8D 42 ??",
                criAtomExPlayer_SetSamplingRate = "48 89 5C 24 ?? 57 48 83 EC 20 8B FA 48 8B D9 48 85 C9 74 ?? 85 D2",
                criAtomExPlayer_SetCueName = "48 89 5C 24 ?? 48 89 6C 24 ?? 56 57 41 56 48 83 EC 20 45 33 F6 49 8B F0 48 8B FA",
                criAtomExPlayer_SetCategoryById = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 50 48 8B F9 8B F2",

                criAtomExAcb_LoadAcbData = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 50 33 F6",

                criAtomExCategory_GetVolume = "40 53 48 83 EC 20 48 0F BF D9 33 C9 E8 ?? ?? ?? ?? 33 C9 85 C0 75 ?? 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 33 C0 48 83 C4 20 5B C3 66 85 DB 79 ??",
                criAtomExCategory_GetVolumeById = "40 53 48 83 EC 20 8B D9 33 C9 E8 ?? ?? ?? ?? 85 C0",
                criAtomExCategory_SetVolume = "40 53 48 83 EC 30 0F 29 74 24 ?? 0F 28 F1 48 8B D9 48 85 C9 75 ?? 48 8D 15 ?? ?? ?? ?? EB ?? 0F 57 C0 F3 0F 5A C6 E8 ?? ?? ?? ?? 33 C9",

                criAtomExAcb_GetCueInfoByName = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F0 48 8B FA 48 8B D9 48 85 D2 75 ??",
                criAtomExAcb_GetCueInfoById = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F8 8B F2 48 8B D9 4D 85 C0",
                criAtomConfig_GetCategoryIndexById = "48 89 5C 24 ?? 57 48 83 EC 20 8B F9 48 8B 0D ?? ?? ?? ??",
            },
            _ => throw new Exception($"Unknown game: {game}"),
        };
}
