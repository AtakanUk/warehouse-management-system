<%@ Page Title="Product" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Edit.aspx.cs" Inherits="Warehouse.Web.Products.ProductEdit" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <nav class="breadcrumb-nav" aria-label="Breadcrumb">
        <a href="<%: ResolveUrl("~/products") %>">Products</a> <span aria-hidden="true">/</span> <span><%: Title %></span>
    </nav>
    <div class="page-header">
        <div>
            <h1 class="page-title"><%: Title %></h1>
            <p class="page-subtitle"><%: IsNew ? "Add a stock-keeping unit to the catalog. Stock is booked through goods receipts." : "Catalog data of this stock-keeping unit." %></p>
        </div>
    </div>

    <div class="form-layout">
        <section class="card card-body-padded">
            <div class="row g-3">
                <div class="col-sm-5">
                    <asp:Label runat="server" AssociatedControlID="SkuBox" CssClass="form-label">SKU</asp:Label>
                    <asp:TextBox ID="SkuBox" runat="server" CssClass="form-control text-uppercase" MaxLength="32" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="SkuBox" ErrorMessage="Enter a SKU." Display="Dynamic" CssClass="field-error" SetFocusOnError="true" />
                    <asp:RegularExpressionValidator runat="server" ControlToValidate="SkuBox" ValidationExpression="^\s*[A-Za-z0-9][A-Za-z0-9\-_.]*\s*$" ErrorMessage="Letters, digits, '-', '_' and '.' only." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-sm-7">
                    <asp:Label runat="server" AssociatedControlID="NameBox" CssClass="form-label">Name</asp:Label>
                    <asp:TextBox ID="NameBox" runat="server" CssClass="form-control" MaxLength="100" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="NameBox" ErrorMessage="Enter a name." Display="Dynamic" CssClass="field-error" SetFocusOnError="true" />
                </div>
                <div class="col-12">
                    <asp:Label runat="server" AssociatedControlID="DescriptionBox" CssClass="form-label">Description <span class="optional">optional</span></asp:Label>
                    <asp:TextBox ID="DescriptionBox" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" MaxLength="500" />
                </div>
                <div class="col-sm-4">
                    <asp:Label runat="server" AssociatedControlID="WeightBox" CssClass="form-label">Weight (kg) <span class="optional">optional</span></asp:Label>
                    <asp:TextBox ID="WeightBox" runat="server" CssClass="form-control" inputmode="decimal" MaxLength="12" />
                    <asp:RangeValidator runat="server" ControlToValidate="WeightBox" Type="Double" MinimumValue="0" MaximumValue="99999" ErrorMessage="Enter a weight between 0 and 99,999 kg." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-sm-4">
                    <asp:Label runat="server" AssociatedControlID="DimensionsBox" CssClass="form-label">Dimensions <span class="optional">optional</span></asp:Label>
                    <asp:TextBox ID="DimensionsBox" runat="server" CssClass="form-control" MaxLength="50" placeholder="40 × 30 × 30 cm" />
                </div>
                <div class="col-sm-4">
                    <asp:Label runat="server" AssociatedControlID="CountryBox" CssClass="form-label">Country of origin <span class="optional">optional</span></asp:Label>
                    <asp:TextBox ID="CountryBox" runat="server" CssClass="form-control text-uppercase" MaxLength="2" placeholder="DE" />
                    <asp:RegularExpressionValidator runat="server" ControlToValidate="CountryBox" ValidationExpression="^[A-Za-z]{2}$" ErrorMessage="Use a two-letter ISO code." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-sm-4">
                    <asp:Label runat="server" AssociatedControlID="ReorderLevelBox" CssClass="form-label">Reorder level</asp:Label>
                    <asp:TextBox ID="ReorderLevelBox" runat="server" CssClass="form-control" TextMode="Number" min="0" Text="0" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="ReorderLevelBox" ErrorMessage="Enter a reorder level." Display="Dynamic" CssClass="field-error" />
                    <asp:RangeValidator runat="server" ControlToValidate="ReorderLevelBox" Type="Integer" MinimumValue="0" MaximumValue="1000000" ErrorMessage="Enter a whole number of 0 or more." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-sm-8 d-flex align-items-end">
                    <p class="form-text mb-2">The product is flagged as <em>low stock</em> once its available quantity drops to this level.</p>
                </div>
            </div>

            <div class="form-actions">
                <asp:Button ID="SaveButton" runat="server" Text="Save product" CssClass="btn btn-primary" OnClick="Save_Click" />
                <a class="btn btn-light" href="<%: ResolveUrl("~/products") %>">Cancel</a>
            </div>
        </section>

        <asp:PlaceHolder ID="StockPanel" runat="server" Visible="false">
            <aside class="card card-body-padded stock-summary" aria-label="Stock position">
                <h2 class="card-title">Stock position</h2>
                <dl class="stat-list">
                    <div><dt>On hand</dt><dd class="num"><%: Display.Number(OnHand) %></dd></div>
                    <div><dt>Reserved for picked orders</dt><dd class="num"><%: Display.Number(Reserved) %></dd></div>
                    <div class="stat-strong"><dt>Available</dt><dd class="num"><%: Display.Number(OnHand - Reserved) %></dd></div>
                </dl>
                <a class="btn btn-outline-secondary w-100" href="<%: ResolveUrl("~/inventory/receive?product=" + RouteId) %>"><%= Icons.Get("receive") %> Receive stock</a>
            </aside>
        </asp:PlaceHolder>
    </div>
</asp:Content>
