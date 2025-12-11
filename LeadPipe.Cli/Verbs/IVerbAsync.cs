namespace LeadPipe.Cli.Verbs;

internal interface IVerbAsync
{
    public Task<int> Run(IServiceProvider service);
}