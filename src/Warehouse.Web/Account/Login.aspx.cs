using System;
using System.Web.Security;
using Warehouse.Core.Services;
using Warehouse.Web.Infrastructure;

namespace Warehouse.Web.Account
{
    public partial class Login : WarehousePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (User.Identity.IsAuthenticated && !IsPostBack)
            {
                RedirectTo(FormsAuthentication.DefaultUrl);
                return;
            }

            DemoCredentials.Visible = AppSettings.ShowDemoCredentials;
            RememberMe.InputAttributes["class"] = "form-check-input";
        }

        protected void SignIn_Click(object sender, EventArgs e)
        {
            if (!IsValid)
            {
                return;
            }

            var result = new AccountService(Db).SignIn(UserNameBox.Text, PasswordBox.Text);

            if (result.Succeeded)
            {
                FormsAuthentication.SetAuthCookie(result.UserName, RememberMe.Checked);

                // GetRedirectUrl validates ReturnUrl and falls back to the start page for
                // anything outside this application (no open redirects).
                RedirectTo(FormsAuthentication.GetRedirectUrl(result.UserName, RememberMe.Checked));
                return;
            }

            if (result.IsLockedOut)
            {
                var minutes = Math.Max(1, (int)Math.Ceiling(((result.LockoutEndUtc ?? DateTime.UtcNow) - DateTime.UtcNow).TotalMinutes));
                ShowAlert($"Too many failed attempts. The account is locked for {minutes} more minute(s).", AlertKind.Warning);
            }
            else
            {
                ShowAlert("The user name or password is incorrect.");
            }

            PasswordBox.Focus();
        }
    }
}
