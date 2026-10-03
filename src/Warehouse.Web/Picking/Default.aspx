<%@ Page Title="Picking" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Warehouse.Web.Picking.PickingQueue" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <div>
            <h1 class="page-title">Picking</h1>
            <p class="page-subtitle">Open orders, oldest first. Picking reserves the stock of every line in one step.</p>
        </div>
    </div>

    <section class="card">
        <asp:ListView ID="QueueGrid" runat="server" ItemType="Warehouse.Core.Services.PickingQueueItem" SelectMethod="GetQueue" OnItemCommand="Queue_ItemCommand">
            <LayoutTemplate>
                <div class="table-responsive">
                    <table class="table data-table">
                        <thead>
                            <tr>
                                <th scope="col">Order</th>
                                <th scope="col">Customer</th>
                                <th scope="col">Waiting</th>
                                <th scope="col" class="text-end">Units</th>
                                <th scope="col">Stock check</th>
                                <th scope="col"><span class="visually-hidden">Actions</span></th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr runat="server" id="itemPlaceholder" />
                        </tbody>
                    </table>
                </div>
            </LayoutTemplate>
            <ItemTemplate>
                <tr>
                    <td><a class="fw-semibold" href="<%#: OrderUrl(Item.Id) %>"><%#: Item.Number %></a></td>
                    <td><%#: Item.CustomerName %></td>
                    <td class="text-nowrap" title="<%#: Display.Date(Item.CreatedAtUtc) %>"><%#: Display.Ago(Item.CreatedAtUtc) %></td>
                    <td class="text-end num"><%#: Display.Number(Item.TotalUnits) %> <span class="text-muted small">/ <%#: Item.LineCount %> line(s)</span></td>
                    <td><%# Item.IsReady ? "<span class=\"badge-pill stock-ok\">All lines available</span>" : "<span class=\"badge-pill stock-low\">" + Item.ShortLineCount + " line(s) short</span>" %></td>
                    <td class="text-end text-nowrap">
                        <asp:Button runat="server" Text="Pick" CommandName="Pick" CommandArgument="<%# Item.Id %>" CausesValidation="false"
                            CssClass="btn btn-sm btn-primary" Enabled="<%# Item.IsReady %>" ToolTip='<%# Item.IsReady ? "Reserve the stock for this order" : "Receive the missing stock first" %>' />
                        <a class="btn btn-sm btn-light" href="<%#: OrderUrl(Item.Id) %>">Details</a>
                    </td>
                </tr>
            </ItemTemplate>
            <EmptyDataTemplate>
                <div class="empty-state">
                    <%= Icons.Get("check", "icon icon-lg") %>
                    <p>No open orders. Everything has been picked.</p>
                </div>
            </EmptyDataTemplate>
        </asp:ListView>
    </section>
</asp:Content>
