// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Core.Implementations;

public partial class RedisDatabase
{
    /// <inheritdoc/>
    public Task<RedisResult> ScriptEvaluateAsync(string script, RedisKey[]? keys = null, RedisValue[]? values = null, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(script);
#else
        if (string.IsNullOrEmpty(script))
            throw new ArgumentException("script cannot be empty.", nameof(script));
#endif

        return Database.ScriptEvaluateAsync(script, keys, values, flag);
    }

    /// <inheritdoc/>
    public async Task<T?> ScriptEvaluateAsync<T>(string script, RedisKey[]? keys = null, RedisValue[]? values = null, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(script);
#else
        if (string.IsNullOrEmpty(script))
            throw new ArgumentException("script cannot be empty.", nameof(script));
#endif

        var result = await Database.ScriptEvaluateAsync(script, keys, values, flag).ConfigureAwait(false);

        if (result.IsNull)
            return default;

        return Serializer.Deserialize<T>((byte[])result!);
    }

    /// <inheritdoc/>
    public Task<RedisResult> ScriptEvaluateReadOnlyAsync(string script, RedisKey[]? keys = null, RedisValue[]? values = null, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(script);
#else
        if (string.IsNullOrEmpty(script))
            throw new ArgumentException("script cannot be empty.", nameof(script));
#endif

        return Database.ScriptEvaluateReadOnlyAsync(script, keys, values, flag);
    }

    /// <inheritdoc/>
    public async Task<T?> ScriptEvaluateReadOnlyAsync<T>(string script, RedisKey[]? keys = null, RedisValue[]? values = null, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(script);
#else
        if (string.IsNullOrEmpty(script))
            throw new ArgumentException("script cannot be empty.", nameof(script));
#endif

        var result = await Database.ScriptEvaluateReadOnlyAsync(script, keys, values, flag).ConfigureAwait(false);

        if (result.IsNull)
            return default;

        return Serializer.Deserialize<T>((byte[])result!);
    }
}
