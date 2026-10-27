using Ryo.Definitions.Enums;
using Ryo.Definitions.Functions;
using Ryo.Interfaces.Enums;

// ReSharper disable MemberCanBePrivate.Global

namespace Ryo.Interfaces.Classes;

/// <summary>
/// Audio configuration.
/// </summary>
public class AudioConfig
{
    #region General Settings

    /// <summary>
    /// Version of this audio config.
    /// </summary>
    public int Version { get; set; } = 2;
    
    /// <summary>
    /// Gets or sets the container group.
    /// Allows for the container to be retrieved as part of a group.
    /// </summary>
    public string? GroupId { get; set; }

    /// <summary>
    /// Whether this audio is enabled.
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the shared container ID.
    /// Setting this allows for multiple audios to be added
    /// to the same cue container.
    /// </summary>
    public string? SharedContainerId { get; set; }

    /// <summary>
    /// Custom volume to set for the audio.
    /// Ryo 3+: The volume is set on the cue or player.
    /// Ryo Classic: The volume is set either on a sound category or the player itself.
    /// </summary>
    public float? Volume { get; set; }

    /// <summary>
    /// Represents the volume level specific to the Ryo Classic audio configuration.
    /// </summary>
    public float? RyoClassicVolume { get; set; }

    /// <summary>
    /// Custom playback modes when audio contains multiple files.
    /// </summary>
    public PlaybackMode? PlaybackMode { get; set; }

    #endregion

    #region Cue Settings
    
    /// <summary>
    /// The cue name or ID this audio is assigned to. Can be a new or existing cue.<br/>
    /// Ryo Classic: Used when replacing audio played with <see cref="CriAtomExFunctions.criAtomExPlayer_SetCueId"/> and <see cref="CriAtomExFunctions.criAtomExPlayer_SetCueName"/> .
    /// </summary>
    public string? CueName { get; set; }

    /// <summary>
    /// ACB name the target cue is from.
    /// Used when replacing audio played with <see cref="CriAtomExFunctions.criAtomExPlayer_SetCueId"/> and <see cref="CriAtomExFunctions.criAtomExPlayer_SetCueName"/> .
    /// </summary>
    public string? AcbName { get; set; }

    /// <summary>
    /// Name of cue to use as a template.
    /// </summary>
    public string? TemplateCueName { get; set; }

    /// <summary>
    /// ID of cue to use as a template.
    /// </summary>
    public int? TemplateCueId { get; set; }
    
    /// <summary>
    /// Sound categories to apply to cue.
    /// </summary>
    public int[]? CategoryIds { get; set; }

    /// <summary>
    /// Sound categories to apply to cue.
    /// </summary>
    public string[]? CategoryNames { get; set; }
    
    #endregion

    /// <summary>
    /// The audio hash this audio is assigned to.
    /// Used when replacing audio played with <see cref="CriAtomExFunctions.criAtomExPlayer_SetData"/>.
    /// </summary>
    public string? AudioHash { get; set; }

    /// <summary>
    /// File path this audio is assigned to. Can be a new or existing file.
    /// Used when replacing audio played with <see cref="CriAtomExFunctions.criAtomExPlayer_SetFile"/>.
    /// </summary>
    public string? AudioFilePath { get; set; }

    /// <summary>
    /// Ryo Classic: Set a custom volume on the player instead of a category.
    /// </summary>
    public bool? UsePlayerVolume { get; set; }

    #region Ryo Classic

    /// <summary>
    /// Specific player to play this audio with.
    /// </summary>
    public int? PlayerId { get; set; }
    
    /// <summary>
    /// Category ID to set custom volume on.
    /// If not set, or category is not found in <c>CategoryIds</c>, the first category will be used.
    /// </summary>
    public int? VolumeCategoryId { get; set; }

    /// <summary/>
    public CriAtomFormat? Format { get; set; }

    /// <summary>
    /// Sample rate of audio.
    /// Seems to be ignored if an audio format includes this data, such as HCA and ADX.
    /// </summary>
    public int? SampleRate { get; set; }

    /// <summary/>
    public int? NumChannels { get; set; }

    /// <summary/>
    public string? Key { get; set; }

    /// <summary/>
    public string[]? Tags { get; set; }

    #endregion

    /// <summary>
    /// Creates a shallow copy of this audio config.
    /// </summary>
    /// <returns></returns>
    public AudioConfig Clone() => (AudioConfig)MemberwiseClone();

    /// <summary>
    /// Applies defined settings from <paramref name="newConfig"/>.
    /// </summary>
    /// <param name="newConfig">Config containing the settings to apply.</param>
    public void Apply(AudioConfig newConfig)
    {
        // General
        GroupId = newConfig.GroupId ?? GroupId;
        IsEnabled = newConfig.IsEnabled ?? IsEnabled;
        SharedContainerId = newConfig.SharedContainerId ?? SharedContainerId;
        Volume = newConfig.Volume ?? Volume;
        UsePlayerVolume = newConfig.UsePlayerVolume ?? UsePlayerVolume;
        PlaybackMode = newConfig.PlaybackMode ?? PlaybackMode;
        
        // Cues
        CueName = newConfig.CueName ?? CueName;
        AcbName = newConfig.AcbName ?? AcbName;
        CategoryIds = newConfig.CategoryIds ?? CategoryIds;
        TemplateCueName = newConfig.TemplateCueName ?? TemplateCueName;
        TemplateCueId = newConfig.TemplateCueId ?? TemplateCueId;
        Volume = newConfig.Volume ?? Volume;
        CategoryIds = newConfig.CategoryIds ?? CategoryIds;
        CategoryNames = newConfig.CategoryNames ?? CategoryNames;
        
        // SetFile/SetData
        AudioHash = newConfig.AudioHash ?? AudioHash;
        AudioFilePath = newConfig.AudioFilePath ?? AudioFilePath;
        
        // Deprecated/Unknown
        PlayerId = newConfig.PlayerId ?? PlayerId;
        VolumeCategoryId = newConfig.VolumeCategoryId ?? VolumeCategoryId;
        NumChannels = newConfig.NumChannels ?? NumChannels;
        SampleRate = newConfig.SampleRate ?? SampleRate;
        Tags = newConfig.Tags ?? Tags;
        Key = newConfig.Key ?? Key;
    }

    /// <summary>
    /// Migrates this audio config to the latest version.
    /// </summary>
    public void Migrate()
    {
        if (Version < 3)
        {
            RyoClassicVolume = Volume;
            Volume = null;
            Version = 3;
        }
    }
}
