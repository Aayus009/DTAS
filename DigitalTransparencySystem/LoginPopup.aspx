<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LoginPopup.aspx.cs" Inherits="DigitalTransparencySystem.LoginPopup" EnableViewState="true" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Sign in | DTAS</title>
    <script>if (/[?&]embed=1/.test(location.search)) document.documentElement.className += " embed-mode";</script>
    <link href="https://fonts.googleapis.com/css2?family=Playfair+Display:ital,wght@1,700;1,800&family=Hanken+Grotesk:wght@400;500;600;700;800&display=swap" rel="stylesheet" />
    <link href="/Assets/css/auth.css?v=15" rel="stylesheet" />
    <link href="/Assets/css/auth-embed.css?v=2" rel="stylesheet" />
    <style>
        html, body { margin: 0; background: transparent; font-family: "Hanken Grotesk", "Segoe UI", sans-serif; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="auth-wrap">
            <div class="auth-card">
                <button type="button" class="auth-card-close" onclick="closeEmbed()" aria-label="Close">&times;</button>
                <div class="auth-card-brand">
                    <span class="dtas-wordmark">DTAS</span>
                </div>
                <div class="auth-card-head">
                    <h1>Welcome back</h1>
                    <p>Sign in with your institutional email or Google.</p>
                </div>

                <asp:Label ID="lblMessage" runat="server" CssClass="auth-alert"></asp:Label>

                <asp:Panel ID="pnlLogin" runat="server" DefaultButton="btnLogin">
                    <div class="auth-field">
                        <label for="<%= txtUsername.ClientID %>">Email or username</label>
                        <div class="auth-input-wrap">
                            <svg class="auth-ico" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 6h16v12H4z"/><path d="m4 7 8 6 8-6"/></svg>
                            <asp:TextBox ID="txtUsername" runat="server" placeholder="name@institution.edu"></asp:TextBox>
                        </div>
                    </div>
                    <div class="auth-field">
                        <label for="<%= txtPassword.ClientID %>">Password</label>
                        <div class="auth-input-wrap">
                            <svg class="auth-ico" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="5" y="10" width="14" height="10" rx="2"/><path d="M8 10V8a4 4 0 0 1 8 0v2"/></svg>
                            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" placeholder="Enter your password"></asp:TextBox>
                        </div>
                        <a class="auth-forgot" href="/Modules/Authentication/ForgotPassword.aspx" target="_top">Forgot password?</a>
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
                        <asp:TextBox ID="txtOTP" runat="server" MaxLength="6" placeholder="000000" CssClass="auth-plain"></asp:TextBox>
                    </div>
                    <asp:Button ID="btnVerifyOTP" runat="server" Text="Verify and sign in" CssClass="auth-primary" OnClick="btnVerifyOTP_Click" />
                    <asp:Button ID="btnResendOTP" runat="server" Text="Resend code" CssClass="auth-ghost" OnClick="btnResendOTP_Click" />
                </asp:Panel>

                <div class="auth-foot">
                    New to DTAS?
                    <a href="javascript:void(0)" onclick="switchToRegister()">Create an account</a>
                </div>
            </div>
        </div>
    </form>
    <script>
        function closeEmbed() {
            if (window.parent && window.parent !== window && window.parent.closeAuthModal) window.parent.closeAuthModal();
        }
        function switchToRegister() {
            if (/[?&]embed=1/.test(location.search) && window.parent && window.parent !== window) {
                window.parent.openRegister();
                return;
            }
            location.href = "/RegisterPopup.aspx" + (location.search || "");
        }
        (function () {
            if (!/[?&]embed=1/.test(location.search) || !window.parent) return;
            var timer;
            function report() {
                var card = document.querySelector(".auth-card") || document.body;
                var h = Math.ceil(card.getBoundingClientRect().height);
                window.parent.postMessage({ type: "auth-ready", kind: "login", height: h }, "*");
            }
            function schedule() {
                clearTimeout(timer);
                timer = setTimeout(report, 30);
            }
            window.addEventListener("load", report);
            window.addEventListener("message", function (e) {
                if (e.data && e.data.type === "auth-measure") report();
            });
            if (window.ResizeObserver) new ResizeObserver(schedule).observe(document.querySelector(".auth-card") || document.body);
            report();
        })();
    </script>
</body>
</html>
