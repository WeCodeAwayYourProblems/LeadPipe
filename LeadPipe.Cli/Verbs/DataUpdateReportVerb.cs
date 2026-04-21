using CommandLine;
using CSharpFunctionalExtensions;
using LeadPipe.Application.Manager;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Cli.Verbs;

[Verb("data", HelpText = "This updates specific data.")]
internal class DataUpdateReportVerb : IVerbAsync
{

    #region Options

    [Option('s', "source", Required = false, HelpText = """
        Enter the source you wish to update. All will be updated if none are chosen
        Here are the options:
        Calli
        Lab
        Leaf
        Leased
        Libacion
        Pan
        Yeller
        Lather
        """)]
    public Source Source { get; set; } = Source.Test;
    [Option('u', "update", Required = false, HelpText = "Whether to update.")]
    public bool Update { get; set; } = false;
    [Option('R', "refresh", Required = false, HelpText = "Whether or not to perform a data refresh. If refresh, the process is likely to take less time, but the data may not be full.")]
    public bool Refresh { get; set; } = false;
    [Option('f', "force", Required = false, HelpText = "Whether or not to force the run. Will run updates even if it was already run recently")]
    public bool ForceRun { get; set; } = false;

    #endregion

    #region Public

    public async Task<int> Run(IServiceProvider provider)
    {
        ForceRunRefresh frr = new(ForceRun: ForceRun, Refresh: Refresh);
        IUpdateManager update = provider.GetRequiredService<IUpdateManager>();
        Result updated = Source == Source.Test
            ? await update.Manage(frr)
            : await update.Manage(Source, frr);

        if (updated.IsFailure)
            Console.WriteLine(updated.Error);

        int code = updated.IsSuccess ? 0 : 1;
        Environment.ExitCode = code; // Setting the exit code here helps with cli usage
        return code;
    }

    #endregion
}
