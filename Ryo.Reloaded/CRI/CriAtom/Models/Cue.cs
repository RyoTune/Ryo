namespace Ryo.Reloaded.CRI.CriAtom.Models;

public record Cue(string Acb, int Id, string Name)
{
    public static readonly Cue None = new(string.Empty, -1, string.Empty);
}