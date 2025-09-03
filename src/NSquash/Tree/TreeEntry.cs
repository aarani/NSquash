using NSquash.Inodes;

namespace NSquash.Tree;

/// <summary>
/// Base class for an in-memory logical tree representation of SquashFS entries.
/// Wraps an <see cref="Inode"/> plus hierarchical navigation helpers.
/// </summary>
internal abstract class TreeEntry
{
    protected TreeEntry(string name, Inode inode)
    {
        Name = name;
        Inode = inode;
    }

    /// <summary>
    /// The basename of this entry (no directory separators). Root has an empty name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Parent directory entry (null for root).
    /// </summary>
    public DirectoryEntry? Parent { get; internal set; }

    /// <summary>
    /// The underlying parsed inode.
    /// </summary>
    public Inode Inode { get; }

    /// <summary>
    /// Convenience accessor for the on-disk inode number.
    /// </summary>
    public uint InodeNumber
        => Inode.InodeNumber;

    /// <summary>
    /// Full path assembled from parents. Root resolves to "/".
    /// </summary>
    public string Path => Parent == null
        ? "/"
        : (Parent.Parent == null ? $"/{Name}" : $"{Parent.Path.TrimEnd('/')}/{Name}");

    /// <summary>
    /// True if this entry represents a directory.
    /// </summary>
    public abstract bool IsDirectory { get; }
}
