// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Extensions.Core.Abstractions;

/// <summary>
/// The Redis Database
/// </summary>
public partial interface IRedisDatabase
{
    /// <summary>
    ///     Add the entry to a sorted set with a score
    /// </summary>
    /// <remarks>
    ///     Time complexity: O(1)
    /// </remarks>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">Key of the set</param>
    /// <param name="value">The instance of T.</param>
    /// <param name="score">Score of the entry</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>
    ///     True if the object has been added. Otherwise false
    /// </returns>
    Task<bool> SortedSetAddAsync<T>(string key, T value, double score, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Remove the entry to a sorted set
    /// </summary>
    /// <remarks>
    ///     Time complexity: O(1)
    /// </remarks>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">Key of the set</param>
    /// <param name="value">The instance of T.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>
    ///     True if the object has been removed. Otherwise false
    /// </returns>
    Task<bool> SortedSetRemoveAsync<T>(string key, T value, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Get entries from sorted-set ordered
    /// </summary>
    /// <remarks>
    ///     Time complexity: O(log(N)+M) with N being the number of elements in the sorted set and M the number of elements being returned. If M is constant (e.g. always asking for the first 10 elements with LIMIT), you can consider it O(log(N)
    /// </remarks>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">Key of the set</param>
    /// <param name="start">Min score</param>
    /// <param name="stop">Max score</param>
    /// <param name="exclude">Exclude start / stop</param>
    /// <param name="order">Order of sorted set</param>
    /// <param name="skip">Skip count</param>
    /// <param name="take">Take count</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>
    ///     The list of elements in the specified score range.
    /// </returns>
    Task<IEnumerable<T?>> SortedSetRangeByScoreAsync<T>(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0L, long take = -1L, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Get entries from sorted-set ordered by rank
    /// </summary>
    /// <remarks>
    ///     Time complexity: O(log(N)+M) with N being the number of elements in the sorted set and M the number of elements being returned. If M is constant (e.g. always asking for the first 10 elements with LIMIT), you can consider it O(log(N)
    /// </remarks>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">Key of the set</param>
    /// <param name="start">Min score</param>
    /// <param name="stop">Max score</param>
    /// <param name="order">Order of sorted set</param>
    /// <param name="commandFlags">Behaviour markers associated with a given command</param>
    /// <returns>
    ///     The list of elements in the specified rank range along with their scores.
    /// </returns>
    Task<IEnumerable<ScoreRankResult<T>>> SortedSetRangeByRankWithScoresAsync<T>(string key, long start = 0L, long stop = -1L, Order order = Order.Ascending, CommandFlags commandFlags = CommandFlags.None);

    /// <summary>
    ///     Add the entry to a sorted set with  an increment score
    /// </summary>
    /// <remarks>
    ///     Time complexity: O(1)
    /// </remarks>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">Key of the set</param>
    /// <param name="value">The instance of T.</param>
    /// <param name="score">Score of the entry</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>
    ///      if the object has been added return previous score. Otherwise return 0.0 when first add
    /// </returns>
    Task<double> SortedSetAddIncrementAsync<T>(string key, T? value, double score, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns the number of elements in the sorted set stored at key.
    /// </summary>
    /// <param name="key">Key of the set</param>
    /// <param name="min">The minimum score to filter by (default: negative infinity).</param>
    /// <param name="max">The maximum score to filter by (default: positive infinity).</param>
    /// <param name="exclude">Which of min and max to exclude.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The cardinality (number of elements) of the sorted set.</returns>
    Task<long> SortedSetLengthAsync(string key, double min = double.NegativeInfinity, double max = double.PositiveInfinity, Exclude exclude = Exclude.None, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns the score of a member in the sorted set stored at key.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">Key of the set</param>
    /// <param name="member">The member to look up.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The score of the member, or null if the member does not exist in the sorted set.</returns>
    Task<double?> SortedSetScoreAsync<T>(string key, T member, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns the scores of multiple members in the sorted set stored at key.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">Key of the set</param>
    /// <param name="members">The members to look up.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The scores of the members, with null for members that do not exist in the sorted set.</returns>
    Task<double?[]> SortedSetScoresAsync<T>(string key, T[] members, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns the rank of a member in the sorted set stored at key.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">Key of the set</param>
    /// <param name="member">The member to look up.</param>
    /// <param name="order">The order (default: ascending, i.e. lowest score = rank 0).</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The rank of the member, or null if the member does not exist in the sorted set.</returns>
    Task<long?> SortedSetRankAsync<T>(string key, T member, Order order = Order.Ascending, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Increments the score of a member in the sorted set stored at key.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">Key of the set</param>
    /// <param name="member">The member whose score to increment.</param>
    /// <param name="value">The amount to increment by.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The new score of the member.</returns>
    Task<double> SortedSetIncrementAsync<T>(string key, T member, double value, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Decrements the score of a member in the sorted set stored at key.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">Key of the set</param>
    /// <param name="member">The member whose score to decrement.</param>
    /// <param name="value">The amount to decrement by.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The new score of the member.</returns>
    Task<double> SortedSetDecrementAsync<T>(string key, T member, double value, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Removes and returns the specified number of elements with the lowest or highest scores from the sorted set.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">Key of the set</param>
    /// <param name="count">The number of elements to pop.</param>
    /// <param name="order">The order: Ascending pops lowest scores, Descending pops highest.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The popped elements with their scores.</returns>
    Task<IEnumerable<ScoreRankResult<T>>> SortedSetPopAsync<T>(string key, long count = 1, Order order = Order.Ascending, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns a random member from the sorted set stored at key.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">Key of the set</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>A random element, or default if the set is empty.</returns>
    Task<T?> SortedSetRandomMemberAsync<T>(string key, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns the specified number of random members from the sorted set stored at key.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">Key of the set</param>
    /// <param name="count">The number of random members to return.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>An array of random elements.</returns>
    Task<T?[]> SortedSetRandomMembersAsync<T>(string key, long count, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Computes a set operation over multiple sorted sets and stores the result in a destination key.
    /// </summary>
    /// <param name="operation">The set operation to perform (Union, Intersect, Difference).</param>
    /// <param name="destinationKey">The key to store the result.</param>
    /// <param name="keys">The source sorted set keys.</param>
    /// <param name="weights">Optional weights for the source sets.</param>
    /// <param name="aggregate">Optional aggregate function (Sum, Min, Max).</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The number of elements in the resulting sorted set.</returns>
    Task<long> SortedSetCombineAndStoreAsync(SetOperation operation, string destinationKey, string[] keys, double[]? weights = null, Aggregate aggregate = Aggregate.Sum, CommandFlags flag = CommandFlags.None);
}
