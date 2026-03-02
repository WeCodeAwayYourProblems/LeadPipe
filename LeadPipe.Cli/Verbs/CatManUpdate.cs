using CommandLine;
using CSharpFunctionalExtensions;
using LeadPipe.Application.Manager;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Cli.Verbs;

[Verb(Name, HelpText = "Update CatMan data.")]
internal class CatManUpdate : IVerbAsync
{
    private const string Name = "catman";

    #region Options
    [Option('r', "refresh", Default = false, HelpText = "Whether or not to performa refresh. If not, all data will be retrieved. Defaults to false")]
    public bool Refresh { get; set; }
    #endregion

    #region Public (Other than Options)
    public async Task<int> Run(IServiceProvider service)
    {
        ICatManManager manager = service.GetRequiredService<ICatManManager>();
        Result result = await manager.Manage(Refresh);
        
        int code = result.IsSuccess ? 0 : 1;
        Environment.ExitCode = code;
        
        return code;
    }
    #endregion

}
