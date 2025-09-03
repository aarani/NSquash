
using System.Buffers.Binary;

namespace NSquash.Inodes;

internal class BasicBlockDeviceInode : Inode
{
    public BasicBlockDeviceInode(uint blockSize, ReadOnlyMemory<byte> data) : base(blockSize, data)
    {
        int offset = CommonMetadataLength;
        LinkCount = BinaryPrimitives.ReadUInt32LittleEndian(data.Span[offset..(offset + sizeof(UInt32))]);
        offset += sizeof(UInt32);
        DeviceNumber = BinaryPrimitives.ReadUInt32LittleEndian(data.Span[offset..(offset + sizeof(UInt32))]);
        offset += sizeof(UInt32);

        if (offset != GetMetadataLength())
        {
            throw new ArgumentException("Data length does not match expected metadata length for DeviceSpecialInode", nameof(data));
        }
    }

    public override uint GetTypeIdentifier() => 4;
    protected override int GetSpecialMetadataLength() => 8;

    /// <summary>
    /// The number of hard links to this entry.
    /// </summary>
    public uint LinkCount { get; set; }
        
    /// <summary>
    /// The system specific device number.
    /// On Linux, this consists of major and minor device numbers that can be extracted as follows:
    /// major = (dev & 0xFFF00) >> 8.
    /// minor = (dev & 0x000FF)
    /// </summary>
    public uint DeviceNumber { get; set; }
}