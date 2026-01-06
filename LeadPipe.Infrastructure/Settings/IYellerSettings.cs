using LeadPipe.Infrastructure.Dto;

namespace LeadPipe.Infrastructure.Settings;

public interface IYellerSettings
{
    string? YellerId { get; set; }
    string? YellerName { get; set; }
    string? YellerSecret { get; set; }
    Token? YellerToken { get; set; }
    string? YellerBase { get; set; }
    string? YellerPrelimEndpoint { get; set; }
    int YellerConcurrentMax { get; set; }
    string? YellerFinalEndpoint { get; set; }
    string? YellerPrelimId { get; set; }
    string? YellerCallSource1 { get; set; }
    string? YellerCallSource2 { get; set; }
}