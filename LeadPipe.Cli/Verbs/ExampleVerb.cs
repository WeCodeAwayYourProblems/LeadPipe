using CommandLine;

namespace LeadPipe.Cli.Verbs;

[Verb(Example, HelpText = "This is an example.")]
internal class ExampleVerb : IVerb
{
    private const string Example = "example";

    #region Options
    [Option('f', "fileName", Required = false, HelpText = "Enter the name of the file.")]
    public string FileLocation { get; set; } = string.Empty;
    [Option('s', "secondFile", Required = false, HelpText = "Enter the name of the second file.")]
    public string SecondFileLocation { get; set; } = string.Empty;
    [Option('t', "thirdFile", Required = false, HelpText = "Enter the name of the third file.")]
    public string ThirdFileLocation { get; set; } = string.Empty;
    #endregion

    #region Public (Other than Options)
    public int Run(IServiceProvider service)
    {
        string fileLocation = GetMessage(FileLocation);
        string secondLocation = GetMessage(SecondFileLocation);
        string thirdLocation = GetMessage(ThirdFileLocation);
        Console.WriteLine($"The following is the current working directory:\n\t{Environment.CurrentDirectory}");
        Console.WriteLine("The following are the options the user chose.");
        Console.WriteLine($"{nameof(FileLocation)}:\n\t{fileLocation}");
        Console.WriteLine($"{nameof(SecondFileLocation)}:\n\t{secondLocation}");
        Console.WriteLine($"{nameof(ThirdFileLocation)}:\n\t{thirdLocation}");

        return ProgramErrorCodes.Success;
    }
    #endregion

    #region Private
    private static string GetMessage(string location)
    {
        return location == string.Empty || !Path.Exists(location)
            ? $"The user either chose to provide nothing or the provided path does not exist, so an empty string was used. Here is the literal input: {location}"
            : $"Here is the literal path:\n\t{Path.GetFullPath(location)}\n\tAnd here is the literal input: {location}";
    }
    #endregion
}
