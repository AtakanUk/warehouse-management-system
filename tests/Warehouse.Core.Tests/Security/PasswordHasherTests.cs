using System.Text.RegularExpressions;
using Warehouse.Core.Data;
using Warehouse.Core.Security;
using Xunit;

namespace Warehouse.Core.Tests.Security
{
    public class PasswordHasherTests
    {
        // A low work factor keeps the suite fast; the algorithm is identical.
        private readonly PasswordHasher _hasher = new PasswordHasher(iterations: 1_000);

        [Fact]
        public void Hash_is_self_describing()
        {
            var hash = _hasher.Hash("correct horse battery staple");

            Assert.Matches(@"^PBKDF2-SHA512\$1000\$[A-Za-z0-9+/=]{24}\$[A-Za-z0-9+/=]{44}$", hash);
        }

        [Fact]
        public void Verify_accepts_the_original_password_only()
        {
            var hash = _hasher.Hash("correct horse battery staple");

            Assert.True(_hasher.Verify("correct horse battery staple", hash));
            Assert.False(_hasher.Verify("Correct horse battery staple", hash));
            Assert.False(_hasher.Verify(string.Empty, hash));
        }

        [Fact]
        public void Same_password_produces_different_hashes()
        {
            Assert.NotEqual(_hasher.Hash("password123"), _hasher.Hash("password123"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("plain-text-password")]
        [InlineData("PBKDF2-SHA512$abc$c2FsdA==$a2V5")]
        [InlineData("PBKDF2-SHA512$1000$not base64$a2V5")]
        [InlineData("SHA1$1000$c2FsdA==$a2V5")]
        public void Verify_rejects_malformed_hashes(string storedHash)
        {
            Assert.False(_hasher.Verify("password", storedHash));
        }

        [Fact]
        public void Hashes_with_a_lower_work_factor_need_rehashing()
        {
            var weakHash = new PasswordHasher(iterations: 500).Hash("password123");

            Assert.True(_hasher.NeedsRehash(weakHash));
            Assert.False(_hasher.NeedsRehash(_hasher.Hash("password123")));
            Assert.True(_hasher.Verify("password123", weakHash));
        }

        [Fact]
        public void Demo_account_in_seed_script_matches_the_documented_password()
        {
            var seed = DatabaseProvisioner.ReadScript("seed.sql");
            var storedHash = Regex.Match(seed, @"PBKDF2-SHA512\$[^']+").Value;

            Assert.Contains("demo / Demo@12345", seed);
            Assert.True(new PasswordHasher().Verify("Demo@12345", storedHash));
        }
    }
}
