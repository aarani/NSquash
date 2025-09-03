using System.Buffers.Binary;

namespace NSquash.Inodes;

internal class ExtendedSymlinkInode : Inode
{
    public ExtendedSymlinkInode(uint blockSize, ReadOnlyMemory<byte> data)
        : base(blockSize, data)
    {
        int offset = CommonMetadataLength;
        LinkCount = BinaryPrimitives.ReadUInt32LittleEndian(data.Span[offset..(offset + sizeof(UInt32))]);
        offset += sizeof(UInt32);
        TargetSize = BinaryPrimitives.ReadUInt32LittleEndian(data.Span[offset..(offset + sizeof(UInt32))]);
        offset += sizeof(UInt32);
        TargetPath = data.Slice(offset, (int)TargetSize).ToArray();
        offset += (int)TargetSize;
        XattrIndex = BinaryPrimitives.ReadUInt32LittleEndian(data.Span[offset..(offset + sizeof(UInt32))]);
        offset += sizeof(UInt32);

        if (offset != GetMetadataLength())
        {
            throw new ArgumentException("Data length does not match expected metadata length for BasicSymlinkInode", nameof(data));
        }
    }


    public override uint GetTypeIdentifier() => 10;

    protected override int GetSpecialMetadataLength() => 12 + (int)TargetSize;

    /// <summary>
    /// The number of hard links to this symlink.
    /// </summary>
    public uint LinkCount { get; set; }

    /// <summary>
    /// The size in bytes of the target path this symlink points to.
    /// </summary>
    public uint TargetSize { get; set; }

    /// <summary>
    /// An array of bytes holding the target path this symlink points to. The path is 'target size' bytes long and NOT null-terminated.
    /// </summary>
    public byte[] TargetPath { get; set; }

    /// <summary>
    /// An index into the Xattr Table or 0xFFFFFFFF if the inode has no extended attributes.
    /// </summary>
    public uint XattrIndex { get; set; }
}