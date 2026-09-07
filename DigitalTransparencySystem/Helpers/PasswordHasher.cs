using System;
using System.Security.Cryptography;
using System.Text;

namespace DigitalTransparencySystem.Helpers
{
    public static class PasswordHasher
    {
        private const int Iterations = 10000;
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const string Prefix = "pbkdf2$";

        public static string Hash(string password)
        {
            if (password == null) password = string.Empty;

            byte[] salt = new byte[SaltSize];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(salt);

            byte[] key;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
                key = pbkdf2.GetBytes(KeySize);

            return Prefix + Iterations + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(key);
        }

        public static bool Verify(string password, string stored)
        {
            if (string.IsNullOrEmpty(stored))
                return false;

            if (stored.StartsWith(Prefix, StringComparison.Ordinal))
                return VerifyPbkdf2(password ?? string.Empty, stored);

            return FixedEquals(LegacySha256(password ?? string.Empty), stored);
        }

        public static bool NeedsUpgrade(string stored)
        {
            return string.IsNullOrEmpty(stored)
                || !stored.StartsWith(Prefix, StringComparison.Ordinal);
        }

        private static bool VerifyPbkdf2(string password, string stored)
        {
            string[] parts = stored.Split('$');
            if (parts.Length != 4)
                return false;

            int iterations;
            if (!int.TryParse(parts[1], out iterations) || iterations < 1)
                return false;

            byte[] salt;
            byte[] expected;
            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expected = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] actual;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
                actual = pbkdf2.GetBytes(expected.Length);

            return FixedEquals(actual, expected);
        }

        private static string LegacySha256(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder(bytes.Length * 2);
                for (int i = 0; i < bytes.Length; i++)
                    builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }

        private static bool FixedEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }

        private static bool FixedEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
