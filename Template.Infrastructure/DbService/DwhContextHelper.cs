using Microsoft.EntityFrameworkCore;

namespace Template.Infrastructure.DbService;

internal static class DwhContextHelper
{
    internal static async Task<IEnumerable<T>> GetItemsFromFileAsync<T>(DwhContext<T> context, FileInfo query) where T : class
    {
        return await context.Result.FromSqlRaw(File.ReadAllText(query.FullName)).ToListAsync();
    }
    internal static async Task<IEnumerable<T>> GetItemsFromRawAsync<T>(DwhContext<T> context, string query) where T : class
    {
        return await context.Result.FromSqlRaw(query).ToListAsync();
    }
}