// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Buffers;

using Snappier;

namespace StackExchange.Redis.Extensions.Core;

/// <summary>
/// An <see cref="ICompressor"/> implementation using Snappy compression via Snappier.
/// Snappy prioritizes speed over compression ratio, similar to LZ4.
/// </summary>
public class SnappierCompressor : ICompressor
{
    /// <inheritdoc/>
    public byte[] Compress(byte[] data)
    {
        // The worst-case buffer (~1.17x input) is only transient: renting it avoids a heap (or LOH) allocation per call.
        var maxLength = Snappy.GetMaxCompressedLength(data.Length);
        var buffer = ArrayPool<byte>.Shared.Rent(maxLength);

        try
        {
            var compressedLength = Snappy.Compress(data, buffer);

            return buffer.AsSpan(0, compressedLength).ToArray();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <inheritdoc/>
    public byte[] Decompress(byte[] compressedData)
    {
        var decompressedLength = Snappy.GetUncompressedLength(compressedData);
        var buffer = new byte[decompressedLength];
        var actualLength = Snappy.Decompress(compressedData, buffer);

        // The header-declared length matches the actual output, so the buffer can be returned as-is
        // instead of being copied a second time.
        return actualLength == decompressedLength ? buffer : buffer.AsSpan(0, actualLength).ToArray();
    }
}
