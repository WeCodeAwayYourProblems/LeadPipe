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

    [Fact]
    public async Task AddAsync_ShouldAddEntity()
    {
        var repo = CreateRepository();
        var entity = new CallMySqlEntity { call_id = 1 };

        var result = await repo.AddAsync(entity);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.call_id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity()
    {
        var repo = CreateRepository();
        var entity = new CallMySqlEntity { call_id = 2 };
        await repo.AddAsync(entity);

        var result = await repo.GetByIdAsync(2);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.call_id);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntity()
    {
        var repo = CreateRepository();
        var entity = new CallMySqlEntity { call_id = 3 };
        await repo.AddAsync(entity);

        var deleteResult = await repo.DeleteAsync(entity.call_id);

        Assert.True(deleteResult.IsSuccess);
        Assert.True(deleteResult.Value);
    }
}
