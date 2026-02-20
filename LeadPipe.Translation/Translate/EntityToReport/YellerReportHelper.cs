using System.Security.Cryptography;
using System.Text;

namespace LeadPipe.Translation.Translate.EntityToReport;

public sealed class YellerReportHelper
{
    public const string Currency = "USD";
    public const string Country = "us";
    public static string HashSha256(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hash = SHA256.HashData(bytes);

        string result = Convert.ToHexString(hash);
        return result;
    }
}
