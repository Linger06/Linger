using Linger.Helper;
using Xunit.v3;

namespace Linger.UnitTests.Helper;

public class GuidCodeTests
{
    [Fact]
    public void NewId_ShouldGenerateDifferentIds()
    {
        // Arrange & Act
        var id1 = GuidCode.NewId;
        var id2 = GuidCode.NewId;

        // Assert
        Assert.NotEqual(id1, id2);
        Assert.Equal(31, id1.Length); // 21 for date/time + 10 for GUID suffix
        Assert.Equal(31, id2.Length);
    }

    [Fact]
    public void NewId_ShouldStartWithCurrentDateTime()
    {
        // Arrange & Act
        var id = GuidCode.NewId;
        var dateTimePrefix = DateTime.Now.ToString("yyyyMMdd");

        // Assert
        Assert.StartsWith(dateTimePrefix, id);
    }

    [Fact]
    public void GuidNewGuid_ShouldGenerateUniqueGuids()
    {
        // Arrange & Act
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();

        // Assert
        Assert.NotEqual(guid1, guid2);
    }
}
