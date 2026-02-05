namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class PlumbingCaliperLink : IEntity
{
    public long Id { get; set; }

    public required long PlumbingId { get; set; }
    public PlumbingEntity? PlumbingEntity { get; set; }

    public required long CaliperId { get; set; }
    public CaliperEntity? CaliperEntity { get; set; }

    public required long MatchingPhone { get; set; }
    public required long UnixMatchDate { get; set; }
}
