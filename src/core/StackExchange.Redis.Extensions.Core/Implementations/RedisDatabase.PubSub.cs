// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Core.Extensions;
using StackExchange.Redis.Extensions.Core.Logging;

namespace StackExchange.Redis.Extensions.Core.Implementations;

public partial class RedisDatabase
{
    /// <inheritdoc/>
    public Task<long> PublishAsync<T>(RedisChannel channel, T message, CommandFlags flag = CommandFlags.None)
    {
        var sub = connectionPoolManager.GetConnection().GetSubscriber();
        return sub.PublishAsync(channel, Serializer.Serialize(message), flag);
    }

    /// <inheritdoc/>
    public Task SubscribeAsync<T>(RedisChannel channel, Func<T?, Task> handler, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(handler);
#else
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));
#endif

        var sub = connectionPoolManager.GetConnection().GetSubscriber();

        return sub.SubscribeAsync(channel, Handler, flag);

        void Handler(RedisChannel redisChannel, RedisValue value)
        {
            // Task.Run keeps user handlers off the SE.Redis callback thread; the try/catch replaces a
            // ContinueWith continuation that was allocated per message even on the success path.
            _ = Task.Run(async () =>
            {
                try
                {
                    var deserialized = Serializer.Deserialize<T>(value);
                    await handler(deserialized).ConfigureAwait(false);
                }
#pragma warning disable CA1031 // User handlers can throw anything; a failed message must never tear down the subscription.
                catch (Exception ex)
#pragma warning restore CA1031
                {
                    LogMessages.SubscriptionHandlerError(logger, ex, (string?)redisChannel ?? "unknown");
                }
            });
        }
    }

    /// <inheritdoc/>
    public async Task UnsubscribeAsync<T>(RedisChannel channel, Func<T?, Task> handler, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(handler);
#else
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));
#endif

        foreach (var connection in connectionPoolManager.GetConnections())
        {
            var sub = connection.GetSubscriber();

            if (sub.SubscribedEndpoint(channel) is not null)
                await sub.UnsubscribeAsync(channel, null, flag).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async Task UnsubscribeAllAsync(CommandFlags flag = CommandFlags.None)
    {
        foreach (var connection in connectionPoolManager.GetConnections())
        {
            var sub = connection.GetSubscriber();
            await sub.UnsubscribeAllAsync(flag).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public Task<bool> UpdateExpiryAsync(string key, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None)
    {
        // EXPIRE already returns false on missing keys: the previous EXISTS pre-check doubled the round-trips
        // without adding correctness (the two commands were not atomic anyway).
        return Database.KeyExpireAsync(key, expiresAt.UtcDateTime.Subtract(DateTime.UtcNow), flag);
    }

    /// <inheritdoc/>
    public Task<bool> UpdateExpiryAsync(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None)
    {
        return Database.KeyExpireAsync(key, expiresIn, flag);
    }

    /// <inheritdoc/>
    public async Task<IDictionary<string, bool>> UpdateExpiryAllAsync(HashSet<string> keys, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None)
    {
        // Computed once: the previous per-key computation re-read DateTime.UtcNow for every key, drifting the TTL.
        var expiresIn = expiresAt.UtcDateTime.Subtract(DateTime.UtcNow);

        var tasks = keys.ToFastArray(key => UpdateExpiryAsync(key, expiresIn, flag));

        await Task.WhenAll(tasks).ConfigureAwait(false);

        var results = new Dictionary<string, bool>(keys.Count, StringComparer.Ordinal);
        var i = 0;

        foreach (var key in keys)
            results.Add(key, tasks[i++].Result);

        return results;
    }

    /// <inheritdoc/>
    public async Task<IDictionary<string, bool>> UpdateExpiryAllAsync(HashSet<string> keys, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None)
    {
        var tasks = keys.ToFastArray(key => UpdateExpiryAsync(key, expiresIn, flag));

        await Task.WhenAll(tasks).ConfigureAwait(false);

        var results = new Dictionary<string, bool>(keys.Count, StringComparer.Ordinal);
        var i = 0;

        foreach (var key in keys)
            results.Add(key, tasks[i++].Result);

        return results;
    }
}
