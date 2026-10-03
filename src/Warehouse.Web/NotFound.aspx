<%@ Page Title="Page not found" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="NotFound.aspx.cs" Inherits="Warehouse.Web.NotFoundPage" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="status-page">
        <p class="status-code">404</p>
        <h1 class="page-title">We could not find that page</h1>
        <p class="page-subtitle">The record may have been deleted, or the link is out of date.</p>
        <a class="btn btn-primary" href="<%: ResolveUrl("~/") %>">Back to the dashboard</a>
    </div>
</asp:Content>
