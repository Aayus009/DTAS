<%@ Page Title="Finish Google sign-in | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GoogleComplete.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Authentication.GoogleComplete" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/auth.css") %>?v=9" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="auth-shell">
        <aside class="auth-hero">
            <div class="auth-hero-brand">
                <span class="dtas-wordmark dtas-wordmark-light">DTAS</span>
            </div>
            <div class="auth-hero-copy">
                <h3>One last step.</h3>
                <p>Your Google account is verified. Choose the role that matches how you use the campus portal.</p>
            </div>
        </aside>
        <section class="auth-panel">
            <div class="auth-card">
                <div class="auth-card-head">
                    <h1>Finish sign-in</h1>
                    <p>Signed in as <asp:Literal ID="litEmail" runat="server"></asp:Literal></p>
                </div>
                <asp:Label ID="lblMessage" runat="server" CssClass="auth-alert"></asp:Label>
                <div class="auth-field">
                    <label>Full name</label>
                    <asp:TextBox ID="txtFullName" runat="server" CssClass="auth-plain"></asp:TextBox>
                </div>
                <div class="auth-field">
                    <label>Role</label>
                    <asp:DropDownList ID="ddlRole" runat="server" CssClass="auth-plain">
                        <asp:ListItem Text="Select your role" Value="" Selected="True"></asp:ListItem>
                        <asp:ListItem Text="Student" Value="3"></asp:ListItem>
                        <asp:ListItem Text="Teacher" Value="2"></asp:ListItem>
                        <asp:ListItem Text="Staff" Value="4"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <asp:Button ID="btnFinish" runat="server" Text="Continue" CssClass="auth-primary" OnClick="btnFinish_Click" />
            </div>
        </section>
    </div>
</asp:Content>
