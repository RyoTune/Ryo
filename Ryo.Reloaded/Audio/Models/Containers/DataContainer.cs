using Ryo.Interfaces.Classes;

namespace Ryo.Reloaded.Audio.Models.Containers;

internal class DataContainer : AudioContainer
{
    public DataContainer(string audioHash, AudioConfig? config = null)
        : base(config)
    {
        if (string.IsNullOrEmpty(audioHash))
        {
            throw new ArgumentException($"'{nameof(audioHash)}' cannot be null or empty.", nameof(audioHash));
        }

        this.Name = $"Audio Data: {audioHash}";
    }

    public override string Name { get; }
}
