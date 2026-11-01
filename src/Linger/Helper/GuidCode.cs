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
}
