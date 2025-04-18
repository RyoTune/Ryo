// ReSharper disable InconsistentNaming
namespace Ryo.Reloaded.CRI.CriWare;

public class CriWarePatterns
{
    public static CriWarePatterns GetGamePatterns(string game)
        => game.ToLowerInvariant() switch
        {
            "metaphor" => new()
            {
                HCADecoder_DecodeHeader = "48 89 E0 48 89 58 ?? 48 89 68 ?? 48 89 70 ?? 57 41 54 41 55 41 56 41 57 48 81 EC 90 00 00 00",
            },
            _ => new()
            {
                HCADecoder_DecodeHeader = "48 8B C4 48 89 58 ?? 48 89 68 ?? 48 89 70 ?? 57 41 54 41 55 41 56 41 57 48 81 EC 90 00 00 00 4C 8D 70",
            },
        };

    public string? HCADecoder_DecodeHeader { get; private init; }
}