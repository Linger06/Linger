namespace Linger.FileSystem.Local;

public interface ILocalFileSystem : IFileSystemOperations
{
    string RootDirectoryPath { get; }
    bool Exists();
    void CreateIfNotExists();

    // 保留特定的上传方法（多种参数的版本）
    Task<UploadedInfo> UploadAsync(Stream inputStream, string sourceFileName, string containerName = "", string destPath = "", NamingRule? namingRule = null,
        bool? overwrite = null,
        bool? useSequencedName = null);
}