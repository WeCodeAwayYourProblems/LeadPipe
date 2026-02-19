using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Translation.Primitives;
using System.Data;

namespace LeadPipe.Translation.Translate.DtoToVo;

internal class YellerDtoToPlumbing : IDtoToVo<YellerDto, Plumbing>
{
    public Plumbing Translate(YellerDto data)
    {
        // Find Phone Number
        PhoneNumber number = new(PhoneNumber.Default);
        if (data.project?.survey_answers is SurveyAnswer[] answers)
        {
            foreach (SurveyAnswer ans in answers)
                if (ans.answer_text is string[] answerText)
                    foreach (string a in answerText)
                        if (PhoneNumber.TryParse(a, out PhoneNumber? parsed) && parsed.Number != PhoneNumber.Default && parsed is not null)
                        {
                            number = parsed;
                            break;
                        }
        }

        // Date
        DateTimeOffset date =
            data.time_created is DateTime dtc
                ? new(DateTime.SpecifyKind(dtc, DateTimeKind.Utc), TimeSpan.Zero)
                : DateTimeOffset.MaxValue;

        // Contents
        IEnumerable<string> contentsStr = [];
        if (data.project?.survey_answers is SurveyAnswer[] surveyAnswers)
            contentsStr = surveyAnswers.Select(a =>
            {
                string question = a.question_text ?? "(Question text missing)";
                string[] answers = a.answer_text ?? [];

                return string.Join(" | ",
                    answers.Length == 0
                        ? [question, "(Answer missing)"]
                        : [question, .. answers]
                );
            });
        else contentsStr = [""];

        string contents = string.Join(" <|> ", contentsStr);

        string metadata = string.Empty;
        return new(
            Id: 0,
            PhoneNumber: number,
            Date: date,
            Contents: contents,
            Branch: null,
            MetaData: metadata,
            Source.Yeller
        );
    }
}

internal class LatherDtoToPlumbing(IDateTimeTranslate translate) : IDtoToVo<LatherDto, Plumbing>
{
    private readonly IDateTimeTranslate _translate = translate;
    public Plumbing Translate(LatherDto data)
    {
        long id = long.TryParse(data.LeadId, out long i) ? i : 0;
        PhoneNumber phoneNumber = PhoneNumber.TryParse(data.Phone, out PhoneNumber p) ? p : new(PhoneNumber.Default);

        DateTime dt = DateTime.TryParse(string.Join(" ", [data.Date, data.Time]), out DateTime d) ? d : DateTime.MinValue;
        ETimeZone zone = data.TimeZone?.ToLowerInvariant() switch
        {
            "est" or "edt" => ETimeZone.Eastern,
            "pst" or "pdt" => ETimeZone.Pacific,
            "cst" or "cdt" => ETimeZone.Central,
            "utc" => ETimeZone.Utc,
            _ => ETimeZone.Mountain, // Default to mountain

        };
        DateTimeOffset date = _translate.Convert(dt, zone);
        string metaData = $"Lead Id: {id}";

        Plumbing result = new(
            Id: id,
            PhoneNumber: phoneNumber,
            Date: date,
            Contents: null,
            Branch: null,
            MetaData: metaData,
            Source: Source.Lather
            );

        return result;
    }
}