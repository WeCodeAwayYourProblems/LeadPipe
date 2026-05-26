using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.DtoToVo;

internal sealed class CatManDtoToCornFormula : IDtoToVo<CatManDto, CornFormula>
{
    public CornFormula Translate(CatManDto data)
    {
        const string source = "utm_source";
        const string medium = "utm_medium";
        const string campaign = "utm_campaign";
        const string content = "utm_content";
        const string term = "utm_term";

        DateTimeOffset date = DateTimeOffset.TryParse(data.called_at, out var d)
            ? d
            : DateTimeOffset.MinValue;

        PhoneNumber number = new(data.caller_number_bare);

        string payLoad = data.ToString();

        var meta = (data.form?.custom ?? [])
            .Where(c =>
                !string.IsNullOrWhiteSpace(c.id) &&
                !string.IsNullOrWhiteSpace(c.value) &&
                !string.IsNullOrWhiteSpace(c.label))
            .Select(LabelIdValue)
            .ToList();

        string src = string.Join(" | ",
            new[] { data.source ?? "Unknown", }
                .Concat(ExtractUtm(medium, meta))
                .Concat(ExtractUtm(campaign, meta))
                .Concat(ExtractUtm(source, meta)));
        string metaData = string.Join(" | ",
            meta.Select(v => v.ToString())
                .Where(v => !string.IsNullOrWhiteSpace(v)));
        string utmSource = string.Join(" | ", ExtractUtm(source, meta));
        string utmMedium = string.Join(" | ", ExtractUtm(medium, meta));
        string utmCampaign = string.Join(" | ", ExtractUtm(campaign, meta));
        string utmContent = string.Join(" | ", ExtractUtm(content, meta));
        string utmTerm = string.Join(" | ", ExtractUtm(term, meta));

        var result = new CornFormula(
            Id: data.id,
            PhoneNumber: number,
            Date: date,
            PayLoad: payLoad,
            MetaData: metaData,
            Source: src,
            UtmSource: utmSource,
            UtmMedium: utmMedium,
            UtmCampaign: utmCampaign,
            UtmContent: utmContent,
            UtmTerm: utmTerm
            );

        return result;
    }
    private static LabelIdValueDto LabelIdValue(Custom c) => new(c.label, c.id, c.value);
    private static IEnumerable<string> ExtractUtm(string key, List<LabelIdValueDto> meta) =>
        meta.Where(v => v.Id == key || v.Label == key || v.Value == key)
            .Select(v => v.ToString());
    private record LabelIdValueDto(string? Label, string? Id, string? Value)
    {
        public override string ToString() => $"Label: {Label}, Id: {Id}, Value: {Value}";
    };
}