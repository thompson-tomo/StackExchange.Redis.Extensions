# HyperLogLog

HyperLogLog is a probabilistic data structure used for estimating the cardinality (unique count) of a set. It uses minimal memory (~12 KB) regardless of the number of elements, making it ideal for counting unique visitors, events, or any scenario requiring approximate distinct counts at scale.

## API Reference

| Method | Description |
|--------|-------------|
| `HyperLogLogAddAsync<T>(key, value)` | Add a single element |
| `HyperLogLogAddAsync<T>(key, values)` | Add multiple elements |
| `HyperLogLogLengthAsync(key)` | Get approximate cardinality of one key |
| `HyperLogLogLengthAsync(keys)` | Get approximate cardinality of the union of multiple keys |
| `HyperLogLogMergeAsync(dest, sourceKeys)` | Merge multiple HyperLogLogs into a destination key |

## Usage

```csharp
// Count unique visitors per page
await redis.HyperLogLogAddAsync("page:home:visitors", userId);

// Check how many unique visitors
var uniqueVisitors = await redis.HyperLogLogLengthAsync("page:home:visitors");

// Add multiple values at once
await redis.HyperLogLogAddAsync("page:about:visitors", new[] { "user1", "user2", "user3" });

// Count unique visitors across multiple pages
var totalUnique = await redis.HyperLogLogLengthAsync(new[] { "page:home:visitors", "page:about:visitors" });

// Merge daily counts into a monthly aggregate
await redis.HyperLogLogMergeAsync("visitors:2024:01", new[]
{
    "visitors:2024:01:01",
    "visitors:2024:01:02",
    "visitors:2024:01:03",
});
```

## Notes

- Values are serialized through the configured `ISerializer` before being added
- HyperLogLog has a standard error rate of 0.81%
- The `Add` methods return `true` if at least one internal register was altered (the cardinality estimate changed), `false` if the element was likely already counted
- Memory usage is constant (~12 KB per key) regardless of the number of unique elements
