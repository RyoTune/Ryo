using Ryo.Interfaces.Classes;

namespace Ryo.Reloaded.Audio.Models.Containers;

internal class CueContainer : AudioContainer
{
    public CueContainer(string cueName, string acbName, AudioConfig? config = null)
        : base(config)
    {
        if (string.IsNullOrEmpty(cueName))
        {
            throw new ArgumentException($"'{nameof(cueName)}' cannot be null or empty.", nameof(cueName));
        }

        if (string.IsNullOrEmpty(acbName))
        {
            throw new ArgumentException($"'{nameof(acbName)}' cannot be null or empty.", nameof(acbName));
        }
        
        Name = $"Cue: {cueName} / {acbName}";
    }

    public override string Name { get; }
}
