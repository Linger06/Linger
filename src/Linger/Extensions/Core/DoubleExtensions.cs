namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="double"/> extensions
/// </summary>
public static class DoubleExtensions
{
    /// <summary>
    /// Returns the size corresponding to the ContentLength.
    /// </summary>
    /// <param name="contentLength">The length of the ContentLength.</param>
    /// <returns>The size in Bytes/KB/MB/GB/TB/PB.</returns>
    public static string FormatFileSize(this double contentLength)
    {
        ReadOnlySpan<string> units = ["Bytes", "KB", "MB", "GB", "TB", "PB"];
        const double Mod = 1024.0;
        var i = 0;
        // 先取整再判断进位：否则 1023.6KB 取整后会显示成 "1024KB"，而不是 "1MB"
        while (i < units.Length - 1 && Math.Round(contentLength, MidpointRounding.AwayFromZero) >= Mod)
        {
            contentLength /= Mod;
            i++;
        }

        // 与 DecimalExtensions.Round 保持一致；默认的银行家舍入会把 2.5KB 显示成 "2KB"
        return $"{Math.Round(contentLength, MidpointRounding.AwayFromZero)}{units[i]}";
    }

}
