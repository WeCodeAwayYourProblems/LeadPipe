using LeadPipe.Infrastructure.MySql.Context;
using LeadPipe.Infrastructure.MySql.Repository;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.MySql;

public class SandMySqlRepositoryTests
{
    private SandMySqlRepository CreateRepository()
    {
        var settings = Substitute.For<IMySqlSettings>();
        settings.Schema1.Returns("dbo");
        settings.Schema2.Returns("dbo");

        var context = new MySqlSchema1Context(
            new DbContextOptionsBuilder<MySqlSchema1Context>()
                .UseInMemoryDatabase(nameof(SandMySqlRepositoryTests))
                .Options,
            settings);
        return new SandMySqlRepository(context);
    }

    
}
