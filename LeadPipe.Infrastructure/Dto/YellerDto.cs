namespace LeadPipe.Infrastructure.Dto;

#pragma warning disable IDE1006 // Naming Styles
public class YellerDto
{
    #region Ctor
    public YellerDto() { }
    private YellerDto(YellerDto d)
    {
        id = d.id;
        business_id = d.business_id;
        conversation_id = d.conversation_id;
        temporary_email_address = d.temporary_email_address;
        temporary_email_address_expiry = d.temporary_email_address_expiry;
        temporary_phone_number = d.temporary_phone_number;
        time_created = d.time_created;
        last_event_time = d.last_event_time;
        user = d.user;
        project = d.project?.Clone();
        events = d.events;
    }
    public YellerDto Clone() => new(this);
    #endregion

    public string? id { get; set; }
    public string? business_id { get; set; }
    public string? conversation_id { get; set; }
    public string? temporary_email_address { get; set; }
    public DateTime? temporary_email_address_expiry { get; set; }
    public string? temporary_phone_number { get; set; }
    public DateTime? time_created { get; set; }
    public DateTime? last_event_time { get; set; }
    public string? phone_number { get; set; }
    public object? user { get; set; }
    public Project? project { get; set; }
    public Ilq? ilq { get; set; }
    public YellerEventDto? events { get; set; }
}

#region Attached
public class Project
{
    #region Ctor
    public Project() { }
    private Project(Project p)
    {
        location = p.location?.Clone();
        availability = p.availability?.Clone();
        job_names = p.job_names?.ToArray();
        survey_answers = p.survey_answers?.Select(a => a.Clone()).ToArray();
        attachments = p.attachments?.Select(a => a.Clone()).ToArray();
    }
    public Project Clone() => new(this);
    #endregion

    public SurveyAnswer[]? survey_answers { get; set; }
    public Location? location { get; set; }
    public string? additional_info { get; set; }
    public Availability? availability { get; set; }
    public string[]? job_names { get; set; }
    public Attach[]? attachments { get; set; }
}

public class Attach
{
    #region Ctor
    public Attach() { }
    private Attach(Attach a)
    {
        id = a.id;
        url = a.url;
        resource_name = a.resource_name;
        mime_type = a.mime_type;
    }
    public Attach Clone() => new(this);
    #endregion

    public string? id { get; set; }
    public string? url { get; set; }
    public string? resource_name { get; set; }
    public string? mime_type { get; set; }
}

public class SurveyAnswer
{
    #region Ctor
    public SurveyAnswer() { }
    private SurveyAnswer(SurveyAnswer a)
    {
        question_text = a.question_text;
        question_identifier = a.question_identifier;
        answer_text = a.answer_text?.ToArray();
    }
    public SurveyAnswer Clone() => new(this);
    #endregion

    public string? question_text { get; set; }
    public string? question_identifier { get; set; }
    public string[]? answer_text { get; set; }
}

public class Location
{
    #region Ctor
    public Location() { }
    private Location(Location l)
    {
        postal_code = l.postal_code;
    }
    public Location Clone() => new(this);
    #endregion

    public string? postal_code { get; set; }
}

public class Availability
{
    #region Ctor
    public Availability() { }
    private Availability(Availability a)
    {
        status = a.status;
        dates = a.dates?.ToArray();
    }
    public Availability Clone() => new(this);
    #endregion

    public string? status { get; set; }
    public string[]? dates { get; set; }
}
public class Ilq
{
    public string? summary { get; set; }
    public string? status { get; set; }
}
public class YellerHelperDto
{
    #region Ctor
    public YellerHelperDto() { }
    private YellerHelperDto(YellerHelperDto y)
    {
        lead_ids = y.lead_ids?.ToArray();
        has_more = y.has_more;
    }
    public YellerHelperDto Clone() => new(this);
    #endregion

    public string[]? lead_ids { get; set; }
    public bool has_more { get; set; }
}

#endregion
