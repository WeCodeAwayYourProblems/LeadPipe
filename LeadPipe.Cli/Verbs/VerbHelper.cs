namespace LeadPipe.Cli.Verbs;

internal class VerbHelper
{
    public static bool TryCreate(string location, out string error)
    {
        bool result = false;
        error = string.Empty;
        try
        {
            if (!Path.Exists(location))
                File.Create(location);
            result = true;
        }
        catch (Exception ex)
        {
            error = ex.ToString();
        }
        return result;
    }
}
