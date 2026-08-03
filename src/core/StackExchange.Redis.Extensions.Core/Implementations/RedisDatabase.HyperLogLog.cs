// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Core.Helpers;

namespace StackExchange.Redis.Extensions.Core.Implementations;

public partial class RedisDatabase
{
    /// <inheritdoc/>
    public Task<bool> HyperLogLogAddAsync<T>(string key, T value, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (value == null)
            throw new ArgumentNullException(nameof(value), "value cannot be null.");
#endif

        var serializedValue = Serializer.Serialize(value);

        return Database.HyperLogLogAddAsync(key, serializedValue, flag);
    }

    /// <inheritdoc/>
    public Task<bool> HyperLogLogAddAsync<T>(string key, T[] values, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(values);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (values == null)
            throw new ArgumentNullException(nameof(values), "values cannot be null.");
#endif

        var serializedValues = values.ToFastArray(v => (RedisValue)Serializer.Serialize(v));

        return Database.HyperLogLogAddAsync(key, serializedValues, flag);
    }

    /// <inheritdoc/>
    public Task<long> HyperLogLogLengthAsync(string key, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));
#endif

        return Database.HyperLogLogLengthAsync(key, flag);
    }

    /// <inheritdoc/>
    public Task<long> HyperLogLogLengthAsync(string[] keys, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(keys);
#else
        if (keys == null)
            throw new ArgumentNullException(nameof(keys), "keys cannot be null.");
#endif

        var redisKeys = keys.ToFastArray(k => (RedisKey)k);

        return Database.HyperLogLogLengthAsync(redisKeys, flag);
    }

    /// <inheritdoc/>
    public Task HyperLogLogMergeAsync(string destinationKey, string[] sourceKeys, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(destinationKey);
        ArgumentNullException.ThrowIfNull(sourceKeys);
#else
        if (string.IsNullOrEmpty(destinationKey))
            throw new ArgumentException("destinationKey cannot be empty.", nameof(destinationKey));

        if (sourceKeys == null)
            throw new ArgumentNullException(nameof(sourceKeys), "sourceKeys cannot be null.");
#endif

        var redisSourceKeys = sourceKeys.ToFastArray(k => (RedisKey)k);

        return Database.HyperLogLogMergeAsync(destinationKey, redisSourceKeys, flag);
    }
}
