// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Core.Abstractions;

namespace StackExchange.Redis.Extensions.Core.Implementations;

public partial class RedisDatabase
{
    private static readonly TimeSpan DefaultRetryDelay = TimeSpan.FromMilliseconds(200);

    /// <inheritdoc/>
    public Task<bool> LockTakeAsync(string key, string value, TimeSpan expiry, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(value);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("value cannot be empty.", nameof(value));
#endif

        return Database.LockTakeAsync(key, value, expiry, flag);
    }

    /// <inheritdoc/>
    public Task<bool> LockReleaseAsync(string key, string value, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(value);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("value cannot be empty.", nameof(value));
#endif

        return Database.LockReleaseAsync(key, value, flag);
    }

    /// <inheritdoc/>
    public Task<bool> LockExtendAsync(string key, string value, TimeSpan expiry, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(value);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("value cannot be empty.", nameof(value));
#endif

        return Database.LockExtendAsync(key, value, expiry, flag);
    }

    /// <inheritdoc/>
    public async Task<string?> LockQueryAsync(string key, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));
#endif

        var result = await Database.LockQueryAsync(key, flag).ConfigureAwait(false);

        return result.IsNull ? null : (string?)result;
    }

    /// <inheritdoc/>
    public async Task<IRedisLock?> LockAcquireAsync(string key, TimeSpan expiry, int maxRetries = 3, TimeSpan? retryDelay = null, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));
#endif

        var lockValue = Guid.NewGuid().ToString("N");
        var delay = retryDelay ?? DefaultRetryDelay;

        for (var attempt = 0; attempt <= maxRetries; attempt++)
        {
            if (await Database.LockTakeAsync(key, lockValue, expiry, flag).ConfigureAwait(false))
                return new RedisLock(Database, key, lockValue, true);

            if (attempt < maxRetries)
                await Task.Delay(delay).ConfigureAwait(false);
        }

        return null;
    }
}
