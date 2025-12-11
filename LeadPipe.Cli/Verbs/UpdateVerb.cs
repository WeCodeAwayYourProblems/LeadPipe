using CommandLine;
using CSharpFunctionalExtensions;
using LeadPipe.Application.Manager;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Cli.Verbs;

[Verb(Update, HelpText = "This updates specific data.")]
internal class UpdateVerb : IVerb
{
    private const string Update = "update";

    #region Options
    [Option('s', "source", Required = true, HelpText = """
        Enter the source you wish to update.
        Here are the options
        Calli
        Lab
        Leaf
        Leased
        Libacion
        Pan
        Yeller
        All
        """)]
    public Source Source { get; set; }

    #endregion

    #region Public (Other than Options)
    public async Task<int> Run(IServiceProvider service)
    {
        Func<Task<Result<List<Plumbing>>>> Make<T>() where T : IUpdateManager =>
            async () => await service.GetRequiredService<T>().ManageAsync();

        Func<Task<Result<List<Plumbing>>>> action = Source switch
        {
            Source.Calli => Make<IUpdateCalliManager>(),
            Source.Lab => Make<IUpdateLabManager>(),
            Source.Leaf => Make<IUpdateLeafManager>(),
            Source.Leased => Make<IUpdateLeasedManager>(),
            Source.Libacion => Make<IUpdateLibacionManager>(),
            Source.Pan => Make<IUpdatePanManager>(),
            Source.Yeller => Make<IUpdateYellerManager>(),
            Source.All=>Result.Combine<List<Plumbing>>(
                 Make<IUpdateCalliManager>(),
Make<IUpdateLabManager>(),
Make<IUpdateLeafManager>(),
Make<IUpdateLeasedManager>(),
Make<IUpdateLibacionManager>(),
Make<IUpdatePanManager>(),
Make<IUpdateYellerManager>()

                ),

            _ => async () => Result.Failure<List<Plumbing>>("Wrong input")
        };

        Result<List<Plumbing>> value = await action();


    }
    #endregion
}
