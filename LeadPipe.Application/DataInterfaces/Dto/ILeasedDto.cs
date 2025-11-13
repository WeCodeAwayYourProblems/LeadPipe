using CsvHelper.Configuration.Attributes;

namespace LeadPipe.Application.DataInterfaces.Dto;

public interface ILeasedDto
{
    string? Date { get; set; }
    string? CompletionDate { get; set; }
    string? PhoneNumber { get; set; }
    string? Branch { get; set; }
    string? Contents { get; set; }
    string? Lead { get; set; }
}
