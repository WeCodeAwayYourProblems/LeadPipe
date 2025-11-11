namespace LeadPipe.Domain.Dto;

public class LeafDto
{
#pragma warning disable IDE1006 // Naming Styles
    public string? uuid { get; set; }
    public string? profile { get; set; }
    public string? category { get; set; }
    public bool read { get; set; }
    public bool spam { get; set; }
    public string? state { get; set; }
    public string[]? channels { get; set; }
    public DateTime creation { get; set; }
    public DateTime modification { get; set; }
    public bool isCallRequest { get; set; }
    public object[]? tags { get; set; }
    public Prospect? prospect { get; set; }
    public Assignee? assignee { get; set; }
    public Message[]? messages { get; set; }
    public DateTime reminder { get; set; }
}

public class Prospect
{
    public string? uuid { get; set; }
    public string? first_name { get; set; }
    public string? last_name { get; set; }
    public string? cellphone { get; set; }
    public DateTime creation { get; set; }
    public DateTime modification { get; set; }
    public bool consent { get; set; }
    public bool blocked { get; set; }
    public object[]? metadata { get; set; }
    public string? customer { get; set; }
    public string[]? profiles { get; set; }
}

public class Assignee
{
    public string? uuid { get; set; }
    public string? first_name { get; set; }
    public string? last_name { get; set; }
}

public class Message
{
    public string? uuid { get; set; }
    public object? message { get; set; }
    public string? state { get; set; }
    public string? type { get; set; }
    public bool auto_reply { get; set; }
    public DateTime creation { get; set; }
    public DateTime modification { get; set; }
    public DateTime sent { get; set; }
    public string? profile { get; set; }
    public string? thread { get; set; }
    public Sender? sender { get; set; }
    public string? direction { get; set; }
    public string? source { get; set; }
    public string? error_description { get; set; }
    public Medium[]? media { get; set; }
}

public class Sender
{
    public string? uuid { get; set; }
    public string? first_name { get; set; }
    public string? last_name { get; set; }
}

public class Medium
{
    public string? path { get; set; }
    public string? url { get; set; }
    public string? type { get; set; }
}

#pragma warning restore IDE1006 // Naming Styles