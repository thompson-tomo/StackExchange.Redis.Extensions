// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using NSubstitute;

using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.ServerIteration;

using Xunit;

namespace StackExchange.Redis.Extensions.Core.Tests;

public class ServerSourceTests
{
    [Fact]
    public void GetServers_SingleMode_ReturnsOnlyFirstServer()
    {
        var (multiplexer, servers) = BuildMultiplexer(3);
        var strategy = new ServerEnumerationStrategy { Mode = ServerEnumerationStrategy.ModeOptions.Single };

        var result = new ServerSource(multiplexer).GetServers(strategy).ToList();

        Assert.Single(result);
        Assert.Same(servers[0], result[0]);
    }

    [Fact]
    public void GetServers_AllMode_ReturnsEveryServer()
    {
        var (multiplexer, servers) = BuildMultiplexer(3);
        var strategy = new ServerEnumerationStrategy { Mode = ServerEnumerationStrategy.ModeOptions.All };

        var result = new ServerSource(multiplexer).GetServers(strategy).ToList();

        Assert.Equal(3, result.Count);
        Assert.Equal(servers, result);
    }

    [Fact]
    public void GetServers_PreferSlave_SkipsPrimaries()
    {
        var (multiplexer, servers) = BuildMultiplexer(3);
        servers[1].IsReplica.Returns(true);

        var strategy = new ServerEnumerationStrategy
        {
            Mode = ServerEnumerationStrategy.ModeOptions.All,
            TargetRole = ServerEnumerationStrategy.TargetRoleOptions.PreferSlave
        };

        var result = new ServerSource(multiplexer).GetServers(strategy).ToList();

        Assert.Single(result);
        Assert.Same(servers[1], result[0]);
    }

    [Fact]
    public void GetServers_IgnoreIfOtherAvailable_SkipsDisconnectedServers()
    {
        var (multiplexer, servers) = BuildMultiplexer(3);
        servers[0].IsConnected.Returns(false);

        var strategy = new ServerEnumerationStrategy
        {
            Mode = ServerEnumerationStrategy.ModeOptions.Single,
            UnreachableServerAction = ServerEnumerationStrategy.UnreachableServerActionOptions.IgnoreIfOtherAvailable
        };

        var result = new ServerSource(multiplexer).GetServers(strategy).ToList();

        Assert.Single(result);
        Assert.Same(servers[1], result[0]);
    }

    // Guards the deferred-execution contract: GetServers must not touch the multiplexer until the
    // result is enumerated. An earlier revision evaluated the Single branch eagerly, which this catches.
    [Theory]
    [InlineData(ServerEnumerationStrategy.ModeOptions.Single)]
    [InlineData(ServerEnumerationStrategy.ModeOptions.All)]
    public void GetServers_IsLazy_DoesNotResolveServersUntilEnumerated(ServerEnumerationStrategy.ModeOptions mode)
    {
        var (multiplexer, _) = BuildMultiplexer(3);

        var deferred = new ServerSource(multiplexer).GetServers(new() { Mode = mode });

        multiplexer.DidNotReceive().GetServer(Arg.Any<EndPoint>());

        _ = deferred.ToList();

        multiplexer.Received().GetServer(Arg.Any<EndPoint>());
    }

    private static (IConnectionMultiplexer Multiplexer, List<IServer> Servers) BuildMultiplexer(int count)
    {
        var multiplexer = Substitute.For<IConnectionMultiplexer>();
        var endPoints = new EndPoint[count];
        var servers = new List<IServer>(count);

        for (var i = 0; i < count; i++)
        {
            var endPoint = new IPEndPoint(IPAddress.Loopback, 6379 + i);
            endPoints[i] = endPoint;

            var server = Substitute.For<IServer>();
            server.IsReplica.Returns(false);
            server.IsConnected.Returns(true);

            // SCAN landed in Redis 2.8; without a version the mock reports Features.Scan == false
            // and the IgnoreIfOtherAvailable filter would discard every server.
            server.Features.Returns(new RedisFeatures(new Version(2, 8)));

            servers.Add(server);

            multiplexer.GetServer(endPoint).Returns(server);
        }

        multiplexer.GetEndPoints().Returns(endPoints);

        return (multiplexer, servers);
    }
}
