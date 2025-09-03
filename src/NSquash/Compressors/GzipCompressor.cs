
using System.Buffers.Binary;
using System.IO.Compression;
using System.IO.Pipelines;

namespace NSquash.Compressors;
    
public record GzipCompressor : ICompressor
{
    private readonly uint _compressionLevel;
    private readonly ushort _windowSize;
    private readonly ushort _strategy;

    internal GzipCompressor (uint compressionLevel, ushort windowSize, ushort strategy)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(compressionLevel, (uint)9);
        ArgumentOutOfRangeException.ThrowIfLessThan(compressionLevel, (uint)1);
        _compressionLevel = compressionLevel;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(windowSize, (uint)15);
        ArgumentOutOfRangeException.ThrowIfLessThan(windowSize, (uint)8);
        _windowSize = windowSize;
        _strategy = strategy;
    }

    internal GzipCompressor(ReadOnlyMemory<byte> metadata)
    {
        if (metadata.Length < 8)
        {
            throw new ArgumentException("Metadata length is insufficient for GzipCompressor", nameof(metadata));
        }
        _compressionLevel = BinaryPrimitives.ReadUInt32LittleEndian(metadata.Span);
        _windowSize = BinaryPrimitives.ReadUInt16LittleEndian(metadata.Span[4..]);
        _strategy = BinaryPrimitives.ReadUInt16LittleEndian(metadata.Span[6..]);
    }

    internal GzipCompressor() : this(6, 15, 1) { }

    public static GzipCompressor Default => new(6, 15, 1);

    ushort ICompressor.GetId() => 1;

    int ICompressor.GetMetadataLength() => 8;
    
    void ICompressor.WriteMetadata(Span<byte> buffer)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(buffer, _compressionLevel);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer[4..], _windowSize);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer[6..], _strategy);
    }

    async Task<byte[]> ICompressor.Decompress(ReadOnlyMemory<byte> data)
    {
        using var input = new MemoryStream(data.ToArray());
        using var output = new MemoryStream(data.Length * 3); //heuristic, should be enough for most cases

        //FIXME: do we need to set the window size and strategy here?
        using (var deflateStream = new ZLibStream(input, CompressionMode.Decompress))
        {
            await deflateStream.CopyToAsync(output);
            await output.FlushAsync();
        }

        return output.ToArray();
    }

    public enum CompressionStrategy : ushort
    {
        Default = 1,
        Filtered = 2,
        HuffmanOnly = 4,
        RunLengthEncoded = 8,
        Fixed = 16
    }
}