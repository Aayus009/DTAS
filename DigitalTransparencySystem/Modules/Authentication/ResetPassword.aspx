<%@ Page Title="Reset Password | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Authentication.ResetPassword" %>

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

                <!-- Error Message Banner -->
                <asp:Label ID="lblMessage" runat="server" CssClass="mb-4 p-3 bg-error-container text-error rounded-lg text-center font-body-md" />

                <!-- Valid Token: New Password Form -->
                <asp:Panel ID="pnlValid" runat="server">
                    <div class="text-center mb-8">
                        <div class="w-16 h-16 bg-primary-container/20 rounded-full flex items-center justify-center mx-auto mb-4">
                            <span class="material-symbols-outlined text-primary text-3xl">lock_reset</span>
                        </div>
                        <h2 class="font-headline-md text-headline-md text-on-surface mb-2">Choose a New Password</h2>
                        <p class="font-body-md text-body-md text-on-surface-variant">Your reset link is valid. Create a strong password you do not use anywhere else.</p>
                    </div>

                    <div class="space-y-6">
                        <div>
                            <label class="block font-label-md text-label-md text-on-surface-variant mb-2" for="<%= txtNewPassword.ClientID %>">New Password</label>
                            <div class="relative">
                                <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                                    <span class="material-symbols-outlined text-outline text-[20px]">lock</span>
                                </div>
                                <asp:TextBox ID="txtNewPassword" runat="server" TextMode="Password"
                                    CssClass="w-full bg-surface-container-low border border-outline-variant rounded-xl py-3 pl-12 pr-4 focus:ring-2 focus:ring-primary focus:border-primary transition-all font-body-md text-body-md placeholder:text-outline"
                                    placeholder="At least 6 characters" />
                            </div>
                            <asp:RequiredFieldValidator ID="rfvNewPassword" runat="server" ControlToValidate="txtNewPassword"
                                CssClass="text-error text-[12px] mt-1 inline-block" Display="Dynamic" ValidationGroup="Reset"
                                ErrorMessage="Please choose a new password." />
                            <asp:RegularExpressionValidator ID="revNewPassword" runat="server" ControlToValidate="txtNewPassword"
                                CssClass="text-error text-[12px] mt-1 inline-block" Display="Dynamic" ValidationGroup="Reset"
                                ValidationExpression="^(?=.*[A-Za-z])(?=.*\d).{6,}$"
                                ErrorMessage="Password must be at least 6 characters and contain letters and numbers." />
                        </div>

                        <div>
                            <label class="block font-label-md text-label-md text-on-surface-variant mb-2" for="<%= txtConfirmPassword.ClientID %>">Confirm New Password</label>
                            <div class="relative">
                                <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                                    <span class="material-symbols-outlined text-outline text-[20px]">lock</span>
                                </div>
                                <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password"
                                    CssClass="w-full bg-surface-container-low border border-outline-variant rounded-xl py-3 pl-12 pr-4 focus:ring-2 focus:ring-primary focus:border-primary transition-all font-body-md text-body-md placeholder:text-outline"
                                    placeholder="Re-enter your new password" />
                            </div>
                            <asp:RequiredFieldValidator ID="rfvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword"
                                CssClass="text-error text-[12px] mt-1 inline-block" Display="Dynamic" ValidationGroup="Reset"
                                ErrorMessage="Please confirm your new password." />
                            <asp:CompareValidator ID="cvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword"
                                ControlToCompare="txtNewPassword" CssClass="text-error text-[12px] mt-1 inline-block"
                                Display="Dynamic" ValidationGroup="Reset" ErrorMessage="Passwords do not match." />
                        </div>

                        <asp:Button ID="btnResetPassword" runat="server" Text="Update Password" OnClick="btnResetPassword_Click"
                            CausesValidation="true" ValidationGroup="Reset"
                            CssClass="w-full bg-primary text-on-primary font-title-lg text-title-lg py-4 rounded-full shadow-md hover:bg-primary-container hover:shadow-lg transition-all duration-300 btn-hover-effect flex items-center justify-center gap-2 cursor-pointer" />
                    </div>
                </asp:Panel>

                <!-- Invalid/Expired Token -->
                <asp:Panel ID="pnlInvalid" runat="server" Visible="false" CssClass="text-center py-6 space-y-4">
                    <div class="w-16 h-16 bg-error-container/20 rounded-full flex items-center justify-center mx-auto">
                        <span class="material-symbols-outlined text-error text-3xl">priority_high</span>
                    </div>
                    <h3 class="font-headline-md text-headline-md text-on-surface">Link Invalid or Expired</h3>
                    <p class="font-body-md text-body-md text-on-surface-variant leading-relaxed">
                        This password reset link is invalid, already used, or has expired (links expire after 60 minutes).
                        Please request a fresh link to continue.
                    </p>
                    <a href="ForgotPassword.aspx" class="inline-block bg-primary text-on-primary font-title-lg text-title-lg px-8 py-4 rounded-full shadow-md hover:bg-primary-container hover:shadow-lg transition-all duration-300 cursor-pointer">Request New Link</a>
                </asp:Panel>

                <!-- Success -->
                <asp:Panel ID="pnlSuccess" runat="server" Visible="false" CssClass="text-center py-6 space-y-4">
                    <div class="w-16 h-16 bg-tertiary-container/20 rounded-full flex items-center justify-center mx-auto">
                        <span class="material-symbols-outlined text-tertiary text-3xl">task_alt</span>
                    </div>
                    <h3 class="font-headline-md text-headline-md text-on-surface">Password Updated</h3>
                    <p class="font-body-md text-body-md text-on-surface-variant leading-relaxed">
                        Your password has been changed successfully. You can now sign in with your new credentials.
                    </p>
                    <a href="/LoginPopup.aspx" class="inline-block bg-primary text-on-primary font-title-lg text-title-lg px-8 py-4 rounded-full shadow-md hover:bg-primary-container hover:shadow-lg transition-all duration-300 cursor-pointer">Sign In</a>
                </asp:Panel>

                <!-- Card Footer -->
                <div class="mt-10 pt-6 border-t border-surface-container-high text-center">
                    <p class="font-body-md text-body-md text-on-surface-variant">
                        <a class="text-primary font-bold hover:underline decoration-primary/30 underline-offset-4" href="/LoginPopup.aspx">Back to Login</a>
                    </p>
                </div>
            </div>

            <p class="mt-6 text-center font-label-md text-label-md text-secondary/40">
                © 2026 DTAS. All rights reserved.
            </p>
        </div>
    </main>
</asp:Content>