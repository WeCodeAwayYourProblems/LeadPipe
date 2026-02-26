using LeadPipe.Infrastructure.Dto;

namespace LeadPipe.Infrastructure.Settings;

public interface IYellerSettings
{
    string? YellerId { get; set; }
    string? YellerGetterName { get; set; }
    string? YellerReporterName { get; set; }
    string? YellerSecret { get; set; }
    Token? YellerToken { get; set; }
    string? YellerBase { get; set; }
    string? YellerPrelimEndpoint1 { get; set; }
    string? YellerPrelimEndpoint2 { get; set; }
    int YellerConcurrentMax { get; set; }
    string? YellerFinalEndpoint { get; set; }
    string? YellerPrelimId { get; set; }
    string? YellerCaliperSource1 { get; set; }
    string? YellerCaliperSource2 { get; set; }
    string? YellerClientReporterLoc { get; set; }
    string[]? YellerBellerId { get; set; }
    string? YellerSalt { get; set; }
    string? YellerCornSource { get; set; }
    string? YellerReportEndpoint { get; set; }
    string? YellerActionSource { get; set; }
}
