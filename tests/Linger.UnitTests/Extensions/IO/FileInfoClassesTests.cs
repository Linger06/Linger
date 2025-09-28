using Linger.Extensions.IO;

namespace Linger.UnitTests.Extensions.IO;

public class FileInfoClassesTests
{
    #region BaseFileInfo Tests

    [Fact]
    public void BaseFileInfo_Constructor_ShouldInitializeWithDefaults()
    {
        // Act
        var fileInfo = new BaseFileInfo();

        // Assert
        Assert.NotNull(fileInfo);
        Assert.Null(fileInfo.HashData);
        Assert.Null(fileInfo.FileName);
        Assert.Null(fileInfo.FullFilePath);
        Assert.Null(fileInfo.FileSize);
        Assert.Equal(0, fileInfo.Length);
    }

    [Fact]
    public void BaseFileInfo_Properties_ShouldGetAndSetCorrectly()
    {
        // Arrange
        var fileInfo = new BaseFileInfo();
        const string hashData = "ABC123";
        const string fileName = "test.txt";
        const string fullFilePath = @"C:\temp\test.txt";
        const string fileSize = "1024 KB";
        const long length = 1048576;

        // Act
        fileInfo.HashData = hashData;
        fileInfo.FileName = fileName;
        fileInfo.FullFilePath = fullFilePath;
        fileInfo.FileSize = fileSize;
        fileInfo.Length = length;

        // Assert
        Assert.Equal(hashData, fileInfo.HashData);
        Assert.Equal(fileName, fileInfo.FileName);
        Assert.Equal(fullFilePath, fileInfo.FullFilePath);
        Assert.Equal(fileSize, fileInfo.FileSize);
        Assert.Equal(length, fileInfo.Length);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("valid_hash")]
    [InlineData("MD5:ABC123")]
    [InlineData("SHA256:DEF456")]
    public void BaseFileInfo_HashData_ShouldAcceptVariousValues(string hashData)
    {
        // Arrange
        var fileInfo = new BaseFileInfo();

        // Act
        fileInfo.HashData = hashData;

        // Assert
        Assert.Equal(hashData, fileInfo.HashData);
    }

    [Theory]
    [InlineData("")]
    [InlineData("file.txt")]
    [InlineData("document.pdf")]
    [InlineData("image with spaces.jpg")]
    [InlineData("file_with_underscores.doc")]
    public void BaseFileInfo_FileName_ShouldAcceptVariousValues(string fileName)
    {
        // Arrange
        var fileInfo = new BaseFileInfo();

        // Act
        fileInfo.FileName = fileName;

        // Assert
        Assert.Equal(fileName, fileInfo.FileName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1024)]
    [InlineData(1048576)]
    [InlineData(long.MaxValue)]
    public void BaseFileInfo_Length_ShouldAcceptVariousValues(long length)
    {
        // Arrange
        var fileInfo = new BaseFileInfo();

        // Act
        fileInfo.Length = length;

        // Assert
        Assert.Equal(length, fileInfo.Length);
    }

    #endregion

    #region ExtendedFileInfo Tests

    [Fact]
    public void ExtendedFileInfo_Constructor_ShouldInitializeWithDefaults()
    {
        // Act
        var fileInfo = new ExtendedFileInfo();

        // Assert
        Assert.NotNull(fileInfo);
        Assert.Null(fileInfo.HashData);
        Assert.Null(fileInfo.FileName);
        Assert.Null(fileInfo.FullFilePath);
        Assert.Null(fileInfo.FileSize);
        Assert.Equal(0, fileInfo.Length);
        Assert.Null(fileInfo.RelativeFilePath);
    }

    [Fact]
    public void ExtendedFileInfo_InheritsFromBaseFileInfo()
    {
        // Act
        var fileInfo = new ExtendedFileInfo();

        // Assert
        Assert.IsAssignableFrom<BaseFileInfo>(fileInfo);
    }

    [Fact]
    public void ExtendedFileInfo_Properties_ShouldGetAndSetCorrectly()
    {
        // Arrange
        var fileInfo = new ExtendedFileInfo();
        const string relativeFilePath = @"subfolder\test.txt";
        const string hashData = "XYZ789";
        const string fileName = "existing.txt";

        // Act
        fileInfo.RelativeFilePath = relativeFilePath;
        fileInfo.HashData = hashData;
        fileInfo.FileName = fileName;

        // Assert
        Assert.Equal(relativeFilePath, fileInfo.RelativeFilePath);
        Assert.Equal(hashData, fileInfo.HashData);
        Assert.Equal(fileName, fileInfo.FileName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("file.txt")]
    [InlineData(@"folder\file.txt")]
    [InlineData(@"folder\subfolder\file.txt")]
    [InlineData(@"..\parent\file.txt")]
    [InlineData("./unix/style/path.txt")]
    public void ExtendedFileInfo_RelativeFilePath_ShouldAcceptVariousValues(string relativeFilePath)
    {
        // Arrange
        var fileInfo = new ExtendedFileInfo();

        // Act
        fileInfo.RelativeFilePath = relativeFilePath;

        // Assert
        Assert.Equal(relativeFilePath, fileInfo.RelativeFilePath);
    }

    [Fact]
    public void ExtendedFileInfo_ShouldAllowNullRelativeFilePath()
    {
        // Arrange
        var fileInfo = new ExtendedFileInfo();

        // Act
        fileInfo.RelativeFilePath = null!;

        // Assert
        Assert.Null(fileInfo.RelativeFilePath);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FileInfoClasses_Inheritance_ShouldWorkCorrectly()
    {
        // Arrange & Act
        var baseFileInfo = new BaseFileInfo();
        var extendedFileInfo = new ExtendedFileInfo();

        // Assert inheritance chain
        Assert.True(extendedFileInfo is BaseFileInfo);

        // Assert types
        Assert.IsType<BaseFileInfo>(baseFileInfo);
        Assert.IsType<ExtendedFileInfo>(extendedFileInfo);
    }

    [Fact]
    public void FileInfoClasses_PolymorphicUsage_ShouldWorkCorrectly()
    {
        // Arrange
        BaseFileInfo[] fileInfos = {
            new BaseFileInfo { FileName = "base.txt" },
            new ExtendedFileInfo { FileName = "exist.txt", RelativeFilePath = "folder/exist.txt" },
        };

        // Act & Assert
        foreach (var fileInfo in fileInfos)
        {
            Assert.NotNull(fileInfo);
            Assert.NotNull(fileInfo.FileName);

            if (fileInfo is ExtendedFileInfo existFileInfo)
            {
                Assert.NotNull(existFileInfo.RelativeFilePath);
            }
        }
    }

    [Theory]
    [InlineData(typeof(BaseFileInfo))]
    [InlineData(typeof(ExtendedFileInfo))]
    public void FileInfoClasses_CanBeInstantiated(Type fileInfoType)
    {
        // Act
        var instance = Activator.CreateInstance(fileInfoType);

        // Assert
        Assert.NotNull(instance);
        Assert.IsAssignableFrom<BaseFileInfo>(instance);
    }

    #endregion
}
