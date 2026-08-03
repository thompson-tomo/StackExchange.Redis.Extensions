// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace StackExchange.Redis.Extensions.Core.Extensions;

internal static class ValueLengthExtensions
{
    public static KeyValuePair<RedisKey, RedisValue>[] ToRedisEntries<T>(this Tuple<string, T>[] items, ISerializer serializer, uint maxValueLength)
    {
        var result = new KeyValuePair<RedisKey, RedisValue>[items.Length];
        var count = 0;

        foreach (var item in items)
        {
            if (item == null)
                continue;

            result[count++] = new(item.Item1, item.Item2.SerializeItem(serializer).CheckLength(maxValueLength, item.Item1));
        }

        if (count != result.Length)
            Array.Resize(ref result, count);

        return result;
    }

    public static byte[] OfValueSize<T>(this T? value, ISerializer serializer, uint maxValueLength, string key)
    {
        return value == null
            ? []
            : serializer.Serialize(value).CheckLength(maxValueLength, key);
    }

    private static byte[] SerializeItem<T>(this T? item, ISerializer serializer)
    {
        return item == null
            ? []
            : serializer.Serialize(item);
    }

    private static byte[] CheckLength(this byte[] byteArray, uint maxValueLength, string paramName)
    {
        if (maxValueLength > default(uint) && byteArray.Length > maxValueLength)
            throw new ArgumentException("value cannot be longer than the MaxValueLength", paramName);

        return byteArray;
    }
}
