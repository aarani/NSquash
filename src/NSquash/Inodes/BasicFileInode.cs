
using System.Buffers.Binary;

namespace NSquash.Inodes;

internal class BasicFileInode : Inode
{
    public BasicFileInode(uint blockSize, ReadOnlyMemory<byte> data) : base(blockSize, data)
    {
        int offset = CommonMetadataLength;
        BlocksStartOffset = BinaryPrimitives.ReadUInt32LittleEndian(data.Span[offset..(offset + sizeof(UInt32))]);
        offset += sizeof(UInt32);
        FragmentIndex = BinaryPrimitives.ReadUInt32LittleEndian(data.Span[offset..(offset + sizeof(UInt32))]);
        offset += sizeof(UInt32);
        BlockOffset = BinaryPrimitives.ReadUInt32LittleEndian(data.Span[offset..(offset + sizeof(UInt32))]);
        offset += sizeof(UInt32);
        FileSize = BinaryPrimitives.ReadUInt32LittleEndian(data.Span[offset..(offset + sizeof(UInt32))]);
        offset += sizeof(UInt32);
        int blockCount =
            FragmentIndex == 0xFFFFFFFF ?
                (int)Math.Ceiling((double)FileSize / blockSize) :
                (int)Math.Floor((double)FileSize / blockSize);

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

    public override uint GetTypeIdentifier() => 2;

    protected override int GetSpecialMetadataLength()
    {
        return 16 + (BlockSizes.Length * 4);
    }


    /// <summary>
    /// The offset from the start of the archive to the first data block.
    /// </summary>
    public uint BlocksStartOffset { get; set; }

    /// <summary>
    /// An index into the Fragment Table which describes the fragment block that the tail end of this file is stored in. If not used, this is set to 0xFFFFFFFF.
    /// </summary>
    public uint FragmentIndex { get; set; }

    /// <summary>
    // The (uncompressed) offset within the fragment block where the tail end of this file is. See Data and Fragment Blocks for details.
    /// </summary>
    public uint BlockOffset { get; set; }

    /// <summary>
    /// The (uncompressed) size of this file.
    /// </summary>
    public uint FileSize { get; set; }

    /// <summary>
    /// An array of consecutive block sizes. See Data and Fragment Blocks for details.
    /// </summary>
    public uint[] BlockSizes { get; set; } = [];
}