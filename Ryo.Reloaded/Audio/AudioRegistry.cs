using System.Diagnostics.CodeAnalysis;
using Ryo.Reloaded.Audio.Models;
using Ryo.Definitions.Enums;
using Ryo.Interfaces.Classes;
using Ryo.Reloaded.Audio.Models.Containers;
using Ryo.Reloaded.Common.Models;
using RyoTune.Reloaded.Serialization.Yaml.Interfaces;

namespace Ryo.Reloaded.Audio;

internal class AudioRegistry(string game, AudioPreprocessor preprocessor, IYamlDeserializer yamlDeserializer)
{
    private const string RyoFileDirName = "FILE.ryo";
    private const string RyoDataDirName = "DATA.ryo";

    private readonly AudioConfig _defaultConfig = GameDefaults.CreateDefaultConfig(game);
    private readonly AudioPreprocessor _preprocessor = preprocessor;
    
    private readonly Dictionary<string, Dictionary<CueKey, CueRegistration>> _registeredCues = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<CueKey, List<AudioContainer>> _cueContainers = new(CueComparer.Instance);
    private readonly Dictionary<string, List<AudioContainer>> _fileContainers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<AudioContainer>> _dataContainers = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<CueRegistration> GetRegisteredCuesForAcb(string acbName)
    {
        if (_registeredCues.TryGetValue(acbName, out var cues))
            return cues.Values.ToArray();

        return [];
    }

    private bool RegisterCue(AudioConfig config)
    {
        if (string.IsNullOrEmpty(config.AcbName) || string.IsNullOrEmpty(config.CueName))
        {
            Log.Error($"{nameof(RegisterCue)} || Cue name and ACB name must be set. Cue: {config.CueName} / ACB: {config.AcbName}");
            return false;
        }
        
        if (!_registeredCues.TryGetValue(config.AcbName, out var cueRegistrations))
            _registeredCues[config.AcbName] = cueRegistrations = [];

        var cueConfig = new CueConfig()
        {
            CategoryIds = config.CategoryIds,
            CategoryNames = config.CategoryNames,
            Volume = config.Volume,
            RyoClassicVolume = config.RyoClassicVolume,
        };
        
        cueRegistrations[new(config.CueName, config.AcbName)] = new(config.CueName, cueConfig);
        return true;
    }

    public void AddAudioPath(string path, AudioConfig? config)
    {
        if (Directory.Exists(path))
        {
            this.AddAudioFolder(path, config);
        }
        else if (File.Exists(path))
        {
            this.AddAudioFile(path, config);
        }
        else
        {
            Log.Error($"Audio path was not found.\nPath: {path}");
        }
    }

    private void AddAudioFile(string file, AudioConfig? preConfig)
    {
        // Start with the game default config.
        var config = this._defaultConfig.Clone();

        // Apply config settings from arg.
        if (preConfig != null)
        {
            config.Apply(preConfig);
        }

        // Apply config settings from file config.
        var configFile = Path.ChangeExtension(file, ".yaml");
        if (File.Exists(configFile) && ParseConfigFile(configFile) is { } fileConfig)
        {
            config.Apply(fileConfig);
        }

        // Get format from ext.
        config.Format = GetAudioFormat(file);

        // Using the parent directory forces replacement files to be
        // in a subfolder of the actual file path. Basically a requirement since
        // new files can be different formats.
        var parentDir = Path.GetDirectoryName(file)!;
        if (file.Contains(RyoFileDirName, StringComparison.OrdinalIgnoreCase))
        {
            var fileDirIndex = parentDir.IndexOf(RyoFileDirName, StringComparison.OrdinalIgnoreCase);
            config.AudioFilePath = parentDir[(fileDirIndex + RyoFileDirName.Length + 1)..].Replace('\\', '/');
        }

        if (file.Contains(RyoDataDirName, StringComparison.OrdinalIgnoreCase))
        {
            config.AudioHash = Path.GetFileNameWithoutExtension(file);
        }

        AudioContainer? container;
        var audio = new RyoAudio(file, config);

        // Create an audio file container.
        if (config.AudioFilePath != null)
        {
            container = this.CreateOrGetContainer(ContainerType.File, config);
        }

        // Create audio data container.
        else if (config.AudioHash != null)
        {
            container = this.CreateOrGetContainer(ContainerType.Data, config);
        }

        // Default to cue container.
        else
        {
            // Use file name for cue name if none set.
            if (string.IsNullOrEmpty(config.CueName))
            {
                config.CueName = GetCueName(file);
            }

            config.Migrate();
            
            if (!RegisterCue(config)) return;
            container = this.CreateOrGetContainer(ContainerType.Cue, config);
        }

        container.AddAudio(audio);
    }

    private AudioContainer CreateOrGetContainer(ContainerType type, AudioConfig config)
    {
        // Create container.
        AudioContainer? container;

        switch (type)
        {
            case ContainerType.Cue:
                var cueKey = new CueKey(config.CueName!, config.AcbName!);
                container = RegisterOrGetSharedContainer(this._cueContainers, cueKey, new CueContainer(config.CueName!, config.AcbName!, config));
                break;
            case ContainerType.File:
                container = RegisterOrGetSharedContainer(this._fileContainers, config.AudioFilePath!, new FileContainer(config.AudioFilePath!, config));
                break;
            case ContainerType.Data:
                container = RegisterOrGetSharedContainer(this._dataContainers, config.AudioHash!, new DataContainer(config.AudioHash!, config));
                break;
            default: throw new Exception("Unknown container.");
        }

        return container;
    }

    private static AudioContainer RegisterOrGetSharedContainer<TKey>(Dictionary<TKey, List<AudioContainer>> containers, TKey key, AudioContainer container)
        where TKey : notnull
    {
        // Add a new container list with a single item.
        if (!containers.ContainsKey(key))
        {
            containers[key] = [container];
            return container;
        }

        // Use an existing shared container.
        if (container.SharedContainerId != null)
        {
            var sharedContainer = containers[key].FirstOrDefault(x => x.SharedContainerId == container.SharedContainerId);
            if (sharedContainer != null)
            {
                return sharedContainer;
            }
        }

        // Add the container to the list.
        containers[key].Add(container);
        return container;
    }

    public IContainer[] GetContainersByGroup(string groupId)
        => this._cueContainers.Values.SelectMany(x => x)
            .Concat(this._fileContainers.Values.SelectMany(x => x))
            .Concat(this._dataContainers.Values.SelectMany(x => x))
            .Where(x => x.GroupId != null && x.GroupId == groupId)
            .ToArray<IContainer>();

    public void AddAudioFile(string file)
        => this.AddAudioFile(file, null);

    public void AddAudioFolder(string dir, AudioConfig? preConfig = null)
    {
        //Log.Debug($"Adding audio folder: {dir}");

        // Audio config for folder items.
        var config = preConfig?.Clone() ?? new();

        // Apply config from a folder config file.
        var dirConfigFile = Path.Join(dir, "config.yaml");
        if (File.Exists(dirConfigFile) && ParseConfigFile(dirConfigFile) is { } dirConfig)
        {
            config.Apply(dirConfig);
        }

        var folderExt = Path.GetExtension(dir);

        // Folder likely contains items for a single item/file,
        // and thus items should be added to a single container.
        if (string.IsNullOrEmpty(folderExt) == false)
        {
            // Set shared container ID.
            config.SharedContainerId = Guid.NewGuid().ToString();
        }

        // Folder sets the ACB name for items.
        // Ex: A folder named "Voices.acb" indicates to use "Voice" for the ACB Name.
        if (folderExt.Equals(".acb", StringComparison.OrdinalIgnoreCase))
        {
            config.AcbName = Path.GetFileNameWithoutExtension(dir);
        }

        // Folder is a Cue Folder, all audio files get added to the same
        // audio container
        else if (folderExt.Equals(".cue", StringComparison.OrdinalIgnoreCase))
        {
            // Set the cue name.
            config.CueName = Path.GetFileNameWithoutExtension(dir);
        }

        foreach (var file in Directory.EnumerateFiles(dir))
        {
            var ext = Path.GetExtension(file).ToLower();
            if (ext == ".hca" || ext == ".adx" || ext == ".wav")
            {
                this.AddAudioFile(file, config);
            }
        }

        foreach (var folder in Directory.EnumerateDirectories(dir))
        {
            this.AddAudioFolder(folder, config);
        }
    }

    public bool TryGetCueContainer(string cueName, string acbName, [NotNullWhen(true)] out AudioContainer? container)
    {
        if (this._cueContainers.TryGetValue(new(cueName, acbName), out var containerList))
        {
            container = containerList.LastOrDefault(x => x.IsEnabled);
            return container != null;
        }

        container = null;
        return false;
    }

    public bool TryGetFileContainer(string filePath, [NotNullWhen(true)] out AudioContainer? container)
    {
        if (this._fileContainers.TryGetValue(filePath, out var containerList))
        {
            container = containerList.LastOrDefault(x => x.IsEnabled);
            return container != null;
        }

        container = null;
        return false;
    }

    public bool TryGetDataContainer(string audioHash, [NotNullWhen(true)] out AudioContainer? container)
    {
        if (this._dataContainers.TryGetValue(audioHash, out var containerList))
        {
            container = containerList.LastOrDefault(x => x.IsEnabled);
            return container != null;
        }

        container = null;
        return false;
    }

    private AudioConfig? ParseConfigFile(string configFile)
    {
        try
        {
            Log.Debug($"Loading audio config: {configFile}");
            var config = yamlDeserializer.Deserialize<AudioConfig>(File.ReadAllText(configFile));
            return config;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to parse audio config.\nFile: {configFile}");
            return null;
        }
    }

    private static CriAtomFormat GetAudioFormat(string file)
        => Path.GetExtension(file).ToLower() switch
        {
            ".hca" => CriAtomFormat.HCA,
            ".adx" => CriAtomFormat.ADX,
            ".wav" => CriAtomFormat.WAVE,
            _ => throw new Exception($"Unknown format: {file}"),
        };

    private static string GetCueName(string file)
    {
        var nameParts = Path.GetFileNameWithoutExtension(file).Split('_');
        if (nameParts.Length > 0 && int.TryParse(nameParts[0], out var bgmId))
        {
            return bgmId.ToString();
        }

        return Path.GetFileNameWithoutExtension(file);
    }

    private enum ContainerType
    {
        Cue,
        File,
        Data,
    }
}
