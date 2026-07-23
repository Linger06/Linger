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
        _testDirectory = Path.Combine(Path.GetTempPath(), $"LingerTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _createdDirectories.Add(_testDirectory);
    }

    public void Dispose()
    {
        try
        {
            foreach (var file in _createdFiles)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }

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
        var content = "test content";
        var filePath = CreateTestFile("test.txt", content);

        var result = File.ReadAllText(filePath, Encoding.UTF8);

        Assert.Equal(content, result);
    }

    [Fact]
    public void ReadText_WithExistingFileAndEncoding_ReturnsContent()
    {
        var content = "UTF-8 test content";
        var filePath = CreateTestFile("test.txt", content);

        var result = File.ReadAllText(filePath, Encoding.UTF8);

        Assert.Equal(content, result);
    }

    [Fact]
    public void TryReadText_WithExistingFile_ReturnsTrueAndContent()
    {
        var content = "test content";
        var filePath = CreateTestFile("test.txt", content);

        var success = FileHelper.TryReadText(filePath, out var result);

        Assert.True(success);
        Assert.Equal(content, result);
    }

    [Fact]
    public void TryReadText_WithNonExistentFile_ReturnsFalseAndNull()
    {
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.txt");

        var success = FileHelper.TryReadText(nonExistentPath, out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void WriteText_WithValidPath_WritesContent()
    {
        var content = "test content";
        var filePath = Path.Combine(_testDirectory, "writeTest.txt");
        _createdFiles.Add(filePath);

        File.WriteAllText(filePath, content, Encoding.UTF8);

        Assert.True(File.Exists(filePath));
        Assert.Equal(content, File.ReadAllText(filePath));
    }

    [Fact]
    public void WriteText_WithDirectoryThatDoesNotExist_CreatesDirectoryAndWritesContent()
    {
        var content = "test content";
        var subDir = Path.Combine(_testDirectory, "subdir");
        var filePath = Path.Combine(subDir, "writeTest.txt");
        _createdFiles.Add(filePath);
        _createdDirectories.Add(subDir);

        Directory.CreateDirectory(subDir);
        File.WriteAllText(filePath, content, Encoding.UTF8);

        Assert.True(Directory.Exists(subDir));
        Assert.True(File.Exists(filePath));
        Assert.Equal(content, File.ReadAllText(filePath));
    }

    [Fact]
    public void AppendText_WithExistingFile_AppendsContent()
    {
        var initialContent = "initial content";
        var contentToAppend = " appended content";
        var filePath = CreateTestFile("appendTest.txt", initialContent);

        File.AppendAllText(filePath, contentToAppend);

        Assert.Equal(initialContent + contentToAppend, File.ReadAllText(filePath));
    }

    [Fact]
    public void AppendText_WithNewFile_CreatesFileWithContent()
    {
        var content = "new content";
        var filePath = Path.Combine(_testDirectory, "newAppendTest.txt");
        _createdFiles.Add(filePath);

        File.AppendAllText(filePath, content);

        Assert.True(File.Exists(filePath));
        Assert.Equal(content, File.ReadAllText(filePath));
    }

    [Fact]
    public void MoveFile_WithValidPaths_MovesFile()
    {
        var content = "test content";
        var sourceFilePath = CreateTestFile("source.txt", content);
        var destDir = CreateTestDirectory("destDir");

        FileHelper.MoveFile(sourceFilePath, destDir);

        Assert.False(File.Exists(sourceFilePath));
        Assert.True(File.Exists(Path.Combine(destDir, "source.txt")));
        Assert.Equal(content, File.ReadAllText(Path.Combine(destDir, "source.txt")));
    }

    [Fact]
    public void DeleteFolderFiles_ShouldBeIdempotentWhenTargetFileIsAlreadyMissing()
    {
        var sourceDir = CreateTestDirectory("source");
        var targetDir = CreateTestDirectory("target");
        var sourceFile = Path.Combine(sourceDir, "shared.txt");
        var targetFile = Path.Combine(targetDir, "shared.txt");

        File.WriteAllText(sourceFile, "source");
        File.WriteAllText(targetFile, "target");
        _createdFiles.Add(sourceFile);
        _createdFiles.Add(targetFile);

        FileHelper.DeleteFolderFiles(sourceDir, targetDir);
        Assert.False(File.Exists(targetFile));

        FileHelper.DeleteFolderFiles(sourceDir, targetDir);
        Assert.False(File.Exists(targetFile));
    }

    [Fact]
    public void CopyFile_WithValidPaths_CopiesFile()
    {
        var content = "test content";
        var sourceFilePath = CreateTestFile("source.txt", content);
        var destFilePath = Path.Combine(_testDirectory, "dest.txt");
        _createdFiles.Add(destFilePath);

        FileHelper.CopyFile(sourceFilePath, destFilePath);

        Assert.True(File.Exists(sourceFilePath));
        Assert.True(File.Exists(destFilePath));
        Assert.Equal(content, File.ReadAllText(destFilePath));
    }

    [Fact]
    public void CopyFile_WithDestDirectoryNotExist_CreatesDirectoryAndCopiesFile()
    {
        var content = "test content";
        var sourceFilePath = CreateTestFile("source.txt", content);
        var destDir = Path.Combine(_testDirectory, "newDestDir");
        var destFilePath = Path.Combine(destDir, "dest.txt");
        _createdFiles.Add(destFilePath);
        _createdDirectories.Add(destDir);

        FileHelper.CopyFile(sourceFilePath, destFilePath);

        Assert.True(Directory.Exists(destDir));
        Assert.True(File.Exists(destFilePath));
        Assert.Equal(content, File.ReadAllText(destFilePath));
    }

    [Fact]
    public void DeleteFileIfExists_WithExistingFile_DeletesFile()
    {
        var filePath = CreateTestFile("toDelete.txt", "test");

        FileHelper.DeleteFileIfExists(filePath);

        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void DeleteFileIfExists_WithNonExistentFile_DoesNotThrow()
    {
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.txt");

        FileHelper.DeleteFileIfExists(nonExistentPath);
    }

    [Fact]
    public void ClearFile_WithExistingFile_EmptiesFile()
    {
        var filePath = CreateTestFile("toClear.txt", "test content");

        FileHelper.ClearFile(filePath);

        Assert.Equal(0, new FileInfo(filePath).Length);
    }

    [Fact]
    public void CreateFile_WithContent_CreatesFileWithContent()
    {
        var content = "test content";
        var filePath = Path.Combine(_testDirectory, "created.txt");
        _createdFiles.Add(filePath);

        FileHelper.CreateFile(filePath, content);

        Assert.True(File.Exists(filePath));
        Assert.Equal(content, File.ReadAllText(filePath));
    }

    [Fact]
    public void CreateFile_WithBuffer_CreatesFileWithBuffer()
    {
        var buffer = Encoding.UTF8.GetBytes("test content");
        var filePath = Path.Combine(_testDirectory, "bufferFile.txt");
        _createdFiles.Add(filePath);

        FileHelper.CreateFile(filePath, buffer: buffer);

        Assert.True(File.Exists(filePath));
        Assert.Equal(buffer, File.ReadAllBytes(filePath));
    }

    [Fact]
    public void CreateFile_WithNoContentOrBuffer_CreatesEmptyFile()
    {
        var filePath = Path.Combine(_testDirectory, "empty.txt");
        _createdFiles.Add(filePath);

        FileHelper.CreateFile(filePath);

        Assert.True(File.Exists(filePath));
        Assert.Equal(0, new FileInfo(filePath).Length);
    }

    [Fact]
    public void Contains_WithMatchingFile_ReturnsTrue()
    {
        CreateTestFile("test.txt", "content");

        var result = FileHelper.Contains(_testDirectory, "*.txt");

        Assert.True(result);
    }

    [Fact]
    public void Contains_WithNoMatchingFile_ReturnsFalse()
    {
        CreateTestFile("test.txt", "content");

        var result = FileHelper.Contains(_testDirectory, "*.doc");

        Assert.False(result);
    }

    [Fact]
    public void GetDirectories_WithExistingDirectories_ReturnsDirectories()
    {
        var dir1 = CreateTestDirectory("dir1");
        var dir2 = CreateTestDirectory("dir2");

        var result = FileHelper.GetDirectories(_testDirectory);

        Assert.Equal(2, result.Length);
        Assert.Contains(dir1, result);
        Assert.Contains(dir2, result);
    }

    [Fact]
    public void GetDirectories_WithFilter_ReturnsFilteredDirectories()
    {
        var dir1 = CreateTestDirectory("dir1");
        var dir2 = CreateTestDirectory("dir2");

        var result = FileHelper.GetDirectories(_testDirectory, filter: d => d.Name.EndsWith("1"));

        Assert.Single(result);
        Assert.Contains(dir1, result);
    }

    [Fact]
    public void GetFileNames_WithExistingFiles_ReturnsFileNames()
    {
        CreateTestFile("file1.txt", "content");
        CreateTestFile("file2.txt", "content");

        var result = FileHelper.GetFileNames(_testDirectory, "*.txt");

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetFileNames_WithoutPath_ReturnsFileNamesWithoutPath()
    {
        CreateTestFile("file.txt", "content");

        var result = FileHelper.GetFileNames(_testDirectory, "*.txt", containPath: false);

        Assert.Single(result);
        Assert.Equal("file.txt", result[0]);
    }

    [Fact]
    public void GetFileNames_WithoutExtension_ReturnsFileNamesWithoutExtension()
    {
        CreateTestFile("file.txt", "content");

        var result = FileHelper.GetFileNames(_testDirectory, "*.txt", false, false);

        Assert.Single(result);
        Assert.Equal("file", result[0]);
    }

    [Fact]
    public void IsEmptyDirectory_WithEmptyDirectory_ReturnsTrue()
    {
        var emptyDir = CreateTestDirectory("emptyDir");

        var result = FileHelper.IsEmptyDirectory(emptyDir);

        Assert.True(result);
    }

    [Fact]
    public void IsEmptyDirectory_WithNonEmptyDirectory_ReturnsFalse()
    {
        var dir = CreateTestDirectory("nonEmptyDir");
        CreateTestFile(Path.Combine("nonEmptyDir", "file.txt"), "content");

        var result = FileHelper.IsEmptyDirectory(dir);

        Assert.False(result);
    }

    [Fact]
    public void CopyDir_WithValidDirectories_CopiesDirectory()
    {
        var sourceDir = CreateTestDirectory("sourceDir");
        CreateTestFile(Path.Combine("sourceDir", "file.txt"), "content");
        var subDir = CreateTestDirectory(Path.Combine("sourceDir", "subDir"));
        CreateTestFile(Path.Combine("sourceDir", "subDir", "subFile.txt"), "subcontent");

        var destDir = Path.Combine(_testDirectory, "destDir");
        _createdDirectories.Add(destDir);

        FileHelper.CopyDir(sourceDir, destDir);

        Assert.True(Directory.Exists(destDir));
        Assert.True(File.Exists(Path.Combine(destDir, "file.txt")));
        Assert.True(Directory.Exists(Path.Combine(destDir, "subDir")));
        Assert.True(File.Exists(Path.Combine(destDir, "subDir", "subFile.txt")));
    }

    [Fact]
    public void ClearDirectory_WithPopulatedDirectory_EmptiesDirectory()
    {
        var dir = CreateTestDirectory("toClear");
        CreateTestFile(Path.Combine("toClear", "file.txt"), "content");
        var subDir = CreateTestDirectory(Path.Combine("toClear", "subDir"));
        CreateTestFile(Path.Combine("toClear", "subDir", "subFile.txt"), "subcontent");

        FileHelper.ClearDirectory(dir);

        Assert.True(Directory.Exists(dir));
        Assert.Empty(Directory.GetFileSystemEntries(dir));
    }

    [Fact]
    public void DeleteDirectory_WithExistingDirectory_DeletesDirectory()
    {
        var dir = CreateTestDirectory("toDelete");

        FileHelper.DeleteDirectory(dir);

        Assert.False(Directory.Exists(dir));
    }

    [Fact]
    public void DeleteDirectory_WithNonExistentDirectory_DoesNotThrow()
    {
        var nonExistentDir = Path.Combine(_testDirectory, "nonexistent");

        FileHelper.DeleteDirectory(nonExistentDir);
    }

    [Fact]
    public void GetExistingFileInfo_WithExistingFile_ReturnsFileInfo()
    {
        var content = "test content for hash";
        var fileName = "fileForInfo.txt";
        var filePath = CreateTestFile(fileName, content);

        var fileInfo = FileHelper.GetExistingFileInfo(filePath);

        Assert.NotNull(fileInfo);
        Assert.Equal(fileName, fileInfo.FileName);
        Assert.Equal(filePath, fileInfo.FullFilePath);
        Assert.Equal(content.Length, fileInfo.Length);
        Assert.NotNull(fileInfo.HashData);
        Assert.NotNull(fileInfo.FileSize);
    }

    [Fact]
    public void GetExistingFileInfo_WithRelativeBasePath_ComputesRelativePath()
    {
        var baseDirectory = CreateTestDirectory("relativeBase");
        var nestedFileName = Path.Combine("relativeBase", "nested.txt");
        var filePath = CreateTestFile(nestedFileName, "content");

        var fileInfo = FileHelper.GetExistingFileInfo(filePath, baseDirectory);

        Assert.NotNull(fileInfo);
        Assert.Equal(Path.GetFileName(nestedFileName), fileInfo!.RelativeFilePath);
    }

    [Fact]
    public void GetExistingFileInfo_WithRelativeFilePath_UsesCurrentDirectory()
    {
        var originalCurrentDirectory = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = _testDirectory;
            var fileName = "currentDirFile.txt";
            var filePath = CreateTestFile(fileName, "content");

            var fileInfo = FileHelper.GetExistingFileInfo(fileName);

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
        var nonExistentPath = Path.Combine(_testDirectory, "nonExistentFile.txt");

        var fileInfo = FileHelper.GetExistingFileInfo(nonExistentPath);

        Assert.Null(fileInfo);
    }

    [Fact]
    public void GetExistingFileInfo_WithNullPath_ReturnsNull()
    {
        var fileInfo = FileHelper.GetExistingFileInfo(null);

        Assert.Null(fileInfo);
    }

    [Fact]
    public void ReadText_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        var nonExistentPath = Path.Combine(_testDirectory, "nonExistent.txt");

        Assert.Throws<FileNotFoundException>(() => File.ReadAllText(nonExistentPath, Encoding.UTF8));
    }

    [Fact]
    public void WriteText_WithNullPath_ThrowsArgumentNullException()
    {
        Assert.Throws<System.ArgumentNullException>(() => File.WriteAllText(null!, "content", Encoding.UTF8));
    }

    [Fact]
    public void WriteText_WithNullEncoding_ThrowsArgumentNullException()
    {
        var filePath = Path.Combine(_testDirectory, "test.txt");
        _createdFiles.Add(filePath);

        FileHelper.CreateFile(filePath, content: "content", encoding: null);

        Assert.True(File.Exists(filePath));
        Assert.Equal("content", File.ReadAllText(filePath));
    }

    [Fact]
    public void AppendText_WithNullPath_ThrowsArgumentNullException()
    {
        Assert.Throws<System.ArgumentNullException>(() => File.AppendAllText(null!, "content"));
    }

    [Fact]
    public void MoveFile_WithNonExistentSourceFile_ThrowsFileNotFoundException()
    {
        var nonExistentSource = Path.Combine(_testDirectory, "nonExistent.txt");
        var destDir = CreateTestDirectory("destForMove");

        Assert.Throws<FileNotFoundException>(() => FileHelper.MoveFile(nonExistentSource, destDir));
    }

    [Fact]
    public void CopyFile_WithNonExistentSourceFile_ThrowsFileNotFoundException()
    {
        var nonExistentSource = Path.Combine(_testDirectory, "nonExistent.txt");
        var destFile = Path.Combine(_testDirectory, "dest.txt");

        Assert.Throws<FileNotFoundException>(() => FileHelper.CopyFile(nonExistentSource, destFile));
    }

    [Fact]
    public void ClearFile_WithNullPath_ThrowsArgumentNullException()
    {
        Assert.Throws<System.ArgumentNullException>(() => FileHelper.ClearFile(null));
    }

    [Fact]
    public void CreateFile_WithNullPath_ThrowsArgumentNullException()
    {
        Assert.Throws<System.ArgumentNullException>(() => FileHelper.CreateFile(null));
    }

    [Fact]
    public void Contains_WithNullDirectoryPath_ReturnsFalse()
    {
        var result = FileHelper.Contains(null, "*.txt");

        Assert.False(result);
    }

    [Fact]
    public void Contains_WithNullSearchPattern_ReturnsFalse()
    {
        var result = FileHelper.Contains(_testDirectory, null);

        Assert.False(result);
    }

    [Fact]
    public void Contains_WithUnauthorizedAccess_ReturnsFalse()
    {
        var mockPath = Path.Combine(_testDirectory, "restrictedFolder");
        Directory.CreateDirectory(mockPath);
        _createdDirectories.Add(mockPath);

        var result = FileHelper.Contains(mockPath, "*.xyz");

        Assert.False(result);
    }

    [Fact]
    public void Contains_WithInvalidDirectoryPath_ReturnsFalse()
    {
        var result = FileHelper.Contains("bad\0path", "*.txt");

        Assert.False(result);
    }

    [Fact]
    public void Contains_WithInvalidSearchPattern_ReturnsFalse()
    {
        var result = FileHelper.Contains(_testDirectory, "bad\0pattern");

        Assert.False(result);
    }

    [Fact]
    public void GetDirectories_WithNullDirectoryPath_ThrowsArgumentNullException()
    {
        Assert.Throws<System.ArgumentNullException>(() => FileHelper.GetDirectories(null));
    }

    [Fact]
    public void GetDirectories_WithRecursiveSearch_FindsAllSubdirectories()
    {
        var dir1 = CreateTestDirectory("parentDir");
        var subDir1 = CreateTestDirectory(Path.Combine("parentDir", "subDir1"));
        var subDir2 = CreateTestDirectory(Path.Combine("parentDir", "subDir2"));

        var result = FileHelper.GetDirectories(dir1, searchOption: SearchOption.AllDirectories);

        Assert.Equal(2, result.Length);
        Assert.Contains(subDir1, result);
        Assert.Contains(subDir2, result);
    }

    [Fact]
    public void GetFileNames_WithNullDirectoryPath_ThrowsArgumentNullException()
    {
        Assert.Throws<System.ArgumentNullException>(() => FileHelper.GetFileNames(null));
    }

    [Fact]
    public void GetFileNames_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
    {
        var nonExistentDir = Path.Combine(_testDirectory, "nonExistentDir");

        Assert.Throws<DirectoryNotFoundException>(() => FileHelper.GetFileNames(nonExistentDir));
    }

    [Fact]
    public void GetFileNames_WithSearchOptionAllDirectories_FindsAllFiles()
    {
        CreateTestFile("rootFile.txt", "content");
        var subDir = CreateTestDirectory("subDirForFiles");
        CreateTestFile(Path.Combine("subDirForFiles", "subFile.txt"), "content");

        var result = FileHelper.GetFileNames(_testDirectory, searchOption: SearchOption.AllDirectories);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void IsEmptyDirectory_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
    {
        var nonExistentDir = Path.Combine(_testDirectory, "nonExistentDir");

        Assert.Throws<DirectoryNotFoundException>(() => FileHelper.IsEmptyDirectory(nonExistentDir));
    }

    [Fact]
    public void CopyDir_WithNullSourceDirectory_ThrowsArgumentNullException()
    {
        Assert.Throws<System.ArgumentNullException>(() => FileHelper.CopyDir(null, "dest"));
    }

    [Fact]
    public void CopyDir_WithNullDestinationDirectory_ThrowsArgumentNullException()
    {
        Assert.Throws<System.ArgumentNullException>(() => FileHelper.CopyDir("source", null));
    }

    [Fact]
    public void CopyDir_WithNonExistentSourceDirectory_ThrowsDirectoryNotFoundException()
    {
        var nonExistentDir = Path.Combine(_testDirectory, "nonExistentDir");
        var destDir = Path.Combine(_testDirectory, "destForNonExistent");

        Assert.Throws<DirectoryNotFoundException>(() => FileHelper.CopyDir(nonExistentDir, destDir));
    }

    [Fact]
    public void ClearDirectory_WithNullDirectoryPath_ThrowsArgumentNullException()
    {
        Assert.Throws<System.ArgumentNullException>(() => FileHelper.ClearDirectory(null));
    }

    [Fact]
    public void ClearDirectory_WithNonExistentDirectory_DoesNotThrow()
    {
        var nonExistentDir = Path.Combine(_testDirectory, "nonExistentDir");

        FileHelper.ClearDirectory(nonExistentDir);
    }

}
