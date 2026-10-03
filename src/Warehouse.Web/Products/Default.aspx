<%@ Page Title="Products" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Warehouse.Web.Products.ProductList" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <div>
            <h1 class="page-title">Products</h1>
            <p class="page-subtitle">The catalog of stock-keeping units handled in the warehouse.</p>
        </div>
        <div class="page-actions">
            <a class="btn btn-primary" href="<%: ResolveUrl("~/products/new") %>"><%= Icons.Get("plus") %> New product</a>
        </div>
    </div>

    <section class="card">
        <asp:Panel runat="server" CssClass="toolbar" DefaultButton="SearchButton">
            <div class="search-box">
                <%= Icons.Get("search") %>
                <asp:TextBox ID="SearchBox" runat="server" CssClass="form-control" placeholder="Search by SKU or name" aria-label="Search products" />
            </div>
            <asp:Button ID="SearchButton" runat="server" Text="Search" CssClass="btn btn-outline-secondary" OnClick="Search_Click" CausesValidation="false" />
        </asp:Panel>

        <asp:ListView ID="ProductGrid" runat="server" ItemType="Warehouse.Core.Services.ProductListItem"
            SelectMethod="GetProducts" DeleteMethod="DeleteProduct" DataKeyNames="Id">
            <LayoutTemplate>
                <div class="table-responsive">
                    <table class="table data-table">
                        <thead>
                            <tr>
                                <th scope="col"><asp:LinkButton runat="server" CommandName="Sort" CommandArgument="Sku" Text="SKU" /></th>
                                <th scope="col"><asp:LinkButton runat="server" CommandName="Sort" CommandArgument="Name" Text="Name" /></th>
                                <th scope="col" class="text-end"><asp:LinkButton runat="server" CommandName="Sort" CommandArgument="QuantityOnHand" Text="On hand" /></th>
                                <th scope="col" class="text-end"><asp:LinkButton runat="server" CommandName="Sort" CommandArgument="QuantityAvailable" Text="Available" /></th>
                                <th scope="col" class="text-end"><asp:LinkButton runat="server" CommandName="Sort" CommandArgument="ReorderLevel" Text="Reorder level" /></th>
                                <th scope="col">Status</th>
                                <th scope="col"><span class="visually-hidden">Actions</span></th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr runat="server" id="itemPlaceholder" />
                        </tbody>
                    </table>
                </div>
                <asp:DataPager runat="server" PageSize="10" CssClass="pager">
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
                    <td class="text-end num"><%#: Display.Number(Item.QuantityAvailable) %></td>
                    <td class="text-end num"><%#: Display.Number(Item.ReorderLevel) %></td>
                    <td><%# Display.StockBadge(Item.StockStatus) %></td>
                    <td class="text-end text-nowrap">
                        <a class="btn btn-sm btn-light" href="<%#: ProductUrl(Item.Id) %>">Edit</a>
                        <asp:LinkButton runat="server" CommandName="Delete" Text="Delete" CssClass="btn btn-sm btn-light text-danger" Visible="<%# Item.CanDelete %>" data-confirm='<%# "Delete " + Item.Sku + "? This cannot be undone." %>' />
                    </td>
                </tr>
            </ItemTemplate>
            <EmptyDataTemplate>
                <div class="empty-state">
                    <%= Icons.Get("package", "icon icon-lg") %>
                    <p>No products match your search.</p>
                </div>
            </EmptyDataTemplate>
        </asp:ListView>
    </section>
</asp:Content>
