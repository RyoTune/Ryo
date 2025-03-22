namespace Ryo.Reloaded.CRI.CriUnreal;

internal static class CriManaGames
{
    private static readonly CriManaPatterns[] patterns = new CriManaPatterns[]
    {
        new("p3r")
        {
            criManaPlayer_SetFile = "4C 89 44 24 ?? 48 89 54 24 ?? 48 89 4C 24 ?? 48 83 EC 38 48 83 7C 24 ?? 00 75 ?? 41 B8 B8 48 33 01",
        },
        new("metaphor")
        {
            criManaPlayer_SetFile = "4C 89 44 24 ?? 48 89 54 24 ?? 48 89 4C 24 ?? 48 83 EC 38 48 83 7C 24 ?? 00 75 ?? 41 B8 FE FF FF FF 48 8D 15 ?? ?? ?? ?? 33 C9 E8 ?? ?? ?? ?? EB ?? 48 8B 44 24 ?? 48 8B 40 ?? 48 89 44 24 ?? 48 8B 44 24",
        },
        new("SMT5V-Win64-Shipping")
        {
            criManaPlayer_SetFile = "4C 89 44 24 ?? 48 89 54 24 ?? 48 89 4C 24 ?? 48 83 EC 38 48 83 7C 24 ?? 00 75 ?? 41 B8 FE FF FF FF 48 8D 15 ?? ?? ?? ?? 31 C9 E8 ?? ?? ?? ?? EB",
        },
    };

    public static CriManaPatterns GetGamePatterns(string game)
        => patterns.FirstOrDefault(x => x.Games.Contains(game, StringComparer.OrdinalIgnoreCase)) ?? new();
}

internal class CriManaPatterns
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public CriManaPatterns(params string[] games)
    {
        this.Games = games;
    }

    public string[] Games { get; }
    public string criManaPlayer_SetFile { get; internal set; }
}
