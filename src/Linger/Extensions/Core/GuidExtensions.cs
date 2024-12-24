namespace Linger.Extensions.Core;

/// <summary>
/// Provides extension methods for the <see cref="Guid"/> struct.
/// </summary>
public static class GuidExtensions
{
    /// <summary>
    /// Generates a new <see cref="Guid"/> for database use.
    /// </summary>
    /// <remarks>
    /// The generated <see cref="Guid"/> is based on the current date and time, 
    /// ensuring uniqueness and suitability for database operations.
    /// </remarks>
    public static Guid NewId
    {
        get
        {
            var guidArray = Guid.NewGuid().ToByteArray();
            DateTime dbMinDate = DateTimeExtensions.MsSqlDateTimeInitial;
            DateTime nowDate = DateTime.Now;

            var days = new TimeSpan(nowDate.Ticks - dbMinDate.Ticks);
            var msSecond =
                new TimeSpan(nowDate.Ticks - new DateTime(nowDate.Year, nowDate.Month, nowDate.Day).Ticks);

            var daysArray = BitConverter.GetBytes(days.Days);
            var msSecondArray = BitConverter.GetBytes((long)(msSecond.TotalMilliseconds / 3.333333));

            Array.Reverse(daysArray);
            Array.Reverse(msSecondArray);

            Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
            Array.Copy(msSecondArray, daysArray.Length - 4, guidArray, guidArray.Length - 4, 4);

            return new Guid(guidArray);
        }
    }

    /// <summary>
    /// Check if the specified <see cref="Guid"/> is <see cref="Guid.Empty"/>.
    /// </summary>
    /// <param name="value">The <see cref="Guid"/> to check.</param>
    /// <returns><c>true</c> if the <see cref="Guid"/> is <see cref="Guid.Empty"/>; otherwise, <c>false</c>.</returns>
    public static bool IsEmpty(this Guid value)
    {
        return value == Guid.Empty;
    }

    /// <summary>
    /// Check if the specified <see cref="Guid"/> is not <see cref="Guid.Empty"/>.
    /// </summary>
    /// <param name="value">The <see cref="Guid"/> to check.</param>
    /// <returns><c>true</c> if the <see cref="Guid"/> is not <see cref="Guid.Empty"/>; otherwise, <c>false</c>.</returns>
    public static bool IsNotEmpty(this Guid value)
    {
        return value != Guid.Empty;
    }

    /// <summary>
    /// Check if the specified nullable <see cref="Guid"/> is null.
    /// </summary>
    /// <param name="value">The nullable <see cref="Guid"/> to check.</param>
    /// <returns><c>true</c> if the nullable <see cref="Guid"/> is null; otherwise, <c>false</c>.</returns>
    public static bool IsNull(this Guid? value)
    {
        return value == null;
    }

    /// <summary>
    /// Check if the specified nullable <see cref="Guid"/> is not null.
    /// </summary>
    /// <param name="value">The nullable <see cref="Guid"/> to check.</param>
    /// <returns><c>true</c> if the nullable <see cref="Guid"/> is not null; otherwise, <c>false</c>.</returns>
    public static bool IsNotNull(this Guid? value)
    {
        return value != null;
    }

    /// <summary>
    /// Check if the specified nullable <see cref="Guid"/> is null or <see cref="Guid.Empty"/>.
    /// </summary>
    /// <param name="value">The nullable <see cref="Guid"/> to check.</param>
    /// <returns><c>true</c> if the nullable <see cref="Guid"/> is null or <see cref="Guid.Empty"/>; otherwise, <c>false</c>.</returns>
    public static bool IsNullOrEmpty(this Guid? value)
    {
        if (value == null)
        {
            return true;
        }

        if (value.Value == Guid.Empty)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check if the specified nullable <see cref="Guid"/> is not null and not <see cref="Guid.Empty"/>.
    /// </summary>
    /// <param name="value">The nullable <see cref="Guid"/> to check.</param>
    /// <returns><c>true</c> if the nullable <see cref="Guid"/> is not null and not <see cref="Guid.Empty"/>; otherwise, <c>false</c>.</returns>
    public static bool IsNotNullAndEmpty(this Guid? value)
    {
        return !value.IsNullOrEmpty();
    }

    /// <summary>
    /// Converts the specified <see cref="Guid"/> to a 64-bit integer.
    /// </summary>
    /// <param name="value">The <see cref="Guid"/> to convert.</param>
    /// <returns>A 64-bit integer representation of the <see cref="Guid"/>.</returns>
    public static long ToInt64(this Guid value)
    {
        var bytes = value.ToByteArray();
        return BitConverter.ToInt64(bytes, 0);
    }

    /// <summary>
    /// Converts the specified <see cref="Guid"/> to a 32-bit integer.
    /// </summary>
    /// <param name="value">The <see cref="Guid"/> to convert.</param>
    /// <returns>A 32-bit integer representation of the <see cref="Guid"/>.</returns>
    public static int ToInt32(this Guid value)
    {
        var bytes = value.ToByteArray();
        return BitConverter.ToInt32(bytes, 0);
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Extracts the timestamp from a version 7 GUID.
    /// </summary>
    /// <param name="guid">The version 7 GUID to extract the timestamp from.</param>
    /// <returns>A DateTimeOffset representing the timestamp embedded in the GUID.</returns>
    /// <remarks>
    /// This method is only available in .NET 9.0 or greater.
    /// It extracts the Unix timestamp (in milliseconds) from the first 12 characters of the GUID,
    /// which represents the creation time for version 7 GUIDs according to the UUID v7 specification.
    /// </remarks>
    /// <exception cref="NotSupportedException">Thrown when the GUID is not version 7.</exception>
    public static DateTimeOffset GetTimestamp(this Guid guid)
    {
        if (guid.Version == 7)
        {
            string str = guid.ToString("N")[..12];
            long milliseconds = Convert.ToInt64(str, 16);
            DateTimeOffset timestamp = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
            return timestamp;
        }
        else
        {
            throw new NotSupportedException("This method is only supported for GUIDs with version 7.");
        }
    }
#endif
}