namespace LeadPipe.Infrastructure.Dto;

public class YellerDto
{
#pragma warning disable IDE1006 // Naming Styles
    public string? id { get; set; }
    public string? business_id { get; set; }
    public string? conversation_id { get; set; }
    public string? temporary_email_address { get; set; }
    public DateTime? temporary_email_address_expiry { get; set; }
    public string? temporary_phone_number { get; set; }
    public DateTime? time_created { get; set; }
    public DateTime? last_event_time { get; set; }
    public object? user { get; set; }
    public Project? project { get; set; }

#pragma warning restore IDE1006 // Naming Styles
}

#region Attached
public class Project
{
#pragma warning disable IDE1006 // Naming Styles
    public Location? location { get; set; }
    public Availability? availability { get; set; }
    public string[]? job_names { get; set; }
    public SurveyAnswer[]? survey_answers { get; set; }
    public Attach[]? attachments { get; set; }
#pragma warning restore IDE1006 // Naming Styles
}
public class Attach
{
#pragma warning disable IDE1006 // Naming Styles
    public string? id { get; set; }
    public string? url { get; set; }
    public string? resource_name { get; set; }
    public string? mime_type { get; set; }
#pragma warning restore IDE1006 // Naming Styles
}
public class SurveyAnswer
{
#pragma warning disable IDE1006 // Naming Styles
    public string? question_text { get; set; }
    public string? question_identifier { get; set; }
    public string[]? answer_text { get; set; }
#pragma warning restore IDE1006 // Naming Styles
}
public class Location
{
#pragma warning disable IDE1006 // Naming Styles
    public string? postal_code { get; set; }
#pragma warning restore IDE1006 // Naming Styles
}
public class Availability
{
#pragma warning disable IDE1006 // Naming Styles
    public string? status { get; set; }
    public string[]? dates { get; set; }
#pragma warning restore IDE1006 // Naming Styles
}
public class YellerHelperDto
{
#pragma warning disable IDE1006 // Naming Styles
    public string[]? lead_ids { get; set; }
    public bool has_more { get; set; }
#pragma warning restore IDE1006 // Naming Styles
}
#endregion