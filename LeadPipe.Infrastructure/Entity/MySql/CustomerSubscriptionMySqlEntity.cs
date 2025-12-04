namespace LeadPipe.Infrastructure.Entity.MySql;

public class CustomerCallMySqlEntity
{
    public long Id { get; set; }

    public int CustomerId { get; set; }
    public long CallId { get; set; }

    // The phone number that matched between Call.contact_number_clean,
    // Customer.phone1, or Customer.phone2
    public long MatchingPhone { get; set; }

    // Navigation Properties
    public CustardMySqlEntity? Customer { get; set; }
    public CallMySqlEntity? Call { get; set; }
}

