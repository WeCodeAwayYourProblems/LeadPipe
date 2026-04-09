using LeadPipe.Infrastructure.Dto;

namespace LeadPipe.Infrastructure.Settings;

public interface IYellerSettings
{
    string? YellerId { get; set; }
    string? YellerGetterName { get; set; }
    string? YellerReporterName { get; set; }
    string? YellerSecret { get; set; }
    string? YellerBase { get; set; }
    string? YellerPrelimEndpoint1 { get; set; }
    string? YellerPrelimEndpoint2 { get; set; }
    int YellerConcurrentMax { get; set; }
    string? YellerFinalEndpoint { get; set; }
    string? YellerPrelimId { get; set; }
    string? YellerCaliperSource1 { get; set; }
    string? YellerCaliperSource2 { get; set; }
    string? YellerJsonReporterLoc { get; set; }
    string? YellerCsvReporterLoc { get; set; }
    string[]? YellerBellerId { get; set; }
    string? YellerSalt { get; set; }
    string? YellerCornSource { get; set; }
    string? YellerReportEndpoint { get; set; }
    string? YellerActionSource { get; set; }
    string? YellerPlumbingName { get; set; }
    string? YellerCaliperName { get; set; }
    string? YellerCornName { get; set; }
    string? YellerEventsEndpoint { get; set; }
    string? YellerRecursion { get; set; }
    string? YellerAuthUrl { get; set; }
    string? YellerOAuthName { get; set; }
}
