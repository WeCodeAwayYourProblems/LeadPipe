namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class CustardCornLink : IEntity
{
    public long Id { get; set; }

    public long CustardId { get; set; }
    public CustardEntity Custard { get; set; } = default!;

    public long CornId { get; set; }
    public CornEntity Corn { get; set; } = default!;

    public long MatchingPhone { get; set; }
}
