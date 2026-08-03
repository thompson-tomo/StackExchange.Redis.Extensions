// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Core.Extensions;
using StackExchange.Redis.Extensions.Core.Helpers;

namespace StackExchange.Redis.Extensions.Core.Implementations;

public partial class RedisDatabase
{
    /// <inheritdoc/>
    public async Task<IEnumerable<T?>> GetByTagAsync<T>(string tag, CommandFlags flag = CommandFlags.None)
    {
        var tagKey = TagHelper.GenerateTagKey(tag);

        var keys = await SetMembersAsync<string>(tagKey, flag).ConfigureAwait(false);

        if (keys.Length == 0)
            return [];

        var hashKeys = new HashSet<string>(keys.Length, StringComparer.Ordinal);

        foreach (var key in keys)
            hashKeys.Add(key!);

        var result = await GetAllAsync<T>(hashKeys, flag).ConfigureAwait(false);

        return result.Values;
    }

    /// <inheritdoc/>
    public async Task<long> RemoveByTagAsync(string tag, CommandFlags flag = CommandFlags.None)
    {
        var tagKey = TagHelper.GenerateTagKey(tag);

        var keys = await SetMembersAsync<string>(tagKey, flag).ConfigureAwait(false);

        var deletedCount = await RemoveAllAsync(keys, flag).ConfigureAwait(false);

        await Database.KeyDeleteAsync(tagKey, flag).ConfigureAwait(false);

        return deletedCount;
    }

    private Task<bool> ExecuteAddWithTagsAsync(
        string key,
        HashSet<string> tags,
        Func<IDatabaseAsync, Task<bool>> action,
        When when = When.Always,
        CommandFlags commandFlags = CommandFlags.None)
    {
        var transaction = Database.CreateTransaction();

        TryAddCondition(transaction, when, key);

        // Serialized once: the previous per-tag call produced N identical byte arrays for the same key.
        var serializedKey = key.OfValueSize(Serializer, maxValueLength, key);

        foreach (var tag in tags)
            transaction.SetAddAsync(TagHelper.GenerateTagKey(tag), serializedKey, commandFlags);

        action(transaction);

        return transaction.ExecuteAsync(commandFlags);
    }

    private static void TryAddCondition(ITransaction transaction, When when, string key)
    {
        var condition = when switch
        {
            When.NotExists => Condition.KeyNotExists(key),
            When.Exists => Condition.KeyExists(key),
            _ => null
        };

        if (condition is null)
            return;

        transaction.AddCondition(condition);
    }
}
