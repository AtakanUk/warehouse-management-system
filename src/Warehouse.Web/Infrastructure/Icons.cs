using System.Collections.Generic;

namespace Warehouse.Web.Infrastructure
{
    /// <summary>
    /// Inline SVG icons (outline style, based on Lucide, ISC licence - see THIRD-PARTY-NOTICES.md).
    /// Inline markup avoids an icon font download and keeps the Content-Security-Policy strict.
    /// </summary>
    public static class Icons
    {
        private static readonly Dictionary<string, string> Paths = new Dictionary<string, string>
        {
            ["dashboard"] = "<rect x=\"3\" y=\"3\" width=\"7\" height=\"9\" rx=\"1\"/><rect x=\"14\" y=\"3\" width=\"7\" height=\"5\" rx=\"1\"/><rect x=\"14\" y=\"12\" width=\"7\" height=\"9\" rx=\"1\"/><rect x=\"3\" y=\"16\" width=\"7\" height=\"5\" rx=\"1\"/>",
            ["package"] = "<path d=\"m7.5 4.27 9 5.15\"/><path d=\"M21 8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16Z\"/><path d=\"m3.3 7 8.7 5 8.7-5\"/><path d=\"M12 22V12\"/>",
            ["layers"] = "<path d=\"M12.83 2.18a2 2 0 0 0-1.66 0L2.6 6.08a1 1 0 0 0 0 1.83l8.58 3.91a2 2 0 0 0 1.66 0l8.58-3.9a1 1 0 0 0 0-1.83Z\"/><path d=\"m22 17.65-9.17 4.16a2 2 0 0 1-1.66 0L2 17.65\"/><path d=\"m22 12.65-9.17 4.16a2 2 0 0 1-1.66 0L2 12.65\"/>",
            ["receive"] = "<path d=\"M12 3v12\"/><path d=\"m7 10 5 5 5-5\"/><path d=\"M5 21h14\"/>",
            ["history"] = "<path d=\"M3 12a9 9 0 1 0 3-6.7L3 8\"/><path d=\"M3 3v5h5\"/><path d=\"M12 7v5l4 2\"/>",
            ["orders"] = "<rect x=\"8\" y=\"2\" width=\"8\" height=\"4\" rx=\"1\"/><path d=\"M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2\"/><path d=\"M12 11h4\"/><path d=\"M12 16h4\"/><path d=\"M8 11h.01\"/><path d=\"M8 16h.01\"/>",
            ["plus"] = "<path d=\"M12 5v14\"/><path d=\"M5 12h14\"/>",
            ["picking"] = "<path d=\"m3 17 2 2 4-4\"/><path d=\"m3 7 2 2 4-4\"/><path d=\"M13 6h8\"/><path d=\"M13 12h8\"/><path d=\"M13 18h8\"/>",
            ["truck"] = "<path d=\"M14 18V6a2 2 0 0 0-2-2H4a2 2 0 0 0-2 2v11a1 1 0 0 0 1 1h2\"/><path d=\"M15 18H9\"/><path d=\"M19 18h2a1 1 0 0 0 1-1v-3.65a1 1 0 0 0-.22-.62l-3.48-4.35A1 1 0 0 0 17.52 8H14\"/><circle cx=\"17\" cy=\"18\" r=\"2\"/><circle cx=\"7\" cy=\"18\" r=\"2\"/>",
            ["users"] = "<path d=\"M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2\"/><circle cx=\"9\" cy=\"7\" r=\"4\"/><path d=\"M22 21v-2a4 4 0 0 0-3-3.87\"/><path d=\"M16 3.13a4 4 0 0 1 0 7.75\"/>",
            ["route"] = "<circle cx=\"6\" cy=\"19\" r=\"3\"/><path d=\"M9 19h8.5a3.5 3.5 0 0 0 0-7h-11a3.5 3.5 0 0 1 0-7H15\"/><circle cx=\"18\" cy=\"5\" r=\"3\"/>",
            ["warehouse"] = "<path d=\"M22 8.35V20a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V8.35A2 2 0 0 1 3.26 6.5l8-3.2a2 2 0 0 1 1.48 0l8 3.2A2 2 0 0 1 22 8.35Z\"/><path d=\"M6 18h12\"/><path d=\"M6 14h12\"/><rect x=\"6\" y=\"10\" width=\"12\" height=\"12\"/>",
            ["logout"] = "<path d=\"M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4\"/><path d=\"m16 17 5-5-5-5\"/><path d=\"M21 12H9\"/>",
            ["menu"] = "<path d=\"M4 6h16\"/><path d=\"M4 12h16\"/><path d=\"M4 18h16\"/>",
            ["alert"] = "<path d=\"m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3\"/><path d=\"M12 9v4\"/><path d=\"M12 17h.01\"/>",
            ["check"] = "<path d=\"M20 6 9 17l-5-5\"/>",
            ["search"] = "<circle cx=\"11\" cy=\"11\" r=\"8\"/><path d=\"m21 21-4.3-4.3\"/>",
            ["arrow-right"] = "<path d=\"M5 12h14\"/><path d=\"m12 5 7 7-7 7\"/>",
            ["inbox"] = "<path d=\"M22 12h-6l-2 3h-4l-2-3H2\"/><path d=\"M5.45 5.11 2 12v6a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2v-6l-3.45-6.89A2 2 0 0 0 16.76 4H7.24a2 2 0 0 0-1.79 1.11z\"/>"
        };

        public static string Get(string name, string cssClass = "icon") =>
            "<svg class=\"" + cssClass + "\" viewBox=\"0 0 24 24\" aria-hidden=\"true\" focusable=\"false\">" + Paths[name] + "</svg>";
    }
}
