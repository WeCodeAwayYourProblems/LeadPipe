namespace LeadPipe.Domain.ValueObjects;
public sealed record SyncKey
{
    public string Value { get; }

    private SyncKey(string value) => Value = value;

    // Predefined keys
    public static readonly SyncKey Caliper = new(nameof(Caliper).ToLowerInvariant());
    public static readonly SyncKey CornFormula = new(nameof(CornFormula).ToLowerInvariant());
    public static readonly SyncKey Custard = new(nameof(Custard).ToLowerInvariant());
    public static readonly SyncKey Plumbing = new(nameof(Plumbing).ToLowerInvariant());
    public static readonly SyncKey Sandwich = new(nameof(Sandwich).ToLowerInvariant());
    public static readonly SyncKey Associate = new("association");

    // All predefined keys for TryParse and iteration
    private static readonly List<SyncKey> AllKeys = 
    [
        Caliper, 
        CornFormula, 
        Custard, 
        Plumbing, 
        Sandwich
    ];

    /// <summary>
    /// Tries to parse a string into a known SyncKey.
    /// Returns true if successful, false otherwise.
    /// </summary>
    public static bool TryParse(string value, out SyncKey key)
    {
        var match = AllKeys.FirstOrDefault(k => k.Value.Equals(value, StringComparison.OrdinalIgnoreCase));
        if (match is not null)
        {
            key = match;
            return true;
        }

        key = null!; // force null-forgiving operator, safe because TryParse returns false
        return false;
    }
    
    public override string ToString() => Value;

}
