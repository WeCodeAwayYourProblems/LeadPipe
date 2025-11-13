using LeadPipe.Application.DataInterfaces.Entity;

namespace LeadPipe.Infrastructure.Entity;

internal class PanEntity : IPanEntity
{
    public string? Contents { get; set; }
    public DateTime Date { get; set; }
    public long PhoneNumber { get; set; }
    public long UnixDate { get; set; }
}
