using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.MySql.Context;
using LeadPipe.Infrastructure.MySql.Repository;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace LeadPipe.Infrastructure.Test.RepositoryTests;

public class CallMySqlRepositoryTests
{
    private CallMySqlRepository CreateRepository()
    {
        var settings = Substitute.For<IMySqlSettings>();
        settings.Schema1.Returns("dbo");
        settings.Schema2.Returns("dbo");

        var context = new MySqlContext(
            new DbContextOptionsBuilder<MySqlContext>()
                .UseInMemoryDatabase(nameof(CallMySqlRepositoryTests))
                .Options,
            settings); // Mock settings
        return new CallMySqlRepository(context);
    }

    
}
