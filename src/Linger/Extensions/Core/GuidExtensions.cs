namespace Linger.Extensions.Core;

/// <summary>
/// Provides extension methods for the <see cref="Guid"/> struct.
/// </summary>
public static class GuidExtensions
{
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
        return value is null;
    }

    /// <summary>
    /// Check if the specified nullable <see cref="Guid"/> is not null.
    /// </summary>
    /// <param name="value">The nullable <see cref="Guid"/> to check.</param>
    /// <returns><c>true</c> if the nullable <see cref="Guid"/> is not null; otherwise, <c>false</c>.</returns>
    public static bool IsNotNull(this Guid? value)
    {
        return value is not null;
    }

    /// <summary>
    /// Check if the specified nullable <see cref="Guid"/> is null or <see cref="Guid.Empty"/>.
    /// </summary>
    /// <param name="value">The nullable <see cref="Guid"/> to check.</param>
    /// <returns><c>true</c> if the nullable <see cref="Guid"/> is null or <see cref="Guid.Empty"/>; otherwise, <c>false</c>.</returns>
    public static bool IsNullOrEmpty(this Guid? value)
    {
        return value is null || value.Value == Guid.Empty;
    }

    /// <summary>
    /// Determines whether the specified nullable <see cref="Guid"/> is neither null nor <see cref="Guid.Empty"/>.
    /// </summary>
    /// <param name="value">The nullable Guid.</param>
    /// <returns><c>true</c> if value has a non-empty Guid; otherwise, <c>false</c>.</returns>
    public static bool IsNotNullOrEmpty(this Guid? value) => !value.IsNullOrEmpty();

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
            var str = guid.ToString("N")[..12];
            var milliseconds = Convert.ToInt64(str, 16);
            var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
            return timestamp;
        }

        throw new NotSupportedException("This method is only supported for GUIDs with version 7.");
    }
#endif
}
