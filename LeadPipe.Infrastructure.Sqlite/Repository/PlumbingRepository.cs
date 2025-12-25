using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class PlumbingRepository(PlumbingContext context, ILogger<PlumbingRepository> logger) 
    : PlumbingContextRepository<PlumbingEntity>(context), IPlumbingRepository
{
    private readonly ILogger<PlumbingRepository> _logger = logger;
    public async Task<Result<List<PlumbingEntity>>> GetAllAsync(Source source)
    {
        try
        {
            List<PlumbingEntity> plumbing = await _set
                .Where(p => p.Source == source)
                .ToListAsync();
            return Result.Success(plumbing);
        }
        catch (Exception ex) { return Result.Failure<List<PlumbingEntity>>(ex.Message); }
    }
    public override async Task<Result<List<PlumbingEntity>>> AddRangeAsync(List<PlumbingEntity> entities)
    {
        if (entities == null || entities.Count == 0)
            return Result.Failure<List<PlumbingEntity>>("No entities provided.");

        try
        {
            // Sort entities
            List<PlumbingEntity> sortedEntities = [.. entities.OrderBy(e => e.Date)];

            // Deduplicate input entities
            HashSet<(long, Source)> seenKeys = [];
            List<PlumbingEntity> uniqueEntities = [];
            uniqueEntities.AddRange(
                from e in sortedEntities
                let key = (e.PhoneNumber, e.Source)
                where seenKeys.Add(key) // This will be true only for the first key, so no duplicates are added
                select e);

            // Extract numbers from unique set
            HashSet<long> phoneNumbers = [.. uniqueEntities.Select(e => e.PhoneNumber)];

            // Query based on unique phone numbers
            var existing = await _set
                .Where(p => phoneNumbers.Contains(p.PhoneNumber))
                .Select(p => new { p.PhoneNumber, p.Source })
                .ToListAsync();

            // Now finish the composite match in memory
            HashSet<(long PhoneNumber, Source Source)> existingSet = [.. existing.Select(x => (x.PhoneNumber, x.Source))];

            List<PlumbingEntity> toInsert = [.. uniqueEntities.Where(e => !existingSet.Contains((e.PhoneNumber, e.Source)))];

            if (toInsert.Count == 0)
                return Result.Success(new List<PlumbingEntity>());

            await _set.AddRangeAsync(toInsert);
            await _context.SaveChangesAsync();

            _logger.LogDebug(
                "Plumbing bulk insert: {Inserted}/{Total}",
                toInsert.Count,
                entities.Count);

            return Result.Success(toInsert);
        }
        catch (Exception ex) { return Result.Failure<List<PlumbingEntity>>($"Failed to save Plumbing entities: {ex.Message}"); }
    }

    public override async Task<Result<List<PlumbingEntity>>> UpsertRangeAsync(List<PlumbingEntity> entities)
    {
        if (entities == null || entities.Count == 0)
            return Result.Success(new List<PlumbingEntity>());

        // Deduplicate in-memory by (PhoneNumber, Source)
        List<PlumbingEntity> uniqueEntities = [.. entities
            .GroupBy(e => (e.PhoneNumber, e.Source))
            .Select(g => g.Last())];

        const int parametersPerRow = 6;
        const int batchSize = 999 / parametersPerRow; // Max rows per batch
        var batches = uniqueEntities
            .Select((e, i) => new { e, i })
            .GroupBy(x => x.i / batchSize)
            .Select(g => g.Select(x => x.e).ToList())
            .ToList();

        try
        {
            await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();

            foreach (List<PlumbingEntity> batch in batches)
            {
                StringBuilder sqlBuilder = new();
                sqlBuilder.Append(
                    "INSERT INTO PlumbingEntities " +
                    "(PhoneNumber, Date, UnixDate, Contents, Source, MetaData) VALUES ");

                List<SqliteParameter> parameters = [];
                for (int i = 0; i < batch.Count; i++)
                {
                    PlumbingEntity e = batch[i];
                    sqlBuilder.Append(
                        $"(@PhoneNumber{i}, @Date{i}, @UnixDate{i}, @Contents{i}, @Source{i}, @MetaData{i})"
                        );
                    if (i < batch.Count - 1)
                        sqlBuilder.Append(", ");

                    parameters.AddRange(
                    [
                        new SqliteParameter($"@PhoneNumber{i}", e.PhoneNumber),
                        new SqliteParameter($"@Date{i}", e.Date),
                        new SqliteParameter($"@UnixDate{i}", e.UnixDate),
                        new SqliteParameter($"@Contents{i}", (object?)e.Contents ?? DBNull.Value),
                        new SqliteParameter($"@Source{i}", e.Source.ToString()),
                        new SqliteParameter($"@MetaData{i}", e.MetaData)
                    ]);
                }

                sqlBuilder.AppendLine(
                    " ON CONFLICT(PhoneNumber, Source) DO UPDATE SET " +
                    "Date = excluded.Date, " +
                    "UnixDate = excluded.UnixDate, " +
                    "Contents = excluded.Contents, " +
                    "MetaData = excluded.MetaData;");

                await _context.Database.ExecuteSqlRawAsync(sqlBuilder.ToString(), parameters);
            }

            await transaction.CommitAsync();

            _logger.LogDebug("Plumbing upsert completed: Total={Total}, Unique={Unique}", entities.Count, uniqueEntities.Count);

            return Result.Success(uniqueEntities);
        }
        catch (Exception ex) { return Result.Failure<List<PlumbingEntity>>(ex.Message); }
    }
}
