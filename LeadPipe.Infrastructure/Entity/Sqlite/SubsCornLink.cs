namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class SubsCornLink
{
    public long Id { get; set; }

    public long SubsId { get; set; }
    public SubsEntity SubsEntity { get; set; } = default!;

    public long CornId { get; set; }
    public CornEntity CornEntity { get; set; } = default!;

    public long MatchingPhone { get; set; }
}
