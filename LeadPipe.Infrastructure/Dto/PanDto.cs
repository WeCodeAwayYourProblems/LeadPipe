using CsvHelper.Configuration.Attributes;
using LeadPipe.Application.DataInterfaces.Dto;

namespace LeadPipe.Infrastructure.Dto;

internal class PanDto : IPanDto
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
}
