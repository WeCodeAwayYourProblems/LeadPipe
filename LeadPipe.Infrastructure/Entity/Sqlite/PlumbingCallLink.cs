namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class PlumbingCallLink : IEntity
{
    public long Id { get; set; }

    public long PlumbingId { get; set; }
    public PlumbingEntity? PlumbingEntity { get; set; }

    public long CallId { get; set; }
    public CallEntity? CallEntity { get; set; }
}
