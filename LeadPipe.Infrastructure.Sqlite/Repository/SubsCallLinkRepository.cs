using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class SubsCallLinkRepository(PlumbingContext context) : PlumbingContextRepository<CallSubsLink>(context), ISubsCallLinkRepository
{
    public override async Task<Result<List<CallSubsLink>>> UpsertRangeAsync(List<CallSubsLink> entities)
    {
        if (entities.Count == 0)
            return Result.Success(new List<CallSubsLink>());

        // Deduplicate in-memory by (SubsId, CallId)
        List<CallSubsLink> uniqueEntities = [.. entities
            .GroupBy(e => (e.SubsId, e.CallId))
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

            foreach (List<CallSubsLink> batch in batches)
            {
                StringBuilder sqlBuilder = new();
                sqlBuilder.Append(
                    "INSERT INTO CallSubsLinks " +
                    "(SubsId, CallId, MatchingNumber) VALUES ");

                List<SqliteParameter> parameters = new List<SqliteParameter>();
                for (int i = 0; i < batch.Count; i++)
                {
                    CallSubsLink e = batch[i];
                    sqlBuilder.Append($"(@SubsId{i}, @CallId{i}, @MatchingNumber{i})");
                    if (i < batch.Count - 1)
                        sqlBuilder.Append(", ");

                    parameters.AddRange(
                    [
                        new SqliteParameter($"@SubsId{i}", e.SubsId),
                        new SqliteParameter($"@CallId{i}", e.CallId),
                        new SqliteParameter($"@MatchingNumber{i}", e.MatchingNumber)
                    ]);
                }

                sqlBuilder.AppendLine(
                    " ON CONFLICT(SubsId, CallId) DO UPDATE SET " +
                    "MatchingNumber = excluded.MatchingNumber;");

                await _context.Database.ExecuteSqlRawAsync(sqlBuilder.ToString(), parameters);
            }

            await transaction.CommitAsync();

            return Result.Success(uniqueEntities);
        }
        catch (Exception ex) { return Result.Failure<List<CallSubsLink>>(ex.Message); }
    }
}
