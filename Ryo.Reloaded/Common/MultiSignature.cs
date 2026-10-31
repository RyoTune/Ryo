using SharedScans.Interfaces;

namespace Ryo.Reloaded.Common;

public class MultiSignature<TFunction>
{
    public readonly WrapperContainer<TFunction> Function;
    // public TFunction? Wrapper;
    private readonly object Lock = new();
    private string[] Signatures;
    private int ScansCompleted;

    // Add support for multiple signature candidates to allow support for multiple game versions instead of *just* the
    // latest version. The selected function will be the first candidate to successfully match their signature to
    // some offset in the executable.
    // Note that if you are using both hardcoded signatures and Scan INI, the mod may complain that it can't find a
    // signature from the hardcoded list but does from Scan INI.
    public MultiSignature(ISharedScans scans, string[] Signatures)
    {
        this.Signatures = Signatures;
        // Check for a single pattern defined inside a Scan INI
        scans.AddScan(typeof(TFunction).Name, null);
        Function = scans.CreateWrapper<TFunction>(Mod.Name);
        
        // Check for multiple signature candidates defined in code
        foreach (var (Index, Candidate) in this.Signatures.Select((x, i) => (i, x)))
        {
            Project.Scans.AddScanHook($"{typeof(TFunction).Name}[{Index}]", Candidate, (result, hooks) =>
            {
                lock (Lock)
                {
                    ScansCompleted++;
                    if (Function.Wrapper == null)
                    {
                        scans.Broadcast<TFunction>(result);
                    }
                    Function.Wrapper ??= hooks.CreateWrapper<TFunction>(result, out _);
                }
            }, () =>
            {
                lock (Lock)
                {
                    ScansCompleted++;
                    if (Function.Wrapper == null && ScansCompleted == this.Signatures.Length)
                    {
                        Log.Error($"Failed to find a pattern for {typeof(TFunction).Name}.");
                    }
                    else
                    {
                        Log.Debug($"No matching pattern for {typeof(TFunction).Name}[{Index}].");
                    }
                }    
            });
        }
    }
}