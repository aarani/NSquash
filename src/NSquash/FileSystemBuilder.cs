using System.Diagnostics;
using NSquash.Compressors;

namespace NSquash;

public class FileSystemBuilder
{

    internal const uint MinBlockSize = 4096;
    internal const uint MaxBlockSize = 1048576;

    private ICompressor? _compressor = null;

    private uint _blockSize = 4096;

    private const ushort COMPRESSOR_OPTIONS_IS_PRESENT = 0x0400;

    // The only flag that actually carries information is the "Compressor options are present" flag. In fact,
    // this is the only flag that the Linux kernel implementation actually tests for.
    private readonly ushort _flags = COMPRESSOR_OPTIONS_IS_PRESENT;

    internal const ushort MajorVersion = 4;
    internal const ushort MinorVersion = 0;


    private FileSystemBuilder WithCompressor(ICompressor compressor)
    {
        _compressor = compressor;
        return this;
    }

    public FileSystemBuilder WithGzipCompressor(GzipCompressor.CompressionStrategy[] enabledStrategy, int compressionLevel = 6, int windowSize = 15)
    {
        var strategy = enabledStrategy.Aggregate((a, b) => a | b);
        return WithCompressor(new GzipCompressor((uint)compressionLevel, (ushort)windowSize, (ushort)strategy));
    }

    public FileSystemBuilder WithBlockSize(uint blockSize)
    {
        if (blockSize > MaxBlockSize || blockSize < MinBlockSize || (blockSize & (blockSize - 1)) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(blockSize), $"Block size must be a power of two between {MinBlockSize} and {MaxBlockSize}");
        }

        _blockSize = blockSize;
        return this;
    }
    
    public FileSystem Build()
    {
        if (_compressor == null)
        {
            throw new InvalidOperationException("Compressor must be set before building the filesystem");
        }

        Debug.Assert(_compressor != null);

        return new FileSystem(MajorVersion, MinorVersion, _flags, _blockSize, _compressor);
    }
}
