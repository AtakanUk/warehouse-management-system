<%@ Page Title="Sign in" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Warehouse.Web.Account.Login" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="auth-wrap">
        <div class="auth-card">
            <div class="auth-brand"><%= Icons.Get("warehouse", "brand-icon") %> Warehouse<span class="brand-accent">WMS</span></div>
            <h1 class="auth-title">Sign in</h1>
            <p class="auth-subtitle">Inventory, picking and shipping in one workspace.</p>

            <asp:Panel runat="server" DefaultButton="SignInButton">
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="UserNameBox" CssClass="form-label">User name</asp:Label>
                    <asp:TextBox ID="UserNameBox" runat="server" CssClass="form-control" MaxLength="50" autocomplete="username" autofocus="autofocus" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="UserNameBox" ErrorMessage="Enter your user name." Display="Dynamic" CssClass="field-error" SetFocusOnError="true" />
                </div>
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="PasswordBox" CssClass="form-label">Password</asp:Label>
                    <asp:TextBox ID="PasswordBox" runat="server" CssClass="form-control" TextMode="Password" MaxLength="128" autocomplete="current-password" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="PasswordBox" ErrorMessage="Enter your password." Display="Dynamic" CssClass="field-error" SetFocusOnError="true" />
                </div>
                <div class="form-check mb-4">
                    <asp:CheckBox ID="RememberMe" runat="server" />
                    <asp:Label runat="server" AssociatedControlID="RememberMe" CssClass="form-check-label">Keep me signed in on this device</asp:Label>
                </div>
                <asp:Button ID="SignInButton" runat="server" Text="Sign in" CssClass="btn btn-primary w-100" OnClick="SignIn_Click" />
            </asp:Panel>

            <p class="auth-footer">New here? <a href="<%: ResolveUrl("~/account/register") %>">Create an account</a></p>

            <asp:PlaceHolder ID="DemoCredentials" runat="server">
                <div class="demo-hint">
                    <strong>Demo account</strong>
                    <span>User <code>demo</code> · Password <code>Demo@12345</code></span>
                </div>
            </asp:PlaceHolder>
        </div>
    </div>
</asp:Content>
