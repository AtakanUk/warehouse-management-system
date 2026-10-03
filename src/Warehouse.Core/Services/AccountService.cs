using System;
using System.Collections.Concurrent;
using System.Linq;
using Warehouse.Core.Data;
using Warehouse.Core.Domain;
using Warehouse.Core.Security;

namespace Warehouse.Core.Services
{
    public sealed class SignInResult
    {
        private SignInResult()
        {
        }

        public bool Succeeded { get; private set; }

        public bool IsLockedOut { get; private set; }

        public DateTime? LockoutEndUtc { get; private set; }

        /// <summary>The canonical user name as stored, when sign-in succeeded.</summary>
        public string UserName { get; private set; }

        internal static SignInResult Success(string userName) => new SignInResult { Succeeded = true, UserName = userName };

        internal static SignInResult Failed() => new SignInResult();

        internal static SignInResult LockedOut(DateTime? lockoutEndUtc) => new SignInResult { IsLockedOut = true, LockoutEndUtc = lockoutEndUtc };
    }

    /// <summary>Registration and credential checks with brute-force protection.</summary>
    public sealed class AccountService
    {
        public const int PasswordMinLength = 8;
        public const int PasswordMaxLength = 128;

        // Used to spend the same time on unknown user names as on real ones (no user enumeration by timing).
        private static readonly ConcurrentDictionary<int, string> DummyHashes = new ConcurrentDictionary<int, string>();

        private readonly WarehouseDbContext _db;
        private readonly PasswordHasher _hasher;
        private readonly Func<DateTime> _utcNow;

        public AccountService(WarehouseDbContext db, PasswordHasher hasher = null, Func<DateTime> utcNow = null)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _hasher = hasher ?? new PasswordHasher();
            _utcNow = utcNow ?? (() => DateTime.UtcNow);
        }

        public User Register(string userName, string password)
        {
            var name = User.NormalizeUserName(userName);
            ValidatePassword(name, password);

            if (_db.Users.Any(u => u.UserName == name))
            {
                throw new BusinessRuleException("That user name is already taken.");
            }

            var user = User.Create(name, _hasher.Hash(password), _utcNow());
            _db.Users.Add(user);
            _db.Commit();
            return user;
        }

        public SignInResult SignIn(string userName, string password)
        {
            var name = userName?.Trim();
            var user = string.IsNullOrEmpty(name) ? null : _db.Users.SingleOrDefault(u => u.UserName == name);

            if (user == null)
            {
                _hasher.Verify(password ?? string.Empty, DummyHashes.GetOrAdd(_hasher.Iterations, _ => _hasher.Hash(Guid.NewGuid().ToString())));
                return SignInResult.Failed();
            }

            var now = _utcNow();
            if (user.IsLockedOut(now))
            {
                return SignInResult.LockedOut(user.LockoutEndUtc);
            }

            if (!_hasher.Verify(password ?? string.Empty, user.PasswordHash))
            {
                user.RecordFailedSignIn(now);
                _db.Commit();
                return user.IsLockedOut(now) ? SignInResult.LockedOut(user.LockoutEndUtc) : SignInResult.Failed();
            }

            user.RecordSuccessfulSignIn(now);
            if (_hasher.NeedsRehash(user.PasswordHash))
            {
                user.UpgradePasswordHash(_hasher.Hash(password));
            }

            _db.Commit();
            return SignInResult.Success(user.UserName);
        }

        private static void ValidatePassword(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < PasswordMinLength || password.Length > PasswordMaxLength)
            {
                throw new BusinessRuleException($"Password must be between {PasswordMinLength} and {PasswordMaxLength} characters.");
            }

            if (string.Equals(password, userName, StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessRuleException("Password must be different from the user name.");
            }
        }
    }
}
