namespace LeadPipe.Domain.ValueObjects;

public class Sandwich
{
    public long SubscriptionId { get; set; }
    public long CustomerId { get; set; }
    public DateTimeOffset Date { get; set; }
    public DateTimeOffset SubDate { get; set; }
    public required PhoneNumber Number { get; set; }
    public required PhoneNumber Number2 { get; set; }
    public DateTimeOffset CancelDate { get; set; }
    public DateTimeOffset SubCancelDate { get; set; }
    public bool Active { get; set; }
    public bool SubActive { get; set; }
    public bool Complete { get; set; }
    public string? Type { get; set; }
    public decimal Value { get; set; }
    public string? Seller { get; set; }
    public string? Seller2 { get; set; }
    public string? Seller3 { get; set; }
}
