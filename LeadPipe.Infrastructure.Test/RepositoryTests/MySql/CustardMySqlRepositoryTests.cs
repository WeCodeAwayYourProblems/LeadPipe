using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.MySql.Context;
using LeadPipe.Infrastructure.MySql.Repository;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.MySql;

public class CustardMySqlRepositoryTests
{
    private static CustardMySqlRepository CreateRepo(out MySqlSchema1Context context)
    {
        context = DbContextTestFactory.CreateTestContext<MySqlSchema1Context>(
            nameof(CustardMySqlRepositoryTests));
        return new CustardMySqlRepository(context);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Entity_When_Found()
    {
        var repo = CreateRepo(out var ctx);

        var entity = new CustardMySqlEntity { customerID = 1 };
        ctx.Add(entity);
        await ctx.SaveChangesAsync();

        var result = await repo.GetByIdAsync(1);

        ResultAssertions.ShouldBeSuccess(result);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Failure_When_Not_Found()
    {
        var repo = CreateRepo(out _);
        var result = await repo.GetByIdAsync(999);
        ResultAssertions.ShouldBeFailure(result);
    }

    [Fact]
    public async Task FindAsync_Returns_Filtered_List()
    {
        var repo = CreateRepo(out var ctx);

        ctx.AddRange(
            new CustardMySqlEntity { customerID = 1 },
            new CustardMySqlEntity { customerID = 2 }
        );
        await ctx.SaveChangesAsync();

        var result = await repo.FindAsync(c => c.customerID == 2);

        ResultAssertions.ShouldBeSuccess(result);
        Assert.Single(result.Value!);
        Assert.Equal(2, result.Value[0].customerID);
    }
}
