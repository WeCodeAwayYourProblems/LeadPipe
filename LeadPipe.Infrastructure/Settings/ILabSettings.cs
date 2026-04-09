using LeadPipe.Infrastructure.Dto;

namespace LeadPipe.Infrastructure.Settings;

public interface ILabSettings
{
    string? LabId { get; set; }
    string? LabSecret { get; set; }
    string? LabName { get; set; }
    string? LabBase { get; set; }
    string? LabPlumbing { get; set; }
    string? LabAuth { get; set; }
    TokenDto? LabToken { get; set; }
    int LabConcurrentMax { get; set; }
    string? LabAccept { get; set; }
    string? LabAuthorizationUrl { get; set; }
    string? LabOAuthName { get; set; }
}
