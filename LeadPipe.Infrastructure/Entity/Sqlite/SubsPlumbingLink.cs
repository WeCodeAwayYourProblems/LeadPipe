namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class SubsPlumbingLink : IEntity
{
    public long Id { get; set; }

    public long SubsId { get; set; }
    public SubsEntity? SubsEntity { get; set; }

    public long PlumbingId { get; set; }
    public PlumbingEntity? PlumbingEntity { get; set; }

    public long MatchingSubPhone { get; set; }
}
