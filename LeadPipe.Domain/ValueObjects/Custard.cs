namespace LeadPipe.Domain.ValueObjects;

public record Custard
(
    long Id,
    bool Status,
    PhoneNumber Phone1,
    PhoneNumber Phone2,
    DateTimeOffset Date,
    DateTimeOffset DateCancelled
);
