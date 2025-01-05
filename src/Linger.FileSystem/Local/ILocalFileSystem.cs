namespace Linger.FileSystem.Local;

public interface ILocalFileSystem : IFileSystem
{
    string RootDirectoryPath { get; }
    bool Exists();
    void CreateIfNotExists();
    Task<UploadedInfo> UploadAsync(Stream inputStream, string sourceFileName, string containerName, string destPath = "", bool useUUIDName = true, bool overwrite = false, bool useSequencedName = true, bool useHashMd5Name = true);
    void DeleteAsync(string filePath);
}