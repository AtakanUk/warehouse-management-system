using System;
using System.Collections.Generic;
using System.Web;

namespace Warehouse.Web.Infrastructure
{
    public enum AlertKind
    {
        Success,
        Info,
        Warning,
        Danger
    }

    [Serializable]
    public sealed class Alert
    {
        public Alert(string message, AlertKind kind)
        {
            Message = message;
            Kind = kind;
        }

        public string Message { get; }

        public AlertKind Kind { get; }

        public string CssClass => "alert alert-" + Kind.ToString().ToLowerInvariant();
    }

    /// <summary>
    /// Messages that survive exactly one redirect (Post/Redirect/Get), e.g. "Order SO-00012 created".
    /// </summary>
    public static class FlashMessages
    {
        private const string SessionKey = "Warehouse.FlashMessages";

        public static void Add(HttpContext context, string message, AlertKind kind)
        {
            var messages = context.Session[SessionKey] as List<Alert> ?? new List<Alert>();
            messages.Add(new Alert(message, kind));
            context.Session[SessionKey] = messages;
        }

        public static IReadOnlyList<Alert> Take(HttpContext context)
        {
            if (context.Session == null || !(context.Session[SessionKey] is List<Alert> messages))
            {
                return Array.Empty<Alert>();
            }

            context.Session.Remove(SessionKey);
            return messages;
        }
    }
}
