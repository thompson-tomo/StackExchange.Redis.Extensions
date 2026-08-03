// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Text;

using MsgPack.Serialization;

using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.MsgPack;

/// <summary>
/// MsgPac implementation of <see cref="ISerializer"/>
/// </summary>
public class MsgPackObjectSerializer : ISerializer
{
    private readonly Encoding encoding;

    /// <summary>
    /// Initializes a new instance of the <see cref="MsgPackObjectSerializer"/> class.
    /// </summary>
    public MsgPackObjectSerializer()
        : this(null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MsgPackObjectSerializer"/> class.
    /// </summary>
    public MsgPackObjectSerializer(Action<SerializerRepository>? customSerializerRegistrar = null, Encoding? encoding = null)
    {
        customSerializerRegistrar?.Invoke(SerializationContext.Default.Serializers);

        encoding ??= Encoding.UTF8;

        this.encoding = encoding;
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(byte[]? serializedObject)
    {
        if (serializedObject == null)
            return default;

        if (typeof(T) == typeof(string))
            return (T)(object)encoding.GetString(serializedObject);

        var serializer = MessagePackSerializer.Get<T>();

        // UnpackSingleObject reads the array directly, without a wrapping MemoryStream.
        return serializer.UnpackSingleObject(serializedObject);
    }

    /// <inheritdoc/>
    public byte[] Serialize<T>(T? item)
    {
        if (item is null)
            return [];

        if (item is string str)
            return encoding.GetBytes(str);

        var serializer = MessagePackSerializer.Get(item.GetType());

        // PackSingleObject returns the buffer directly, without MemoryStream growth plus final copy.
        return serializer.PackSingleObject(item);
    }
}
