<%@ Page Title="Register | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Authentication.Register" %>

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
                <h3>Join the transparency network.</h3>
                <p>Create an account to propose work, follow decisions, and keep campus records visible to the people they affect.</p>
                <ul class="auth-hero-points">
                    <li><span class="material-symbols-outlined">school</span> Built for students, faculty, and staff</li>
                    <li><span class="material-symbols-outlined">lock</span> Email verification on every new account</li>
                    <li><span class="material-symbols-outlined">history_edu</span> A public trail of institutional action</li>
                </ul>
            </div>
            <p class="auth-hero-kicker">2026 DTAS</p>
        </aside>

        <section class="auth-panel">
            <div class="auth-card auth-card-wide">
                <div class="auth-card-head">
                    <h1>Create your account</h1>
                    <p>Register with your institutional email or Google.</p>
                </div>

                <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

                <asp:Panel ID="pnlRegister" runat="server" DefaultButton="btnRegister">
                    <div class="auth-field">
                        <asp:Label ID="lblFullName" runat="server" AssociatedControlID="txtFullName" Text="Full name"></asp:Label>
                        <div class="auth-input-wrap">
                            <span class="material-symbols-outlined">badge</span>
                            <asp:TextBox ID="txtFullName" runat="server" placeholder="Your legal name"></asp:TextBox>
                        </div>
                        <asp:RequiredFieldValidator ID="rfvFullName" runat="server" ControlToValidate="txtFullName"
                            ErrorMessage="Full name is required." CssClass="text-error" Display="Dynamic"></asp:RequiredFieldValidator>
                    </div>

                    <div class="auth-field">
                        <asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail" Text="Institutional email"></asp:Label>
                        <div class="auth-input-wrap">
                            <span class="material-symbols-outlined">mail</span>
                            <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" placeholder="name@institution.edu"></asp:TextBox>
                        </div>
                        <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                            ErrorMessage="Email is required." CssClass="text-error" Display="Dynamic"></asp:RequiredFieldValidator>
                        <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
                            ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
                            ErrorMessage="Please enter a valid email address." CssClass="text-error" Display="Dynamic"></asp:RegularExpressionValidator>
                    </div>

                    <div class="auth-field">
                        <asp:Label ID="lblRole" runat="server" AssociatedControlID="ddlRole" Text="Role"></asp:Label>
                        <asp:DropDownList ID="ddlRole" runat="server" CssClass="auth-plain">
                            <asp:ListItem Text="Select your role" Value="" Selected="True"></asp:ListItem>
                            <asp:ListItem Text="Student" Value="student"></asp:ListItem>
                            <asp:ListItem Text="Teacher" Value="teacher"></asp:ListItem>
                            <asp:ListItem Text="Staff" Value="staff"></asp:ListItem>
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="rfvRole" runat="server" ControlToValidate="ddlRole"
                            ErrorMessage="Please select a role." CssClass="text-error" Display="Dynamic" InitialValue=""></asp:RequiredFieldValidator>
                    </div>

                    <div class="auth-grid-2">
                        <div class="auth-field">
                            <asp:Label ID="lblPassword" runat="server" AssociatedControlID="txtPassword" Text="Password"></asp:Label>
                            <div class="auth-input-wrap">
                                <span class="material-symbols-outlined">lock</span>
                                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" placeholder="8+ characters"></asp:TextBox>
                            </div>
                            <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword"
                                ErrorMessage="Password is required." CssClass="text-error" Display="Dynamic"></asp:RequiredFieldValidator>
                        </div>
                        <div class="auth-field">
                            <asp:Label ID="lblConfirmPassword" runat="server" AssociatedControlID="txtConfirmPassword" Text="Confirm password"></asp:Label>
                            <div class="auth-input-wrap">
                                <span class="material-symbols-outlined">lock</span>
                                <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" placeholder="Repeat password"></asp:TextBox>
                            </div>
                            <asp:RequiredFieldValidator ID="rfvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword"
                                ErrorMessage="Please confirm your password." CssClass="text-error" Display="Dynamic"></asp:RequiredFieldValidator>
                            <asp:CompareValidator ID="cvPassword" runat="server" ControlToValidate="txtConfirmPassword"
                                ControlToCompare="txtPassword" ErrorMessage="Passwords do not match." CssClass="text-error" Display="Dynamic"></asp:CompareValidator>
                        </div>
                    </div>

                    <div class="auth-check">
                        <asp:CheckBox ID="chkCharter" runat="server" />
                        <asp:Label ID="lblCharter" runat="server" AssociatedControlID="chkCharter">
                            I agree to the <a href="#">Data Ethics and Transparency Charter</a>.
                        </asp:Label>
                    </div>
                    <asp:CustomValidator ID="cvCharter" runat="server"
                        ErrorMessage="You must agree to the Data Ethics and Transparency Charter."
                        CssClass="text-error" Display="Dynamic"
                        ClientValidationFunction="validateCharter"
                        OnServerValidate="cvCharter_ServerValidate"></asp:CustomValidator>

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

                    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="auth-alert" Style="display:block;margin-top:0.9rem;">
                        <asp:Label ID="lblMessage" runat="server"></asp:Label>
                    </asp:Panel>
                </asp:Panel>

                <asp:Panel ID="pnlOTP" runat="server" Visible="false" CssClass="auth-otp">
                    <div class="auth-card-head">
                        <h1>Verify your email</h1>
                        <asp:Label ID="lblOtpSentTo" runat="server"></asp:Label>
                    </div>
                    <div class="auth-field">
                        <asp:TextBox ID="txtOTP" runat="server" MaxLength="6" placeholder="000000" CssClass="auth-plain"></asp:TextBox>
                    </div>
                    <asp:Button ID="btnVerifyOTP" runat="server" Text="Verify and continue" CssClass="auth-primary" OnClick="btnVerifyOTP_Click" CausesValidation="false" />
                    <asp:Button ID="btnResendOTP" runat="server" Text="Resend code" CssClass="auth-ghost" OnClick="btnResendOTP_Click" CausesValidation="false" />
                    <asp:Panel ID="pnlOtpMessage" runat="server" Visible="false">
                        <asp:Label ID="lblOtpMessage" runat="server"></asp:Label>
                    </asp:Panel>
                </asp:Panel>

                <div class="auth-foot">
                    Already have an account?
                    <a href="/Modules/Authentication/Login.aspx">Sign in</a>
                </div>
            </div>
        </section>
    </div>
    <script src="/Assets/js/register.js"></script>
</asp:Content>
