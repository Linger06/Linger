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
    public static string NewDateGuid
    {
        get
        {
            string dateStr = DateTime.Now.ToString("yyMMdd", CultureInfo.InvariantCulture);

            // 1. 生成标准的 GUID
            Guid guid = Guid.NewGuid();

            // 2. 导出其 16 字节的原始数组
            byte[] bytes = guid.ToByteArray();

            // 3. 提取前 4 个字节，转换为一个正整数
            int randomInt = BitConverter.ToInt32(bytes, 0) & int.MaxValue;

            // 4. 取模限制在 4 位 36 进制范围内 (36^4 = 1679616)
            int seed = randomInt % 1679616;
            // 5. 转换为 4 位 36 进制字符串
            char[] keys = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            char[] buffer = new char[4];
            for (int i = 3; i >= 0; i--)
            {
                buffer[i] = keys[seed % 36];
                seed /= 36;
            }
            return dateStr + new string(buffer);
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
    [Obsolete("This 64-bit value is not guaranteed to be unique. Use a full Guid instead. This API will be removed in 2.0.0.")]
    public static long GetInt64UniqueCode()
    {
        var value = Guid.NewGuid();
        return value.ToInt64();
    }

    /// <summary>
    /// Gets a 32-bit code based on part of a GUID.
    /// </summary>
    /// <returns>A 32-bit integer that is not guaranteed to be unique.</returns>
    [Obsolete("This 32-bit value is not guaranteed to be unique. Use a full Guid instead. This API will be removed in 2.0.0.")]
    public static int GetInt32UniqueCode()
    {
        var value = Guid.NewGuid();
        return value.ToInt32();
    }
}
