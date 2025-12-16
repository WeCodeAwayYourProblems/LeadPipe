namespace LeadPipe.Infrastructure.Settings;

public interface IInfrastructureSettings : IDwhSettings,
    ILabSettings,
    ILeafSettings,
    IYellerSettings
{
    string? CalliReportLoc { get; set; }
    string? CalliSourceLoc { get; set; }

    string? LibacionSourceLoc { get; set; }
    string? LibacionReportLoc { get; set; }

    string? LeasedSourceLoc { get; set; }
    string? LeasedReportLoc { get; set; }

    string? PanReportLoc { get; set; }
    string? PanSourceLoc { get; set; }
}
