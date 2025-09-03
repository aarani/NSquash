
using System.Buffers.Binary;

namespace NSquash.Inodes;

internal class ExtendedNamedPipeInode : Inode
{
    public ExtendedNamedPipeInode(uint blockSize, ReadOnlyMemory<byte> data) : base(blockSize, data)
    {
        int offset = CommonMetadataLength;
        LinkCount = BinaryPrimitives.ReadUInt32LittleEndian(data.Span[offset..(offset + sizeof(UInt32))]);
        offset += sizeof(UInt32);
        XattrIndex = BinaryPrimitives.ReadUInt32LittleEndian(data.Span[offset..(offset + sizeof(UInt32))]);
        offset += sizeof(UInt32);

        if (offset != GetMetadataLength())
        {
            throw new ArgumentException("Data length does not match expected metadata length for DeviceSpecialInode", nameof(data));
        }
    }

    public override uint GetTypeIdentifier() => 13;
    protected override int GetSpecialMetadataLength() => 8;

    /// <summary>
    /// The number of hard links to this entry.
    /// </summary>
    public uint LinkCount { get; set; }

    /// <summary>
    /// An index into the Xattr Table or 0xFFFFFFFF if the inode has no extended attributes.
    /// </summary>
    public uint XattrIndex { get; set; }
}