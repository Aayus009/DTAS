<%@ Page Title="Identity Verification | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="IdentityVerification.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Settings.IdentityVerification" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <uc:UserTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:UserSidebar ActivePage="Identity" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            <header class="mb-8">
                <a href="<%= ResolveUrl("~/Modules/Dashboard/UsersDashboard.aspx") %>" class="inline-flex items-center gap-1 text-primary font-bold text-sm mb-3">
                    <span class="material-symbols-outlined text-[20px]">arrow_back</span>
                    Back
                </a>
                <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Identity verification</h1>
                <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">
                    Upload a clear photo or PDF of your institutional ID. Core portal features unlock after an administrator approves it.
                </p>
            </header>

            <asp:Panel ID="pnlStatus" runat="server" CssClass="standard-card rounded-xl p-6 mb-6">
                <span class="font-badge-cap text-badge-cap uppercase tracking-widest text-outline block mb-2">Current status</span>
                <asp:Label ID="lblStatus" runat="server" CssClass="font-title-lg text-title-lg text-on-surface block"></asp:Label>
                <asp:Label ID="lblStatusDetail" runat="server" CssClass="font-body-md text-body-md text-on-surface-variant block mt-2"></asp:Label>
            </asp:Panel>

            <asp:Panel ID="pnlCurrent" runat="server" Visible="false" CssClass="standard-card rounded-xl p-6 mb-6">
                <h3 class="font-title-lg text-title-lg text-primary mb-4">Submitted document</h3>
                <p class="font-body-md text-on-surface-variant mb-2">
                    <asp:Literal ID="litFileName" runat="server"></asp:Literal>
                    - <asp:Literal ID="litUploadDate" runat="server"></asp:Literal>
                </p>
                <asp:HyperLink ID="lnkViewDoc" runat="server" Target="_blank" CssClass="text-primary font-label-md font-bold hover:underline">View file</asp:HyperLink>
            </asp:Panel>

            <asp:Panel ID="pnlUpload" runat="server" CssClass="standard-card rounded-xl p-6">
                <h3 class="font-title-lg text-title-lg text-primary mb-6">Upload institutional ID</h3>
                <div class="space-y-5 max-w-xl">
                    <div>
                        <label class="font-label-md text-on-surface-variant block mb-2">Institutional ID (optional)</label>
                        <asp:TextBox ID="txtInstitutionalID" runat="server" MaxLength="100"
                            CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md"
                            placeholder="Student / staff number"></asp:TextBox>
                    </div>
                    <div>
                        <label class="font-label-md text-on-surface-variant block mb-2">ID document</label>
                        <asp:FileUpload ID="fuDocument" runat="server" />
                        <p class="text-xs text-outline mt-1">JPG, PNG, or PDF. Maximum 5 MB.</p>
                    </div>
                    <asp:Button ID="btnUpload" runat="server" Text="Submit for review" CssClass="btn-primary" OnClick="btnUpload_Click" />
                    <asp:Label ID="lblMessage" runat="server" CssClass="font-label-md block"></asp:Label>
                </div>
            </asp:Panel>
        </main>
    </div>
</asp:Content>
