using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using Warehouse.Core.Domain;
using Warehouse.Core.Services;
using Warehouse.Web.Infrastructure;

namespace Warehouse.Web.Orders
{
    public partial class NewOrder : WarehousePage
    {
        private const string LinesKey = "DraftLines";
        private IReadOnlyList<ProductOption> _products;

        protected int LineCount { get; private set; }

        protected int TotalUnits { get; private set; }

        private IReadOnlyList<ProductOption> ProductOptions => _products ?? (_products = new ProductService(Db).GetOptions());

        /// <summary>
        /// Lines entered so far. Kept in the (MAC-protected) view state as "productId:quantity"
        /// pairs, so nothing is written to the database until the order is created.
        /// </summary>
        private List<OrderLineInput> DraftLines
        {
            get => ((ViewState[LinesKey] as string) ?? string.Empty)
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(pair => pair.Split(':'))
                .Select(parts => new OrderLineInput(int.Parse(parts[0], CultureInfo.InvariantCulture), int.Parse(parts[1], CultureInfo.InvariantCulture)))
                .ToList();
            set => ViewState[LinesKey] = string.Join(";", value.Select(l => l.ProductId.ToString(CultureInfo.InvariantCulture) + ":" + l.Quantity.ToString(CultureInfo.InvariantCulture)));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CustomerList.DataBind();
                ProductList.DataBind();
            }
        }

        public IReadOnlyList<PartnerOption> GetCustomers() => new CustomerService(Db).GetOptions();

        public IReadOnlyList<ProductOption> GetProducts() => ProductOptions;

        protected void AddLine_Click(object sender, EventArgs e)
        {
            if (!IsValid)
            {
                return;
            }

            var productId = int.Parse(ProductList.SelectedValue, CultureInfo.InvariantCulture);
            var quantity = int.Parse(QuantityBox.Text, CultureInfo.InvariantCulture);

            var lines = DraftLines;
            var index = lines.FindIndex(l => l.ProductId == productId);
            if (index >= 0)
            {
                lines[index] = new OrderLineInput(productId, lines[index].Quantity + quantity);
            }
            else
            {
                lines.Add(new OrderLineInput(productId, quantity));
            }

            DraftLines = lines;
            QuantityBox.Text = string.Empty;
            ProductList.SelectedIndex = 0;
            ProductList.Focus();
        }

        protected void Lines_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Remove")
            {
                var productId = int.Parse((string)e.CommandArgument, CultureInfo.InvariantCulture);
                DraftLines = DraftLines.Where(l => l.ProductId != productId).ToList();
            }
        }

        protected void CreateOrder_Click(object sender, EventArgs e)
        {
            if (!IsValid)
            {
                return;
            }

            var lines = DraftLines;
            if (lines.Count == 0)
            {
                ShowAlert("Add at least one line before creating the order.", AlertKind.Warning);
                return;
            }

            var customerId = int.Parse(CustomerList.SelectedValue, CultureInfo.InvariantCulture);
            Order order = null;
            if (TryRun(() => order = new OrderService(Db).Create(customerId, lines, ReferenceBox.Text, CurrentUserName)))
            {
                RedirectWithMessage(OrderUrl(order.Id), $"Order {order.Number} created and queued for picking.");
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            var products = ProductOptions.ToDictionary(p => p.Id);
            var rows = DraftLines
                .Where(l => products.ContainsKey(l.ProductId))
                .Select(l => new DraftLineView(products[l.ProductId], l.Quantity))
                .ToList();

            LineRepeater.DataSource = rows;
            LineRepeater.DataBind();
            LineRepeater.Visible = rows.Count > 0;
            NoLines.Visible = rows.Count == 0;
            ShortageNote.Visible = rows.Any(r => r.Shortfall > 0);
            LineCount = rows.Count;
            TotalUnits = rows.Sum(r => r.Quantity);
        }
    }

    /// <summary>A draft order line as shown on the new order screen.</summary>
    public class DraftLineView
    {
        public DraftLineView(ProductOption product, int quantity)
        {
            ProductId = product.Id;
            Sku = product.Sku;
            Name = product.Name;
            Available = product.QuantityAvailable;
            Quantity = quantity;
        }

        public int ProductId { get; }

        public string Sku { get; }

        public string Name { get; }

        public int Available { get; }

        public int Quantity { get; }

        public int Shortfall => Math.Max(0, Quantity - Available);
    }
}
