using System.Buffers.Binary;

namespace NSquash.Inodes;

internal abstract class Inode
{
    public abstract uint GetTypeIdentifier();

    protected const int CommonMetadataLength = 16;

    public int GetMetadataLength() =>
        GetSpecialMetadataLength() + CommonMetadataLength; // Size of common metadata fields
    protected abstract int GetSpecialMetadataLength();

    public ushort Permissions { get; set; }
    public ushort Uid { get; set; }
    public ushort Gid { get; set; }
    public uint ModificationTime { get; set; }
    public uint InodeNumber { get; set; }

    public Inode(uint blockSize, ReadOnlyMemory<byte> data)
    {
        if (data.Length < 16)
        {
            throw new ArgumentException("Data length is insufficient even for common header", nameof(data));
        }

        var type = BinaryPrimitives.ReadUInt16LittleEndian(data.Span[0..2]);
        if (type != GetTypeIdentifier())
        {
            throw new ArgumentException($"Inode type identifier mismatch. Expected {GetTypeIdentifier()}, got {type}", nameof(data));
        }

        Permissions = BitConverter.ToUInt16(data.Span[2..4]);
        Uid = BitConverter.ToUInt16(data.Span[4..6]);
        Gid = BitConverter.ToUInt16(data.Span[6..8]);
        ModificationTime = BitConverter.ToUInt32(data.Span[8..12]);
        InodeNumber = BitConverter.ToUInt32(data.Span[12..16]);
    }
}