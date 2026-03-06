using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace LeadPipe.Cli;

public static class OnePasswordConfigurationExtensions
{
    public static IConfigurationBuilder AddOnePasswordDocumentJson(
        this IConfigurationBuilder builder,
        string documentTitle,
        string? vault = null,
        bool required = true)
    {
        try
        {
            // Build document get command
            StringBuilder args = new($"document get \"{documentTitle}\"");
            if (!string.IsNullOrEmpty(vault))
                args.Append($" --vault \"{vault}\"");

            var psi = new ProcessStartInfo
            {
                FileName = "op",
                Arguments = args.ToString(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start 'op' process.");
            using var ms = new MemoryStream();
            process.StandardOutput.BaseStream.CopyTo(ms);
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                var error = new StreamReader(process.StandardError.BaseStream).ReadToEnd();
                throw new Exception($"1Password CLI failed: {error}");
            }

            ms.Position = 0;
            return builder.AddJsonStream(ms);
        }
        catch (Exception ex)
        {
            if (required) throw;

            Console.WriteLine($"[Warning] 1Password document not loaded: {ex.Message}");
            return builder;
        }
    }
}
