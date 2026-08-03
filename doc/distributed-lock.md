# Distributed Lock

Redis-based distributed locking for coordinating access to shared resources across multiple processes or services. Built on top of SE.Redis's `LockTake`/`LockRelease`/`LockExtend` primitives.

## API Reference

### Low-Level API

| Method | Description |
|--------|-------------|
| `LockTakeAsync(key, value, expiry)` | Acquire a lock with a specific holder value |
| `LockReleaseAsync(key, value)` | Release a lock (only if held by the specified value) |
| `LockExtendAsync(key, value, expiry)` | Extend the lock TTL (only if held by the specified value) |
| `LockQueryAsync(key)` | Query the current lock holder value (null if not locked) |

### High-Level API (IAsyncDisposable)

| Method | Description |
|--------|-------------|
| `LockAcquireAsync(key, expiry, maxRetries, retryDelay)` | Acquire a lock with retry logic, returns `IRedisLock` |

`IRedisLock` properties and methods:

| Member | Description |
|--------|-------------|
| `Key` | The lock key |
| `Value` | The unique holder identifier (auto-generated GUID) |
| `IsAcquired` | Whether the lock was successfully acquired |
| `ExtendAsync(expiry)` | Extend the lock TTL |
| `DisposeAsync()` | Release the lock automatically |

## Usage

### Using the IAsyncDisposable wrapper (recommended)

```csharp
// Acquire a lock with automatic release on dispose
await using var lockObj = await redis.LockAcquireAsync(
    "resource:order:123",
    expiry: TimeSpan.FromSeconds(30),
    maxRetries: 5,
    retryDelay: TimeSpan.FromMilliseconds(200));

if (lockObj is null)
{
    // Could not acquire the lock after all retries
    throw new InvalidOperationException("Could not acquire lock");
}

// Critical section — lock is held
await ProcessOrder(123);

// Lock is automatically released when `lockObj` is disposed
```

### Extending a lock

```csharp
await using var lockObj = await redis.LockAcquireAsync("long-task", TimeSpan.FromSeconds(10));

if (lockObj is not null)
{
    // ... doing work ...

    // Need more time? Extend the lock
    var extended = await lockObj.ExtendAsync(TimeSpan.FromSeconds(30));
    if (!extended)
    {
        // Lock was lost (expired before we could extend)
        return;
    }

    // ... continue work ...
}
```

### Using the low-level API

```csharp
var lockKey = "resource:inventory";
var lockValue = Guid.NewGuid().ToString();
var expiry = TimeSpan.FromSeconds(30);

if (await redis.LockTakeAsync(lockKey, lockValue, expiry))
{
    try
    {
        // Critical section
        await UpdateInventory();
    }
    finally
    {
        await redis.LockReleaseAsync(lockKey, lockValue);
    }
}
```

## Notes

- The lock value acts as a holder identifier — only the holder with the matching value can release or extend the lock
- `LockAcquireAsync` generates a random GUID as the lock value automatically
- The default retry delay is 200ms with 3 retries
- `LockAcquireAsync` returns `null` (not an exception) when the lock cannot be acquired
- Always set a reasonable expiry to prevent deadlocks if the holder crashes
- This is a single-instance lock — for multi-instance Redis (Cluster), consider Redlock
