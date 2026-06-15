namespace Linger.Helper;

public static partial class FileHelper
{
    /// <summary>
    /// 获取文本文件的行数
    /// </summary>
    /// <param name="filePath">文件的绝对路径</param>
    public static int GetLineCount(string filePath)
    {
        //将文本文件的各行读到一个字符串数组中
        var rows = File.ReadAllLines(filePath);

        //返回行数
        return rows.Length;
    }

    /// <summary>
    /// 获取一个文件的长度,单位为Byte
    /// </summary>
    /// <param name="filePath">文件的绝对路径</param>
    public static long GetFileSize(string filePath)
    {
        //创建一个文件对象
        var fi = new FileInfo(filePath);

        //获取文件的大小
        return fi.Length;
    }

    /// <summary>
    /// 获取指定目录中所有子目录列表,若要搜索嵌套的子目录列表,请使用重载方法.
    /// </summary>
    /// <param name="directoryPath">指定目录的绝对路径</param>
    public static string[] GetDirectories(string directoryPath)
    {
        return Directory.GetDirectories(directoryPath);
    }

    /// <summary>
    /// 获取指定目录及子目录中所有子目录列表
    /// </summary>
    /// <param name="directoryPath">指定目录的绝对路径</param>
    /// <param name="searchPattern">模式字符串，"*"代表0或N个字符，"?"代表1个字符。 范例："Log*.xml"表示搜索所有以Log开头的Xml文件。</param>
    /// <param name="isSearchChild">是否搜索子目录</param>
    public static string[] GetDirectories(string directoryPath, string searchPattern, bool isSearchChild)
    {
        return GetDirectories(directoryPath, searchPattern, searchOption: isSearchChild ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToArray();
    }

    /// <summary>
    /// 获取指定目录及子目录中所有文件列表
    /// </summary>
    /// <param name="directoryPath">指定目录的绝对路径</param>
    /// <param name="searchPattern">模式字符串，"*"代表0或N个字符，"?"代表1个字符。 范例："Log*.xml"表示搜索所有以Log开头的Xml文件。</param>
    /// <param name="isSearchChild">是否搜索子目录</param>
    public static string[] GetFileNames(string directoryPath, string searchPattern, bool isSearchChild)
    {
        return GetFileNames(directoryPath, searchPattern, searchOption: isSearchChild ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToArray();
    }

    /// <summary>
    /// 创建目录
    /// </summary>
    /// <param name="directoryPath">要创建的目录路径包括目录名</param>
    public static void CreateDirectoryIfNotExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            _ = Directory.CreateDirectory(directoryPath);
        }
    }

    /// <summary>
    /// 删除第二个文件夹里与第一个文件夹共有的文件
    /// </summary>
    /// <param name="varFromDirectory">指定文件夹路径</param>
    /// <param name="varToDirectory">对应其他文件夹路径</param>
    public static void DeleteFolderFiles(string varFromDirectory, string varToDirectory)
    {
        _ = Directory.CreateDirectory(varToDirectory);

        if (!Directory.Exists(varFromDirectory))
        {
            return;
        }

        var directories = Directory.GetDirectories(varFromDirectory);

        if (directories.Length > 0)
        {
            foreach (var d in directories)
            {
                var destDir = Path.Combine(varToDirectory, Path.GetFileName(d));
                DeleteFolderFiles(d, destDir);
            }
        }

        var files = Directory.GetFiles(varFromDirectory);

        if (files.Length > 0)
        {
            foreach (var s in files)
            {
                var destFile = Path.Combine(varToDirectory, Path.GetFileName(s));
                File.Delete(destFile);
            }
        }
    }
}
