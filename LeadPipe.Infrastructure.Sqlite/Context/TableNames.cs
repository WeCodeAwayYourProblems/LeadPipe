namespace LeadPipe.Infrastructure.Sqlite.Context;

internal class TableNames
{
    public const string SyncStateName = nameof(PlumbingContext.SyncState);
    public const string SyncStampName = nameof(PlumbingContext.SyncStamp);
    public const string CaliperEntitiesName = nameof(PlumbingContext.CaliperEntities);
    public const string CornEntitiesName = nameof(PlumbingContext.CornEntities);
    public const string PlumbingEntitiesName = nameof(PlumbingContext.PlumbingEntities);
    public const string CustardEntitiesName = nameof(PlumbingContext.CustardEntities);
    public const string SandEntitiesName = nameof(PlumbingContext.SandEntities);
    public const string CornCaliperLinksName = nameof(PlumbingContext.CornCaliperLinks);
    public const string CornPlumbingLinksName = nameof(PlumbingContext.CornPlumbingLinks);
    public const string PlumbingCaliperLinksName = nameof(PlumbingContext.PlumbingCaliperLinks);
    public const string CustardCaliperLinksName = nameof(PlumbingContext.CustardCaliperLinks);
    public const string CustardCornLinksName = nameof(PlumbingContext.CustardCornLinks);
    public const string CustardPlumbingLinksName = nameof(PlumbingContext.CustardPlumbingLinks);
    public const string SandCaliperLinksName = nameof(PlumbingContext.SandCaliperLinks);
    public const string SandCornLinksName = nameof(PlumbingContext.SandCornLinks);
    public const string SandPlumbingLinksName = nameof(PlumbingContext.SandPlumbingLinks);
}
