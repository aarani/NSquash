
using System.Buffers.Binary;

namespace NSquash.Inodes;

internal class ExtendedFileInode : Inode
{
    public ExtendedFileInode(uint blockSize, ReadOnlyMemory<byte> data) : base(blockSize, data)
    {
        int offset = CommonMetadataLength;
        BlocksStartOffset = BinaryPrimitives.ReadUInt64LittleEndian(data.Span[offset..(offset + sizeof(UInt64))]);
        offset += sizeof(UInt64);
        FileSize = BinaryPrimitives.ReadUInt64LittleEndian(data.Span[offset..(offset + sizeof(UInt64))]);
        offset += sizeof(UInt64);
        Sparse = BinaryPrimitives.ReadUInt64LittleEndian(data.Span[offset..(offset + sizeof(UInt64))]);
        offset += sizeof(UInt64);
        LinkCount = BinaryPrimitives.ReadUInt32LittleEndian(data.Span[offset..(offset + sizeof(UInt32))]);
        offset += sizeof(UInt32);
        FragmentIndex = BinaryPrimitives.ReadUInt32LittleEndian(data.Span[offset..(offset + sizeof(UInt32))]);
        offset += sizeof(UInt32);
        BlockOffset = BinaryPrimitives.ReadUInt32LittleEndian(data.Span[offset..(offset + sizeof(UInt32))]);
        offset += sizeof(UInt32);
        int blockCount =
            FragmentIndex == 0xFFFFFFFF ?
                (int)Math.Ceiling((double)FileSize / blockSize) :
                (int)Math.Floor((double)FileSize / blockSize);

        XattrIndex = BinaryPrimitives.ReadUInt32LittleEndian(data.Span[offset..(offset + sizeof(UInt32))]);
        offset += sizeof(UInt32);

        BlockSizes = new uint[blockCount];
        for (int i = 0; i < blockCount; i++)
        {
            BlockSizes[i] = BinaryPrimitives.ReadUInt32LittleEndian(data.Span[offset..(offset + sizeof(UInt32))]);
            offset += sizeof(UInt32);
        }

        if (offset != GetMetadataLength())
        {
            throw new ArgumentException("Data length does not match expected metadata length for BasicFileInode", nameof(data));
        }
    }

    public override uint GetTypeIdentifier() => 9;

    protected override int GetSpecialMetadataLength()
    {
        return 40 + (BlockSizes.Length * 4);
    }


    /// <summary>
    /// The offset from the start of the archive to the first data block.
    /// </summary>
    public ulong BlocksStartOffset { get; set; }

    /// <summary>
    /// The (uncompressed) size of this file.
    /// </summary>
    public ulong FileSize { get; set; }
    /// <summary>
    /// The number of bytes saved by omitting zero bytes. Used in the kernel for sparse file accounting.
    /// </summary>
    public ulong Sparse { get; set; }
    /// <summary>
    /// The number of hard links to this node.
    /// </summary>
    public uint LinkCount { get; set; }
    /// <summary>
    /// An index into the Fragment Table which describes the fragment block that the tail end of this file is stored in. If not used, this is set to 0xFFFFFFFF.
    /// </summary>
    public uint FragmentIndex { get; set; }

    /// <summary>
    // The (uncompressed) offset within the fragment block where the tail end of this file is. See Data and Fragment Blocks for details.
    /// </summary>
    public uint BlockOffset { get; set; }
    /// <summary>
    /// An index into the Xattr Table or 0xFFFFFFFF if the inode has no extended attributes.
    /// </summary>
    public uint XattrIndex { get; set; }
    /// <summary>
    /// An array of consecutive block sizes. See Data and Fragment Blocks for details.
    /// </summary>
    public uint[] BlockSizes { get; set; } = [];
}