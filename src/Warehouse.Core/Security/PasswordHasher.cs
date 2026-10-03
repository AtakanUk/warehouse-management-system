using System;
using System.Globalization;
using System.Security.Cryptography;

namespace Warehouse.Core.Security
{
    /// <summary>
    /// Salted PBKDF2-HMAC-SHA512 password hashing following the OWASP Password Storage Cheat Sheet
    /// (210,000 iterations). Hashes are self-describing:
    /// <c>PBKDF2-SHA512$&lt;iterations&gt;$&lt;base64 salt&gt;$&lt;base64 key&gt;</c>, so the work
    /// factor can be raised later without invalidating existing passwords.
    /// </summary>
    public sealed class PasswordHasher
    {
        public const int DefaultIterations = 210_000;

        private const string Algorithm = "PBKDF2-SHA512";
        private const int SaltSize = 16;
        private const int KeySize = 32;

        public PasswordHasher()
            : this(DefaultIterations)
        {
        }

        /// <param name="iterations">Work factor; lower values are only meant for tests.</param>
        public PasswordHasher(int iterations)
        {
            if (iterations < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(iterations));
            }

            Iterations = iterations;
        }

        public int Iterations { get; }

        public string Hash(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            var salt = new byte[SaltSize];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt);
            }

            var key = DeriveKey(password, salt, Iterations, KeySize);
            return string.Join("$", Algorithm, Iterations.ToString(CultureInfo.InvariantCulture), Convert.ToBase64String(salt), Convert.ToBase64String(key));
        }

        public bool Verify(string password, string storedHash)
        {
            if (password == null || !TryParse(storedHash, out var iterations, out var salt, out var expectedKey))
            {
                return false;
            }

            var actualKey = DeriveKey(password, salt, iterations, expectedKey.Length);
            return FixedTimeEquals(actualKey, expectedKey);
        }

        /// <summary>True when the hash was produced with a weaker work factor than the current one.</summary>
        public bool NeedsRehash(string storedHash) =>
            !TryParse(storedHash, out var iterations, out _, out _) || iterations < Iterations;

        private static bool TryParse(string storedHash, out int iterations, out byte[] salt, out byte[] key)
        {
            iterations = 0;
            salt = key = null;

            var parts = storedHash?.Split('$');
            if (parts == null || parts.Length != 4 || parts[0] != Algorithm
                || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out iterations) || iterations < 1)
            {
                return false;
            }

            try
            {
                salt = Convert.FromBase64String(parts[2]);
                key = Convert.FromBase64String(parts[3]);
                return salt.Length > 0 && key.Length > 0;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static byte[] DeriveKey(string password, byte[] salt, int iterations, int length)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA512))
            {
                return pbkdf2.GetBytes(length);
            }
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
            {
                return false;
            }

            var difference = 0;
            for (var i = 0; i < left.Length; i++)
            {
                difference |= left[i] ^ right[i];
            }

            return difference == 0;
        }
    }
}
