using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;

namespace LeadPipe.Infrastructure.Entity;

public enum AttributionSource
{
    Plumbing,
    Corn,
    Caliper
}

public sealed class AttributionResult
{
    /// <summary>
    /// The phone number that was matched and grouped by.
    /// Useful for auditing and debugging.
    /// </summary>
    public required long MatchingPhone { get; init; }

    /// <summary>
    /// The resolved first-touch date (UnixMatchDate from the winning link).
    /// This becomes ReportYeller.event_time.
    /// </summary>
    public required long FirstTouchUnixDate { get; init; }

    /// <summary>
    /// Which source won first-touch (Plumbing, Corn, Caliper).
    /// Important for reporting, auditing, tie-break logic.
    /// </summary>
    public required AttributionSource Source { get; init; }

    /// <summary>
    /// The entity itself
    /// </summary>
    public required IPhoneDateIdEntity Entity { get; init; }

    /// <summary>
    /// The Custard that is being attributed.
    /// Must not be null.
    /// </summary>
    public required CustardEntity Custard { get; init; }

    /// <summary>
    /// The single attributable Sand.
    /// Guaranteed:
    /// - Completed == true
    /// - First chronological after first-touch
    /// - Valid according to all attribution rules
    /// </summary>
    public required SandEntity Sand { get; init; }

    /// <summary>
    /// Convenience projection for value.
    /// Keeps translation simple and prevents sand traversal.
    /// </summary>
    public decimal Value => Sand.Value;
}
