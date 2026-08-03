// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Core.Abstractions;

/// <summary>
/// The Redis Database
/// </summary>
public partial interface IRedisDatabase
{
    /// <summary>
    ///     Adds the specified element to the HyperLogLog data structure stored at the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="value">The value to add.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>True if at least one internal register was altered. False otherwise.</returns>
    Task<bool> HyperLogLogAddAsync<T>(string key, T value, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Adds the specified elements to the HyperLogLog data structure stored at the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="values">The values to add.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>True if at least one internal register was altered. False otherwise.</returns>
    Task<bool> HyperLogLogAddAsync<T>(string key, T[] values, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns the approximated cardinality of the set observed by the HyperLogLog at the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The approximated number of unique elements observed via HyperLogLogAddAsync.</returns>
    Task<long> HyperLogLogLengthAsync(string key, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns the approximated cardinality of the union of the sets observed by the HyperLogLogs at the specified keys.
    /// </summary>
    /// <param name="keys">The keys.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The approximated number of unique elements in the union of the observed sets.</returns>
    Task<long> HyperLogLogLengthAsync(string[] keys, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Merges the HyperLogLogs at the specified source keys into the destination key.
    /// </summary>
    /// <param name="destinationKey">The destination key.</param>
    /// <param name="sourceKeys">The source keys to merge.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HyperLogLogMergeAsync(string destinationKey, string[] sourceKeys, CommandFlags flag = CommandFlags.None);
}
