namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class SubsCallLink : IEntity
{
    public long Id { get; set; }
    public long SubsId { get; set; }
    public SubsEntity? SubsEntity { get; set; }

    public long CallId { get; set; }
    public CallEntity? CallEntity { get; set; }

    public long MatchingNumber { get; set; }
}
