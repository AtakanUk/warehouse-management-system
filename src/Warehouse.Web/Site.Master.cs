using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using Warehouse.Web.Infrastructure;

namespace Warehouse.Web
{
    public partial class SiteMaster : MasterPage
    {
        private const string AntiXsrfTokenKey = "__AntiXsrfToken";
        private const string AntiXsrfUserNameKey = "__AntiXsrfUserName";

        private static readonly ConcurrentDictionary<string, string> AssetVersions = new ConcurrentDictionary<string, string>();

        private readonly List<Alert> _alerts = new List<Alert>();
        private string _antiXsrfTokenValue;

        protected bool IsAuthenticated => Context.User.Identity.IsAuthenticated;

        protected string UserName => Context.User.Identity.Name;

        protected string UserInitial => string.IsNullOrEmpty(UserName) ? "?" : UserName.Substring(0, 1).ToUpperInvariant();

        /// <summary>Shows a message on the current response (see <see cref="FlashMessages"/> for redirects).</summary>
        public void AddAlert(string message, AlertKind kind) => _alerts.Add(new Alert(message, kind));

        /// <summary>
        /// Cross-site request forgery protection: a random token is kept in an HttpOnly cookie and
        /// bound to the (MAC-protected) view state together with the user name. A forged post from
        /// another site cannot know the token, so the view state check fails.
        /// </summary>
        protected void Page_Init(object sender, EventArgs e)
        {
            var requestCookie = Request.Cookies[AntiXsrfTokenKey];
            if (requestCookie != null && Guid.TryParse(requestCookie.Value, out _))
            {
                _antiXsrfTokenValue = requestCookie.Value;
            }
            else
            {
                _antiXsrfTokenValue = Guid.NewGuid().ToString("N");
                Response.Cookies.Set(new HttpCookie(AntiXsrfTokenKey, _antiXsrfTokenValue)
                {
                    HttpOnly = true,
                    Secure = FormsAuthentication.RequireSSL && Request.IsSecureConnection,
                    SameSite = SameSiteMode.Lax
                });
            }

            Page.ViewStateUserKey = _antiXsrfTokenValue;
            Page.PreLoad += Page_PreLoad;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            NavigationPanel.Visible = IsAuthenticated;
            TopBar.Visible = IsAuthenticated;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // The page lifecycle continues after Response.Redirect(url, false); keep flash
            // messages for the page the browser is being redirected to.
            if (Response.IsRequestBeingRedirected)
            {
                return;
            }

            AlertList.DataSource = FlashMessages.Take(Context).Concat(_alerts).ToList();
            AlertList.DataBind();
        }

        protected void SignOut_Click(object sender, EventArgs e)
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            Response.Redirect(FormsAuthentication.LoginUrl, endResponse: false);
            Context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// URL of a static file with a version stamp, so the long client cache configured in
        /// Web.config never serves an outdated stylesheet or script.
        /// </summary>
        protected string Asset(string path)
        {
            var version = Context.IsDebuggingEnabled
                ? ComputeVersion(path)
                : AssetVersions.GetOrAdd(path, ComputeVersion);

            return ResolveUrl(path) + "?v=" + version;
        }

        private string ComputeVersion(string path) =>
            File.GetLastWriteTimeUtc(Server.MapPath(path)).Ticks.ToString("x", CultureInfo.InvariantCulture);

        /// <summary>CSS class for a navigation link, marking the section of the current URL.</summary>
        protected string NavClass(string path, bool exact = false, string exclude = null)
        {
            var current = VirtualPathUtility.ToAppRelative(Request.Path).TrimEnd('/');
            var target = path.TrimEnd('/');

            var isActive = exact
                ? string.Equals(current, target, StringComparison.OrdinalIgnoreCase)
                : string.Equals(current, target, StringComparison.OrdinalIgnoreCase)
                  || current.StartsWith(target + "/", StringComparison.OrdinalIgnoreCase);

            if (exclude != null && current.StartsWith(exclude.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
            {
                isActive = false;
            }

            return isActive ? "side-nav-link active" : "side-nav-link";
        }

        private void Page_PreLoad(object sender, EventArgs e)
        {
            var userName = Context.User.Identity.Name ?? string.Empty;
            if (!IsPostBack)
            {
                ViewState[AntiXsrfTokenKey] = Page.ViewStateUserKey;
                ViewState[AntiXsrfUserNameKey] = userName;
            }
            else if ((string)ViewState[AntiXsrfTokenKey] != _antiXsrfTokenValue
                || (string)ViewState[AntiXsrfUserNameKey] != userName)
            {
                throw new HttpException(400, "Validation of the anti-forgery token failed.");
            }
        }
    }
}
