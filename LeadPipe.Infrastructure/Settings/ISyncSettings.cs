namespace LeadPipe.Infrastructure.Settings;

public interface ISyncSettings
{
    double DefaultInterval { get; set; }
    double DefaultSourceInterval { get; set; }
    double CalliInterval { get; set; }
    double LabInterval {get;set;}
    double LeafInterval {get;set;}
    double LeasedInterval {get;set;}
    double LibacionInterval {get;set;}
    double PanInterval {get;set;}
    double YellerInterval {get;set;}
}