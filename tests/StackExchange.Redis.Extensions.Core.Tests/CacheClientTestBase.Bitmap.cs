// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

using Xunit;

namespace StackExchange.Redis.Extensions.Core.Tests;

public abstract partial class CacheClientTestBase
{
    [Fact]
    public async Task StringSetBit_ShouldSetBitAndReturnOriginal_Async()
    {
        var key = Guid.NewGuid().ToString();

        var original = await Sut.GetDefaultDatabase().StringSetBitAsync(key, 7, true);

        Assert.False(original);
    }

    [Fact]
    public async Task StringGetBit_ShouldReturnSetBit_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().StringSetBitAsync(key, 7, true);

        var result = await Sut.GetDefaultDatabase().StringGetBitAsync(key, 7);

        Assert.True(result);
    }

    [Fact]
    public async Task StringGetBit_UnsetBit_ShouldReturnFalse_Async()
    {
        var key = Guid.NewGuid().ToString();

        var result = await Sut.GetDefaultDatabase().StringGetBitAsync(key, 7);

        Assert.False(result);
    }

    [Fact]
    public async Task StringBitCount_ShouldCountSetBits_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().StringSetBitAsync(key, 0, true);
        await Sut.GetDefaultDatabase().StringSetBitAsync(key, 1, true);
        await Sut.GetDefaultDatabase().StringSetBitAsync(key, 2, true);
        await Sut.GetDefaultDatabase().StringSetBitAsync(key, 10, true);

        var count = await Sut.GetDefaultDatabase().StringBitCountAsync(key);

        Assert.Equal(4, count);
    }

    [Fact]
    public async Task StringBitCount_EmptyKey_ShouldReturnZero_Async()
    {
        var key = Guid.NewGuid().ToString();

        var count = await Sut.GetDefaultDatabase().StringBitCountAsync(key);

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task StringBitOperation_And_ShouldComputeIntersection_Async()
    {
        var key1 = Guid.NewGuid().ToString();
        var key2 = Guid.NewGuid().ToString();
        var destKey = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().StringSetBitAsync(key1, 0, true);
        await Sut.GetDefaultDatabase().StringSetBitAsync(key1, 1, true);

        await Sut.GetDefaultDatabase().StringSetBitAsync(key2, 0, true);
        await Sut.GetDefaultDatabase().StringSetBitAsync(key2, 2, true);

        await Sut.GetDefaultDatabase().StringBitOperationAsync(
            Bitwise.And, destKey, new[] { key1, key2 });

        var bit0 = await Sut.GetDefaultDatabase().StringGetBitAsync(destKey, 0);
        var bit1 = await Sut.GetDefaultDatabase().StringGetBitAsync(destKey, 1);
        var bit2 = await Sut.GetDefaultDatabase().StringGetBitAsync(destKey, 2);

        Assert.True(bit0);
        Assert.False(bit1);
        Assert.False(bit2);
    }

    [Fact]
    public async Task StringBitOperation_Or_ShouldComputeUnion_Async()
    {
        var key1 = Guid.NewGuid().ToString();
        var key2 = Guid.NewGuid().ToString();
        var destKey = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().StringSetBitAsync(key1, 0, true);
        await Sut.GetDefaultDatabase().StringSetBitAsync(key2, 1, true);

        await Sut.GetDefaultDatabase().StringBitOperationAsync(
            Bitwise.Or, destKey, new[] { key1, key2 });

        var bit0 = await Sut.GetDefaultDatabase().StringGetBitAsync(destKey, 0);
        var bit1 = await Sut.GetDefaultDatabase().StringGetBitAsync(destKey, 1);

        Assert.True(bit0);
        Assert.True(bit1);
    }

    [Fact]
    public async Task StringBitPosition_ShouldFindFirstSetBit_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().StringSetBitAsync(key, 10, true);

        var pos = await Sut.GetDefaultDatabase().StringBitPositionAsync(key, true);

        Assert.Equal(10, pos);
    }

    [Fact]
    public async Task StringBitPosition_NoBitsSet_ShouldReturnNegative_Async()
    {
        var key = Guid.NewGuid().ToString();

        var pos = await Sut.GetDefaultDatabase().StringBitPositionAsync(key, true);

        Assert.Equal(-1, pos);
    }

    [Fact]
    public async Task StringSetBit_ToggleBit_ShouldWork_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().StringSetBitAsync(key, 5, true);
        Assert.True(await Sut.GetDefaultDatabase().StringGetBitAsync(key, 5));

        await Sut.GetDefaultDatabase().StringSetBitAsync(key, 5, false);
        Assert.False(await Sut.GetDefaultDatabase().StringGetBitAsync(key, 5));
    }
}
