namespace Linger.Extensions.IO;

public static class PathExtensions
{
#if NETCOREAPP

    /// <summary>
    /// 获取相对路径
    /// </summary>
    /// <param name="path"></param>
    /// <param name="relativeTo"></param>
    /// <returns></returns>
    [Obsolete("Use StandardPathHelper.GetRelativePath intead of this method. Please note the order of those parameters is important!")]
    public static string GetRelativePath(this string path, string? relativeTo = null)
    {
        relativeTo ??= Environment.CurrentDirectory;
        return Path.GetRelativePath(relativeTo, path);
    }

#elif NETFRAMEWORK || NETSTANDARD
    /// <summary>
    /// 获取相对路径
    /// </summary>
    /// <param name="path"></param>
    /// <param name="relativeTo"></param>
    /// <returns></returns>
    [Obsolete("Use StandardPathHelper.GetRelativePath intead of this method. Please note the order of those parameters is important!")]
    public static string GetRelativePath(this string path, string? relativeTo = null)
    {
        relativeTo ??= Environment.CurrentDirectory;
        var absolutePath = path.GetAbsolutePath(relativeTo);
        return absolutePath.RelativeTo(relativeTo);
    }
#endif

    /// <summary>
    /// 获取绝对路径
    /// </summary>
    /// <param name="path">"..\Test" or "C:\Test"</param>
    /// <param name="basePath"></param>
    /// <returns>如果 path 为绝对路径，直接返回，若path为相对路径，就需要basePath</returns>
    [Obsolete("Use StandardPathHelper.ResolveToAbsolutePath intead of this method. Please note the order of those parameters is important!")]
    public static string GetAbsolutePath(this string path, string? basePath = null)
    {
        if (path.IsAbsolutePath())
        {
            return Path.GetFullPath(path);
        }

        basePath ??= Environment.CurrentDirectory;

        var combined = Path.Combine(basePath, path);
        combined = Path.GetFullPath(combined);
        return combined;
    }

    /// <summary>
    /// 判断此路径是否为绝对路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    [Obsolete]
    public static bool IsAbsolutePath(this string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return Path.IsPathRooted(path);
    }

    [Obsolete]
    public static string RelativeTo(this string sourcePath, string folder)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourcePath);
        ArgumentException.ThrowIfNullOrEmpty(folder);

        var pathUri = new Uri(sourcePath);

        if (!folder.IsAbsolutePath())
        {
            folder = folder.GetAbsolutePath();
        }

        // Folders must end in a slash
        if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            folder += Path.DirectorySeparatorChar;
        }

        var folderUri = new Uri(folder);
        Uri relativeUri = folderUri.MakeRelativeUri(pathUri);
        var relativePath = Uri.UnescapeDataString(relativeUri.ToString());
        return relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
    }
}
