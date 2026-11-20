namespace Linger.FileSystem.Local;

public partial class LocalFileSystem
{
    /// <summary>
    /// 异步下载文件到当前工作目录下的指定文件名
    /// </summary>
    /// <param name="sourceFilePath">源文件的相对路径（相对于 RootDirectoryPath）或绝对路径</param>
    /// <param name="localDestinationPath">目标文件名（仅文件名，不包含路径）。文件将保存到当前工作目录</param>
    /// <param name="overwrite">是否覆盖已存在的同名文件。默认为 false</param>
    /// <param name="useSequencedName">当目标文件已存在且不允许覆盖时，是否使用序号命名（如 file[1].txt）。默认为 true</param>
    /// <returns>返回实际下载后的文件完整路径</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="sourceFilePath"/> 或 <paramref name="localDestinationPath"/> 为 null 或空字符串时抛出</exception>
    /// <exception cref="FileNotFoundException">当源文件不存在时抛出</exception>
    /// <exception cref="DuplicateFileException">当目标文件已存在且 <paramref name="overwrite"/> 为 false 且 <paramref name="useSequencedName"/> 为 false 时抛出</exception>
    /// <remarks>
    /// <para>此方法会自动处理文件名冲突：</para>
    /// <list type="bullet">
    /// <item><description>如果 <paramref name="overwrite"/> 为 true，直接覆盖现有文件</description></item>
    /// <item><description>如果 <paramref name="overwrite"/> 为 false 且 <paramref name="useSequencedName"/> 为 true，会生成序号文件名（如 file[1].txt, file[2].txt）</description></item>
    /// <item><description>如果 <paramref name="overwrite"/> 为 false 且 <paramref name="useSequencedName"/> 为 false，会抛出 <see cref="DuplicateFileException"/> 异常</description></item>
    /// </list>
    /// <para>方法内部使用 <see cref="RetryHelper"/> 进行重试，确保操作的可靠性。</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 下载文件到当前目录，如果存在则使用序号命名
    /// var actualPath = await fileSystem.DownloadAsync("uploads/document.pdf", "document.pdf");
    ///
    /// // 下载文件并直接覆盖同名文件
    /// var actualPath = await fileSystem.DownloadAsync("uploads/document.pdf", "document.pdf", overwrite: true);
    ///
    /// // 下载文件，如果存在同名文件则抛出异常
    /// var actualPath = await fileSystem.DownloadAsync("uploads/document.pdf", "document.pdf",
    ///     overwrite: false, useSequencedName: false);
    /// </code>
    /// </example>
    public async Task<string> DownloadAsync(string sourceFilePath, string localDestinationPath, bool overwrite = false, bool useSequencedName = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceFilePath);
        ArgumentException.ThrowIfNullOrEmpty(localDestinationPath);

        Logger.LogDebug("Starting download: {Source} -> {Destination}", sourceFilePath, localDestinationPath);

        var realSourcePath = GetRealPath(sourceFilePath);
        if (!File.Exists(realSourcePath))
        {
            throw new FileNotFoundException("The source file does not exist.", realSourcePath);
        }

        var destFilePath = GetUniqueDestFilePath(
            localDestinationPath,
            overwrite,
            useSequencedName,
            cancellationToken);

        return await RetryHelper.ExecuteAsync(
            async operationCancellationToken =>
            {
                await CopyFileAtomicallyAsync(
                    realSourcePath,
                    destFilePath,
                    overwrite,
                    _options.DownloadBufferSize,
                    operationCancellationToken).ConfigureAwait(false);

                return destFilePath;
            },
            "File download",
            IsRetryableLocalFileOperationException,
            cancellationToken: cancellationToken).ConfigureAwait(false);

    }

    /// <summary>
    /// 下载文件到指定的流（内部实现）
    /// </summary>
    /// <param name="filePath">源文件路径</param>
    /// <param name="destStream">目标流</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>完成任务</returns>
    /// <exception cref="ArgumentNullException">文件路径为空时抛出</exception>
    /// <exception cref="ArgumentNullException">目标流为空时抛出</exception>
    /// <exception cref="FileNotFoundException">源文件不存在时抛出</exception>
    private async Task DownloadToStreamInternalAsync(string filePath, Stream destStream, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        ArgumentNullException.ThrowIfNull(destStream);

        var sourceFilePath = GetRealPath(filePath);
        if (!File.Exists(sourceFilePath))
        {
            throw new FileNotFoundException("Source file not found", sourceFilePath);
        }

        var sourceStream = File.OpenRead(sourceFilePath);

#if NET8_0_OR_GREATER
        await using (sourceStream.ConfigureAwait(false))
#else
        using (sourceStream)
#endif
        {
            sourceStream.Position = 0;
            await sourceStream.CopyToAsync(destStream, _options.DownloadBufferSize, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 获取唯一的目标文件路径并确保其父目录存在。
    /// </summary>
    /// <param name="destFileName">目标文件名（可以包含相对路径）</param>
    /// <param name="overwrite">是否允许覆盖已存在的文件</param>
    /// <param name="useSequencedName">当文件已存在且不允许覆盖时，是否使用序号命名（如 file[1].txt）</param>
    /// <returns>返回唯一的目标文件路径，如果需要会自动生成序号</returns>
    /// <exception cref="DuplicateFileException">当文件已存在且不允许覆盖和序号命名时抛出</exception>
    /// <remarks>
    /// <para>此方法基于 <see cref="GetDestFilePath(string, string, bool, bool, string)"/> 处理命名冲突，并使用调用方提供的本地目标路径。</para>
    /// <list type="bullet">
    /// <item><description>自动创建目标目录（如果不存在）</description></item>
    /// <item><description>支持相对路径的文件名参数</description></item>
    /// </list>
    /// <para>处理逻辑：</para>
    /// <list type="number">
    /// <item><description>调用 <see cref="GetDestFilePath(string, string, bool, bool, string)"/> 获取唯一路径</description></item>
    /// <item><description>提取目标路径中的目录部分</description></item>
    /// <item><description>返回最终的文件路径</description></item>
    /// </list>
    /// <para>此方法主要用于下载操作，确保目标文件可以被成功创建。</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 基本使用 - 仅文件名
    /// var path1 = GetUniqueDestFilePath("document.pdf", false, true);
    /// // 如果document.pdf存在，可能返回: "document[1].pdf"
    ///
    /// // 包含相对路径的文件名
    /// var path2 = GetUniqueDestFilePath("downloads\\document.pdf", false, true);
    /// // 会自动创建downloads目录，如果文件存在可能返回: "downloads\\document[1].pdf"
    ///
    /// // 允许覆盖的情况
    /// var path3 = GetUniqueDestFilePath("temp\\file.txt", true, false);
    /// // 返回: "temp\\file.txt"，会创建temp目录并允许覆盖
    ///
    /// // 严格模式
    /// try
    /// {
    ///     var path4 = GetUniqueDestFilePath("existing.txt", false, false);
    /// }
    /// catch (DuplicateFileException)
    /// {
    ///     // 如果existing.txt已存在，会抛出此异常
    /// }
    /// </code>
    /// </example>
    private static string GetUniqueDestFilePath(string destFileName, bool overwrite, bool useSequencedName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fullDestinationPath = Path.GetFullPath(destFileName);
        var directory = Path.GetDirectoryName(fullDestinationPath);
        if (string.IsNullOrEmpty(directory))
        {
            throw new ArgumentException("Destination path must include a directory.", nameof(destFileName));
        }

        Directory.CreateDirectory(directory);

        var destinationFilePath = GetDestFilePath(
            directory,
            Path.GetFileName(fullDestinationPath),
            overwrite,
            useSequencedName);

        return destinationFilePath;
    }
}
