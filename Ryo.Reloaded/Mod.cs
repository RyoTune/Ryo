#if DEBUG
using System.Diagnostics;
#endif

using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Ryo.Interfaces;
using Ryo.Reloaded.Audio;
using Ryo.Reloaded.Configuration;
using Ryo.Reloaded.CRI.CriAtomEx;
using Ryo.Reloaded.CRI.Mana;
using Ryo.Reloaded.Movies;
using Ryo.Reloaded.Template;
using SharedScans.Interfaces;
using System.Drawing;
using System.Runtime.InteropServices;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Ryo.Reloaded.Common.Memory;
using Ryo.Reloaded.CRI;
using Ryo.Reloaded.CRI.CriAtom;
using Ryo.Reloaded.CRI.CriWare;
using RyoTune.Reloaded.Serialization.Yaml.Interfaces;

namespace Ryo.Reloaded;

public unsafe class Mod : ModBase, IExports
{
    public const string Name = "Ryo";

    private readonly IModLoader _modLoader;
    private readonly IReloadedHooks _reloadedHooks;
    private readonly ILogger _log;
    private readonly IMod _owner;

    private readonly IModConfig _modConfig;

    private readonly string _game;

    private CriAtomEx? _criAtomEx;
    private readonly CriAtomRegistry _criAtomRegistry;
    private readonly CriMana _criMana;
    private readonly CriWareHooks _criHooks;

    private readonly AudioRegistry _audioRegistry;
    private AudioService? _audioService;
    private readonly AudioPreprocessor _preprocessor = new();

    private readonly MovieRegistry _movieRegistry;
    private readonly MovieService _movieService;

    private readonly RyoApi _ryoApi;
    private CriAtomService? _atomService;

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _log = context.Logger;
        _owner = context.Owner;
        _modConfig = context.ModConfig;
        _reloadedHooks = context.Hooks!;
        Config = context.Configuration;

#if DEBUG
        Debugger.Launch();
#endif

        _game = Path.GetFileNameWithoutExtension(_modLoader.GetAppConfig().AppLocation);

        Project.Initialize(_modConfig, _modLoader, _log, Color.FromArgb(110, 209, 248), true);
        Log.LogLevel = Config.LogLevel;
        Log.Debug($"Game: {_game}");

        _modLoader.GetController<ISharedScans>().TryGetTarget(out var scans);
        
        _modLoader.GetController<IReloadedYaml>().TryGetTarget(out var yaml);
        var yamlDeserializer = yaml!.CreateDeserializer();
        
        // TODO: BGME for Unreal currently requests this, but doesn't use it.
        _modLoader.AddOrReplaceController<ICriAtomEx>(_owner, _criAtomEx);

        _criAtomRegistry = new();
        _modLoader.AddOrReplaceController<ICriAtomRegistry>(_owner, _criAtomRegistry);

        _criMana = new(scans!, _game);
        _criHooks = new(scans!, _game);
        _audioRegistry = new(_game, _preprocessor, yamlDeserializer);
        
        _modLoader.WhenModsLoaded(() =>
        {
            // Use Ryo 3+ for games that support it.
            if (!Config.ForceRyoClassic
                && Project.Scans.HasScan(nameof(CriAtomService.CRIATOM_BuildCueWaveformSource)))
            {
                var isUnrealGame = Project.Scans.HasScan("GMalloc");
                _atomService = isUnrealGame
                    ? new(_audioRegistry, new UnrealAllocator())
                    : new(_audioRegistry, new DefaultAllocator());
                
                Log.Information($"Using Ryo 3+ for: {_game}");
            }
            else
            {
                _criAtomEx = new(_game, scans!, _reloadedHooks);
                _modLoader.AddOrReplaceController<ICriAtomEx>(_owner, _criAtomEx);
                _audioService = new(_game, scans!, _criAtomEx, _criAtomRegistry, _audioRegistry);
                _audioService.SetDevMode(Config.DevMode);
                
                Log.Information($"Using Ryo Classic for: {_game}");
            }
        });
        
        _movieRegistry = new(yamlDeserializer);
        _movieService = new(scans!, _criMana, _movieRegistry);

        _ryoApi = new(_criAtomRegistry, _audioRegistry, _preprocessor, _movieRegistry);
        _modLoader.AddOrReplaceController<IRyoApi>(_owner, _ryoApi);

        Project.Scans.AddScan("\"CRI AtomEx/PC\" String", "43 52 49 20 41 74 6F 6D 45 78 2F 50 43", result =>
        {
            var criBuild = Marshal.PtrToStringAnsi(result)!.TrimEnd('\n');
            Log.Debug($"Cri AtomEx Build: {criBuild}");

            var startOfs = criBuild.IndexOf("Ver.", StringComparison.Ordinal) + 4;
            var endOfs = criBuild.IndexOf("Build", StringComparison.Ordinal) - 1;
            var atomExVer = criBuild.Substring(startOfs, endOfs - startOfs);
            
            CriWareConfig.SetAtomExVersion(new(atomExVer));
        });
        
        Project.Scans.AddScan("HCADecoder_SetDecryptionTable" ,"48 85 C9 75 ?? 8D 41 ?? C3 48 85 D2 74 1D", result =>
        {
            var tableFieldOfs = (int*)(result + 0x2B + 0x3);
            Log.Debug($"HCA Decryption Table Field Offset: 0x{*tableFieldOfs:X}");
            CriWareConfig.HcaDecodedEncryptKeyOffset = *tableFieldOfs;
        }, () => Log.Information("Failed to find 'HCADecoder_SetDecryptionTable'. Unencrypted HCA in encrypted games is unsupported."));

        _modLoader.ModLoading += OnModLoading;

        ApplyConfig();
    }

    public static Config Config { get; private set; } = null!;

    private void OnModLoading(IModV1 newMod, IModConfigV1 newModConfig)
    {
        if (!Project.IsModDependent(newModConfig)) return;

        var modDir = _modLoader.GetDirectoryForModId(newModConfig.ModId);
        var ryoDir = Path.Join(modDir, "ryo", _game);
        if (Directory.Exists(ryoDir))
        {
            _audioRegistry.AddAudioFolder(ryoDir);
            _movieRegistry.AddMoviePath(ryoDir);
        }
    }

    private void ApplyConfig()
    {
        Log.LogLevel = Config.LogLevel;
        _criAtomEx?.SetDevMode(Config.DevMode);
        _audioService?.SetDevMode(Config.DevMode);
        _movieService.SetDevMode(Config.DevMode);
        _criHooks.SetConfig(Config);
    }

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        Config = configuration;
        _log.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
        ApplyConfig();
    }

    public Type[] GetTypes() => [typeof(ICriAtomEx), typeof(IRyoApi), typeof(ICriAtomRegistry)];

    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}