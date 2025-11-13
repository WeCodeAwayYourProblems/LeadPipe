using CsvHelper.Configuration.Attributes;
using LeadPipe.Application.DataInterfaces.Dto;

namespace LeadPipe.Infrastructure.Dto;

internal class LibacionDto : ILibacionDto
{
    [Name("Date")]
    public string? Date { get; set; }
    [Name("Time")]
    public string? Time { get; set; }
    [Name("Phone Number")]
    public string? PhoneNumber { get; set; }
    [Name("Time Zone")]
    public string? TimeZone { get; set; }
    [Name("Message")]
    public string? Message { get; set; }
    [Name("Commercial?")]
    public bool Commercial { get; set; }
}
