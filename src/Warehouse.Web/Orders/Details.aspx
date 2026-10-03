<%@ Page Title="Order" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Details.aspx.cs" Inherits="Warehouse.Web.Orders.OrderDetails" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <nav class="breadcrumb-nav" aria-label="Breadcrumb">
        <a href="<%: ResolveUrl("~/orders") %>">Orders</a> <span aria-hidden="true">/</span> <span><%: Order.Number %></span>
    </nav>
    <div class="page-header">
        <div>
            <h1 class="page-title"><%: Order.Number %> <%= Display.OrderStatusBadge(Order.Status) %></h1>
            <p class="page-subtitle"><%: Order.Customer.Name %><%: string.IsNullOrEmpty(Order.CustomerReference) ? string.Empty : " · Ref. " + Order.CustomerReference %></p>
        </div>
    </div>

    <div class="form-layout">
        <div>
            <section class="card">
                <div class="card-header-row">
                    <h2 class="card-title">Lines</h2>
                    <span class="text-muted small"><%: Order.Lines.Count %> line(s) · <%: Display.Number(Order.TotalUnits) %> units</span>
                </div>
                <asp:Repeater ID="LineRepeater" runat="server" ItemType="Warehouse.Core.Domain.OrderLine">
                    <HeaderTemplate>
                        <div class="table-responsive">
                            <table class="table data-table mb-0">
                                <thead>
                                    <tr>
                                        <th scope="col">Product</th>
                                        <th scope="col" class="text-end">Ordered</th>
                                        <th scope="col" class="text-end"><%# Order.Status == Warehouse.Core.Domain.OrderStatus.Open ? "Available now" : "On hand now" %></th>
                                    </tr>
                                </thead>
                                <tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td><a class="sku" href="<%#: ProductUrl(Item.ProductId) %>"><%#: Item.Product.Sku %></a> <%#: Item.Product.Name %></td>
                            <td class="text-end num fw-semibold"><%#: Display.Number(Item.Quantity) %></td>
                            <td class="text-end num">
                                <%#: Display.Number(Order.Status == Warehouse.Core.Domain.OrderStatus.Open ? Item.Product.QuantityAvailable : Item.Product.QuantityOnHand) %>
                                <%# Order.Status == Warehouse.Core.Domain.OrderStatus.Open && Item.Product.QuantityAvailable < Item.Quantity ? "<span class=\"badge-pill stock-low ms-1\">short</span>" : string.Empty %>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                                </tbody>
                            </table>
                        </div>
                    </FooterTemplate>
                </asp:Repeater>
            </section>

            <section class="card card-body-padded">
                <h2 class="card-title">Timeline</h2>
                <ol class="timeline">
                    <li class="done">
                        <span class="timeline-title">Created</span>
                        <span class="timeline-meta"><%: Display.Date(Order.CreatedAtUtc) %> by <%: Order.CreatedBy %></span>
                    </li>
                    <li class="<%: Order.PickedAtUtc.HasValue ? "done" : "pending" %>">
                        <span class="timeline-title">Picked · stock reserved</span>
                        <span class="timeline-meta"><%: Order.PickedAtUtc.HasValue ? Display.Date(Order.PickedAtUtc) + " by " + Order.PickedBy : "Waiting for picking" %></span>
                    </li>
                    <li class="<%: Order.ShippedAtUtc.HasValue ? "done" : "pending" %>">
                        <span class="timeline-title">Shipped</span>
                        <span class="timeline-meta"><%: Order.ShippedAtUtc.HasValue ? Display.Date(Order.ShippedAtUtc) + " by " + Order.ShippedBy + " via " + Order.Carrier.Name + (string.IsNullOrEmpty(Order.TrackingNumber) ? string.Empty : " · tracking " + Order.TrackingNumber) : "Not shipped yet" %></span>
                    </li>
                    <asp:PlaceHolder ID="CancelledStep" runat="server">
                        <li class="cancelled">
                            <span class="timeline-title">Cancelled</span>
                            <span class="timeline-meta"><%: Display.Date(Order.CancelledAtUtc) %> by <%: Order.CancelledBy %></span>
                        </li>
                    </asp:PlaceHolder>
                </ol>
            </section>
        </div>

        <aside class="card card-body-padded order-actions" aria-label="Order actions">
            <h2 class="card-title">Next step</h2>

            <asp:PlaceHolder ID="PickPanel" runat="server">
                <p class="text-muted small">Picking checks every line against the available stock and reserves it for this order.</p>
                <asp:Button ID="PickButton" runat="server" Text="Pick order" CssClass="btn btn-primary w-100" OnClick="Pick_Click" CausesValidation="false" />
            </asp:PlaceHolder>

            <asp:PlaceHolder ID="ShipPanel" runat="server">
                <p class="text-muted small">Hand the order to a carrier. The reserved stock leaves the warehouse.</p>
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="CarrierList" CssClass="form-label">Carrier</asp:Label>
                    <asp:DropDownList ID="CarrierList" runat="server" CssClass="form-select" SelectMethod="GetCarriers"
                        DataTextField="Name" DataValueField="Id" AppendDataBoundItems="true">
                        <asp:ListItem Text="Select a carrier…" Value="" />
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="CarrierList" ValidationGroup="ship" ErrorMessage="Select a carrier." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="TrackingBox" CssClass="form-label">Tracking number <span class="optional">optional</span></asp:Label>
                    <asp:TextBox ID="TrackingBox" runat="server" CssClass="form-control" MaxLength="50" />
                </div>
                <asp:Button ID="ShipButton" runat="server" Text="Ship order" CssClass="btn btn-primary w-100" ValidationGroup="ship" OnClick="Ship_Click" />
            </asp:PlaceHolder>

            <asp:PlaceHolder ID="DonePanel" runat="server">
                <p class="text-muted small mb-0"><%: Order.Status == Warehouse.Core.Domain.OrderStatus.Shipped ? "This order has been shipped. No further action is needed." : "This order was cancelled." %></p>
            </asp:PlaceHolder>

            <asp:PlaceHolder ID="CancelPanel" runat="server">
                <hr />
                <asp:Button ID="CancelButton" runat="server" Text="Cancel order" CssClass="btn btn-outline-danger w-100" OnClick="Cancel_Click" CausesValidation="false"
                    data-confirm="Cancel this order? Any reserved stock is released." />
            </asp:PlaceHolder>
        </aside>
    </div>
</asp:Content>
