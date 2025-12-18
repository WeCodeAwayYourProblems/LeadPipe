using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LeadPipe.Infrastructure.Sqlite.Context;

/// <summary>
/// This is here specifically for adding ef migrations
/// </summary>
public sealed class PlumbingContextFactory : IDesignTimeDbContextFactory<PlumbingContext>
{
    public PlumbingContext CreateDbContext(string[] args)
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true)
            .AddUserSecrets<PlumbingContextFactory>()
            .AddEnvironmentVariables()
            .Build();

        string cs = config.GetConnectionString("Plumbing")
            ?? throw new InvalidOperationException("ConnectionStrings:Plumbing is missing.");

        string dataSource =
            new SqliteConnectionStringBuilder(cs).DataSource;

        Directory.CreateDirectory(Path.GetDirectoryName(dataSource)!);

        var options = new DbContextOptionsBuilder<PlumbingContext>()
            .UseSqlite(cs)
            .Options;

        return new PlumbingContext(options);
    }
}
