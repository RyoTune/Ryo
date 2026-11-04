namespace Ryo.Reloaded.Audio.Models;

public class CueConfig
{
    public string? TemplateCueName { get; init; }
    
    public int? TemplateCueId { get; init; }

    public float? Volume { get; init; }
    
    public ClassicVolume? ClassicVolume { get; init; }

    public int[]? CategoryIds { get; init; }

    public string[]? CategoryNames { get; init; }
}

public record ClassicVolume(float? Volume, bool ShouldBoost);
