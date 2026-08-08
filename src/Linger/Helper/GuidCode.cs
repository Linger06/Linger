using Linger.Extensions.Core;

namespace Linger.Helper;

/// <summary>
/// Provides methods to generate identifier values.
/// </summary>
public static class GuidCode
{
    /// <summary>
    /// Gets a new identifier based on the current date and time and a GUID suffix.
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

#if NET9_0_OR_GREATER
    /// <summary>
    /// Creates a new version 7 GUID.
    /// </summary>
    /// <returns>A new version 7 GUID.</returns>
    [Obsolete("Use Guid.CreateVersion7() instead.")]
    public static Guid CreateVersion7()
    {
        return Guid.CreateVersion7();
    }
#endif

}
