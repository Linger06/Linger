namespace Linger.UnitTests.Helper;

public class PathHelperTests
{
    [Theory]
    [InlineData("C:\\example\\path", "C:\\example\\path")]
    [InlineData("C:\\example\\path\\", "C:\\example\\path")]
    [InlineData("\\\\server\\share\\path", "\\\\server\\share\\path")]
    [InlineData("//server/share/path", "//server/share/path")]
    [InlineData("//server/share/path/", "//server/share/path")]
    [InlineData("", "")]
    [InlineData(null, null)]
    [InlineData("C:\\example\\path\\test.txt", "C:\\example\\path\\test.txt")]
    [InlineData("C:\\example\\path\\../test", "C:\\example\\test")]
    [InlineData("C:/example/path", "C:\\example\\path")]
    [InlineData("C:/example/path/", "C:\\example\\path")]
    [InlineData("/unix/path/", "/unix/path")]
    [InlineData("/unix/path", "/unix/path")]
    [InlineData("/", "")]
    [InlineData("file:///C:/folder/file.txt", "C:\\folder\\file.txt")]
    public void NormalizePath_ShouldHandleVariousPathFormats(string input, string expected)
    {
        var result = PathHelper.NormalizePath(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("C:\\test\\path", "C:\\test\\path\\")]
    [InlineData("C:\\test\\path\\", "C:\\test\\path\\")]
    [InlineData("", "")]
    [InlineData(null, null)]
    public void NormalizePathEndingDirectorySeparator_ShouldAddTrailingSlash(string input, string expected)
    {
        // Act
        var result = PathHelper.NormalizePathEndingDirectorySeparator(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, typeof(ArgumentNullException))]
    [InlineData("", false)]
    [InlineData(" ", false)]
    public void IsFile_ShouldHandleInvalidInput(string input, object expected)
    {
        if (expected is bool boolResult)
        {
            // Act
            var result = PathHelper.IsFile(input);

            // Assert
            Assert.Equal(boolResult, result);
        }
        else if (expected is Type exceptionType)
        {
            // Assert
            Assert.Throws(exceptionType, () => PathHelper.IsFile(input));
        }
    }

    [Theory]
    [InlineData(null, typeof(ArgumentNullException))]
    [InlineData("", false)]
    [InlineData(" ", false)]
    public void IsDirectory_ShouldHandleInvalidInput(string input, object expected)
    {
        if (expected is bool boolResult)
        {
            // Act
            var result = PathHelper.IsDirectory(input);

            // Assert
            Assert.Equal(boolResult, result);
        }
        else if (expected is Type exceptionType)
        {
            // Assert
            Assert.Throws(exceptionType, () => PathHelper.IsDirectory(input));
        }
    }

    [Fact]
    public void IsFile_ShouldIdentifyFileCorrectly()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            var result = PathHelper.IsFile(tempFile);

            // Assert
            Assert.True(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void IsDirectory_ShouldIdentifyDirectoryCorrectly()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var result = PathHelper.IsDirectory(tempDir);

            // Assert
            Assert.True(result);
        }
        finally
        {
            Directory.Delete(tempDir);
        }
    }
}