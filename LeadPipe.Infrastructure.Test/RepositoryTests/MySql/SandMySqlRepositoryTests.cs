using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.MySql.Context;
using LeadPipe.Infrastructure.MySql.Repository;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.MySql;

public class SandMySqlRepositoryTests
{
    private static SandMySqlRepository CreateRepo(out MySqlSchema1Context context, string name)
    {
        context = DbContextTestFactory.CreateTestContext<MySqlSchema1Context>(
            nameof(SandMySqlRepositoryTests) + name);
        return new SandMySqlRepository(context);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Entity_When_Found()
    {
        var repo = CreateRepo(out var ctx, nameof(GetByIdAsync_Returns_Entity_When_Found));

        var entity = new SandMySqlEntity { subscriptionID = 1 };
        ctx.Add(entity);
        await ctx.SaveChangesAsync();

        var result = await repo.GetByIdAsync(1, false);

        ResultAssertions.ShouldBeSuccess(result);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Failure_When_Not_Found()
    {
        var repo = CreateRepo(out _, nameof(GetByIdAsync_Returns_Failure_When_Not_Found));
        var result = await repo.GetByIdAsync(999);
        ResultAssertions.ShouldBeFailure(result);
    }

    [Fact]
    public async Task FindAsync_Returns_Filtered_List()
    {
        var repo = CreateRepo(out var ctx, nameof(FindAsync_Returns_Filtered_List));

        ctx.AddRange(
            new SandMySqlEntity { subscriptionID = 1 },
            new SandMySqlEntity { subscriptionID = 2 }
        );
        await ctx.SaveChangesAsync();

        var result = await repo.FindAsync(c => c.subscriptionID == 2, false);

        ResultAssertions.ShouldBeSuccess(result);
        Assert.Single(result.Value!);
        Assert.Equal(2, result.Value[0].subscriptionID);
    }
}
