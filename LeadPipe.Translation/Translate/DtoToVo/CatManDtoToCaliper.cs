using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.DtoToVo;

internal sealed class CatManDtoToCaliper : IDtoToVo<CatManDto, Caliper>
{
    public Caliper Translate(CatManDto data)
    {
        long id = data.id;

        long unix = (long)(data.unix_time is null ? 0 : data.unix_time);
        DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(unix);

        PhoneNumber number = new(data.caller_number_bare);

        double dur = (double)(data.duration is null ? 0 : data.duration);
        TimeSpan duration = TimeSpan.FromSeconds(dur);

        var location = data.location!;
        string note = data.form?.ToString() ?? string.Empty;
        string source = data.tracking_label!;
        bool billable = false;

        Caliper result = new(
            Id: id,
            Date: date,
            Number: number,
            Duration: duration,
            Note: note,
            Source: source,
            Label: data.tracking_label ?? "Unknown",
            Billable: billable,
            Location: location
        );

        return result;
    }
}