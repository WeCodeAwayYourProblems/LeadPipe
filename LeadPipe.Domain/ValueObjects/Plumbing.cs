namespace LeadPipe.Domain.ValueObjects;

public record Plumbing(long Id, PhoneNumber PhoneNumber, DateTimeOffset Date, string? Contents, string MetaData, Source Source);
