namespace LeadPipe.Domain.ValueObjects;

public record Sandwich
(
    long SandId,
    long CustardId,
    Custard Custard,
    DateTimeOffset Date,
    DateOnly? DateAddedDate,
    DateTimeOffset? DateCancelled,
    bool Active,
    bool Complete,
    string? Type,
    decimal Value,
    int Seller,
    int Seller2,
    int Seller3,
    string Offerman
);
