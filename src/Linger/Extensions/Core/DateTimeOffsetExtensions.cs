namespace Linger.Extensions.Core;

public static class DateTimeOffsetExtensions
{
    public static DateTime ToDateTime(this DateTimeOffset dateTime)
    {
        if (dateTime.Offset.Equals(TimeSpan.Zero))
            return dateTime.UtcDateTime;
        else if (dateTime.Offset.Equals(TimeZoneInfo.Local.GetUtcOffset(dateTime.DateTime)))
            return DateTime.SpecifyKind(dateTime.DateTime, DateTimeKind.Local);
        else
            return dateTime.DateTime;
    }
}
