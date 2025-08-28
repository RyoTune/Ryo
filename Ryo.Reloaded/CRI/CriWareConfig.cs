namespace Ryo.Reloaded.CRI;

public static class CriWareConfig
{
    public static int AcbNameOffset { get; private set; } = 0x98;
    
    public static int HcaDecodedEncryptKeyOffset { get; set; } = 0x120;
    
    public static void SetVersion(Version ver)
    {
        // From: Sonic Racing CrossWorlds
        // Name PTR shifted by 8.
        if (ver >= new Version("2.87.29"))
        {
            AcbNameOffset = 0x98 + 0x8;
            //HcaDecodedEncryptKeyOffset = 0xb0;
        }
        
        Log.Information($"CriWare Version: {ver}");
    }
}