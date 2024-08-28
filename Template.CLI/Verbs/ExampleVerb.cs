using CommandLine;

namespace Template.CLI.Verbs
{
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
            static string nothing(string input) =>
                $"The user either chose to provide nothing or the provided path does not exist, so an empty string was used. Here is the literal input: {input}";
            string fileLocation =
                FileLocation == string.Empty || !Path.Exists(FileLocation)
                ? nothing(FileLocation)
                : $"Here is the literal path: {Path.GetFullPath(FileLocation)} And here is the literal input: {FileLocation}";
            string secondLocation =
                SecondFileLocation == string.Empty || !Path.Exists(SecondFileLocation)
                ? nothing(SecondFileLocation)
                : $"Here is the literal path: {Path.GetFullPath(SecondFileLocation)} And here is the literal input: {SecondFileLocation}";
            string thirdLocation =
                ThirdFileLocation == string.Empty || !Path.Exists(ThirdFileLocation)
                ? nothing(ThirdFileLocation)
                : $"Here is the literal path: {Path.GetFullPath(ThirdFileLocation)} And here is the literal input: {ThirdFileLocation}";
            System.Console.WriteLine("The following are the options the user chose:");
            System.Console.WriteLine($"{nameof(FileLocation)}:  {fileLocation}");
            System.Console.WriteLine($"{nameof(SecondFileLocation)}:  {secondLocation}");
            System.Console.WriteLine($"{nameof(ThirdFileLocation)}:  {thirdLocation}");

            return ProgramErrorCodes.Success;
        }
        #endregion
    }
}
