using CSharpFunctionalExtensions;
using LeadPipe.Application.UpdateReportPipeline;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Manager;


public interface IUpdateAndReportAllManager
{
    Task<Result> Manage();
    Task<Result> Manage(Source source);
}

internal sealed class UpdateAndReportAllManager(
    IUpdateCalliManager calliUpdate,
    IUpdateCallsManager callsUpdate,
    IUpdateLabManager labUpdate,
    IUpdateLeafManager leafUpdate,
    IUpdateLeasedManager leasedUpdate,
    IUpdateLibacionManager libacionUpdate,
    IUpdatePanManager panUpdate,
    IUpdateSandwichManager sandwichUpdate,
    IUpdateYellerManager yellerUpdate,

    IPlumbingAssociationManager associate,

    IReportCalliManager calliReport,
    IReportLabManager labReport,
    IReportLeafManager leafReport,
    IReportLeasedManager leasedReport,
    IReportLibacionManager libacionReport,
    IReportPanManager panReport,
    IReportYellerManager yellerReport
    ) : IUpdateAndReportAllManager
{
    private readonly IUpdateCalliManager _calliUpdate = calliUpdate;
    private readonly IUpdateCallsManager _callsUpdate = callsUpdate;
    private readonly IUpdateLabManager _labUpdate = labUpdate;
    private readonly IUpdateLeafManager _leafUpdate = leafUpdate;
    private readonly IUpdateLeasedManager _leasedUpdate = leasedUpdate;
    private readonly IUpdateLibacionManager _libacionUpdate = libacionUpdate;
    private readonly IUpdatePanManager _panUpdate = panUpdate;
    private readonly IUpdateSandwichManager _sandwichUpdate = sandwichUpdate;
    private readonly IUpdateYellerManager _yellerUpdate = yellerUpdate;

    private readonly IPlumbingAssociationManager _associate = associate;

    private readonly IReportCalliManager _calliReport = calliReport;
    private readonly IReportLabManager _labReport = labReport;
    private readonly IReportLeafManager _leafReport = leafReport;
    private readonly IReportLeasedManager _leasedReport = leasedReport;
    private readonly IReportLibacionManager _libacionReport = libacionReport;
    private readonly IReportPanManager _panReport = panReport;
    private readonly IReportYellerManager _yellerReport = yellerReport;
    private const string ErrorMessagesSeparator = " | ";
    public async Task<Result> Manage()
    {
        // Updaters
        Result<List<Call>> callsUpdateResult = await _callsUpdate.ManageAsync();
        Result<List<Sandwich>> sandwichUpdateResult = await _sandwichUpdate.ManageAsync();
        
        // Plumbing updaters
        Result<List<Plumbing>> calliUpdateResult = await _calliUpdate.ManageAsync();
        Result<List<Plumbing>> labUpdateResult = await _labUpdate.ManageAsync();
        Result<List<Plumbing>> leafUpdateResult = await _leafUpdate.ManageAsync();
        Result<List<Plumbing>> leasedUpdateResult = await _leasedUpdate.ManageAsync();
        Result<List<Plumbing>> libacionUpdateResult = await _libacionUpdate.ManageAsync();
        Result<List<Plumbing>> panUpdateResult = await _panUpdate.ManageAsync();
        Result<List<Plumbing>> yellerUpdateResult = await _yellerUpdate.ManageAsync();

        Result associated = await _associate.ManageAsync();

        Result associatedCallsSandwich = Result.Combine(ErrorMessagesSeparator, callsUpdateResult, sandwichUpdateResult, associated);

        // We can't do the reporters because this step must succeed first
        if (associatedCallsSandwich.IsFailure) 
            return Result.Failure(associatedCallsSandwich.Error);

        // Reporters
        Result<List<Plumbing>> calliReportResult = calliUpdateResult.IsSuccess
            ? await _calliReport.ManageAsync()
            : Result.Failure<List<Plumbing>>(calliUpdateResult.Error);
        Result<List<Plumbing>> labReportResult = labUpdateResult.IsSuccess
            ? await _labReport.ManageAsync()
            : Result.Failure<List<Plumbing>>(labUpdateResult.Error);
        Result<List<Plumbing>> leafReportResult = leafUpdateResult.IsSuccess
            ? await _leafReport.ManageAsync()
            : Result.Failure<List<Plumbing>>(leafUpdateResult.Error);
        Result<List<Plumbing>> leasedReportResult = leasedUpdateResult.IsSuccess
            ? await _leasedReport.ManageAsync()
            : Result.Failure<List<Plumbing>>(leasedUpdateResult.Error);
        Result<List<Plumbing>> libacionReportResult = libacionUpdateResult.IsSuccess
            ? await _libacionReport.ManageAsync()
            : Result.Failure<List<Plumbing>>(libacionUpdateResult.Error);
        Result<List<Plumbing>> panReportResult = panUpdateResult.IsSuccess
            ? await _panReport.ManageAsync()
            : Result.Failure<List<Plumbing>>(panUpdateResult.Error);
        Result<List<Plumbing>> yellerReportResult = yellerUpdateResult.IsSuccess
            ? await _yellerReport.ManageAsync()
            : Result.Failure<List<Plumbing>>(yellerUpdateResult.Error);

        return Result.Combine(ErrorMessagesSeparator,
            calliUpdateResult,
            labUpdateResult,
            leafUpdateResult,
            leasedUpdateResult,
            libacionUpdateResult,
            panUpdateResult,
            yellerUpdateResult,
            calliReportResult,
            labReportResult,
            leafReportResult,
            leasedReportResult,
            libacionReportResult,
            panReportResult,
            yellerReportResult
        );
    }
    public async Task<Result> Manage(Source source)
    {
        Result<List<Call>> callsUpdateResult = await _callsUpdate.ManageAsync();
        Result<List<Sandwich>> sandwichUpdateResult = await _sandwichUpdate.ManageAsync();

        Result<List<Plumbing>> sourceUpdateResult = source switch
        {
            Source.Calli => await _calliUpdate.ManageAsync(),
            Source.Lab => await _labUpdate.ManageAsync(),
            Source.Leaf => await _leafUpdate.ManageAsync(),
            Source.Leased => await _leasedUpdate.ManageAsync(),
            Source.Libacion => await _libacionUpdate.ManageAsync(),
            Source.Pan => await _panUpdate.ManageAsync(),
            Source.Yeller => await _yellerUpdate.ManageAsync(),
            _ => Result.Failure<List<Plumbing>>($"Unknown source: {source}")
        };

        Result associate = await _associate.ManageAsync();
        
        Result combined = Result.Combine(ErrorMessagesSeparator, callsUpdateResult, sandwichUpdateResult, sourceUpdateResult, associate);
        
        Result<List<Plumbing>> sourceReportResult = combined.IsSuccess
            ? source switch
            {
                Source.Calli => await _calliReport.ManageAsync(),
                Source.Lab => await _labReport.ManageAsync(),
                Source.Leaf => await _leafReport.ManageAsync(),
                Source.Leased => await _leasedReport.ManageAsync(),
                Source.Libacion => await _libacionReport.ManageAsync(),
                Source.Pan => await _panReport.ManageAsync(),
                Source.Yeller => await _yellerReport.ManageAsync(),
                _ => Result.Failure<List<Plumbing>>($"Unknown source: {source}")
            }
            : Result.Failure<List<Plumbing>>(combined.Error);
        
        return sourceReportResult;
    }
}