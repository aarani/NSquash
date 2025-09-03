namespace NSquash.Compressors;

public interface ICompressor
{
    internal ushort GetId();
    internal int GetMetadataLength();
    internal void WriteMetadata(Span<byte> buffer);
    internal Task<byte[]> Decompress(ReadOnlyMemory<byte> data);
}