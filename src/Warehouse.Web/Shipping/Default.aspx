<%@ Page Title="Shipping" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Warehouse.Web.Shipping.ShippingQueue" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <div>
            <h1 class="page-title">Shipping</h1>
            <p class="page-subtitle">Picked orders waiting at the dock. Assign a carrier to ship them.</p>
        </div>
    </div>

    <section class="card">
        <asp:ListView ID="QueueGrid" runat="server" ItemType="Warehouse.Core.Services.ShippingQueueItem" SelectMethod="GetQueue" OnItemCommand="Queue_ItemCommand">
            <LayoutTemplate>
                <div class="table-responsive">
                    <table class="table data-table align-middle">
                        <thead>
                            <tr>
                                <th scope="col">Order</th>
                                <th scope="col">Customer</th>
                                <th scope="col">Picked</th>
                                <th scope="col" class="text-end">Units</th>
                                <th scope="col" class="ship-col">Carrier</th>
                                <th scope="col" class="ship-col">Tracking number</th>
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
                    <td class="text-nowrap" title="<%#: Display.Date(Item.PickedAtUtc) %>"><%#: Display.Ago(Item.PickedAtUtc) %> <span class="text-muted small">by <%#: Item.PickedBy %></span></td>
                    <td class="text-end num"><%#: Display.Number(Item.TotalUnits) %></td>
                    <td class="ship-col">
                        <asp:DropDownList ID="CarrierList" runat="server" CssClass="form-select form-select-sm" SelectMethod="GetCarriers"
                            DataTextField="Name" DataValueField="Id" AppendDataBoundItems="true" aria-label="Carrier">
                            <asp:ListItem Text="Select a carrier…" Value="" />
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="CarrierList" ValidationGroup='<%# "ship" + Item.Id %>' ErrorMessage="Select a carrier." Display="Dynamic" CssClass="field-error" />
                    </td>
                    <td class="ship-col">
                        <asp:TextBox ID="TrackingBox" runat="server" CssClass="form-control form-control-sm" MaxLength="50" placeholder="optional" aria-label="Tracking number" />
                    </td>
                    <td class="text-end">
                        <asp:Button runat="server" Text="Ship" CommandName="Ship" CommandArgument="<%# Item.Id %>" ValidationGroup='<%# "ship" + Item.Id %>' CssClass="btn btn-sm btn-primary" />
                    </td>
                </tr>
            </ItemTemplate>
            <EmptyDataTemplate>
                <div class="empty-state">
                    <%= Icons.Get("truck", "icon icon-lg") %>
                    <p>Nothing to ship. Picked orders appear here.</p>
                </div>
            </EmptyDataTemplate>
        </asp:ListView>
    </section>
</asp:Content>
