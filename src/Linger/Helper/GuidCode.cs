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
            var id = DateTime.Now.ToString("yyyyMMddHHmmssfffffff");
            var guid = Guid.NewGuid().ToString().Replace("-", string.Empty);
            id += guid.Substring(0, 10);
            return id;
        }
    }

    /// <summary>
    /// Gets a new identifier based on the current date and time.
    /// </summary>
    [Obsolete("Use NewId instead. This method generate the same Id in same seconds")]
    public static string NewDateTimeId
    {
        get
        {
            var id = DateTime.Now.ToString("yyyyMMddHHmmssfffffff");
            return id;
        }
    }

    /// <summary>
    /// Gets a new unique identifier based on the current date and a GUID.
    /// </summary>
    public static string NewDateGuid
    {
        get
        {
            var id = DateTime.Now.ToString("yyMMdd");
            var guid = Guid.NewGuid().ToString().Replace("-", string.Empty);
            id += guid.Substring(0, 4);
            return id;
        }
    }

    /// <summary>
    /// Gets a new GUID.
    /// </summary>
    /// <returns>A new GUID.</returns>
    public static Guid NewGuid()
    {
        return Guid.NewGuid();
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
    /// Gets a new unique 64-bit integer based on a GUID.
    /// </summary>
    /// <returns>A new unique 64-bit integer.</returns>
    public static long GetInt64UniqueCode()
    {
        var value = Guid.NewGuid();
        return value.ToInt64();
    }

    /// <summary>
    /// Gets a new unique 32-bit integer based on a GUID.
    /// </summary>
    /// <returns>A new unique 32-bit integer.</returns>
    public static int GetInt32UniqueCode()
    {
        var value = Guid.NewGuid();
        return value.ToInt32();
    }
}
