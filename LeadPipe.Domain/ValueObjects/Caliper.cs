namespace LeadPipe.Domain.ValueObjects;

public record Caliper(long Id, DateTimeOffset Date, PhoneNumber Number, TimeSpan Duration, string Note, string Source, bool Billable, string Location);
