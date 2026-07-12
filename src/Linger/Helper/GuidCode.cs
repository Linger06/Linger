using Linger.Extensions.Core;

namespace Linger.Helper;

/// <summary>
/// Provides methods to generate various types of unique identifiers.
/// </summary>
public static class GuidCode
{
    /// <summary>
    /// Gets a new unique identifier based on the current date and time and a GUID.
    /// </summary>
    public static string NewId
    {
        get
        {
            var id = DateTime.Now.ToString("yyyyMMddHHmmssfffffff", CultureInfo.InvariantCulture);
            var guid = Guid.NewGuid().ToString().Replace("-", string.Empty);
            id += guid.Take(10);
            return id;
        }
    }

    /// <summary>
    /// Gets a compact identifier based on the current date and part of a GUID.
    /// </summary>
    /// <remarks>This 10-character value is not guaranteed to be unique. Use a full GUID when uniqueness is required.</remarks>
    [Obsolete("This compact value is not guaranteed to be unique. Use Guid.NewGuid() or CreateVersion7() instead.")]
    public static string NewDateGuid
    {
        get
        {
            var id = DateTime.Now.ToString("yyMMdd", CultureInfo.InvariantCulture);
            var guid = Guid.NewGuid().ToString().Replace("-", string.Empty);
            id += guid.Take(4);
            return id;
        }
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Creates a new version 7 GUID.
    /// </summary>
    /// <returns>A new version 7 GUID.</returns>
    public static Guid CreateVersion7()
    {
        return Guid.CreateVersion7();
    }
#endif

    /// <summary>
    /// Gets a 64-bit code based on part of a GUID.
    /// </summary>
    /// <returns>A 64-bit integer that is not guaranteed to be unique.</returns>
    [Obsolete("This 64-bit value is not guaranteed to be unique. Use a full Guid instead.")]
    public static long GetInt64UniqueCode()
    {
        var value = Guid.NewGuid();
        return value.ToInt64();
    }

    /// <summary>
    /// Gets a 32-bit code based on part of a GUID.
    /// </summary>
    /// <returns>A 32-bit integer that is not guaranteed to be unique.</returns>
    [Obsolete("This 32-bit value is not guaranteed to be unique. Use a full Guid instead.")]
    public static int GetInt32UniqueCode()
    {
        var value = Guid.NewGuid();
        return value.ToInt32();
    }
}
