namespace Linger.FileSystem.Helpers;

public class FtpHelper
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

    public static string ServerDetails(string host, string port, string? userName, string type = "Ftp")
    {
        return $"Type: '{type}' Host:'{host}' Port:'{port}' User:'{userName}'";
    }
}