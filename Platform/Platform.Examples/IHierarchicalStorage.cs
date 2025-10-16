namespace Platform.Examples
{
    /// <summary>
    /// Universal interface for importing hierarchical data structures into links storage.
    /// Can be used for XML, JSON, File System, and other hierarchical formats.
    /// </summary>
    public interface IHierarchicalStorage<TLink>
    {
        TLink CreateRoot(string name);
        TLink CreateNode(string name, string type = null);
        TLink CreateValueNode(string content);
        void AttachToParent(TLink child, TLink parent);
    }
}
