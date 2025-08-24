using System;
using System.IO;
using Linger.Helper.PathHelpers;
using Xunit;

namespace Linger.UnitTests.Helper;

public class StandardPathHelperTests
{
    [Fact]
    public void NormalizePath_ShouldStandardizePaths()
    {
        // ����·������
        string path1 = "path/to/file";
        string path2 = "path\\to\\file";
        string path3 = "path//to\\\\file";

        string result1 = StandardPathHelper.NormalizePath(path1);
        string result2 = StandardPathHelper.NormalizePath(path2);
        string result3 = StandardPathHelper.NormalizePath(path3);

        string expectedSeparator = OSPlatformHelper.IsWindows ? "\\" : "/";
        string expected = $"path{expectedSeparator}to{expectedSeparator}file";

        Assert.Equal(expected, result1);
        Assert.Equal(expected, result2);
        Assert.Equal(expected, result3);

        // �߽���������
        Assert.Equal(null, StandardPathHelper.NormalizePath(null));
        Assert.Equal(string.Empty, StandardPathHelper.NormalizePath(string.Empty));
        Assert.Equal(string.Empty, StandardPathHelper.NormalizePath("   "));
    }

    [Fact]
    public void NormalizePath_WithEndingSeparator_ShouldPreserveEnding()
    {
        // ���Ա���ĩβ�ָ���
        string path1 = "path/to/folder";
        string path2 = "path/to/folder/";

        string result1 = StandardPathHelper.NormalizePath(path1, true);
        string result2 = StandardPathHelper.NormalizePath(path2, true);

        string expectedSeparator = OSPlatformHelper.IsWindows ? "\\" : "/";

        Assert.Equal($"path{expectedSeparator}to{expectedSeparator}folder{expectedSeparator}", result1);
        Assert.Equal($"path{expectedSeparator}to{expectedSeparator}folder{expectedSeparator}", result2);
    }

    [Fact]
    public void PathEquals_ShouldComparePathsCorrectly()
    {
        // ������Ȳ���
        Assert.True(StandardPathHelper.PathEquals("path/to/file", "path\\to\\file"));
        Assert.True(StandardPathHelper.PathEquals("path/to/file/", "path\\to\\file"));

        // ��Сд����
        Assert.True(StandardPathHelper.PathEquals("Path/To/File", "path\\to\\file"));
        Assert.False(StandardPathHelper.PathEquals("Path/To/File", "path\\to\\file", false));

        // null�Ϳ��ַ�������
        Assert.True(StandardPathHelper.PathEquals(null, null));
        Assert.False(StandardPathHelper.PathEquals(null, ""));
        Assert.False(StandardPathHelper.PathEquals("path", null));

        // ·����ͬ����
        Assert.False(StandardPathHelper.PathEquals("path1", "path2"));
    }

    [Fact]
    public void IsWindowsDriveLetter_ShouldIdentifyDriveLettersCorrectly()
    {
        // ��Ч��Windows�̷�
        Assert.True(StandardPathHelper.IsWindowsDriveLetter("C:"));
        Assert.True(StandardPathHelper.IsWindowsDriveLetter("Z:"));
        Assert.True(StandardPathHelper.IsWindowsDriveLetter("C:\\"));
        Assert.True(StandardPathHelper.IsWindowsDriveLetter("D:/"));
        Assert.True(StandardPathHelper.IsWindowsDriveLetter("E:\\path"));

        // ��Ч��Windows�̷�
        Assert.False(StandardPathHelper.IsWindowsDriveLetter(null));
        Assert.False(StandardPathHelper.IsWindowsDriveLetter(""));
        Assert.False(StandardPathHelper.IsWindowsDriveLetter("C"));
        Assert.False(StandardPathHelper.IsWindowsDriveLetter(":C"));
        Assert.False(StandardPathHelper.IsWindowsDriveLetter("1:"));
        Assert.False(StandardPathHelper.IsWindowsDriveLetter("C:*")); // �Ƿ��ַ�
    }

    [Fact]
    public void GetRelativePath_ShouldCalculateRelativePaths()
    {
        // ��ȡ��׼·����Ŀ��·��
        string baseDir = OSPlatformHelper.IsWindows ? "C:\\base\\path" : "/base/path";
        string targetSameLevel = OSPlatformHelper.IsWindows ? "C:\\base\\other" : "/base/other";
        string targetSubDir = OSPlatformHelper.IsWindows ? "C:\\base\\path\\subdir" : "/base/path/subdir";
        string targetParentDir = OSPlatformHelper.IsWindows ? "C:\\base" : "/base";
        string targetDifferentRoot = OSPlatformHelper.IsWindows ? "D:\\other\\path" : "/other/path";

        // ���Դ�basePath��targets�����·��
#if NETCOREAPP
        // .NET Core������ʹ��Path.GetRelativePathֱ�Ӳ���
        var relSameLevel = StandardPathHelper.GetRelativePath(baseDir, targetSameLevel);
        var relSubDir = StandardPathHelper.GetRelativePath(baseDir, targetSubDir);
        var relParent = StandardPathHelper.GetRelativePath(baseDir, targetParentDir);

        // ����ƽ̨�����Ľ��
        string expectedSameLevel = OSPlatformHelper.IsWindows ? "..\\other" : "../other";
        string expectedSubDir = "subdir";
        string expectedParent = "..";

        Assert.Equal(expectedSameLevel, relSameLevel);
        Assert.Equal(expectedSubDir, relSubDir);
        Assert.Equal(expectedParent, relParent);
#else
        // �������.NET Core������������֤���������쳣�ҷ��طǿս��
        Assert.NotNull(StandardPathHelper.GetRelativePath(baseDir, targetSameLevel));
        Assert.NotNull(StandardPathHelper.GetRelativePath(baseDir, targetSubDir));
        Assert.NotNull(StandardPathHelper.GetRelativePath(baseDir, targetParentDir));
#endif

        // ��Ե�������
        Assert.Equal(".", StandardPathHelper.GetRelativePath(baseDir, baseDir));
        Assert.Throws<System.ArgumentException>(() => StandardPathHelper.GetRelativePath("", targetSubDir));
        Assert.Equal(string.Empty, StandardPathHelper.GetRelativePath(baseDir, ""));
    }

    [Fact]
    public void ContainsInvalidPathChars_ShouldDetectInvalidCharacters()
    {
        // ��������Windows�Ƿ��ַ���·��
        string invalidWinChars = OSPlatformHelper.IsWindows ? "path*to?file" : null;

        // ��������ϵͳ�Ƿ��ַ���·��
        string invalidPath = $"path{Path.GetInvalidPathChars()[0]}file";
        string validPath = "path/to/file";

        // ���ԷǷ��ַ����
        Assert.True(StandardPathHelper.ContainsInvalidPathChars(invalidPath));
        Assert.False(StandardPathHelper.ContainsInvalidPathChars(validPath));

        // Windows�ض�����
        if (OSPlatformHelper.IsWindows && invalidWinChars != null)
        {
            Assert.True(StandardPathHelper.ContainsInvalidPathChars(invalidWinChars));
        }

        // ���Windows������
        if (OSPlatformHelper.IsWindows)
        {
            Assert.True(StandardPathHelper.ContainsInvalidPathChars("C:\\CON\\file.txt"));
            Assert.True(StandardPathHelper.ContainsInvalidPathChars("C:\\path\\NUL"));
            Assert.True(StandardPathHelper.ContainsInvalidPathChars("LPT1.txt"));
        }

        // null�Ϳ��ַ�������
        Assert.False(StandardPathHelper.ContainsInvalidPathChars(null));
        Assert.False(StandardPathHelper.ContainsInvalidPathChars(string.Empty));
    }

    [Fact]
    public void GetParentDirectory_ShouldReturnCorrectParentPath()
    {
        // ��������·��
        string testPath = Path.Combine("dir1", "dir2", "dir3");
        var fullPath = Path.GetFullPath(testPath);

        // ��ȡһ����Ŀ¼
        var parentDir = StandardPathHelper.GetParentDirectory(fullPath, 1);
        var expectedParent = Directory.GetParent(fullPath).FullName;
        Assert.Equal(expectedParent, parentDir);

        // ��ȡ������Ŀ¼
        var parentOfParent = StandardPathHelper.GetParentDirectory(fullPath, 2);
        var expectedGrandParent = Directory.GetParent(Directory.GetParent(fullPath).FullName).FullName;
        Assert.Equal(expectedGrandParent, parentOfParent);

        // ��Ե�������
        var noChange = StandardPathHelper.GetParentDirectory(fullPath, 0);
        Assert.Equal(fullPath, noChange);

        // ���Ը�ֵ����Ӧ��ȡ����ֵ��
        var negativeLevel = StandardPathHelper.GetParentDirectory(fullPath, -1);
        Assert.Equal(expectedParent, negativeLevel);
    }

    [Fact]
    public void Exists_ShouldDetectFileAndDirectoryExistence()
    {
        // ��ȡ��ǰĿ¼���϶����ڵ�Ŀ¼��
        string currentDir = Directory.GetCurrentDirectory();

        // ����Ŀ¼���ڼ��
        Assert.True(StandardPathHelper.Exists(currentDir, false));

        // ����һ����ʱ�ļ����ڲ���
        string tempFile = Path.GetTempFileName();
        try
        {
            // �����ļ����ڼ��
            Assert.True(StandardPathHelper.Exists(tempFile, true));

            // ���Բ����ڵ�·��
            Assert.False(StandardPathHelper.Exists(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()), true));
            Assert.False(StandardPathHelper.Exists(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()), false));

            // ������Ч·��
            Assert.False(StandardPathHelper.Exists("||invalid||path||"));

            // ���Կ�·��
            Assert.False(StandardPathHelper.Exists(null));
            Assert.False(StandardPathHelper.Exists(""));
            Assert.False(StandardPathHelper.Exists("   "));
        }
        finally
        {
            // ������ʱ�ļ�
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public void ResolveToAbsolutePath_ShouldResolveRelativePaths()
    {
        // ���Ի���·�������·��
        string baseDir = Directory.GetCurrentDirectory();
        string relativePath = "subdir/file.txt";

        var result = StandardPathHelper.ResolveToAbsolutePath(baseDir, relativePath);
        var expected = Path.GetFullPath(Path.Combine(baseDir, relativePath));

        Assert.Equal(expected, result);

        // ���Ծ���·��
        string absolutePath = Path.GetFullPath("file.txt");
        var absoluteResult = StandardPathHelper.ResolveToAbsolutePath(baseDir, absolutePath);
        Assert.Equal(absolutePath, absoluteResult);

        // ���Ա�Ե���
        Assert.Equal(string.Empty, StandardPathHelper.ResolveToAbsolutePath(baseDir, ""));
        Assert.Equal(string.Empty, StandardPathHelper.ResolveToAbsolutePath(baseDir, null));

        // ���Ա���ĩβ�ָ���
        var withSeparator = StandardPathHelper.ResolveToAbsolutePath(baseDir, "subdir/", true);
        Assert.EndsWith(Path.DirectorySeparatorChar.ToString(), withSeparator);
    }

    [Fact]
    public void PathEquals_ShouldHandlePathExceptions()
    {
        // ���԰����Ƿ��ַ���·��������ܵ���Path.GetFullPath�쳣
        string invalidPath1 = "C:\\" + new string('\0', 1) + "invalid"; // �������ַ�
        string invalidPath2 = "normal/path";

        // Ӧ���ܹ������쳣���������false�������׳��쳣
        var result = StandardPathHelper.PathEquals(invalidPath1, invalidPath2);
        Assert.False(result);

        // ���Լ���·��
        var longPath = new string('a', 300);
        var normalPath = "short/path";
        var longPathResult = StandardPathHelper.PathEquals(longPath, normalPath);
        Assert.False(longPathResult);
    }

    [Fact]
    public void GetRelativePath_ShouldHandlePathExceptions()
    {
        string validPath = Directory.GetCurrentDirectory();
        
        // ����null path�������ؿ��ַ������
        var nullPathResult = StandardPathHelper.GetRelativePath(validPath, null);
        Assert.Equal(string.Empty, nullPathResult);

        // ���԰����Ƿ��ַ���·��
        string invalidPath = "invalid\0path";
        
        // Ӧ���׳�ArgumentException�����������쳣
        Assert.Throws<System.ArgumentException>(() => StandardPathHelper.GetRelativePath(validPath, invalidPath));
    }

    [Fact]
    public void IsWindowsDriveLetter_ShouldHandleInvalidPathChars()
    {
        // ���԰����Ƿ��ַ������ȴ���3�����
        string pathWithInvalidChars = "C:\\inv*lid";
        
        // Ӧ�ü�⵽�Ƿ��ַ�������false
        var result = StandardPathHelper.IsWindowsDriveLetter(pathWithInvalidChars);
        Assert.False(result);

        // ���Գ���Ϊ3�ҵ������ַ����Ƿָ��������  
        var invalidDrive = "C:x";
        Assert.False(StandardPathHelper.IsWindowsDriveLetter(invalidDrive));

        // ������Ч�Ľϳ�·��
        var validLongPath = "D:\\ValidPath\\SubDir";
        Assert.True(StandardPathHelper.IsWindowsDriveLetter(validLongPath));
    }

    [Fact] 
    public void ContainsInvalidPathChars_ShouldCheckNonWindowsPlatforms()
    {
        // ���������Ҫ��Ϊ�˸��Ƿ�Windowsƽ̨�Ĵ���·��
        // ��Windows��������Կ��ܲ���ִ�е�Unix��֧������Ȼ�������������߼�
        
        // ���԰���null�ַ���·����Unixϵͳ�еķǷ��ַ���
        string pathWithNull = "path\0with\0null";
        
        // ������ƽ̨�ϣ�����null�ַ���·����Ӧ�ñ���Ϊ�ǷǷ���
        var result = StandardPathHelper.ContainsInvalidPathChars(pathWithNull);
        
        // ע�⣺��Windows�����ͨ��ϵͳ�Ƿ��ַ���鲶����Unix��ͨ��null�ַ���鲶��
        Assert.True(result);
    }

    [Fact]
    public void GetParentDirectory_ShouldHandleRootPath()
    {
        // ���Ը�Ŀ¼�����Directory.GetParent���ܷ���null
        string rootPath;
        if (OSPlatformHelper.IsWindows)
        {
            rootPath = "C:\\";
        }
        else
        {
            rootPath = "/";
        }

        // ���Ի�ȡ��Ŀ¼�ĸ�Ŀ¼
        var result = StandardPathHelper.GetParentDirectory(rootPath, 1);
        
        // ����޷���ȡ��Ŀ¼��Ӧ�÷���ԭ·��
        Assert.NotNull(result);
    }

    [Fact] 
    public void GetRelativePathCore_ShouldHandleEmptyResult()
    {
        // ����������GetRelativePathCore������result.CountΪ0�����
        string basePath = Directory.GetCurrentDirectory();
        string samePath = basePath;
        
        var result = StandardPathHelper.GetRelativePath(basePath, samePath);
        
        // ��ͬ·��Ӧ�÷���"."
        Assert.Equal(".", result);
    }

    [Fact]
    public void NormalizePath_ShouldHandleNullAndWhitespace()
    {
        // ��ϸ���Ը��ֿ�ֵ���
        Assert.Equal(string.Empty, StandardPathHelper.NormalizePath(""));
        Assert.Equal(string.Empty, StandardPathHelper.NormalizePath("   "));
        Assert.Equal(string.Empty, StandardPathHelper.NormalizePath("\t\n"));
        Assert.Null(StandardPathHelper.NormalizePath(null));
    }

    [Fact]
    public void Exists_ShouldHandleExceptions()
    {
        // ���Էǳ�����·�������ܵ���PathTooLongException
        var veryLongPath = new string('a', 500);
        
        // Ӧ�����Ŵ����쳣������false�������׳��쳣
        var result = StandardPathHelper.Exists(veryLongPath);
        Assert.False(result);

        // ���԰����Ƿ��ַ���·��
        var invalidPath = "path\0with\0null";
        var invalidResult = StandardPathHelper.Exists(invalidPath);
        Assert.False(invalidResult);
    }
}
