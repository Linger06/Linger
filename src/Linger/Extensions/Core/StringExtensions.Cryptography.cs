using System.Security.Cryptography;
using System.Text;

namespace Linger.Extensions.Core;

/// <summary>
/// 字符串加密解密扩展方法
/// </summary>
public static partial class StringExtensions
{
    private static readonly byte[] s_aesPayloadMagic = { 0x4C, 0x4E, 0x47, 0x32 }; // LNG2
    private const int AesSaltSize = 16;
    private const int AesIvSize = 16;
    private const int AesTagSize = 32;
    private const int AesDerivedKeySize = 64;
    private const int AesPbkdf2Iterations = 100_000;

    // DES 加密方法已被移除，因为 DES 算法不安全。
    // 新代码应使用 AesEncryptAuthenticated 和 AesDecryptAuthenticated。

    /// <summary>
    /// 使用旧版 AES-256-CBC 格式加密字符串，以兼容已有密文使用方。
    /// </summary>
    /// <param name="input">要加密的字符串</param>
    /// <param name="key">密钥字符串（任意长度，将通过 SHA-256 处理为 32 字节）。</param>
    /// <returns>Base64 编码的旧版加密结果（包含 IV）。</returns>
    /// <exception cref="ArgumentException">当输入参数为null或空时抛出</exception>
    /// <exception cref="CryptographicException">当加密操作失败时抛出</exception>
    /// <remarks>
    /// 此格式不提供完整性认证，仅用于兼容。新代码应使用 <see cref="AesEncryptAuthenticated"/>。
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
            throw new ArgumentException("输入文本不能为null或空字符串", nameof(input));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("密钥不能为null或空字符串", nameof(key));
        try
        {
            using var aes = Aes.Create();
            aes.Key = key.ToSha256HashByte();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            byte[] encrypted = PerformEncryptionWithIV(aes, input);
            byte[] result = new byte[aes.IV.Length + encrypted.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);
            return Convert.ToBase64String(result);
        }
        catch (Exception ex)
        {
            throw new CryptographicException($"AES加密失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Encrypts a string using PBKDF2, AES-256-CBC, and HMAC-SHA256 authentication.
    /// </summary>
    /// <param name="input">The plaintext to encrypt.</param>
    /// <param name="key">The password or key material used to derive encryption and authentication keys.</param>
    /// <returns>A Base64-encoded, versioned authenticated payload.</returns>
    public static string AesEncryptAuthenticated(this string input, string key)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("输入文本不能为null或空字符串", nameof(input));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("密钥不能为null或空字符串", nameof(key));

        try
        {
            return Convert.ToBase64String(EncryptAuthenticated(input, key));
        }
        catch (Exception ex) when (ex is not CryptographicException)
        {
            throw new CryptographicException($"AES认证加密失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 解密旧版 AES-256-CBC 密文或当前认证格式密文。
    /// </summary>
    /// <param name="encryptedInput">Base64 编码的加密字符串（包含IV）</param>
    /// <param name="key">密钥字符串（任意长度）</param>
    /// <returns>解密后的原始字符串</returns>
    /// <exception cref="ArgumentException">当输入参数为null或空时抛出</exception>
    /// <exception cref="CryptographicException">当解密操作失败时抛出</exception>
    /// <remarks>
    /// 自动识别当前认证格式，也兼容解密旧版仅包含 IV 和密文的结果。
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
            throw new ArgumentException("加密文本不能为null或空字符串", nameof(encryptedInput));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("密钥不能为null或空字符串", nameof(key));

        try
        {
            byte[] fullCipher = Convert.FromBase64String(encryptedInput);

            if (IsAuthenticatedPayload(fullCipher))
            {
                return DecryptAuthenticated(fullCipher, key);
            }

            return DecryptLegacy(fullCipher, key);
        }
        catch (FormatException ex)
        {
            throw new CryptographicException("无效的Base64编码格式", ex);
        }
        catch (Exception ex) when (ex is not CryptographicException)
        {
            throw new CryptographicException($"AES解密失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Decrypts and authenticates a payload produced by <see cref="AesEncryptAuthenticated"/>.
    /// </summary>
    public static string AesDecryptAuthenticated(this string encryptedInput, string key)
    {
        if (string.IsNullOrEmpty(encryptedInput))
            throw new ArgumentException("加密文本不能为null或空字符串", nameof(encryptedInput));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("密钥不能为null或空字符串", nameof(key));

        try
        {
            byte[] payload = Convert.FromBase64String(encryptedInput);
            if (!IsAuthenticatedPayload(payload))
            {
                throw new CryptographicException("加密数据不是受支持的认证格式。");
            }

            return DecryptAuthenticated(payload, key);
        }
        catch (FormatException ex)
        {
            throw new CryptographicException("无效的Base64编码格式", ex);
        }
        catch (Exception ex) when (ex is not CryptographicException)
        {
            throw new CryptographicException($"AES认证解密失败: {ex.Message}", ex);
        }
    }

    #region Private Helper Methods

    private static byte[] EncryptAuthenticated(string input, string password)
    {
        byte[] salt = new byte[AesSaltSize];
        using (var random = RandomNumberGenerator.Create())
        {
            random.GetBytes(salt);
        }

        byte[] keyMaterial = DeriveAesKeys(password, salt);
        try
        {
            using var aes = Aes.Create();
            aes.Key = CopyRange(keyMaterial, 0, 32);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            byte[] encrypted = PerformEncryptionWithIV(aes, input);
            int tagOffset = s_aesPayloadMagic.Length + salt.Length + aes.IV.Length + encrypted.Length;
            byte[] payload = new byte[tagOffset + AesTagSize];

            Buffer.BlockCopy(s_aesPayloadMagic, 0, payload, 0, s_aesPayloadMagic.Length);
            Buffer.BlockCopy(salt, 0, payload, s_aesPayloadMagic.Length, salt.Length);
            Buffer.BlockCopy(aes.IV, 0, payload, s_aesPayloadMagic.Length + salt.Length, aes.IV.Length);
            Buffer.BlockCopy(encrypted, 0, payload, s_aesPayloadMagic.Length + salt.Length + aes.IV.Length, encrypted.Length);

            using var hmac = new HMACSHA256(CopyRange(keyMaterial, 32, 32));
            byte[] tag = hmac.ComputeHash(payload, 0, tagOffset);
            Buffer.BlockCopy(tag, 0, payload, tagOffset, tag.Length);
            return payload;
        }
        finally
        {
            Array.Clear(keyMaterial, 0, keyMaterial.Length);
        }
    }

    private static string DecryptAuthenticated(byte[] payload, string password)
    {
        int saltOffset = s_aesPayloadMagic.Length;
        int ivOffset = saltOffset + AesSaltSize;
        int encryptedOffset = ivOffset + AesIvSize;
        int tagOffset = payload.Length - AesTagSize;
        int encryptedLength = tagOffset - encryptedOffset;

        byte[] salt = CopyRange(payload, saltOffset, AesSaltSize);
        byte[] keyMaterial = DeriveAesKeys(password, salt);
        try
        {
            using var hmac = new HMACSHA256(CopyRange(keyMaterial, 32, 32));
            byte[] expectedTag = hmac.ComputeHash(payload, 0, tagOffset);
            byte[] actualTag = CopyRange(payload, tagOffset, AesTagSize);
            if (!FixedTimeEquals(expectedTag, actualTag))
            {
                throw new CryptographicException("加密数据认证失败，密钥错误或数据已被篡改。");
            }

            using var aes = Aes.Create();
            aes.Key = CopyRange(keyMaterial, 0, 32);
            aes.IV = CopyRange(payload, ivOffset, AesIvSize);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            return PerformDecryptionWithData(aes, CopyRange(payload, encryptedOffset, encryptedLength));
        }
        finally
        {
            Array.Clear(keyMaterial, 0, keyMaterial.Length);
        }
    }

    private static string DecryptLegacy(byte[] fullCipher, string key)
    {
        if (fullCipher.Length < AesIvSize)
        {
            throw new CryptographicException("加密数据格式无效：长度不足");
        }

        using var aes = Aes.Create();
        aes.Key = key.ToSha256HashByte();
        aes.IV = CopyRange(fullCipher, 0, AesIvSize);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        return PerformDecryptionWithData(aes, CopyRange(fullCipher, AesIvSize, fullCipher.Length - AesIvSize));
    }

    private static byte[] DeriveAesKeys(string password, byte[] salt)
    {
#pragma warning disable SYSLIB0041, SYSLIB0060 // Required for one payload format across all target frameworks.
        using var deriveBytes = new Rfc2898DeriveBytes(
            password,
            salt,
            AesPbkdf2Iterations);
#pragma warning restore SYSLIB0041, SYSLIB0060
        return deriveBytes.GetBytes(AesDerivedKeySize);
    }

    private static bool IsAuthenticatedPayload(byte[] payload)
    {
        int minimumLength = s_aesPayloadMagic.Length + AesSaltSize + AesIvSize + 16 + AesTagSize;
        if (payload.Length < minimumLength)
            return false;

        for (var i = 0; i < s_aesPayloadMagic.Length; i++)
        {
            if (payload[i] != s_aesPayloadMagic[i])
                return false;
        }

        return true;
    }

    private static bool FixedTimeEquals(byte[] left, byte[] right)
    {
        if (left.Length != right.Length)
            return false;

        var difference = 0;
        for (var i = 0; i < left.Length; i++)
        {
            difference |= left[i] ^ right[i];
        }

        return difference == 0;
    }

    private static byte[] CopyRange(byte[] source, int offset, int count)
    {
        var result = new byte[count];
        Buffer.BlockCopy(source, offset, result, 0, count);
        return result;
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
