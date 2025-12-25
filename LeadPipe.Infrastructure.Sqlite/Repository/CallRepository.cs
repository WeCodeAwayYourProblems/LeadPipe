using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CallRepository(PlumbingContext context) : PlumbingContextRepository<CallEntity>(context), ICallRepository
{
    public override async Task<Result<List<CallEntity>>> UpsertRangeAsync(List<CallEntity> entities)
    {
        if (entities.Count == 0)
            return Result.Success(new List<CallEntity>());

        // Deduplicate in-memory by (PhoneNumber, CallDate)
        List<CallEntity> uniqueEntities = [.. entities
            .GroupBy(e => (e.PhoneNumber, e.CallDate))
            .Select(g => g.Last())];

        // SQLite allows batching multiple rows in a single INSERT
        const int parametersPerRow = 8;
        const int batchSize = 999 / parametersPerRow;
        List<List<CallEntity>> batches = [.. uniqueEntities
            .Select((e, i) => new { e, i })
            .GroupBy(x => x.i / batchSize)
            .Select(g => g.Select(x => x.e).ToList())];

        try
        {
            await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();

            foreach (List<CallEntity>? batch in batches)
            {
                // Build SQL for multi-row upsert
                StringBuilder sqlBuilder = new();
                sqlBuilder.Append(
                    "INSERT INTO CallEntities " +
                    "(PhoneNumber, CallDate, UnixCallDate, Note, Source, Location, Duration, Billable) VALUES ");

                List<SqliteParameter> parameters = [];
                for (int i = 0; i < batch.Count; i++)
                {
                    CallEntity e = batch[i];
                    sqlBuilder.Append(
                        $"(@PhoneNumber{i}, @CallDate{i}, @UnixCallDate{i}, @Note{i}, @Source{i}, @Location{i}, @Duration{i}, @Billable{i})");
                    if (i < batch.Count - 1)
                        sqlBuilder.Append(", ");

                    parameters.AddRange(
                    [
                        new SqliteParameter($"@PhoneNumber{i}", e.PhoneNumber),
                        new SqliteParameter($"@CallDate{i}", e.CallDate),
                        new SqliteParameter($"@UnixCallDate{i}", e.UnixCallDate),
                        new SqliteParameter($"@Note{i}", e.Note),
                        new SqliteParameter($"@Source{i}", e.Source),
                        new SqliteParameter($"@Location{i}", e.Location),
                        new SqliteParameter($"@Duration{i}", e.Duration),
                        new SqliteParameter($"@Billable{i}", e.Billable)
                    ]);
                }

                sqlBuilder.AppendLine(
                    " ON CONFLICT(PhoneNumber, CallDate) DO UPDATE SET " +
                    "UnixCallDate=excluded.UnixCallDate, " +
                    "Note=excluded.Note, " +
                    "Source=excluded.Source, " +
                    "Location=excluded.Location, " +
                    "Duration=excluded.Duration, " +
                    "Billable=excluded.Billable;");

                await _context.Database.ExecuteSqlRawAsync(sqlBuilder.ToString(), parameters);
            }

            await transaction.CommitAsync();
            return Result.Success(uniqueEntities);
        }
        catch (Exception ex) { return Result.Failure<List<CallEntity>>(ex.Message); }
    }
}
