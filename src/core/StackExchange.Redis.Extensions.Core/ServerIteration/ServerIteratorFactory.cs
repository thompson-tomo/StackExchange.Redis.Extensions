// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

using StackExchange.Redis.Extensions.Core.Configuration;

namespace StackExchange.Redis.Extensions.Core.ServerIteration;

/// <summary>
/// The factory that allows you to enumerate all Redis servers.
/// </summary>
#if NET5_0_OR_GREATER
[Obsolete("This type is removed in v14. Fire an issue if you require similar functionality.", DiagnosticId = "SRE0002")]
#else
[Obsolete("This type is removed in v14. Fire an issue if you require similar functionality.")]
#endif
public static class ServerIteratorFactory
{
    /// <summary>
    /// Rerturn all Redis servers
    /// </summary>
    /// <param name="multiplexer">The redis connection.</param>
    /// <param name="serverEnumerationStrategy">The iterate strategy.</param>
    /// <exception cref="NotImplementedException">In case of wrong enum.</exception>
    public static IEnumerable<IServer> GetServers(
        IConnectionMultiplexer multiplexer,
        ServerEnumerationStrategy serverEnumerationStrategy)
        => new ServerSource(multiplexer).GetServers(serverEnumerationStrategy);
}
