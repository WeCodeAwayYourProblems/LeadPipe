namespace LeadPipe.Application.DataInterfaces.Dto;

public interface ILibacionDto
{
    string? Date { get; set; }
    string? Time { get; set; }
    string? PhoneNumber { get; set; }
    string? TimeZone { get; set; }
    string? Message { get; set; }
}
