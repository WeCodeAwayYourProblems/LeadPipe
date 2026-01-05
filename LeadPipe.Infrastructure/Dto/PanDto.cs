using CsvHelper.Configuration.Attributes;

namespace LeadPipe.Infrastructure.Dto;

public class PanDto
{
    [Name("Name")]
    public string? Name { get; set; }
    [Name("Customer #")]
    public string? Number { get; set; }
    [Name("Date")]
    public string? Date { get; set; }
    [Name("Time")]
    public string? Time { get; set; }
    [Name("FormCustomFields")]
    public string? Content { get; set; }
    [Name("Email")]
    public string? Metadata { get; set; }
}
