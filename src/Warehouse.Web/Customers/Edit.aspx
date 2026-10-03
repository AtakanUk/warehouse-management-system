<%@ Page Title="Customer" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Edit.aspx.cs" Inherits="Warehouse.Web.Customers.CustomerEdit" %>
<%@ Register Src="~/Controls/PartnerForm.ascx" TagPrefix="wms" TagName="PartnerForm" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <nav class="breadcrumb-nav" aria-label="Breadcrumb">
        <a href="<%: ResolveUrl("~/customers") %>">Customers</a> <span aria-hidden="true">/</span> <span><%: Title %></span>
    </nav>
    <div class="page-header">
        <div>
            <h1 class="page-title"><%: Title %></h1>
            <p class="page-subtitle">Customer master data used on outbound orders.</p>
        </div>
    </div>

    <section class="card card-body-padded form-narrow">
        <wms:PartnerForm ID="ContactForm" runat="server" />
        <div class="form-actions">
            <asp:Button ID="SaveButton" runat="server" Text="Save customer" CssClass="btn btn-primary" OnClick="Save_Click" />
            <a class="btn btn-light" href="<%: ResolveUrl("~/customers") %>">Cancel</a>
        </div>
    </section>
</asp:Content>
