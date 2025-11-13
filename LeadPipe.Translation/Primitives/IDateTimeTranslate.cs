namespace LeadPipe.Translation.Primitives;

public interface IDateTimeTranslate
{
    DateTimeOffset Convert(DateTime date, TimeSpan offset);
    DateTimeOffset Convert(DateTime localTime, ETimeZone zone);
    bool Convert(DateTime localTime, ETimeZone zone, out DateTimeOffset result);
}
public enum ETimeZone
{
    Pacific,
    Mountain,
    Central,
    Eastern
}
