namespace LeadPipe.Infrastructure.Entity;

internal class SubsPlumbingLink
{
    public long SubsId { get; set; }
    public required SubsEntity SubsEntity { get; set; }
    public long MatchingSubPhone { get; set; }
    public long PlumbingId { get; set; }
    public required PlumbingEntity PlumbingEntity { get; set; }
}
