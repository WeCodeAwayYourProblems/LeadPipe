namespace LeadPipe.Infrastructure.Dto;

#pragma warning disable IDE1006 // Naming Styles
public class ReportYeller
{
    public required string event_id { get; set; }
    public required long event_time { get; set; }
    public string? event_name { get; set; }
    public required string action_source { get; set; }
    public required UserData user_data { get; set; }
    public required CustomData custom_data { get; set; }
}
public class UserData
{
    public string[]? em { get; set; }
    public string? fn { get; set; }
    public string? ln { get; set; }
    public string? db { get; set; }
    public string? ge { get; set; }
    public required string[] ph { get; set; }
    public string[]? country { get; set; }
    public string[]? st { get; set; }
    public string[]? zp { get; set; }
    public string? lead_id { get; set; }
    public string[]? ct { get; set; }
    public string[]? external_id { get; set; }
    public string? client_ip_address { get; set; }
    public string? client_user_agent { get; set; }
    public string? madid { get; set; }
}
public class CustomData
{
    public required decimal value { get; set; }
    public required string currency { get; set; }
    public string? order_id { get; set; }
    public string? content_category { get; set; }
    public string[]? content_ids { get; set; }
    public object[]? contents { get; set; }
    public object[]? event_labels { get; set; }
}

#pragma warning restore IDE1006 // Naming Styles