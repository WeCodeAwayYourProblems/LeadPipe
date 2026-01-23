using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.MySql.Context;
using LeadPipe.Infrastructure.MySql.Repository;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.MySql;

public class TranscriptionMySqlRepositoryTests
{
    private static TranscriptionMySqlRepository CreateRepo(out MySqlSchema2Context context, string name)
    {
        context = DbContextTestFactory.CreateTestContext<MySqlSchema2Context>(
            nameof(TranscriptionMySqlRepositoryTests) + name);
        return new TranscriptionMySqlRepository(context);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Entity_When_Found()
    {
        var repo = CreateRepo(out var ctx, nameof(GetByIdAsync_Returns_Entity_When_Found));

        var entity = new TranscriptionMySqlEntity { call_id = 1 };
        ctx.Add(entity);
        await ctx.SaveChangesAsync();

        var result = await repo.GetByIdAsync(1);

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
            new TranscriptionMySqlEntity { call_id = 1 },
            new TranscriptionMySqlEntity { call_id = 2 }
        );
        await ctx.SaveChangesAsync();

        var result = await repo.FindAsync(c => c.call_id == 2);

        ResultAssertions.ShouldBeSuccess(result);
        Assert.Single(result.Value!);
    }
}
