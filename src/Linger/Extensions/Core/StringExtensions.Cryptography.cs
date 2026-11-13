using System.Security.Cryptography;
using System.Text;

namespace Linger.Extensions.Core;

/// <summary>
/// 字符串加密解密扩展方法
/// </summary>
public static partial class StringExtensions
{
    // .NET Standard 2.0 的编译期 API 没有 PBKDF2-HMAC-SHA256，只能靠反射或手写实现补齐；为避免这两者，该目标框架不提供加密 API
#if !NETSTANDARD2_0
    private static readonly byte[] s_aesPayloadMagicV2 = { 0x4C, 0x4E, 0x47, 0x32 }; // LNG2
    private static readonly byte[] s_aesPayloadMagicV3 = { 0x4C, 0x4E, 0x47, 0x33 }; // LNG3
    private static readonly byte[] s_aesPayloadMagicV4 = { 0x4C, 0x4E, 0x47, 0x34 }; // LNG4
    private const int AesSaltSize = 16;
    private const int AesIvSize = 16;
    private const int AesTagSize = 32;
    private const int AesDerivedKeySize = 64;
    private const int AesIterationFieldSize = sizeof(int);

    // V2/V3 没有在密文中记录迭代次数，固定使用该值；新加密写入 AesPbkdf2Iterations（V4）
    private const int AesLegacyPbkdf2Iterations = 100_000;
    private const int AesPbkdf2Iterations = 600_000;

    // 解密 V4 时接受的迭代次数范围：下限防止被降级，上限限制恶意头部造成的 CPU 消耗
    private const int AesMinAcceptedIterations = 100_000;
    private const int AesMaxAcceptedIterations = 10_000_000;

    // DES 加密方法已被移除，因为 DES 算法不安全。
    // 新代码应使用 AesEncryptAuthenticated 和 AesDecryptAuthenticated。

    /// <summary>
    /// Encrypts a string using PBKDF2-HMAC-SHA256 (600,000 iterations, recorded in the payload header), AES-256-CBC, and HMAC-SHA256 authentication.
    /// </summary>
    /// <param name="input">The plaintext to encrypt.</param>
    /// <param name="key">The password or key material used to derive encryption and authentication keys.</param>
    /// <returns>A Base64-encoded, versioned authenticated payload.</returns>
    public static string AesEncryptAuthenticated(this string input, string key)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("The input text cannot be null or empty.", nameof(input));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("The key cannot be null or empty.", nameof(key));

        try
        {
            return Convert.ToBase64String(EncryptAuthenticated(input, key));
        }
        catch (Exception ex) when (ex is not CryptographicException)
        {
            throw new CryptographicException($"AES authenticated encryption failed: {ex.Message}", ex);
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
    /// 旧格式没有完整性校验，对不可信输入使用本方法存在填充预言攻击风险；
    /// 解密 <see cref="AesEncryptAuthenticated"/> 的结果请使用 <see cref="AesDecryptAuthenticated"/>，
    /// 本方法仅用于迁移旧密文。
    /// </remarks>
    /// <example>
    /// <code>
    /// // 仅在迁移旧密文时使用：解密后立即用认证格式重新加密
    /// string key = "mySecretKey";
    /// string legacyCiphertext = "base64_encrypted_string_with_iv";
    /// string plaintext = legacyCiphertext.AesDecrypt(key);
    /// string migrated = plaintext.AesEncryptAuthenticated(key);
    /// </code>
    /// </example>
    [Obsolete("AesDecrypt falls back to the unauthenticated legacy format. Use AesDecryptAuthenticated for payloads produced by AesEncryptAuthenticated; use AesDecrypt only to migrate legacy ciphertext.")]
    public static string AesDecrypt(this string encryptedInput, string key)
    {
        if (string.IsNullOrEmpty(encryptedInput))
            throw new ArgumentException("The encrypted text cannot be null or empty.", nameof(encryptedInput));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("The key cannot be null or empty.", nameof(key));

        try
        {
            byte[] fullCipher = Convert.FromBase64String(encryptedInput);

            if (TryGetAuthenticatedPayloadVersion(fullCipher, out AesPayloadVersion version))
            {
                return DecryptAuthenticated(fullCipher, key, version);
            }

            return DecryptLegacy(fullCipher, key);
        }
        catch (FormatException ex)
        {
            throw new CryptographicException("The encrypted text is not valid Base64.", ex);
        }
        catch (Exception ex) when (ex is not CryptographicException)
        {
            throw new CryptographicException($"AES decryption failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Decrypts and authenticates a payload produced by <see cref="AesEncryptAuthenticated"/>.
    /// </summary>
    public static string AesDecryptAuthenticated(this string encryptedInput, string key)
    {
        if (string.IsNullOrEmpty(encryptedInput))
            throw new ArgumentException("The encrypted text cannot be null or empty.", nameof(encryptedInput));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("The key cannot be null or empty.", nameof(key));

        try
        {
            byte[] payload = Convert.FromBase64String(encryptedInput);
            if (!TryGetAuthenticatedPayloadVersion(payload, out AesPayloadVersion version))
            {
                throw new CryptographicException("The encrypted data is not in a supported authenticated format.");
            }

            return DecryptAuthenticated(payload, key, version);
        }
        catch (FormatException ex)
        {
            throw new CryptographicException("The encrypted text is not valid Base64.", ex);
        }
        catch (Exception ex) when (ex is not CryptographicException)
        {
            throw new CryptographicException($"AES authenticated decryption failed: {ex.Message}", ex);
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

        byte[] keyMaterial = DeriveAesKeys(password, salt, AesPayloadVersion.V4, AesPbkdf2Iterations);
        try
        {
            using var aes = Aes.Create();
            aes.Key = CopyRange(keyMaterial, 0, 32);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            byte[] encrypted = PerformEncryptionWithIV(aes, input);

            // 头部布局：魔数 | 迭代次数（大端 int32） | 盐 | IV，随后是密文和 HMAC 标签；标签同样覆盖迭代次数
            int saltOffset = s_aesPayloadMagicV4.Length + AesIterationFieldSize;
            int ivOffset = saltOffset + salt.Length;
            int encryptedOffset = ivOffset + aes.IV.Length;
            int tagOffset = encryptedOffset + encrypted.Length;
            byte[] payload = new byte[tagOffset + AesTagSize];

            Buffer.BlockCopy(s_aesPayloadMagicV4, 0, payload, 0, s_aesPayloadMagicV4.Length);
            WriteInt32BigEndian(payload, s_aesPayloadMagicV4.Length, AesPbkdf2Iterations);
            Buffer.BlockCopy(salt, 0, payload, saltOffset, salt.Length);
            Buffer.BlockCopy(aes.IV, 0, payload, ivOffset, aes.IV.Length);
            Buffer.BlockCopy(encrypted, 0, payload, encryptedOffset, encrypted.Length);

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

    private static string DecryptAuthenticated(byte[] payload, string password, AesPayloadVersion version)
    {
        var iterations = AesLegacyPbkdf2Iterations;
        var saltOffset = s_aesPayloadMagicV3.Length;
        if (version == AesPayloadVersion.V4)
        {
            iterations = ReadInt32BigEndian(payload, s_aesPayloadMagicV4.Length);

            // 迭代次数来自尚未认证的头部：先限定范围，避免恶意数据让密钥派生长时间占用 CPU
            if (iterations is < AesMinAcceptedIterations or > AesMaxAcceptedIterations)
            {
                throw new CryptographicException($"The encrypted data declares an unsupported PBKDF2 iteration count ({iterations}).");
            }

            saltOffset += AesIterationFieldSize;
        }

        int ivOffset = saltOffset + AesSaltSize;
        int encryptedOffset = ivOffset + AesIvSize;
        int tagOffset = payload.Length - AesTagSize;
        int encryptedLength = tagOffset - encryptedOffset;

        byte[] salt = CopyRange(payload, saltOffset, AesSaltSize);
        byte[] keyMaterial = DeriveAesKeys(password, salt, version, iterations);
        try
        {
            using var hmac = new HMACSHA256(CopyRange(keyMaterial, 32, 32));
            byte[] expectedTag = hmac.ComputeHash(payload, 0, tagOffset);
            byte[] actualTag = CopyRange(payload, tagOffset, AesTagSize);
            if (!FixedTimeEquals(expectedTag, actualTag))
            {
                throw new CryptographicException("Authentication of the encrypted data failed: the key is wrong or the data was tampered with.");
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
            throw new CryptographicException("The encrypted data is invalid: it is too short.");
        }

        using var aes = Aes.Create();
        aes.Key = key.ToSha256HashByte();
        aes.IV = CopyRange(fullCipher, 0, AesIvSize);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        return PerformDecryptionWithData(aes, CopyRange(fullCipher, AesIvSize, fullCipher.Length - AesIvSize));
    }

    private static byte[] DeriveAesKeys(string password, byte[] salt, AesPayloadVersion version, int iterations)
    {
        if (version == AesPayloadVersion.V2)
        {
#pragma warning disable SYSLIB0041, SYSLIB0060 // LNG2 compatibility requires the historical PBKDF2-SHA1 format.
            using var legacyDeriveBytes = new Rfc2898DeriveBytes(password, salt, iterations);
#pragma warning restore SYSLIB0041, SYSLIB0060
            return legacyDeriveBytes.GetBytes(AesDerivedKeySize);
        }

#if NET10_0_OR_GREATER
        return Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            AesDerivedKeySize);
#else
        using var deriveBytes = new Rfc2898DeriveBytes(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256);
        return deriveBytes.GetBytes(AesDerivedKeySize);
#endif
    }

    private static bool TryGetAuthenticatedPayloadVersion(byte[] payload, out AesPayloadVersion version)
    {
        // 盐、IV、至少一个 AES 块的密文和标签；V4 头部在魔数之后多一个迭代次数字段
        int minimumBodyLength = AesSaltSize + AesIvSize + 16 + AesTagSize;

        if (payload.Length >= s_aesPayloadMagicV4.Length + AesIterationFieldSize + minimumBodyLength
            && HasMagic(payload, s_aesPayloadMagicV4))
        {
            version = AesPayloadVersion.V4;
            return true;
        }

        if (payload.Length >= s_aesPayloadMagicV3.Length + minimumBodyLength)
        {
            if (HasMagic(payload, s_aesPayloadMagicV3))
            {
                version = AesPayloadVersion.V3;
                return true;
            }

            if (HasMagic(payload, s_aesPayloadMagicV2))
            {
                version = AesPayloadVersion.V2;
                return true;
            }
        }

        version = default;
        return false;
    }

    private static bool HasMagic(byte[] payload, byte[] magic)
    {
        for (var i = 0; i < magic.Length; i++)
        {
            if (payload[i] != magic[i])
            {
                return false;
            }
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

    private static void WriteInt32BigEndian(byte[] destination, int offset, int value)
    {
        destination[offset] = (byte)(value >> 24);
        destination[offset + 1] = (byte)(value >> 16);
        destination[offset + 2] = (byte)(value >> 8);
        destination[offset + 3] = (byte)value;
    }

    private static int ReadInt32BigEndian(byte[] source, int offset)
    {
        return (source[offset] << 24) | (source[offset + 1] << 16) | (source[offset + 2] << 8) | source[offset + 3];
    }

    private static byte[] CopyRange(byte[] source, int offset, int count)
    {
        var result = new byte[count];
        Buffer.BlockCopy(source, offset, result, 0, count);
        return result;
    }

    private enum AesPayloadVersion
    {
        V2,
        V3,
        V4
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
#endif
}
