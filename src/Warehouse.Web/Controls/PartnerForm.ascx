<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PartnerForm.ascx.cs" Inherits="Warehouse.Web.Controls.PartnerForm" %>

<div class="row g-3">
    <div class="col-sm-7">
        <asp:Label runat="server" AssociatedControlID="NameBox" CssClass="form-label">Company name</asp:Label>
        <asp:TextBox ID="NameBox" runat="server" CssClass="form-control" MaxLength="100" />
        <asp:RequiredFieldValidator runat="server" ControlToValidate="NameBox" ErrorMessage="Enter the company name." Display="Dynamic" CssClass="field-error" SetFocusOnError="true" />
    </div>
    <div class="col-sm-5">
        <asp:Label runat="server" AssociatedControlID="ContactBox" CssClass="form-label">Contact person <span class="optional">optional</span></asp:Label>
        <asp:TextBox ID="ContactBox" runat="server" CssClass="form-control" MaxLength="100" />
    </div>
    <div class="col-sm-7">
        <asp:Label runat="server" AssociatedControlID="EmailBox" CssClass="form-label">Email <span class="optional">optional</span></asp:Label>
        <asp:TextBox ID="EmailBox" runat="server" CssClass="form-control" TextMode="Email" MaxLength="254" />
        <asp:RegularExpressionValidator runat="server" ControlToValidate="EmailBox" ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" ErrorMessage="Enter a valid email address." Display="Dynamic" CssClass="field-error" />
    </div>
    <div class="col-sm-5">
        <asp:Label runat="server" AssociatedControlID="PhoneBox" CssClass="form-label">Phone <span class="optional">optional</span></asp:Label>
        <asp:TextBox ID="PhoneBox" runat="server" CssClass="form-control" TextMode="Phone" MaxLength="32" />
    </div>
    <div class="col-12">
        <asp:Label runat="server" AssociatedControlID="AddressBox" CssClass="form-label">Address <span class="optional">optional</span></asp:Label>
        <asp:TextBox ID="AddressBox" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" MaxLength="250" />
    </div>
</div>
