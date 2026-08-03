// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

using Xunit;

namespace StackExchange.Redis.Extensions.Core.Tests;

public abstract partial class CacheClientTestBase
{
    [Fact]
    public async Task HyperLogLogAdd_SingleValue_ShouldReturnTrue_Async()
    {
        var key = Guid.NewGuid().ToString();
        var result = await Sut.GetDefaultDatabase().HyperLogLogAddAsync(key, "user1");
        Assert.True(result);
    }

    [Fact]
    public async Task HyperLogLogAdd_MultipleValues_ShouldReturnTrue_Async()
    {
        var key = Guid.NewGuid().ToString();
        var values = new[] { "user1", "user2", "user3" };
        var result = await Sut.GetDefaultDatabase().HyperLogLogAddAsync(key, values);
        Assert.True(result);
    }

    [Fact]
    public async Task HyperLogLogAdd_DuplicateValue_ShouldReturnFalse_Async()
    {
        var key = Guid.NewGuid().ToString();
        await Sut.GetDefaultDatabase().HyperLogLogAddAsync(key, "user1");
        var result = await Sut.GetDefaultDatabase().HyperLogLogAddAsync(key, "user1");
        Assert.False(result);
    }

    [Fact]
    public async Task HyperLogLogLength_WithElements_ShouldReturnApproximateCount_Async()
    {
        var key = Guid.NewGuid().ToString();
        await Sut.GetDefaultDatabase().HyperLogLogAddAsync(key, new[] { "user1", "user2", "user3" });
        var count = await Sut.GetDefaultDatabase().HyperLogLogLengthAsync(key);
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task HyperLogLogLength_EmptyKey_ShouldReturnZero_Async()
    {
        var key = Guid.NewGuid().ToString();
        var count = await Sut.GetDefaultDatabase().HyperLogLogLengthAsync(key);
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task HyperLogLogLength_MultipleKeys_ShouldReturnUnionCount_Async()
    {
        var key1 = Guid.NewGuid().ToString();
        var key2 = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().HyperLogLogAddAsync(key1, new[] { "user1", "user2" });
        await Sut.GetDefaultDatabase().HyperLogLogAddAsync(key2, new[] { "user2", "user3" });

        var count = await Sut.GetDefaultDatabase().HyperLogLogLengthAsync(new[] { key1, key2 });
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task HyperLogLogMerge_ShouldCombineSets_Async()
    {
        var key1 = Guid.NewGuid().ToString();
        var key2 = Guid.NewGuid().ToString();
        var destKey = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().HyperLogLogAddAsync(key1, new[] { "user1", "user2" });
        await Sut.GetDefaultDatabase().HyperLogLogAddAsync(key2, new[] { "user3", "user4" });

        await Sut.GetDefaultDatabase().HyperLogLogMergeAsync(destKey, new[] { key1, key2 });

        var count = await Sut.GetDefaultDatabase().HyperLogLogLengthAsync(destKey);
        Assert.Equal(4, count);
    }

    [Fact]
    public async Task HyperLogLogMerge_WithOverlap_ShouldDeduplicateCount_Async()
    {
        var key1 = Guid.NewGuid().ToString();
        var key2 = Guid.NewGuid().ToString();
        var destKey = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().HyperLogLogAddAsync(key1, new[] { "user1", "user2", "user3" });
        await Sut.GetDefaultDatabase().HyperLogLogAddAsync(key2, new[] { "user2", "user3", "user4" });

        await Sut.GetDefaultDatabase().HyperLogLogMergeAsync(destKey, new[] { key1, key2 });

        var count = await Sut.GetDefaultDatabase().HyperLogLogLengthAsync(destKey);
        Assert.Equal(4, count);
    }
}
