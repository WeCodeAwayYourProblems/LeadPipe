namespace LeadPipe.Application.DataInterfaces.Dto;

public interface ICalliDto
{
    string? Date { get; set; }
    string? PestProblem { get; set; }
    long Phone { get; set; }
    string? Time { get; set; }
    string? TimeZone { get; set; }
}
