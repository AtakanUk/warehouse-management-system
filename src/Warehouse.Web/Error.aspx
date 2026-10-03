<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="Warehouse.Web.ErrorPage" EnableViewState="false" EnableSessionState="false" %>
<%-- Deliberately standalone (no master page, no database access) so it renders even when those fail. --%>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Something went wrong · Warehouse WMS</title>
    <link rel="icon" type="image/svg+xml" href="<%: ResolveUrl("~/favicon.svg") %>" />
    <link rel="stylesheet" href="<%: ResolveUrl("~/Content/lib/bootstrap/bootstrap.min.css") %>" />
    <link rel="stylesheet" href="<%: ResolveUrl("~/Content/site.css") %>" />
</head>
<body class="auth-body">
    <main class="status-page">
        <p class="status-code">500</p>
        <h1 class="page-title">Something went wrong</h1>
        <p class="page-subtitle">The error has been logged. Please try again in a moment.</p>
        <a class="btn btn-primary" href="<%: ResolveUrl("~/") %>">Back to the dashboard</a>
    </main>
</body>
</html>
