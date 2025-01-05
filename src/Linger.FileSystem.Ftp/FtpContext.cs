using FluentFTP;
using Linger.FileSystem.Remote;
using Linger.Logging;

namespace Linger.FileSystem.Ftp;

/*FluentFTP example : https://github.com/robinrodricks/FluentFTP
 * Local Directory:             @"C:\Files\Temp\file.csv"
 *                              @"C:\Files\Temp"
 * Ftp Directory(FluentFTP):    @"/Files/Temp/file.csv"
 *                              @"/Files/Temp/
 */

public abstract class FtpContext : IRemoteFileSystemContext
{
    protected IFtpClient FtpClient { get; set; } = default!;

    //public void Connect()
    //{
    //    FtpClient.Connect();
    //}

    public void Disconnect()
    {
        FtpClient.Disconnect();
    }

    public void Dispose()
    {
        if (!FtpClient.IsDisposed)
        {
            FtpClient.Dispose();
        }
    }

    /*actions*/

    public bool FileExists(string filePath)
    {
        return FtpClient.FileExists(filePath);
    }

    public void DeleteFileIfExists(string filePath)
    {
        if (!FileExists(filePath))
        {
            FtpClient.DeleteFile(filePath);
        }
    }

    public bool DirectoryExists(string directoryPath)
    {
        return FtpClient.DirectoryExists(directoryPath);
    }

    public void CreateDirectoryIfNotExists(string directoryPath)
    {
        if (!DirectoryExists(directoryPath))
        {
            _ = FtpClient.CreateDirectory(directoryPath);
        }
    }

    public void SetWorkingDirectory(string directoryPath)
    {
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            if (DirectoryExists(directoryPath))
            {
                FtpClient.SetWorkingDirectory(directoryPath);
            }
        }
    }

    public void SetRootAsWorkingDirectory()
    {
        SetWorkingDirectory(string.Empty);
    }

    public void UploadFile(string localFilePath, string remoteFilePath)
    {
        _ = FtpClient.UploadFile(localFilePath, remoteFilePath);
    }

    public abstract string ServerDetails();

    /// <summary>
    ///     获取文件最后修改时间
    /// </summary>
    /// <param name="remotePath">远程路径("/test/abc.txt")</param>
    /// <returns></returns>
    public DateTime GetLastModifiedTime(string remotePath)
    {
        DateTime dateTime = FtpClient.GetModifiedTime(remotePath);
        return dateTime;
    }

    #region FTP是否已连接

    /// <summary>
    ///     FTP是否已连接
    /// </summary>
    /// <returns></returns>
    public bool IsConnected()
    {
        var result = FtpClient.IsConnected;

        return result;
    }

    #endregion FTP是否已连接

    #region 连接FTP

    /// <summary>
    ///     连接FTP
    /// </summary>
    /// <returns></returns>
    public void Connect()
    {
        if (!IsConnected())
        {
            FtpClient.Connect();
        }
    }

    #endregion 连接FTP

    #region 下载单文件

    /// <summary>
    ///     下载单文件
    /// </summary>
    /// <param name="localDic">本地目录(@"D:\test")</param>
    /// <param name="remotePath">远程路径("/test/abc.txt")</param>
    /// <returns></returns>
    public bool DownloadFile(string localDic, string remotePath)
    {
        var boolResult = false;

        try
        {
            //本地目录不存在，则自动创建
            if (!Directory.Exists(localDic))
            {
                _ = Directory.CreateDirectory(localDic);
            }

            //取下载文件的文件名
            var strFileName = Path.GetFileName(remotePath);
            //拼接本地路径
            localDic = Path.Combine(localDic, strFileName);

            Connect();
            boolResult = FtpClient.DownloadFile(localDic, remotePath) == FtpStatus.Success;
        }
        catch (Exception ex)
        {
            LogHelper.Error("DownloadFile->下载文件 异常:" + ex + "|*|remotePath:" + remotePath);
        }
        finally
        {
            DisConnect();
        }

        return boolResult;
    }

    #endregion 下载单文件

    #region 断开FTP

    /// <summary>
    ///     断开FTP
    /// </summary>
    public void DisConnect()
    {
        if (FtpClient.IsConnected)
        {
            //每次操作完成后断开连接，导致WorkDirectory失效
            FtpClient.Disconnect();
        }
    }

    #endregion 断开FTP

    #region 取得文件或目录列表

    /// <summary>
    ///     取得文件或目录列表
    /// </summary>
    /// <param name="remoteDic">远程目录</param>
    /// <param name="type">类型:file-文件,dic-目录</param>
    /// <returns></returns>
    public List<string> ListDirectory(string? remoteDic = null, string type = "file")
    {
        var list = new List<string>();
        type = type.ToLower();

        try
        {
            Connect();

            FtpListItem[] files = remoteDic == null ? FtpClient.GetListing() : FtpClient.GetListing(remoteDic);
            foreach (FtpListItem file in files)
            {
                if (type == "file")
                {
                    if (file.Type == FtpObjectType.File)
                    {
                        list.Add(file.Name);
                    }
                }
                else if (type == "dic")
                {
                    if (file.Type == FtpObjectType.Directory)
                    {
                        list.Add(file.Name);
                    }
                }
                else
                {
                    list.Add(file.Name);
                }
            }
        }
        catch (Exception ex)
        {
            LogHelper.Error("ListDirectory->取得文件或目录列表 异常:" + ex);
        }
        //finally
        //{
        //    DisConnect();
        //}

        return list;
    }

    #endregion 取得文件或目录列表

    #region 上传多文件

    /// <summary>
    ///     上传多文件
    /// </summary>
    /// <param name="localFiles">本地路径列表</param>
    /// <param name="remoteDic">远端目录("/test")</param>
    /// <param name="remoteExistsMode"></param>
    /// <returns></returns>
    public int UploadFiles(IEnumerable<string> localFiles, string remoteDic, FtpRemoteExists remoteExistsMode)
    {
        var count = 0;
        var listFiles = new List<FileInfo>();

        try
        {
            foreach (var file in localFiles)
            {
                if (!File.Exists(file))
                {
                    LogHelper.Error("UploadFiles->本地文件不存在:" + file);
                    continue;
                }

                listFiles.Add(new FileInfo(file));
            }

            //远端路径校验
            if (string.IsNullOrEmpty(remoteDic))
            {
                remoteDic = "/";
            }

            if (!remoteDic.StartsWith("/"))
            {
                remoteDic = "/" + remoteDic;
            }

            if (!remoteDic.EndsWith("/"))
            {
                remoteDic += "/";
            }

            Connect();
            if (listFiles.Count > 0)
            {
                List<FtpResult>? list = FtpClient.UploadFiles(listFiles, remoteDic, remoteExistsMode);
                count = list.Count;
            }
        }
        catch (Exception ex)
        {
            LogHelper.Error("UploadFiles->上传文件 异常:" + ex);
        }
        finally
        {
            DisConnect();
        }

        return count;
    }

    #endregion 上传多文件

    #region 下载多文件

    /// <summary>
    ///     下载多文件
    /// </summary>
    /// <param name="localDic">本地目录(@"D:\test")</param>
    /// <param name="remoteFiles">远程路径列表</param>
    /// <returns></returns>
    public int DownloadFiles(string localDic, IEnumerable<string> remoteFiles)
    {
        var count = 0;

        try
        {
            //本地目录不存在，则自动创建
            if (!Directory.Exists(localDic))
            {
                _ = Directory.CreateDirectory(localDic);
            }

            Connect();
            List<FtpResult> list = FtpClient.DownloadFiles(localDic, remoteFiles);
            count = list.Count;
        }
        catch (Exception ex)
        {
            LogHelper.Error("DownloadFiles->下载文件 异常:" + ex);
        }
        finally
        {
            DisConnect();
        }

        return count;
    }

    #endregion 下载多文件

    #region 判断文件是否存在

    /// <summary>
    ///     判断文件是否存在
    /// </summary>
    /// <param name="remotePath">远程路径("/test/abc.txt")</param>
    /// <returns></returns>
    public bool IsFileExists(string remotePath)
    {
        var boolResult = false;

        try
        {
            Connect();
            boolResult = FtpClient.FileExists(remotePath);
        }
        catch (Exception ex)
        {
            LogHelper.Error("IsFileExists->判断文件是否存在 异常:" + ex + "|*|remotePath:" + remotePath);
        }
        finally
        {
            DisConnect();
        }

        return boolResult;
    }

    #endregion 判断文件是否存在

    #region 判断目录是否存在

    /// <summary>
    ///     判断目录是否存在
    /// </summary>
    /// <param name="remotePath">远程路径("/test")</param>
    /// <returns></returns>
    public bool IsDirExists(string remotePath)
    {
        var boolResult = false;

        try
        {
            Connect();
            boolResult = FtpClient.DirectoryExists(remotePath);
        }
        catch (Exception ex)
        {
            LogHelper.Error("IsDirExists->判断目录是否存在 异常:" + ex + "|*|remotePath:" + remotePath);
        }
        finally
        {
            DisConnect();
        }

        return boolResult;
    }

    #endregion 判断目录是否存在

    #region 新建目录

    /// <summary>
    ///     新建目录
    /// </summary>
    /// <param name="remoteDic">远程目录("/test")</param>
    /// <returns></returns>
    public bool MakeDir(string remoteDic)
    {
        var boolResult = false;

        try
        {
            Connect();
            _ = FtpClient.CreateDirectory(remoteDic);

            boolResult = true;
        }
        catch (Exception ex)
        {
            LogHelper.Error("MakeDir->新建目录 异常:" + ex + "|*|remoteDic:" + remoteDic);
        }
        finally
        {
            DisConnect();
        }

        return boolResult;
    }

    #endregion 新建目录

    #region 获取文件最后修改时间

    /// <summary>
    ///     获取文件最后修改时间
    /// </summary>
    /// <param name="remotePath">远程路径("/test/abc.txt")</param>
    /// <returns></returns>
    public DateTime GetModifiedTime(string remotePath)
    {
        DateTime dateTime = DateTime.MinValue;

        try
        {
            Connect();
            dateTime = FtpClient.GetModifiedTime(remotePath);
        }
        catch (Exception ex)
        {
            LogHelper.Error("GetModifiedTime->获取文件最后修改时间 异常:" + ex + "|*|remotePath:" + remotePath);
        }
        finally
        {
            DisConnect();
        }

        return dateTime;
    }

    #endregion 获取文件最后修改时间

    #region 清理

    /// <summary>
    ///     清理
    /// </summary>
    public void Clean()
    {
        //断开FTP
        DisConnect();

        FtpClient.Dispose();
    }

    #endregion 清理

    #region 上传单文件

    /// <summary>
    ///     上传单文件 如果设定了WorkingDirectory,需要使用此方法
    /// </summary>
    /// <param name="localPath">本地路径(@"D:\abc.txt")</param>
    /// <param name="remoteFileName">远端文件名称("abc.txt"),若为null,默认使用本地的文件名称</param>
    /// <param name="remoteExistsMode">默认覆写</param>
    /// <returns></returns>
    public bool UploadFile2(string localPath, string? remoteFileName = null,
        FtpRemoteExists remoteExistsMode = FtpRemoteExists.Overwrite)
    {
        var boolResult = false;

        try
        {
            //本地路径校验
            if (!File.Exists(localPath))
            {
                LogHelper.Error("UploadFile->本地文件不存在:" + localPath);
                return boolResult;
            }

            var fileInfo = new FileInfo(localPath);
            //拼接远端路径
            remoteFileName ??= fileInfo.Name;

            Connect();
            using FileStream fs = fileInfo.OpenRead();
            //重名覆盖
            FtpStatus result = FtpClient.UploadStream(fs, remoteFileName, remoteExistsMode, true);

            boolResult = result == FtpStatus.Success;
        }
        catch (Exception ex)
        {
            LogHelper.Error("UploadFile->上传文件 异常:" + ex + "|*|localPath:" + localPath);
        }
        finally
        {
            DisConnect();
        }

        return boolResult;
    }

    #endregion 上传单文件
}