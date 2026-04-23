using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.MySql.Context;
using LeadPipe.Infrastructure.MySql.Repository;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.MySql;

public class CornMySqlRepositoryTests
{
    private static CornMySqlRepository CreateRepo(out MySqlSchema3Context context, string name)
    {
        context = DbContextTestFactory.CreateTestContext<MySqlSchema3Context>(
            nameof(CornMySqlRepositoryTests) + name);
        return new CornMySqlRepository(context);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Entity_When_Found()
    {
        CornMySqlRepository repo = CreateRepo(out var ctx, nameof(GetByIdAsync_Returns_Entity_When_Found));

        var entity = new CornMySqlEntity
        {
            id = 1,
        };

        ctx.Add(entity);
        await ctx.SaveChangesAsync();

        Result<CornMySqlEntity> result = await repo.GetByIdAsync(1);

        ResultAssertions.ShouldBeSuccess(result);
        Assert.Equal(1, result.Value.id);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Failure_When_Not_Found()
    {
        CornMySqlRepository repo = CreateRepo(out _, nameof(GetByIdAsync_Returns_Failure_When_Not_Found));

        var result = await repo.GetByIdAsync(999);

        ResultAssertions.ShouldBeFailure(result);
    }

    [Fact]
    public async Task FindAsync_Returns_Filtered_List()
    {
        CornMySqlRepository repo = CreateRepo(out var ctx, nameof(FindAsync_Returns_Filtered_List));

        ctx.AddRange(
            new CornMySqlEntity { id = 1 },
            new CornMySqlEntity { id = 2 }
        );
        await ctx.SaveChangesAsync();

        var result = await repo.FindAsync(c => c.id == 2);

        ResultAssertions.ShouldBeSuccess(result);
        Assert.Single(result.Value!);
    }

    [Fact]
    public async Task FindAsync_Returns_Empty_List_When_No_Match()
    {
        CornMySqlRepository repo = CreateRepo(out var ctx, nameof(FindAsync_Returns_Empty_List_When_No_Match));

        ctx.AddRange(
            new CornMySqlEntity { id = 1 },
            new CornMySqlEntity { id = 2 }
        );
        await ctx.SaveChangesAsync();

        var result = await repo.FindAsync(c => c.id == 999);

        ResultAssertions.ShouldBeSuccess(result);
        Assert.Empty(result.Value!);
    }
}
