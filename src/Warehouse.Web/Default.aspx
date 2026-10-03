<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Warehouse.Web.DefaultPage" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <div>
            <h1 class="page-title">Good <%: DayPart %>, <%: CurrentUserName %></h1>
            <p class="page-subtitle">Here is what is happening in the warehouse today.</p>
        </div>
        <div class="page-actions">
            <a class="btn btn-outline-secondary" href="<%: ResolveUrl("~/inventory/receive") %>"><%= Icons.Get("receive") %> Receive goods</a>
            <a class="btn btn-primary" href="<%: ResolveUrl("~/orders/new") %>"><%= Icons.Get("plus") %> New order</a>
        </div>
    </div>

    <section class="kpi-grid" aria-label="Key figures">
        <a class="kpi" href="<%: ResolveUrl("~/products") %>">
            <span class="kpi-icon tone-teal"><%= Icons.Get("package") %></span>
            <span class="kpi-label">Active SKUs</span>
            <span class="kpi-value"><%: Display.Number(Summary.ProductCount) %></span>
            <span class="kpi-meta"><%: Display.Number(Summary.UnitsOnHand) %> units on hand</span>
        </a>
        <a class="kpi" href="<%: ResolveUrl("~/inventory?filter=low") %>">
            <span class="kpi-icon tone-amber"><%= Icons.Get("alert") %></span>
            <span class="kpi-label">Needs replenishment</span>
            <span class="kpi-value"><%: Display.Number(Summary.LowStockCount) %></span>
            <span class="kpi-meta">at or below reorder level</span>
        </a>
        <a class="kpi" href="<%: ResolveUrl("~/picking") %>">
            <span class="kpi-icon tone-blue"><%= Icons.Get("picking") %></span>
            <span class="kpi-label">Waiting to be picked</span>
            <span class="kpi-value"><%: Display.Number(Summary.OpenOrders) %></span>
            <span class="kpi-meta">open orders</span>
        </a>
        <a class="kpi" href="<%: ResolveUrl("~/shipping") %>">
            <span class="kpi-icon tone-violet"><%= Icons.Get("truck") %></span>
            <span class="kpi-label">Ready to ship</span>
            <span class="kpi-value"><%: Display.Number(Summary.PickedOrders) %></span>
            <span class="kpi-meta"><%: Display.Number(Summary.UnitsReserved) %> units reserved</span>
        </a>
        <a class="kpi" href="<%: ResolveUrl("~/orders?status=shipped") %>">
            <span class="kpi-icon tone-green"><%= Icons.Get("check") %></span>
            <span class="kpi-label">Shipped this week</span>
            <span class="kpi-value"><%: Display.Number(Summary.ShippedLast7Days) %></span>
            <span class="kpi-meta">last 7 days</span>
        </a>
    </section>

    <div class="dashboard-grid">
        <section class="card">
            <div class="card-header-row">
                <h2 class="card-title">Orders awaiting action</h2>
                <a class="card-link" href="<%: ResolveUrl("~/orders") %>">All orders <%= Icons.Get("arrow-right", "icon icon-sm") %></a>
            </div>
            <asp:Repeater ID="AwaitingOrders" runat="server" ItemType="Warehouse.Core.Services.OrderListItem">
                <HeaderTemplate><ul class="item-list"></HeaderTemplate>
                <ItemTemplate>
                    <li>
                        <a class="item-row" href="<%#: OrderUrl(Item.Id) %>">
                            <span class="item-main">
                                <span class="item-title"><%#: Item.Number %> · <%#: Item.CustomerName %></span>
                                <span class="item-sub"><%#: Item.LineCount %> line(s), <%#: Display.Number(Item.TotalUnits) %> units · created <%#: Display.Ago(Item.CreatedAtUtc) %></span>
                            </span>
                            <%# Display.OrderStatusBadge(Item.Status) %>
                        </a>
                    </li>
                </ItemTemplate>
                <FooterTemplate></ul></FooterTemplate>
            </asp:Repeater>
            <asp:PlaceHolder ID="NoAwaitingOrders" runat="server" Visible="false">
                <p class="empty-state">Nothing to do — every order has been shipped.</p>
            </asp:PlaceHolder>
        </section>

        <section class="card">
            <div class="card-header-row">
                <h2 class="card-title">Low stock</h2>
                <a class="card-link" href="<%: ResolveUrl("~/inventory?filter=low") %>">Stock levels <%= Icons.Get("arrow-right", "icon icon-sm") %></a>
            </div>
            <asp:Repeater ID="LowStock" runat="server" ItemType="Warehouse.Core.Services.ProductListItem">
                <HeaderTemplate><ul class="item-list"></HeaderTemplate>
                <ItemTemplate>
                    <li>
                        <a class="item-row" href="<%#: ResolveUrl("~/inventory/receive?product=" + Item.Id) %>" title="Receive stock for <%#: Item.Sku %>">
                            <span class="item-main">
                                <span class="item-title"><span class="sku"><%#: Item.Sku %></span> <%#: Item.Name %></span>
                                <span class="item-sub"><%#: Display.Number(Item.QuantityAvailable) %> available · reorder at <%#: Display.Number(Item.ReorderLevel) %></span>
                            </span>
                            <%# Display.StockBadge(Item.StockStatus) %>
                        </a>
                    </li>
                </ItemTemplate>
                <FooterTemplate></ul></FooterTemplate>
            </asp:Repeater>
            <asp:PlaceHolder ID="NoLowStock" runat="server" Visible="false">
                <p class="empty-state">All products are above their reorder level.</p>
            </asp:PlaceHolder>
        </section>
    </div>

    <section class="card">
        <div class="card-header-row">
            <h2 class="card-title">Recent stock movements</h2>
            <a class="card-link" href="<%: ResolveUrl("~/inventory/movements") %>">Stock ledger <%= Icons.Get("arrow-right", "icon icon-sm") %></a>
        </div>
        <asp:Repeater ID="RecentMovements" runat="server" ItemType="Warehouse.Core.Services.StockMovementItem">
            <HeaderTemplate>
                <div class="table-responsive">
                    <table class="table data-table">
                        <thead>
                            <tr>
                                <th scope="col">When</th>
                                <th scope="col">Product</th>
                                <th scope="col">Type</th>
                                <th scope="col" class="text-end">Quantity</th>
                                <th scope="col">Reference</th>
                                <th scope="col">By</th>
                            </tr>
                        </thead>
                        <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td class="text-nowrap" title="<%#: Display.Date(Item.CreatedAtUtc) %>"><%#: Display.Ago(Item.CreatedAtUtc) %></td>
                    <td><span class="sku"><%#: Item.Sku %></span> <%#: Item.ProductName %></td>
                    <td><%# Display.MovementBadge(Item.Type) %></td>
                    <td class="text-end num <%#: Item.Quantity > 0 ? "qty-in" : "qty-out" %>"><%#: Display.SignedQuantity(Item.Quantity) %></td>
                    <td><%# Item.OrderId.HasValue ? "<a href=\"" + OrderUrl(Item.OrderId.Value) + "\">" + Encode(Item.Reference) + "</a>" : Display.Text(Item.Reference) %></td>
                    <td><%#: Item.CreatedBy %></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                        </tbody>
                    </table>
                </div>
            </FooterTemplate>
        </asp:Repeater>
    </section>
</asp:Content>
