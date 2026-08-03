// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Core.Abstractions;

/// <summary>
/// The Redis Database
/// </summary>
public partial interface IRedisDatabase
{
    /// <summary>
    ///     Lists the add to left asynchronous.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="item">The item.</param>
    /// <param name="when">The condition (Always is the default value).</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The length of the list after the push operation.</returns>
    Task<long> ListAddToLeftAsync<T>(string key, T item, When when = When.Always, CommandFlags flag = CommandFlags.None)
       ;

    /// <summary>
    ///     Lists the add to left asynchronous.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="items">The items.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The length of the list after the push operation.</returns>
    Task<long> ListAddToLeftAsync<T>(string key, T[] items, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Removes and returns the last element of the list stored at key.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The element being popped, or null when the key does not exist.</returns>
    /// <remarks>
    ///     http://redis.io/commands/rpop
    /// </remarks>
    Task<T?> ListGetFromRightAsync<T>(string key, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Pushes an item to the right of the list.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="item">The item.</param>
    /// <param name="when">The condition (Always is the default value).</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The length of the list after the push operation.</returns>
    Task<long> ListAddToRightAsync<T>(string key, T item, When when = When.Always, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Pushes multiple items to the right of the list.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="items">The items.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The length of the list after the push operation.</returns>
    Task<long> ListAddToRightAsync<T>(string key, T[] items, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Removes and returns the first element of the list stored at key.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The element being popped, or null when the key does not exist.</returns>
    Task<T?> ListGetFromLeftAsync<T>(string key, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns the length of the list stored at key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The length of the list at key.</returns>
    Task<long> ListLengthAsync(string key, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns the specified elements of the list stored at key.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="start">The start index (inclusive, 0-based).</param>
    /// <param name="stop">The stop index (inclusive, -1 means end of list).</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The elements in the specified range.</returns>
    Task<IEnumerable<T?>> ListRangeAsync<T>(string key, long start = 0, long stop = -1, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns the element at index in the list stored at key.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="index">The zero-based index.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The element at the specified index, or null.</returns>
    Task<T?> ListGetByIndexAsync<T>(string key, long index, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Sets the list element at index to value.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="index">The zero-based index.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ListSetByIndexAsync<T>(string key, long index, T value, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Trims an existing list so that it will contain only the specified range of elements.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="start">The start index (inclusive).</param>
    /// <param name="stop">The stop index (inclusive).</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ListTrimAsync(string key, long start, long stop, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Removes the first count occurrences of elements equal to value from the list.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="value">The value to remove.</param>
    /// <param name="count">Number of occurrences to remove (0 = all, positive = from head, negative = from tail).</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The number of removed elements.</returns>
    Task<long> ListRemoveAsync<T>(string key, T value, long count = 0, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Inserts value in the list stored at key before the reference pivot value.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="pivot">The pivot value to insert before.</param>
    /// <param name="value">The value to insert.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The length of the list after the insert, or -1 if the pivot was not found.</returns>
    Task<long> ListInsertBeforeAsync<T>(string key, T pivot, T value, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Inserts value in the list stored at key after the reference pivot value.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="pivot">The pivot value to insert after.</param>
    /// <param name="value">The value to insert.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The length of the list after the insert, or -1 if the pivot was not found.</returns>
    Task<long> ListInsertAfterAsync<T>(string key, T pivot, T value, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Atomically pops an element from the source list and pushes it to the destination list.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="sourceKey">The source list key.</param>
    /// <param name="destinationKey">The destination list key.</param>
    /// <param name="sourceSide">Which side to pop from.</param>
    /// <param name="destinationSide">Which side to push to.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The element being moved, or null if the source list is empty.</returns>
    Task<T?> ListMoveAsync<T>(string sourceKey, string destinationKey, ListSide sourceSide, ListSide destinationSide, CommandFlags flag = CommandFlags.None);
}
