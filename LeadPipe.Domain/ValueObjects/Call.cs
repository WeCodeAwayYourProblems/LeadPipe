namespace LeadPipe.Domain.ValueObjects;

public record Call(DateTimeOffset Date, PhoneNumber Number, TimeSpan Duration, string Note, string Source, bool Billable);
