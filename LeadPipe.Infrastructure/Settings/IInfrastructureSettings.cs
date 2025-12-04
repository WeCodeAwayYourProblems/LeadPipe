namespace LeadPipe.Infrastructure.Settings;

public interface IInfrastructureSettings : IDwhSettings, ILeafSettings, ILabSettings
{
    string? CalliLocation { get; set; }
    string? LeasedLocation { get; set; }
    string? LibacionLocation { get; set; }
    string? PanLocation { get; set; }

}
