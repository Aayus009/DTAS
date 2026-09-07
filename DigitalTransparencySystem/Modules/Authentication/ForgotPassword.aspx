<%@ Page Title="Forgot Password | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Authentication.ForgotPassword" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="/Assets/css/login.css" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="accent-blob-top"></div>
    <div class="accent-blob-bottom"></div>

    <main class="flex-grow flex items-center justify-center px-container-padding-mobile py-12 relative z-10">
        <div class="w-full max-w-[480px]">

            <!-- Logo Section -->
            <div class="flex flex-col items-center mb-8 animate-in fade-in slide-in-from-bottom-4 duration-700">
                <div class="w-12 h-12 bg-primary rounded-lg flex items-center justify-center mb-4 shadow-lg">
                    <span class="text-on-primary font-bold text-xl">DT</span>
                </div>
                <h1 class="font-headline-lg text-headline-lg text-primary tracking-tight">DTAS</h1>
                <p class="font-label-md text-label-md text-secondary mt-1 tracking-widest uppercase">DIGITAL TRUTH SOURCE</p>
            </div>

            <!-- Reset Card -->
            <div class="glass-card rounded-xl p-8 md:p-10 animate-in fade-in zoom-in-95 duration-1000">
                <div class="text-center mb-8">
                    <div class="w-16 h-16 bg-primary-container/20 rounded-full flex items-center justify-center mx-auto mb-4">
                        <span class="material-symbols-outlined text-primary text-3xl">password</span>
                    </div>
                    <h2 class="font-headline-md text-headline-md text-on-surface mb-2">Reset Your Password</h2>
                    <p class="font-body-md text-body-md text-on-surface-variant">Enter your registered email. We will send a one-time code so you can choose a new password.</p>
                </div>

                <!-- Error/Success Message -->
                <asp:Label ID="lblMessage" runat="server" CssClass="mb-4 p-3 bg-error-container text-error rounded-lg text-center font-body-md" />

                <!-- Request Panel -->
                <asp:Panel ID="pnlRequest" runat="server" CssClass="space-y-6">
                    <div>
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2" for="<%= txtEmail.ClientID %>">Email Address</label>
                        <div class="relative">
                            <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                                <span class="material-symbols-outlined text-outline text-[20px]">mail</span>
                            </div>
                            <asp:TextBox ID="txtEmail" runat="server" TextMode="Email"
                                CssClass="w-full bg-surface-container-low border border-outline-variant rounded-xl py-3 pl-12 pr-4 focus:ring-2 focus:ring-primary focus:border-primary transition-all font-body-md text-body-md placeholder:text-outline"
                                placeholder="you@institution.edu" />
                        </div>
                        <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                            CssClass="text-error text-[12px] mt-1 inline-block" Display="Dynamic"
                            ErrorMessage="Please enter your email address." ValidationGroup="Reset" />
                        <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
                            CssClass="text-error text-[12px] mt-1 inline-block" Display="Dynamic" ValidationGroup="Reset"
                            ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
                            ErrorMessage="Please enter a valid email address." />
                    </div>

                    <asp:Button ID="btnSendReset" runat="server" Text="Send verification code" OnClick="btnSendReset_Click" CausesValidation="true" ValidationGroup="Reset"
                        CssClass="w-full bg-primary text-on-primary font-title-lg text-title-lg py-4 rounded-full shadow-md hover:bg-primary-container hover:shadow-lg transition-all duration-300 btn-hover-effect flex items-center justify-center gap-2 cursor-pointer" />
                </asp:Panel>

                <asp:Panel ID="pnlOTP" runat="server" Visible="false" CssClass="space-y-6">
                    <asp:Label ID="lblOtpSentTo" runat="server" CssClass="font-body-md text-body-md text-on-surface-variant block text-center" />
                    <div>
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Verification code</label>
                        <asp:TextBox ID="txtOTP" runat="server" MaxLength="6"
                            CssClass="w-full text-center text-[32px] tracking-[12px] bg-surface-container-low border border-outline-variant rounded-xl py-3"
                            placeholder="000000" />
                    </div>
                    <div>
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2">New password</label>
                        <asp:TextBox ID="txtNewPassword" runat="server" TextMode="Password"
                            CssClass="w-full bg-surface-container-low border border-outline-variant rounded-xl py-3 px-4" />
                    </div>
                    <div>
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Confirm password</label>
                        <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password"
                            CssClass="w-full bg-surface-container-low border border-outline-variant rounded-xl py-3 px-4" />
                    </div>
                    <asp:Button ID="btnVerifyReset" runat="server" Text="Reset password" OnClick="btnVerifyReset_Click" CausesValidation="false"
                        CssClass="w-full bg-primary text-on-primary font-title-lg text-title-lg py-4 rounded-full shadow-md cursor-pointer" />
                    <div class="text-center">
                        <asp:Button ID="btnResendOTP" runat="server" Text="Resend code" OnClick="btnResendOTP_Click" CausesValidation="false"
                            CssClass="text-primary font-label-md bg-transparent border-none cursor-pointer" />
                    </div>
                </asp:Panel>

                <!-- Success Panel -->
                <asp:Panel ID="pnlSuccess" runat="server" Visible="false" CssClass="space-y-6 text-center">
                    <div class="w-16 h-16 bg-tertiary-container/20 rounded-full flex items-center justify-center mx-auto">
                        <span class="material-symbols-outlined text-tertiary text-3xl">mark_email_read</span>
                    </div>
                    <div>
                        <h3 class="font-headline-md text-headline-md text-on-surface mb-2">Password updated</h3>
                        <p class="font-body-md text-body-md text-on-surface-variant leading-relaxed">You can now sign in with your new password.</p>
                    </div>
                    <a class="text-primary font-bold hover:underline" href="/Modules/Authentication/Login.aspx">Back to sign in</a>
                    <asp:Label ID="lblDevLink" runat="server" CssClass="hidden block text-[12px] text-secondary break-all bg-surface-container-low rounded-lg p-3" />
                </asp:Panel>

                <!-- Card Footer -->
                <div class="mt-10 pt-6 border-t border-surface-container-high text-center">
                    <p class="font-body-md text-body-md text-on-surface-variant">
                        Remembered it after all?
                        <a class="text-primary font-bold hover:underline decoration-primary/30 underline-offset-4 ml-1" href="/Modules/Authentication/Login.aspx">Back to Login</a>
                    </p>
                </div>
            </div>

            <p class="mt-6 text-center font-label-md text-label-md text-secondary/40">
                © 2026 DTAS. All rights reserved.
            </p>
        </div>
    </main>
</asp:Content>