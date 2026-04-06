using CommandLine;
using CSharpFunctionalExtensions;
using LeadPipe.Application.Manager;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Cli.Verbs;

[Verb("data", HelpText = "This updates and/or reports specific data.")]
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
        """)]
    public Source Source { get; set; } = Source.Test;
    [Option('r', "report", Required = false, HelpText = "Whether to perform the report.")]
    public bool Report { get; set; } = false;
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
        Result result = (Update, Report) switch
        {
            (true, false) => await Updated(provider, Source, Refresh, ForceRun),
            (false, true) => await Reported(provider, Source),
            (true, true) or (false, false) => await Both(provider, Source, Refresh, ForceRun),
        };

        if (result.IsFailure)
            Console.WriteLine(result.Error);

        int code = result.IsSuccess ? 0 : 1;
        Environment.ExitCode = code; // Setting the exit code here helps for cli usage
        return code;
    }

    #endregion

    #region Private

    private static async Task<Result> Updated(IServiceProvider service, Source source, bool refresh, bool forceRun)
    {
        IUpdateManager update = service.GetRequiredService<IUpdateManager>();
        Result updated = source == Source.Test
            ? await update.Manage(refresh, forceRun)
            : await update.Manage(source, refresh, forceRun);
        return updated;
    }
    private static async Task<Result> Reported(IServiceProvider service, Source source)
    {
        IReportManager report = service.GetRequiredService<IReportManager>();
        Result reported = source == Source.Test
            ? await report.Manage()
            : await report.Manage(source);
        return reported;
    }
    private static async Task<Result> Both(IServiceProvider service, Source source, bool refresh, bool forceRun)
    {
        Result updated = await Updated(service, source, refresh, forceRun);
        if (updated.IsFailure)
            return updated;

        Result reported = await Reported(service, source);
        return reported;
    }

    #endregion
}
