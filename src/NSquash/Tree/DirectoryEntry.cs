using NSquash.Inodes;

namespace NSquash.Tree;

/// <summary>
/// Represents a directory in the logical tree. Holds child entries.
/// </summary>
internal sealed class DirectoryEntry : TreeEntry
{
    private readonly Dictionary<string, TreeEntry> _children = new(StringComparer.Ordinal);

    public DirectoryEntry(string name, Inode inode) : base(name, inode) { }

    public override bool IsDirectory => true;

    /// <summary>
    /// Enumerates child entries. Order not guaranteed (insertion order of dictionary implementation).
    /// </summary>
    public IEnumerable<TreeEntry> Children => _children.Values;

    /// <summary>
    /// Attempts to get a child by name.
    /// </summary>
    public bool TryGetChild(string name, out TreeEntry entry) => _children.TryGetValue(name, out entry!);

    /// <summary>
    /// Adds a child to this directory. Throws if a child with the same name already exists.
    /// </summary>
    public void AddChild(TreeEntry child)
    {
        if (child.Parent != null)
        {
            throw new InvalidOperationException("TreeEntry already has a parent");
        }
        if (string.IsNullOrEmpty(child.Name))
        {
            throw new ArgumentException("Child entry name must be non-empty", nameof(child));
        }
        if (_children.ContainsKey(child.Name))
        {
            throw new InvalidOperationException($"An entry named '{child.Name}' already exists in directory '{Path}'");
        }
        _children.Add(child.Name, child);
        child.Parent = this;
    }

    /// <summary>
    /// Removes a child with the specified name. Returns true if removed.
    /// </summary>
    public bool RemoveChild(string name)
    {
        if (_children.TryGetValue(name, out var existing))
        {
            existing.Parent = null;
            return _children.Remove(name);
        }
        return false;
    }
}
