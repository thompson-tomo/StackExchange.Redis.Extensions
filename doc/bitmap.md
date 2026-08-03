# Bitmap Operations

Redis bitmaps provide space-efficient boolean tracking using individual bits within string values. Each bit can be independently set, cleared, and queried, making bitmaps ideal for tracking user activity, feature flags, and real-time analytics.

## API Reference

| Method | Description |
|--------|-------------|
| `StringSetBitAsync(key, offset, value)` | Set or clear the bit at a specific offset |
| `StringGetBitAsync(key, offset)` | Get the bit value at a specific offset |
| `StringBitCountAsync(key, start, end)` | Count the number of set bits (1s) in a range |
| `StringBitOperationAsync(operation, dest, keys)` | Perform bitwise AND/OR/XOR/NOT across keys |
| `StringBitPositionAsync(key, bit, start, end)` | Find the position of the first 0 or 1 bit |

## Usage

### Daily Active Users

```csharp
// Track user login (1 bit per user ID, 1 key per day)
await redis.StringSetBitAsync("dau:2024-01-15", userId, true);

// Count daily active users
var activeCount = await redis.StringBitCountAsync("dau:2024-01-15");

// Check if a specific user was active
var wasActive = await redis.StringGetBitAsync("dau:2024-01-15", userId);
```

### Weekly Active Users

```csharp
// Compute users active on ALL 7 days (AND operation)
await redis.StringBitOperationAsync(Bitwise.And, "wau:week1:all-days", new[]
{
    "dau:2024-01-15", "dau:2024-01-16", "dau:2024-01-17",
    "dau:2024-01-18", "dau:2024-01-19", "dau:2024-01-20", "dau:2024-01-21",
});

// Compute users active on ANY day (OR operation)
await redis.StringBitOperationAsync(Bitwise.Or, "wau:week1:any-day", new[]
{
    "dau:2024-01-15", "dau:2024-01-16", "dau:2024-01-17",
    "dau:2024-01-18", "dau:2024-01-19", "dau:2024-01-20", "dau:2024-01-21",
});

var dailyRetention = await redis.StringBitCountAsync("wau:week1:all-days");
var weeklyReach = await redis.StringBitCountAsync("wau:week1:any-day");
```

### Feature Flags

```csharp
// Enable feature for a user
await redis.StringSetBitAsync("feature:dark-mode", userId, true);

// Check if feature is enabled
var isDarkMode = await redis.StringGetBitAsync("feature:dark-mode", userId);

// Find the first user with the feature enabled
var firstUser = await redis.StringBitPositionAsync("feature:dark-mode", true);
```

## Notes

- Bitmap operations do not use serialization — they work directly with bit offsets
- Memory usage: 1 bit per user. For 1 million users, that's ~122 KB per key
- The `start`/`end` parameters in `StringBitCountAsync` and `StringBitPositionAsync` refer to byte positions, not bit positions
- `StringBitOperationAsync` supports `Bitwise.And`, `Bitwise.Or`, `Bitwise.Xor`, and `Bitwise.Not`
