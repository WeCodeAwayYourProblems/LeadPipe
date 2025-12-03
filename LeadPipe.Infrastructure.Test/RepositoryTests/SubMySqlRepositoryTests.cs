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

        var context = new MySqlContext(
            new DbContextOptionsBuilder<MySqlContext>()
                .UseInMemoryDatabase(nameof(SubMySqlRepositoryTests))
                .Options,
            settings);
        return new SubMySqlRepository(context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntity()
    {
        var repo = CreateRepository();
        var entity = new SubMySqlEntity { subscriptionID = 1 };

        var result = await repo.AddAsync(entity);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.subscriptionID);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity()
    {
        var repo = CreateRepository();
        var entity = new SubMySqlEntity { subscriptionID = 2 };
        await repo.AddAsync(entity);

        var result = await repo.GetByIdAsync(2);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.subscriptionID);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntity()
    {
        var repo = CreateRepository();
        var entity = new SubMySqlEntity { subscriptionID = 3 };
        await repo.AddAsync(entity);

        var deleteResult = await repo.DeleteAsync(entity.subscriptionID);

        Assert.True(deleteResult.IsSuccess);
        Assert.True(deleteResult.Value);
    }
}
