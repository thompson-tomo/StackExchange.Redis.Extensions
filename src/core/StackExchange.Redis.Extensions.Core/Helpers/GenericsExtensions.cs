using System;
using System.Collections.Generic;
#if NET8_0_OR_GREATER
using System.Runtime.InteropServices;
#endif

namespace StackExchange.Redis.Extensions.Core.Helpers;

internal static class GenericsExtensions
{
    public static TResult[] ToFastArray<TSource, TResult>(this TSource[]? source, Func<TSource, TResult> action)
    {
        if (source is not { Length: > 0 })
            return [];

        var result = new TResult[source.Length];
        for (var i = 0; i < source.Length; i++)
            result[i] = action.Invoke(source[i]);

        return result;
    }

    public static TResult[] ToFastArray<TSource, TResult>(this ICollection<TSource>? source, Func<TSource, TResult> action)
    {
        if (source is null)
            return [];

        if (source is TSource[] sourceArray)
            return sourceArray.ToFastArray(action);

#if NET8_0_OR_GREATER
        // Iterating a List<T> through ICollection<T> would box its struct enumerator; the span avoids both
        // the boxing and the per-item interface dispatch.
        if (source is List<TSource> sourceList)
        {
            var span = CollectionsMarshal.AsSpan(sourceList);

            if (span.Length == 0)
                return [];

            var listResult = new TResult[span.Length];
            for (var i = 0; i < span.Length; i++)
                listResult[i] = action.Invoke(span[i]);

            return listResult;
        }
#endif

        var srcCnt = source.Count;
        if (srcCnt == 0)
            return [];

        var result = new TResult[srcCnt];
        var i2 = 0;
        foreach (var item in source)
        {
            result[i2] = action.Invoke(item);
            i2++;
        }

        return result;
    }
}
