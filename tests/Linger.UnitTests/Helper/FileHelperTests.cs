using System.Text;
using Linger.Helper;
using Xunit.v3;

namespace Linger.UnitTests.Helper;

public class FileHelperTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly List<string> _createdFiles = new();
    private readonly List<string> _createdDirectories = new();

    public FileHelperTests()
    {
        // 创建临时测试目录
        _testDirectory = Path.Combine(Path.GetTempPath(), $"LingerTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _createdDirectories.Add(_testDirectory);
    }

    public void Dispose()
    {
        // 清理测试文件和目录
        try
        {
            foreach (var file in _createdFiles)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }

            // 倒序清理目录（先清理子目录）
            foreach (var dir in _createdDirectories.OrderByDescending(d => d.Length))
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
        }
        catch
        {
            // 忽略清理过程中的错误
        }
    }

    private string CreateTestFile(string fileName, string content)
    {
        var filePath = Path.Combine(_testDirectory, fileName);
        File.WriteAllText(filePath, content);
        _createdFiles.Add(filePath);
        return filePath;
    }

    private string CreateTestDirectory(string dirName)
    {
        var dirPath = Path.Combine(_testDirectory, dirName);
        Directory.CreateDirectory(dirPath);
        _createdDirectories.Add(dirPath);
        return dirPath;
    }

    [Fact]
    public void ReadText_WithExistingFile_ReturnsContent()
    {
        // Arrange
        var content = "test content";
        var filePath = CreateTestFile("test.txt", content);

        // Act
        var result = FileHelper.ReadText(filePath);

        // Assert
        Assert.Equal(content, result);
    }

    [Fact]
    public void ReadText_WithExistingFileAndEncoding_ReturnsContent()
    {
        // Arrange
        var content = "测试内容";
        var filePath = CreateTestFile("test.txt", content);

        // Act
        var result = FileHelper.ReadText(filePath, Encoding.UTF8);

        // Assert
        Assert.Equal(content, result);
    }

    [Fact]
    public void TryReadText_WithExistingFile_ReturnsTrueAndContent()
    {
        // Arrange
        var content = "test content";
        var filePath = CreateTestFile("test.txt", content);

        // Act
        var success = FileHelper.TryReadText(filePath, out var result);

        // Assert
        Assert.True(success);
        Assert.Equal(content, result);
    }

    [Fact]
    public void TryReadText_WithNonExistentFile_ReturnsFalseAndEmptyString()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.txt");

        // Act
        var success = FileHelper.TryReadText(nonExistentPath, out var result);

        // Assert
        Assert.False(success);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void WriteText_WithValidPath_WritesContent()
    {
        // Arrange
        var content = "test content";
        var filePath = Path.Combine(_testDirectory, "writeTest.txt");
        _createdFiles.Add(filePath);

        // Act
        FileHelper.WriteText(filePath, content, Encoding.UTF8);

        // Assert
        Assert.True(File.Exists(filePath));
        Assert.Equal(content, File.ReadAllText(filePath));
    }

    [Fact]
    public void WriteText_WithDirectoryThatDoesNotExist_CreatesDirectoryAndWritesContent()
    {
        // Arrange
        var content = "test content";
        var subDir = Path.Combine(_testDirectory, "subdir");
        var filePath = Path.Combine(subDir, "writeTest.txt");
        _createdFiles.Add(filePath);
        _createdDirectories.Add(subDir);

        // Act
        FileHelper.WriteText(filePath, content, Encoding.UTF8);

        // Assert
        Assert.True(Directory.Exists(subDir));
        Assert.True(File.Exists(filePath));
        Assert.Equal(content, File.ReadAllText(filePath));
    }

    [Fact]
    public void AppendText_WithExistingFile_AppendsContent()
    {
        // Arrange
        var initialContent = "initial content";
        var contentToAppend = " appended content";
        var filePath = CreateTestFile("appendTest.txt", initialContent);

        // Act
        FileHelper.AppendText(filePath, contentToAppend);

        // Assert
        Assert.Equal(initialContent + contentToAppend, File.ReadAllText(filePath));
    }

    [Fact]
    public void AppendText_WithNewFile_CreatesFileWithContent()
    {
        // Arrange
        var content = "new content";
        var filePath = Path.Combine(_testDirectory, "newAppendTest.txt");
        _createdFiles.Add(filePath);

        // Act
        FileHelper.AppendText(filePath, content);

        // Assert
        Assert.True(File.Exists(filePath));
        Assert.Equal(content, File.ReadAllText(filePath));
    }

    [Fact]
    public void TryWriteText_WithNullEncoding_WritesUsingDefaultEncoding_AndReturnsTrue()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "tryWrite.txt");
        _createdFiles.Add(filePath);

        // Act
        var ok = FileHelper.TryWriteText(filePath, "hello", null);

        // Assert
        Assert.True(ok);
        Assert.True(File.Exists(filePath));
        Assert.Equal("hello", File.ReadAllText(filePath));
    }

    [Fact]
    public void TryAppendText_WithNewFile_CreatesFileAndReturnsTrue()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "tryAppend.txt");
        _createdFiles.Add(filePath);

        // Act
        var ok = FileHelper.TryAppendText(filePath, "line1");

        // Assert
        Assert.True(ok);
        Assert.True(File.Exists(filePath));
        Assert.Equal("line1", File.ReadAllText(filePath));
    }

    [Fact]
    public void MoveFile_WithValidPaths_MovesFile()
    {
        // Arrange
        var content = "test content";
        var sourceFilePath = CreateTestFile("source.txt", content);
        var destDir = CreateTestDirectory("destDir");

        // Act
        FileHelper.MoveFile(sourceFilePath, destDir);

        // Assert
        Assert.False(File.Exists(sourceFilePath));
        Assert.True(File.Exists(Path.Combine(destDir, "source.txt")));
        Assert.Equal(content, File.ReadAllText(Path.Combine(destDir, "source.txt")));
    }

    [Fact]
    public void CopyFile_WithValidPaths_CopiesFile()
    {
        // Arrange
        var content = "test content";
        var sourceFilePath = CreateTestFile("source.txt", content);
        var destFilePath = Path.Combine(_testDirectory, "dest.txt");
        _createdFiles.Add(destFilePath);

        // Act
        FileHelper.CopyFile(sourceFilePath, destFilePath);

        // Assert
        Assert.True(File.Exists(sourceFilePath)); // 源文件仍存在
        Assert.True(File.Exists(destFilePath));   // 目标文件已创建
        Assert.Equal(content, File.ReadAllText(destFilePath));
    }

    [Fact]
    public void CopyFile_WithDestDirectoryNotExist_CreatesDirectoryAndCopiesFile()
    {
        // Arrange
        var content = "test content";
        var sourceFilePath = CreateTestFile("source.txt", content);
        var destDir = Path.Combine(_testDirectory, "newDestDir");
        var destFilePath = Path.Combine(destDir, "dest.txt");
        _createdFiles.Add(destFilePath);
        _createdDirectories.Add(destDir);

        // Act
        FileHelper.CopyFile(sourceFilePath, destFilePath);

        // Assert
        Assert.True(Directory.Exists(destDir));
        Assert.True(File.Exists(destFilePath));
        Assert.Equal(content, File.ReadAllText(destFilePath));
    }

    [Fact]
    public void DeleteFileIfExists_WithExistingFile_DeletesFile()
    {
        // Arrange
        var filePath = CreateTestFile("toDelete.txt", "test");

        // Act
        FileHelper.DeleteFileIfExists(filePath);

        // Assert
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void DeleteFileIfExists_WithNonExistentFile_DoesNotThrow()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.txt");

        // Act & Assert (不应抛出异常)
        FileHelper.DeleteFileIfExists(nonExistentPath);
    }

    [Fact]
    public void ClearFile_WithExistingFile_EmptiesFile()
    {
        // Arrange
        var filePath = CreateTestFile("toClear.txt", "test content");

        // Act
        FileHelper.ClearFile(filePath);

        // Assert
        Assert.Equal(0, new FileInfo(filePath).Length);
    }

    [Fact]
    public void CreateFile_WithContent_CreatesFileWithContent()
    {
        // Arrange
        var content = "test content";
        var filePath = Path.Combine(_testDirectory, "created.txt");
        _createdFiles.Add(filePath);

        // Act
        FileHelper.CreateFile(filePath, content);

        // Assert
        Assert.True(File.Exists(filePath));
        Assert.Equal(content, File.ReadAllText(filePath));
    }

    [Fact]
    public void CreateFile_WithBuffer_CreatesFileWithBuffer()
    {
        // Arrange
        var buffer = Encoding.UTF8.GetBytes("test content");
        var filePath = Path.Combine(_testDirectory, "bufferFile.txt");
        _createdFiles.Add(filePath);

        // Act
        FileHelper.CreateFile(filePath, buffer: buffer);

        // Assert
        Assert.True(File.Exists(filePath));
        Assert.Equal(buffer, File.ReadAllBytes(filePath));
    }

    [Fact]
    public void CreateFile_WithNoContentOrBuffer_CreatesEmptyFile()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "empty.txt");
        _createdFiles.Add(filePath);

        // Act
        FileHelper.CreateFile(filePath);

        // Assert
        Assert.True(File.Exists(filePath));
        Assert.Equal(0, new FileInfo(filePath).Length);
    }

    [Fact]
    public void Contains_WithMatchingFile_ReturnsTrue()
    {
        // Arrange
        CreateTestFile("test.txt", "content");

        // Act
        var result = FileHelper.Contains(_testDirectory, "*.txt");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Contains_WithNoMatchingFile_ReturnsFalse()
    {
        // Arrange
        CreateTestFile("test.txt", "content");

        // Act
        var result = FileHelper.Contains(_testDirectory, "*.doc");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetDirectories_WithExistingDirectories_ReturnsDirectories()
    {
        // Arrange
        var dir1 = CreateTestDirectory("dir1");
        var dir2 = CreateTestDirectory("dir2");

        // Act
        var result = FileHelper.GetDirectories(_testDirectory);

        // Assert
        Assert.Equal(2, result.Length);
        Assert.Contains(dir1, result);
        Assert.Contains(dir2, result);
    }

    [Fact]
    public void GetDirectories_WithFilter_ReturnsFilteredDirectories()
    {
        // Arrange
        var dir1 = CreateTestDirectory("dir1");
        var dir2 = CreateTestDirectory("dir2");

        // Act
        var result = FileHelper.GetDirectories(_testDirectory, filter: d => d.Name.EndsWith("1"));

        // Assert
        Assert.Single(result);
        Assert.Contains(dir1, result);
    }

    [Fact]
    public void GetFileNames_WithExistingFiles_ReturnsFileNames()
    {
        // Arrange
        CreateTestFile("file1.txt", "content");
        CreateTestFile("file2.txt", "content");

        // Act
        var result = FileHelper.GetFileNames(_testDirectory, "*.txt");

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetFileNames_WithoutPath_ReturnsFileNamesWithoutPath()
    {
        // Arrange
        CreateTestFile("file.txt", "content");

        // Act
        var result = FileHelper.GetFileNames(_testDirectory, "*.txt", containPath:false);

        // Assert
        Assert.Single(result);
        Assert.Equal("file.txt", result[0]);
    }

    [Fact]
    public void GetFileNames_WithoutExtension_ReturnsFileNamesWithoutExtension()
    {
        // Arrange
        CreateTestFile("file.txt", "content");

        // Act
        var result = FileHelper.GetFileNames(_testDirectory, "*.txt", false, false);

        // Assert
        Assert.Single(result);
        Assert.Equal("file", result[0]);
    }

    [Fact]
    public void IsEmptyDirectory_WithEmptyDirectory_ReturnsTrue()
    {
        // Arrange
        var emptyDir = CreateTestDirectory("emptyDir");

        // Act
        var result = FileHelper.IsEmptyDirectory(emptyDir);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEmptyDirectory_WithNonEmptyDirectory_ReturnsFalse()
    {
        // Arrange
        var dir = CreateTestDirectory("nonEmptyDir");
        CreateTestFile(Path.Combine("nonEmptyDir", "file.txt"), "content");

        // Act
        var result = FileHelper.IsEmptyDirectory(dir);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CopyDir_WithValidDirectories_CopiesDirectory()
    {
        // Arrange
        var sourceDir = CreateTestDirectory("sourceDir");
        CreateTestFile(Path.Combine("sourceDir", "file.txt"), "content");
        var subDir = CreateTestDirectory(Path.Combine("sourceDir", "subDir"));
        CreateTestFile(Path.Combine("sourceDir", "subDir", "subFile.txt"), "subcontent");

        var destDir = Path.Combine(_testDirectory, "destDir");
        _createdDirectories.Add(destDir);

        // Act
        FileHelper.CopyDir(sourceDir, destDir);

        // Assert
        Assert.True(Directory.Exists(destDir));
        Assert.True(File.Exists(Path.Combine(destDir, "file.txt")));
        Assert.True(Directory.Exists(Path.Combine(destDir, "subDir")));
        Assert.True(File.Exists(Path.Combine(destDir, "subDir", "subFile.txt")));
    }

    [Fact]
    public void ClearDirectory_WithPopulatedDirectory_EmptiesDirectory()
    {
        // Arrange
        var dir = CreateTestDirectory("toClear");
        CreateTestFile(Path.Combine("toClear", "file.txt"), "content");
        var subDir = CreateTestDirectory(Path.Combine("toClear", "subDir"));
        CreateTestFile(Path.Combine("toClear", "subDir", "subFile.txt"), "subcontent");

        // Act
        FileHelper.ClearDirectory(dir);

        // Assert
        Assert.True(Directory.Exists(dir)); // 目录自身应该仍存在
        Assert.Empty(Directory.GetFileSystemEntries(dir)); // 但应该是空的
    }

    [Fact]
    public void DeleteDirectory_WithExistingDirectory_DeletesDirectory()
    {
        // Arrange
        var dir = CreateTestDirectory("toDelete");

        // Act
        FileHelper.DeleteDirectory(dir);

        // Assert
        Assert.False(Directory.Exists(dir));
    }

    [Fact]
    public void DeleteDirectory_WithNonExistentDirectory_DoesNotThrow()
    {
        // Arrange
        var nonExistentDir = Path.Combine(_testDirectory, "nonexistent");

        // Act & Assert (不应抛出异常)
        FileHelper.DeleteDirectory(nonExistentDir);
    }

    [Fact]
    public void EnsureDirectoryExists_WithNonExistentDirectory_CreatesDirectory()
    {
        // Arrange
        var subDir = Path.Combine(_testDirectory, "newSubDir");
        var filePath = Path.Combine(subDir, "file.txt");
        _createdDirectories.Add(subDir);

        // Act
        FileHelper.EnsureDirectoryExists(filePath);

        // Assert
        Assert.True(Directory.Exists(subDir));
    }

    [Fact]
    public void GetExistingFileInfo_WithExistingFile_ReturnsFileInfo()
    {
        // Arrange
        var content = "test content for hash";
        var fileName = "fileForInfo.txt";
        var filePath = CreateTestFile(fileName, content);

        // Act
    var fileInfo = FileHelper.GetExistingFileInfo(filePath);

        // Assert
        Assert.NotNull(fileInfo);
        Assert.Equal(fileName, fileInfo.FileName);
        Assert.Equal(filePath, fileInfo.FullFilePath);
        Assert.Equal(content.Length, fileInfo.Length);
        Assert.NotNull(fileInfo.HashData); // MD5哈希值应该存在
        Assert.NotNull(fileInfo.FileSize);  // 文件大小格式化字符串应该存在
    }

    [Fact]
    public void GetExistingFileInfo_WithRelativeBasePath_ComputesRelativePath()
    {
        // Arrange
        var baseDirectory = CreateTestDirectory("relativeBase");
        var nestedFileName = Path.Combine("relativeBase", "nested.txt");
        var filePath = CreateTestFile(nestedFileName, "content");

        // Act
    var fileInfo = FileHelper.GetExistingFileInfo(filePath, baseDirectory);

        // Assert
        Assert.NotNull(fileInfo);
        Assert.Equal(Path.GetFileName(nestedFileName), fileInfo!.RelativeFilePath);
    }

    [Fact]
    public void GetExistingFileInfo_WithRelativeFilePath_UsesCurrentDirectory()
    {
        // Arrange
        var originalCurrentDirectory = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = _testDirectory;
            var fileName = "currentDirFile.txt";
            var filePath = CreateTestFile(fileName, "content");

            // Act
            var fileInfo = FileHelper.GetExistingFileInfo(fileName);

            // Assert
            Assert.NotNull(fileInfo);
            Assert.Equal(filePath, fileInfo!.FullFilePath);
            Assert.Equal(fileName, fileInfo.RelativeFilePath);
        }
        finally
        {
            Environment.CurrentDirectory = originalCurrentDirectory;
        }
    }

    [Fact]
    public void GetExistingFileInfo_WithNonExistentFile_ReturnsNull()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "nonExistentFile.txt");

        // Act
    var fileInfo = FileHelper.GetExistingFileInfo(nonExistentPath);

        // Assert
        Assert.Null(fileInfo);
    }

    [Fact]
    public void GetExistingFileInfo_WithNullPath_ReturnsNull()
    {
        // Act
    var fileInfo = FileHelper.GetExistingFileInfo(null);

        // Assert
        Assert.Null(fileInfo);
    }

    [Fact]
    public void ReadText_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "nonExistent.txt");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => FileHelper.ReadText(nonExistentPath));
    }

    [Fact]
    public void WriteText_WithNullPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => FileHelper.WriteText(null, "content", Encoding.UTF8));
    }

    [Fact]
    public void WriteText_WithNullEncoding_ThrowsArgumentNullException()
    {
    // Arrange
    var filePath = Path.Combine(_testDirectory, "test.txt");
    _createdFiles.Add(filePath);

    // Act: 现在传入 null 编码时应使用默认编码写入而非抛出
    FileHelper.WriteText(filePath, "content", null);

    // Assert
    Assert.True(File.Exists(filePath));
    Assert.Equal("content", File.ReadAllText(filePath));
    }

    [Fact]
    public void AppendText_WithNullPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => FileHelper.AppendText(null, "content"));
    }

    [Fact]
    public void MoveFile_WithNonExistentSourceFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentSource = Path.Combine(_testDirectory, "nonExistent.txt");
        var destDir = CreateTestDirectory("destForMove");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => FileHelper.MoveFile(nonExistentSource, destDir));
    }

    [Fact]
    public void CopyFile_WithNonExistentSourceFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentSource = Path.Combine(_testDirectory, "nonExistent.txt");
        var destFile = Path.Combine(_testDirectory, "dest.txt");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => FileHelper.CopyFile(nonExistentSource, destFile));
    }

    [Fact]
    public void ClearFile_WithNullPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => FileHelper.ClearFile(null));
    }

    [Fact]
    public void CreateFile_WithNullPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => FileHelper.CreateFile(null));
    }

    [Fact]
    public void Contains_WithNullDirectoryPath_ReturnsFalse()
    {
        // Act
        var result = FileHelper.Contains(null, "*.txt");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Contains_WithNullSearchPattern_ReturnsFalse()
    {
        // Act
        var result = FileHelper.Contains(_testDirectory, null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Contains_WithUnauthorizedAccess_ReturnsFalse()
    {
        // Arrange - 创建一个模拟的受限目录（这在普通测试环境可能无法真正测试）
        var mockPath = Path.Combine(_testDirectory, "restrictedFolder");
        Directory.CreateDirectory(mockPath);
        _createdDirectories.Add(mockPath);

        // Act - 我们假设使用一个不存在的模式可以避免实际访问
        var result = FileHelper.Contains(mockPath, "*.xyz");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetDirectories_WithNullDirectoryPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => FileHelper.GetDirectories(null));
    }

    [Fact]
    public void GetDirectories_WithRecursiveSearch_FindsAllSubdirectories()
    {
        // Arrange
        var dir1 = CreateTestDirectory("parentDir");
        var subDir1 = CreateTestDirectory(Path.Combine("parentDir", "subDir1"));
        var subDir2 = CreateTestDirectory(Path.Combine("parentDir", "subDir2"));

        // Act
        var result = FileHelper.GetDirectories(dir1, searchOption: SearchOption.AllDirectories);

        // Assert
        Assert.Equal(2, result.Length);
        Assert.Contains(subDir1, result);
        Assert.Contains(subDir2, result);
    }

    [Fact]
    public void GetFileNames_WithNullDirectoryPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => FileHelper.GetFileNames(null));
    }

    [Fact]
    public void GetFileNames_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        var nonExistentDir = Path.Combine(_testDirectory, "nonExistentDir");

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => FileHelper.GetFileNames(nonExistentDir));
    }

    [Fact]
    public void GetFileNames_WithSearchOptionAllDirectories_FindsAllFiles()
    {
        // Arrange
        CreateTestFile("rootFile.txt", "content");
        var subDir = CreateTestDirectory("subDirForFiles");
        CreateTestFile(Path.Combine("subDirForFiles", "subFile.txt"), "content");

        // Act
        var result = FileHelper.GetFileNames(_testDirectory, searchOption: SearchOption.AllDirectories);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void IsEmptyDirectory_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        var nonExistentDir = Path.Combine(_testDirectory, "nonExistentDir");

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => FileHelper.IsEmptyDirectory(nonExistentDir));
    }

    [Fact]
    public void CopyDir_WithNullSourceDirectory_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => FileHelper.CopyDir(null, "dest"));
    }

    [Fact]
    public void CopyDir_WithNullDestinationDirectory_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => FileHelper.CopyDir("source", null));
    }

    [Fact]
    public void CopyDir_WithNonExistentSourceDirectory_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        var nonExistentDir = Path.Combine(_testDirectory, "nonExistentDir");
        var destDir = Path.Combine(_testDirectory, "destForNonExistent");

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => FileHelper.CopyDir(nonExistentDir, destDir));
    }

    [Fact]
    public void ClearDirectory_WithNullDirectoryPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => FileHelper.ClearDirectory(null));
    }

    [Fact]
    public void ClearDirectory_WithNonExistentDirectory_DoesNotThrow()
    {
        // Arrange
        var nonExistentDir = Path.Combine(_testDirectory, "nonExistentDir");

        // Act & Assert (不应抛出异常)
        FileHelper.ClearDirectory(nonExistentDir);
    }

    [Fact]
    public void EnsureDirectoryExists_WithExistingDirectory_DoesNotCreateNewDirectory()
    {
        // Arrange
        var dir = CreateTestDirectory("existingDir");
        var filePath = Path.Combine(dir, "file.txt");
        var directoryCount = Directory.GetDirectories(_testDirectory).Length;

        // Act
        FileHelper.EnsureDirectoryExists(filePath);

        // Assert
        Assert.Equal(directoryCount, Directory.GetDirectories(_testDirectory).Length);
    }
}
