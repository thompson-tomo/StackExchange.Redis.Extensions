using System;
using System.Diagnostics.CodeAnalysis;

namespace StackExchange.Redis.Extensions.Core.Helpers;
internal static class ExceptionThrowHelper
{
    public static void ThrowIfExistsNullElement<T>(ReadOnlySpan<T> argument, string paramName)
    {
        foreach (var item in argument)
        {
            if (item is null)
                ThrowNullElementException(paramName);
        }
    }

    [DoesNotReturn]
    private static void ThrowNullElementException(string? paramName)
    {
        throw new ArgumentException("items cannot contains any null item.", paramName);
    }
}
