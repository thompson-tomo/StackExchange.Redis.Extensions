# Lua Scripting

Execute Lua scripts directly on the Redis server for atomic multi-command operations. Lua scripts run atomically — no other command will execute while the script is running.

## API Reference

| Method | Description |
|--------|-------------|
| `ScriptEvaluateAsync(script, keys, values)` | Execute a Lua script and return raw `RedisResult` |
| `ScriptEvaluateAsync<T>(script, keys, values)` | Execute a Lua script and deserialize the result |
| `ScriptEvaluateReadOnlyAsync(script, keys, values)` | Execute a read-only Lua script (suitable for replica routing) |
| `ScriptEvaluateReadOnlyAsync<T>(script, keys, values)` | Execute a read-only Lua script and deserialize the result |

## Usage

### Basic script execution

```csharp
// Simple return value
var result = await redis.ScriptEvaluateAsync("return 42");
var value = (long)result; // 42

// Using KEYS and ARGV
var sum = await redis.ScriptEvaluateAsync(
    "return tonumber(ARGV[1]) + tonumber(ARGV[2])",
    values: new RedisValue[] { 10, 20 });
// sum = 30
```

### Atomic operations

```csharp
// Atomic increment with ceiling (cap at 100)
var script = @"
    local current = tonumber(redis.call('GET', KEYS[1]) or '0')
    if current < tonumber(ARGV[1]) then
        return redis.call('INCR', KEYS[1])
    end
    return current";

var capped = await redis.ScriptEvaluateAsync(
    script,
    new RedisKey[] { "counter:views" },
    new RedisValue[] { 100 });
```

### Rate limiting

```csharp
// Sliding window rate limiter
var rateLimitScript = @"
    local key = KEYS[1]
    local limit = tonumber(ARGV[1])
    local window = tonumber(ARGV[2])
    local current = tonumber(redis.call('GET', key) or '0')
    if current < limit then
        redis.call('INCR', key)
        if current == 0 then
            redis.call('EXPIRE', key, window)
        end
        return 1
    end
    return 0";

var allowed = (long)await redis.ScriptEvaluateAsync(
    rateLimitScript,
    new RedisKey[] { $"ratelimit:{userId}" },
    new RedisValue[] { 100, 60 }); // 100 requests per 60 seconds
```

### Read-only scripts (replica routing)

```csharp
// Read-only scripts can be routed to replicas for better throughput
var stats = await redis.ScriptEvaluateReadOnlyAsync(
    "return redis.call('GET', KEYS[1])",
    new RedisKey[] { "stats:daily" });
```

## Notes

- Always use `KEYS[n]` and `ARGV[n]` in scripts instead of hardcoded key names — this enables proper cluster routing
- The generic `ScriptEvaluateAsync<T>` method deserializes the result bytes through the configured `ISerializer`
- The raw `ScriptEvaluateAsync` method returns `RedisResult` which can be cast to `long`, `string`, `byte[]`, or `RedisResult[]` (for Lua tables)
- Read-only variants (`ScriptEvaluateReadOnlyAsync`) use `EVALRO` and can be routed to replicas
- Lua scripts execute atomically — use them sparingly for long-running operations as they block the Redis event loop
