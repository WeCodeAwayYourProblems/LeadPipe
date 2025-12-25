using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class SubsRepository(PlumbingContext context) : PlumbingContextRepository<SubsEntity>(context), ISubsRepository
{
    public override async Task<Result<List<SubsEntity>>> UpsertRangeAsync(List<SubsEntity> entities)
    {
        if (entities.Count == 0)
            return Result.Success(new List<SubsEntity>());

        // Deduplicate in-memory by Number
        List<SubsEntity> uniqueEntities = [.. entities
            .GroupBy(e => e.Number)
            .Select(g => g.Last())];

        const int parametersPerRow = 19;
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
                    "INSERT INTO SubsEntities " +
                    "(CustomerId, Date, UnixDate, SubDate, UnixSubDate, Number, Number2, CancelDate, UnixCancelDate, SubCancelDate, UnixSubCancelDate, Active, SubActive, Complete, Value, Type, Seller, Seller2, Seller3) VALUES ");

                List<SqliteParameter> parameters = new List<SqliteParameter>();
                for (int i = 0; i < batch.Count; i++)
                {
                    SubsEntity e = batch[i];
                    sqlBuilder.Append(
                        "(" +
                        $"@CustomerId{i}, @Date{i}, @UnixDate{i}, @SubDate{i}, @UnixSubDate{i}, @Number{i}, @Number2{i}, @CancelDate{i}, @UnixCancelDate{i}, @SubCancelDate{i}, @UnixSubCancelDate{i}, @Active{i}, @SubActive{i}, @Complete{i}, @Value{i}, @Type{i}, @Seller{i}, @Seller2{i}, @Seller3{i}" +
                        ")");
                    if (i < batch.Count - 1)
                        sqlBuilder.Append(", ");

                    parameters.AddRange(
                    [
                        new SqliteParameter($"@CustomerId{i}", e.CustomerId),
                        new SqliteParameter($"@Date{i}", e.Date),
                        new SqliteParameter($"@UnixDate{i}", e.UnixDate),
                        new SqliteParameter($"@SubDate{i}", e.SubDate),
                        new SqliteParameter($"@UnixSubDate{i}", e.UnixSubDate),
                        new SqliteParameter($"@Number{i}", e.Number),
                        new SqliteParameter($"@Number2{i}", e.Number2),
                        new SqliteParameter($"@CancelDate{i}", e.CancelDate),
                        new SqliteParameter($"@UnixCancelDate{i}", e.UnixCancelDate),
                        new SqliteParameter($"@SubCancelDate{i}", e.SubCancelDate),
                        new SqliteParameter($"@UnixSubCancelDate{i}", e.UnixSubCancelDate),
                        new SqliteParameter($"@Active{i}", e.Active),
                        new SqliteParameter($"@SubActive{i}", e.SubActive),
                        new SqliteParameter($"@Complete{i}", e.Complete),
                        new SqliteParameter($"@Value{i}", e.Value),
                        new SqliteParameter($"@Type{i}", (object?)e.Type ?? DBNull.Value),
                        new SqliteParameter($"@Seller{i}", e.Seller),
                        new SqliteParameter($"@Seller2{i}", e.Seller2),
                        new SqliteParameter($"@Seller3{i}", e.Seller3)
                    ]);
                }

                sqlBuilder.AppendLine(
                    " ON CONFLICT(Number) DO UPDATE SET " +
                    "CustomerId = excluded.CustomerId, " +
                    "Date = excluded.Date, " +
                    "UnixDate = excluded.UnixDate, " +
                    "SubDate = excluded.SubDate, " +
                    "UnixSubDate = excluded.UnixSubDate, " +
                    "Number2 = excluded.Number2, " +
                    "CancelDate = excluded.CancelDate, " +
                    "UnixCancelDate = excluded.UnixCancelDate, " +
                    "SubCancelDate = excluded.SubCancelDate, " +
                    "UnixSubCancelDate = excluded.UnixSubCancelDate, " +
                    "Active = excluded.Active, " +
                    "SubActive = excluded.SubActive, " +
                    "Complete = excluded.Complete, " +
                    "Value = excluded.Value, " +
                    "Type = excluded.Type, " +
                    "Seller = excluded.Seller, " +
                    "Seller2 = excluded.Seller2, " +
                    "Seller3 = excluded.Seller3;");

                await _context.Database.ExecuteSqlRawAsync(sqlBuilder.ToString(), parameters);
            }

            await transaction.CommitAsync();

            return Result.Success(uniqueEntities);
        }
        catch (Exception ex) { return Result.Failure<List<SubsEntity>>(ex.Message); }
    }
}
