using Linger.Helper;
using Xunit.v3;

namespace Linger.UnitTests.Helper;

public class GuidCodeTests
{
    [Fact]
    public void NewId_ShouldGenerateUniqueIds()
    {
        // Arrange & Act
        var id1 = GuidCode.NewId;
        var id2 = GuidCode.NewId;

        // Assert
        Assert.NotEqual(id1, id2);
        Assert.Equal(31, id1.Length); // 24 for date + 10 for guid
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
    public void NewDateGuid_ShouldGenerateUniqueIds()
    {
        // Arrange & Act
        var id1 = GuidCode.NewDateGuid;
        var id2 = GuidCode.NewDateGuid;

        // Assert
        Assert.NotEqual(id1, id2);
        Assert.Equal(10, id1.Length); // 6 for date + 4 for guid
        Assert.Equal(10, id2.Length);
    }

    [Fact]
    public void NewDateGuid_ShouldStartWithCurrentDate()
    {
        // Arrange & Act
        var id = GuidCode.NewDateGuid;
        var datePrefix = DateTime.Now.ToString("yyMMdd");

        // Assert
        Assert.StartsWith(datePrefix, id);
    }

    [Fact]
    public void NewGuid_ShouldGenerateUniqueGuids()
    {
        // Arrange & Act
        var guid1 = GuidCode.NewGuid();
        var guid2 = GuidCode.NewGuid();

        // Assert
        Assert.NotEqual(guid1, guid2);
    }

#if NET9_0_OR_GREATER
    [Fact]
    public void CreateVersion7_ShouldGenerateVersion7Guid()
    {
        // Arrange & Act
        var guid = GuidCode.CreateVersion7();

        // Assert
        Assert.Equal(7, (guid.Version));
    }
#endif

    [Fact]
    public void GetInt64UniqueCode_ShouldGenerateUniqueValues()
    {
        // Arrange & Act
        var code1 = GuidCode.GetInt64UniqueCode();
        var code2 = GuidCode.GetInt64UniqueCode();

        // Assert
        Assert.NotEqual(code1, code2);
    }

    [Fact]
    public void GetInt32UniqueCode_ShouldGenerateUniqueValues()
    {
        // Arrange & Act
        var code1 = GuidCode.GetInt32UniqueCode();
        var code2 = GuidCode.GetInt32UniqueCode();

        // Assert
        Assert.NotEqual(code1, code2);
    }

#pragma warning disable CS0618 // 类型或成员已过时
    [Fact]
    public void NewDateTimeId_ShouldGenerateIdsWithCorrectLength()
    {
        // Arrange & Act
        var id = GuidCode.NewDateTimeId;

        // Assert
        Assert.Equal(21, id.Length);
        Assert.StartsWith(DateTime.Now.ToString("yyyyMMdd"), id);
    }
#pragma warning restore CS0618
}
