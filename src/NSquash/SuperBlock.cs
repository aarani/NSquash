
using System.Buffers.Binary;
using System.Diagnostics;

namespace NSquash;

internal class SuperBlock
{
    private uint _blockSize;

    public static int Size => 96;

    public uint Magic { get; } = 0x73717368;
    public uint InodeCount { get; set; }
    public uint ModificationTime { get; set; }
    public uint BlockSize
    {
        get
        {
            return _blockSize;
        }
        set
        {
            if (value > FileSystemBuilder.MaxBlockSize || value < FileSystemBuilder.MinBlockSize || (value & (value - 1)) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"Block size must be a power of two between {FileSystemBuilder.MinBlockSize} and {FileSystemBuilder.MaxBlockSize}");
            }

            _blockSize = value;
        }
    } 
    public uint FragmentsCount { get; set; }
    public ushort CompressorId { get; set; }
    public uint BlockSizeLog => (uint)Math.Log2(BlockSize);
    public ushort Flags { get; set; }
    public ushort IdEntriesCount { get; set; }
    public ushort MajorVersion { get; set; }
    public ushort MinorVersion { get; set; }
    public ulong RootInode { get; set; }
    public ulong BytesUsed { get; set; }
    public ulong IdTableOffset { get; set; }
    public ulong XattrOffset { get; set; }
    public ulong InodeTableOffset { get; set; }
    public ulong DirectoryTableOffset { get; set; }
    public ulong FragmentTableOffset { get; set; }
    public ulong ExportTableOffset { get; set; }

    internal static SuperBlock FromSpan(ReadOnlySpan<byte> span)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(span.Length, Size);

        int offset = 0;

        SuperBlock superBlock = new();

        var magic = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(offset, sizeof(UInt32)));
        offset += sizeof(UInt32);

        if (magic != superBlock.Magic)
        {
            throw new InvalidDataException("Invalid magic number in superblock");
        }

        superBlock.InodeCount = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(offset, sizeof(UInt32)));
        offset += sizeof(UInt32);

        superBlock.ModificationTime = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(offset, sizeof(UInt32)));
        offset += sizeof(UInt32);

        superBlock.BlockSize = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(offset, sizeof(UInt32)));
        offset += sizeof(UInt32);

        superBlock.FragmentsCount = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(offset, sizeof(UInt32)));
        offset += sizeof(UInt32);

        superBlock.CompressorId = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(offset, sizeof(UInt16)));
        offset += sizeof(UInt16);

        if (BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(offset, sizeof(UInt16))) != superBlock.BlockSizeLog)
        {
            throw new InvalidDataException("Block size log does not match block size, corruption likely");
        }
        offset += sizeof(UInt16);

        superBlock.Flags = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(offset, sizeof(UInt16)));
        offset += sizeof(UInt16);

        superBlock.IdEntriesCount = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(offset, sizeof(UInt16)));
        offset += sizeof(UInt16);

        superBlock.MajorVersion = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(offset, sizeof(UInt16)));
        offset += sizeof(UInt16);

        superBlock.MinorVersion = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(offset, sizeof(UInt16)));
        offset += sizeof(UInt16);

        if (superBlock.MajorVersion != FileSystemBuilder.MajorVersion || superBlock.MinorVersion != FileSystemBuilder.MinorVersion)
        {
            throw new NotSupportedException($"Unsupported version {superBlock.MajorVersion}.{superBlock.MinorVersion}");
        }

        superBlock.RootInode = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(offset, sizeof(UInt64)));
        offset += sizeof(UInt64);

        superBlock.BytesUsed = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(offset, sizeof(UInt64)));
        offset += sizeof(UInt64);

        superBlock.IdTableOffset = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(offset, sizeof(UInt64)));
        offset += sizeof(UInt64);

        superBlock.XattrOffset = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(offset, sizeof(UInt64)));
        offset += sizeof(UInt64);

        superBlock.InodeTableOffset = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(offset, sizeof(UInt64)));
        offset += sizeof(UInt64);

        superBlock.DirectoryTableOffset = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(offset, sizeof(UInt64)));
        offset += sizeof(UInt64);

        superBlock.FragmentTableOffset = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(offset, sizeof(UInt64)));
        offset += sizeof(UInt64);

        superBlock.ExportTableOffset = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(offset, sizeof(UInt64)));
        offset += sizeof(UInt64);

        Debug.Assert(offset == Size);

        return superBlock;
    }
}