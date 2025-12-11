using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Translate;
using System.Data;

namespace LeadPipe.Translation.Translate.DtoToVo;

internal class YellerDtoToPlumbing : IDtoToVo<YellerDto, Plumbing>
{
    public Plumbing Translate(YellerDto data)
    {
        // Phone Number
        PhoneNumber number = new(PhoneNumber.Default);
        if (data.project?.survey_answers is SurveyAnswer[] answers)
            foreach (SurveyAnswer ans in answers)
                if (ans.answer_text is string[] answerText)
                    foreach (string a in answerText)
                        if (PhoneNumber.TryParse(a, out PhoneNumber? parsed) && parsed.Number != PhoneNumber.Default && parsed is not null)
                        {
                            number = parsed;
                            break;
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

        return new(
            PhoneNumber: number,
            Date: date,
            Contents: contents,
            Source.Yeller
        );
    }
}
