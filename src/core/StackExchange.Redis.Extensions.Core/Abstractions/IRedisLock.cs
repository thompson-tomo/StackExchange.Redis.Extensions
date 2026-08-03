// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Core.Abstractions;

/// <summary>
///     Represents a distributed lock acquired on a Redis key.
///     Disposing the lock releases it automatically.
/// </summary>
public interface IRedisLock : IAsyncDisposable
{
    /// <summary>
    ///     Gets the key of the lock.
    /// </summary>
    string Key { get; }

    /// <summary>
    ///     Gets the unique value identifying this lock holder.
    /// </summary>
    string Value { get; }

    /// <summary>
    ///     Gets a value indicating whether the lock was successfully acquired.
    /// </summary>
    bool IsAcquired { get; }

    /// <summary>
    ///     Extends the lock expiry by the specified duration.
    /// </summary>
    /// <param name="expiry">The new expiry duration from now.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>True if the lock was successfully extended. False if the lock was lost.</returns>
    Task<bool> ExtendAsync(TimeSpan expiry, CommandFlags flag = CommandFlags.None);
}
