namespace LeadPipe.Cli.Verbs;

internal interface IVerb
{
    public int Run(IServiceProvider service);
}
