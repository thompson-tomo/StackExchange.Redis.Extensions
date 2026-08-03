// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Threading;

using ZstdSharp;

namespace StackExchange.Redis.Extensions.Core;

/// <summary>
/// An <see cref="ICompressor"/> implementation using Zstandard compression via ZstdSharp.
/// Zstd offers an excellent balance between compression ratio and speed.
/// </summary>
public class ZstdSharpCompressor : ICompressor
{
    // Zstd contexts are designed for reuse and are expensive to initialize, but they are not thread-safe:
    // one context per thread replaces the previous new-context-per-call pattern.
    private readonly ThreadLocal<Compressor> compressor;
    private readonly ThreadLocal<Decompressor> decompressor = new(static () => new Decompressor());

    /// <summary>
    /// Initializes a new instance of the <see cref="ZstdSharpCompressor"/> class.
    /// </summary>
    /// <param name="compressionLevel">The Zstd compression level (1-22). Defaults to 3 (fast).</param>
    public ZstdSharpCompressor(int compressionLevel = 3)
    {
        compressor = new(() => new Compressor(compressionLevel));
    }

    /// <inheritdoc/>
    public byte[] Compress(byte[] data)
    {
        var bound = Compressor.GetCompressBound(data.Length);
        var buffer = ArrayPool<byte>.Shared.Rent(bound);

        try
        {
            var written = compressor.Value!.Wrap(data, buffer);

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
        var size = Decompressor.GetDecompressedSize(compressedData);

        // Frames produced by Wrap always carry the content size; the fallback covers foreign frames without it.
        if (size == 0 || size > int.MaxValue)
            return decompressor.Value!.Unwrap(compressedData).ToArray();

        var result = new byte[(int)size];
        var written = decompressor.Value!.Unwrap(compressedData, result);

        return written == result.Length ? result : result.AsSpan(0, written).ToArray();
    }
}
