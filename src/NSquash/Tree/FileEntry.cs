using NSquash.Inodes;

namespace NSquash.Tree;

/// <summary>
/// Represents a file entry (regular file) in the logical tree.
/// </summary>
internal sealed class FileEntry : TreeEntry
{
    public FileEntry(string name, Inode inode) : base(name, inode) { }

    public override bool IsDirectory => false;
}
