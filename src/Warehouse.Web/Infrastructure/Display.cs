using System;
using System.Globalization;
using System.Web;
using Warehouse.Core.Domain;

namespace Warehouse.Web.Infrastructure
{
    /// <summary>Formatting helpers used by the markup. Every method returns HTML-safe output.</summary>
    public static class Display
    {
        private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-GB");

        public static string Number(int value) => value.ToString("N0", Culture);

        public static string Date(DateTime utc) => utc.ToLocalTime().ToString("d MMM yyyy, HH:mm", Culture);

        public static string Date(DateTime? utc) => utc.HasValue ? Date(utc.Value) : "—";

        public static string ShortDate(DateTime utc) => utc.ToLocalTime().ToString("d MMM", Culture);

        /// <summary>"just now", "25 min ago", "3 h ago", "2 days ago", then the date.</summary>
        public static string Ago(DateTime utc)
        {
            var elapsed = DateTime.UtcNow - utc;
            if (elapsed.TotalMinutes < 1)
            {
                return "just now";
            }

            if (elapsed.TotalHours < 1)
            {
                return $"{(int)elapsed.TotalMinutes} min ago";
            }

            if (elapsed.TotalDays < 1)
            {
                return $"{(int)elapsed.TotalHours} h ago";
            }

            return elapsed.TotalDays < 7 ? $"{(int)elapsed.TotalDays} day{((int)elapsed.TotalDays == 1 ? string.Empty : "s")} ago" : ShortDate(utc);
        }

        public static string Ago(DateTime? utc) => utc.HasValue ? Ago(utc.Value) : "—";

        public static string OrderStatusBadge(OrderStatus status)
        {
            var label = status == OrderStatus.Picked ? "Picked · ready to ship" : status.ToString();
            return Badge("status-" + status.ToString().ToLowerInvariant(), label);
        }

        public static string StockBadge(StockStatus status)
        {
            switch (status)
            {
                case StockStatus.OutOfStock:
                    return Badge("stock-out", "Out of stock");
                case StockStatus.Low:
                    return Badge("stock-low", "Low");
                default:
                    return Badge("stock-ok", "In stock");
            }
        }

        public static string MovementBadge(StockMovementType type) =>
            type == StockMovementType.Receipt ? Badge("movement-in", "Receipt") : Badge("movement-out", "Shipment");

        /// <summary>Signed quantity, e.g. "+50" or "−20" (true minus sign).</summary>
        public static string SignedQuantity(int quantity) =>
            quantity > 0 ? "+" + Number(quantity) : "−" + Number(-quantity);

        public static string Text(string value) =>
            string.IsNullOrWhiteSpace(value) ? "<span class=\"text-muted\">—</span>" : HttpUtility.HtmlEncode(value);

        private static string Badge(string cssClass, string label) =>
            $"<span class=\"badge-pill {cssClass}\">{HttpUtility.HtmlEncode(label)}</span>";
    }
}
