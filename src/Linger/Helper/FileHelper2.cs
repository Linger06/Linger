using System.Text;
using Linger.Extensions.IO;

namespace Linger.Helper;

public static partial class FileHelper
{
    /// <summary>
    /// 检测指定文件是否存在,如果存在则返回true。
    /// </summary>
    /// <param name="filePath">文件的绝对路径</param>
    /// <remarks>This method is obsolete. Use <see cref="File.Exists(string)"/> directly.</remarks>
    [Obsolete("Use File.Exists() directly. This method will be removed in a future version.")]
    public static bool IsExistFile(string filePath)
    {
        return File.Exists(filePath);
    }

    /// <summary>
    /// Writes text content to a file.
    /// </summary>
    /// <param name="fileName">The file name to write to.</param>
    /// <param name="content">The content to write.</param>
    /// <param name="encoding">The encoding to use. Defaults to UTF-8.</param>
    /// <remarks>This method is obsolete. Use <see cref="WriteText(string, string, Encoding?)"/> instead.</remarks>
    [Obsolete("Use WriteText() instead. This method will be removed in a future version.")]
    public static void WriteTxt(string fileName, string content, Encoding? encoding = null)
    {
        var filePath = fileName.GetFilePathString();
        CreateDirectoryIfNotExists(filePath);

        encoding ??= Encoding.UTF8;

        using var sw = new StreamWriter(fileName, false, encoding);
        sw.Write(content);
        sw.Flush();
    }

    /// <summary>
    /// 将源文件的内容复制到目标文件中
    /// </summary>
    /// <param name="sourceFilePath">源文件的绝对路径</param>
    /// <param name="destFilePath">目标文件的绝对路径</param>
    /// <remarks>This method is obsolete. Use <see cref="CopyFile(string, string)"/> instead.</remarks>
    [Obsolete("Use CopyFile() instead. This method will be removed in a future version.")]
    public static void Copy(string sourceFilePath, string destFilePath)
    {
        File.Copy(sourceFilePath, destFilePath, true);
    }

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
    /// Reads text content from a file.
    /// </summary>
    /// <param name="filename">The file name to read from.</param>
    /// <param name="encoding">The encoding to use. Defaults to UTF-8.</param>
    /// <returns>The file content, or empty string if file doesn't exist or read fails.</returns>
    /// <remarks>This method is obsolete. Use <see cref="ReadText(string, Encoding?)"/> or <see cref="TryReadText(string, out string, Encoding?)"/> instead.</remarks>
    [Obsolete("Use ReadText() or TryReadText() instead. This method will be removed in a future version.")]
    public static string ReadTxt(string filename, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        var str = string.Empty;

        if (!File.Exists(filename))
        {
            return str;
        }

        try
        {
            using var sr = new StreamReader(filename, encoding);
            str = sr.ReadToEnd();
        }
        catch
        {
            // ignored
        }

        return str;
    }

    /// <summary>
    /// 检测指定目录是否存在
    /// </summary>
    /// <param name="directoryPath">目录的绝对路径</param>
    /// <returns></returns>
    /// <remarks>This method is obsolete. Use <see cref="Directory.Exists(string)"/> directly.</remarks>
    [Obsolete("Use Directory.Exists() directly. This method will be removed in a future version.")]
    public static bool IsExistDirectory(string directoryPath)
    {
        return Directory.Exists(directoryPath);
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
    /// 复制文件夹(递归)
    /// </summary>
    /// <param name="varFromDirectory">源文件夹路径</param>
    /// <param name="varToDirectory">目标文件夹路径</param>
    /// <remarks>This method is obsolete. Use <see cref="CopyDir(string, string)"/> instead.</remarks>
    [Obsolete("Use CopyDir() instead. This method will be removed in a future version.")]
    public static void CopyFolder(string varFromDirectory, string varToDirectory)
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
                CopyFolder(d, destDir);
            }
        }

        var files = Directory.GetFiles(varFromDirectory);
        if (files.Length > 0)
        {
            foreach (var s in files)
            {
                var destFile = Path.Combine(varToDirectory, Path.GetFileName(s));
                File.Copy(s, destFile, true);
            }
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

    /// <summary>
    /// 从文件的绝对路径中获取文件名( 包含扩展名 )
    /// </summary>
    /// <param name="filePath">文件的绝对路径</param>
    /// <remarks>This method is obsolete. Use <see cref="Path.GetFileName(string)"/> directly.</remarks>
    [Obsolete("Use Path.GetFileName() directly. This method will be removed in a future version.")]
    public static string GetFileName(string filePath)
    {
        //获取文件的名称
        var fi = new FileInfo(filePath);
        return fi.Name;
    }
}
