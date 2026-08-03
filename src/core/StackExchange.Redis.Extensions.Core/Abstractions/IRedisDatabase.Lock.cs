// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Core.Abstractions;

/// <summary>
/// The Redis Database
/// </summary>
public partial interface IRedisDatabase
{
    /// <summary>
    ///     Takes a lock on the specified key if it is not already held.
    /// </summary>
    /// <param name="key">The lock key.</param>
    /// <param name="value">The value to use as the lock holder identifier.</param>
    /// <param name="expiry">The lock expiry duration.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>True if the lock was successfully taken. False if the key was already locked.</returns>
    Task<bool> LockTakeAsync(string key, string value, TimeSpan expiry, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Releases the lock on the specified key, but only if the current value matches.
    /// </summary>
    /// <param name="key">The lock key.</param>
    /// <param name="value">The lock holder identifier that was used when taking the lock.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>True if the lock was released. False if the lock was not held or the value did not match.</returns>
    Task<bool> LockReleaseAsync(string key, string value, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Extends the lock expiry on the specified key, but only if the current value matches.
    /// </summary>
    /// <param name="key">The lock key.</param>
    /// <param name="value">The lock holder identifier that was used when taking the lock.</param>
    /// <param name="expiry">The new lock expiry duration.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>True if the lock expiry was extended. False if the lock was not held or the value did not match.</returns>
    Task<bool> LockExtendAsync(string key, string value, TimeSpan expiry, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Queries the current holder value of the lock on the specified key.
    /// </summary>
    /// <param name="key">The lock key.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The lock holder value, or null if the key is not locked.</returns>
    Task<string?> LockQueryAsync(string key, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Acquires a distributed lock with automatic retry and returns an <see cref="IRedisLock"/> wrapper.
    ///     The lock is automatically released when the returned object is disposed.
    /// </summary>
    /// <param name="key">The lock key.</param>
    /// <param name="expiry">The lock expiry duration.</param>
    /// <param name="maxRetries">Maximum number of retry attempts to acquire the lock. Default is 3.</param>
    /// <param name="retryDelay">Delay between retry attempts. Default is 200ms.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>An <see cref="IRedisLock"/> instance, or null if the lock could not be acquired after all retries.</returns>
    Task<IRedisLock?> LockAcquireAsync(string key, TimeSpan expiry, int maxRetries = 3, TimeSpan? retryDelay = null, CommandFlags flag = CommandFlags.None);
}
