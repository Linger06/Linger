#pragma warning disable
#if !NET5_0_OR_GREATER
using System.Security.Cryptography;

namespace System.Security.Cryptography;

/// <summary>
/// Provides downlevel polyfill for MD5.HashData.
/// </summary>
public static class MD5Polyfills
{
    extension(MD5)
    {
        /// <summary>
        /// Computes the hash of data using the MD5 algorithm.
        /// </summary>
        /// <param name="source">The data to hash.</param>
        /// <returns>The hash value.</returns>
        public static byte[] HashData(byte[] source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            using var md5 = MD5.Create();
            return md5.ComputeHash(source);
        }
    }
}

/// <summary>
/// Provides downlevel polyfill for SHA256.HashData.
/// </summary>
public static class SHA256Polyfills
{
    extension(SHA256)
    {
        /// <summary>
        /// Computes the hash of data using the SHA256 algorithm.
        /// </summary>
        /// <param name="source">The data to hash.</param>
        /// <returns>The hash value.</returns>
        public static byte[] HashData(byte[] source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(source);
        }
    }
}

/// <summary>
/// Provides downlevel polyfill for SHA384.HashData.
/// </summary>
public static class SHA384Polyfills
{
    extension(SHA384)
    {
        /// <summary>
        /// Computes the hash of data using the SHA384 algorithm.
        /// </summary>
        /// <param name="source">The data to hash.</param>
        /// <returns>The hash value.</returns>
        public static byte[] HashData(byte[] source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            using var sha384 = SHA384.Create();
            return sha384.ComputeHash(source);
        }
    }
}

/// <summary>
/// Provides downlevel polyfill for SHA512.HashData.
/// </summary>
public static class SHA512Polyfills
{
    extension(SHA512)
    {
        /// <summary>
        /// Computes the hash of data using the SHA512 algorithm.
        /// </summary>
        /// <param name="source">The data to hash.</param>
        /// <returns>The hash value.</returns>
        public static byte[] HashData(byte[] source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            using var sha512 = SHA512.Create();
            return sha512.ComputeHash(source);
        }
    }
}
#endif
