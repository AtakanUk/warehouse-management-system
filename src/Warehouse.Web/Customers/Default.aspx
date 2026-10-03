<%@ Page Title="Customers" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Warehouse.Web.Customers.CustomerList" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <div>
            <h1 class="page-title">Customers</h1>
            <p class="page-subtitle">Companies that place outbound orders.</p>
        </div>
        <div class="page-actions">
            <a class="btn btn-primary" href="<%: ResolveUrl("~/customers/new") %>"><%= Icons.Get("plus") %> New customer</a>
        </div>
    </div>

    <section class="card">
        <asp:Panel runat="server" CssClass="toolbar" DefaultButton="SearchButton">
            <div class="search-box">
                <%= Icons.Get("search") %>
                <asp:TextBox ID="SearchBox" runat="server" CssClass="form-control" placeholder="Search by name, contact or email" aria-label="Search customers" />
            </div>
            <asp:Button ID="SearchButton" runat="server" Text="Search" CssClass="btn btn-outline-secondary" OnClick="Search_Click" CausesValidation="false" />
        </asp:Panel>

        <asp:ListView ID="CustomerGrid" runat="server" ItemType="Warehouse.Core.Services.PartnerListItem"
            SelectMethod="GetCustomers" DeleteMethod="DeleteCustomer" DataKeyNames="Id">
            <LayoutTemplate>
                <div class="table-responsive">
                    <table class="table data-table">
                        <thead>
                            <tr>
                                <th scope="col"><asp:LinkButton runat="server" CommandName="Sort" CommandArgument="Name" Text="Name" /></th>
                                <th scope="col">Contact</th>
                                <th scope="col">Email</th>
                                <th scope="col">Phone</th>
                                <th scope="col" class="text-end"><asp:LinkButton runat="server" CommandName="Sort" CommandArgument="OrderCount" Text="Orders" /></th>
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
                    <td><a class="fw-semibold" href="<%#: ResolveUrl("~/customers/" + Item.Id + "/edit") %>"><%#: Item.Name %></a></td>
                    <td><%# Display.Text(Item.ContactName) %></td>
                    <td><%# string.IsNullOrEmpty(Item.Email) ? Display.Text(null) : "<a href=\"mailto:" + Encode(Item.Email) + "\">" + Encode(Item.Email) + "</a>" %></td>
                    <td class="text-nowrap"><%# Display.Text(Item.Phone) %></td>
                    <td class="text-end num"><a href="<%#: ResolveUrl("~/orders?q=" + Server.UrlEncode(Item.Name)) %>"><%#: Display.Number(Item.OrderCount) %></a></td>
                    <td class="text-end text-nowrap">
                        <a class="btn btn-sm btn-light" href="<%#: ResolveUrl("~/customers/" + Item.Id + "/edit") %>">Edit</a>
                        <asp:LinkButton runat="server" CommandName="Delete" Text="Delete" CssClass="btn btn-sm btn-light text-danger" Visible="<%# Item.OrderCount == 0 %>" data-confirm='<%# "Delete " + Item.Name + "?" %>' />
                    </td>
                </tr>
            </ItemTemplate>
            <EmptyDataTemplate>
                <div class="empty-state">
                    <%= Icons.Get("users", "icon icon-lg") %>
                    <p>No customers match your search.</p>
                </div>
            </EmptyDataTemplate>
        </asp:ListView>
    </section>
</asp:Content>
