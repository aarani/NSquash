namespace NSquash.Compressors;

public static class CompressorRegistry
{
    public static IReadOnlyDictionary<ushort, Type> RegisteredCompressors { get; } = new Dictionary<ushort, Type>
    {
        { 1, typeof(GzipCompressor) }
    };
}