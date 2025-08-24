using System.Security.Cryptography;
using System.Text;

namespace Linger.Extensions.Core;

/// <summary>
/// 字符串加密解密扩展方法
/// </summary>
public static partial class StringExtensions
{
    // DES 加密方法已被移除，因为 DES 算法不安全。
    // 请使用 AesEncrypt 和 AesDecrypt 方法进行安全的加密操作。

    /// <summary>
    /// 使用 AES-256-CBC 算法加密字符串（推荐）
    /// </summary>
    /// <param name="input">要加密的字符串</param>
    /// <param name="key">密钥字符串（任意长度，将自动处理为32字节）</param>
    /// <returns>Base64 编码的加密结果（包含IV）</returns>
    /// <exception cref="ArgumentException">当输入参数为null或空时抛出</exception>
    /// <exception cref="CryptographicException">当加密操作失败时抛出</exception>
    /// <remarks>
    /// 使用 AES-256-CBC 模式，自动生成随机 IV 并包含在结果中，确保每次加密结果都不同
    /// </remarks>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     string plainText = "Hello World";
    ///     string key = "mySecretKey"; // 任意长度密钥
    ///     string encrypted = plainText.AesEncrypt(key);
    ///     Console.WriteLine($"加密结果: {encrypted}");
    ///     
    ///     // 解密
    ///     string decrypted = encrypted.AesDecrypt(key);
    ///     Console.WriteLine($"解密结果: {decrypted}"); // 输出: Hello World
    /// }
    /// catch (ArgumentException ex)
    /// {
    ///     Console.WriteLine($"参数错误: {ex.Message}");
    /// }
    /// catch (CryptographicException ex)
    /// {
    ///     Console.WriteLine($"加密失败: {ex.Message}");
    /// }
    /// </code>
    /// </example>
    public static string AesEncrypt(this string input, string key)
    {
        if (string.IsNullOrEmpty(input))
            throw new System.ArgumentException("输入文本不能为null或空字符串", nameof(input));

        if (string.IsNullOrEmpty(key))
            throw new System.ArgumentException("密钥不能为null或空字符串", nameof(key));
        try
        {
            using var aes = Aes.Create();

            // 使用 SHA256 哈希确保密钥长度为 32 字节（AES-256）
            aes.Key = ComputeKeyHash(key);
            aes.Mode = CipherMode.CBC; // 使用更安全的 CBC 模式
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV(); // 生成随机 IV

            byte[] encrypted = PerformEncryptionWithIV(aes, input);

            // 将 IV 和加密数据组合：前16字节是IV，后面是加密数据
            byte[] result = new byte[aes.IV.Length + encrypted.Length];
            Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
            Array.Copy(encrypted, 0, result, aes.IV.Length, encrypted.Length);

            return Convert.ToBase64String(result);
        }
        catch (Exception ex)
        {
            throw new CryptographicException($"AES加密失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 使用 AES-256-CBC 算法解密字符串（推荐）
    /// </summary>
    /// <param name="encryptedInput">Base64 编码的加密字符串（包含IV）</param>
    /// <param name="key">密钥字符串（任意长度）</param>
    /// <returns>解密后的原始字符串</returns>
    /// <exception cref="ArgumentException">当输入参数为null或空时抛出</exception>
    /// <exception cref="CryptographicException">当解密操作失败时抛出</exception>
    /// <remarks>
    /// 自动从加密数据中提取 IV 并解密，与 AesEncrypt 方法配对使用
    /// </remarks>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     string encryptedText = "base64_encrypted_string_with_iv";
    ///     string key = "mySecretKey"; // 任意长度密钥
    ///     string decrypted = encryptedText.AesDecrypt(key);
    ///     Console.WriteLine($"解密结果: {decrypted}");
    /// }
    /// catch (ArgumentException ex)
    /// {
    ///     Console.WriteLine($"参数错误: {ex.Message}");
    /// }
    /// catch (CryptographicException ex)
    /// {
    ///     Console.WriteLine($"解密失败: {ex.Message}");
    /// }
    /// </code>
    /// </example>
    public static string AesDecrypt(this string encryptedInput, string key)
    {
        if (string.IsNullOrEmpty(encryptedInput))
            throw new System.ArgumentException("加密文本不能为null或空字符串", nameof(encryptedInput));

        if (string.IsNullOrEmpty(key))
            throw new System.ArgumentException("密钥不能为null或空字符串", nameof(key));

        try
        {
            byte[] fullCipher = Convert.FromBase64String(encryptedInput);

            // AES-256 的 IV 长度是 16 字节
            if (fullCipher.Length < 16)
            {
                throw new CryptographicException("加密数据格式无效：长度不足");
            }

            using var aes = Aes.Create();

            // 使用相同的密钥处理方法
            aes.Key = ComputeKeyHash(key);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // 提取 IV（前16字节）和加密数据（剩余字节）
            byte[] iv = new byte[16];
            byte[] encrypted = new byte[fullCipher.Length - 16];
            Array.Copy(fullCipher, 0, iv, 0, 16);
            Array.Copy(fullCipher, 16, encrypted, 0, encrypted.Length);

            aes.IV = iv;

            return PerformDecryptionWithData(aes, encrypted);
        }
        catch (FormatException ex)
        {
            throw new CryptographicException("无效的Base64编码格式", ex);
        }
        catch (Exception ex) when (!(ex is CryptographicException))
        {
            throw new CryptographicException($"AES解密失败: {ex.Message}", ex);
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// 计算密钥的SHA256哈希（缓存优化）
    /// </summary>
    private static byte[] ComputeKeyHash(string key)
    {
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
    }

    /// <summary>
    /// 为AES执行加密操作（返回字节数组）
    /// </summary>
    private static byte[] PerformEncryptionWithIV(SymmetricAlgorithm algorithm, string input)
    {
        using var encryptor = algorithm.CreateEncryptor();
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        return encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
    }

    /// <summary>
    /// 为AES执行解密操作（使用字节数组）
    /// </summary>
    private static string PerformDecryptionWithData(SymmetricAlgorithm algorithm, byte[] encryptedData)
    {
        using var decryptor = algorithm.CreateDecryptor();
        byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    #endregion
}
