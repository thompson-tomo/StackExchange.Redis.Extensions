// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

using Xunit;

namespace StackExchange.Redis.Extensions.Core.Tests;

public abstract partial class CacheClientTestBase
{
    [Fact]
    public async Task ScriptEvaluate_SimpleReturn_ShouldReturnResult_Async()
    {
        var result = await Sut.GetDefaultDatabase().ScriptEvaluateAsync("return 42");

        Assert.Equal(42, (long)result);
    }

    [Fact]
    public async Task ScriptEvaluate_WithKeys_ShouldSetAndGetValue_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().ScriptEvaluateAsync(
            "redis.call('SET', KEYS[1], ARGV[1]) return 1",
            new RedisKey[] { key },
            new RedisValue[] { "test-value" });

        var value = await db.StringGetAsync(key);
        Assert.Equal("test-value", (string?)value);
    }

    [Fact]
    public async Task ScriptEvaluate_ReturnString_ShouldWork_Async()
    {
        var result = await Sut.GetDefaultDatabase().ScriptEvaluateAsync("return 'hello'");

        Assert.Equal("hello", (string?)result);
    }

    [Fact]
    public async Task ScriptEvaluate_ReturnTable_ShouldReturnArray_Async()
    {
        var result = await Sut.GetDefaultDatabase().ScriptEvaluateAsync(
            "return {1, 2, 3}");

        var array = (RedisResult[]?)result;
        Assert.NotNull(array);
        Assert.Equal(3, array!.Length);
    }

    [Fact]
    public async Task ScriptEvaluate_ReturnNil_ShouldReturnNull_Async()
    {
        var result = await Sut.GetDefaultDatabase().ScriptEvaluateAsync("return nil");

        Assert.True(result.IsNull);
    }

    [Fact]
    public async Task ScriptEvaluateReadOnly_ShouldWork_Async()
    {
        var key = Guid.NewGuid().ToString();
        await db.StringSetAsync(key, "readonly-test");

        var result = await Sut.GetDefaultDatabase().ScriptEvaluateReadOnlyAsync(
            "return redis.call('GET', KEYS[1])",
            new RedisKey[] { key });

        Assert.Equal("readonly-test", (string?)result);
    }

    [Fact]
    public async Task ScriptEvaluate_WithArgv_ShouldPassValues_Async()
    {
        var result = await Sut.GetDefaultDatabase().ScriptEvaluateAsync(
            "return tonumber(ARGV[1]) + tonumber(ARGV[2])",
            values: new RedisValue[] { 10, 20 });

        Assert.Equal(30, (long)result);
    }

    [Fact]
    public async Task ScriptEvaluate_MultipleKeys_ShouldWork_Async()
    {
        var key1 = Guid.NewGuid().ToString();
        var key2 = Guid.NewGuid().ToString();

        await db.StringSetAsync(key1, "100");
        await db.StringSetAsync(key2, "200");

        var result = await Sut.GetDefaultDatabase().ScriptEvaluateAsync(
            "return tonumber(redis.call('GET', KEYS[1])) + tonumber(redis.call('GET', KEYS[2]))",
            new RedisKey[] { key1, key2 });

        Assert.Equal(300, (long)result);
    }
}
