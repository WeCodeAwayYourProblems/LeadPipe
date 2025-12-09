namespace LeadPipe.Infrastructure.Settings;

public interface IYellerSettings
{
    string? YellerId { get; set; }
    string? YellerName { get; set; }
    string? YellerSecret { get; set; }
    string? YellerToken { get; set; }
    string? YellerBase { get; set; }
}