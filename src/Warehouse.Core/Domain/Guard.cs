using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Warehouse.Core.Domain
{
    /// <summary>Input validation helpers that raise user-facing <see cref="BusinessRuleException"/>s.</summary>
    internal static class Guard
    {
        private static readonly Regex EmailPattern =
            new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string Required(string value, string fieldName, int maxLength)
        {
            var trimmed = Optional(value, fieldName, maxLength);
            if (trimmed == null)
            {
                throw new BusinessRuleException($"{fieldName} is required.");
            }

            return trimmed;
        }

        public static string Optional(string value, string fieldName, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim();
            if (trimmed.Length > maxLength)
            {
                throw new BusinessRuleException($"{fieldName} cannot be longer than {maxLength} characters.");
            }

            return trimmed;
        }

        public static string OptionalEmail(string value, string fieldName = "Email")
        {
            var email = Optional(value, fieldName, 254);
            if (email != null && !EmailPattern.IsMatch(email))
            {
                throw new BusinessRuleException($"{fieldName} is not a valid email address.");
            }

            return email;
        }

        public static void Positive(int quantity, string fieldName = "Quantity")
        {
            if (quantity <= 0)
            {
                throw new BusinessRuleException($"{fieldName} must be greater than zero.");
            }
        }

        public static void NotNegative(int value, string fieldName)
        {
            if (value < 0)
            {
                throw new BusinessRuleException($"{fieldName} cannot be negative.");
            }
        }

        public static string UpperInvariant(string value) =>
            value?.ToUpper(CultureInfo.InvariantCulture);

        public static DateTime Utc(DateTime value)
        {
            if (value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Timestamps must be expressed in UTC.", nameof(value));
            }

            return value;
        }
    }
}
