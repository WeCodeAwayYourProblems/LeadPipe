using System.Text.Json;
using System.Text.Json.Serialization;

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
    public Item[]? items { get; set; }
    public string? next_page { get; set; }
    public object[]? checked_projects { get; set; }
    public int response_statuses_count { get; set; }
    public int unfiltered_total { get; set; }
}

[JsonConverter(typeof(ItemConverter))]
public class Item
{
    public LabDto? labDto { get; set; }
    public Created_At? created_at { get; set; }
    public Quote? quote { get; set; }
    public string? note { get; set; }
    public Last_Message? last_message { get; set; }
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
    [JsonConverter(typeof(SingleOrArrayConverter<Entities>))]
    public List<Entities>? entities { get; set; }
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
}

public class Interactions
{
    public bool is_shortlisted { get; set; }
    public string? interaction_types { get; set; }
    public int last_interaction { get; set; }
    public string? last_message { get; set; }
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
    public string? telephone_formatted { get; set; }
    public object? business_name { get; set; }
    public string? short_name { get; set; }
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

public class Quote
{
    public object? type { get; set; }
    public object? value { get; set; }
    public string? detail { get; set; }
}

public class Last_Message
{
    public string? type { get; set; }
    public string? label { get; set; }
    public int time { get; set; }
    public bool is_read { get; set; }
    public string? sender { get; set; }
}

#pragma warning restore IDE1006 // Naming Styles

public class SingleOrArrayConverter<T> : JsonConverter<List<T>>
{
    public override List<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.StartArray => JsonSerializer.Deserialize<List<T>>(ref reader, options),
            JsonTokenType.StartObject => StartObjectFunction(ref reader, options),
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Unexpected token {reader.TokenType}")
        };

        static List<T> StartObjectFunction(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            var single = JsonSerializer.Deserialize<T>(ref reader, options);

            if (single == null)
                return [];

            // Optional: filter "empty" objects
            if (EqualityComparer<T>.Default.Equals(single, default!))
                return [];

            return [single];
        }
    }

    public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}

public class ItemConverter : JsonConverter<Item>
{
    public override Item? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        var item = new Item();

        if (root.TryGetProperty(LabDtoAttributeNames.LabDtoAttributeName, out var labElement) ||
            root.TryGetProperty(LabDtoAttributeNames.LabDtoAttributeNamePlural, out labElement))
        {
            item.labDto = labElement.Deserialize<LabDto>(options);
        }

        if (root.TryGetProperty("created_at", out var createdAt))
            item.created_at = createdAt.Deserialize<Created_At>(options);

        if (root.TryGetProperty("quote", out var quote))
            item.quote = quote.Deserialize<Quote>(options);

        if (root.TryGetProperty("note", out var note))
            item.note = note.GetString();

        if (root.TryGetProperty("last_message", out var lastMessage))
            item.last_message = lastMessage.Deserialize<Last_Message>(options);

        return item;
    }

    public override void Write(Utf8JsonWriter writer, Item value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}