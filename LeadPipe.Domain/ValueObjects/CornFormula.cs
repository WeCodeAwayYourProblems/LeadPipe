namespace LeadPipe.Domain.ValueObjects;

public record CornFormula(long Id, PhoneNumber PhoneNumber, DateTimeOffset Date, string PayLoad, string MetaData, Source Source);