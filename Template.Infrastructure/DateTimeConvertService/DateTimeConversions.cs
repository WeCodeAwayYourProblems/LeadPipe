namespace Template.Infrastructure.DateTimeConvertService;

internal static class DateTimeConversions
{
    #region Public
    public static DateTimeOffset ConvertLocalToDTOffset(DateTime localTime, TimeZoneEnum zone)
    {
        string timeZoneName = ConvertTimeZoneToTimeZoneId(zone);

        // Find the TimeZoneInfo for the specified time zone name
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneName);

        return ConvertLocalToDTOffset(localTime, timeZone);
    }

    public static bool ConvertLocalToDTOffset(DateTime localTime, TimeZoneEnum zone, out DateTimeOffset result)
    {
        // Find the TimeZoneInfo for the specified time zone name
        bool returnVal;
        string timeZoneName = ConvertTimeZoneToTimeZoneId(zone);
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneName);
        try
        {
            result = ConvertLocalToDTOffset(localTime, timeZone);
            returnVal = true;
        }
        finally { }
        return returnVal;
    }

    public static DateTimeOffset ConvertLocalToDTOffset(DateTime date, TimeSpan offset) => new(date, offset);
    #endregion

    #region Internal
    private static TimeZoneEnum StringToZone(string zoneStr) => zoneStr.ToLower().Split(" ")[0] switch
    {
        "pacific" => TimeZoneEnum.Pacific,
        "mountain" => TimeZoneEnum.Mountain,
        "central" => TimeZoneEnum.Central,
        "eastern" or "east" => TimeZoneEnum.Eastern,
        _ => TimeZoneEnum.Mountain
    };

    private static string ConvertTimeZoneToTimeZoneId(TimeZoneEnum zone)
        => $"{zone} Standard Time";

    private static DateTimeOffset ConvertLocalToDTOffset(DateTime date, TimeZoneInfo timeZone)
    {
        TimeSpan fourHours = new(4, 0, 0);

        // Check to ensure the time is valid
        bool isInvalid = timeZone.IsInvalidTime(date);
        if (isInvalid)
            date += fourHours;

        // Convert from local to utc time
        DateTimeOffset result = TimeZoneInfo.ConvertTimeToUtc(date, timeZone);

        // Correct time if needed
        if (isInvalid)
            result -= fourHours;
        return result;
    }

    private static DateTimeOffset DLSConversion(DateTime date, TimeZoneEnum zone)
    {
        TimeSpan oneHour = TimeSpan.FromHours(1);
        TimeSpan timeZone = ConvertToTimeSpan(zone);

        // Determine whether the given local time falls within DLS time, and adjust the TimeSpan offset accordingly
        Dictionary<int, DateTime[]> dlsDates = DLSDates(date.Year);
        DateTime dlsStart = dlsDates[date.Year][0];
        DateTime dlsEnd = dlsDates[date.Year][1];
        TimeSpan finalZone = date < dlsStart || date > dlsEnd ? timeZone : timeZone + oneHour;

        // Revert the original date to the prime meridian by zoning it to the offset
        DateTime returnDate = date - finalZone;

        return new(returnDate, finalZone);
    }
    private static DateTimeOffset DLSConversion(DateTime date, TimeZoneEnum zone, out TimeSpan finalZone)
    {
        TimeSpan oneHour = TimeSpan.FromHours(1);
        TimeSpan timeZone = ConvertToTimeSpan(zone);

        // Determine whether the given local time falls within DLS time, and adjust the TimeSpan offset accordingly
        Dictionary<int, DateTime[]> dlsDates = DLSDates(date.Year);
        DateTime dlsStart = dlsDates[date.Year][0];
        DateTime dlsEnd = dlsDates[date.Year][1];
        finalZone = date < dlsStart || date > dlsEnd ? timeZone : timeZone + oneHour;

        return DLSConversion(date, zone);
    }
    private static TimeSpan ConvertToTimeSpan(TimeZoneEnum zone) =>
        zone switch
        {
            TimeZoneEnum.Pacific => TimeSpan.FromHours(-8),
            TimeZoneEnum.Mountain => TimeSpan.FromHours(-7),
            TimeZoneEnum.Central => TimeSpan.FromHours(-6),
            TimeZoneEnum.Eastern => TimeSpan.FromHours(-5),
            _ => throw new ArgumentException($"The {nameof(DLSConversion)} method only accepts the following time zones from the lower 48 states of the US: {nameof(TimeZoneEnum.Pacific)}, {nameof(TimeZoneEnum.Mountain)}, {nameof(TimeZoneEnum.Central)}, {nameof(TimeZoneEnum.Eastern)}.")
        };
    #endregion

    #region Private
    private static DateTime Sunday(int year, int month, int hour, int whichSunday)
    {
        if (whichSunday > 5)
            whichSunday %= 5;
        DateTime start = new(year, month, 1, hour, 0, 0);
        for (var i = 0; i < whichSunday; i++)
            start = FindNext(DayOfWeek.Sunday, start.DayOfWeek == DayOfWeek.Sunday ? start : start.AddDays(1));
        return start;
    }
    private static DateTime FindNext(DayOfWeek dayOfWeek, DateTime after)
    {
        DateTime day = after;
        while (day.DayOfWeek != dayOfWeek) day = day.AddDays(1);
        return day;
    }
    private static Dictionary<int, DateTime[]> DLSDates(int year)
        => new() { { year, [Sunday(year, 03, 02, 2), Sunday(year, 11, 2, 1)] } };
    #endregion
}
