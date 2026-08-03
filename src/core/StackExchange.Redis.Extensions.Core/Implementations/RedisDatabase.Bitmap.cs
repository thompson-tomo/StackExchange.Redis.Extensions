// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Core.Helpers;

namespace StackExchange.Redis.Extensions.Core.Implementations;

public partial class RedisDatabase
{
    /// <inheritdoc/>
    public Task<bool> StringSetBitAsync(string key, long offset, bool value, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));
#endif

        return Database.StringSetBitAsync(key, offset, value, flag);
    }

    /// <inheritdoc/>
    public Task<bool> StringGetBitAsync(string key, long offset, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));
#endif

        return Database.StringGetBitAsync(key, offset, flag);
    }

    /// <inheritdoc/>
    public Task<long> StringBitCountAsync(string key, long start = 0, long end = -1, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));
#endif

        return Database.StringBitCountAsync(key, start, end, flag);
    }

    /// <inheritdoc/>
    public Task<long> StringBitOperationAsync(Bitwise operation, string destinationKey, string[] keys, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(destinationKey);
        ArgumentNullException.ThrowIfNull(keys);
#else
        if (string.IsNullOrEmpty(destinationKey))
            throw new ArgumentException("destinationKey cannot be empty.", nameof(destinationKey));

        if (keys == null)
            throw new ArgumentNullException(nameof(keys), "keys cannot be null.");
#endif

        var redisKeys = keys.ToFastArray(k => (RedisKey)k);

        return Database.StringBitOperationAsync(operation, destinationKey, redisKeys, flag);
    }

    /// <inheritdoc/>
    public Task<long> StringBitPositionAsync(string key, bool bit, long start = 0, long end = -1, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));
#endif

        return Database.StringBitPositionAsync(key, bit, start, end, flag);
    }
}
