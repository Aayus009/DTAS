<%@ Page Title="Sign in | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Authentication.Login" %>

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
                <h3>Sign in to a clearer campus.</h3>
                <p>Track decisions, assignments, and community work in one place - with a record people can actually follow.</p>
                <ul class="auth-hero-points">
                    <li><span class="material-symbols-outlined">verified_user</span> Verified institutional access</li>
                    <li><span class="material-symbols-outlined">forum</span> Connect with your groups</li>
                    <li><span class="material-symbols-outlined">visibility</span> Public progress, private workspaces</li>
                </ul>
            </div>
            <p class="auth-hero-kicker">2026 DTAS</p>
        </aside>

        <section class="auth-panel">
            <div class="auth-card">
                <div class="auth-card-head">
                    <h1>Welcome back</h1>
                    <p>Sign in with your institutional email or Google.</p>
                </div>

                <asp:Label ID="lblMessage" runat="server" CssClass="auth-alert"></asp:Label>

                <asp:Panel ID="pnlLogin" runat="server" DefaultButton="btnLogin">
                    <div class="auth-field">
                        <label for="<%= txtUsername.ClientID %>">Email or username</label>
                        <div class="auth-input-wrap">
                            <span class="material-symbols-outlined">mail</span>
                            <asp:TextBox ID="txtUsername" runat="server" placeholder="name@institution.edu"></asp:TextBox>
                        </div>
                    </div>

                    <div class="auth-field">
                        <label for="<%= txtPassword.ClientID %>">Password</label>
                        <div class="auth-input-wrap">
                            <span class="material-symbols-outlined">lock</span>
                            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" placeholder="Enter your password"></asp:TextBox>
                            <button type="button" class="auth-toggle-pass" data-toggle-password="<%= txtPassword.ClientID %>" title="Show password">
                                <span class="material-symbols-outlined">visibility</span>
                            </button>
                        </div>
                        <a class="auth-forgot" href="ForgotPassword.aspx">Forgot password?</a>
                    </div>

                    <asp:Button ID="btnLogin" runat="server" Text="Sign in" CssClass="auth-primary" OnClick="btnLogin_Click" />

                    <div class="auth-divider">or</div>

                    <asp:LinkButton ID="btnGoogleLogin" runat="server" CssClass="google-btn" CausesValidation="false" OnClick="btnGoogleLogin_Click">
                        <svg viewBox="0 0 48 48" aria-hidden="true">
                            <path fill="#EA4335" d="M24 9.5c3.54 0 6.71 1.22 9.21 3.6l6.85-6.85C35.9 2.38 30.47 0 24 0 14.62 0 6.51 5.38 2.56 13.22l7.98 6.19C12.43 13.72 17.74 9.5 24 9.5z"/>
                            <path fill="#4285F4" d="M46.98 24.55c0-1.57-.15-3.09-.38-4.55H24v9.02h12.94c-.58 2.96-2.26 5.48-4.78 7.18l7.73 6c4.51-4.18 7.09-10.36 7.09-17.65z"/>
                            <path fill="#FBBC05" d="M10.53 28.59c-.48-1.45-.76-2.99-.76-4.59s.27-3.14.76-4.59l-7.98-6.19C.92 16.46 0 20.12 0 24c0 3.88.92 7.54 2.56 10.78l7.97-6.19z"/>
                            <path fill="#34A853" d="M24 48c6.48 0 11.93-2.13 15.89-5.81l-7.73-6c-2.15 1.45-4.92 2.3-8.16 2.3-6.26 0-11.57-4.22-13.47-9.91l-7.98 6.19C6.51 42.62 14.62 48 24 48z"/>
                        </svg>
                        <span>Sign in with Google</span>
                    </asp:LinkButton>
                </asp:Panel>

                <asp:Panel ID="pnlOTP" runat="server" Visible="false" CssClass="auth-otp">
                    <div class="auth-card-head">
                        <h1>Check your email</h1>
                        <asp:Label ID="lblOtpSentTo" runat="server"></asp:Label>
                    </div>
                    <div class="auth-field">
                        <label for="<%= txtOTP.ClientID %>">6-digit code</label>
                        <asp:TextBox ID="txtOTP" runat="server" MaxLength="6" placeholder="000000" CssClass="auth-plain"></asp:TextBox>
                    </div>
                    <asp:Button ID="btnVerifyOTP" runat="server" Text="Verify and sign in" CssClass="auth-primary" OnClick="btnVerifyOTP_Click" />
                    <asp:Button ID="btnResendOTP" runat="server" Text="Resend code" CssClass="auth-ghost" OnClick="btnResendOTP_Click" />
                </asp:Panel>

                <div class="auth-foot">
                    New to DTAS?
                    <a href="/Modules/Authentication/Register.aspx">Create an account</a>
                </div>
            </div>
        </section>
    </div>
</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="<%= ResolveUrl("~/Assets/js/login.js") %>"></script>
</asp:Content>
