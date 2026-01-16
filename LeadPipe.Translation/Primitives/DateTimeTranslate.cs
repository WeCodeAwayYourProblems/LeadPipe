using System.Runtime.InteropServices;

namespace LeadPipe.Translation.Primitives;

internal class DateTimeTranslate : IDateTimeTranslate
{
    // Cache TimeZoneInfo objects for efficiency
    private static readonly Lazy<Dictionary<ETimeZone, TimeZoneInfo>> TimeZones =
        new(() => new Dictionary<ETimeZone, TimeZoneInfo>
        {
            [ETimeZone.Pacific] = FindTimeZone(
                windowsId: "Pacific Standard Time",
                ianaId: "America/Los_Angeles"),

            [ETimeZone.Mountain] = FindTimeZone(
                windowsId: "Mountain Standard Time",
                ianaId: "America/Denver"),

            [ETimeZone.Central] = FindTimeZone(
                windowsId: "Central Standard Time",
                ianaId: "America/Chicago"),

            [ETimeZone.Eastern] = FindTimeZone(
                windowsId: "Eastern Standard Time",
                ianaId: "America/New_York"),
        });

    private static TimeZoneInfo FindTimeZone(string windowsId, string ianaId)
    {
        try
        {
            // Prefer OS-native ID
            return TimeZoneInfo.FindSystemTimeZoneById(
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? windowsId
                    : ianaId);
        }
        catch (TimeZoneNotFoundException)
        {
            // Fallback to the other ID if OS detection is wrong / container mismatch
            try { return TimeZoneInfo.FindSystemTimeZoneById(windowsId); }
            catch { return TimeZoneInfo.FindSystemTimeZoneById(ianaId); }
        }
    }


    #region Public

    /// <summary>
    /// Convert a local DateTime in the given ETimeZone to UTC DateTimeOffset.
    /// Handles invalid and ambiguous times deterministically.
    /// </summary>
    public DateTimeOffset Convert(DateTime localTime, ETimeZone zone)
    {
        // 1) If the caller already gave us a UTC DateTime, return it exactly as UTC +00:00
        if (localTime.Kind == DateTimeKind.Utc)
        {
            // Use the ticks directly to preserve full fidelity
            return new DateTimeOffset(localTime, TimeSpan.Zero);
        }

        // 2) Lookup timezone (fallback to UTC)
        Dictionary<ETimeZone, TimeZoneInfo> zoneDict = TimeZones.Value;
        if (!zoneDict.TryGetValue(zone, out var tz))
            tz = TimeZoneInfo.Utc;

        // 3) Treat the incoming DateTime as a local time in the specified tz.
        //    Ensure it's "unspecified" so we don't accidentally treat it as system local.
        if (localTime.Kind != DateTimeKind.Unspecified)
            localTime = DateTime.SpecifyKind(localTime, DateTimeKind.Unspecified);

        // 4) Handle invalid local times (DST gaps)
        if (tz.IsInvalidTime(localTime))
            localTime = AdjustForwardToValid(localTime, tz);

        // 5) Handle ambiguous times deterministically (choose earlier offset policy)
        if (tz.IsAmbiguousTime(localTime))
        {
            TimeSpan chosenOffset = tz.GetAmbiguousTimeOffsets(localTime).Min();
            // create DTO with chosen offset and convert to UTC, then return explicit zero-offset DTO
            DateTimeOffset withOffset = new DateTimeOffset(localTime, chosenOffset);
            DateTime utcFromAmbiguous = withOffset.UtcDateTime;
            return new DateTimeOffset(utcFromAmbiguous, TimeSpan.Zero);
        }

        // 6) Normal conversion: get UTC DateTime explicitly and return a zero-offset DTO
        DateTime utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localTime, tz);
        return new DateTimeOffset(utcDateTime, TimeSpan.Zero);
    }


    /// <summary>
    /// Convert a local DateTime to UTC using out parameter.
    /// Returns false if conversion fails.
    /// </summary>
    public bool Convert(DateTime localTime, ETimeZone zone, out DateTimeOffset result)
    {
        try
        {
            result = Convert(localTime, zone);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Convert a DateTime to DateTimeOffset with the specified offset.
    /// </summary>
    public DateTimeOffset Convert(DateTime date, TimeSpan offset) => new(date, offset);

    #endregion

    #region Private Helpers

    /// <summary>
    /// Adjusts an invalid local time forward until it becomes valid.
    /// </summary>
    private static DateTime AdjustForwardToValid(DateTime date, TimeZoneInfo tz)
    {
        DateTime adjusted = date;
        while (tz.IsInvalidTime(adjusted))
            adjusted = adjusted.AddMinutes(1);
        return adjusted;
    }

    #endregion
}
