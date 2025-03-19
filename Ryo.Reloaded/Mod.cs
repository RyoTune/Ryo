using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Ryo.Interfaces;
using Ryo.Reloaded.Audio;
using Ryo.Reloaded.Configuration;
using Ryo.Reloaded.CRI.CriAtomEx;
using Ryo.Reloaded.CRI.CriUnreal;
using Ryo.Reloaded.CRI.Mana;
using Ryo.Reloaded.Movies;
using Ryo.Reloaded.Template;
using SharedScans.Interfaces;
using System.Diagnostics;
using System.Drawing;

namespace Ryo.Reloaded;

public class Mod : ModBase, IExports
{
    public const string NAME = "Ryo";

    private readonly IModLoader modLoader;
    private readonly IReloadedHooks hooks;
    private readonly ILogger log;
    private readonly IMod owner;

    private Config config;
    private readonly IModConfig modConfig;

    private readonly string game;

    private readonly CriAtomEx criAtomEx;
    private readonly CriAtomRegistry criAtomRegistry;
    private readonly CriUnreal criUnreal;
    private readonly CriMana criMana;

    private readonly AudioRegistry audioRegistry;
    private readonly AudioService audioService;
    private readonly AudioPreprocessor preprocessor = new();

    private readonly MovieRegistry movieRegistry;
    private readonly MovieService movieService;

    private readonly RyoApi ryoApi;

    public Mod(ModContext context)
    {
        this.modLoader = context.ModLoader;
        this.hooks = context.Hooks!;
        this.log = context.Logger;
        this.owner = context.Owner;
        this.config = context.Configuration;
        this.modConfig = context.ModConfig;

#if DEBUG
        Debugger.Launch();
#endif

        this.game = Path.GetFileNameWithoutExtension(this.modLoader.GetAppConfig().AppId);

        Log.Init($"Ryo ({this.game.ToUpper()})", this.log, Color.FromArgb(110, 209, 248), true);
        Log.LogLevel = this.config.LogLevel;

        this.modLoader.GetController<IStartupScanner>().TryGetTarget(out var scanner);
        this.modLoader.GetController<ISharedScans>().TryGetTarget(out var scans);

        this.criAtomEx = new(this.game, scans!, this.hooks);
        this.modLoader.AddOrReplaceController<ICriAtomEx>(this.owner, this.criAtomEx);

        this.criAtomRegistry = new();
        this.modLoader.AddOrReplaceController<ICriAtomRegistry>(this.owner, this.criAtomRegistry);

        this.criUnreal = new(this.game);
        this.criMana = new(scans!, this.game);

        this.audioRegistry = new(this.game, this.preprocessor);
        this.audioService = new(this.game, scans!, this.criAtomEx, this.criAtomRegistry, this.audioRegistry);

        this.movieRegistry = new();
        this.movieService = new(scans!, this.criMana, this.movieRegistry);

        this.ryoApi = new(this.criAtomRegistry, this.audioRegistry, this.preprocessor, this.movieRegistry);
        this.modLoader.AddOrReplaceController<IRyoApi>(this.owner, this.ryoApi);

        ScanHooks.Init(scanner!, this.hooks);

        this.modLoader.ModLoading += this.OnModLoading;
        this.modLoader.OnModLoaderInitialized += this.OnModLoaderInitialized;

        this.ApplyConfig();
    }

    private void OnModLoaderInitialized()
    {
        if (this.config.PreloadAudio)
        {
            Log.Information("Preloading audio.");
            this.audioRegistry.PreloadAudio();
        }
    }

    private void OnModLoading(IModV1 mod, IModConfigV1 config)
    {
        if (!config.ModDependencies.Contains(this.modConfig.ModId))
        {
            return;
        }

        var modDir = this.modLoader.GetDirectoryForModId(config.ModId);
        var ryoDir = Path.Join(modDir, "ryo", this.game);
        if (Directory.Exists(ryoDir))
        {
            this.audioRegistry.AddAudioFolder(ryoDir);
            this.movieRegistry.AddMoviePath(ryoDir);
        }
    }

    private void ApplyConfig()
    {
        Log.LogLevel = this.config.LogLevel;
        this.criAtomEx.SetDevMode(this.config.DevMode);
        this.criUnreal.SetDevMode(this.config.DevMode);
        this.audioService.SetDevMode(this.config.DevMode);
        this.movieService.SetDevMode(this.config.DevMode);
    }

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        this.config = configuration;
        this.log.WriteLine($"[{this.modConfig.ModId}] Config Updated: Applying");
        this.ApplyConfig();
    }

    public Type[] GetTypes() => new[] { typeof(ICriAtomEx), typeof(IRyoApi), typeof(ICriAtomRegistry) };

    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}