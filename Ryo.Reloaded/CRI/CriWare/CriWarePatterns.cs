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
            _ => new(),
        };

    public string? HCADecoder_DecodeHeader { get; init; } = "48 8B C4 48 89 58 ?? 48 89 68 ?? 48 89 70 ?? 57 41 54 41 55 41 56 41 57 48 81 EC 90 00 00 00 4C 8D 70";
    public string? criadxcodec_DecodeStream { get; init; } =  "48 8B C4 48 89 58 ?? 4C 89 48 ?? 55 56 57 41 54 41 55 41 56 41 57 48 83 EC 50";
    public string? criAdxDecCore_SetEncryptionKey { get; init; } = "66 89 91 A2 ?? ?? ?? 66 44 89 81";
}