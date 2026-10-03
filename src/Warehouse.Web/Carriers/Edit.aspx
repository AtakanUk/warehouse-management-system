<%@ Page Title="Carrier" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Edit.aspx.cs" Inherits="Warehouse.Web.Carriers.CarrierEdit" %>
<%@ Register Src="~/Controls/PartnerForm.ascx" TagPrefix="wms" TagName="PartnerForm" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <nav class="breadcrumb-nav" aria-label="Breadcrumb">
        <a href="<%: ResolveUrl("~/carriers") %>">Carriers</a> <span aria-hidden="true">/</span> <span><%: Title %></span>
    </nav>
    <div class="page-header">
        <div>
            <h1 class="page-title"><%: Title %></h1>
            <p class="page-subtitle">Carrier details used when orders are shipped.</p>
        </div>
    </div>

    <section class="card card-body-padded form-narrow">
        <wms:PartnerForm ID="ContactForm" runat="server" />
        <div class="form-actions">
            <asp:Button ID="SaveButton" runat="server" Text="Save carrier" CssClass="btn btn-primary" OnClick="Save_Click" />
            <a class="btn btn-light" href="<%: ResolveUrl("~/carriers") %>">Cancel</a>
        </div>
    </section>
</asp:Content>
