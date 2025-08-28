// ReSharper disable InconsistentNaming

using System.Runtime.InteropServices;
using Ryo.Reloaded.Configuration;
using SharedScans.Interfaces;

namespace Ryo.Reloaded.CRI.CriWare;

public unsafe class CriWareHooks
{
    private delegate nint HCADecoder_DecodeHeader(HcaDecoded* hca, Hca* param_2, nint param_3, byte* param_4, nint param_5, nint* param_6);
    
    private readonly HookContainer<HCADecoder_DecodeHeader> _hcaDecodeHeader;
    private bool _encryptedHcaEnabled;

    public CriWareHooks(ISharedScans scans, string game)
    {
        var patterns = CriWarePatterns.GetGamePatterns(game);
        
        scans.AddScan<HCADecoder_DecodeHeader>(patterns.HCADecoder_DecodeHeader);
        _hcaDecodeHeader = scans.CreateHook<HCADecoder_DecodeHeader>(HcaDecodeHeader, Mod.NAME);
    }

    public void SetConfig(Config config) => _encryptedHcaEnabled = config.UnencryptedHcaEnabled;

    private nint HcaDecodeHeader(HcaDecoded* hcaOut, Hca* hcaSrc, nint param_3, byte* param_4, nint param_5, nint* param_6)
    {
        // Remove encryption key from output if source is unencrypted (checks for readable HCA sig).
        if (_encryptedHcaEnabled && hcaSrc->Signature == 0x414348)
        {
            *(nint*)((nint)hcaOut + CriWareConfig.HcaDecodedEncryptKeyOffset) = 0; 
                
            Log.Verbose("Removed encryption before HCA playback.");
        }
        
        return _hcaDecodeHeader.Hook!.OriginalFunction(hcaOut, hcaSrc, param_3, param_4, param_5, param_6);
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct HcaDecoded
    {
        [FieldOffset(0x120)]
        public nint EncryptionKey;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    private struct Hca
    {
        public int Signature;
    }
}