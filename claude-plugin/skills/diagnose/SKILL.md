---
name: redis-diagnose
description: Troubleshoot common StackExchange.Redis.Extensions issues — timeouts, connection failures, serialization problems, pool exhaustion, distributed locks, Lua scripting
---

# Redis Diagnose

Diagnose and fix common issues with StackExchange.Redis.Extensions.

## When to use

When the user reports:
- Timeout exceptions
- Connection failures
- Serialization errors
- Pool exhaustion
- Pub/Sub messages not being received
- Distributed lock failures
- Lua scripting errors
- Performance issues

## Diagnostic Tree

### RedisTimeoutException
**Symptoms:** `StackExchange.Redis.RedisTimeoutException`

**Check in order:**
1. **SyncTimeout too low** — default is 5000ms, increase for slow networks
   ```csharp
   config.SyncTimeout = 10000; // 10 seconds
   ```
2. **Pool size too small** — default 5, increase for high-throughput
   ```csharp
   config.PoolSize = 10;
   ```
3. **Connection strategy** — switch to LeastLoaded if using RoundRobin
   ```csharp
   config.ConnectionSelectionStrategy = ConnectionSelectionStrategy.LeastLoaded;
   ```
4. **Large values** — enable compression to reduce payload size
   ```csharp
   services.AddRedisCompression<LZ4Compressor>();
   ```
5. **ThreadPool starvation** — check with `ThreadPool.GetAvailableThreads()`, increase min threads

### RedisConnectionException
**Symptoms:** `SocketClosed`, `ConnectionFailed`

**Check:**
1. **Redis server reachable?** — `redis-cli -h <host> -p <port> ping`
2. **Firewall/NSG rules** — port 6379 (or 6380 for TLS) open?
3. **TLS misconfiguration** — if using Ssl=true, check certificate callbacks
4. **Sentinel misconfiguration** — ServiceName must match Redis master name exactly
5. **Azure Cache for Redis** — ensure Managed Identity is configured if not using password

### Serialization Errors
**Symptoms:** `JsonException`, `InvalidOperationException`, corrupt data

**Check:**
1. **Type mismatch** — GetAsync<T> must use same T as AddAsync<T>
2. **String values are JSON-encoded** — "hello" is stored as "\"hello\"", this is by design
3. **Compression migration** — enabling compression makes old (uncompressed) data unreadable
   - Error: `InvalidOperationException: Failed to decompress data from Redis`
   - Fix: flush the database or read old data without compression first
4. **Value type quirk** — `GetAsync<int>()` returns 0 (not null) for missing keys because `default(int)` is `0`. Use `GetAsync<int?>()` to distinguish missing keys from actual zero values.

### Pub/Sub Not Receiving Messages
**Check:**
1. **KeyPrefix** — channels are automatically prefixed. Don't add prefix manually.
2. **Serializer mismatch** — publisher and subscriber must use the same serializer
3. **Handler exceptions** — check logs for EventId 4001 errors. Handlers that throw don't crash but the message is lost.
4. **Different connection pools** — ensure pub and sub use the same IRedisDatabase instance

### Pool Exhaustion / All Connections Down
**Symptoms:** All operations fail, logs show EventId 1003

**Check:**
1. **Pool health** — inject `IRedisClient` and call `client.ConnectionPoolManager.GetConnectionInformation()`
2. **Use health check** — register `builder.Services.AddHealthChecks().AddRedisExtensionsHealthCheck()` to monitor pool status automatically (returns Healthy/Degraded/Unhealthy)
3. **Redis server overloaded** — check `INFO clients` on Redis
4. **Network partition** — the pool skips disconnected connections automatically and logs warnings
5. **Dispose pattern** — ensure IRedisConnectionPoolManager is not disposed prematurely

### IDistributedCache Issues
**Symptoms:** Data not found, expiration not working, migration issues

**Check:**
1. **Registration order** — `AddRedisDistributedCache()` must be called after `AddStackExchangeRedisExtensions<T>()`
2. **KeyPrefix applies** — IDistributedCache goes through `IRedisDatabase.Database` which uses `WithKeyPrefix`. Cache keys are prefixed automatically.
3. **Migration from Microsoft provider** — hash schema is compatible (`data`/`absexp`/`sldexp` fields), but key prefix format may differ (this library uses `KeyPrefix`, Microsoft uses `InstanceName`)
4. **Sliding expiration not refreshing** — `Get` and `Refresh` both refresh the TTL. Check that `SlidingExpiration` was set in `DistributedCacheEntryOptions`

### Keyed DI Not Resolving
**Symptoms:** `[FromKeyedServices("name")]` returns null

**Check:**
1. **Config must have a Name** — `RedisConfiguration.Name` must be non-empty for keyed registration
2. **Use eager overloads** — keyed services are only registered with the overloads that receive `RedisConfiguration` directly, NOT the `Func<IServiceProvider, ...>` overload
3. **Name must match exactly** — `[FromKeyedServices("cache")]` must match `config.Name = "cache"` (case-sensitive)

### Distributed Lock Issues
**Symptoms:** Lock not acquired, deadlocks, lock lost during processing

**Check:**
1. **LockAcquireAsync returns null** — increase `maxRetries` or `retryDelay`, the resource may be legitimately contended
2. **Lock expires during processing** — use `lockObj.ExtendAsync(TimeSpan)` to extend the TTL before it expires
3. **Lock not released on error** — always use `await using` pattern, never try/finally with manual release
4. **Deadlock** — ensure lock expiry is always set. If the holder crashes, the lock auto-expires
5. **This is a single-instance lock** — for Redis Cluster, consider Redlock algorithm

### Lua Script Errors
**Symptoms:** `RedisServerException`, NOSCRIPT, wrong return type

**Check:**
1. **NOSCRIPT error** — the script was cached but the server restarted. SE.Redis retries with EVAL automatically, but check for transient failures
2. **Wrong return type** — Lua `return 1` returns `long`, `return "hello"` returns `string`. Use explicit casts on RedisResult
3. **CROSSSLOT error in Cluster** — all KEYS[] must hash to the same slot. Use hash tags: `{user}:counter`, `{user}:data`
4. **Script blocks Redis** — Lua runs atomically, blocking the event loop. Keep scripts short (<1ms). Use read-only variant (`ScriptEvaluateReadOnlyAsync`) for read operations to route to replicas
5. **Typed deserialization fails** — `ScriptEvaluateAsync<T>` passes the raw bytes through ISerializer. Ensure the script returns a value serialized in the same format

### Performance Issues
**Check:**
1. **Enable logging** — set log level to Debug to see connection selection
   ```json
   { "Logging": { "LogLevel": { "StackExchange.Redis.Extensions.Core": "Debug" } } }
   ```
2. **Check outstanding commands** — pool info shows outstanding count per connection
3. **Use compression** for large objects — LZ4 adds ~1ms latency but reduces network 5-10x
4. **Use AddAllAsync** for bulk writes instead of loop of AddAsync
5. **Use GetAllAsync** for bulk reads (note: requires `HashSet<string>` for keys, not arrays)

## Logging Reference

| EventId | Level | Meaning |
|---------|-------|---------|
| 1001 | Info | Pool initialized successfully |
| 1003 | Warning | All connections disconnected — degraded mode |
| 1006 | Error | Pool initialization failed |
| 2001 | Error | Connection failed |
| 2002 | Warning | Connection restored |
| 4001 | Error | Pub/Sub handler threw exception |

Enable with:
```json
{ "Logging": { "LogLevel": { "StackExchange.Redis.Extensions.Core": "Information" } } }
```
