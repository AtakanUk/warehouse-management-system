using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Warehouse.Core;
using Warehouse.Core.Data;

namespace Warehouse.Web.Infrastructure
{
    /// <summary>
    /// Base class for all pages: one <see cref="WarehouseDbContext"/> per request, user feedback
    /// helpers, Post/Redirect/Get and small URL helpers for the markup.
    /// </summary>
    public abstract class WarehousePage : Page
    {
        private WarehouseDbContext _db;
        private bool _isRedirecting;

        /// <summary>Unit of work for the current request; disposed when the page unloads.</summary>
        protected WarehouseDbContext Db => _db ?? (_db = new WarehouseDbContext());

        protected string CurrentUserName => User.Identity.Name;

        protected SiteMaster SiteMaster => (SiteMaster)Master;

        /// <summary>The numeric {id} route value, or null when the URL has none.</summary>
        protected int? RouteId =>
            int.TryParse(RouteData.Values["id"] as string, out var id) ? id : (int?)null;

        protected void ShowAlert(string message, AlertKind kind = AlertKind.Danger) => SiteMaster.AddAlert(message, kind);

        /// <summary>
        /// Runs a command and shows business rule violations to the user instead of failing the
        /// request. Returns <c>true</c> when the command succeeded.
        /// </summary>
        /// <remarks>
        /// On failure the unit of work is discarded: entities it tracked may hold changes that were
        /// never saved, and the page must render what is actually stored.
        /// </remarks>
        protected bool TryRun(Action command)
        {
            try
            {
                command();
                return true;
            }
            catch (BusinessRuleException ex)
            {
                _db?.Dispose();
                _db = null;
                ShowAlert(ex.Message);
                return false;
            }
        }

        /// <summary>Redirects after a successful POST and shows the message on the next page.</summary>
        protected void RedirectWithMessage(string url, string message, AlertKind kind = AlertKind.Success)
        {
            FlashMessages.Add(Context, message, kind);
            RedirectTo(url);
        }

        protected void RedirectTo(string url)
        {
            _isRedirecting = true;
            Response.Redirect(url, endResponse: false);
            Context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>Renders the 404 page for a missing record.</summary>
        protected void ShowNotFound() => Server.Transfer("~/NotFound.aspx");

        protected string Encode(string value) => HttpUtility.HtmlEncode(value);

        protected string OrderUrl(int id) => ResolveUrl("~/orders/" + id);

        protected string ProductUrl(int id) => ResolveUrl("~/products/" + id + "/edit");

        protected override void OnPreRenderComplete(EventArgs e)
        {
            base.OnPreRenderComplete(e);
            DecorateListViews(Form);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (!_isRedirecting)
            {
                base.Render(writer);
            }
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            _db?.Dispose();
        }

        /// <summary>Marks the active sort column and hides pagers that have a single page.</summary>
        private static void DecorateListViews(Control root)
        {
            if (root == null)
            {
                return;
            }

            foreach (Control control in root.Controls)
            {
                if (control is LinkButton link && link.CommandName == "Sort" && link.NamingContainer is ListView list)
                {
                    var isSorted = string.Equals(list.SortExpression, link.CommandArgument, StringComparison.Ordinal);
                    var direction = list.SortDirection == SortDirection.Ascending ? "ascending" : "descending";
                    link.CssClass = "sort-link" + (isSorted ? " sorted-" + direction : string.Empty);
                    link.ToolTip = "Sort by " + link.Text;
                }
                else if (control is DataPager pager)
                {
                    pager.Visible = pager.TotalRowCount > pager.PageSize;
                }

                if (control.HasControls())
                {
                    DecorateListViews(control);
                }
            }
        }
    }
}
