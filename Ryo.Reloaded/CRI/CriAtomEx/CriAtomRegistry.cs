using Ryo.Definitions.Structs;
using Ryo.Interfaces;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Ryo.Reloaded.CRI.CriAtomEx;

internal class CriAtomRegistry : ICriAtomRegistry
{
    private static readonly ConcurrentDictionary<nint, Player> players = new();
    private static readonly ConcurrentDictionary<nint, Acb> acbs = new();
    private static readonly ConcurrentDictionary<nint, Awb> awbs = new();
    private static readonly ConcurrentDictionary<nint, AudioData> audioDatas = new();

    public static Player RegisterPlayer(nint playerHn)
    {
        var player = new Player(players.Count, playerHn);
        players[player.Handle] = player;
        Log.Debug($"Registered Player || ID: {player.Id} || Handle: {player.Handle:X}");
        return player;
    }

    public unsafe static Acb RegisterAcb(AcbHn* acbHn)
    {
        var namePtr = *(nint*)((nint)acbHn->acb + CriWareConfig.AcbNameOffset);
        var name = Marshal.PtrToStringAnsi(namePtr)!;
        var acb = new Acb(name, (nint)acbHn);
        acbs[acb.Handle] = acb;
        Log.Debug($"Registered ACB || Name: {acb.Name} || Handle: {acb.Handle:X}");
        return acb;
    }

    public unsafe static Awb RegisterAwb(string path, nint handle)
    {
        var awb = new Awb(path, handle);
        awbs[awb.Handle] = awb;
        Log.Debug($"Registered AWB || Path: {awb.Path} || Handle: {awb.Handle:X}");
        return awb;
    }

    public Awb? GetAwbByHn(nint awbHn)
    {
        if (awbs.TryGetValue(awbHn, out var awb))
        {
            return awb;
        }

        Log.Debug($"Unknown AWB Hn: {awbHn:X}");
        return null;
    }

    public Awb? GetAwbByPath(string awbPath)
    {
        var existingAwb = awbs.Values.FirstOrDefault(x => x.Path.Equals(awbPath, StringComparison.OrdinalIgnoreCase));
        if (existingAwb != null)
        {
            return existingAwb;
        }

        Log.Debug($"Unknown AWB: {awbPath}");
        return null;
    }

    public Acb? GetAcbByHn(nint acbHn)
    {
        if (acbs.TryGetValue(acbHn, out var acb))
        {
            return acb;
        }

        Log.Debug($"Unknown ACB Hn: {acbHn:X}");
        return null;
    }

    public Acb? GetAcbByName(string acbName)
    {
        var existingAcb = acbs.Values.FirstOrDefault(x => x.Name.Equals(acbName, StringComparison.OrdinalIgnoreCase));
        if (existingAcb != null)
        {
            return existingAcb;
        }

        Log.Debug($"Unknown ACB: {acbName}");
        return null;
    }

    public Player? GetPlayerByHn(nint playerHn)
    {
        if (players.TryGetValue(playerHn, out var player))
        {
            return player;
        }

        Log.Debug($"Unknown Player Hn: {playerHn:X}");
        return null;
    }

    public Player? GetPlayerById(int playerId)
    {
        var player = players.Values.FirstOrDefault(x => x.Id == playerId);
        if (player == null)
        {
            Log.Debug($"Unknown Player ID: {playerId}");
        }

        return player;
    }

    public void RegisterAudioData(nint address, string name)
    {
        if (audioDatas.TryGetValue(address, out var prevData) && prevData.Name == name)
        {
            return;
        }

        var audioData = new AudioData(name, address);
        audioDatas[address] = audioData;
        Log.Debug($"Registered Audio Data || Name: {name} || Address: {address:X}");
    }

    public AudioData? GetAudioDataByAddress(nint address)
    {
        if (audioDatas.TryGetValue(address, out var audioData))
        {
            return audioData;
        }

        Log.Debug($"Unknown Audio Data: {address:X}");
        return null;
    }

    public AudioData? GetAudioDataByName(string name)
    {
        var audioData = audioDatas.Values.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (audioData == null)
        {
            Log.Debug($"Unknown Audio Data: {name}");
        }

        return audioData;
    }
}
