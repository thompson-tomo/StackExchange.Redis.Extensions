// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Core.Helpers;
using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Extensions.Core.Implementations;

public partial class RedisDatabase
{
    /// <inheritdoc/>
    public Task<bool> SortedSetAddAsync<T>(
        string key,
        T value,
        double score,
        CommandFlags flag = CommandFlags.None)
    {
        var entryBytes = Serializer.Serialize(value);

        return Database.SortedSetAddAsync(key, entryBytes, score, flag);
    }

    /// <inheritdoc/>
    public Task<bool> SortedSetRemoveAsync<T>(
        string key,
        T value,
        CommandFlags flag = CommandFlags.None)
    {
        var entryBytes = Serializer.Serialize(value);

        return Database.SortedSetRemoveAsync(key, entryBytes, flag);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T?>> SortedSetRangeByScoreAsync<T>(
        string key,
        double start = double.NegativeInfinity,
        double stop = double.PositiveInfinity,
        Exclude exclude = Exclude.None,
        Order order = Order.Ascending,
        long skip = 0L,
        long take = -1L,
        CommandFlags flag = CommandFlags.None)
    {
        var result = await Database.SortedSetRangeByScoreAsync(key, start, stop, exclude, order, skip, take, flag).ConfigureAwait(false);

        // Materialized eagerly: a lazy Select would re-deserialize every element on each enumeration.
        return result.ToFastArray(m => m == RedisValue.Null ? default : Serializer.Deserialize<T>(m));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ScoreRankResult<T>>> SortedSetRangeByRankWithScoresAsync<T>(
        string key,
        long start = 0L,
        long stop = -1L,
        Order order = Order.Ascending,
        CommandFlags commandFlags = CommandFlags.None)
    {
        var result = await Database.SortedSetRangeByRankWithScoresAsync(key, start, stop, order, commandFlags).ConfigureAwait(false);

        return result.ToFastArray(x => new ScoreRankResult<T>(Serializer.Deserialize<T>(x.Element), x.Score));
    }

    /// <inheritdoc/>
    public Task<long> SortedSetLengthAsync(
        string key,
        double min = double.NegativeInfinity,
        double max = double.PositiveInfinity,
        Exclude exclude = Exclude.None,
        CommandFlags flag = CommandFlags.None)
    {
        return Database.SortedSetLengthAsync(key, min, max, exclude, flag);
    }

    /// <inheritdoc/>
    public Task<double?> SortedSetScoreAsync<T>(
        string key,
        T member,
        CommandFlags flag = CommandFlags.None)
    {
        var entryBytes = Serializer.Serialize(member);

        return Database.SortedSetScoreAsync(key, entryBytes, flag);
    }

    /// <inheritdoc/>
    public Task<double?[]> SortedSetScoresAsync<T>(
        string key,
        T[] members,
        CommandFlags flag = CommandFlags.None)
    {
        var serializedMembers = members.ToFastArray(m => (RedisValue)Serializer.Serialize(m));

        return Database.SortedSetScoresAsync(key, serializedMembers, flag);
    }

    /// <inheritdoc/>
    public Task<long?> SortedSetRankAsync<T>(
        string key,
        T member,
        Order order = Order.Ascending,
        CommandFlags flag = CommandFlags.None)
    {
        var entryBytes = Serializer.Serialize(member);

        return Database.SortedSetRankAsync(key, entryBytes, order, flag);
    }

    /// <inheritdoc/>
    public Task<double> SortedSetIncrementAsync<T>(
        string key,
        T member,
        double value,
        CommandFlags flag = CommandFlags.None)
    {
        var entryBytes = Serializer.Serialize(member);

        return Database.SortedSetIncrementAsync(key, entryBytes, value, flag);
    }

    /// <inheritdoc/>
    public Task<double> SortedSetDecrementAsync<T>(
        string key,
        T member,
        double value,
        CommandFlags flag = CommandFlags.None)
    {
        var entryBytes = Serializer.Serialize(member);

        return Database.SortedSetDecrementAsync(key, entryBytes, value, flag);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ScoreRankResult<T>>> SortedSetPopAsync<T>(
        string key,
        long count = 1,
        Order order = Order.Ascending,
        CommandFlags flag = CommandFlags.None)
    {
        var result = await Database.SortedSetPopAsync(key, count, order, flag).ConfigureAwait(false);

        return result.ToFastArray(x => new ScoreRankResult<T>(Serializer.Deserialize<T>(x.Element), x.Score));
    }

    /// <inheritdoc/>
    public async Task<T?> SortedSetRandomMemberAsync<T>(
        string key,
        CommandFlags flag = CommandFlags.None)
    {
        var result = await Database.SortedSetRandomMemberAsync(key, flag).ConfigureAwait(false);

        return result == RedisValue.Null ? default : Serializer.Deserialize<T>(result);
    }

    /// <inheritdoc/>
    public async Task<T?[]> SortedSetRandomMembersAsync<T>(
        string key,
        long count,
        CommandFlags flag = CommandFlags.None)
    {
        var result = await Database.SortedSetRandomMembersAsync(key, count, flag).ConfigureAwait(false);

        return result.ToFastArray(m => m == RedisValue.Null ? default : Serializer.Deserialize<T>(m));
    }

    /// <inheritdoc/>
    public Task<long> SortedSetCombineAndStoreAsync(
        SetOperation operation,
        string destinationKey,
        string[] keys,
        double[]? weights = null,
        Aggregate aggregate = Aggregate.Sum,
        CommandFlags flag = CommandFlags.None)
    {
        var redisKeys = keys.ToFastArray(k => (RedisKey)k);

        return Database.SortedSetCombineAndStoreAsync(operation, destinationKey, redisKeys, weights, aggregate, flag);
    }
}
