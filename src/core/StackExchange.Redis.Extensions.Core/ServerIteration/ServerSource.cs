// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;

using StackExchange.Redis.Extensions.Core.Configuration;

using static StackExchange.Redis.Extensions.Core.Configuration.ServerEnumerationStrategy;

namespace StackExchange.Redis.Extensions.Core.ServerIteration;

/// <summary>
/// Represents a source of servers from a connection multiplexer.
/// </summary>
/// <param name="multiplexer">The connection multiplexer to retrieve servers from.</param>
/// <remarks>
/// DO NOT implement <see cref="IEnumerable{IServer}"/> to avoid boxing struct enumerators and to keep allocation semantics explicit.
/// </remarks>
internal readonly struct ServerSource(IConnectionMultiplexer multiplexer)
{
    private readonly EndPoint[] endPoints = multiplexer.GetEndPoints();

    public IEnumerable<IServer> GetServers(ServerEnumerationStrategy strategy)
        => strategy.Mode == ModeOptions.Single
            ? EnumerateCore(strategy).Take(1)
            : EnumerateCore(strategy);

    private IEnumerable<IServer> EnumerateCore(ServerEnumerationStrategy strategy)
    {
        foreach (var endPoint in endPoints)
        {
            var server = multiplexer.GetServer(endPoint);

            if (strategy.TargetRole == TargetRoleOptions.PreferSlave)
            {
                if (!server.IsReplica)
                    continue;
            }

            if (strategy.UnreachableServerAction == UnreachableServerActionOptions.IgnoreIfOtherAvailable)
            {
                if (!server.IsConnected || !server.Features.Scan)
                    continue;
            }

            yield return server;
        }
    }
}
