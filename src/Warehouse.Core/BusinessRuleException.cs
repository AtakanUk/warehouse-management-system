using System;

namespace Warehouse.Core
{
    /// <summary>
    /// Raised when an operation would violate a warehouse business rule.
    /// The message is written for end users and is safe to display.
    /// </summary>
    [Serializable]
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message)
            : base(message)
        {
        }

        public BusinessRuleException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected BusinessRuleException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// Raised when another user changed the same data between it being read and saved
    /// (optimistic concurrency via SQL Server ROWVERSION columns).
    /// </summary>
    [Serializable]
    public class ConcurrencyConflictException : BusinessRuleException
    {
        public const string DefaultMessage =
            "Someone else updated this data at the same time. Reload the page and try again.";

        public ConcurrencyConflictException(Exception innerException)
            : base(DefaultMessage, innerException)
        {
        }

        protected ConcurrencyConflictException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
