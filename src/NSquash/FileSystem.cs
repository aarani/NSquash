using System.Buffers;
using System.Buffers.Binary;
using System.Reflection;
using NSquash.Compressors;
using NSquash.Inodes;

namespace NSquash;

public class FileSystem
{
    private readonly ushort _majorVersion;
    private readonly ushort _minorVersion;
    private readonly ushort _flags;
    private readonly uint _blockSize;
    private readonly ICompressor _compressor;
    private readonly SuperBlock _superBlock;

    private Inode[] Inodes { get; set; } = [];

    internal FileSystem(ushort majorVersion, ushort minorVersion, ushort flags, uint blockSize, ICompressor compressor)
    {
        _majorVersion = majorVersion;
        _minorVersion = minorVersion;
        _flags = flags;
        _blockSize = blockSize;
        _compressor = compressor;
        _superBlock = new()
        {
            BlockSize = _blockSize,
            Flags = _flags,
            MajorVersion = _majorVersion,
            MinorVersion = _minorVersion,
            CompressorId = compressor.GetId(),
        };
    }

    private FileSystem(ushort majorVersion, ushort minorVersion, ushort flags, uint blockSize, ICompressor compressor, SuperBlock superBlock)
    {
        _majorVersion = majorVersion;
        _minorVersion = minorVersion;
        _flags = flags;
        _blockSize = blockSize;
        _compressor = compressor;
        _superBlock = superBlock;
    }

    public static async Task<FileSystem> ImportFromStream(Stream stream)
    {
        if (!stream.CanSeek) throw new ArgumentException("Stream must be seekable", nameof(stream));

        /*
            _________________
            |               |  Important information about the archive, including
            |  Superblock   |  locations of other sections.
            |_______________|
        */

        using var superBlockBytes = MemoryPool<byte>.Shared.Rent(SuperBlock.Size);
        await stream.ReadExactlyAsync(superBlockBytes.Memory[..SuperBlock.Size]);
        var superBlock = SuperBlock.FromSpan(superBlockBytes.Memory.Span[..SuperBlock.Size]);

        /*
            _________________
            |               |  If non-default compression options have been used,
            |  Compression  |  they can optionally be stored here, to facilitate
            |    options    |  later, offline editing of the archive.
            |_______________|
        */

        object[]? compressorArgs = null;

        if ((superBlock.Flags & 0x0400) != 0)
        {
            using var compressionOptions = MemoryPool<byte>.Shared.Rent(8);
            await stream.ReadExactlyAsync(compressionOptions.Memory[..8]);
            compressorArgs = [(ReadOnlyMemory<byte>)compressionOptions.Memory];
        }
        else
        {
            compressorArgs = null;
        }

        var compressor =
            Activator.CreateInstance(CompressorRegistry.RegisteredCompressors[superBlock.CompressorId], BindingFlags.NonPublic | BindingFlags.Instance, null, args: compressorArgs, null) as ICompressor
                ?? throw new InvalidOperationException("Failed to create compressor instance");

        if ((superBlock.Flags & 0x0400) != 0)
        {
            stream.Position -= 8 - compressor.GetMetadataLength();
        }

        var dataBlockOffset = stream.Position;
        
        var fileSystem = new FileSystem(
            superBlock.MajorVersion,
            superBlock.MinorVersion,
            superBlock.Flags,
            superBlock.BlockSize,
            compressor,
            superBlock
        );
        
        stream.Seek((long)superBlock.InodeTableOffset, SeekOrigin.Begin);
        /*
            |_______________|
            |               |  Metadata (ownership, permissions, etc) for
            |  Inode table  |  items in the archive.
            |_______________|
        */

        fileSystem.Inodes = new Inode[superBlock.InodeCount];
    
        int offset = (int)superBlock.InodeTableOffset;

        var buffer = new ArrayBufferWriter<byte>();

        do
        {
            var header = new byte[2];
            await stream.ReadExactlyAsync(header);
            var size = BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan());
            var actualSize = size & 0x7FFF;

            if ((size & 0x8000) == 0)
            {
                using var compressed = MemoryPool<byte>.Shared.Rent(actualSize);
                await stream.ReadExactlyAsync(compressed.Memory[..actualSize]);
                buffer.Write(await compressor.Decompress(compressed.Memory[..actualSize]));
            }
            else
            {
                using var decompressed = MemoryPool<byte>.Shared.Rent(actualSize);
                await stream.ReadExactlyAsync(decompressed.Memory[..actualSize]);
                buffer.Write(decompressed.Memory.Span[..actualSize]);
            }

            offset += actualSize + 2;
        } while (offset < (int)superBlock.DirectoryTableOffset);

        var result = buffer.WrittenMemory;
        int i = 0;
        offset = 0;
        
        do
        {
            var inodeType = BinaryPrimitives.ReadUInt16LittleEndian(result.Span[offset..(offset + 2)]);
            fileSystem.Inodes[i++] = InodeRegistry.CreateInode(superBlock.BlockSize, inodeType, result[offset..]);
        } while (i < superBlock.InodeCount);

        return fileSystem;
    }
}
