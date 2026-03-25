using LeadPipe.Domain;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.MySql.Settings;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Cli;

internal class Settings : IInfrastructureSettings, IDomainSettings, IMySqlSettings
{
    // IDwhSettings
    public string? SqlConnectionString1 { get; set; }
    public string? SqlConnectionString2 { get; set; }

    // Lab
    public string? LabId { get; set; }
    public string? LabSecret { get; set; }
    public string? LabName { get; set; }
    public string? LabBase { get; set; }
    public string? LabPlumbing { get; set; }
    public string? LabAuth { get; set; }
    public int LabConcurrentMax { get; set; }
    public Token? LabToken { get; set; }
    public string? LabAccept { get; set; }

    // Infrastructure
    public Ef? Ef { get; set; }
    public HttpClients? HttpClients { get; set; }

    // Leaf
    public string? LeafName { get; set; }
    public Token? LeafToken { get; set; }
    public string? LeafBase { get; set; }
    public string? LeafAcctUuid { get; set; }
    public string? LeafUuid { get; set; }
    public string? LeafThreadsEndpoint { get; set; }
    public string? LeafMessagesEndpoint { get; set; }
    public int LeafConcurrentMax { get; set; }
    public string? PlumbingConnectionString { get; set; }
    public string? Schema1ConnectionString { get; set; }
    public string? Schema2ConnectionString { get; set; }
    public string? Schema3ConnectionString { get; set; }
    public string? Schema1 { get; set; }
    public string? Schema2 { get; set; }
    public string? Schema3 { get; set; }
    public string? CornTableName { get; set; }
    public string? SandTableName { get; set; }
    public string? CustardTableName { get; set; }
    public string[]? CornSources { get; set; }
    public string? CornFilter { get; set; }

    // Yeller 
    public string? YellerGetterName { get; set; }
    public string? YellerReporterName { get; set; }
    public string? YellerSecret { get; set; }
    public Token? YellerToken { get; set; }
    public string? YellerBase { get; set; }
    public string? YellerId { get; set; }
    public string? YellerPrelimEndpoint1 { get; set; }
    public string? YellerPrelimEndpoint2 { get; set; }
    public int YellerConcurrentMax { get; set; }
    public string? YellerFinalEndpoint { get; set; }
    public string? YellerPrelimId { get; set; }
    public string? YellerCaliperSource1 { get; set; }
    public string? YellerCaliperSource2 { get; set; }
    public string? YellerJsonReporterLoc { get; set; }
    public string? YellerCsvReporterLoc { get; set; }
    public string[]? YellerBellerId { get; set; }
    public string? YellerSalt { get; set; }
    public string? YellerCornSource { get; set; }
    public string? YellerReportEndpoint { get; set; }
    public string? YellerActionSource { get; set; }
    public string? YellerPlumbingName { get; set; }
    public string? YellerCaliperName { get; set; }
    public string? YellerCornName { get; set; }

    // SyncState
    public double DefaultInterval { get; set; }
    public double DefaultAssociationInterval { get; set; }
    public double DefaultSourceInterval { get; set; }
    public double CalliInterval { get; set; }
    public double LabInterval { get; set; }
    public double LeafInterval { get; set; }
    public double LeasedInterval { get; set; }
    public double LibacionInterval { get; set; }
    public double PanInterval { get; set; }
    public double YellerInterval { get; set; }
    public double LatherInterval { get; set; }

    // Catman Settings
    public string? CatManClientName { get; set; }
    public string? CatToken { get; set; }
    public string? CatManDateFormat { get; set; }
    public string? CatBaseEndpoint { get; set; }
    public int CatAccountId { get; set; }
    public CatAccount? CatAccount { get; set; }
    public string? CatmanSecret { get; set; }
    public string? CatmanKey { get; set; }

    // Data
    public LocationPair? CalliLoc { get; set; }
    public LocationPair? LabLoc { get; set; }
    public LocationPair? LibacionLoc { get; set; }
    public LocationPair? LeasedLoc { get; set; }
    public LocationPair? PanLoc { get; set; }
    public LocationPair? LatherLoc { get; set; }
    public LocationPair? YellerLoc { get; set; }
}
