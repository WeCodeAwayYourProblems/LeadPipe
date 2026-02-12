using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Infrastructure.Service.Report;

[SourceKey(Source.Yeller)]
internal sealed class ReportBothYeller(
    [FromKeyedServices(Schedule.Daily)] IReportService<Plumbing> daily,
    [FromKeyedServices(Schedule.TwoDays)] IReportService<Plumbing> twoDays
    ) : IReportService<Plumbing>
{
    private readonly IReportService<Plumbing> _daily = daily;
    private readonly IReportService<Plumbing> _twoDays = twoDays;
    private List<Plumbing>? DailyData { get; set; }
    private List<Plumbing>? TwoDaysData { get; set; }
    public async Task<Result<List<Plumbing>>> GetDataAsync(bool withDetails = false)
    {
        // Daily Data
        Result<List<Plumbing>> dailyData = await _daily.GetDataAsync(false);
        if (dailyData.IsFailure)
            return dailyData;
        DailyData = dailyData.Value;

        // Two Days Data
        Result<List<Plumbing>> twoDaysData = await _twoDays.GetDataAsync(false);
        if (twoDaysData.IsFailure)
            return twoDaysData;
        TwoDaysData = twoDaysData.Value;

        List<Plumbing> result = [.. dailyData.Value, .. twoDaysData.Value];
        return result;
    }

    public async Task<Result> ReportAsync(List<Plumbing> data)
    {
        if (DailyData is not List<Plumbing> daily || TwoDaysData is not List<Plumbing> twoDays)
        {
            string fault = (DailyData is null, TwoDaysData is null) switch
            {
                (true, false) => "Two Days Data",
                (false, true) => "Daily Data",
                _ => "Two Days Data and Daily Data"
            };
            return Result.Failure($"Did not fetch data properly. Culprit: {fault}");
        }

        Result dailyData = await _daily.ReportAsync(daily);
        Result twoDaysData = await _twoDays.ReportAsync(twoDays);

        return Result.Combine(dailyData, twoDaysData);
    }
}