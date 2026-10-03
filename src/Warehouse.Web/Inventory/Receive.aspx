<%@ Page Title="Receive goods" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Receive.aspx.cs" Inherits="Warehouse.Web.Inventory.Receive" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <div>
            <h1 class="page-title">Receive goods</h1>
            <p class="page-subtitle">Book an inbound delivery. Every receipt is written to the stock ledger.</p>
        </div>
    </div>

    <div class="form-layout">
        <section class="card card-body-padded">
            <h2 class="card-title">Goods receipt</h2>
            <asp:Panel runat="server" DefaultButton="ReceiveButton" CssClass="row g-3">
                <div class="col-12">
                    <asp:Label runat="server" AssociatedControlID="ProductList" CssClass="form-label">Product</asp:Label>
                    <asp:DropDownList ID="ProductList" runat="server" CssClass="form-select" SelectMethod="GetProducts"
                        DataTextField="DisplayName" DataValueField="Id" AppendDataBoundItems="true">
                        <asp:ListItem Text="Select a product…" Value="" />
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="ProductList" ErrorMessage="Select a product." Display="Dynamic" CssClass="field-error" SetFocusOnError="true" />
                </div>
                <div class="col-sm-5">
                    <asp:Label runat="server" AssociatedControlID="QuantityBox" CssClass="form-label">Quantity received</asp:Label>
                    <asp:TextBox ID="QuantityBox" runat="server" CssClass="form-control" TextMode="Number" min="1" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="QuantityBox" ErrorMessage="Enter the quantity." Display="Dynamic" CssClass="field-error" SetFocusOnError="true" />
                    <asp:RangeValidator runat="server" ControlToValidate="QuantityBox" Type="Integer" MinimumValue="1" MaximumValue="1000000" ErrorMessage="Enter a whole number greater than zero." Display="Dynamic" CssClass="field-error" SetFocusOnError="true" />
                </div>
                <div class="col-sm-7">
                    <asp:Label runat="server" AssociatedControlID="ReferenceBox" CssClass="form-label">Delivery note / PO <span class="optional">optional</span></asp:Label>
                    <asp:TextBox ID="ReferenceBox" runat="server" CssClass="form-control" MaxLength="50" placeholder="PO-24031" />
                </div>
                <div class="col-12 form-actions">
                    <asp:Button ID="ReceiveButton" runat="server" Text="Book receipt" CssClass="btn btn-primary" OnClick="Receive_Click" />
                </div>
            </asp:Panel>
        </section>

        <aside class="card card-body-padded help-card">
            <h2 class="card-title">How stock is tracked</h2>
            <ul class="help-list">
                <li><strong>Receiving</strong> increases the quantity on hand.</li>
                <li><strong>Picking</strong> reserves stock for an order; it stays on hand until shipped.</li>
                <li><strong>Shipping</strong> removes the reserved stock and records a ledger entry.</li>
            </ul>
        </aside>
    </div>

    <section class="card">
        <div class="card-header-row">
            <h2 class="card-title">Latest receipts</h2>
            <a class="card-link" href="<%: ResolveUrl("~/inventory/movements?type=receipt") %>">All receipts <%= Icons.Get("arrow-right", "icon icon-sm") %></a>
        </div>
        <asp:ListView ID="ReceiptGrid" runat="server" ItemType="Warehouse.Core.Services.StockMovementItem" SelectMethod="GetRecentReceipts">
            <LayoutTemplate>
                <div class="table-responsive">
                    <table class="table data-table">
                        <thead>
                            <tr>
                                <th scope="col">Received</th>
                                <th scope="col">Product</th>
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
            </LayoutTemplate>
            <ItemTemplate>
                <tr>
                    <td class="text-nowrap" title="<%#: Display.Date(Item.CreatedAtUtc) %>"><%#: Display.Ago(Item.CreatedAtUtc) %></td>
                    <td><span class="sku"><%#: Item.Sku %></span> <%#: Item.ProductName %></td>
                    <td class="text-end num qty-in"><%#: Display.SignedQuantity(Item.Quantity) %></td>
                    <td><%# Display.Text(Item.Reference) %></td>
                    <td><%#: Item.CreatedBy %></td>
                </tr>
            </ItemTemplate>
            <EmptyDataTemplate>
                <p class="empty-state">No goods have been received yet.</p>
            </EmptyDataTemplate>
        </asp:ListView>
    </section>
</asp:Content>
