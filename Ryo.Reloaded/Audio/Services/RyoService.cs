using Ryo.Interfaces;
using Ryo.Reloaded.Audio.Models;
using Ryo.Reloaded.Audio.Models.Containers;
using SharedScans.Interfaces;
using static Ryo.Definitions.Functions.CriAtomExFunctions;

namespace Ryo.Reloaded.Audio.Services;

internal unsafe class RyoService
{
    private readonly ICriAtomEx criAtomEx;
    private readonly ICriAtomRegistry criAtomRegistry;
    private readonly Dictionary<int, float> modifiedCategories = [];
    
    private readonly HookContainer<criAtomExPlayer_SetVolume> setVolume;
    private readonly Dictionary<nint, float> currPlayerVolumes = [];
    private readonly Dictionary<nint, float> currPlayerModVolumes = [];
    private readonly HashSet<nint> modifiedPlayers = [];

    private bool setFileSupported;
    private bool setDataSupported;
    private bool resetParamsSupported;

    public RyoService(ICriAtomEx criAtomEx, ICriAtomRegistry criAtomRegistry, ISharedScans scans)
    {
        this.criAtomEx = criAtomEx;
        this.criAtomRegistry = criAtomRegistry;

        scans.CreateListener<criAtomExPlayer_SetFile>(_ => setFileSupported = true);
        scans.CreateListener<criAtomExPlayer_SetData>(_ => setDataSupported = true);
        scans.CreateListener<criAtomExPlayer_ResetParameters>(_ => resetParamsSupported = true);
        
        this.setVolume = scans.CreateHook<criAtomExPlayer_SetVolume>(this.CriAtomExPlayer_SetVolume, Mod.Name);
    }

    public void SetAudio(Player player, AudioContainer container, int[]? categories)
    {
        var currentPlayer = player;

        var manualStart = false;
        if (container.PlayerId != -1
            && currentPlayer.Id != container.PlayerId
            && this.criAtomRegistry.GetPlayerById(container.PlayerId) is Player newPlayer)
        {
            currentPlayer = newPlayer;
            manualStart = true;
        }

        var newAudio = container.GetAudio();

        if (this.setFileSupported)
        {
            this.criAtomEx.Player_SetFile(currentPlayer.Handle, IntPtr.Zero, (byte*)newAudio.FilePath.AsPointerAnsi(true));
        }
        else if (this.setDataSupported)
        {
            var audioData = AudioCache.GetAudioData(newAudio.FilePath);
            this.criAtomEx.Player_SetData(currentPlayer.Handle, (byte*)audioData.Address, audioData.Size);
        }
        else
        {
            Log.Error($"{nameof(SetAudio)} || No supported method for playing new audio.");
        }

        this.SetAudioVolume(currentPlayer, newAudio, categories);

        this.criAtomEx.Player_SetFormat(currentPlayer.Handle, newAudio.Format);
        this.criAtomEx.Player_SetSamplingRate(currentPlayer.Handle, newAudio.SampleRate);
        this.criAtomEx.Player_SetNumChannels(currentPlayer.Handle, newAudio.NumChannels);

        // Apply categories.
        if (categories?.Length > 0)
        {
            foreach (var id in categories)
            {
                this.criAtomEx.Player_SetCategoryById(player.Handle, (uint)id);
            }
        }

        if (manualStart)
        {
            this.criAtomEx.Player_Start(currentPlayer.Handle);
            Log.Debug($"Manually started player with ID: {currentPlayer.Id}");
        }

        Log.Debug($"Redirected {container.Name}\nFile: {newAudio.FilePath}");
    }
    
    private void CriAtomExPlayer_SetVolume(nint playerHn, float volume)
    {
        // If player has modded volume, adjust new volume to it.
        if (this.currPlayerModVolumes.TryGetValue(playerHn, out var modVol))
        {
            var newVol = volume * modVol;
            this.setVolume.Hook!.OriginalFunction(playerHn, newVol);
        }
        else
        {
            this.currPlayerVolumes[playerHn] = volume;
            this.setVolume.Hook!.OriginalFunction(playerHn, volume);
        }
    }

    private void SetAudioVolume(Player player, RyoAudio audio, int[]? categories)
    {
        if (audio.Volume < 0)
        {
            Log.Verbose("No custom volume set for file.");
            return;
        }

        // Set volume by player
        if (audio.UsePlayerVolume)
        {
            // Set audio volume relative to player's current volume.
            // This should allow for new audio to respect a game's volume setting
            // if they're set on the player itself, such as in Digimon Story: Time Stranger.
            if (this.currPlayerVolumes.TryGetValue(player.Handle, out var currVol))
            {
                // Save current audio's volume setting to player.
                this.currPlayerModVolumes[player.Handle] = audio.Volume;
                
                // Bypass hook, and set the audio's volume relative to the player's current volume.
                var newVol = currVol * audio.Volume;
                this.setVolume.Hook!.OriginalFunction(player.Handle, newVol);
                Log.Debug($"Modified player volume relative to original. Player ID: {player.Id} || Player: {currVol} || Audio: {audio.Volume} || Final: {newVol}");
            }
            else
            {
                this.criAtomEx.Player_SetVolume(player.Handle, audio.Volume);
                this.modifiedPlayers.Add(player.Handle);
                Log.Debug($"Modified player volume. Player ID: {player.Id} || Volume: {audio.Volume}");
            }
        }

        // Set volume by category.
        else if (categories?.Length > 0)
        {
            // Use first category for setting custom volume.
            int volumeCategory = audio.VolumeCategoryId != -1 ? audio.VolumeCategoryId : categories[0];

            // Save original category volume.
            if (!this.modifiedCategories.ContainsKey(volumeCategory))
            {
                var currentVolume = this.criAtomEx.Category_GetVolumeById((uint)volumeCategory);
                this.modifiedCategories[volumeCategory] = currentVolume;
            }

            this.criAtomEx.Category_SetVolumeById((uint)volumeCategory, audio.Volume);
            Log.Debug($"Modified volume. Category ID: {volumeCategory} || Volume: {audio.Volume}");
        }
    }

    public void ResetCustomVolumes(Player player, IEnumerable<int> categoryIds)
    {
        foreach (var id in categoryIds)
        {
            if (this.modifiedCategories.TryGetValue(id, out var ogVolume))
            {
                this.criAtomEx.Category_SetVolumeById((uint)id, ogVolume);
                this.modifiedCategories.Remove(id);
                Log.Debug($"Reset volume for Category ID: {id}");
            }
        }

        if (resetParamsSupported && this.modifiedPlayers.Contains(player.Handle))
        {
            this.criAtomEx.Player_ResetParameters(player.Handle);
            Log.Debug($"Reset volume for Player ID: {player.Id}");
        }

        // Clear player mod audio volume.
        if (this.currPlayerModVolumes.ContainsKey(player.Handle))
        {
            this.currPlayerModVolumes.Remove(player.Handle);
        }
    }
}
