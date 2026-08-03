// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Core.Abstractions;

/// <summary>
/// The Redis Database
/// </summary>
public partial interface IRedisDatabase
{
    /// <summary>
    ///     Sets or clears the bit at offset in the string value stored at key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="offset">The bit offset.</param>
    /// <param name="value">The bit value (true = 1, false = 0).</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The original bit value stored at offset.</returns>
    Task<bool> StringSetBitAsync(string key, long offset, bool value, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns the bit value at offset in the string value stored at key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="offset">The bit offset.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The bit value stored at offset.</returns>
    Task<bool> StringGetBitAsync(string key, long offset, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Count the number of set bits (population counting) in a string.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="start">The start byte position (inclusive).</param>
    /// <param name="end">The end byte position (inclusive, -1 means end of string).</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The number of bits set to 1.</returns>
    Task<long> StringBitCountAsync(string key, long start = 0, long end = -1, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Performs a bitwise operation between multiple keys and stores the result in the destination key.
    /// </summary>
    /// <param name="operation">The bitwise operation (And, Or, Xor, Not).</param>
    /// <param name="destinationKey">The destination key to store the result.</param>
    /// <param name="keys">The source keys.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The size of the string stored in the destination key (in bytes).</returns>
    Task<long> StringBitOperationAsync(Bitwise operation, string destinationKey, string[] keys, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Return the position of the first bit set to 1 or 0 in a string.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="bit">The bit value to search for.</param>
    /// <param name="start">The start byte position.</param>
    /// <param name="end">The end byte position (-1 means end of string).</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The position of the first bit set to the specified value, or -1 if not found.</returns>
    Task<long> StringBitPositionAsync(string key, bool bit, long start = 0, long end = -1, CommandFlags flag = CommandFlags.None);
}
