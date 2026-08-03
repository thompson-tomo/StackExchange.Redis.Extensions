// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Core.Helpers;

namespace StackExchange.Redis.Extensions.Core.Implementations;

public partial class RedisDatabase
{
    /// <inheritdoc/>
    public Task<long> ListAddToLeftAsync<T>(string key, T item, When when = When.Always, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(item);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (item == null)
            throw new ArgumentNullException(nameof(item), "item cannot be null.");
#endif

        var serializedItem = Serializer.Serialize(item);

        return Database.ListLeftPushAsync(key, serializedItem, when, flag);
    }

    /// <inheritdoc/>
    public Task<long> ListAddToLeftAsync<T>(string key, T[] items, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(items);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (items == null)
            throw new ArgumentNullException(nameof(items), "item cannot be null.");
#endif

        var serializedItems = items.ToFastArray(item => (RedisValue)Serializer.Serialize(item));

        return Database.ListLeftPushAsync(key, serializedItems, flag);
    }

    /// <inheritdoc/>
    public async Task<T?> ListGetFromRightAsync<T>(string key, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));
#endif

        var item = await Database.ListRightPopAsync(key, flag).ConfigureAwait(false);

        if (item == RedisValue.Null)
            return default;

        return item == RedisValue.Null
            ? default
            : Serializer.Deserialize<T>(item);
    }

    /// <inheritdoc/>
    public Task<long> ListAddToRightAsync<T>(string key, T item, When when = When.Always, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(item);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (item == null)
            throw new ArgumentNullException(nameof(item), "item cannot be null.");
#endif

        var serializedItem = Serializer.Serialize(item);

        return Database.ListRightPushAsync(key, serializedItem, when, flag);
    }

    /// <inheritdoc/>
    public Task<long> ListAddToRightAsync<T>(string key, T[] items, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(items);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (items == null)
            throw new ArgumentNullException(nameof(items), "items cannot be null.");
#endif

        var serializedItems = items.ToFastArray(item => (RedisValue)Serializer.Serialize(item));

        return Database.ListRightPushAsync(key, serializedItems, flag);
    }

    /// <inheritdoc/>
    public async Task<T?> ListGetFromLeftAsync<T>(string key, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));
#endif

        var item = await Database.ListLeftPopAsync(key, flag).ConfigureAwait(false);

        return item == RedisValue.Null
            ? default
            : Serializer.Deserialize<T>(item);
    }

    /// <inheritdoc/>
    public Task<long> ListLengthAsync(string key, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));
#endif

        return Database.ListLengthAsync(key, flag);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T?>> ListRangeAsync<T>(string key, long start = 0, long stop = -1, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));
#endif

        var result = await Database.ListRangeAsync(key, start, stop, flag).ConfigureAwait(false);

        return result.ToFastArray(m => m == RedisValue.Null ? default : Serializer.Deserialize<T>(m));
    }

    /// <inheritdoc/>
    public async Task<T?> ListGetByIndexAsync<T>(string key, long index, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));
#endif

        var item = await Database.ListGetByIndexAsync(key, index, flag).ConfigureAwait(false);

        return item == RedisValue.Null
            ? default
            : Serializer.Deserialize<T>(item);
    }

    /// <inheritdoc/>
    public Task ListSetByIndexAsync<T>(string key, long index, T value, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (value == null)
            throw new ArgumentNullException(nameof(value), "value cannot be null.");
#endif

        var serializedValue = Serializer.Serialize(value);

        return Database.ListSetByIndexAsync(key, index, serializedValue, flag);
    }

    /// <inheritdoc/>
    public Task ListTrimAsync(string key, long start, long stop, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));
#endif

        return Database.ListTrimAsync(key, start, stop, flag);
    }

    /// <inheritdoc/>
    public Task<long> ListRemoveAsync<T>(string key, T value, long count = 0, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (value == null)
            throw new ArgumentNullException(nameof(value), "value cannot be null.");
#endif

        var serializedValue = Serializer.Serialize(value);

        return Database.ListRemoveAsync(key, serializedValue, count, flag);
    }

    /// <inheritdoc/>
    public Task<long> ListInsertBeforeAsync<T>(string key, T pivot, T value, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(pivot);
        ArgumentNullException.ThrowIfNull(value);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (pivot == null)
            throw new ArgumentNullException(nameof(pivot), "pivot cannot be null.");

        if (value == null)
            throw new ArgumentNullException(nameof(value), "value cannot be null.");
#endif

        var serializedPivot = Serializer.Serialize(pivot);
        var serializedValue = Serializer.Serialize(value);

        return Database.ListInsertBeforeAsync(key, serializedPivot, serializedValue, flag);
    }

    /// <inheritdoc/>
    public Task<long> ListInsertAfterAsync<T>(string key, T pivot, T value, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(pivot);
        ArgumentNullException.ThrowIfNull(value);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (pivot == null)
            throw new ArgumentNullException(nameof(pivot), "pivot cannot be null.");

        if (value == null)
            throw new ArgumentNullException(nameof(value), "value cannot be null.");
#endif

        var serializedPivot = Serializer.Serialize(pivot);
        var serializedValue = Serializer.Serialize(value);

        return Database.ListInsertAfterAsync(key, serializedPivot, serializedValue, flag);
    }

    /// <inheritdoc/>
    public async Task<T?> ListMoveAsync<T>(string sourceKey, string destinationKey, ListSide sourceSide, ListSide destinationSide, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(sourceKey);
        ArgumentException.ThrowIfNullOrEmpty(destinationKey);
#else
        if (string.IsNullOrEmpty(sourceKey))
            throw new ArgumentException("sourceKey cannot be empty.", nameof(sourceKey));

        if (string.IsNullOrEmpty(destinationKey))
            throw new ArgumentException("destinationKey cannot be empty.", nameof(destinationKey));
#endif

        var result = await Database.ListMoveAsync(sourceKey, destinationKey, sourceSide, destinationSide, flag).ConfigureAwait(false);

        return result == RedisValue.Null
            ? default
            : Serializer.Deserialize<T>(result);
    }
}
