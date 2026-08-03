// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

using Xunit;

namespace StackExchange.Redis.Extensions.Core.Tests;

public abstract partial class CacheClientTestBase
{
    [Fact]
    public async Task LockTake_WhenNotHeld_ShouldSucceed_Async()
    {
        var key = Guid.NewGuid().ToString();
        var value = Guid.NewGuid().ToString();

        var result = await Sut.GetDefaultDatabase().LockTakeAsync(key, value, TimeSpan.FromSeconds(10));

        Assert.True(result);
    }

    [Fact]
    public async Task LockTake_WhenAlreadyHeld_ShouldFail_Async()
    {
        var key = Guid.NewGuid().ToString();
        var value1 = Guid.NewGuid().ToString();
        var value2 = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().LockTakeAsync(key, value1, TimeSpan.FromSeconds(10));
        var result = await Sut.GetDefaultDatabase().LockTakeAsync(key, value2, TimeSpan.FromSeconds(10));

        Assert.False(result);
    }

    [Fact]
    public async Task LockRelease_WithCorrectValue_ShouldSucceed_Async()
    {
        var key = Guid.NewGuid().ToString();
        var value = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().LockTakeAsync(key, value, TimeSpan.FromSeconds(10));
        var result = await Sut.GetDefaultDatabase().LockReleaseAsync(key, value);

        Assert.True(result);
    }

    [Fact]
    public async Task LockRelease_WithWrongValue_ShouldFail_Async()
    {
        var key = Guid.NewGuid().ToString();
        var value = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().LockTakeAsync(key, value, TimeSpan.FromSeconds(10));
        var result = await Sut.GetDefaultDatabase().LockReleaseAsync(key, "wrong-value");

        Assert.False(result);
    }

    [Fact]
    public async Task LockExtend_WithCorrectValue_ShouldSucceed_Async()
    {
        var key = Guid.NewGuid().ToString();
        var value = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().LockTakeAsync(key, value, TimeSpan.FromSeconds(10));
        var result = await Sut.GetDefaultDatabase().LockExtendAsync(key, value, TimeSpan.FromSeconds(20));

        Assert.True(result);
    }

    [Fact]
    public async Task LockQuery_WhenHeld_ShouldReturnValue_Async()
    {
        var key = Guid.NewGuid().ToString();
        var value = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().LockTakeAsync(key, value, TimeSpan.FromSeconds(10));
        var result = await Sut.GetDefaultDatabase().LockQueryAsync(key);

        Assert.Equal(value, result);
    }

    [Fact]
    public async Task LockQuery_WhenNotHeld_ShouldReturnNull_Async()
    {
        var key = Guid.NewGuid().ToString();
        var result = await Sut.GetDefaultDatabase().LockQueryAsync(key);

        Assert.Null(result);
    }

    [Fact]
    public async Task LockAcquire_ShouldAcquireAndReturn_Async()
    {
        var key = Guid.NewGuid().ToString();

        await using var lockObj = await Sut.GetDefaultDatabase().LockAcquireAsync(key, TimeSpan.FromSeconds(10));

        Assert.NotNull(lockObj);
        Assert.True(lockObj.IsAcquired);
        Assert.Equal(key, lockObj.Key);
        Assert.False(string.IsNullOrEmpty(lockObj.Value));
    }

    [Fact]
    public async Task LockAcquire_WhenContested_ShouldReturnNull_Async()
    {
        var key = Guid.NewGuid().ToString();
        var value = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().LockTakeAsync(key, value, TimeSpan.FromSeconds(30));

        var lockObj = await Sut.GetDefaultDatabase().LockAcquireAsync(
            key,
            TimeSpan.FromSeconds(10),
            maxRetries: 1,
            retryDelay: TimeSpan.FromMilliseconds(50));

        Assert.Null(lockObj);
    }

    [Fact]
    public async Task LockAcquire_Dispose_ShouldReleaseLock_Async()
    {
        var key = Guid.NewGuid().ToString();

        var lockObj = await Sut.GetDefaultDatabase().LockAcquireAsync(key, TimeSpan.FromSeconds(10));
        Assert.NotNull(lockObj);

        await lockObj.DisposeAsync();

        var queryResult = await Sut.GetDefaultDatabase().LockQueryAsync(key);
        Assert.Null(queryResult);
    }

    [Fact]
    public async Task LockAcquire_Extend_ShouldSucceed_Async()
    {
        var key = Guid.NewGuid().ToString();

        await using var lockObj = await Sut.GetDefaultDatabase().LockAcquireAsync(key, TimeSpan.FromSeconds(5));
        Assert.NotNull(lockObj);

        var extended = await lockObj.ExtendAsync(TimeSpan.FromSeconds(30));
        Assert.True(extended);
    }

    [Fact]
    public async Task LockRelease_AfterExpiry_ShouldAllowReacquisition_Async()
    {
        var key = Guid.NewGuid().ToString();
        var value = Guid.NewGuid().ToString();

        await Sut.GetDefaultDatabase().LockTakeAsync(key, value, TimeSpan.FromMilliseconds(100));

        await Task.Delay(200);

        var result = await Sut.GetDefaultDatabase().LockTakeAsync(key, "new-holder", TimeSpan.FromSeconds(10));
        Assert.True(result);
    }
}
