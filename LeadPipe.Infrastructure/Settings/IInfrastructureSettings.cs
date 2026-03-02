using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Settings;

public interface IInfrastructureSettings :
    IDwhSettings,
    ILabSettings,
    ILeafSettings,
    IYellerSettings,
    ICatManSettings,
    ISyncSettings
{
    public Ef? Ef { get; set; }
    public HttpClients? HttpClients { get; set; }

    LocationPair? CalliLoc { get; set; }
    LocationPair? LabLoc { get; set; }
    LocationPair? LibacionLoc { get; set; }
    LocationPair? LeasedLoc { get; set; }
    LocationPair? PanLoc { get; set; }
    LocationPair? LatherLoc { get; set; }

    string[]? CornSources { get; set; }
}
public class LocationPair
{
    public string? Source { get; set; }
    public string? Report { get; set; }
}
public class Ef
{
    public LogLevel LogLevel { get; set; }
    public bool UseInMemoryDatabase { get; set; }
    public bool UseInMemoryConnection { get; set; }
    public bool SensitiveLogging { get; set; }
    public Mysql? MySql { get; set; }
    public Sqlite? Sqlite { get; set; }
}

public abstract class Sql
{
    public LogLevel LogLevel { get; set; }
    public bool SensitiveLogging { get; set; }
}

public sealed class Mysql : Sql
{
    public bool UseInMemoryDatabase { get; set; }
}

public sealed class Sqlite : Sql
{
    public bool UseInMemoryConnection { get; set; }
}

public class HttpClients
{
    public bool UseTestClients { get; set; }
    public Yeller? Yeller { get; set; }
}

public class Yeller
{
    public Getter? Getter { get; set; }
    public Reporter? Reporter { get; set; }
}

public class Getter
{
    public bool UseTestClients { get; set; }
}

public class Reporter
{
    public bool UseTestClients { get; set; }
}
