using CsvHelper.Configuration.Attributes;
using LeadPipe.Application.DataInterfaces.Dto;

namespace LeadPipe.Infrastructure.Dto;

internal class CalliDto : ICalliDto
{
    [Name("Phone")]
    public long Phone { get; set; }
    [Name("Date")]
    public string? Date { get; set; }
    [Name("Time")]
    public string? Time { get; set; }
    [Name("Time Zone")]
    public string? TimeZone { get; set; }
    [Name("Pest Problem")]
    public string? PestProblem { get; set; }
}
