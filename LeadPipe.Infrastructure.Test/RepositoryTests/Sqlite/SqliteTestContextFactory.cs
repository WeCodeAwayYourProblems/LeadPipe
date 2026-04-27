using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.Sqlite;

public static class SqliteTestContextFactory
{
    public static PlumbingContext Create(out SqliteConnection connection)
    {
        connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<PlumbingContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;

        var context = new PlumbingContext(options);

        // Required for SQLite in-memory
        context.Database.EnsureCreated();

        return context;
    }
}
