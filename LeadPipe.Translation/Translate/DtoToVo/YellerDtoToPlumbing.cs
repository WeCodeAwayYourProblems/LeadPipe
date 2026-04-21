using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Translate;
using System.Data;

namespace LeadPipe.Translation.Translate.DtoToVo;

internal partial class YellerDtoToPlumbing : IDtoToVo<YellerDto, Plumbing>
{
    public Plumbing Translate(YellerDto data)
    {
        // Find Phone Number
        HashSet<long> seen = [];
        List<PhoneNumber> collectedNumbers = (data.project?.survey_answers?
            .SelectMany(ans => ans.answer_text ?? []) // flatten all answer_text arrays
            .SelectMany(raw =>
                PhoneNumber.TryParseMany(raw, out var nums)
                    ? nums.Where(n => seen.Add(n.Number)) // only keep new numbers
                    : []
            )
            .ToList()
        ) ?? [];

        collectedNumbers.AddRange(
        data.events?.events?
            .SelectMany(e =>
                PhoneNumber.TryParseMany(FindTextInEvents(e), out var phoneNumbers)
                    ? phoneNumbers.Where(n => seen.Add(n.Number))
                    : []
            ) ?? []);

        if (data.phone_number is not null && PhoneNumber.TryParse(data.phone_number, out var ph) && seen.Add(ph.Number))
            collectedNumbers.Add(ph);

        PhoneNumber canonicalPhoneNumber = collectedNumbers.Count == 0
            ? PhoneNumber.DefaultPhoneNumber
            : collectedNumbers[^1];

        // Date
        DateTimeOffset date =
            data.time_created is DateTime dtc
                ? new(DateTime.SpecifyKind(dtc, DateTimeKind.Utc), TimeSpan.Zero)
                : DateTimeOffset.MaxValue; // This is a domain rule. When compared, there is significance to MaxValue vs MinValue

        // Contents
        List<string> contentsStr = [];
        if (data.project?.survey_answers is SurveyAnswer[] surveyAnswers)
            contentsStr = [.. surveyAnswers.Select(a =>
            {
                string question = a.question_text ?? "(Question text missing)";
                string[] answers = a.answer_text ?? [];

                return string.Join(" | ",
                    answers.Length == 0
                        ? [question, "(Answer missing)"]
                        : [question, .. answers]
                );
            })];

        if (data.ilq?.summary is not null)
            contentsStr.Add(data.ilq.summary);

        if (data.events?.events is Event[] events)
        {
            contentsStr.AddRange(
                events
                    .OrderBy(e => e.time_created ?? DateTime.MaxValue)
                    .Select(FindTextInEvents)
            );
        }

        string contents = string.Join(" <> ", contentsStr);

        string metadata = $"ID: {data.id}";
        return new(
            Id: 0,
            PhoneNumber: canonicalPhoneNumber,
            Date: date,
            Contents: contents,
            Branch: null,
            MetaData: metadata,
            Source.Yeller,
            Numbers: [.. collectedNumbers]
        );
    }

    private static string FindTextInEvents(Event e)
    {
        string? t = e.event_content?.text;
        string? fallback = e.event_content?.fallback_text;

        if (string.Equals(t, fallback, StringComparison.InvariantCultureIgnoreCase))
            return t ?? string.Empty;

        return string.Join(" | ", new[] { t, fallback }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}
