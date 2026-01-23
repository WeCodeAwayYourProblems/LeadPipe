using LeadPipe.Infrastructure.MySql.Context;
using LeadPipe.Infrastructure.MySql.Repository;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.MySql;

public static class DbContextTestFactory
{
    private static IMySqlSettings Settings
    {
        get
        {
            var s = Substitute.For<IMySqlSettings>();

            s.Schema1.Returns("Schema1");
            s.Schema2.Returns("Schema2");
            s.Schema3.Returns("Schema3");

            s.CornTableName.Returns("Corn");
            s.CustardTableName.Returns("Custard");
            s.SandTableName.Returns("Sand");

            return s;
        }
    }

    public static TContext CreateTestContext<TContext>(string dbName)
        where TContext : DbContext
    {
        // Build options for TContext
        var contextOptions = new DbContextOptionsBuilder<TContext>()
            .UseInMemoryDatabase(dbName)
            .EnableSensitiveDataLogging()
            .Options;

        return (TContext)Activator.CreateInstance(typeof(TContext), contextOptions, Settings, true)!;
    }

}
