namespace LeadPipe.Domain.ValueObjects;

public record CornFormula(
    long Id, 
    PhoneNumber PhoneNumber, 
    DateTimeOffset Date, 
    string PayLoad, 
    string MetaData, 
    string Source,
    string? UtmSource,
    string? UtmMedium,
    string? UtmCampaign,
    string? UtmContent,
    string? UtmTerm
);