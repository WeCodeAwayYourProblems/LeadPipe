namespace LeadPipe.Application.Manager;

public record UpdateReportManagement
{
    public bool Update { get; }
    public bool Report { get; }
    public UpdateReportManagement(bool update, bool report)
    {
        if (!update && !report)
            throw new ArgumentException("At least one Update or Report must be true");

        Update = update;
        Report = report;
    }
}
