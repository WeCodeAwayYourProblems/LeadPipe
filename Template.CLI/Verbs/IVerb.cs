namespace Template.CLI.Verbs;

internal interface IVerb
{
    public int Run(IServiceProvider service);
}
