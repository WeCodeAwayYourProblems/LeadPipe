namespace LeadPipe.Domain.ValueObjects;

public record Plumbing(PhoneNumber PhoneNumber, DateTimeOffset Date, string? Contents, Source Source);