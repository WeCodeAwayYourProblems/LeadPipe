namespace LeadPipe.Infrastructure.Dto;

#pragma warning disable IDE1006 // Naming Styles
public class LabHelperDto
{
    public bool status { get; set; }
    public Data? data { get; set; }
    public long PhoneNumber { get; internal set; }
    public DateTime Date { get; internal set; }
}

public class Data
{
    public int total { get; set; }
    public int per_page { get; set; }
    public int current_page { get; set; }
    public int last_page { get; set; }
    public LabDto[]? items { get; set; }
    public string? next_page { get; set; }
    public object[]? checked_projects { get; set; }
    public Filters? filters { get; set; }
    public object[]? customer_orders { get; set; }
    public int unfiltered_total { get; set; }
    public int count { get; set; }
}

public class Filters
{
    public object[]? credits { get; set; }
    public Categories? categories { get; set; }
    public Date[]? dates { get; set; }
}

public class Categories
{
    public string? PestControl { get; set; }
}

public class Date
{
    public string? title { get; set; }
    public string? value { get; set; }
    public bool default_selected { get; set; }
}

public class LabDto
{
    public int id { get; set; }
    public int credits_required { get; set; }
    public int purchase_cap { get; set; }
    public int purchased_count { get; set; }
    public Created_At? created_at { get; set; }
    public Display? display { get; set; }
    public Interactions? interactions { get; set; }
    public Entities? entities { get; set; }
    public Metadata? metadata { get; set; }
    public int project_response_status { get; set; }
    public bool is_urgent { get; set; }
    public bool is_top_opportunity { get; set; }
    public Contact_Preferences? contact_preferences { get; set; }
    public Trusted_Form? trusted_form { get; set; }
    public object? business_name { get; set; }
    public string? response_type { get; set; }
    public string? enquiry_message { get; set; }
    public object? project_source { get; set; }
}

public class Created_At
{
    public string? date_utc { get; set; }
    public string? friendly { get; set; }
}

public class Display
{
    public string? html { get; set; }
    public string? text { get; set; }
    public string? url { get; set; }
}

public class Interactions
{
    public bool is_shortlisted { get; set; }
    public object? interaction_types { get; set; }
    public object? last_interaction { get; set; }
    public object? last_message { get; set; }
}

public class Entities
{
    public Buyer? buyer { get; set; }
}

public class Buyer
{
    public string? name { get; set; }
    public string? email { get; set; }
    public string? telephone { get; set; }
    public object? telephone_formatted { get; set; }
    public object? business_name { get; set; }
}

public class Metadata
{
    public object[]? images { get; set; }
    public Country? country { get; set; }
    public Category? category { get; set; }
    public Questions? questions { get; set; }
    public LabLocation? location { get; set; }
}

public class Country
{
    public int id { get; set; }
    public string? name { get; set; }
}

public class Category
{
    public int id { get; set; }
    public string? name { get; set; }
}

public class Questions
{
    public Datum[]? data { get; set; }
}

public class Datum
{
    public string? question { get; set; }
    public string? type { get; set; }
    public string[]? possible_answers { get; set; }
    public bool is_required { get; set; }
    public bool is_custom_answer { get; set; }
    public string? answer_type { get; set; }
    public object? answer { get; set; }
}

public class LabLocation
{
    public string? name { get; set; }
    public string? latitude { get; set; }
    public string? longitude { get; set; }
    public bool is_local { get; set; }
    public string? postcode { get; set; }
}

public class Contact_Preferences
{
    public bool prefers_contact_by_phone { get; set; }
    public bool prefers_contact_by_email { get; set; }
    public bool prefers_contact_by_sms { get; set; }
}

public class Trusted_Form
{
    public bool has_trusted_form_certificate { get; set; }
    public string? certificate_id { get; set; }
    public string? certificate_url { get; set; }
}

#pragma warning restore IDE1006 // Naming Styles
