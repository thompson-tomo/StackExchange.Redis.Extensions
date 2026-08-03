// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Buffers;
using System.IO;
using System.Text;

using Newtonsoft.Json;

using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.Newtonsoft;

/// <summary>
/// JSon.Net implementation of <see cref="ISerializer"/>
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="NewtonsoftSerializer"/> class.
/// </remarks>
/// <param name="settings">The settings.</param>
public class NewtonsoftSerializer(JsonSerializerSettings? settings) : ISerializer
{
    /// <summary>
    /// Encoding to use to convert string to byte[] and the other way around.
    /// </summary>
    /// <remarks>
    /// StackExchange.Redis uses Encoding.UTF8 to convert strings to bytes,
    /// hence we do same here. The BOM is suppressed to keep the on-wire bytes
    /// identical to the previous Encoding.UTF8.GetBytes-based implementation.
    /// </remarks>
    private static readonly UTF8Encoding encoding = new(false);

    // Writing straight to a stream avoids materializing every payload as an intermediate UTF-16 string,
    // and the cached serializers avoid the JsonSerializer.CreateDefault call JsonConvert performs per invocation.
    private readonly JsonSerializer writeSerializer = JsonSerializer.CreateDefault(settings);

    // JsonConvert.DeserializeObject enables CheckAdditionalContent unless explicitly configured:
    // a dedicated instance preserves that behavior on the read path.
    private readonly JsonSerializer readSerializer = CreateReadSerializer(settings);

    /// <summary>
    /// Initializes a new instance of the <see cref="NewtonsoftSerializer"/> class.
    /// </summary>
    public NewtonsoftSerializer()
        : this(null)
    {
    }

    /// <inheritdoc/>
    public byte[] Serialize<T>(T? item)
    {
        if (item == null)
            return [];

        using var ms = new MemoryStream(256);

        using (var streamWriter = new StreamWriter(ms, encoding, 1024, leaveOpen: true))
        using (var jsonWriter = new JsonTextWriter(streamWriter) { ArrayPool = JsonCharArrayPool.Instance })
            writeSerializer.Serialize(jsonWriter, item, item.GetType());

        return ms.ToArray();
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(byte[]? serializedObject)
    {
        if (serializedObject == null)
            return default;

        using var ms = new MemoryStream(serializedObject, writable: false);
        using var streamReader = new StreamReader(ms, encoding);
        using var jsonReader = new JsonTextReader(streamReader) { ArrayPool = JsonCharArrayPool.Instance };

        return readSerializer.Deserialize<T>(jsonReader);
    }

    private static JsonSerializer CreateReadSerializer(JsonSerializerSettings? settings)
    {
        var serializer = JsonSerializer.CreateDefault(settings);
        serializer.CheckAdditionalContent = true;
        return serializer;
    }

    private sealed class JsonCharArrayPool : IArrayPool<char>
    {
        public static readonly JsonCharArrayPool Instance = new();

        public char[] Rent(int minimumLength) => ArrayPool<char>.Shared.Rent(minimumLength);

        public void Return(char[]? array)
        {
            if (array != null)
                ArrayPool<char>.Shared.Return(array);
        }
    }
}
