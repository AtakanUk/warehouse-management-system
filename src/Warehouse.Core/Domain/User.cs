using System;
using System.Text.RegularExpressions;

namespace Warehouse.Core.Domain
{
    /// <summary>An operator who can sign in to the application.</summary>
    public class User
    {
        public const int UserNameMinLength = 3;
        public const int UserNameMaxLength = 50;
        public const int MaxFailedSignIns = 5;
        public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(5);

        private static readonly Regex UserNamePattern = new Regex(@"^[A-Za-z0-9._-]+$", RegexOptions.Compiled);

        protected User()
        {
        }

        public int Id { get; private set; }

        public string UserName { get; private set; }

        public string PasswordHash { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }

        public DateTime? LastSignInAtUtc { get; private set; }

        public int FailedSignInCount { get; private set; }

        public DateTime? LockoutEndUtc { get; private set; }

        public static User Create(string userName, string passwordHash, DateTime nowUtc)
        {
            return new User
            {
                UserName = NormalizeUserName(userName),
                PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash)),
                CreatedAtUtc = Guard.Utc(nowUtc)
            };
        }

        public static string NormalizeUserName(string userName)
        {
            var name = Guard.Required(userName, "User name", UserNameMaxLength);
            if (name.Length < UserNameMinLength || !UserNamePattern.IsMatch(name))
            {
                throw new BusinessRuleException(
                    $"User name must be {UserNameMinLength}-{UserNameMaxLength} characters and may contain letters, digits, '.', '_' and '-'.");
            }

            return name;
        }

        public bool IsLockedOut(DateTime nowUtc) => LockoutEndUtc.HasValue && LockoutEndUtc.Value > nowUtc;

        public void RecordFailedSignIn(DateTime nowUtc)
        {
            FailedSignInCount++;
            if (FailedSignInCount >= MaxFailedSignIns)
            {
                LockoutEndUtc = nowUtc.Add(LockoutDuration);
                FailedSignInCount = 0;
            }
        }

        public void RecordSuccessfulSignIn(DateTime nowUtc)
        {
            FailedSignInCount = 0;
            LockoutEndUtc = null;
            LastSignInAtUtc = nowUtc;
        }

        /// <summary>Replaces the stored hash, e.g. after the hashing parameters were strengthened.</summary>
        public void UpgradePasswordHash(string passwordHash)
        {
            PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        }
    }
}
