using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.DbService;

internal static class DwhContextHelper
{
    internal static async Task<IEnumerable<T>> GetItemsFromFileAsync<T>(DwhContext<T> context, FileInfo queryLoc) where T : class
    {
        string text = File.ReadAllText(queryLoc.FullName);
        return await GetItemsFromRawAsync(context, text);
    }
    internal static async Task<IEnumerable<T>> GetItemsFromRawAsync<T>(DwhContext<T> context, string rawQuery) where T : class
    {
        return await context.Result.FromSqlRaw(rawQuery).ToListAsync();
    }
}