// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using System.Diagnostics;

using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Extensions;
using StackExchange.Redis.Extensions.Core.Logging;
using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Extensions.Core.Implementations;

/// <inheritdoc/>
public sealed partial class RedisConnectionPoolManager : IRedisConnectionPoolManager
{
    private readonly IStateAwareConnection[] connections;
    private readonly RedisConfiguration redisConfiguration;
    private readonly ILogger<RedisConnectionPoolManager> logger;
    private int roundRobinIndex = -1;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisConnectionPoolManager"/> class.
    /// </summary>
    /// <param name="redisConfiguration">The redis configuration.</param>
    /// <param name="logger">The logger. If null will create one from redisConfiguration.LoggerFactory if factory provided</param>
    public RedisConnectionPoolManager(RedisConfiguration redisConfiguration, ILogger<RedisConnectionPoolManager>? logger = null)
    {
        this.redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));
        logger ??= redisConfiguration.LoggerFactory?.CreateLogger<RedisConnectionPoolManager>();
        this.logger = logger ?? NullLogger<RedisConnectionPoolManager>.Instance;

        connections = new IStateAwareConnection[redisConfiguration.PoolSize];

#pragma warning disable VSTHRD002 // Synchronous wait is required here because constructors cannot be async
        EmitConnectionsAsync().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (isDisposed)
            return;

        if (disposing)
        {
            LogMessages.PoolDisposing(logger, connections.Length);

            foreach (var connection in connections)
                connection?.Dispose();
        }

        isDisposed = true;
    }

    /// <inheritdoc/>
    public IConnectionMultiplexer GetConnection()
    {
        IStateAwareConnection connection;

        switch (redisConfiguration.ConnectionSelectionStrategy)
        {
            case ConnectionSelectionStrategy.RoundRobin:
                // Casting to uint handles the int wraparound: the modulo stays valid across overflow.
                var nextIdx = (int)((uint)Interlocked.Increment(ref roundRobinIndex) % (uint)connections.Length);
                connection = connections[nextIdx];

                if (!connection.IsConnected())
                {
                    // Selected connection is disconnected, try to find a connected one
                    for (var i = 0; i < connections.Length; i++)
                    {
                        if (connections[i].IsConnected())
                        {
                            connection = connections[i];
                            break;
                        }
                    }
                }

                break;

            case ConnectionSelectionStrategy.LeastLoaded:
                // Prefer connected connections; fall back to any if all are disconnected.
                // TotalOutstanding() allocates a ServerCounters snapshot in SE.Redis, so it must be called at most once per connection.
                IStateAwareConnection? candidate = null;
                var candidateOutstanding = long.MaxValue;

                for (var i = 0; i < connections.Length; i++)
                {
                    if (!connections[i].IsConnected())
                        continue;

                    var outstanding = connections[i].TotalOutstanding();

                    if (outstanding < candidateOutstanding)
                    {
                        candidate = connections[i];
                        candidateOutstanding = outstanding;
                    }
                }

                if (candidate == null)
                {
                    for (var i = 0; i < connections.Length; i++)
                    {
                        var outstanding = connections[i].TotalOutstanding();

                        if (outstanding < candidateOutstanding)
                        {
                            candidate = connections[i];
                            candidateOutstanding = outstanding;
                        }
                    }
                }

                connection = candidate!;
                break;

            default:
                throw new InvalidEnumArgumentException(nameof(redisConfiguration.ConnectionSelectionStrategy), (int)redisConfiguration.ConnectionSelectionStrategy, typeof(ConnectionSelectionStrategy));
        }

        if (!connection.IsConnected())
            LogMessages.AllConnectionsDisconnected(logger, connection.Connection.GetHashCode());

        // Guarded because TotalOutstanding() allocates: log arguments are evaluated at the call-site even when the level is disabled.
        if (logger.IsEnabled(LogLevel.Debug))
            LogMessages.ConnectionSelected(logger, connection.Connection.GetHashCode(), connection.TotalOutstanding());

        return connection.Connection;
    }

    /// <inheritdoc/>
    public IEnumerable<IConnectionMultiplexer> GetConnections()
    {
        foreach (var connection in connections)
            yield return connection.Connection;
    }

    /// <inheritdoc/>
    public ConnectionPoolInformation GetConnectionInformation()
    {
        var activeConnections = 0;
        var invalidConnections = 0;

        ref var searchSpace = ref MemoryMarshal.GetReference(connections.AsSpan());

        for (var i = 0; i < connections.Length; i++)
        {
            ref var connection = ref Unsafe.Add(ref searchSpace, i);

            if (!connection.IsConnected())
            {
                invalidConnections++;
                continue;
            }

            activeConnections++;
        }

        return new()
        {
            RequiredPoolSize = redisConfiguration.PoolSize,
            ActiveConnections = activeConnections,
            InvalidConnections = invalidConnections
        };
    }

    private async Task EmitConnectionsAsync()
    {
        var sw = Stopwatch.StartNew();
        var baseOpts = redisConfiguration.ConfigurationOptions;

        if (redisConfiguration.ConfigurationOptionsAsyncHandler != null)
            LogMessages.UsingAsyncConfigHandler(logger);

        for (var index = 0; index < redisConfiguration.PoolSize; index++)
        {
            try
            {
                var opts = baseOpts;

                if (redisConfiguration.ConfigurationOptionsAsyncHandler != null)
                {
                    opts = baseOpts.Clone();
                    opts = await redisConfiguration.ConfigurationOptionsAsyncHandler(opts).ConfigureAwait(false);
                }

                var multiplexer = await ConnectionMultiplexer.ConnectAsync(opts).ConfigureAwait(false);

                if (redisConfiguration.ProfilingSessionProvider != null)
                    multiplexer.RegisterProfiler(redisConfiguration.ProfilingSessionProvider);

                connections[index] = redisConfiguration.StateAwareConnectionFactory(multiplexer, logger);
            }
            catch
            {
                LogMessages.PoolInitializationFailed(logger, index, redisConfiguration.PoolSize, index);

                for (var i = 0; i < index; i++)
                    connections[i]?.Dispose();

                throw;
            }
        }

        sw.Stop();
        var endpoint = baseOpts.EndPoints.Count > 0 ? baseOpts.EndPoints[0].ToString()! : "unknown";
        LogMessages.PoolInitialized(logger, redisConfiguration.PoolSize, endpoint, sw.ElapsedMilliseconds);
    }
}
