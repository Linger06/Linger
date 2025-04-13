using System;
using Linger.Extensions.Core;
using Xunit.v3;

namespace Linger.UnitTests.Extensions.Core;

public class DateTimeOffsetExtensionsTests
{
    [Fact]
    public void ToDateTime_WithUtcOffset_ReturnsUtcDateTime()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        var dateTimeOffset = new DateTimeOffset(utcNow);
        
        // Act
        var result = dateTimeOffset.ToDateTime();
        
        // Assert
        Assert.Equal(utcNow, result);
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }
    
    [Fact]
    public void ToDateTime_WithLocalOffset_ReturnsLocalDateTime()
    {
        // Arrange
        var localNow = DateTime.Now;
        var localOffset = TimeZoneInfo.Local.GetUtcOffset(localNow);
        var dateTimeOffset = new DateTimeOffset(localNow, localOffset);
        
        // Act
        var result = dateTimeOffset.ToDateTime();
        
        // Assert
        Assert.Equal(localNow, result);
        Assert.Equal(DateTimeKind.Local, result.Kind);
    }
    
    [Fact]
    public void ToDateTime_WithCustomOffset_ReturnsDateTime()
    {
        // Arrange
        // 使用具有Unspecified Kind的DateTime来创建带自定义偏移量的DateTimeOffset
        var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        // 创建一个不是UTC也不是本地的自定义偏移量
        var customOffset = TimeSpan.FromHours(3); // 例如 UTC+3
        
        // 确保这个偏移量与本地偏移量不同
        if (customOffset.Equals(TimeZoneInfo.Local.GetUtcOffset(now)))
        {
            customOffset = TimeSpan.FromHours(4); // 尝试使用另一个偏移量
        }
        
        var dateTimeOffset = new DateTimeOffset(now, customOffset);
        
        // Act
        var result = dateTimeOffset.ToDateTime();
        
        // Assert
        Assert.Equal(dateTimeOffset.DateTime, result);
        Assert.Equal(DateTimeKind.Unspecified, result.Kind);
    }
    
    [Fact]
    public void ToDateTime_WithDifferentOffsets_ReturnsDifferentKinds()
    {
        // Arrange
        var baseDateTime = new DateTime(2025, 4, 11, 15, 30, 45);
        
        var utcOffset = new DateTimeOffset(baseDateTime, TimeSpan.Zero);
        var localOffset = new DateTimeOffset(baseDateTime, TimeZoneInfo.Local.GetUtcOffset(baseDateTime));
        var customOffset = new DateTimeOffset(baseDateTime, TimeSpan.FromHours(5));
        
        // Act
        var utcResult = utcOffset.ToDateTime();
        var localResult = localOffset.ToDateTime();
        var customResult = customOffset.ToDateTime();
        
        // Assert
        Assert.Equal(DateTimeKind.Utc, utcResult.Kind);
        Assert.Equal(DateTimeKind.Local, localResult.Kind);
        
        // 对于自定义偏移量，DateTime.Kind可能是Unspecified
        Assert.Equal(baseDateTime, customResult);
    }
}