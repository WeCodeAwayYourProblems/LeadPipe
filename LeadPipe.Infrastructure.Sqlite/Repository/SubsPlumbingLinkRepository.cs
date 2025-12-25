using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class SubsPlumbingLinkRepository(PlumbingContext context) : PlumbingContextRepository<SubsPlumbingLink>(context), ISubsPlumbingLinkRepository
{
    public override async Task<Result<List<SubsPlumbingLink>>> UpsertRangeAsync(List<SubsPlumbingLink> entities)
    {
        if (entities.Count == 0)
            return Result.Success(new List<SubsPlumbingLink>());

        // Deduplicate in-memory by (SubsId, PlumbingId)
        List<SubsPlumbingLink> uniqueEntities = [.. entities
                .GroupBy(e => (e.SubsId, e.PlumbingId))
                .Select(g => g.Last())];

        const int parametersPerRow = 3;
        const int batchSize = 999 / parametersPerRow; // Max rows per batch
        var batches = uniqueEntities
            .Select((e, i) => new { e, i })
            .GroupBy(x => x.i / batchSize)
            .Select(g => g.Select(x => x.e).ToList())
            .ToList();

        try
        {
            await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();

            foreach (var batch in batches)
            {
                StringBuilder sqlBuilder = new();
                sqlBuilder.Append(
                    "INSERT INTO SubsPlumbingLinks " +
                    "(SubsId, PlumbingId, MatchingSubPhone) VALUES ");

                List<SqliteParameter> parameters = [];
                for (int i = 0; i < batch.Count; i++)
                {
                    SubsPlumbingLink e = batch[i];
                    sqlBuilder.Append($"(@SubsId{i}, @PlumbingId{i}, @MatchingSubPhone{i})");
                    if (i < batch.Count - 1)
                        sqlBuilder.Append(", ");

                    parameters.AddRange(
                    [
                        new SqliteParameter($"@SubsId{i}", e.SubsId),
                        new SqliteParameter($"@PlumbingId{i}", e.PlumbingId),
                        new SqliteParameter($"@MatchingSubPhone{i}", e.MatchingSubPhone)
                    ]);
                }

                sqlBuilder.AppendLine(
                    " ON CONFLICT(SubsId, PlumbingId) DO UPDATE SET " +
                    "MatchingSubPhone = excluded.MatchingSubPhone;");

                await _context.Database.ExecuteSqlRawAsync(sqlBuilder.ToString(), parameters);
            }

            await transaction.CommitAsync();

            return Result.Success(uniqueEntities);
        }
        catch (Exception ex) { return Result.Failure<List<SubsPlumbingLink>>(ex.Message); }
    }

    public async Task<Result<List<SubsPlumbingLink>>> GetAllAsync(IEnumerable<PlumbingEntity> filter)
    {
        try
        {
            HashSet<long> ids = [.. filter.Select(p => p.Id)];
            List<SubsPlumbingLink> set = await _set
                .Where(e => ids.Contains(e.PlumbingId))
                .ToListAsync();
            return Result.Success(set);
        }
        catch (Exception ex) { return Result.Failure<List<SubsPlumbingLink>>(ex.Message); }
    }
}
