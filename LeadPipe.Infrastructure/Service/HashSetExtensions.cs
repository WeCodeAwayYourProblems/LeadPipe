namespace LeadPipe.Infrastructure.Service;

public static class HashSetExtensions
{
    public static HashSet<TResult> ToHashSetFast<TSource, TResult>(
        this ICollection<TSource> source,
        Func<TSource, TResult> selector)
    {
        HashSet<TResult> result = new(source.Count);
        foreach (TSource? item in source)
            result.Add(selector(item));

        return result;
    }
}