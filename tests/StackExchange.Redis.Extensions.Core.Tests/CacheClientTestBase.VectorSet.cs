// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

using Xunit;

namespace StackExchange.Redis.Extensions.Core.Tests;

public abstract partial class CacheClientTestBase
{
    private static ReadOnlyMemory<float> Vec(params float[] values) => values.AsMemory();

    [Fact]
    public async Task VectorSetAdd_NewMember_ShouldReturnTrue_Async()
    {
        var key = Guid.NewGuid().ToString();
        var request = VectorSetAddRequest.Member("item1", Vec(1.0f, 0.0f, 0.0f));
        var result = await Sut.GetDefaultDatabase().VectorSetAddAsync(key, request);
        Assert.True(result);
    }

    [Fact]
    public async Task VectorSetAdd_ExistingMember_ShouldReturnFalse_Async()
    {
        var key = Guid.NewGuid().ToString();
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item1", Vec(1.0f, 0.0f, 0.0f)));

        var result = await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item1", Vec(0.0f, 1.0f, 0.0f)));
        Assert.False(result);
    }

    [Fact]
    public async Task VectorSetContains_ExistingMember_ShouldReturnTrue_Async()
    {
        var key = Guid.NewGuid().ToString();
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item1", Vec(1.0f, 0.0f, 0.0f)));

        var result = await Sut.GetDefaultDatabase().VectorSetContainsAsync(key, "item1");
        Assert.True(result);
    }

    [Fact]
    public async Task VectorSetContains_NonExistingMember_ShouldReturnFalse_Async()
    {
        var key = Guid.NewGuid().ToString();
        var result = await Sut.GetDefaultDatabase().VectorSetContainsAsync(key, "nonexistent");
        Assert.False(result);
    }

    [Fact]
    public async Task VectorSetLength_ShouldReturnCorrectCount_Async()
    {
        var key = Guid.NewGuid().ToString();
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item1", Vec(1.0f, 0.0f, 0.0f)));
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item2", Vec(0.0f, 1.0f, 0.0f)));
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item3", Vec(0.0f, 0.0f, 1.0f)));

        var count = await Sut.GetDefaultDatabase().VectorSetLengthAsync(key);
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task VectorSetLength_EmptyKey_ShouldReturnZero_Async()
    {
        var key = Guid.NewGuid().ToString();
        var count = await Sut.GetDefaultDatabase().VectorSetLengthAsync(key);
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task VectorSetDimension_ShouldReturnCorrectDimension_Async()
    {
        var key = Guid.NewGuid().ToString();
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item1", Vec(1.0f, 2.0f, 3.0f, 4.0f)));

        var dim = await Sut.GetDefaultDatabase().VectorSetDimensionAsync(key);
        Assert.Equal(4, dim);
    }

    [Fact]
    public async Task VectorSetRemove_ExistingMember_ShouldReturnTrue_Async()
    {
        var key = Guid.NewGuid().ToString();
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item1", Vec(1.0f, 0.0f, 0.0f)));

        var result = await Sut.GetDefaultDatabase().VectorSetRemoveAsync(key, "item1");
        Assert.True(result);

        var exists = await Sut.GetDefaultDatabase().VectorSetContainsAsync(key, "item1");
        Assert.False(exists);
    }

    [Fact]
    public async Task VectorSetRemove_NonExistingMember_ShouldReturnFalse_Async()
    {
        var key = Guid.NewGuid().ToString();
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item1", Vec(1.0f, 0.0f, 0.0f)));

        var result = await Sut.GetDefaultDatabase().VectorSetRemoveAsync(key, "nonexistent");
        Assert.False(result);
    }

    [Fact]
    public async Task VectorSetSimilaritySearch_ShouldReturnRankedResults_Async()
    {
        var key = Guid.NewGuid().ToString();
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("close", Vec(1.0f, 0.0f, 0.0f)));
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("medium", Vec(0.7f, 0.7f, 0.0f)));
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("far", Vec(0.0f, 0.0f, 1.0f)));

        var query = VectorSetSimilaritySearchRequest.ByVector(Vec(1.0f, 0.0f, 0.0f));
        query.Count = 3;
        using var results = await Sut.GetDefaultDatabase().VectorSetSimilaritySearchAsync(key, query);

        Assert.NotNull(results);
        Assert.True(results.Length > 0);
        Assert.Equal("close", results.Span[0].Member.ToString());
    }

    [Fact]
    public async Task VectorSetAttributes_SetAndGet_ShouldRoundTrip_Async()
    {
        var key = Guid.NewGuid().ToString();
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item1", Vec(1.0f, 0.0f, 0.0f)));

        const string json = "{\"category\":\"test\",\"score\":42}";
        var setResult = await Sut.GetDefaultDatabase().VectorSetSetAttributesJsonAsync(key, "item1", json);
        Assert.True(setResult);

        var retrieved = await Sut.GetDefaultDatabase().VectorSetGetAttributesJsonAsync(key, "item1");
        Assert.NotNull(retrieved);
        Assert.Contains("category", retrieved, StringComparison.Ordinal);
        Assert.Contains("test", retrieved, StringComparison.Ordinal);
    }

    [Fact]
    public async Task VectorSetInfo_ShouldReturnMetadata_Async()
    {
        var key = Guid.NewGuid().ToString();
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item1", Vec(1.0f, 0.0f, 0.0f)));
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item2", Vec(0.0f, 1.0f, 0.0f)));

        var info = await Sut.GetDefaultDatabase().VectorSetInfoAsync(key);
        Assert.NotNull(info);
    }

    [Fact]
    public async Task VectorSetRandomMember_ShouldReturnExistingMember_Async()
    {
        var key = Guid.NewGuid().ToString();
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item1", Vec(1.0f, 0.0f, 0.0f)));
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item2", Vec(0.0f, 1.0f, 0.0f)));

        var member = await Sut.GetDefaultDatabase().VectorSetRandomMemberAsync(key);
        Assert.False(member.IsNull);
        var memberStr = member.ToString();
        Assert.True(memberStr == "item1" || memberStr == "item2");
    }

    [Fact]
    public async Task VectorSetRandomMembers_ShouldReturnRequestedCount_Async()
    {
        var key = Guid.NewGuid().ToString();
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item1", Vec(1.0f, 0.0f, 0.0f)));
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item2", Vec(0.0f, 1.0f, 0.0f)));
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item3", Vec(0.0f, 0.0f, 1.0f)));

        var members = await Sut.GetDefaultDatabase().VectorSetRandomMembersAsync(key, 2);
        Assert.Equal(2, members.Length);
    }

    [Fact]
    public async Task VectorSetGetApproximateVector_ShouldReturnVector_Async()
    {
        var key = Guid.NewGuid().ToString();
        var original = new float[] { 1.0f, 0.0f, 0.0f };
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item1", original.AsMemory()));

        using var vector = await Sut.GetDefaultDatabase().VectorSetGetApproximateVectorAsync(key, "item1");
        Assert.NotNull(vector);
        Assert.Equal(original.Length, vector.Length);
    }

    [Fact]
    public async Task VectorSetGetLinks_ShouldReturnNeighbors_Async()
    {
        var key = Guid.NewGuid().ToString();
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item1", Vec(1.0f, 0.0f, 0.0f)));
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item2", Vec(0.9f, 0.1f, 0.0f)));
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item3", Vec(0.0f, 0.0f, 1.0f)));

        using var links = await Sut.GetDefaultDatabase().VectorSetGetLinksAsync(key, "item1");
        Assert.NotNull(links);
        Assert.True(links.Length > 0);
    }

    [Fact]
    public async Task VectorSetGetLinksWithScores_ShouldReturnNeighborsWithScores_Async()
    {
        var key = Guid.NewGuid().ToString();
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item1", Vec(1.0f, 0.0f, 0.0f)));
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item2", Vec(0.9f, 0.1f, 0.0f)));
        await Sut.GetDefaultDatabase().VectorSetAddAsync(key, VectorSetAddRequest.Member("item3", Vec(0.0f, 0.0f, 1.0f)));

        using var links = await Sut.GetDefaultDatabase().VectorSetGetLinksWithScoresAsync(key, "item1");
        Assert.NotNull(links);
        Assert.True(links.Length > 0);
    }
}
