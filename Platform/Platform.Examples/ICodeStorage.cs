namespace Platform.Examples
{
    public interface ICodeStorage<TLink>
    {
        TLink CreateRepository(string name);
        TLink CreateFile(string path, string content);
        TLink CreateDirectory(string path);
        TLink CreateCommit(string message, string author, TLink parent);
        void AttachFileToDirectory(TLink file, TLink directory);
        void AttachCommitToRepository(TLink commit, TLink repository);
        void AttachFileToCommit(TLink file, TLink commit);
    }
}
