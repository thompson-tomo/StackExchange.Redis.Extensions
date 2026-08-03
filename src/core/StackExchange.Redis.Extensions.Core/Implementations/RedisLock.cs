// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Core.Abstractions;

namespace StackExchange.Redis.Extensions.Core.Implementations;

/// <summary>
///     A distributed lock backed by Redis that automatically releases on dispose.
/// </summary>
internal sealed class RedisLock : IRedisLock
{
    private readonly IDatabase database;
    private bool isDisposed;

    internal RedisLock(IDatabase database, string key, string value, bool isAcquired)
    {
        this.database = database;
        Key = key;
        Value = value;
        IsAcquired = isAcquired;
    }

    /// <inheritdoc/>
    public string Key { get; }

    /// <inheritdoc/>
    public string Value { get; }

    /// <inheritdoc/>
    public bool IsAcquired { get; }

    /// <inheritdoc/>
    public Task<bool> ExtendAsync(TimeSpan expiry, CommandFlags flag = CommandFlags.None)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(RedisLock));

        if (!IsAcquired)
            return Task.FromResult(false);

        return database.LockExtendAsync(Key, Value, expiry, flag);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (isDisposed)
            return;

        isDisposed = true;

        if (IsAcquired)
            await database.LockReleaseAsync(Key, Value).ConfigureAwait(false);
    }
}
