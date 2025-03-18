using System.Threading;
using System.Threading.Tasks;

namespace Linger.FileSystem;

/// <summary>
/// 异步文件系统接口，定义基本的异步文件操作
/// </summary>
public interface IAsyncFileSystem
{
    Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);
    
    Task<bool> DirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default);
    
    Task CreateDirectoryIfNotExistsAsync(string directoryPath, CancellationToken cancellationToken = default);
    
    Task DeleteFileIfExistsAsync(string filePath, CancellationToken cancellationToken = default);
}
