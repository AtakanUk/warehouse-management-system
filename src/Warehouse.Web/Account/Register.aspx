<%@ Page Title="Create account" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="Warehouse.Web.Account.Register" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="auth-wrap">
        <div class="auth-card">
            <div class="auth-brand"><%= Icons.Get("warehouse", "brand-icon") %> Warehouse<span class="brand-accent">WMS</span></div>
            <h1 class="auth-title">Create an account</h1>
            <p class="auth-subtitle">Accounts give operators access to every warehouse screen.</p>

            <asp:Panel runat="server" DefaultButton="RegisterButton">
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="UserNameBox" CssClass="form-label">User name</asp:Label>
                    <asp:TextBox ID="UserNameBox" runat="server" CssClass="form-control" MaxLength="50" autocomplete="username" autofocus="autofocus" aria-describedby="userNameHelp" />
                    <div id="userNameHelp" class="form-text">3–50 characters: letters, digits, '.', '_' or '-'.</div>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="UserNameBox" ErrorMessage="Choose a user name." Display="Dynamic" CssClass="field-error" SetFocusOnError="true" />
                    <asp:RegularExpressionValidator runat="server" ControlToValidate="UserNameBox" ValidationExpression="^[A-Za-z0-9._\-]{3,50}$" ErrorMessage="Use 3–50 letters, digits, '.', '_' or '-'." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="PasswordBox" CssClass="form-label">Password</asp:Label>
                    <asp:TextBox ID="PasswordBox" runat="server" CssClass="form-control" TextMode="Password" MaxLength="128" autocomplete="new-password" aria-describedby="passwordHelp" />
                    <div id="passwordHelp" class="form-text">At least 8 characters.</div>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="PasswordBox" ErrorMessage="Choose a password." Display="Dynamic" CssClass="field-error" />
                    <asp:RegularExpressionValidator runat="server" ControlToValidate="PasswordBox" ValidationExpression="^[\s\S]{8,128}$" ErrorMessage="Use at least 8 characters." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="mb-4">
                    <asp:Label runat="server" AssociatedControlID="ConfirmPasswordBox" CssClass="form-label">Confirm password</asp:Label>
                    <asp:TextBox ID="ConfirmPasswordBox" runat="server" CssClass="form-control" TextMode="Password" MaxLength="128" autocomplete="new-password" />
                    <asp:CompareValidator runat="server" ControlToValidate="ConfirmPasswordBox" ControlToCompare="PasswordBox" ErrorMessage="The passwords do not match." Display="Dynamic" CssClass="field-error" />
                </div>
                <asp:Button ID="RegisterButton" runat="server" Text="Create account" CssClass="btn btn-primary w-100" OnClick="Register_Click" />
            </asp:Panel>

            <p class="auth-footer">Already have an account? <a href="<%: ResolveUrl("~/account/login") %>">Sign in</a></p>
        </div>
    </div>
</asp:Content>
