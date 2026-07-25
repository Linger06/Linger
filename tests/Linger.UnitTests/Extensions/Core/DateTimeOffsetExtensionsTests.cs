using System;
using Linger.Extensions.Core;
using Xunit.v3;

namespace Linger.UnitTests.Extensions.Core;

public class DateTimeOffsetExtensionsTests
{
    [Fact]
    public void ToDateTime_WithUtcOffset_ReturnsUtcDateTime()
    {
        var utcNow = DateTime.UtcNow;
        var dateTimeOffset = new DateTimeOffset(utcNow);

        var result = dateTimeOffset.ToDateTime();

        Assert.Equal(utcNow, result);
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }

    [Fact]
    public void ToDateTime_WithLocalOffset_ReturnsExpectedDateTimeKind()
    {
        var localNow = DateTime.Now;
        var localOffset = TimeZoneInfo.Local.GetUtcOffset(localNow);
        var dateTimeOffset = new DateTimeOffset(localNow, localOffset);

        var result = dateTimeOffset.ToDateTime();

        var expectedKind = localOffset == TimeSpan.Zero ? DateTimeKind.Utc : DateTimeKind.Local;
        Assert.Equal(localNow, result);
        Assert.Equal(expectedKind, result.Kind);
    }

    [Fact]
    public void ToDateTime_WithCustomOffset_ReturnsDateTime()
    {
        var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        var customOffset = TimeSpan.FromHours(3);

        if (customOffset.Equals(TimeZoneInfo.Local.GetUtcOffset(now)))
        {
            customOffset = TimeSpan.FromHours(4);
        }

        var dateTimeOffset = new DateTimeOffset(now, customOffset);

        var result = dateTimeOffset.ToDateTime();

        Assert.Equal(dateTimeOffset.DateTime, result);
        Assert.Equal(DateTimeKind.Unspecified, result.Kind);
    }

    [Fact]
    public void ToDateTime_WithUtcLocalAndCustomOffsets_ReturnsExpectedKinds()
    {
        var baseDateTime = new DateTime(2025, 4, 11, 15, 30, 45);
        var localOffsetValue = TimeZoneInfo.Local.GetUtcOffset(baseDateTime);
        var utcOffset = new DateTimeOffset(baseDateTime, TimeSpan.Zero);
        var localOffset = new DateTimeOffset(baseDateTime, localOffsetValue);
        var customOffset = new DateTimeOffset(baseDateTime, TimeSpan.FromHours(5));

        var utcResult = utcOffset.ToDateTime();
        var localResult = localOffset.ToDateTime();
        var customResult = customOffset.ToDateTime();

        var expectedLocalKind = localOffsetValue == TimeSpan.Zero ? DateTimeKind.Utc : DateTimeKind.Local;
        Assert.Equal(DateTimeKind.Utc, utcResult.Kind);
        Assert.Equal(expectedLocalKind, localResult.Kind);
        Assert.Equal(baseDateTime, customResult);
    }
}
