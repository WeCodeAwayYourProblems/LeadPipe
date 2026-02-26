using System.Text.Json.Serialization;

namespace LeadPipe.Infrastructure.Dto;

public class CatmanDto
{
    [JsonPropertyName("calls")]
    public Call[]? Calls { get; set; }
    [JsonPropertyName("page")]
    public int? Page { get; set; }
    [JsonPropertyName("sort")]
    public Sort[]? Sort { get; set; }
    [JsonPropertyName("total_entries")]
    public int? TotalEntries { get; set; }
    [JsonPropertyName("total_pages")]
    public int? TotalPages { get; set; }
    [JsonPropertyName("per_page")]
    public int? PerPage { get; set; }
    [JsonPropertyName("stats")]
    public Stats? Stats { get; set; }
    [JsonPropertyName("after")]
    public string? After { get; set; }
    [JsonPropertyName("next_page")]
    public string? NextPage { get; set; }
}
public class Stats
{
}
public class Call
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
    [JsonPropertyName("sid")]
    public string? Sid { get; set; }
    [JsonPropertyName("account_id")]
    public int? AccountId { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("cnam")]
    public string? Cnam { get; set; }
    [JsonPropertyName("search")]
    public string? Search { get; set; }
    [JsonPropertyName("referrer")]
    public string? Referrer { get; set; }
    [JsonPropertyName("visitor_sid")]
    public string? VisitorSid { get; set; }
    [JsonPropertyName("location")]
    public string? Location { get; set; }
    [JsonPropertyName("source")]
    public string? Source { get; set; }
    [JsonPropertyName("source_id")]
    public int? SourceId { get; set; }
    [JsonPropertyName("source_sid")]
    public string? SourceSid { get; set; }
    [JsonPropertyName("tgid")]
    public int? Tgid { get; set; }
    [JsonPropertyName("likelihood")]
    public object? Likelihood { get; set; }
    [JsonPropertyName("duration")]
    public int? Duration { get; set; }
    [JsonPropertyName("direction")]
    public string? Direction { get; set; }
    [JsonPropertyName("talk_time")]
    public int? TalkTime { get; set; }
    [JsonPropertyName("ring_time")]
    public int? RingTime { get; set; }
    [JsonPropertyName("hold_time")]
    public int? HoldTime { get; set; }
    [JsonPropertyName("wait_time")]
    public int? WaitTime { get; set; }
    [JsonPropertyName("parent_id")]
    public object? ParentId { get; set; }
    [JsonPropertyName("email")]
    public object? Email { get; set; }
    [JsonPropertyName("street")]
    public object? Street { get; set; }
    [JsonPropertyName("city")]
    public string? City { get; set; }
    [JsonPropertyName("state")]
    public string? State { get; set; }
    [JsonPropertyName("country")]
    public string? Country { get; set; }
    [JsonPropertyName("postal_code")]
    public string? PostalCode { get; set; }
    [JsonPropertyName("called_at")]
    public string? CalledAt { get; set; }
    [JsonPropertyName("unix_time")]
    public int? UnixTime { get; set; }
    [JsonPropertyName("tracking_number_id")]
    public int? TrackingNumberId { get; set; }
    [JsonPropertyName("tracking_number_sid")]
    public string? TrackingNumberSid { get; set; }
    [JsonPropertyName("tracking_number")]
    public string? TrackingNumber { get; set; }
    [JsonPropertyName("tracking_label")]
    public string? TrackingLabel { get; set; }
    [JsonPropertyName("dial_status")]
    public string? DialStatus { get; set; }
    [JsonPropertyName("is_new_caller")]
    public bool IsNewCaller { get; set; }
    [JsonPropertyName("indexed_at")]
    public float IndexedAt { get; set; }
    [JsonPropertyName("agent_insights")]
    public bool AgentInsights { get; set; }
    [JsonPropertyName("duration_period")]
    public Duration_Period? DurationPeriod { get; set; }
    [JsonPropertyName("inbound_rate_center")]
    public Inbound_Rate_Center? InboundRateCenter { get; set; }
    [JsonPropertyName("business_number")]
    public string? BusinessNumber { get; set; }
    [JsonPropertyName("business_label")]
    public string? BusinessLabel { get; set; }
    [JsonPropertyName("receiving_number_id")]
    public int? ReceivingNumberId { get; set; }
    [JsonPropertyName("receiving_number_sid")]
    public string? ReceivingNumberSid { get; set; }
    [JsonPropertyName("billed_amount")]
    public float BilledAmount { get; set; }
    [JsonPropertyName("billed_at")]
    public DateTime BilledAt { get; set; }
    [JsonPropertyName("excluded")]
    public bool Excluded { get; set; }
    [JsonPropertyName("redacted")]
    public bool Redacted { get; set; }
    [JsonPropertyName("tracking_number_format")]
    public string? TrackingNumberFormat { get; set; }
    [JsonPropertyName("business_number_format")]
    public string? BusinessNumberFormat { get; set; }
    [JsonPropertyName("alternative_number")]
    public string? AlternativeNumber { get; set; }
    [JsonPropertyName("tracking_number_bare")]
    public string? TrackingNumberBare { get; set; }
    [JsonPropertyName("caller_number_format")]
    public string? CallerNumberFormat { get; set; }
    [JsonPropertyName("caller_number_complete")]
    public string? CallerNumberComplete { get; set; }
    [JsonPropertyName("caller_number_bare")]
    public string? CallerNumberBare { get; set; }
    [JsonPropertyName("caller_number")]
    public string? CallerNumber { get; set; }
    [JsonPropertyName("caller_number_split")]
    public string[]? CallerNumberSplit { get; set; }
    [JsonPropertyName("contact_number")]
    public string? ContactNumber { get; set; }
    [JsonPropertyName("visitor")]
    public bool Visitor { get; set; }
    [JsonPropertyName("call_path")]
    public Call_Path[]? CallPath { get; set; }
    [JsonPropertyName("left_talk_time")]
    public int? LeftTalkTime { get; set; }
    [JsonPropertyName("right_talk_time")]
    public int? RightTalkTime { get; set; }
    [JsonPropertyName("transfers")]
    public object[]? Transfers { get; set; }
    [JsonPropertyName("call_status")]
    public string? CallStatus { get; set; }
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    [JsonPropertyName("spotted")]
    public object[]? Spotted { get; set; }
    [JsonPropertyName("salesforce")]
    public Salesforce? Salesforce { get; set; }
    [JsonPropertyName("audio")]
    public string? Audio { get; set; }
    [JsonPropertyName("is_s3_link")]
    public bool IsS3Link { get; set; }
    [JsonPropertyName("callbacks")]
    public object[]? Callbacks { get; set; }
    [JsonPropertyName("emails")]
    public object[]? Emails { get; set; }
    [JsonPropertyName("day")]
    public string? Day { get; set; }
    [JsonPropertyName("month")]
    public string? Month { get; set; }
    [JsonPropertyName("hour")]
    public string? Hour { get; set; }
    [JsonPropertyName("paid")]
    public Paid? Paid { get; set; }
    [JsonPropertyName("ga")]
    public Ga? Ga { get; set; }
    [JsonPropertyName("tag_list")]
    public object[]? TagList { get; set; }
    [JsonPropertyName("latitude")]
    public float Latitude { get; set; }
    [JsonPropertyName("longitude")]
    public float Longitude { get; set; }
    [JsonPropertyName("extended_lookup_on")]
    public bool ExtendedLookupOn { get; set; }
    [JsonPropertyName("legs")]
    public object[]? Legs { get; set; }
    [JsonPropertyName("touchpoints")]
    public Touchpoint[]? Touchpoints { get; set; }
    [JsonPropertyName("last_touch")]
    public string? LastTouch { get; set; }
    [JsonPropertyName("geo")]
    public Geo? Geo { get; set; }
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}
public class Duration_Period
{
    [JsonPropertyName("gte")]
    public long Gte { get; set; }
    [JsonPropertyName("lte")]
    public long Lte { get; set; }
}
public class Inbound_Rate_Center
{
    [JsonPropertyName("country")]
    public string? Country { get; set; }
    [JsonPropertyName("prefix")]
    public string? Prefix { get; set; }
    [JsonPropertyName("tollfree")]
    public bool Tollfree { get; set; }
}
public class Salesforce
{
}
public class Paid
{
    [JsonPropertyName("source")]
    public object? Source { get; set; }
    [JsonPropertyName("medium")]
    public object? Medium { get; set; }
}
public class Ga
{
    [JsonPropertyName("cid")]
    public string? Cid { get; set; }
    [JsonPropertyName("ga4")]
    public object[]? Ga4 { get; set; }
}
public class Geo
{
    [JsonPropertyName("lon")]
    public float Lon { get; set; }
    [JsonPropertyName("lat")]
    public float Lat { get; set; }
}
public class Call_Path
{
    [JsonPropertyName("route_name")]
    public string? RouteName { get; set; }
    [JsonPropertyName("route_id")]
    public string? RouteId { get; set; }
    [JsonPropertyName("route_type")]
    public string? RouteType { get; set; }
    [JsonPropertyName("started_at")]
    public DateTime StartedAt { get; set; }
}
public class Touchpoint
{
    [JsonPropertyName("call_id")]
    public long CallId { get; set; }
    [JsonPropertyName("time")]
    public DateTime Time { get; set; }
    [JsonPropertyName("touch_type")]
    public string? TouchType { get; set; }
    [JsonPropertyName("clickid")]
    public object? Clickid { get; set; }
    [JsonPropertyName("type")]
    public object? Type { get; set; }
    [JsonPropertyName("source")]
    public string? Source { get; set; }
    [JsonPropertyName("medium")]
    public object? Medium { get; set; }
    [JsonPropertyName("position")]
    public string? Position { get; set; }
    [JsonPropertyName("weight")]
    public float Weight { get; set; }
}
public class Sort
{
    [JsonPropertyName("unix_time")]
    public Unix_Time? UnixTime { get; set; }
    [JsonPropertyName("id")]
    public Id? Id { get; set; }
}
public class Unix_Time
{
    [JsonPropertyName("order")]
    public string? Order { get; set; }
    [JsonPropertyName("missing")]
    public string? Missing { get; set; }
}
public class Id
{
    [JsonPropertyName("order")]
    public string? Order { get; set; }
}

