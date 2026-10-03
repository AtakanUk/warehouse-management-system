<%@ Page Title="New order" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="New.aspx.cs" Inherits="Warehouse.Web.Orders.NewOrder" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <nav class="breadcrumb-nav" aria-label="Breadcrumb">
        <a href="<%: ResolveUrl("~/orders") %>">Orders</a> <span aria-hidden="true">/</span> <span>New order</span>
    </nav>
    <div class="page-header">
        <div>
            <h1 class="page-title">New order</h1>
            <p class="page-subtitle">Capture what the customer ordered. Stock is reserved when the order is picked.</p>
        </div>
    </div>

    <div class="form-layout">
        <div>
            <section class="card card-body-padded">
                <h2 class="card-title">Customer</h2>
                <div class="row g-3">
                    <div class="col-sm-7">
                        <asp:Label runat="server" AssociatedControlID="CustomerList" CssClass="form-label">Customer</asp:Label>
                        <asp:DropDownList ID="CustomerList" runat="server" CssClass="form-select" SelectMethod="GetCustomers"
                            DataTextField="Name" DataValueField="Id" AppendDataBoundItems="true">
                            <asp:ListItem Text="Select a customer…" Value="" />
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="CustomerList" ValidationGroup="order" ErrorMessage="Select a customer." Display="Dynamic" CssClass="field-error" SetFocusOnError="true" />
                    </div>
                    <div class="col-sm-5">
                        <asp:Label runat="server" AssociatedControlID="ReferenceBox" CssClass="form-label">Customer reference <span class="optional">optional</span></asp:Label>
                        <asp:TextBox ID="ReferenceBox" runat="server" CssClass="form-control" MaxLength="50" placeholder="Customer PO number" />
                    </div>
                </div>
            </section>

            <section class="card">
                <div class="card-body-padded pb-0">
                    <h2 class="card-title">Order lines</h2>
                    <asp:Panel runat="server" DefaultButton="AddLineButton" CssClass="row g-2 align-items-start line-entry">
                        <div class="col-md-7">
                            <asp:Label runat="server" AssociatedControlID="ProductList" CssClass="form-label">Product</asp:Label>
                            <asp:DropDownList ID="ProductList" runat="server" CssClass="form-select" SelectMethod="GetProducts"
                                DataTextField="DisplayName" DataValueField="Id" AppendDataBoundItems="true">
                                <asp:ListItem Text="Select a product…" Value="" />
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator runat="server" ControlToValidate="ProductList" ValidationGroup="line" ErrorMessage="Select a product." Display="Dynamic" CssClass="field-error" />
                        </div>
                        <div class="col-6 col-md-3">
                            <asp:Label runat="server" AssociatedControlID="QuantityBox" CssClass="form-label">Quantity</asp:Label>
                            <asp:TextBox ID="QuantityBox" runat="server" CssClass="form-control" TextMode="Number" min="1" />
                            <asp:RequiredFieldValidator runat="server" ControlToValidate="QuantityBox" ValidationGroup="line" ErrorMessage="Enter a quantity." Display="Dynamic" CssClass="field-error" />
                            <asp:RangeValidator runat="server" ControlToValidate="QuantityBox" ValidationGroup="line" Type="Integer" MinimumValue="1" MaximumValue="100000" ErrorMessage="Whole number above zero." Display="Dynamic" CssClass="field-error" />
                        </div>
                        <div class="col-6 col-md-2">
                            <span class="form-label d-block" aria-hidden="true">&nbsp;</span>
                            <asp:Button ID="AddLineButton" runat="server" Text="Add line" CssClass="btn btn-outline-primary w-100" ValidationGroup="line" OnClick="AddLine_Click" />
                        </div>
                    </asp:Panel>
                </div>

                <asp:Repeater ID="LineRepeater" runat="server" ItemType="Warehouse.Web.Orders.DraftLineView" OnItemCommand="Lines_ItemCommand">
                    <HeaderTemplate>
                        <div class="table-responsive">
                            <table class="table data-table mb-0">
                                <thead>
                                    <tr>
                                        <th scope="col">Product</th>
                                        <th scope="col" class="text-end">Quantity</th>
                                        <th scope="col" class="text-end">Available now</th>
                                        <th scope="col"><span class="visually-hidden">Actions</span></th>
                                    </tr>
                                </thead>
                                <tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td><span class="sku"><%#: Item.Sku %></span> <%#: Item.Name %></td>
                            <td class="text-end num fw-semibold"><%#: Display.Number(Item.Quantity) %></td>
                            <td class="text-end num">
                                <%#: Display.Number(Item.Available) %>
                                <%# Item.Shortfall > 0 ? "<span class=\"badge-pill stock-low ms-1\">short by " + Display.Number(Item.Shortfall) + "</span>" : string.Empty %>
                            </td>
                            <td class="text-end">
                                <asp:LinkButton runat="server" CommandName="Remove" CommandArgument="<%# Item.ProductId %>" CausesValidation="false" CssClass="btn btn-sm btn-light text-danger">Remove</asp:LinkButton>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                                </tbody>
                            </table>
                        </div>
                    </FooterTemplate>
                </asp:Repeater>
                <asp:PlaceHolder ID="NoLines" runat="server">
                    <div class="empty-state">
                        <%= Icons.Get("inbox", "icon icon-lg") %>
                        <p>No lines yet. Add the products the customer ordered.</p>
                    </div>
                </asp:PlaceHolder>
            </section>
        </div>

        <aside class="card card-body-padded order-summary" aria-label="Order summary">
            <h2 class="card-title">Summary</h2>
            <dl class="stat-list">
                <div><dt>Lines</dt><dd class="num"><%: Display.Number(LineCount) %></dd></div>
                <div class="stat-strong"><dt>Units</dt><dd class="num"><%: Display.Number(TotalUnits) %></dd></div>
            </dl>
            <asp:PlaceHolder ID="ShortageNote" runat="server" Visible="false">
                <p class="note note-warning">Some lines exceed the stock available today. The order can be saved, but it can only be picked after replenishment.</p>
            </asp:PlaceHolder>
            <asp:Button ID="CreateButton" runat="server" Text="Create order" CssClass="btn btn-primary w-100" ValidationGroup="order" OnClick="CreateOrder_Click" />
        </aside>
    </div>
</asp:Content>
