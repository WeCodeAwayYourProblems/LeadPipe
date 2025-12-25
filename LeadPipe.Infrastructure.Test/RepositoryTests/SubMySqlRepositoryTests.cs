using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.MySql.Context;
using LeadPipe.Infrastructure.MySql.Repository;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace LeadPipe.Infrastructure.Test.RepositoryTests;

public class SubMySqlRepositoryTests
{
    private SubMySqlRepository CreateRepository()
    {
        var settings = Substitute.For<IMySqlSettings>();
        settings.Schema1.Returns("dbo");
        settings.Schema2.Returns("dbo");

        var context = new MySqlSchemaContext(
            new DbContextOptionsBuilder<MySqlSchemaContext>()
                .UseInMemoryDatabase(nameof(SubMySqlRepositoryTests))
                .Options,
            settings);
        return new SubMySqlRepository(context);
    }

    
}
