using Linger.Extensions.Core;
using Linger.Extensions.IO;
using Linger.FileSystem.Exceptions;
using Linger.Helper;

namespace Linger.FileSystem.Local;

public class LocalFileSystem(string rootDirectoryPath, RetryOptions? retryOptions = null) : ILocalFileSystem
{
    private readonly RetryOptions _retryOptions = retryOptions ?? new RetryOptions();
    private readonly Random _random = new();

    public string RootDirectoryPath { get; } = rootDirectoryPath;
    public bool Exists()
    {
        return DirectoryExists(RootDirectoryPath);
    }

    public void CreateIfNotExists()
    {
        CreateDirectoryIfNotExists(RootDirectoryPath);
    }

    public void CreateDirectoryIfNotExists(string directoryPath)
    {
        var realPath = GetRealPath(directoryPath);
        FileHelper.CreateDirectoryIfNotExists(realPath);
    }

    public void DeleteFileIfExists(string filePath)
    {
        var realPath = GetRealPath(filePath);
        FileHelper.DeleteFileIfExists(realPath);
    }

    public bool DirectoryExists(string directoryPath)
    {
        var realPath = GetRealPath(directoryPath);
        return FileHelper.IsExistDirectory(realPath);
    }

    public bool FileExists(string filePath)
    {
        var realPath = GetRealPath(filePath);
        return FileHelper.IsExistFile(realPath);
    }

    public async Task<UploadedInfo> UploadAsync(
        Stream inputStream,
        string sourceFileName,
        string containerName,
        string destPath = "",
        bool useUuidName = true,
        bool overwrite = false,
        bool useSequencedName = true,
        bool useHashMd5Name = true)
    {
        ArgumentNullException.ThrowIfNull(inputStream);
        ArgumentNullException.ThrowIfNullOrEmpty(sourceFileName);
        ArgumentNullException.ThrowIfNullOrEmpty(containerName);

        Exception? lastException = null;
        for (int retry = 0; retry < _retryOptions.MaxRetries; retry++)
        {
            try
            {
                return await UploadInternalAsync(
                    inputStream,
                    sourceFileName,
                    containerName,
                    destPath,
                    useUuidName,
                    overwrite,
                    useSequencedName,
                    useHashMd5Name);
            }
            catch (DuplicateFileException)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (retry == _retryOptions.MaxRetries - 1)
                {
                    throw new OutOfReTryCountException("已达到最大重试次数", lastException);
                }
                // 计算延迟时间
                var delayMs = CalculateDelayWithJitter(retry);
                // 等待后继续重试
                await Task.Delay(delayMs);
            }
        }
        throw new OutOfReTryCountException("已达到最大重试次数", lastException);
    }

    private int CalculateDelayWithJitter(int retryAttempt)
    {
        // 计算基础延迟
        var delay = _retryOptions.UseExponentialBackoff
            ? _retryOptions.BaseDelayMs * Math.Pow(2, retryAttempt)
            : _retryOptions.BaseDelayMs;

        // 限制最大延迟时间
        delay = Math.Min(delay, _retryOptions.MaxDelayMs);

        // 添加随机抖动
        var jitterRange = delay * _retryOptions.JitterFactor;
        var jitter = _random.NextDouble() * jitterRange;
        delay += jitter;

        return (int)delay;
    }

    private async Task<UploadedInfo> UploadInternalAsync(
        Stream inputStream,
        string sourceFileName,
        string containerName,
        string destPath,
        bool useUuidName,
        bool overwrite,
        bool useSequencedName,
        bool useHashMd5Name)
    {
        // 先将输入流的内容复制到内存流中，这样可以多次读取
        using var memoryStream = new MemoryStream();
        await inputStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        // 计算源文件哈希值
        var sourceHashData = memoryStream.ComputeHashMd5();

        // 获取目标文件路径
        var filePath = GetDestFilePath(
            memoryStream,
            sourceFileName,
            containerName,
            destPath,
            useUuidName,
            overwrite,
            useSequencedName,
            useHashMd5Name);

        var relativeFilePath = Path.Combine(RootDirectoryPath, filePath);

        // 确保目录存在
        InitDirectory(relativeFilePath);

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

        // 验证文件MD5
#if NET8_0_OR_GREATER
        await
#endif
            using (var stream = fileInfo.OpenRead())
        {
            var uploadedFileHash = stream.ToMd5Hash();
            if (uploadedFileHash != sourceHashData)
            {
                throw new InvalidOperationException("File integrity check failed: MD5 hash mismatch");
            }
        }

        // 验证文件元数据
        var customFileInfo = FileHelper.GetCustomFileInfo(fileInfo.FullName);
        if (customFileInfo?.HashData != null && customFileInfo.HashData != sourceHashData)
        {
            throw new InvalidOperationException("File integrity check failed: Metadata hash mismatch");
        }

        // 构建上传信息
        return new UploadedInfo
        {
            HashData = sourceHashData,
            FileName = sourceFileName,
            NewFileName = fileInfo.Name,
            FilePath = filePath,
            RelativeFilePath = relativeFilePath,
            FullFilePath = fileInfo.FullName,
            FileSize = memoryStream.Length.FileSize(),
            Length = fileInfo.Length
        };
    }


    private void InitDirectory(string createFilePath)
    {
        var diDesc = new DirectoryInfo(new FileInfo(createFilePath).DirectoryName!);
        CreateDirectoryIfNotExists(diDesc.FullName);
    }

    public Task<UploadedInfo> UploadAsync(string sourceFilePathName, string containerName, string destPath = "", bool useUuidName = true, bool overwrite = false, bool useSequencedName = true)
    {
        var fileInfo = new FileInfo(sourceFilePathName);
        // 先将文件内容读入内存流中
        using var memoryStream = new MemoryStream();
        using FileStream fileStream = fileInfo.OpenRead();
        fileStream.CopyTo(memoryStream);
        // 重置内存流位置
        memoryStream.Position = 0;
        // 使用内存流进行上传
        return UploadAsync(memoryStream, fileInfo.Name, containerName, destPath, useUuidName, overwrite,
            useSequencedName);
    }

    /// <summary>
    /// 获取目标文件路径
    /// </summary>
    /// <param name="inputStream">输入流</param>
    /// <param name="sourceFileName">源文件名</param>
    /// <param name="containerName">容器名</param>
    /// <param name="destPath">目标路径</param>
    /// <param name="useUuidName">是否使用UUID作为文件名</param>
    /// <param name="overwrite">是否覆盖已存在的文件</param>
    /// <param name="useSequencedName">是否使用序号命名</param>
    /// <param name="useHashMd5Name">是否使用MD5哈希作为文件名</param>
    /// <returns>目标文件相对路径(只返回除destRootPath以外的存储位置)</returns>
    private string GetDestFilePath(
        Stream inputStream,
        string sourceFileName,
        string containerName,
        string destPath,
        bool useUuidName,
        bool overwrite,
        bool useSequencedName,
        bool useHashMd5Name = true)
    {
        ArgumentNullException.ThrowIfNull(inputStream);
        ArgumentNullException.ThrowIfNullOrEmpty(sourceFileName);
        ArgumentNullException.ThrowIfNullOrEmpty(containerName);

        var fileExtension = Path.GetExtension(sourceFileName);

        // 1. UUID命名方式
        if (useUuidName)
        {
            return GenerateUuidBasedPath(containerName, destPath, fileExtension);
        }

        // 2. MD5哈希命名方式
        if (useHashMd5Name)
        {
            return GenerateHashBasedPath(inputStream, sourceFileName, containerName, destPath);
        }

        // 3. 常规命名方式
        var basePath = Path.Combine(containerName, destPath);
        return GetDestFilePath(basePath, sourceFileName, overwrite, useSequencedName, RootDirectoryPath);
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
    private string GenerateHashBasedPath(Stream inputStream, string sourceFileName, string containerName, string destPath)
    {
        var hashData = inputStream.ComputeHashMd5();
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFileName)
                                         .Replace(" ", string.Empty);
        var fileExtension = Path.GetExtension(sourceFileName);

        var fileName = $"{fileNameWithoutExtension}^_^{hashData}{fileExtension}";
        return Path.Combine(containerName, destPath, fileName);
    }

    /// <summary>
    /// 获取文件路径（支持序号命名）
    /// </summary>
    private string GetDestFilePath(string destPath, string destFileName, bool overwrite, bool useSequencedName, string destRootPath = "")
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

        var sourceFilePath = GetRealPath(filePath);
        if (!File.Exists(sourceFilePath))
        {
            throw new FileNotFoundException("Source file not found", sourceFilePath);
        }

        var destFilePath = await GetUniqueDestFilePathAsync(
            destFileName,
            overwrite,
            useSequencedName);

        await CopyFileAsync(sourceFilePath, destFilePath);

        return destFilePath;
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
        await sourceStream.CopyToAsync(destStream);
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
    /// 复制文件到目标位置
    /// </summary>
    private static async Task CopyFileAsync(string sourcePath, string destPath)
    {
#if NET8_0_OR_GREATER
        await
#endif
            using var sourceStream = File.OpenRead(sourcePath);
#if NET8_0_OR_GREATER
        await
#endif
            using var destStream = File.Create(destPath);

        await sourceStream.CopyToAsync(destStream);
    }


    /// <summary>
    /// 得到 带有 RootDirectoryPath 的路径
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private string GetRealPath(string filePath)
    {
        if (filePath.StartsWith(RootDirectoryPath))
        {
            return filePath;
        }
        filePath = Path.Combine(RootDirectoryPath, filePath);
        return filePath;
    }

    public void DeleteAsync(string filePath)
    {
        var realPath = GetRealPath(filePath);
        DeleteFileIfExists(realPath);
    }
}

public class UploadedInfo : CustomFileInfo
{
    /// <summary>
    /// 除 destRootPath 以外的存储位置
    /// </summary>
    public string FilePath { get; set; } = null!;
}

public class RetryOptions
{
    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// 基础延迟时间(毫秒)
    /// </summary>
    public int BaseDelayMs { get; set; } = 1000;

    /// <summary>
    /// 是否使用指数退避
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;

    /// <summary>
    /// 最大延迟时间(毫秒)
    /// </summary>
    public int MaxDelayMs { get; set; } = 30000;

    /// <summary>
    /// 抖动因子(0-1之间)
    /// </summary>
    public double JitterFactor { get; set; } = 0.2;
}
