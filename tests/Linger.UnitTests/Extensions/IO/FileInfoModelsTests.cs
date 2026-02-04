using Linger.Extensions.IO;

namespace Linger.UnitTests.Extensions.IO;

public class FileInfoModelsTests
{
    #region BaseFileInfo Tests

    [Fact]
    public void BaseFileInfo_DefaultValues_AreNull()
    {
        // Arrange & Act
        var fileInfo = new BaseFileInfo();

        // Assert
        Assert.Null(fileInfo.HashData);
        Assert.Null(fileInfo.FileName);
        Assert.Null(fileInfo.FullFilePath);
        Assert.Null(fileInfo.FileSize);
        Assert.Equal(0, fileInfo.Length);
    }

    [Fact]
    public void BaseFileInfo_CanSetAndGetHashData()
    {
        // Arrange
        var fileInfo = new BaseFileInfo();
        var expectedHash = "abc123def456";

        // Act
        fileInfo.HashData = expectedHash;

        // Assert
        Assert.Equal(expectedHash, fileInfo.HashData);
    }

    [Fact]
    public void BaseFileInfo_CanSetAndGetFileName()
    {
        // Arrange
        var fileInfo = new BaseFileInfo();
        var expectedName = "test.txt";

        // Act
        fileInfo.FileName = expectedName;

        // Assert
        Assert.Equal(expectedName, fileInfo.FileName);
    }

    [Fact]
    public void BaseFileInfo_CanSetAndGetFullFilePath()
    {
        // Arrange
        var fileInfo = new BaseFileInfo();
        var expectedPath = @"C:\Users\Test\Documents\test.txt";

        // Act
        fileInfo.FullFilePath = expectedPath;

        // Assert
        Assert.Equal(expectedPath, fileInfo.FullFilePath);
    }

    [Fact]
    public void BaseFileInfo_CanSetAndGetFileSize()
    {
        // Arrange
        var fileInfo = new BaseFileInfo();
        var expectedSize = "1.5MB";

        // Act
        fileInfo.FileSize = expectedSize;

        // Assert
        Assert.Equal(expectedSize, fileInfo.FileSize);
    }

    [Fact]
    public void BaseFileInfo_CanSetAndGetLength()
    {
        // Arrange
        var fileInfo = new BaseFileInfo();
        var expectedLength = 1572864L; // 1.5MB in bytes

        // Act
        fileInfo.Length = expectedLength;

        // Assert
        Assert.Equal(expectedLength, fileInfo.Length);
    }

    [Fact]
    public void BaseFileInfo_CanSetAllProperties()
    {
        // Arrange & Act
        var fileInfo = new BaseFileInfo
        {
            HashData = "hash123",
            FileName = "document.pdf",
            FullFilePath = @"C:\Documents\document.pdf",
            FileSize = "2.3MB",
            Length = 2411724
        };

        // Assert
        Assert.Equal("hash123", fileInfo.HashData);
        Assert.Equal("document.pdf", fileInfo.FileName);
        Assert.Equal(@"C:\Documents\document.pdf", fileInfo.FullFilePath);
        Assert.Equal("2.3MB", fileInfo.FileSize);
        Assert.Equal(2411724, fileInfo.Length);
    }

    #endregion

    #region ExtendedFileInfo Tests

    [Fact]
    public void ExtendedFileInfo_DefaultValues_AreNull()
    {
        // Arrange & Act
        var fileInfo = new ExtendedFileInfo();

        // Assert
        Assert.Null(fileInfo.RelativeFilePath);
        Assert.Null(fileInfo.HashData);
        Assert.Null(fileInfo.FileName);
        Assert.Null(fileInfo.FullFilePath);
        Assert.Null(fileInfo.FileSize);
        Assert.Equal(0, fileInfo.Length);
    }

    [Fact]
    public void ExtendedFileInfo_CanSetAndGetRelativeFilePath()
    {
        // Arrange
        var fileInfo = new ExtendedFileInfo();
        var expectedPath = @"Documents\test.txt";

        // Act
        fileInfo.RelativeFilePath = expectedPath;

        // Assert
        Assert.Equal(expectedPath, fileInfo.RelativeFilePath);
    }

    [Fact]
    public void ExtendedFileInfo_InheritsFromBaseFileInfo()
    {
        // Arrange & Act
        var fileInfo = new ExtendedFileInfo();

        // Assert
        Assert.IsAssignableFrom<BaseFileInfo>(fileInfo);
    }

    [Fact]
    public void ExtendedFileInfo_CanSetAllProperties()
    {
        // Arrange & Act
        var fileInfo = new ExtendedFileInfo
        {
            HashData = "hash456",
            FileName = "image.png",
            FullFilePath = @"C:\Images\image.png",
            FileSize = "512KB",
            Length = 524288,
            RelativeFilePath = @"Images\image.png"
        };

        // Assert
        Assert.Equal("hash456", fileInfo.HashData);
        Assert.Equal("image.png", fileInfo.FileName);
        Assert.Equal(@"C:\Images\image.png", fileInfo.FullFilePath);
        Assert.Equal("512KB", fileInfo.FileSize);
        Assert.Equal(524288, fileInfo.Length);
        Assert.Equal(@"Images\image.png", fileInfo.RelativeFilePath);
    }

    [Fact]
    public void ExtendedFileInfo_CanBeUsedAsBaseFileInfo()
    {
        // Arrange
        var extendedInfo = new ExtendedFileInfo
        {
            FileName = "test.txt",
            Length = 1024
        };

        // Act
        BaseFileInfo baseInfo = extendedInfo;

        // Assert
        Assert.Equal("test.txt", baseInfo.FileName);
        Assert.Equal(1024, baseInfo.Length);
    }

    #endregion
}
