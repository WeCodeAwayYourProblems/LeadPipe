using LeadPipe.Infrastructure.MySql.Context;
using LeadPipe.Infrastructure.MySql.Repository;
using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.MySql;

public class CustomerMySqlRepositoryTests
{
    private CustomerMySqlRepository CreateRepository()
    {
        var settings = Substitute.For<IMySqlSettings>();
        settings.Schema1.Returns("dbo");
        settings.Schema2.Returns("dbo");

        var context = new MySqlSchemaContext(
            new DbContextOptionsBuilder<MySqlSchemaContext>()
                .UseInMemoryDatabase(nameof(CustomerMySqlRepositoryTests))
                .Options,
            settings);
        return new CustomerMySqlRepository(context);
    }

   
}
