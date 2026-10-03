<%@ Page Title="Orders" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Warehouse.Web.Orders.OrderList" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <div>
            <h1 class="page-title">Orders</h1>
            <p class="page-subtitle">Outbound orders move from <em>open</em> to <em>picked</em> to <em>shipped</em>.</p>
        </div>
        <div class="page-actions">
            <a class="btn btn-primary" href="<%: ResolveUrl("~/orders/new") %>"><%= Icons.Get("plus") %> New order</a>
        </div>
    </div>

    <section class="card">
        <asp:Panel runat="server" CssClass="toolbar" DefaultButton="SearchButton">
            <nav class="filter-tabs" aria-label="Order status">
                <a class="<%: TabClass(null) %>" href="<%: FilterUrl(null) %>">All <span class="count"><%: Display.Number(TotalCount) %></span></a>
                <a class="<%: TabClass("open") %>" href="<%: FilterUrl("open") %>">Open <span class="count"><%: Display.Number(CountOf(Warehouse.Core.Domain.OrderStatus.Open)) %></span></a>
                <a class="<%: TabClass("picked") %>" href="<%: FilterUrl("picked") %>">Picked <span class="count"><%: Display.Number(CountOf(Warehouse.Core.Domain.OrderStatus.Picked)) %></span></a>
                <a class="<%: TabClass("shipped") %>" href="<%: FilterUrl("shipped") %>">Shipped <span class="count"><%: Display.Number(CountOf(Warehouse.Core.Domain.OrderStatus.Shipped)) %></span></a>
                <a class="<%: TabClass("cancelled") %>" href="<%: FilterUrl("cancelled") %>">Cancelled <span class="count"><%: Display.Number(CountOf(Warehouse.Core.Domain.OrderStatus.Cancelled)) %></span></a>
            </nav>
            <div class="toolbar-end">
                <div class="search-box">
                    <%= Icons.Get("search") %>
                    <asp:TextBox ID="SearchBox" runat="server" CssClass="form-control" placeholder="Order no., customer or reference" aria-label="Search orders" />
                </div>
                <asp:Button ID="SearchButton" runat="server" Text="Search" CssClass="btn btn-outline-secondary" OnClick="Search_Click" CausesValidation="false" />
            </div>
        </asp:Panel>

        <asp:ListView ID="OrderGrid" runat="server" ItemType="Warehouse.Core.Services.OrderListItem" SelectMethod="GetOrders">
            <LayoutTemplate>
                <div class="table-responsive">
                    <table class="table data-table">
                        <thead>
                            <tr>
                                <th scope="col"><asp:LinkButton runat="server" CommandName="Sort" CommandArgument="Id" Text="Order" /></th>
                                <th scope="col"><asp:LinkButton runat="server" CommandName="Sort" CommandArgument="CustomerName" Text="Customer" /></th>
                                <th scope="col">Reference</th>
                                <th scope="col"><asp:LinkButton runat="server" CommandName="Sort" CommandArgument="CreatedAtUtc" Text="Created" /></th>
                                <th scope="col" class="text-end"><asp:LinkButton runat="server" CommandName="Sort" CommandArgument="TotalUnits" Text="Units" /></th>
                                <th scope="col">Status</th>
                                <th scope="col">Carrier</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr runat="server" id="itemPlaceholder" />
                        </tbody>
                    </table>
                </div>
                <asp:DataPager runat="server" PageSize="12" CssClass="pager">
                    <Fields>
                        <asp:NextPreviousPagerField ButtonType="Link" ShowNextPageButton="false" PreviousPageText="‹ Previous" ButtonCssClass="pager-link" RenderDisabledButtonsAsLabels="true" />
                        <asp:NumericPagerField ButtonType="Link" NumericButtonCssClass="pager-link" CurrentPageLabelCssClass="pager-link current" NextPreviousButtonCssClass="pager-link" />
                        <asp:NextPreviousPagerField ButtonType="Link" ShowPreviousPageButton="false" NextPageText="Next ›" ButtonCssClass="pager-link" RenderDisabledButtonsAsLabels="true" />
                    </Fields>
                </asp:DataPager>
            </LayoutTemplate>
            <ItemTemplate>
                <tr>
                    <td><a class="fw-semibold" href="<%#: OrderUrl(Item.Id) %>"><%#: Item.Number %></a></td>
                    <td><%#: Item.CustomerName %></td>
                    <td><%# Display.Text(Item.CustomerReference) %></td>
                    <td class="text-nowrap" title="<%#: Display.Date(Item.CreatedAtUtc) %>"><%#: Display.Ago(Item.CreatedAtUtc) %></td>
                    <td class="text-end num"><%#: Display.Number(Item.TotalUnits) %> <span class="text-muted small">/ <%#: Item.LineCount %> line(s)</span></td>
                    <td><%# Display.OrderStatusBadge(Item.Status) %></td>
                    <td><%# Display.Text(Item.CarrierName) %></td>
                </tr>
            </ItemTemplate>
            <EmptyDataTemplate>
                <div class="empty-state">
                    <%= Icons.Get("orders", "icon icon-lg") %>
                    <p>No orders match the filter.</p>
                </div>
            </EmptyDataTemplate>
        </asp:ListView>
    </section>
</asp:Content>
