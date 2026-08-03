// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Core.Abstractions;

/// <summary>
/// The Redis Database
/// </summary>
public partial interface IRedisDatabase
{
    /// <summary>
    ///     Evaluates a Lua script against the server.
    /// </summary>
    /// <param name="script">The Lua script to evaluate.</param>
    /// <param name="keys">The keys to pass to the script.</param>
    /// <param name="values">The values to pass to the script.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The raw result of the script evaluation.</returns>
    Task<RedisResult> ScriptEvaluateAsync(string script, RedisKey[]? keys = null, RedisValue[]? values = null, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Evaluates a Lua script against the server and deserializes the result.
    /// </summary>
    /// <typeparam name="T">The type of the expected result.</typeparam>
    /// <param name="script">The Lua script to evaluate.</param>
    /// <param name="keys">The keys to pass to the script.</param>
    /// <param name="values">The values to pass to the script.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The deserialized result of the script evaluation.</returns>
    Task<T?> ScriptEvaluateAsync<T>(string script, RedisKey[]? keys = null, RedisValue[]? values = null, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Evaluates a Lua script against the server using a read-only command (suitable for replica routing).
    /// </summary>
    /// <param name="script">The Lua script to evaluate.</param>
    /// <param name="keys">The keys to pass to the script.</param>
    /// <param name="values">The values to pass to the script.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The raw result of the script evaluation.</returns>
    Task<RedisResult> ScriptEvaluateReadOnlyAsync(string script, RedisKey[]? keys = null, RedisValue[]? values = null, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Evaluates a Lua script against the server using a read-only command and deserializes the result.
    /// </summary>
    /// <typeparam name="T">The type of the expected result.</typeparam>
    /// <param name="script">The Lua script to evaluate.</param>
    /// <param name="keys">The keys to pass to the script.</param>
    /// <param name="values">The values to pass to the script.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The deserialized result of the script evaluation.</returns>
    Task<T?> ScriptEvaluateReadOnlyAsync<T>(string script, RedisKey[]? keys = null, RedisValue[]? values = null, CommandFlags flag = CommandFlags.None);
}
