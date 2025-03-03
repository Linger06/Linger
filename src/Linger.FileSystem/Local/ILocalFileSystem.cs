namespace Linger.FileSystem.Local;

public interface ILocalFileSystem : IFileSystem
{
    string RootDirectoryPath { get; }
    bool Exists();
    void CreateIfNotExists();
    Task<UploadedInfo> UploadAsync(Stream inputStream, string sourceFileName, string containerName = "", string destPath = "", NamingRule? namingRule = null,
        bool? overwrite = null,
        bool? useSequencedName = null);
    Task DeleteAsync(string filePath);
}