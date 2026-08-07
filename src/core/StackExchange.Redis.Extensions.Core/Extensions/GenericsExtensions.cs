using System;
using System.Collections.Generic;

namespace StackExchange.Redis.Extensions.Core.Extensions;

internal static class GenericsExtensions
{
    public static TResult[] ToFastArray<TSource, TResult>(this ReadOnlySpan<TSource> source, Func<TSource, TResult> action)
    {
        if (source.IsEmpty)
            return [];

        var result = new TResult[source.Length];
        for (var i = 0; i < source.Length; i++)
            result[i] = action.Invoke(source[i]);

        return result;
    }

    public static TResult[] ToFastArray<TSource, TResult>(this ICollection<TSource>? source, Func<TSource, TResult> action)
    {
        if (source is not { Count: > 0 })
            return [];

        if (source is TSource[] sourceArray)
            return sourceArray.AsSpan().ToFastArray(action);

        if (source is IList<TSource> sourceList)
        {
            var listResult = new TResult[sourceList.Count];
            for (var i = 0; i < listResult.Length; i++)
                listResult[i] = action.Invoke(sourceList[i]);

            return listResult;
        }

        var result = new TResult[source.Count];
        var index = 0;
        foreach (var item in source)
            result[index++] = action.Invoke(item);

        return result;
    }
}
