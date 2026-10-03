<%@ Page Title="Stock levels" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Warehouse.Web.Inventory.StockLevels" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <div>
            <h1 class="page-title">Stock levels</h1>
            <p class="page-subtitle">On hand is what is physically in the warehouse; reserved stock is allocated to picked orders.</p>
        </div>
        <div class="page-actions">
            <a class="btn btn-primary" href="<%: ResolveUrl("~/inventory/receive") %>"><%= Icons.Get("receive") %> Receive goods</a>
        </div>
    </div>

    <section class="card">
        <asp:Panel runat="server" CssClass="toolbar" DefaultButton="SearchButton">
            <nav class="filter-tabs" aria-label="Filter">
                <a class="<%: LowStockOnly ? "filter-tab" : "filter-tab active" %>" href="<%: ResolveUrl("~/inventory") %>">All products</a>
                <a class="<%: LowStockOnly ? "filter-tab active" : "filter-tab" %>" href="<%: ResolveUrl("~/inventory?filter=low") %>">Needs replenishment</a>
            </nav>
            <div class="toolbar-end">
                <div class="search-box">
                    <%= Icons.Get("search") %>
                    <asp:TextBox ID="SearchBox" runat="server" CssClass="form-control" placeholder="Search by SKU or name" aria-label="Search stock" />
                </div>
                <asp:Button ID="SearchButton" runat="server" Text="Search" CssClass="btn btn-outline-secondary" OnClick="Search_Click" CausesValidation="false" />
            </div>
        </asp:Panel>

        <asp:ListView ID="StockGrid" runat="server" ItemType="Warehouse.Core.Services.ProductListItem" SelectMethod="GetStockLevels">
            <LayoutTemplate>
                <div class="table-responsive">
                    <table class="table data-table">
                        <thead>
                            <tr>
                                <th scope="col"><asp:LinkButton runat="server" CommandName="Sort" CommandArgument="Sku" Text="SKU" /></th>
                                <th scope="col"><asp:LinkButton runat="server" CommandName="Sort" CommandArgument="Name" Text="Product" /></th>
                                <th scope="col" class="text-end"><asp:LinkButton runat="server" CommandName="Sort" CommandArgument="QuantityOnHand" Text="On hand" /></th>
                                <th scope="col" class="text-end"><asp:LinkButton runat="server" CommandName="Sort" CommandArgument="QuantityReserved" Text="Reserved" /></th>
                                <th scope="col" class="text-end"><asp:LinkButton runat="server" CommandName="Sort" CommandArgument="QuantityAvailable" Text="Available" /></th>
                                <th scope="col" class="stock-bar-col">Availability</th>
                                <th scope="col">Status</th>
                                <th scope="col"><span class="visually-hidden">Actions</span></th>
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
                    <td><a class="sku" href="<%#: ProductUrl(Item.Id) %>"><%#: Item.Sku %></a></td>
                    <td><%#: Item.Name %></td>
                    <td class="text-end num"><%#: Display.Number(Item.QuantityOnHand) %></td>
                    <td class="text-end num text-muted"><%#: Display.Number(Item.QuantityReserved) %></td>
                    <td class="text-end num fw-semibold"><%#: Display.Number(Item.QuantityAvailable) %></td>
                    <td class="stock-bar-col">
                        <span class="stock-bar" title="Reorder level <%#: Display.Number(Item.ReorderLevel) %>">
                            <span class="stock-bar-fill <%#: Item.StockStatus.ToString().ToLowerInvariant() %>" style="width: <%#: BarWidth(Item) %>%"></span>
                            <span class="stock-bar-marker" style="left: <%#: MarkerPosition(Item) %>%"></span>
                        </span>
                    </td>
                    <td><%# Display.StockBadge(Item.StockStatus) %></td>
                    <td class="text-end"><a class="btn btn-sm btn-light" href="<%#: ResolveUrl("~/inventory/receive?product=" + Item.Id) %>">Receive</a></td>
                </tr>
            </ItemTemplate>
            <EmptyDataTemplate>
                <div class="empty-state">
                    <%= Icons.Get("check", "icon icon-lg") %>
                    <p><%: LowStockOnly ? "Every product is above its reorder level." : "No products match your search." %></p>
                </div>
            </EmptyDataTemplate>
        </asp:ListView>
    </section>
</asp:Content>
