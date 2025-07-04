using Ryo.Interfaces;
using Ryo.Interfaces.Classes;

namespace Ryo.Reloaded.Audio;

internal static class GameDefaults
{
    private static readonly Dictionary<string, Func<AcbCueInfo, string>> linkCueCallback = new(StringComparer.OrdinalIgnoreCase)
    {
        ["p5r"] = (AcbCueInfo info) =>
        {
            if (info.Acb == "bgm") return info.Cue.Replace("link", "bgm");
            return info.Cue;
        },

        ["p3r"] = (AcbCueInfo info) =>
        {
            if (info.Acb == "bgm")
            {
                var cueParts = info.Cue.Split('_');
                if (cueParts.Length == 2 && int.TryParse(cueParts[1], out var bgmId))
                {
                    if (bgmId >= 1000 && bgmId < 2000)
                    {
                        var adjustedId = bgmId - 1000;
                        var victoryIds = new List<int> { 5, 11, 27, 37, 44 };
                        if (victoryIds.Contains(adjustedId))
                        {
                            adjustedId = victoryIds.IndexOf(adjustedId) + 1;
                            return $"Sound_Result_{adjustedId:00}";
                        }

                        return $"Sound_{adjustedId:00}";
                    }
                    else if (bgmId >= 2000)
                    {
                        return $"EA_Sound_{bgmId - 2000:00}";
                    }
                }

                return info.Cue.Replace("link_", string.Empty);
            }

            return info.Cue;
        },
    };

    public static Func<AcbCueInfo, string>? GetLinkCueCb(string game)
    {
        linkCueCallback.TryGetValue(game, out var cb);
        return cb;
    }

    private static readonly Dictionary<string, AudioConfig> defaults = new(StringComparer.OrdinalIgnoreCase)
    {
        ["p5r"] = new()
        {
            UsePlayerVolume = true,
        },
        ["p3p"] = new()
        {
            UsePlayerVolume = true,
        },
        ["p3r"] = new()
        {
            AcbName = "bgm",
            Volume = 0.15f,
        },
        ["SMT5V-Win64-Shipping"] = new()
        {
            AcbName = "bgm",
            Volume = 0.70f,
            VolumeCategoryId = 4,
        },
        ["likeadragon8"] = new()
        {
            CategoryIds = [11],
            Volume = 0.35f,
        },
        ["likeadragongaiden"] = new()
        {
            CategoryIds = [11],
            Volume = 0.35f,
        },
        ["LostJudgment"] = new()
        {
            CategoryIds = [11],
            Volume = 0.35f,
        },
        ["likeadragonpirates"] = new()
        {
            CategoryIds = [11],
            Volume = 1.0f,
        },
        ["RainCodePlus-Win64-Shipping"] = new()
        {
            Volume = 0.35f,
        },
    };

    public static AudioConfig CreateDefaultConfig(string game)
    {
        if (defaults.TryGetValue(game, out var defaultConfig))
        {
            return defaultConfig.Clone();
        }

        return new();
    }

    public static void ConfigureCriAtom(string game, ICriAtomEx criAtomEx)
    {
        var normalizedGame = game.ToLower();
        switch (normalizedGame)
        {
            case "p5r":
                criAtomEx.SetPlayerConfigById(255, new()
                {
                    maxPathStrings = 2,
                    maxPath = 256,
                    enableAudioSyncedTimer = true,
                    updatesTime = true,
                });
                break;
            case "p4g":
                criAtomEx.SetPlayerConfigById(0, new()
                {
                    maxPathStrings = 2,
                    voiceAllocationMethod = 1,
                    maxPath = 256,
                });
                break;
            case "p3p":
                criAtomEx.SetPlayerConfigById(2, new()
                {
                    maxPathStrings = 10,
                    voiceAllocationMethod = 1,
                });
                break;
            default: break;
        }
    }
}
