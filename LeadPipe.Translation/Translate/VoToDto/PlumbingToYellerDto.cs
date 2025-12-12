using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.VoToDto;

internal class PlumbingToYellerDto : IVoToDto<Plumbing, YellerDto>
{
    public YellerDto Translate(Plumbing data)
    {
        string number = $"{data.PhoneNumber.Number}";
        DateTime date = data.Date.UtcDateTime;
        string contents = data.Contents is null
            ? string.Empty
            : data.Contents;
        
        SurveyAnswer answer = new() { answer_text = [contents] };
        Project project = new() { survey_answers = [answer] };
        
        return new YellerDto() { temporary_phone_number = number, project = project, time_created = date };
    }
}