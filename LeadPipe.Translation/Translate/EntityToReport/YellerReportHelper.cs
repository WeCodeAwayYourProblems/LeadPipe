using System.Security.Cryptography;
using System.Text;

namespace LeadPipe.Translation.Translate.EntityToReport;

internal sealed class YellerReportHelper
{
    internal const string Currency = "USD";
    internal const string Country = "us";
    internal static string HashSha256(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hash = SHA256.HashData(bytes);

        string result = Convert.ToHexString(hash);
        return result;
    }
}
