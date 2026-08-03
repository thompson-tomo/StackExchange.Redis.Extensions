// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Buffers;

using ProtoBuf;

using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.Protobuf;

/// <summary>
/// Protobuf-net implementation of <see cref="ISerializer"/>
/// </summary>
public class ProtobufSerializer : ISerializer
{
    /// <inheritdoc/>
    public byte[] Serialize<T>(T? item)
    {
        if (item == null)
            return [];

        // IBufferWriter avoids the MemoryStream layer; the final ToArray is the only full copy.
        var buffer = new ArrayBufferWriter<byte>(256);

        Serializer.Serialize(buffer, item);

        return buffer.WrittenSpan.ToArray();
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(byte[]? serializedObject)
    {
        if (serializedObject == null)
            return default;

        // The span-based overload reads the array directly, without a wrapping MemoryStream.
        return Serializer.Deserialize<T>((ReadOnlyMemory<byte>)serializedObject);
    }
}
