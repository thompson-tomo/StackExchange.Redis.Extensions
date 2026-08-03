# Migration Guide: v12 → v13

## Overview

v13.0.0 is a **non-breaking** release that adds new APIs without removing or changing existing ones. Upgrading from v12.x requires only a NuGet package update.

## What's New

### New Redis Data Structure APIs

| Domain | Methods Added | Issue |
|--------|--------------|-------|
| **HyperLogLog** | `HyperLogLogAddAsync`, `HyperLogLogLengthAsync`, `HyperLogLogMergeAsync` | #637 |
| **Distributed Lock** | `LockTakeAsync`, `LockReleaseAsync`, `LockExtendAsync`, `LockQueryAsync`, `LockAcquireAsync` | #638 |
| **Sorted Set (enriched)** | `SortedSetLengthAsync`, `SortedSetScoreAsync`, `SortedSetRankAsync`, `SortedSetIncrementAsync`, `SortedSetDecrementAsync`, `SortedSetPopAsync`, `SortedSetRandomMemberAsync`, `SortedSetRandomMembersAsync`, `SortedSetCombineAndStoreAsync` | #639 |
| **List (enriched)** | `ListAddToRightAsync`, `ListGetFromLeftAsync`, `ListLengthAsync`, `ListRangeAsync`, `ListGetByIndexAsync`, `ListSetByIndexAsync`, `ListTrimAsync`, `ListRemoveAsync`, `ListInsertBeforeAsync`, `ListInsertAfterAsync`, `ListMoveAsync` | #640 |
| **Lua Scripting** | `ScriptEvaluateAsync`, `ScriptEvaluateReadOnlyAsync` (raw + typed) | #641 |
| **Bitmap** | `StringSetBitAsync`, `StringGetBitAsync`, `StringBitCountAsync`, `StringBitOperationAsync`, `StringBitPositionAsync` | #642 |

### New Interfaces

- **`IRedisLock`** — Represents an acquired lock with `IAsyncDisposable` for automatic release. Properties: `Key`, `Value`, `IsAcquired`. Methods: `ExtendAsync(TimeSpan)`.

### StackExchange.Redis 3.x Compatibility

The Core package now accepts StackExchange.Redis `[2.12.14, 4.0.0)`, meaning it works with both 2.x and the upcoming 3.x releases (#656).

## Upgrade Steps

1. Update all `StackExchange.Redis.Extensions.*` NuGet packages to 13.0.0
2. No code changes required — all existing APIs remain unchanged
3. If you implement `IRedisDatabase` in a custom class, you will need to add implementations for the new methods

## Custom IRedisDatabase Implementations

If you have a custom class implementing `IRedisDatabase`, you must add the new methods. The interface is partial, with new methods in:

- `IRedisDatabase.HyperLogLog.cs`
- `IRedisDatabase.Lock.cs`
- `IRedisDatabase.Sort.cs` (10 new methods added to existing file)
- `IRedisDatabase.Lists.cs` (12 new methods added to existing file)
- `IRedisDatabase.Scripting.cs`
- `IRedisDatabase.Bitmap.cs`

This is the only potentially breaking change — it only affects users who have their own `IRedisDatabase` implementation outside of this library.
