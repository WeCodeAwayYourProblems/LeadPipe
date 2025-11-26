namespace LeadPipe.Infrastructure.Entity;

public class SubsCallLink : IEntity
{
    public long Id { get; set; }
    public long SubsId { get; set; }
    public required SubsEntity SubsEntity { get; set; }

    public long CallId { get; set; }
    public required CallEntity CallEntity { get; set; }

    public long MatchingNumber { get; set; }
}
