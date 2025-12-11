using CommandLine;
using CSharpFunctionalExtensions;
using LeadPipe.Application.Manager;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Cli.Verbs;

[Verb(Update, HelpText = "This updates specific data.")]
internal class UpdateVerb : IVerbAsync
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

            Source.All => async () =>
            {
                // create an array of funcs then invoke them to get tasks
                Func<Task<Result<List<Plumbing>>>>[] funcs = [
                Make<IUpdateCalliManager>(),
                Make<IUpdateLabManager>(),
                Make<IUpdateLeafManager>(),
                Make<IUpdateLeasedManager>(),
                Make<IUpdateLibacionManager>(),
                Make<IUpdatePanManager>(),
                Make<IUpdateYellerManager>()
                ];

                // start them all and capture individual errors, if any occur
                IEnumerable<Task<Result<List<Plumbing>>>> tasks = funcs.Select(async f =>
                {
                    try { return await f(); }
                    catch (Exception ex) { return Result.Failure<List<Plumbing>>(ex.Message); }
                });

                List<Result<List<Plumbing>>> results = [.. await Task.WhenAll(tasks)];

                // Combine Results
                Result v = Result.Combine(results);
                if (v.IsFailure)
                    return Result.Failure<List<Plumbing>>(v.Error);

                // Flatten results
                List<Plumbing> combination = [];
                foreach (var r in results)
                    combination.AddRange(r.Value);
                return Result.Success(combination);
            }
            ,

            _ => async () => Result.Failure<List<Plumbing>>("Wrong input")
        };

        Result<List<Plumbing>> result = await action();
        if (result.IsSuccess)
            Environment.ExitCode = 0;
        else
            Environment.ExitCode = 1;
        return Environment.ExitCode;
    }

    #endregion
}
