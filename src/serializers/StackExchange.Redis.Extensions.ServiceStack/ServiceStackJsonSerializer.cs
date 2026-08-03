// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.IO;

using ServiceStack.Text;

using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.ServiceStack;

/// <summary>
/// ServiceStack.Text implementation of <see cref="ISerializer"/>
/// </summary>
public class ServiceStackJsonSerializer : ISerializer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceStackJsonSerializer"/> class.
    /// </summary>
    public ServiceStackJsonSerializer()
    {
        JsConfig.Init(new Config
        {
            DateHandler = DateHandler.ISO8601,
            AppendUtcOffset = false, // Append "Z" on UTC and "+00:00" on Local times
            TimeSpanHandler = TimeSpanHandler.DurationFormat,
            AssumeUtc = true,
            SkipDateTimeConversion = true,
            IncludeNullValues = false,
            AlwaysUseUtc = true
        });
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(byte[]? serializedObject)
    {
        if (serializedObject == null)
            return default;

        // The stream API avoids materializing the payload as an intermediate UTF-16 string.
        using var ms = new MemoryStream(serializedObject, writable: false);
        return JsonSerializer.DeserializeFromStream<T>(ms);
    }

    /// <inheritdoc/>
    public byte[] Serialize<T>(T? item)
    {
        if (item == null)
            return [];

        using var ms = new MemoryStream(256);
        JsonSerializer.SerializeToStream(item, ms);
        return ms.ToArray();
    }
}
