using CsvHelper.Configuration.Attributes;

namespace LeadPipe.Infrastructure.Dto;

internal class LeasedDto
{
    [Name("Date Received")]
    public string? Date { get; set; }
    [Name("Date Completed")]
    public string? CompletionDate { get; set; }
    [Name("Phone Number")]
    public string? PhoneNumber { get; set; }
    [Name("BranchData")]
    public string? Branch { get; set; }
    [Name("Message Chain")]
    public string? Contents { get; set; }
    [Name("Was it a Lead?")]
    public string? Lead { get; set; }
}
