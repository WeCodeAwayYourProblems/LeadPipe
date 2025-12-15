using CommandLine;
using CSharpFunctionalExtensions;
using LeadPipe.Application.Manager;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Cli.Verbs;

public enum DataCliType
{
    Report,
    Update
}

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
    [Option('t', "type", Required = false, HelpText = """
        Enter whether to update the data or send a report.
        If no option is chosen, both will execute
        Here are the options.
        Report
        Update
        """)]
    public DataCliType? Type { get; set; } = null;

    #endregion

    #region Public

    public async Task<int> Run(IServiceProvider service)
    {
        Result<List<Plumbing>> result = Type switch
        {
            DataCliType.Update => await Update(service),
            DataCliType.Report => await Report(service),
            _ or null => await Combine(service)
        };

        if (result.IsSuccess)
            Environment.ExitCode = 0;
        else
            Environment.ExitCode = 1;
        return Environment.ExitCode;
    }

    #endregion

    #region Private

    private async Task<Result<List<Plumbing>>> Update(IServiceProvider service)
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

            _ => async () =>
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
        };

        Result<List<Plumbing>> result = await action();
        return result;
    }
    private async Task<Result<List<Plumbing>>> Report(IServiceProvider service)
    {
        Func<Task<Result<List<Plumbing>>>> Make<T>() where T : IReportManager<Plumbing> =>
            async () => await service.GetRequiredService<T>().ManageAsync();

        Func<Task<Result<List<Plumbing>>>> action = Source switch
        {
            Source.Calli => Make<IReportCalliManager>(),
            Source.Lab => Make<IReportLabManager>(),
            Source.Leaf => Make<IReportLeafManager>(),
            Source.Leased => Make<IReportLeasedManager>(),
            Source.Libacion => Make<IReportLibacionManager>(),
            Source.Pan => Make<IReportPanManager>(),
            Source.Yeller => Make<IReportYellerManager>(),

            _ => async () =>
            {
                // create an array of funcs then invoke them to get tasks
                Func<Task<Result<List<Plumbing>>>>[] funcs = [
                Make<IReportCalliManager>(),
                Make<IReportLabManager>(),
                Make<IReportLeafManager>(),
                Make<IReportLeasedManager>(),
                Make<IReportLibacionManager>(),
                Make<IReportPanManager>(),
                Make<IReportYellerManager>()
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
        };

        Result<List<Plumbing>> result = await action();
        return result;
    }
    private async Task<Result<List<Plumbing>>> Combine(IServiceProvider service)
    {
        Result<List<Plumbing>> update = await Update(service);
        Result<List<Plumbing>> report = await Report(service);
        Result result = Result.Combine(" | ", update, report);
        return result.IsSuccess
            ? Result.Success<List<Plumbing>>([.. update.Value, .. report.Value])
            : Result.Failure<List<Plumbing>>(result.Error);
    }

    #endregion

}
