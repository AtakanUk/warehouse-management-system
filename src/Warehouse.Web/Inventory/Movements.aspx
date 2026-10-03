<%@ Page Title="Stock ledger" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Movements.aspx.cs" Inherits="Warehouse.Web.Inventory.Movements" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <div>
            <h1 class="page-title">Stock ledger</h1>
            <p class="page-subtitle">Every receipt and shipment that changed the stock on hand, newest first.</p>
        </div>
    </div>

    <section class="card">
        <asp:Panel runat="server" CssClass="toolbar" DefaultButton="SearchButton">
            <nav class="filter-tabs" aria-label="Movement type">
                <a class="<%: TabClass(null) %>" href="<%: FilterUrl(null) %>">All</a>
                <a class="<%: TabClass("receipt") %>" href="<%: FilterUrl("receipt") %>">Receipts</a>
                <a class="<%: TabClass("shipment") %>" href="<%: FilterUrl("shipment") %>">Shipments</a>
            </nav>
            <div class="toolbar-end">
                <div class="search-box">
                    <%= Icons.Get("search") %>
                    <asp:TextBox ID="SearchBox" runat="server" CssClass="form-control" placeholder="SKU, product or reference" aria-label="Search the ledger" />
                </div>
                <asp:Button ID="SearchButton" runat="server" Text="Search" CssClass="btn btn-outline-secondary" OnClick="Search_Click" CausesValidation="false" />
            </div>
        </asp:Panel>

        <asp:ListView ID="MovementGrid" runat="server" ItemType="Warehouse.Core.Services.StockMovementItem" SelectMethod="GetMovements">
            <LayoutTemplate>
                <div class="table-responsive">
                    <table class="table data-table">
                        <thead>
                            <tr>
                                <th scope="col">Date</th>
                                <th scope="col">Product</th>
                                <th scope="col">Type</th>
                                <th scope="col" class="text-end">Quantity</th>
                                <th scope="col">Reference</th>
                                <th scope="col">By</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr runat="server" id="itemPlaceholder" />
                        </tbody>
                    </table>
                </div>
                <asp:DataPager runat="server" PageSize="15" CssClass="pager">
                    <Fields>
                        <asp:NextPreviousPagerField ButtonType="Link" ShowNextPageButton="false" PreviousPageText="‹ Previous" ButtonCssClass="pager-link" RenderDisabledButtonsAsLabels="true" />
                        <asp:NumericPagerField ButtonType="Link" NumericButtonCssClass="pager-link" CurrentPageLabelCssClass="pager-link current" NextPreviousButtonCssClass="pager-link" />
                        <asp:NextPreviousPagerField ButtonType="Link" ShowPreviousPageButton="false" NextPageText="Next ›" ButtonCssClass="pager-link" RenderDisabledButtonsAsLabels="true" />
                    </Fields>
                </asp:DataPager>
            </LayoutTemplate>
            <ItemTemplate>
                <tr>
                    <td class="text-nowrap"><%#: Display.Date(Item.CreatedAtUtc) %></td>
                    <td><a class="sku" href="<%#: ProductUrl(Item.ProductId) %>"><%#: Item.Sku %></a> <%#: Item.ProductName %></td>
                    <td><%# Display.MovementBadge(Item.Type) %></td>
                    <td class="text-end num <%#: Item.Quantity > 0 ? "qty-in" : "qty-out" %>"><%#: Display.SignedQuantity(Item.Quantity) %></td>
                    <td><%# Item.OrderId.HasValue ? "<a href=\"" + OrderUrl(Item.OrderId.Value) + "\">" + Encode(Item.Reference) + "</a>" : Display.Text(Item.Reference) %></td>
                    <td><%#: Item.CreatedBy %></td>
                </tr>
            </ItemTemplate>
            <EmptyDataTemplate>
                <div class="empty-state">
                    <%= Icons.Get("history", "icon icon-lg") %>
                    <p>No stock movements match the filter.</p>
                </div>
            </EmptyDataTemplate>
        </asp:ListView>
    </section>
</asp:Content>
