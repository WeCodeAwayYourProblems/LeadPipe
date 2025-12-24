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

    #endregion

    #region Public

    public async Task<int> Run(IServiceProvider service)
    {
        IUpdateAndReportAllManager manager = service.GetRequiredService<IUpdateAndReportAllManager>();
        Result result = Source == Source.Test
            ? await manager.Manage()
            : await manager.Manage(Source);
        
        int code = result.IsSuccess ? 1 : 0;
        Environment.ExitCode = code;
        return Environment.ExitCode;
    }

    #endregion

}
