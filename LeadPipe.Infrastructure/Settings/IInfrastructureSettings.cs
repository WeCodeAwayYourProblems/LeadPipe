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

    string? CalliReportLoc { get; set; }
    string? CalliSourceLoc { get; set; }

    string? LabReportLoc { get; set; }
    string? LabSourceLoc { get; set; }

    string? LibacionSourceLoc { get; set; }
    string? LibacionReportLoc { get; set; }

    string? LeasedSourceLoc { get; set; }
    string? LeasedReportLoc { get; set; }

    string? PanReportLoc { get; set; }
    string? PanSourceLoc { get; set; }

    string? LatherReportLoc { get; set; }
    string? LatherSourceLoc { get; set; }
    
    string[]? CornSources { get; set; }
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

public class Mysql
{
    public LogLevel LogLevel { get; set; }
    public bool SensitiveLogging { get; set; }
}

public class Sqlite
{
    public LogLevel LogLevel { get; set; }
    public bool UseInMemoryConnection { get; set; }
    public bool SensitiveLogging { get; set; }
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
