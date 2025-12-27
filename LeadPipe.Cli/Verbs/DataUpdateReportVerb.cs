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
    [Option('r', "report", Required = false, HelpText = "Whether to include the report in the update")]
    public bool IncludeReport { get; set; } = false;

    #endregion

    #region Public

    public async Task<int> Run(IServiceProvider service)
    {
        IUpdateAndReportAllManager manager = service.GetRequiredService<IUpdateAndReportAllManager>();
        Result result = Source == Source.Test
            ? await manager.Manage(IncludeReport)
            : await manager.Manage(Source, IncludeReport);

        int code = result.IsSuccess ? 0 : 1;
        Environment.ExitCode = code;
        return Environment.ExitCode;
    }

    #endregion

}
