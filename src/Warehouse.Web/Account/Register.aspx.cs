using System;
using Warehouse.Core.Services;
using Warehouse.Web.Infrastructure;

namespace Warehouse.Web.Account
{
    public partial class Register : WarehousePage
    {
        protected void Register_Click(object sender, EventArgs e)
        {
            if (!IsValid)
            {
                return;
            }

            string userName = null;
            if (TryRun(() => userName = new AccountService(Db).Register(UserNameBox.Text, PasswordBox.Text).UserName))
            {
                RedirectWithMessage("~/account/login", $"Account {userName} created. You can sign in now.");
            }
        }
    }
}
