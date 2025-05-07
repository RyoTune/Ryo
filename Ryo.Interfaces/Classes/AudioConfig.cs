using Ryo.Definitions.Enums;
using Ryo.Definitions.Functions;
using Ryo.Interfaces.Enums;

namespace Ryo.Interfaces.Classes;

/// <summary>
/// Audio configuration.
/// </summary>
public class AudioConfig
{
    /// <summary>
    /// Gets or sets the container group.
    /// Settings this allows for the container to be retrieved
    /// as part of a group of containers.
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
    /// Cue name this audio is assigned to. Can be a new or existing cue.
    /// Used when replacing audio played with <see cref="CriAtomExFunctions.criAtomExPlayer_SetCueId"/> and <see cref="CriAtomExFunctions.criAtomExPlayer_SetCueName"/> .
    /// </summary>
    public string? CueName { get; set; }

    /// <summary>
    /// ACB name the target cue is from.
    /// Used when replacing audio played with <see cref="CriAtomExFunctions.criAtomExPlayer_SetCueId"/> and <see cref="CriAtomExFunctions.criAtomExPlayer_SetCueName"/> .
    /// </summary>
    public string? AcbName { get; set; }

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
    /// Sample rate of audio.
    /// Seems to be ignored if audio format includes this data, such as HCA and ADX.
    /// </summary>
    public int? SampleRate { get; set; }

    /// <summary/>
    public CriAtomFormat? Format { get; set; }

    /// <summary/>
    public int? NumChannels { get; set; }

    /// <summary>
    /// Specific player to play this audio with.
    /// </summary>
    public int? PlayerId { get; set; }

    /// <summary>
    /// Sound categories to apply to player before playing.
    /// </summary>
    public int[]? CategoryIds { get; set; }

    /// <summary>
    /// Custom volume to set before playing audio.
    /// The volume is set either on a sound category or the player itself.
    /// </summary>
    public float? Volume { get; set; }

    /// <summary/>
    public string? Key { get; set; }

    /// <summary/>
    public string[]? Tags { get; set; }

    /// <summary>
    /// Custom playback modes when audio contains multiple files.
    /// </summary>
    public PlaybackMode? PlaybackMode { get; set; }

    /// <summary>
    /// Set custom volume on the player instead of category.
    /// </summary>
    public bool? UsePlayerVolume { get; set; }

    /// <summary>
    /// Category ID to set custom volume on.
    /// If not set, or category is not found in <c>CategoryIds</c>, the first category will be used.
    /// </summary>
    public int? VolumeCategoryId { get; set; }

    /// <inheritdoc/>
    public AudioConfig Clone() => (AudioConfig)this.MemberwiseClone();

    /// <summary>
    /// Applies defined settings from <paramref name="newConfig"/>.
    /// </summary>
    /// <param name="newConfig">Config containing the settings to apply.</param>
    public void Apply(AudioConfig newConfig)
    {
        this.GroupId = newConfig.GroupId ?? this.GroupId;
        this.IsEnabled = newConfig.IsEnabled ?? this.IsEnabled;
        this.CueName = newConfig.CueName ?? this.CueName;
        this.AcbName = newConfig.AcbName ?? this.AcbName;
        this.PlayerId = newConfig.PlayerId ?? this.PlayerId;
        this.CategoryIds = newConfig.CategoryIds ?? this.CategoryIds;
        this.NumChannels = newConfig.NumChannels ?? this.NumChannels;
        this.SampleRate = newConfig.SampleRate ?? this.SampleRate;
        this.Volume = newConfig.Volume ?? this.Volume;
        this.Tags = newConfig.Tags ?? this.Tags;
        this.Key = newConfig.Key ?? this.Key;
        this.SharedContainerId = newConfig.SharedContainerId ?? this.SharedContainerId;
        this.AudioHash = newConfig.AudioHash ?? this.AudioHash;
        this.AudioFilePath = newConfig.AudioFilePath ?? this.AudioFilePath;
        this.PlaybackMode = newConfig.PlaybackMode ?? this.PlaybackMode;
        this.UsePlayerVolume = newConfig.UsePlayerVolume ?? this.UsePlayerVolume;
        this.VolumeCategoryId = newConfig.VolumeCategoryId ?? this.VolumeCategoryId;
    }
}
