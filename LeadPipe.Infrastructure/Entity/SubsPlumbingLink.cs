namespace LeadPipe.Infrastructure.Entity;

public class SubsPlumbingLink : IEntity
{
    public long Id { get; set; }

    public long SubsId { get; set; }
    public required SubsEntity SubsEntity { get; set; }

    public long PlumbingId { get; set; }
    public required PlumbingEntity PlumbingEntity { get; set; }

    public long MatchingSubPhone { get; set; }
}