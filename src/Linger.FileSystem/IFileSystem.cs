namespace Linger.FileSystem;

public interface IFileSystem
{
    bool FileExists(string filePath);

    bool DirectoryExists(string directoryPath);

    void CreateDirectoryIfNotExists(string directoryPath);

    void DeleteFileIfExists(string filePath);
}