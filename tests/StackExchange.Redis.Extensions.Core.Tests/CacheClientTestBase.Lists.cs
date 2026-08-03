// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

namespace StackExchange.Redis.Extensions.Core.Tests;

public abstract partial class CacheClientTestBase
{
    [Fact]
    public async Task ListAddToRight_ShouldPushItem_Async()
    {
        var key = Guid.NewGuid().ToString();

        var length = await Sut.GetDefaultDatabase().ListAddToRightAsync(key, "item1");

        Assert.Equal(1, length);
    }

    [Fact]
    public async Task ListAddToRight_MultipleItems_ShouldPushAll_Async()
    {
        var key = Guid.NewGuid().ToString();

        var length = await Sut.GetDefaultDatabase().ListAddToRightAsync<string>(key, new[] { "a", "b", "c" });

        Assert.Equal(3, length);
    }

    [Fact]
    public async Task ListGetFromLeft_ShouldPopFirstElement_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().ListAddToRightAsync<string>(key, new[] { "first", "second" });

        var result = await Sut.GetDefaultDatabase().ListGetFromLeftAsync<string>(key);

        Assert.Equal("first", result);
    }

    [Fact]
    public async Task ListGetFromLeft_EmptyList_ShouldReturnDefault_Async()
    {
        var key = Guid.NewGuid().ToString();

        var result = await Sut.GetDefaultDatabase().ListGetFromLeftAsync<string>(key);

        Assert.Null(result);
    }

    [Fact]
    public async Task ListLength_ShouldReturnCount_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().ListAddToLeftAsync<string>(key, new[] { "a", "b", "c" });

        var length = await Sut.GetDefaultDatabase().ListLengthAsync(key);

        Assert.Equal(3, length);
    }

    [Fact]
    public async Task ListLength_EmptyList_ShouldReturnZero_Async()
    {
        var key = Guid.NewGuid().ToString();

        var length = await Sut.GetDefaultDatabase().ListLengthAsync(key);

        Assert.Equal(0, length);
    }

    [Fact]
    public async Task ListRange_ShouldReturnElements_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().ListAddToRightAsync<string>(key, new[] { "a", "b", "c", "d" });

        var result = (await Sut.GetDefaultDatabase().ListRangeAsync<string>(key, 1, 2)).ToArray();

        Assert.Equal(2, result.Length);
        Assert.Equal("b", result[0]);
        Assert.Equal("c", result[1]);
    }

    [Fact]
    public async Task ListRange_AllElements_ShouldReturnFullList_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().ListAddToRightAsync<string>(key, new[] { "a", "b", "c" });

        var result = (await Sut.GetDefaultDatabase().ListRangeAsync<string>(key)).ToArray();

        Assert.Equal(3, result.Length);
    }

    [Fact]
    public async Task ListGetByIndex_ShouldReturnElement_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().ListAddToRightAsync<string>(key, new[] { "a", "b", "c" });

        var result = await Sut.GetDefaultDatabase().ListGetByIndexAsync<string>(key, 1);

        Assert.Equal("b", result);
    }

    [Fact]
    public async Task ListSetByIndex_ShouldUpdateElement_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().ListAddToRightAsync<string>(key, new[] { "a", "b", "c" });

        await Sut.GetDefaultDatabase().ListSetByIndexAsync(key, 1, "updated");

        var result = await Sut.GetDefaultDatabase().ListGetByIndexAsync<string>(key, 1);

        Assert.Equal("updated", result);
    }

    [Fact]
    public async Task ListTrim_ShouldKeepOnlyRange_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().ListAddToRightAsync<string>(key, new[] { "a", "b", "c", "d", "e" });

        await Sut.GetDefaultDatabase().ListTrimAsync(key, 1, 3);

        var length = await Sut.GetDefaultDatabase().ListLengthAsync(key);
        Assert.Equal(3, length);

        var first = await Sut.GetDefaultDatabase().ListGetByIndexAsync<string>(key, 0);
        Assert.Equal("b", first);
    }

    [Fact]
    public async Task ListRemove_ShouldRemoveMatchingElements_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().ListAddToRightAsync<string>(key, new[] { "a", "b", "a", "c", "a" });

        var removed = await Sut.GetDefaultDatabase().ListRemoveAsync(key, "a");

        Assert.Equal(3, removed);

        var length = await Sut.GetDefaultDatabase().ListLengthAsync(key);
        Assert.Equal(2, length);
    }

    [Fact]
    public async Task ListInsertBefore_ShouldInsertAtCorrectPosition_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().ListAddToRightAsync<string>(key, new[] { "a", "c" });

        var newLength = await Sut.GetDefaultDatabase().ListInsertBeforeAsync(key, "c", "b");

        Assert.Equal(3, newLength);

        var result = (await Sut.GetDefaultDatabase().ListRangeAsync<string>(key)).ToArray();
        Assert.Equal("a", result[0]);
        Assert.Equal("b", result[1]);
        Assert.Equal("c", result[2]);
    }

    [Fact]
    public async Task ListInsertAfter_ShouldInsertAtCorrectPosition_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().ListAddToRightAsync<string>(key, new[] { "a", "c" });

        var newLength = await Sut.GetDefaultDatabase().ListInsertAfterAsync(key, "a", "b");

        Assert.Equal(3, newLength);

        var result = (await Sut.GetDefaultDatabase().ListRangeAsync<string>(key)).ToArray();
        Assert.Equal("a", result[0]);
        Assert.Equal("b", result[1]);
        Assert.Equal("c", result[2]);
    }

    [Fact]
    public async Task ListMove_ShouldMoveElement_Async()
    {
        var sourceKey = Guid.NewGuid().ToString();
        var destKey = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().ListAddToRightAsync<string>(sourceKey, new[] { "a", "b", "c" });

        var moved = await Sut.GetDefaultDatabase().ListMoveAsync<string>(
            sourceKey, destKey, ListSide.Left, ListSide.Right);

        Assert.Equal("a", moved);

        var sourceLength = await Sut.GetDefaultDatabase().ListLengthAsync(sourceKey);
        Assert.Equal(2, sourceLength);

        var destLength = await Sut.GetDefaultDatabase().ListLengthAsync(destKey);
        Assert.Equal(1, destLength);
    }

    [Fact]
    public async Task ListMove_EmptySource_ShouldReturnDefault_Async()
    {
        var sourceKey = Guid.NewGuid().ToString();
        var destKey = Guid.NewGuid().ToString();

        var moved = await Sut.GetDefaultDatabase().ListMoveAsync<string>(
            sourceKey, destKey, ListSide.Left, ListSide.Right);

        Assert.Null(moved);
    }

    [Fact]
    public async Task ListAddToLeft_And_ListGetFromRight_Integration_Async()
    {
        var key = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().ListAddToLeftAsync(key, "first");
        await Sut.GetDefaultDatabase().ListAddToLeftAsync(key, "second");

        var right = await Sut.GetDefaultDatabase().ListGetFromRightAsync<string>(key);
        Assert.Equal("first", right);

        var left = await Sut.GetDefaultDatabase().ListGetFromLeftAsync<string>(key);
        Assert.Equal("second", left);
    }
}
