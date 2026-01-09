namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class PlumbingCaliperLink : IEntity
{
    public long Id { get; set; }

    public long PlumbingId { get; set; }
    public PlumbingEntity? PlumbingEntity { get; set; }

    public long CaliperId { get; set; }
    public CaliperEntity? CaliperEntity { get; set; }
}
