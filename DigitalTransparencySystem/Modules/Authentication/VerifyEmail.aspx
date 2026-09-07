<%@ Page Title="Verify Email | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="VerifyEmail.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Authentication.VerifyEmail" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="/Assets/css/login.css" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="accent-blob-top"></div>
    <div class="accent-blob-bottom"></div>

    <main class="flex-grow flex items-center justify-center px-container-padding-mobile py-12 relative z-10">
        <div class="w-full max-w-[480px]">
            <div class="flex flex-col items-center mb-8">
                <div class="w-12 h-12 bg-primary rounded-lg flex items-center justify-center mb-4 shadow-lg">
                    <span class="text-on-primary font-bold text-xl">DT</span>
                </div>
                <h1 class="font-headline-lg text-headline-lg text-primary tracking-tight">DTAS</h1>
            </div>

            <div class="glass-card rounded-xl p-8 md:p-10">
                <h2 class="font-headline-md text-headline-md text-on-surface text-center mb-2">Verify your email</h2>
                <p class="font-body-md text-body-md text-on-surface-variant text-center mb-6">
                    Email verification is required once. After that, you sign in with your password only.
                </p>

                <asp:Label ID="lblMessage" runat="server" CssClass="hidden mb-4 p-3 bg-error-container text-error rounded-lg text-center font-body-md block" />

                <asp:Panel ID="pnlEmail" runat="server" CssClass="space-y-6">
                    <div>
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Institutional email</label>
                        <asp:TextBox ID="txtEmail" runat="server" TextMode="Email"
                            CssClass="w-full bg-surface-container-low border border-outline-variant rounded-xl py-3 px-4 font-body-md text-body-md"
                            placeholder="name@institution.edu" />
                    </div>
                    <asp:Button ID="btnSendCode" runat="server" Text="Send verification code" OnClick="btnSendCode_Click"
                        CssClass="w-full bg-primary text-on-primary font-title-lg text-title-lg py-4 rounded-full shadow-md cursor-pointer" />
                </asp:Panel>

                <asp:Panel ID="pnlOTP" runat="server" Visible="false" CssClass="space-y-6">
                    <asp:Label ID="lblOtpSentTo" runat="server" CssClass="font-body-md text-body-md text-on-surface-variant block text-center" />
                    <asp:TextBox ID="txtOTP" runat="server" MaxLength="6"
                        CssClass="w-full text-center text-[32px] tracking-[12px] bg-surface-container-low border border-outline-variant rounded-xl py-3"
                        placeholder="000000" />
                    <asp:Button ID="btnVerifyOTP" runat="server" Text="Verify email" OnClick="btnVerifyOTP_Click"
                        CssClass="w-full bg-primary text-on-primary font-title-lg text-title-lg py-4 rounded-full shadow-md cursor-pointer" />
                    <div class="text-center">
                        <asp:Button ID="btnResendOTP" runat="server" Text="Resend code" OnClick="btnResendOTP_Click"
                            CssClass="text-primary font-label-md bg-transparent border-none cursor-pointer" />
                    </div>
                </asp:Panel>

                <div class="mt-10 pt-6 border-t border-surface-container-high text-center">
                    <a class="text-primary font-bold hover:underline" href="/Modules/Authentication/Login.aspx">Back to sign in</a>
                </div>
            </div>
        </div>
    </main>
</asp:Content>
