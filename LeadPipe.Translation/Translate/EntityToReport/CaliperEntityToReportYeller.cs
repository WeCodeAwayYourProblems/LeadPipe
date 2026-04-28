using LeadPipe.Core;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Translation.Translate.EntityToReport;

internal sealed class CaliperEntityToReportYeller(IYellerSettings settings) : EntityToReportYeller<CaliperEntity>
{
    protected override string Type => settings.YellerCaliperName!;

    protected override long GetEntityId(CaliperEntity e) => e.Id;

    protected override long GetPhoneNumber(CaliperEntity e) => e.PhoneNumber.Number;

    protected override long GetUnixDate(CaliperEntity e) => e.UnixDate;
}


internal abstract class EntityToReportYeller<TEntity> : IEntityToReport<TEntity, ReportYeller>
{
    private const string _none = "None";
    protected abstract string Type { get; }
    protected abstract long GetUnixDate(TEntity e);
    protected abstract long GetEntityId(TEntity e);
    protected abstract long GetPhoneNumber(TEntity e);
    public ReportYeller Translate(TEntity data)
    {
        var phone = GetPhoneNumber(data);
        var eventDate = DateTimeOffsetExt.FromUnixTime(GetUnixDate(data)).UtcDateTime;
        var entityId = GetEntityId(data);

        var result = new ReportYeller
        {
            EventMedium = Type,
            PhoneNumber = phone,
            EventDateUtc = eventDate,
            CloseDateUtc = DateTimeOffset.MinValue.UtcDateTime,
            Value = 0m,
            Type = _none,
            Completed = false,
            CombinedSellers = _none,
            SId = 0,
            EId = entityId
        };
        
        return result;
    }
}