namespace LeadPipe.Core;

public static class CollectionExtensions
{
    public static HashSet<TResult> ToHashSetFast<TSource, TResult>(
        this ICollection<TSource> source,
        Func<TSource, TResult> selector)
    {
        HashSet<TResult> result = new(source.Count);
        foreach (var item in source)
            result.Add(selector(item));

        return result;
    }

    public static Dictionary<TKey, TValue> ToDictionaryFast<TSource, TKey, TValue>(
        this ICollection<TSource> source,
        Func<TSource, TKey> key,
        Func<TSource, TValue> value
    ) where TKey : notnull
    {
        Dictionary<TKey, TValue> result = new(source.Count);
        foreach (var item in source)
            result.Add(key(item), value(item));

        return result;
    }

    public static Dictionary<TKey, TSource> ToDictionaryFast<TSource, TKey>(
        this ICollection<TSource> source,
        Func<TSource, TKey> keySelector
    ) where TKey : notnull
    {
        Dictionary<TKey, TSource> result = new(source.Count);
        foreach (var item in source)
            result.Add(keySelector(item), item);

        return result;
    }

}
