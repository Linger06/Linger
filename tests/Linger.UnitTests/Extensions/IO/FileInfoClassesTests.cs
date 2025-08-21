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

    #region CustomExistFileInfo Tests

    [Fact]
    public void CustomExistFileInfo_Constructor_ShouldInitializeWithDefaults()
    {
        // Act
        var fileInfo = new CustomExistFileInfo();

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
    public void CustomExistFileInfo_InheritsFromBaseFileInfo()
    {
        // Act
        var fileInfo = new CustomExistFileInfo();

        // Assert
        Assert.IsAssignableFrom<BaseFileInfo>(fileInfo);
    }

    [Fact]
    public void CustomExistFileInfo_Properties_ShouldGetAndSetCorrectly()
    {
        // Arrange
        var fileInfo = new CustomExistFileInfo();
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
    public void CustomExistFileInfo_RelativeFilePath_ShouldAcceptVariousValues(string relativeFilePath)
    {
        // Arrange
        var fileInfo = new CustomExistFileInfo();

        // Act
        fileInfo.RelativeFilePath = relativeFilePath;

        // Assert
        Assert.Equal(relativeFilePath, fileInfo.RelativeFilePath);
    }

    [Fact]
    public void CustomExistFileInfo_ShouldAllowNullRelativeFilePath()
    {
        // Arrange
        var fileInfo = new CustomExistFileInfo();

        // Act
        fileInfo.RelativeFilePath = null!;

        // Assert
        Assert.Null(fileInfo.RelativeFilePath);
    }

    #endregion

    #region CustomFileInfo Tests

    [Fact]
    public void CustomFileInfo_Constructor_ShouldInitializeWithDefaults()
    {
        // Act
        var fileInfo = new CustomFileInfo();

        // Assert
        Assert.NotNull(fileInfo);
        Assert.Null(fileInfo.HashData);
        Assert.Null(fileInfo.FileName);
        Assert.Null(fileInfo.FullFilePath);
        Assert.Null(fileInfo.FileSize);
        Assert.Equal(0, fileInfo.Length);
        Assert.Null(fileInfo.RelativeFilePath);
        Assert.Null(fileInfo.NewFileName);
    }

    [Fact]
    public void CustomFileInfo_InheritsFromCustomExistFileInfo()
    {
        // Act
        var fileInfo = new CustomFileInfo();

        // Assert
        Assert.IsAssignableFrom<CustomExistFileInfo>(fileInfo);
        Assert.IsAssignableFrom<BaseFileInfo>(fileInfo);
    }

    [Fact]
    public void CustomFileInfo_Properties_ShouldGetAndSetCorrectly()
    {
        // Arrange
        var fileInfo = new CustomFileInfo();
        const string newFileName = "renamed_file.txt";
        const string relativeFilePath = @"uploads\temp.txt";
        const string hashData = "DEF456";
        const string fileName = "upload.txt";

        // Act
        fileInfo.NewFileName = newFileName;
        fileInfo.RelativeFilePath = relativeFilePath;
        fileInfo.HashData = hashData;
        fileInfo.FileName = fileName;

        // Assert
        Assert.Equal(newFileName, fileInfo.NewFileName);
        Assert.Equal(relativeFilePath, fileInfo.RelativeFilePath);
        Assert.Equal(hashData, fileInfo.HashData);
        Assert.Equal(fileName, fileInfo.FileName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("new_file.txt")]
    [InlineData("renamed document.pdf")]
    [InlineData("file_with_new_name.doc")]
    [InlineData("特殊字符文件名.txt")]
    public void CustomFileInfo_NewFileName_ShouldAcceptVariousValues(string newFileName)
    {
        // Arrange
        var fileInfo = new CustomFileInfo();

        // Act
        fileInfo.NewFileName = newFileName;

        // Assert
        Assert.Equal(newFileName, fileInfo.NewFileName);
    }

    [Fact]
    public void CustomFileInfo_ShouldAllowNullNewFileName()
    {
        // Arrange
        var fileInfo = new CustomFileInfo();

        // Act
        fileInfo.NewFileName = null;

        // Assert
        Assert.Null(fileInfo.NewFileName);
    }

    [Fact]
    public void CustomFileInfo_CompleteFileInfoScenario_ShouldWorkCorrectly()
    {
        // Arrange
        var fileInfo = new CustomFileInfo();

        // Act - Simulate a file upload scenario
        fileInfo.FileName = "original.jpg";
        fileInfo.NewFileName = "processed_image.jpg";
        fileInfo.FullFilePath = @"C:\uploads\original.jpg";
        fileInfo.RelativeFilePath = @"uploads\original.jpg";
        fileInfo.HashData = "MD5:A1B2C3D4E5F6";
        fileInfo.FileSize = "2.5 MB";
        fileInfo.Length = 2621440; // 2.5 MB in bytes

        // Assert
        Assert.Equal("original.jpg", fileInfo.FileName);
        Assert.Equal("processed_image.jpg", fileInfo.NewFileName);
        Assert.Equal(@"C:\uploads\original.jpg", fileInfo.FullFilePath);
        Assert.Equal(@"uploads\original.jpg", fileInfo.RelativeFilePath);
        Assert.Equal("MD5:A1B2C3D4E5F6", fileInfo.HashData);
        Assert.Equal("2.5 MB", fileInfo.FileSize);
        Assert.Equal(2621440, fileInfo.Length);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FileInfoClasses_Inheritance_ShouldWorkCorrectly()
    {
        // Arrange & Act
        var baseFileInfo = new BaseFileInfo();
        var customExistFileInfo = new CustomExistFileInfo();
        var customFileInfo = new CustomFileInfo();

        // Assert inheritance chain
        Assert.True(customExistFileInfo is BaseFileInfo);
        Assert.True(customFileInfo is CustomExistFileInfo);
        Assert.True(customFileInfo is BaseFileInfo);

        // Assert types
        Assert.IsType<BaseFileInfo>(baseFileInfo);
        Assert.IsType<CustomExistFileInfo>(customExistFileInfo);
        Assert.IsType<CustomFileInfo>(customFileInfo);
    }

    [Fact]
    public void FileInfoClasses_PolymorphicUsage_ShouldWorkCorrectly()
    {
        // Arrange
        BaseFileInfo[] fileInfos = {
            new BaseFileInfo { FileName = "base.txt" },
            new CustomExistFileInfo { FileName = "exist.txt", RelativeFilePath = "folder/exist.txt" },
            new CustomFileInfo { FileName = "custom.txt", NewFileName = "new_custom.txt" }
        };

        // Act & Assert
        foreach (var fileInfo in fileInfos)
        {
            Assert.NotNull(fileInfo);
            Assert.NotNull(fileInfo.FileName);
            
            if (fileInfo is CustomExistFileInfo existFileInfo)
            {
                Assert.NotNull(existFileInfo.RelativeFilePath);
            }
            
            if (fileInfo is CustomFileInfo customFileInfo)
            {
                Assert.True(!string.IsNullOrEmpty(customFileInfo.NewFileName));
            }
        }
    }

    [Theory]
    [InlineData(typeof(BaseFileInfo))]
    [InlineData(typeof(CustomExistFileInfo))]
    [InlineData(typeof(CustomFileInfo))]
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
