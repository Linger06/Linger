using System.IO;
using Linger.Helper;
using Xunit.v3;

namespace Linger.UnitTests.Helper;

public class GuardExtensionsTests
{
    [Fact]
    public void EnsureIsNotNull_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        object? obj = null;

        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => obj.EnsureIsNotNull("testParam"));
    }

    [Fact]
    public void EnsureIsNotNull_WithNonNullValue_DoesNotThrow()
    {
        // Arrange
        object obj = new object();

        // Act & Assert (不应该抛出异常)
        obj.EnsureIsNotNull("testParam");
    }

    [Fact]
    public void EnsureIsNotNullAndEmpty_WithNullString_ThrowsArgumentNullException()
    {
        // Arrange
        string? str = null;

        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => str.EnsureIsNotNullAndEmpty("testParam"));
    }

    [Fact]
    public void EnsureIsNotNullAndEmpty_WithEmptyString_ThrowsArgumentException()
    {
        // Arrange
        string str = string.Empty;

        // Act & Assert
        Assert.Throws<System.ArgumentException>(() => str.EnsureIsNotNullAndEmpty("testParam"));
    }

    [Fact]
    public void EnsureIsNotNullAndEmpty_WithNonEmptyString_DoesNotThrow()
    {
        // Arrange
        var str = "test";

        // Act & Assert
        str.EnsureIsNotNullAndEmpty("testParam");
    }

    [Fact]
    public void EnsureIsNotNullAndWhiteSpace_WithNullString_ThrowsArgumentNullException()
    {
        // Arrange
        string? str = null;

        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => str.EnsureIsNotNullAndWhiteSpace("testParam"));
    }

    [Fact]
    public void EnsureIsNotNullAndWhiteSpace_WithWhiteSpaceString_ThrowsArgumentException()
    {
        // Arrange
        string str = "   ";

        // Act & Assert
        Assert.Throws<System.ArgumentException>(() => str.EnsureIsNotNullAndWhiteSpace("testParam"));
    }

    [Fact]
    public void EnsureIsNotNullAndWhiteSpace_WithNonWhiteSpaceString_DoesNotThrow()
    {
        // Arrange
        var str = "test";

        // Act & Assert
        str.EnsureIsNotNullAndWhiteSpace("testParam");
    }

    [Fact]
    public void EnsureIsNull_WithNullValue_DoesNotThrow()
    {
        // Arrange
        object? obj = null;

        // Act & Assert
        obj.EnsureIsNull("testParam");
    }

    [Fact]
    public void EnsureIsNull_WithNonNullValue_ThrowsArgumentException()
    {
        // Arrange
        object obj = new object();

        // Act & Assert
        Assert.Throws<System.ArgumentException>(() => obj.EnsureIsNull("testParam"));
    }

    [Fact]
    public void EnsureIsTrue_WithTrueCondition_DoesNotThrow()
    {
        // Arrange
        bool condition = true;

        // Act & Assert
        condition.EnsureIsTrue("testParam");
    }

    [Fact]
    public void EnsureIsTrue_WithFalseCondition_ThrowsArgumentException()
    {
        // Arrange
        bool condition = false;

        // Act & Assert
        Assert.Throws<System.ArgumentException>(() => condition.EnsureIsTrue("testParam"));
    }

    [Fact]
    public void EnsureIsFalse_WithFalseCondition_DoesNotThrow()
    {
        // Arrange
        bool condition = false;

        // Act & Assert
        condition.EnsureIsFalse("testParam");
    }

    [Fact]
    public void EnsureIsFalse_WithTrueCondition_ThrowsArgumentException()
    {
        // Arrange
        bool condition = true;

        // Act & Assert
        Assert.Throws<System.ArgumentException>(() => condition.EnsureIsFalse("testParam"));
    }

    [Fact]
    public void EnsureIsInRange_WithValueInRange_DoesNotThrow()
    {
        // Arrange
        int value = 5;

        // Act & Assert
        value.EnsureIsInRange(1, 10, "testParam");
    }

    [Fact]
    public void EnsureIsInRange_WithValueBelowRange_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        int value = 0;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => value.EnsureIsInRange(1, 10, "testParam"));
    }

    [Fact]
    public void EnsureIsInRange_WithValueAboveRange_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        int value = 11;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => value.EnsureIsInRange(1, 10, "testParam"));
    }

    [Fact]
    public void EnsureIsNotNullOrEmpty_WithNullCollection_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<int>? collection = null;

        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => collection.EnsureIsNotNullOrEmpty("testParam"));
    }

    [Fact]
    public void EnsureIsNotNullOrEmpty_WithEmptyCollection_ThrowsArgumentException()
    {
        // Arrange
        var collection = Array.Empty<int>();

        // Act & Assert
        Assert.Throws<System.ArgumentException>(() => collection.EnsureIsNotNullOrEmpty("testParam"));
    }

    [Fact]
    public void EnsureIsNotNullOrEmpty_WithNonEmptyCollection_DoesNotThrow()
    {
        // Arrange
        var collection = new[] { 1, 2, 3 };

        // Act & Assert
        collection.EnsureIsNotNullOrEmpty("testParam");
    }

    [Fact]
    public void EnsureFileExist_WithExistingFile_DoesNotThrow()
    {
        // Arrange
        string tempFile = Path.GetTempFileName();
        try
        {
            // Act & Assert
            tempFile.EnsureFileExist();
        }
        finally
        {
            // 清理临时文件
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void EnsureFileExist_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        string nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => nonExistentFile.EnsureFileExist());
    }

    [Fact]
    public void EnsureDirectoryExist_WithExistingDirectory_DoesNotThrow()
    {
        // Arrange
        string tempDirectory = Path.GetTempPath();

        // Act & Assert
        tempDirectory.EnsureDirectoryExist();
    }

    [Fact]
    public void EnsureDirectoryExist_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        string nonExistentDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => nonExistentDirectory.EnsureDirectoryExist());
    }

    // New naming test coverage
    [Fact]
    public void EnsureFileExists_NewName_WithExistingFile_DoesNotThrow()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            tempFile.EnsureFileExists();
        }
        finally { if (File.Exists(tempFile)) File.Delete(tempFile); }
    }

    [Fact]
    public void EnsureFileExists_NewName_WithMissingFile_Throws()
    {
        var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".tmp");
        Assert.Throws<FileNotFoundException>(() => missing.EnsureFileExists());
    }

    [Fact]
    public void EnsureDirectoryExists_NewName_WithExistingDirectory_DoesNotThrow()
    {
        var dir = Path.GetTempPath();
        dir.EnsureDirectoryExists();
    }

    [Fact]
    public void EnsureDirectoryExists_NewName_WithMissingDirectory_Throws()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Assert.Throws<DirectoryNotFoundException>(() => dir.EnsureDirectoryExists());
    }
}
