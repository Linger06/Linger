using Linger.Extensions.Core;
using Linger.Extensions.IO;
using Linger.FileSystem.Exceptions;
using Linger.Helper;

namespace Linger.FileSystem.Local;

// 添加继承自FileSystemBase
public class LocalFileSystem : FileSystemBase, ILocalFileSystem
{
    private readonly LocalFileSystemOptions _options;

    public string RootDirectoryPath { get; }

    public LocalFileSystem(LocalFileSystemOptions options)
        : base(options.RetryOptions)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        RootDirectoryPath = options.RootDirectoryPath;

        // 确保根目录存在
        Directory.CreateDirectory(RootDirectoryPath);
    }

    public LocalFileSystem(string rootDirectoryPath, RetryOptions? retryOptions = null)
        : this(new LocalFileSystemOptions
        {
            RootDirectoryPath = rootDirectoryPath,
            RetryOptions = retryOptions
        })
    {
    }

    // 这是本地文件系统，所以IsRemoteFileSystem保持为false (默认)

    public bool Exists()
    {
        return DirectoryExists(RootDirectoryPath);
    }

    public void CreateIfNotExists()
    {
        CreateDirectoryIfNotExists(RootDirectoryPath);
    }

    public override void CreateDirectoryIfNotExists(string directoryPath)
    {
        var realPath = GetRealPath(directoryPath);
        Directory.CreateDirectory(realPath);
    }

    public override void DeleteFileIfExists(string filePath)
    {
        var realPath = GetRealPath(filePath);
        FileHelper.DeleteFileIfExists(realPath);
    }

    public void DeleteFileIfExistsAsync(string filePath)
    {
        var realPath = GetRealPath(filePath);
        FileHelper.DeleteFileIfExists(realPath);
    }

    public override bool DirectoryExists(string directoryPath)
    {
        var realPath = GetRealPath(directoryPath);
        return PathHelper.Exists(realPath, false);
    }

    public override bool FileExists(string filePath)
    {
        var realPath = GetRealPath(filePath);
        return PathHelper.Exists(realPath, true);
    }

    public override Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(FileExists(filePath));
    }

    public override Task<bool> DirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(DirectoryExists(directoryPath));
    }

    public override Task CreateDirectoryIfNotExistsAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        CreateDirectoryIfNotExists(directoryPath);
        return Task.CompletedTask;
    }

    public override Task DeleteFileIfExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        DeleteFileIfExists(filePath);
        return Task.CompletedTask;
    }

    public async Task<UploadedInfo> UploadAsync(
        Stream inputStream,
        string sourceFileName,
        string containerName = "",
        string destPath = "",
        NamingRule? namingRule = null,
        bool? overwrite = null,
        bool? useSequencedName = null)
    {
        ArgumentNullException.ThrowIfNull(inputStream);
        ArgumentNullException.ThrowIfNullOrEmpty(sourceFileName);

        // 使用传入的值或默认值
        var effectiveNamingRule = namingRule ?? _options.DefaultNamingRule;
        var effectiveOverwrite = overwrite ?? _options.DefaultOverwrite;
        var effectiveUseSequencedName = useSequencedName ?? _options.DefaultUseSequencedName;


        return await RetryHelper.ExecuteAsync(
            async () => await UploadInternalAsync(
                inputStream,
                sourceFileName,
                containerName,
                destPath,
                effectiveNamingRule,
                effectiveOverwrite,
                effectiveUseSequencedName),
            "文件上传",
            ex => ex is not DuplicateFileException); // 文件重复异常不重试

    }

    private async Task<UploadedInfo> UploadInternalAsync(
        Stream inputStream,
        string sourceFileName,
        string containerName,
        string destPath,
        NamingRule namingRule = NamingRule.Md5, bool overwrite = false, bool useSequencedName = true)
    {
        // 先将输入流的内容复制到内存流中，这样可以多次读取
        using var memoryStream = new MemoryStream();
        await inputStream.CopyToAsync(memoryStream, _options.UploadBufferSize);
        memoryStream.Position = 0;

        // 计算源文件哈希值
        var sourceHashData = memoryStream.ComputeHashMd5();

        // 获取目标文件路径
        var filePath = GetDestFilePath(
            memoryStream,
            sourceFileName,
            containerName,
            destPath,
            namingRule,
                overwrite,
                useSequencedName);

        var relativeFilePath = Path.Combine(RootDirectoryPath, filePath);

        // 确保目录存在
        FileHelper.EnsureDirectoryExists(relativeFilePath);

        // 写入文件
        memoryStream.Position = 0;
#if NET8_0_OR_GREATER
        await
#endif
        using (var fileStream = File.Create(relativeFilePath))
        {
            await memoryStream.CopyToAsync(fileStream);
        }

        // 验证文件完整性
        var fileInfo = new FileInfo(relativeFilePath);

#if NET8_0_OR_GREATER
        await ValidateFileIntegrityAsync(fileInfo, sourceHashData);
#else
        ValidateFileIntegrity(fileInfo, sourceHashData);
#endif


        // 构建上传信息
        return new UploadedInfo
        {
            HashData = sourceHashData,
            FileName = sourceFileName,
            NewFileName = fileInfo.Name,
            FilePath = filePath,
            RelativeFilePath = relativeFilePath,
            FullFilePath = fileInfo.FullName,
            FileSize = memoryStream.Length.FormatFileSize(),
            Length = fileInfo.Length
        };
    }

    private
#if NET8_0_OR_GREATER
       async Task ValidateFileIntegrityAsync
#else
        void ValidateFileIntegrity
#endif
     // 验证文件完整性时使用配置
     (FileInfo fileInfo, string sourceHashData)
    {
        if (!_options.ValidateFileIntegrity)
            return;

        // 验证文件MD5
#if NET8_0_OR_GREATER
        await using (var stream = fileInfo.OpenRead())
#else
        using (var stream = fileInfo.OpenRead())
#endif
        {
            var uploadedFileHash = stream.ComputeHashMd5();
            if (uploadedFileHash != sourceHashData)
            {
                var ex = new InvalidOperationException($"File integrity check failed: MD5 hash mismatch for {fileInfo.FullName}");

                // 根据配置决定是否清理
                if (_options.CleanupOnValidationFailure)
                {
                    try { File.Delete(fileInfo.FullName); } catch { /* 忽略清理失败 */ }
                }

                throw ex;
            }
        }

        // 验证文件元数据
        if (_options.ValidateFileMetadata)
        {
            var customFileInfo = FileHelper.GetCustomFileInfo(fileInfo.FullName);
            if (customFileInfo?.HashData != null && customFileInfo.HashData != sourceHashData)
            {
                var ex = new InvalidOperationException($"File integrity check failed: Metadata hash mismatch for {fileInfo.FullName}");

                if (_options.CleanupOnValidationFailure)
                {
                    try { File.Delete(fileInfo.FullName); } catch { /* 忽略清理失败 */ }
                }

                throw ex;
            }
        }
    }

    private void InitDirectory(string createFilePath)
    {
        var directory = Path.GetDirectoryName(createFilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            CreateDirectoryIfNotExists(directory);
        }
    }

    public Task<UploadedInfo> UploadAsync(string sourceFilePathName, string containerName, string destPath = "", NamingRule namingRule = NamingRule.Md5, bool overwrite = false, bool useSequencedName = true)
    {
        var fileInfo = new FileInfo(sourceFilePathName);
        // 先将文件内容读入内存流中
        using var memoryStream = new MemoryStream();
        using FileStream fileStream = fileInfo.OpenRead();
        fileStream.CopyTo(memoryStream);
        // 重置内存流位置
        memoryStream.Position = 0;
        // 使用内存流进行上传
        return UploadAsync(memoryStream, fileInfo.Name, containerName, destPath, namingRule, overwrite, useSequencedName);
    }

    /// <summary>
    /// 获取目标文件路径
    /// </summary>
    /// <param name="inputStream">输入流</param>
    /// <param name="sourceFileName">源文件名</param>
    /// <param name="containerName">容器名</param>
    /// <param name="destPath">目标路径</param>
    /// <param name="namingRule">文件命名规则（决定如何为上传的文件命名）</param>
    /// <param name="overwrite">是否覆盖已存在的文件</param>
    /// <param name="useSequencedName">是否使用序号命名（当文件名冲突时）</param>
    /// <returns>目标文件相对路径(只返回除destRootPath以外的存储位置)</returns>
    private string GetDestFilePath(
        Stream inputStream,
        string sourceFileName,
        string containerName,
        string destPath,
        NamingRule namingRule = NamingRule.Md5, bool overwrite = false, bool useSequencedName = true)
    {
        ArgumentNullException.ThrowIfNull(inputStream);
        ArgumentNullException.ThrowIfNullOrEmpty(sourceFileName);

        var fileExtension = Path.GetExtension(sourceFileName);

        switch (namingRule)
        {
            case NamingRule.Uuid:
                return GenerateUuidBasedPath(containerName, destPath, fileExtension);
            case NamingRule.Md5:
                return GenerateHashBasedPath(inputStream, sourceFileName, containerName, destPath);
            case NamingRule.Normal:
            default:
                {
                    var basePath = Path.Combine(containerName, destPath);
                    return GetDestFilePath(basePath, sourceFileName, overwrite, useSequencedName, RootDirectoryPath);
                }
        }
    }

    /// <summary>
    /// 生成基于UUID的文件路径
    /// </summary>
    private string GenerateUuidBasedPath(string containerName, string destPath, string fileExtension)
    {
        while (true)
        {
            var fileName = $"{Guid.NewGuid():N}{fileExtension}";
            var fullPath = Path.Combine(RootDirectoryPath, containerName, destPath, fileName);

            if (!File.Exists(fullPath))
            {
                return Path.Combine(containerName, destPath, fileName);
            }
        }
    }

    /// <summary>
    /// 生成基于文件哈希值的文件路径
    /// </summary>
    private static string GenerateHashBasedPath(Stream inputStream, string sourceFileName, string containerName, string destPath)
    {
        var hashData = inputStream.ComputeHashMd5();
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFileName).Replace(" ", string.Empty);
        var fileExtension = Path.GetExtension(sourceFileName);

        // 使用更标准的分隔符
        var fileName = $"{fileNameWithoutExtension}-{hashData}{fileExtension}";
        return Path.Combine(containerName, destPath, fileName);
    }

    /// <summary>
    /// 获取文件路径（支持序号命名）
    /// </summary>
    private static string GetDestFilePath(string destPath, string destFileName, bool overwrite, bool useSequencedName, string destRootPath = "")
    {
        if (overwrite)
        {
            return Path.Combine(destPath, destFileName);
        }

        var fileInfo = new FileInfo(destFileName);
        var baseFileName = Path.GetFileNameWithoutExtension(destFileName);
        var extension = fileInfo.Extension;

        uint sequence = 0;
        while (true)
        {
            var currentFileName = sequence == 0
                ? destFileName
                : $"{baseFileName}[{sequence}]{extension}";

            var fullPath = Path.Combine(destRootPath, destPath, currentFileName);

            if (!File.Exists(fullPath))
            {
                return Path.Combine(destPath, currentFileName);
            }

            if (!useSequencedName)
            {
                throw new DuplicateFileException();
            }

            sequence++;
        }
    }


    /// <summary>
    /// 下载文件到指定的目标文件
    /// </summary>
    /// <param name="filePath">源文件路径</param>
    /// <param name="destFileName">目标文件名</param>
    /// <param name="overwrite">是否覆盖现有文件</param>
    /// <param name="useSequencedName">是否使用序号命名</param>
    /// <returns>下载后的文件路径</returns>
    /// <exception cref="ArgumentNullException">文件路径或目标文件名为空时抛出</exception>
    /// <exception cref="FileNotFoundException">源文件不存在时抛出</exception>
    /// <exception cref="DuplicateFileException">目标文件已存在且不允许覆盖或序号命名时抛出</exception>
    public async Task<string> DownloadAsync(
        string filePath,
        string destFileName,
        bool overwrite = false,
        bool useSequencedName = true)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(filePath);
        ArgumentNullException.ThrowIfNullOrEmpty(destFileName);

        return await RetryHelper.ExecuteAsync(
            async () =>
            {
                var sourceFilePath = GetRealPath(filePath);
                if (!File.Exists(sourceFilePath))
                {
                    throw new FileNotFoundException("源文件不存在", sourceFilePath);
                }

                var destFilePath = await GetUniqueDestFilePathAsync(
                    destFileName,
                    overwrite,
                    useSequencedName);

#if NET8_0_OR_GREATER
                await
#endif
                using var sourceStream = File.OpenRead(sourceFilePath);
#if NET8_0_OR_GREATER
                await
#endif
                using var destStream = File.Create(destFilePath);

                await sourceStream.CopyToAsync(destStream, _options.DownloadBufferSize);
                return destFilePath;
            },
            "文件下载",
            ex => !(ex is FileNotFoundException || ex is DuplicateFileException));

    }

    /// <summary>
    /// 下载文件到指定的流
    /// </summary>
    /// <param name="filePath">源文件路径</param>
    /// <param name="destStream">目标流</param>
    /// <returns>完成任务</returns>
    /// <exception cref="ArgumentNullException">文件路径为空时抛出</exception>
    /// <exception cref="ArgumentNullException">目标流为空时抛出</exception>
    /// <exception cref="FileNotFoundException">源文件不存在时抛出</exception>
    public async Task DownloadToStreamAsync(string filePath, Stream destStream)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(filePath);
        ArgumentNullException.ThrowIfNull(destStream);

        var sourceFilePath = GetRealPath(filePath);
        if (!File.Exists(sourceFilePath))
        {
            throw new FileNotFoundException("Source file not found", sourceFilePath);
        }

#if NET8_0_OR_GREATER
        await
#endif
        using var sourceStream = File.OpenRead(sourceFilePath);

        sourceStream.Position = 0;
        await sourceStream.CopyToAsync(destStream, _options.DownloadBufferSize);
    }

    /// <summary>
    /// 获取唯一的目标文件路径
    /// </summary>
    private async Task<string> GetUniqueDestFilePathAsync(
        string destFileName,
        bool overwrite,
        bool useSequencedName)
    {
        var destFilePath = GetDestFilePath(string.Empty, destFileName, overwrite, useSequencedName);

        // 确保目录存在
        var directory = Path.GetDirectoryName(destFilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            await Task.Run(() => CreateDirectoryIfNotExists(directory));
        }

        return destFilePath;
    }

    /// <summary>
    /// 得到 带有 RootDirectoryPath 的路径
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public string GetRealPath(string filePath)
    {
        if (filePath.StartsWith(RootDirectoryPath))
        {
            return filePath;
        }
        filePath = Path.Combine(RootDirectoryPath, filePath);
        return filePath;
        //return PathHelper.ProcessPath(RootDirectoryPath, filePath);
    }

    public Task DeleteAsync(string filePath)
    {
        var realPath = GetRealPath(filePath);
        DeleteFileIfExistsAsync(realPath);
        return Task.CompletedTask;
    }

    public override async Task<FileOperationResult> UploadAsync(Stream inputStream, string destinationPath, string fileName, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var uploadedInfo = await UploadAsync(inputStream, fileName, string.Empty, destinationPath, 
                _options.DefaultNamingRule, overwrite, !overwrite);
            
            return FileOperationResult.CreateSuccess(
                uploadedInfo.FilePath,
                uploadedInfo.FullFilePath,
                uploadedInfo.Length,
                uploadedInfo.HashData);
        }
        catch (Exception ex)
        {
            return FileOperationResult.CreateFailure($"上传文件失败: {ex.Message}", ex);
        }
    }

    public override async Task<FileOperationResult> UploadFileAsync(string localFilePath, string destinationPath, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(localFilePath))
            {
                return FileOperationResult.CreateFailure($"本地文件不存在: {localFilePath}");
            }

            using var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read);
            var fileName = Path.GetFileName(localFilePath);
            return await UploadAsync(fileStream, destinationPath, fileName, overwrite, cancellationToken);
        }
        catch (Exception ex)
        {
            return FileOperationResult.CreateFailure($"上传文件失败: {ex.Message}", ex);
        }
    }

    public override async Task<FileOperationResult> DownloadToStreamAsync(string filePath, Stream outputStream, CancellationToken cancellationToken = default)
    {
        try
        {
            var realPath = GetRealPath(filePath);
            if (!File.Exists(realPath))
            {
                return FileOperationResult.CreateFailure($"文件不存在: {filePath}");
            }

            using var fileStream = File.OpenRead(realPath);
            await fileStream.CopyToAsync(outputStream, _options.DownloadBufferSize, cancellationToken);
            
            var fileInfo = new FileInfo(realPath);
            return FileOperationResult.CreateSuccess(filePath, realPath, fileInfo.Length);
        }
        catch (Exception ex)
        {
            return FileOperationResult.CreateFailure($"下载文件到流失败: {ex.Message}", ex);
        }
    }

    public override async Task<FileOperationResult> DownloadFileAsync(string filePath, string localDestinationPath, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var realPath = GetRealPath(filePath);
            if (!File.Exists(realPath))
            {
                return FileOperationResult.CreateFailure($"文件不存在: {filePath}");
            }

            // 确保目标目录存在
            var destDir = Path.GetDirectoryName(localDestinationPath);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            // 检查文件是否已存在
            if (File.Exists(localDestinationPath) && !overwrite)
            {
                return FileOperationResult.CreateFailure($"目标文件已存在: {localDestinationPath}");
            }

            using var sourceStream = File.OpenRead(realPath);
            using var destStream = new FileStream(localDestinationPath, overwrite ? FileMode.Create : FileMode.CreateNew);
            await sourceStream.CopyToAsync(destStream, _options.DownloadBufferSize, cancellationToken);
            
            var fileInfo = new FileInfo(localDestinationPath);
            return FileOperationResult.CreateSuccess(filePath, localDestinationPath, fileInfo.Length);
        }
        catch (Exception ex)
        {
            return FileOperationResult.CreateFailure($"下载文件失败: {ex.Message}", ex);
        }
    }

    public override async Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var realPath = GetRealPath(filePath);
            if (!File.Exists(realPath))
            {
                return FileOperationResult.CreateSuccess(filePath); // 文件不存在也视为成功
            }

            await Task.Run(() => File.Delete(realPath), cancellationToken);
            return FileOperationResult.CreateSuccess(filePath);
        }
        catch (Exception ex)
        {
            return FileOperationResult.CreateFailure($"删除文件失败: {ex.Message}", ex);
        }
    }
}

public class UploadedInfo : CustomFileInfo
{
    /// <summary>
    /// 除 destRootPath 以外的存储位置
    /// </summary>
    public string FilePath { get; set; } = null!;
}