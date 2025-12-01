namespace LeadPipe.Infrastructure.Entity.MySql;

public class CustomerCallMySqlEntity
{
    public long Id { get; set; }

    public long CallId { get; set; }
    public required CallMySqlEntity Call { get; set; }

    public int CustomerId { get; set; }
    public required CustardMySqlEntity Customer { get; set; }

    public long MatchedPhoneNumber { get; set; }  // phone1 or phone2
}
