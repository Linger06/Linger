namespace Linger.FileSystem;

/// <summary>
/// 文件系统基础同步接口
/// </summary>
/// <remarks>
/// ⚠️ 已过时：此接口中的同步方法可能导致死锁和性能问题。
/// 请使用 <see cref="IAsyncFileSystem"/> 或 <see cref="IFileSystemOperations"/> 接口。
/// </remarks>
[Obsolete("IFileSystem 的同步方法可能导致死锁。请使用 IAsyncFileSystem 或 IFileSystemOperations 接口", false)]
public interface IFileSystem
{
    bool FileExists(string filePath);

    bool DirectoryExists(string directoryPath);

    void CreateDirectoryIfNotExists(string directoryPath);

    void DeleteFileIfExists(string filePath);
}