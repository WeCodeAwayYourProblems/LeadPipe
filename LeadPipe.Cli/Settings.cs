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
    public string? LabUri { get; set; }
    public string? LabPlumbing { get; set; }
    public string? LabAuth { get; set; }
    public int LabConcurrentMax { get; set; }
    public Token? LabToken { get; set; }
    public string? LabAccept { get; set; }

    // Infrastructure
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
    
    // Yeller 
    public string? YellerName { get; set; }
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
    public string? YellerClientReporterLoc { get; set; }
    public string[]? YellerBellerId { get; set; }
    public string? YellerSalt { get; set; }


    // Data
    public string? CalliSourceLoc { get; set; }
    public string? CalliReportLoc { get; set; }
    
    public string? LabReportLoc { get ; set ; }
    public string? LabSourceLoc { get ; set ; }

    public string? LeasedSourceLoc { get; set; }
    public string? LeasedReportLoc { get; set; }
    
    public string? LibacionSourceLoc { get; set; }
    public string? LibacionReportLoc { get; set; }
    
    public string? PanReportLoc { get; set; }
    public string? PanSourceLoc { get; set; }
}
