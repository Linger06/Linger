using System.Security.Cryptography;
using System.Text;
using Linger.Extensions.Core;

namespace Linger.UnitTests.Extensions.Core;

public class StringExtensionsCryptographyTests
{
    #region Test Data

    // DES test data has been removed as DES encryption methods are no longer supported
    // due to security concerns. All tests now focus on secure AES encryption.

    public static IEnumerable<object[]> ValidAESTestData =>
        new List<object[]>
        {
            new object[] { "Hello World", "mySecretKey12345" },
            new object[] { "测试中文AES加密", "中文密钥测试1234567890" },
            new object[] { "Special chars: !@#$%^&*()", "SpecialKey!@#$%" },
            new object[] { "Numbers: 1234567890", "NumericKey123456" },
            new object[] { "A", "ShortKey" },
            new object[] { "Very long text that needs to be encrypted using AES algorithm with CBC mode", "VeryLongSecretKeyForTesting123456789" }
        };

    #endregion

    // DES encryption and decryption tests have been removed because DES algorithms 
    // are no longer supported due to security vulnerabilities.
    // All encryption tests now focus on secure AES algorithms.

    #region AES Encryption Tests

    [Theory]
    [MemberData(nameof(ValidAESTestData))]
    public void AesEncryptAuthenticated_ValidInputs_ReturnsEncryptedString(string data, string key)
    {
        // Act
        string encrypted = data.AesEncryptAuthenticated(key);

        // Assert
        Assert.NotNull(encrypted);
        Assert.NotEmpty(encrypted);
        Assert.NotEqual(data, encrypted);

        // 验证是否为有效的 Base64 字符串
        var bytes = Convert.FromBase64String(encrypted);
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);

        // AES-CBC 模式，加密结果应该包含IV（至少16字节）+ 加密数据
        Assert.True(bytes.Length >= 16, "加密结果应该包含IV和加密数据");
    }

    [Theory]
    [InlineData(null, "validkey")]
    [InlineData("", "validkey")]
    [InlineData("validdata", null)]
    [InlineData("validdata", "")]
    public void AesEncryptAuthenticated_InvalidParameters_ThrowsArgumentException(string data, string key)
    {
        // Act & Assert
        Assert.Throws<System.ArgumentException>(() => data.AesEncryptAuthenticated(key));
    }

    [Fact]
    public void AesEncryptAuthenticated_SameInputs_ProducesDifferentResults()
    {
        // Arrange
        string data = "Test Data";
        string key = "TestKey123";

        // Act
        string encrypted1 = data.AesEncryptAuthenticated(key);
        string encrypted2 = data.AesEncryptAuthenticated(key);

        // Assert
        // AES with random IV should produce different encrypted results each time
        Assert.NotEqual(encrypted1, encrypted2);
    }

    [Fact]
    public void AesEncryptAuthenticated_DifferentKeys_ProduceDifferentResults()
    {
        // Arrange
        string data = "Test Data";

        // Act
        string encrypted1 = data.AesEncryptAuthenticated("Key1");
        string encrypted2 = data.AesEncryptAuthenticated("Key2");

        // Assert
        Assert.NotEqual(encrypted1, encrypted2);
    }

    #endregion

    #region AES Decryption Tests

    [Theory]
    [MemberData(nameof(ValidAESTestData))]
    public void AesDecrypt_ValidEncryptedData_ReturnsOriginalString(string originalData, string key)
    {
        // Arrange
        string encrypted = originalData.AesEncryptAuthenticated(key);

        // Act
        string decrypted = encrypted.AesDecrypt(key);

        // Assert
        Assert.Equal(originalData, decrypted);
    }

    [Fact]
    public void AesDecrypt_InvalidBase64_ThrowsCryptographicException()
    {
        // Arrange
        string invalidBase64 = "This is not a valid base64 string!";
        string key = "TestKey123";

        // Act & Assert
        Assert.Throws<CryptographicException>(() => invalidBase64.AesDecrypt(key));
    }

    [Theory]
    [InlineData(null, "validkey")]
    [InlineData("", "validkey")]
    [InlineData("validdata", null)]
    [InlineData("validdata", "")]
    public void AesDecrypt_InvalidParameters_ThrowsArgumentException(string data, string key)
    {
        // Act & Assert
        Assert.Throws<System.ArgumentException>(() => data.AesDecrypt(key));
    }

    [Fact]
    public void AesDecrypt_WrongKey_ThrowsCryptographicException()
    {
        // Arrange
        string data = "Hello World";
        string correctKey = "CorrectKey123";
        string wrongKey = "WrongKey456";

        string encrypted = data.AesEncryptAuthenticated(correctKey);

        // Act & Assert
        Assert.Throws<CryptographicException>(() => encrypted.AesDecryptAuthenticated(wrongKey));
    }

    [Fact]
    public void AesDecrypt_TooShortEncryptedData_ThrowsCryptographicException()
    {
        // Arrange
        string shortData = Convert.ToBase64String(new byte[8]); // 小于16字节的IV长度
        string key = "TestKey123";

        // Act & Assert
        Assert.Throws<CryptographicException>(() => shortData.AesDecrypt(key));
    }

    [Fact]
    public void AesDecrypt_TamperedAuthenticatedData_ThrowsCryptographicException()
    {
        string encrypted = "Sensitive data".AesEncryptAuthenticated("TestKey123");
        byte[] payload = Convert.FromBase64String(encrypted);
        payload[40] ^= 1;

        string tampered = Convert.ToBase64String(payload);

        Assert.Throws<CryptographicException>(() => tampered.AesDecryptAuthenticated("TestKey123"));
    }

    [Fact]
    public void AesAuthenticatedEncryptDecrypt_ReturnsOriginalString()
    {
        const string original = "Authenticated encrypted value";
        const string key = "AuthenticatedKey123";

        string encrypted = original.AesEncryptAuthenticated(key);
        string decrypted = encrypted.AesDecryptAuthenticated(key);

        Assert.Equal(original, decrypted);
        Assert.Equal(new byte[] { 0x4C, 0x4E, 0x47, 0x33 }, Convert.FromBase64String(encrypted).Take(4).ToArray());
    }

    [Fact]
    public void AesDecryptAuthenticated_V2Payload_ReturnsOriginalString()
    {
        const string original = "Version 2 authenticated value";
        const string key = "Version2Key123";
        string payload = CreateAuthenticatedV2Payload(original, key);

        string decrypted = payload.AesDecryptAuthenticated(key);

        Assert.Equal(original, decrypted);
    }

    [Fact]
    public void AesDecrypt_LegacyPayload_ReturnsOriginalString()
    {
        const string original = "Legacy encrypted value";
        const string key = "LegacyKey123";
        string legacyPayload = CreateLegacyPayload(original, key);

        string decrypted = legacyPayload.AesDecrypt(key);

        Assert.Equal(original, decrypted);
    }

    #endregion

    #region Edge Cases and Integration Tests

    [Fact]
    public void AesEncryptAuthenticatedDecrypt_EmptyString_WorksCorrectly()
    {
        // Note: Empty string encryption is not supported and should throw ArgumentException
        // This test ensures the behavior is consistent

        // Arrange
        string data = "";
        string key = "TestKey123";

        // Act & Assert
        Assert.Throws<System.ArgumentException>(() => data.AesEncryptAuthenticated(key));
    }

    [Fact]
    public void AesEncryptAuthenticatedDecrypt_SingleCharacter_WorksCorrectly()
    {
        // Arrange
        string data = "A";
        string key = "TestKey123";

        // Act
        string encrypted = data.AesEncryptAuthenticated(key);
        string decrypted = encrypted.AesDecrypt(key);

        // Assert
        Assert.Equal(data, decrypted);
        Assert.NotEqual(data, encrypted);
    }

    [Fact]
    public void AesEncryptAuthenticatedDecrypt_LongString_WorksCorrectly()
    {
        // Arrange
        string data = new string('A', 10000); // 10KB of data
        string key = "TestKey123";

        // Act
        string encrypted = data.AesEncryptAuthenticated(key);
        string decrypted = encrypted.AesDecrypt(key);

        // Assert
        Assert.Equal(data, decrypted);
        Assert.NotEqual(data, encrypted);
    }

    [Fact]
    public void AesEncryptAuthenticatedDecrypt_UnicodeCharacters_WorksCorrectly()
    {
        // Arrange
        string unicodeData = "Hello 世界! 🌍 Émojis: 😀😃😄 العالم мир";
        string key = "TestKey123";

        // Act
        string encrypted = unicodeData.AesEncryptAuthenticated(key);
        string decrypted = encrypted.AesDecrypt(key);

        // Assert
        Assert.Equal(unicodeData, decrypted);
        Assert.NotEqual(unicodeData, encrypted);
    }

    [Fact]
    public void AesEncryptAuthenticatedDecrypt_SpecialCharacters_WorksCorrectly()
    {
        // Arrange
        string specialData = "!@#$%^&*()_+-=[]{}|;:'\",.<>?/~`";
        string key = "TestKey123";

        // Act
        string encrypted = specialData.AesEncryptAuthenticated(key);
        string decrypted = encrypted.AesDecrypt(key);

        // Assert
        Assert.Equal(specialData, decrypted);
        Assert.NotEqual(specialData, encrypted);
    }

    [Fact]
    public void AesEncryptAuthenticatedDecrypt_MultipleRounds_ProducesConsistentResults()
    {
        // Arrange
        string data = "Test data for multiple encryption rounds";
        string key = "TestKey123";

        // Act
        string encrypted1 = data.AesEncryptAuthenticated(key);
        string decrypted1 = encrypted1.AesDecrypt(key);

        string encrypted2 = decrypted1.AesEncryptAuthenticated(key);
        string decrypted2 = encrypted2.AesDecrypt(key);

        // Assert
        Assert.Equal(data, decrypted1);
        Assert.Equal(data, decrypted2);
        Assert.Equal(decrypted1, decrypted2);
        // 每次加密应该产生不同的结果（因为随机IV）
        Assert.NotEqual(encrypted1, encrypted2);
    }

    #endregion

    #region Security and Consistency Tests

    [Fact]
    public void AesEncryptAuthenticated_SameKeyDifferentSessions_ProducesDecryptableResults()
    {
        // Arrange
        string data = "Cross-session test data";
        string key = "ConsistentKey123";

        // Act
        string encrypted1 = data.AesEncryptAuthenticated(key);
        string encrypted2 = data.AesEncryptAuthenticated(key);

        string decrypted1 = encrypted1.AesDecrypt(key);
        string decrypted2 = encrypted2.AesDecrypt(key);

        // Assert
        Assert.Equal(data, decrypted1);
        Assert.Equal(data, decrypted2);
        Assert.Equal(decrypted1, decrypted2);
        // Different encrypted outputs due to random IV
        Assert.NotEqual(encrypted1, encrypted2);
    }

    [Fact]
    public void AesEncryptAuthenticated_KeySensitivity_DifferentKeysProduceDifferentResults()
    {
        // Arrange
        string data = "Sensitive data";
        string key1 = "Key1";
        string key2 = "Key2";
        string key3 = "key1"; // Case sensitivity test

        // Act
        string encrypted1 = data.AesEncryptAuthenticated(key1);
        string encrypted2 = data.AesEncryptAuthenticated(key2);
        string encrypted3 = data.AesEncryptAuthenticated(key3);

        // Assert
        Assert.NotEqual(encrypted1, encrypted2);
        Assert.NotEqual(encrypted1, encrypted3);
        Assert.NotEqual(encrypted2, encrypted3);
    }

    [Fact]
    public void AesEncryptAuthenticated_RandomnessTest_MultipleEncryptionsProduceDifferentResults()
    {
        // Arrange
        string data = "Randomness test data";
        string key = "TestKey123";
        var encryptedResults = new HashSet<string>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            string encrypted = data.AesEncryptAuthenticated(key);
            encryptedResults.Add(encrypted);

            // Verify each can be decrypted correctly
            string decrypted = encrypted.AesDecrypt(key);
            Assert.Equal(data, decrypted);
        }

        // Assert
        // Due to random IV, all encrypted results should be different
        Assert.Equal(10, encryptedResults.Count);
    }

    #endregion

    private static string CreateLegacyPayload(string input, string key)
    {
        using var aes = Aes.Create();
        aes.Key = key.ToSha256HashByte();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] encrypted = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
        byte[] payload = new byte[aes.IV.Length + encrypted.Length];
        Buffer.BlockCopy(aes.IV, 0, payload, 0, aes.IV.Length);
        Buffer.BlockCopy(encrypted, 0, payload, aes.IV.Length, encrypted.Length);
        return Convert.ToBase64String(payload);
    }

    private static string CreateAuthenticatedV2Payload(string input, string password)
    {
        byte[] magic = { 0x4C, 0x4E, 0x47, 0x32 };
        byte[] salt = new byte[16];
        using (var random = RandomNumberGenerator.Create())
        {
            random.GetBytes(salt);
        }

#pragma warning disable SYSLIB0041, SYSLIB0060
        using var deriveBytes = new Rfc2898DeriveBytes(password, salt, 100_000);
#pragma warning restore SYSLIB0041, SYSLIB0060
        byte[] keyMaterial = deriveBytes.GetBytes(64);
        try
        {
            using var aes = Aes.Create();
            aes.Key = keyMaterial.Take(32).ToArray();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] encrypted = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
            int tagOffset = magic.Length + salt.Length + aes.IV.Length + encrypted.Length;
            byte[] payload = new byte[tagOffset + 32];
            Buffer.BlockCopy(magic, 0, payload, 0, magic.Length);
            Buffer.BlockCopy(salt, 0, payload, magic.Length, salt.Length);
            Buffer.BlockCopy(aes.IV, 0, payload, magic.Length + salt.Length, aes.IV.Length);
            Buffer.BlockCopy(encrypted, 0, payload, magic.Length + salt.Length + aes.IV.Length, encrypted.Length);

            using var hmac = new HMACSHA256(keyMaterial.Skip(32).Take(32).ToArray());
            byte[] tag = hmac.ComputeHash(payload, 0, tagOffset);
            Buffer.BlockCopy(tag, 0, payload, tagOffset, tag.Length);
            return Convert.ToBase64String(payload);
        }
        finally
        {
            Array.Clear(keyMaterial, 0, keyMaterial.Length);
        }
    }
}
