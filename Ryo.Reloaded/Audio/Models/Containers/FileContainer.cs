using Ryo.Interfaces.Classes;

namespace Ryo.Reloaded.Audio.Models.Containers;

internal class FileContainer : AudioContainer
{
    public FileContainer(string filePath, AudioConfig? config = null)
        : base(config)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException($"'{nameof(filePath)}' cannot be null or empty.", nameof(filePath));
        }

        this.Name = $"Audio File: {filePath}";
    }

    public override string Name { get; }
}
