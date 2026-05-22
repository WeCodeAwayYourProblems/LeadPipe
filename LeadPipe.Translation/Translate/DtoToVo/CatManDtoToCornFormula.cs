using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Translation.Translate.VoToEntity;

namespace LeadPipe.Translation.Translate.DtoToVo;

internal sealed class CatManDtoToCornFormula : IDtoToVo<CatManDto, CornFormula>
{
    public CornFormula Translate(CatManDto data)
    {
        DateTimeOffset date = DateTimeOffset.TryParse(data.called_at, out var d)
            ? d
            : DateTimeOffset.MinValue;

        PhoneNumber number = new(data.caller_number_bare);

        string payLoad = data.ToString();

        IEnumerable<string> meta = (data.form?.custom ?? [])
            .Where(c => !string.IsNullOrWhiteSpace(c.id) && !string.IsNullOrWhiteSpace(c.value))
            .Select(LabelIdValue);

        IEnumerable<string> src = (data.form?.custom ?? [])
            .Where(c => 
                !string.IsNullOrWhiteSpace(c.id) && 
                (
                    c.id.Equals("utm_source", StringComparison.InvariantCultureIgnoreCase) ||
                    c.id.Equals("utm_medium", StringComparison.InvariantCultureIgnoreCase) ||
                    c.id.Equals("utm_campaign", StringComparison.InvariantCultureIgnoreCase)
                )
            )
            .Select(LabelIdValue);

        string source = string.Join(" | ", [data.source ?? "Unknown", src]);
        string metaData = string.Join(" | ", meta.Where(v => !string.IsNullOrWhiteSpace(v)));

        var result = new CornFormula(
            Id: data.id,
            PhoneNumber: number,
            Date: date,
            PayLoad: payLoad,
            MetaData: metaData,
            Source: source
            );

        return result;
    }
    private static string LabelIdValue(Custom c) => $"Label: {c.label}, Id: {c.id}, Value: {c.value}";
}