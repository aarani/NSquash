namespace NSquash.Inodes;

internal class InodeRegistry
{
    private static readonly Dictionary<uint, Func<uint, ReadOnlyMemory<byte>, Inode>> InodeFactories = new()
    {
        { 1, (blockSize, data) => new BasicDirectoryInode(blockSize, data) },
        { 2, (blockSize, data) => new BasicFileInode(blockSize, data) },
        { 3, (blockSize, data) => new BasicSymlinkInode(blockSize, data) },
        { 4, (blockSize, data) => new BasicBlockDeviceInode(blockSize, data) },
        { 5, (blockSize, data) => new BasicCharacterDeviceInode(blockSize, data) },
        { 6, (blockSize, data) => new BasicNamedPipeInode(blockSize, data) },
        { 7, (blockSize, data) => new BasicSocketInode(blockSize, data) },
        { 8, (blockSize, data) => new ExtendedDirectoryInode(blockSize, data) },
        { 9, (blockSize, data) => new ExtendedFileInode(blockSize, data) },
        { 10, (blockSize, data) => new ExtendedSymlinkInode(blockSize, data) },
        { 11, (blockSize, data) => new ExtendedBlockDeviceInode(blockSize, data) },
        { 12, (blockSize, data) => new ExtendedCharacterDeviceInode(blockSize, data) },
        { 13, (blockSize, data) => new ExtendedNamedPipeInode(blockSize, data) },
        { 14, (blockSize, data) => new ExtendedSocketInode(blockSize, data) },
    };

    internal static Inode CreateInode(uint blockSize, ushort typeIdentifier, ReadOnlyMemory<byte> data)
    {
        if (InodeFactories.TryGetValue(typeIdentifier, out var factory))
        {
            return factory(blockSize, data);
        }

        throw new NotSupportedException($"Unsupported inode type identifier: {typeIdentifier}");
    }
}