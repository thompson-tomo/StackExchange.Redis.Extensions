// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;

namespace StackExchange.Redis.Extensions.Core;

/// <summary>
/// An <see cref="ICompressor"/> implementation using Brotli compression.
/// Higher compression ratio than GZip, especially for text-like data. No external dependencies.
/// </summary>
public class BrotliCompressor : ICompressor
{
    private const int BrotliDefaultWindow = 22;

    private readonly CompressionLevel compressionLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrotliCompressor"/> class.
    /// </summary>
    /// <param name="compressionLevel">The compression level to use. Defaults to <see cref="CompressionLevel.Fastest"/>.</param>
    public BrotliCompressor(CompressionLevel compressionLevel = CompressionLevel.Fastest)
    {
        this.compressionLevel = compressionLevel;
    }

    /// <inheritdoc/>
    public byte[] Compress(byte[] data)
    {
        // The one-shot encoder avoids the BrotliStream state machine and the MemoryStream growth copies:
        // a single rented worst-case buffer plus the exact-size result array.
        var maxLength = BrotliEncoder.GetMaxCompressedLength(data.Length);
        var buffer = ArrayPool<byte>.Shared.Rent(maxLength);

        try
        {
            if (!BrotliEncoder.TryCompress(data, buffer, out var written, GetQuality(compressionLevel), BrotliDefaultWindow))
                throw new InvalidOperationException("Brotli compression failed.");

            return buffer.AsSpan(0, written).ToArray();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <inheritdoc/>
    public byte[] Decompress(byte[] compressedData)
    {
        using var input = new MemoryStream(compressedData);
        using var brotli = new BrotliStream(input, CompressionMode.Decompress);

        // Pre-sized with a typical-ratio heuristic to avoid the growth copies of an empty MemoryStream.
        using var output = new MemoryStream(compressedData.Length * 4);

        brotli.CopyTo(output);

        return output.ToArray();
    }

    // Same CompressionLevel-to-quality mapping the runtime uses internally for BrotliStream.
    private static int GetQuality(CompressionLevel level) => level switch
    {
        CompressionLevel.NoCompression => 0,
        CompressionLevel.Fastest => 1,
        CompressionLevel.SmallestSize => 11,
        _ => 4,
    };
}
