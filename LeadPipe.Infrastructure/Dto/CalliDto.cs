using CsvHelper.Configuration.Attributes;

namespace LeadPipe.Infrastructure.Dto;

public class CalliDto
{
    [Name("Phone")]
    public long Phone { get; set; }
    [Name("Date")]
    public string? Date { get; set; }
    [Name("Time")]
    public string? Time { get; set; }
    [Name("Time Zone")]
    public string? TimeZone { get; set; }
    [Name("Pest Problem/Message Content")]
    public string? PestProblem { get; set; }
}
