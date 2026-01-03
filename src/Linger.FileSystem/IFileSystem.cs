namespace Linger.FileSystem;

/// <summary>
/// 文件系统接口，定义基本的文件操作
/// </summary>
public interface IFileSystem
{
    Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);

    Task<bool> DirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default);

    Task CreateDirectoryIfNotExistsAsync(string directoryPath, CancellationToken cancellationToken = default);

    Task DeleteFileIfExistsAsync(string filePath, CancellationToken cancellationToken = default);
}
