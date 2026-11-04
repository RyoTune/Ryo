using Ryo.Reloaded.Template.Configuration;
using System.ComponentModel;

namespace Ryo.Reloaded.Configuration;

public class Config : Configurable<Config>
{
    [DisplayName("Log Level")]
    [DefaultValue(LogLevel.Information)]
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    [DisplayName("Developer Mode")]
    [Description("Display extra information useful for mod development.")]
    [DefaultValue(false)]
    public bool DevMode { get; set; } = false;

    [DisplayName("Unencrypted HCA Support")]
    [Description("Enables support for playing unencrypted HCA in games with audio encryption.")]
    [DefaultValue(true)]
    public bool UnencryptedHcaEnabled { get; set; } = true;

    [Category("Debugging")]
    [DisplayName("Ryo 3+: Classic Volume Fix")]
    [Description("Boosts the volume by a flat amount for audio mods\nin games that previously had loudness issues.")]
    [DefaultValue(0.75)]
    public double RyoClassicVolumeBoost { get; set; } = 0.75;

    [Category("Debugging")]
    [DisplayName("CriWare Logging")]
    [Description("Show CriWare logs in the console.")]
    [DefaultValue(false)]
    public bool CriLogsEnabled { get; set; } = false;
    
    [Category("Debugging")]
    [DisplayName("Force Rebuild ACBs")]
    [Description("Forces all ACBs to be rebuilt.")]
    [DefaultValue(false)]
    public bool ForceRebuildAcbs { get; set; } = false;

    [Category("Debugging")]
    [DisplayName("Force Ryo Classic")]
    [Description("Uses original Ryo implemenation instead of Ryo 3+.")]
    [DefaultValue(false)]
    public bool ForceRyoClassic { get; set; } = false;
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}