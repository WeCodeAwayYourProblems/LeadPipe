namespace LeadPipe.Infrastructure.Settings;

public interface ICatManSettings
{
    string? CatManClientName { get; set; }
    string? CatToken { get; set; }
    string? CatManDateFormat { get; set; }
    string? CatBaseEndpoint { get; set; }
    string? CatmanSecret { get; set; }
    string? CatmanKey { get; set; }
    int CatAccountId { get; set; }
    CatAccount? CatAccount { get; set; }
}
public class CatAccount
{
    public CatAcctDetails? Fat { get; set; }
    public CatAcctDetails? Sandbox { get; set; }
    public CatAcctDetails? Natal { get; set; }
}
public record CatAcctDetails(int Id, string? Secret, string? Key);