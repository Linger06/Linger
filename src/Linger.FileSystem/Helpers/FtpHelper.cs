namespace Linger.FileSystem.Helpers;

public static class FtpHelper
{
    /// <summary>
    ///     string p1 = "/temp";
    ///     to /temp/
    /// </summary>
    public static string FtpDirectory(string rootDirectory)
    {
        rootDirectory = rootDirectory.Trim('/');
        return $"/{rootDirectory}/";
    }

    /// <summary>
    ///     string p1 = "/temp/";
    ///     string p2 = "/subDir/file/";
    ///     to /temp/subDir/file/
    /// </summary>
    public static string CombineDirectory(string rootDirectory, string childDirectory)
    {
        rootDirectory = rootDirectory.Trim('/');
        childDirectory = childDirectory.Trim('/');
        return $"/{rootDirectory}/{childDirectory}/";
    }

    /// <summary>
    ///     string p1 = "/temp/";
    ///     string p2 = "file.text";
    ///     to /temp/file.text
    /// </summary>
    public static string CombineFile(string rootDirectory, string filePathOrName)
    {
        rootDirectory = rootDirectory.Trim('/');
        filePathOrName = filePathOrName.Trim('/');
        return $"/{rootDirectory}/{filePathOrName}";
    }

    /// <summary>
    /// 生成服务器详情描述字符串
    /// </summary>
    /// <param name="host">主机地址</param>
    /// <param name="port">端口</param>
    /// <param name="userName">用户名</param>
    /// <param name="type">连接类型</param>
    /// <returns>服务器详情描述</returns>
    public static string ServerDetails(string host, string port, string userName, string type)
    {
        return $"{type}://{userName}@{host}:{port}";
    }
}