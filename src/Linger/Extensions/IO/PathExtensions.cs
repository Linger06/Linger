using Linger.Helper;

namespace Linger.Extensions.IO;

/// <summary>
/// Provides Extension methods for path.
/// </summary>
public static class PathExtensions
{
    public static string GetAbsolutePath(this string path, string? basePath = null) => PathHelper.ResolveToAbsolutePath(basePath, path);
    public static string GetRelativePath(this string path, string? relativeTo = null) => PathHelper.GetRelativePath(relativeTo ?? Environment.CurrentDirectory, path);
    public static bool IsAbsolutePath(this string path) => !string.IsNullOrEmpty(path) && Path.IsPathRooted(path);
    public static string RelativeTo(this string sourcePath, string folder) => GetRelativePath(sourcePath, folder);
    public static string NormalizeWithSeparator(this string path) => PathHelper.NormalizePath(path, true);
    public static string ToAbsolutePath(this string path, string? basePath = null) => path.GetAbsolutePath(basePath);
    public static string GetParentPath(this string path, int levels = 1) => PathHelper.GetParentDirectory(path, levels);
    public static bool PathEqualsTo(this string path, string otherPath) => PathHelper.PathEquals(path, otherPath);
}