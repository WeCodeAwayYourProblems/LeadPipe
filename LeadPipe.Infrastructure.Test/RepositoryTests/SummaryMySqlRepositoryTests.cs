using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.MySql.Context;
using LeadPipe.Infrastructure.MySql.Repository;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace LeadPipe.Infrastructure.Test.RepositoryTests;

public class SummaryMySqlRepositoryTests
{
    private SummaryMySqlRepository CreateRepository()
    {
        var settings = Substitute.For<IMySqlSettings>();
        settings.Schema1.Returns("dbo");
        settings.Schema2.Returns("dbo");

        var context = new MySqlSchema2Context(
            new DbContextOptionsBuilder<MySqlSchema2Context>()
                .UseInMemoryDatabase(nameof(SummaryMySqlRepositoryTests))
                .Options,
            settings);
        return new SummaryMySqlRepository(context);
    }

    
}
