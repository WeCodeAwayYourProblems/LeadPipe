using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.MySql.Context;
using LeadPipe.Infrastructure.MySql.Repository;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.MySql;

public class CaliperMySqlRepositoryTests
{
    private static CaliperMySqlRepository CreateRepo(out MySqlSchema2Context context)
    {
        context = DbContextTestFactory.CreateTestContext<MySqlSchema2Context>(nameof(CaliperMySqlRepositoryTests));
        return new CaliperMySqlRepository(context);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Entity_With_Details_When_Requested()
    {
        CaliperMySqlRepository repo = CreateRepo(out var ctx);

        var entity = new CaliperMySqlEntity
        {
            call_id = 1,
            summaries = new List<SummaryMySqlEntity> { new() { call_id = 1 } },
            transcriptions = new List<TranscriptionMySqlEntity> { new() { call_id = 1 } }
        };

        ctx.Add(entity);
        await ctx.SaveChangesAsync();

        Result<CaliperMySqlEntity> result = await repo.GetByIdAsync(1, includeDetails: true);

        ResultAssertions.ShouldBeSuccess(result);
        Assert.NotEmpty(result.Value!.summaries);
        Assert.NotEmpty(result.Value!.transcriptions);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Entity_Without_Details_When_Disabled()
    {
        CaliperMySqlRepository repo = CreateRepo(out var ctx);

        ctx.Add(new CaliperMySqlEntity { call_id = 2 });
        await ctx.SaveChangesAsync();

        Result<CaliperMySqlEntity> result = await repo.GetByIdAsync(2, includeDetails: false);

        ResultAssertions.ShouldBeSuccess(result);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Failure_When_Not_Found()
    {
        CaliperMySqlRepository repo = CreateRepo(out _);

        var result = await repo.GetByIdAsync(999);

        ResultAssertions.ShouldBeFailure(result);
    }

    [Fact]
    public async Task FindAsync_Filters_Correctly()
    {
        CaliperMySqlRepository repo = CreateRepo(out var ctx);

        ctx.AddRange(
            new CaliperMySqlEntity { call_id = 1 },
            new CaliperMySqlEntity { call_id = 2 }
        );
        await ctx.SaveChangesAsync();

        var result = await repo.FindAsync(c => c.call_id == 2);

        ResultAssertions.ShouldBeSuccess(result);
        Assert.Single(result.Value);
        Assert.Equal(2, result.Value[0].call_id);
    }
}
