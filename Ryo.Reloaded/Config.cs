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
    [Description("Enables support for playing unencrypted HCA in games with audio encryption. (Experimental)")]
    [DefaultValue(true)]
    public bool UnencryptedHcaEnabled { get; set; } = true;
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}