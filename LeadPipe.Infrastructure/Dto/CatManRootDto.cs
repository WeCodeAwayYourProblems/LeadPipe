using System.Text.Json;

namespace LeadPipe.Infrastructure.Dto;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable IDE1006 // Naming Styles
public class CatManRootDto
{
    public CatManDto[]? calls { get; set; }
    public int? page { get; set; }
    public Sort[]? sort { get; set; }
    public int? total_entries { get; set; }
    public int? total_pages { get; set; }
    public int? per_page { get; set; }
    public Stats? stats { get; set; }
    public string? after { get; set; }
    public string? next_page { get; set; }
}
public class Stats
{
}
public class CatManDto
{
    public long id { get; set; }
    public string? sid { get; set; }
    public int? account_id { get; set; }
    public string? name { get; set; }
    public string? cnam { get; set; }
    public string? search { get; set; }
    public string? referrer { get; set; }
    public string? visitor_sid { get; set; }
    public string? location { get; set; }
    public string? source { get; set; }
    public int? source_id { get; set; }
    public string? source_sid { get; set; }
    public int? tgid { get; set; }
    public object? likelihood { get; set; }
    public int? duration { get; set; }
    public string? direction { get; set; }
    public int? talk_time { get; set; }
    public int? ring_time { get; set; }
    public int? hold_time { get; set; }
    public int? wait_time { get; set; }
    public object? parent_id { get; set; }
    public object? email { get; set; }
    public object? street { get; set; }
    public string? city { get; set; }
    public string? state { get; set; }
    public string? country { get; set; }
    public string? postal_code { get; set; }
    public string? called_at { get; set; }
    public long? unix_time { get; set; }
    public int? tracking_number_id { get; set; }
    public string? tracking_number_sid { get; set; }
    public string? tracking_number { get; set; }
    public string? tracking_label { get; set; }
    public string? dial_status { get; set; }
    public bool is_new_caller { get; set; }
    public float indexed_at { get; set; }
    public bool agent_insights { get; set; }
    public Duration_Period? duration_period { get; set; }
    public Inbound_Rate_Center? inbound_rate_center { get; set; }
    public string? business_number { get; set; }
    public string? business_label { get; set; }
    public int? receiving_number_id { get; set; }
    public string? receiving_number_sid { get; set; }
    public float billed_amount { get; set; }
    public DateTime billed_at { get; set; }
    public bool excluded { get; set; }
    public bool redacted { get; set; }
    public string? tracking_number_format { get; set; }
    public string? business_number_format { get; set; }
    public string? alternative_number { get; set; }
    public string? tracking_number_bare { get; set; }
    public string? caller_number_format { get; set; }
    public string? caller_number_complete { get; set; }
    public string? caller_number_bare { get; set; }
    public string? caller_number { get; set; }
    public string[]? caller_number_split { get; set; }
    public string? contact_number { get; set; }
    public bool visitor { get; set; }
    public Call_Path[]? call_path { get; set; }
    public int? left_talk_time { get; set; }
    public int? right_talk_time { get; set; }
    public object[]? transfers { get; set; }
    public string? call_status { get; set; }
    public string? status { get; set; }
    public object[]? spotted { get; set; }
    public Salesforce? salesforce { get; set; }
    public string? audio { get; set; }
    public bool is_s3_link { get; set; }
    public object[]? callbacks { get; set; }
    public object[]? emails { get; set; }
    public string? day { get; set; }
    public string? month { get; set; }
    public string? hour { get; set; }
    public Paid? paid { get; set; }
    public Ga? ga { get; set; }
    public object[]? tag_list { get; set; }
    public float? latitude { get; set; }
    public float? longitude { get; set; }
    public bool extended_lookup_on { get; set; }
    public Form? form { get; set; }
    public object[]? legs { get; set; }
    public Touchpoint[]? touchpoints { get; set; }
    public string? last_touch { get; set; }
    public Geo? geo { get; set; }
    public DateTime timestamp { get; set; }

    /// <summary>
    /// Serializes <see cref="CatManDto"/> to Json
    /// </summary>
    /// <returns></returns>
    public override string ToString() => JsonSerializer.Serialize(this, _options);
    private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };
}
public class Duration_Period
{
    public long gte { get; set; }
    public long lte { get; set; }
}
public class Inbound_Rate_Center
{
    public string? country { get; set; }
    public string? prefix { get; set; }
    public bool tollfree { get; set; }
}
public class Salesforce
{
}
public class Paid
{
    public object? source { get; set; }
    public object? medium { get; set; }
}
public class Ga
{
    public string? cid { get; set; }
    public object[]? ga4 { get; set; }
}
public class Geo
{
    public float lon { get; set; }
    public float lat { get; set; }
}
public class Call_Path
{
    public string? route_name { get; set; }
    public string? route_id { get; set; }
    public string? route_type { get; set; }
    public DateTime started_at { get; set; }
}
public class Touchpoint
{
    public long call_id { get; set; }
    public DateTime time { get; set; }
    public string? touch_type { get; set; }
    public object? clickid { get; set; }
    public object? type { get; set; }
    public string? source { get; set; }
    public object? medium { get; set; }
    public string? position { get; set; }
    public float weight { get; set; }
}
public class Sort
{
    public Unix_Time? unix_time { get; set; }
    public Id? id { get; set; }
}
public class Unix_Time
{
    public string? order { get; set; }
    public string? missing { get; set; }
}
public class Id
{
    public string? order { get; set; }
}


public class Form
{
    public string? number_to_call { get; set; }
    public string? customer_number_to_dial { get; set; }
    public string? name { get; set; }
    public string? email { get; set; }
    public string? trackback_id { get; set; }
    public int form_id { get; set; }
    public string? form_name { get; set; }
    public Custom[] custom { get; set; }
    
    /// <summary>
    /// Serializes <see cref="Form"/> to json
    /// </summary>
    /// <returns></returns>
    public override string ToString() => JsonSerializer.Serialize(this, _options);
    private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };
}

public class Custom
{
    public string label { get; set; }
    public string value { get; set; }
    public string id { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning restore IDE1006 // Naming Styles
}
