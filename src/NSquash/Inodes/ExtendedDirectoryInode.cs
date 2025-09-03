
using System.Buffers.Binary;
using System.Diagnostics;

namespace NSquash.Inodes;

internal class ExtendedDirectoryInode : Inode
{
    public ExtendedDirectoryInode(uint blockSize, ReadOnlyMemory<byte> data) : base(blockSize, data)
    {
        int offset = CommonMetadataLength;
        LinkCount = BinaryPrimitives.ReadUInt32LittleEndian(data.Span[offset..(offset + sizeof(UInt32))]);
        offset += sizeof(UInt32);
        FileSize = BinaryPrimitives.ReadUInt16LittleEndian(data.Span[offset..(offset + sizeof(UInt16))]);
        offset += sizeof(UInt16);
        BlockIndex = BinaryPrimitives.ReadUInt32LittleEndian(data.Span[offset..(offset + sizeof(UInt32))]);
        offset += sizeof(UInt32);
        ParentInode = BinaryPrimitives.ReadUInt32LittleEndian(data.Span[offset..(offset + sizeof(UInt32))]);
        offset += sizeof(UInt32);
        IndexCount = BinaryPrimitives.ReadUInt16LittleEndian(data.Span[offset..(offset + sizeof(UInt16))]);
        offset += sizeof(UInt16);
        BlockOffset = BinaryPrimitives.ReadUInt16LittleEndian(data.Span[offset..(offset + sizeof(UInt16))]);
        offset += sizeof(UInt16);
        XattrIndex = BinaryPrimitives.ReadUInt32LittleEndian(data.Span[offset..(offset + sizeof(UInt32))]);
        offset += sizeof(UInt32);
        
        Debug.Assert(offset == GetMetadataLength());
    }

    public override uint GetTypeIdentifier() => 8;

    protected override int GetSpecialMetadataLength() => 24;

    /// <summary>
    /// The number of hard links to this directory.
    /// For historical reasons, the hard link count of a directory includes the number of entries in the directory and is initialized to 2 for an empty directory. I.e. a directory with N entries has at least N + 2 link count.
    /// </summary>
    public uint LinkCount { get; set; }
    /// <summary>
    /// Total (uncompressed) size in bytes of the entry listing in the directory table, including headers.
    /// This value is 3 bytes larger than the real listing. The Linux kernel creates "." and ".." entries for offsets 0 and 1, and only after 3 looks into the listing, subtracting 3 from the size.
    /// If the "file size" is set to a value < 4, the directory is empty and there is no corresponding listing in the directory table.
    /// </summary>
    public ushort FileSize { get; set; }
    /// <summary>
    /// The location of the metadata block in the directory table where the entry information starts. This is relative to the directory table location.
    /// </summary>
    public uint BlockIndex { get; set; }
    /// <summary>
    /// The inode number of the parent of this directory. If this is the root directory, this SHOULD be 0.
    /// </summary>
    public uint ParentInode { get; set; }
    /// <summary>
    /// The number of directory index entries following the inode structure.
    /// </summary>
    public ushort IndexCount { get; set; }
    /// <summary>
    /// The (uncompressed) offset within the metadata block in the directory table where the directory listing starts.
    /// </summary>
    public ushort BlockOffset { get; set; }
    /// <summary>
    /// An index into the Xattr Table or 0xFFFFFFFF if the inode has no extended attributes.
    /// </summary>
    public uint XattrIndex { get; set; }
}