using System.Collections.ObjectModel;

namespace Linger.UnitTests.Extensions;

public class GuardExtensionsTests
{
    [Fact]
    public void EnsureIsNotNull_WithNonNullValue_DoesNotThrow()
    {
        // Arrange
        var obj = new object();

        // Act & Assert
        obj.EnsureIsNotNull();
    }

    [Fact]
    public void EnsureIsNotNull_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        object? obj = null;

        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => obj.EnsureIsNotNull());
    }

    [Fact]
    public void EnsureStringIsNotNullOrEmpty_WithNonEmptyString_DoesNotThrow()
    {
        // Arrange
        var str = "test";

        // Act & Assert
        str.EnsureIsNotNullAndEmpty();
    }

    [Fact]
    public void EnsureStringIsNotNullOrEmpty_WithEmptyString_ThrowsArgumentException()
    {
        // Arrange
        var str = "";

        // Act & Assert
        ArgumentException? exception = Assert.Throws<ArgumentException>(() => str.EnsureIsNotNullAndEmpty());
        Assert.Equal("The value cannot be an empty string.", exception.Message);
    }

    [Fact]
    public void EnsureStringIsNotNullOrEmpty_WithNullString_ThrowsArgumentNullException()
    {
        // Arrange
        string? str = null;

        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => str.EnsureIsNotNullAndEmpty());
    }

    [Fact]
    public void EnsureIsNull_WithNullValue_DoesNotThrow()
    {
        // Arrange
        object? obj = null;

        // Act & Assert
        obj.EnsureIsNull();
    }

    [Fact]
    public void EnsureIsNull_WithNonNullValue_ThrowsArgumentException()
    {
        // Arrange
        var obj = new object();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => obj.EnsureIsNull());
    }

    [Fact]
    public void EnsureIsNull_WithCustomParameterName_ThrowsArgumentExceptionWithCustomParameter()
    {
        // Arrange
        var obj = new object();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => obj.EnsureIsNull("customParam"));
        Assert.Contains("customParam", exception.Message);
    }

    [Fact]
    public void EnsureIsNull_WithCustomMessage_ThrowsArgumentExceptionWithCustomMessage()
    {
        // Arrange
        var obj = new object();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => obj.EnsureIsNull(message: "Custom error message"));
        Assert.Contains("Custom error message", exception.Message);
    }

    [Fact]
    public void EnsureIsNotNullOrEmpty_WithValidString_ReturnsString()
    {
        // Arrange
        var str = "test value";

        // Act
        var result = str.EnsureIsNotNullOrEmpty();

        // Assert
        Assert.Equal("test value", result);
    }

    [Fact]
    public void EnsureIsNotNullOrEmpty_WithNullString_ThrowsArgumentNullException()
    {
        // Arrange
        string? str = null;

        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => str.EnsureIsNotNullOrEmpty());
    }

    [Fact]
    public void EnsureIsNotNullOrEmpty_WithEmptyString_ThrowsArgumentException()
    {
        // Arrange
        var str = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => str.EnsureIsNotNullOrEmpty());
    }

    [Fact]
    public void EnsureIsNotNullOrWhiteSpace_WithValidString_ReturnsString()
    {
        // Arrange
        var str = "test value";

        // Act
        var result = str.EnsureIsNotNullOrWhiteSpace();

        // Assert
        Assert.Equal("test value", result);
    }

    [Fact]
    public void EnsureIsNotNullOrWhiteSpace_WithNullString_ThrowsArgumentNullException()
    {
        // Arrange
        string? str = null;

        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => str.EnsureIsNotNullOrWhiteSpace());
    }

    [Fact]
    public void EnsureIsNotNullOrWhiteSpace_WithWhiteSpaceString_ThrowsArgumentException()
    {
        // Arrange
        var str = "   ";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => str.EnsureIsNotNullOrWhiteSpace());
    }

    [Fact]
    public void EnsureIsTrue_WithTrueCondition_DoesNotThrow()
    {
        // Arrange
        var condition = true;

        // Act & Assert
        condition.EnsureIsTrue();
    }

    [Fact]
    public void EnsureIsTrue_WithFalseCondition_ThrowsArgumentException()
    {
        // Arrange
        var condition = false;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => condition.EnsureIsTrue());
    }

    [Fact]
    public void EnsureIsFalse_WithFalseCondition_DoesNotThrow()
    {
        // Arrange
        var condition = false;

        // Act & Assert
        condition.EnsureIsFalse();
    }

    [Fact]
    public void EnsureIsFalse_WithTrueCondition_ThrowsArgumentException()
    {
        // Arrange
        var condition = true;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => condition.EnsureIsFalse());
    }

    [Fact]
    public void EnsureIsInRange_WithValueInRange_ReturnsValue()
    {
        // Arrange
        var value = 5;
        var min = 1;
        var max = 10;

        // Act
        var result = value.EnsureIsInRange(min, max);

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public void EnsureIsInRange_WithValueBelowRange_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var value = 0;
        var min = 1;
        var max = 10;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => value.EnsureIsInRange(min, max));
    }

    [Fact]
    public void EnsureIsInRange_WithValueAboveRange_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var value = 11;
        var min = 1;
        var max = 10;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => value.EnsureIsInRange(min, max));
    }

    [Fact]
    public void EnsureIsInRange_WithCustomMessage_ThrowsExceptionWithCustomMessage()
    {
        // Arrange
        var value = 0;
        var min = 1;
        var max = 10;

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => value.EnsureIsInRange(min, max, message: "Custom range error"));
        Assert.Contains("Custom range error", exception.Message);
    }

    [Fact]
    public void EnsureFileExists_WithExistingFile_ReturnsFilePath()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            // Act
            var result = tempFile.EnsureFileExists();

            // Assert
            Assert.Equal(tempFile, result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void EnsureFileExists_WithNonExistingFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistingFile = "c:\\nonexistent\\file.txt";

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => nonExistingFile.EnsureFileExists());
    }

    [Fact]
    public void EnsureFileExists_WithNullFilePath_ThrowsArgumentNullException()
    {
        // Arrange
        string? filePath = null;

        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => filePath.EnsureFileExists());
    }

    [Fact]
    public void EnsureFileExists_WithEmptyFilePath_ThrowsArgumentException()
    {
        // Arrange
        var filePath = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => filePath.EnsureFileExists());
    }

    [Fact]
    public void EnsureDirectoryExists_WithExistingDirectory_ReturnsDirectoryPath()
    {
        // Arrange
        var tempDir = Path.GetTempPath();

        // Act
        var result = tempDir.EnsureDirectoryExists();

        // Assert
        Assert.Equal(tempDir, result);
    }

    [Fact]
    public void EnsureDirectoryExists_WithNonExistingDirectory_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        var nonExistingDir = "c:\\nonexistent\\directory";

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => nonExistingDir.EnsureDirectoryExists());
    }

    [Fact]
    public void EnsureDirectoryExists_WithNullDirectoryPath_ThrowsArgumentNullException()
    {
        // Arrange
        string? directoryPath = null;

        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => directoryPath.EnsureDirectoryExists());
    }

    [Fact]
    public void EnsureDirectoryExists_WithEmptyDirectoryPath_ThrowsArgumentException()
    {
        // Arrange
        var directoryPath = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => directoryPath.EnsureDirectoryExists());
    }

    [Fact]
    public void EnsureIsNotNullOrEmpty_WithValidCollection_ReturnsCollection()
    {
        // Arrange
        var collection = new List<int> { 1, 2, 3 };

        // Act
        var result = collection.EnsureIsNotNullOrEmpty();

        // Assert
        Assert.Equal(collection, result);
    }

    [Fact]
    public void EnsureIsNotNullOrEmpty_WithNullCollection_ThrowsArgumentNullException()
    {
        // Arrange
        List<int>? collection = null;

        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => collection.EnsureIsNotNullOrEmpty());
    }

    [Fact]
    public void EnsureIsNotNullOrEmpty_WithEmptyList_ThrowsArgumentException()
    {
        // Arrange
        var collection = new List<int>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => collection.EnsureIsNotNullOrEmpty());
    }

    [Fact]
    public void EnsureIsNotNullOrEmpty_WithEmptyArray_ThrowsArgumentException()
    {
        // Arrange
        var collection = new int[0];

        // Act & Assert
        Assert.Throws<ArgumentException>(() => collection.EnsureIsNotNullOrEmpty());
    }

    [Fact]
    public void EnsureIsNotNullOrEmpty_WithReadOnlyCollection_ReturnsCollection()
    {
        // Arrange
        var collection = new ReadOnlyCollection<int>(new List<int> { 1, 2, 3 });

        // Act
        var result = collection.EnsureIsNotNullOrEmpty();

        // Assert
        Assert.Equal(collection, result);
    }

    [Fact]
    public void EnsureIsNotNullOrEmpty_WithEmptyReadOnlyCollection_ThrowsArgumentException()
    {
        // Arrange
        var collection = new ReadOnlyCollection<int>(new List<int>());

        // Act & Assert
        Assert.Throws<ArgumentException>(() => collection.EnsureIsNotNullOrEmpty());
    }

    [Fact]
    public void EnsureIsNotNullOrEmpty_WithCustomEnumerable_ReturnsCollection()
    {
        // Arrange
        var collection = new CustomEnumerable<int>(new[] { 1, 2, 3 });

        // Act
        var result = collection.EnsureIsNotNullOrEmpty();

        // Assert
        Assert.Equal(collection, result);
    }

    [Fact]
    public void EnsureIsNotNullOrEmpty_WithEmptyCustomEnumerable_ThrowsArgumentException()
    {
        // Arrange
        var collection = new CustomEnumerable<int>(new int[0]);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => collection.EnsureIsNotNullOrEmpty());
    }

    [Fact]
    public void EnsureIsNotNullOrEmpty_WithCustomMessage_ThrowsExceptionWithCustomMessage()
    {
        // Arrange
        var collection = new List<int>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => collection.EnsureIsNotNullOrEmpty(message: "Custom empty collection error"));
        Assert.Contains("Custom empty collection error", exception.Message);
    }

    [Fact]
    public void EnsureIsNotNullOrEmpty_WithCustomParameterName_ThrowsExceptionWithCustomParameter()
    {
        // Arrange
        var collection = new List<int>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => collection.EnsureIsNotNullOrEmpty("customParam"));
        Assert.Contains("customParam", exception.Message);
    }

    // Helper class for testing custom enumerable scenarios
    private class CustomEnumerable<T> : IEnumerable<T>
    {
        private readonly T[] _items;

        public CustomEnumerable(T[] items)
        {
            _items = items;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}