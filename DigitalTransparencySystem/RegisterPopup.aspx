<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RegisterPopup.aspx.cs" Inherits="DigitalTransparencySystem.RegisterPopup" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Register | DTAS</title>
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
                    <h1>Create your account</h1>
                    <p>Register with your institutional email or Google.</p>
                </div>

                <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="auth-alert is-on">
                    <asp:Label ID="lblMessage" runat="server" />
                </asp:Panel>

                <asp:Panel ID="pnlRegister" runat="server" DefaultButton="btnRegister">
                    <div class="auth-grid-2">
                        <div class="auth-field">
                            <label>Full name</label>
                            <asp:TextBox ID="txtFullName" runat="server" CssClass="auth-plain" placeholder="Your legal name"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvFullName" runat="server" ControlToValidate="txtFullName"
                                ErrorMessage="Full name is required." CssClass="text-error" Display="Dynamic" />
                        </div>
                        <div class="auth-field">
                            <label>Institutional email</label>
                            <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" CssClass="auth-plain" placeholder="name@institution.edu"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                                ErrorMessage="Email is required." CssClass="text-error" Display="Dynamic" />
                            <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
                                ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
                                ErrorMessage="Enter a valid email." CssClass="text-error" Display="Dynamic" />
                        </div>
                    </div>
                    <div class="auth-field">
                        <label>Role</label>
                        <asp:DropDownList ID="ddlRole" runat="server" CssClass="auth-plain">
                            <asp:ListItem Text="Select your role" Value="" Selected="True"></asp:ListItem>
                            <asp:ListItem Text="Student" Value="student"></asp:ListItem>
                            <asp:ListItem Text="Teacher" Value="teacher"></asp:ListItem>
                            <asp:ListItem Text="Staff" Value="staff"></asp:ListItem>
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="rfvRole" runat="server" ControlToValidate="ddlRole"
                            ErrorMessage="Please select a role." CssClass="text-error" Display="Dynamic" InitialValue="" />
                    </div>
                    <div class="auth-grid-2">
                        <div class="auth-field">
                            <label>Password</label>
                            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="auth-plain"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword"
                                ErrorMessage="Required." CssClass="text-error" Display="Dynamic" />
                        </div>
                        <div class="auth-field">
                            <label>Confirm</label>
                            <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" CssClass="auth-plain"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword"
                                ErrorMessage="Required." CssClass="text-error" Display="Dynamic" />
                            <asp:CompareValidator ID="cvPassword" runat="server" ControlToValidate="txtConfirmPassword"
                                ControlToCompare="txtPassword" ErrorMessage="No match." CssClass="text-error" Display="Dynamic" />
                        </div>
                    </div>
                    <div class="auth-check">
                        <asp:CheckBox ID="chkCharter" runat="server" />
                        <label>I agree to the <a href="#">Data Ethics and Transparency Charter</a>.</label>
                    </div>
                    <asp:CustomValidator ID="cvCharter" runat="server"
                        ErrorMessage="You must agree to the Charter."
                        CssClass="text-error" Display="Dynamic"
                        OnServerValidate="cvCharter_ServerValidate" />
                    <asp:Button ID="btnRegister" runat="server" Text="Create account" CssClass="auth-primary" OnClick="btnRegister_Click" />
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
                        <h1>Verify your email</h1>
                        <asp:Label ID="lblOtpSentTo" runat="server"></asp:Label>
                    </div>
                    <asp:TextBox ID="txtOTP" runat="server" MaxLength="6" placeholder="000000" CssClass="auth-plain"></asp:TextBox>
                    <asp:Button ID="btnVerifyOTP" runat="server" Text="Verify and continue" CssClass="auth-primary" OnClick="btnVerifyOTP_Click" CausesValidation="false" />
                    <asp:Button ID="btnResendOTP" runat="server" Text="Resend code" CssClass="auth-ghost" OnClick="btnResendOTP_Click" CausesValidation="false" />
                </asp:Panel>

                <div class="auth-foot">
                    Already have an account?
                    <a href="javascript:void(0)" onclick="switchToLogin()">Sign in</a>
                </div>
            </div>
        </div>
    </form>
    <script>
        function closeEmbed() {
            if (window.parent && window.parent !== window && window.parent.closeAuthModal) window.parent.closeAuthModal();
        }
        function switchToLogin() {
            if (/[?&]embed=1/.test(location.search) && window.parent && window.parent !== window) {
                window.parent.openLogin();
                return;
            }
            location.href = "/LoginPopup.aspx" + (location.search || "");
        }
        (function () {
            if (!/[?&]embed=1/.test(location.search) || !window.parent) return;
            var timer;
            function report() {
                var card = document.querySelector(".auth-card") || document.body;
                var h = Math.ceil(card.getBoundingClientRect().height);
                window.parent.postMessage({ type: "auth-ready", kind: "register", height: h }, "*");
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
