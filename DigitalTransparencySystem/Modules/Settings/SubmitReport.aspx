<%@ Page Title="Flag Content | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SubmitReport.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Settings.SubmitReport" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <uc:UserTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:UserSidebar ActivePage="Flag" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            <header class="mb-8">
                <a href="<%= ResolveUrl("~/Modules/Dashboard/UsersDashboard.aspx") %>" class="inline-flex items-center gap-1 text-primary font-bold text-sm mb-3">
                    <span class="material-symbols-outlined text-[20px]">arrow_back</span>
                    Back
                </a>
                <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Flag content</h1>
                <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">
                    Report a user or a specific record. An administrator will review the flag. This is separate from generated Reports.
                </p>
            </header>

            <section class="standard-card rounded-xl p-6 mb-6 max-w-2xl">
                <h3 class="font-title-lg text-title-lg text-primary mb-6">Submit a flag</h3>
                <div class="space-y-5">
                    <div>
                        <label class="font-label-md text-on-surface-variant block mb-2">What are you reporting?</label>
                        <asp:DropDownList ID="ddlTargetType" runat="server"
                            CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md">
                            <asp:ListItem Text="User" Value="User" />
                            <asp:ListItem Text="Event" Value="Event" />
                            <asp:ListItem Text="Task" Value="Task" />
                            <asp:ListItem Text="Club" Value="Club" />
                            <asp:ListItem Text="Assignment" Value="Assignment" />
                            <asp:ListItem Text="Message" Value="Message" />
                        </asp:DropDownList>
                    </div>
                    <div>
                        <label class="font-label-md text-on-surface-variant block mb-2">Target</label>
                        <asp:TextBox ID="txtTarget" runat="server"
                            CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md"
                            placeholder="User email, or a numeric ID for other types"></asp:TextBox>
                        <p class="text-xs text-outline mt-1">For a user, enter their email. For events, tasks, or clubs, enter the record ID.</p>
                    </div>
                    <div>
                        <label class="font-label-md text-on-surface-variant block mb-2">Reason</label>
                        <asp:DropDownList ID="ddlReason" runat="server"
                            CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md">
                            <asp:ListItem Text="Harassment" Value="Harassment" />
                            <asp:ListItem Text="Impersonation" Value="Impersonation" />
                            <asp:ListItem Text="Spam" Value="Spam" />
                            <asp:ListItem Text="Inappropriate content" Value="Inappropriate content" />
                            <asp:ListItem Text="Other" Value="Other" />
                        </asp:DropDownList>
                    </div>
                    <div>
                        <label class="font-label-md text-on-surface-variant block mb-2">Details</label>
                        <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="5"
                            CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md resize-none"
                            placeholder="Describe what happened..."></asp:TextBox>
                    </div>
                    <asp:Button ID="btnSubmit" runat="server" Text="Submit flag" CssClass="btn-primary" OnClick="btnSubmit_Click" />
                    <asp:Label ID="lblMessage" runat="server" CssClass="dtas-notice"></asp:Label>
                </div>
            </section>

            <section class="standard-card rounded-xl p-6 max-w-3xl">
                <h3 class="font-title-lg text-title-lg text-primary mb-6">Your flags</h3>
                <asp:Repeater ID="rptMine" runat="server">
                    <ItemTemplate>
                        <div class="p-4 bg-surface-container-low rounded-lg mb-4 last:mb-0">
                            <div class="flex justify-between items-start gap-4">
                                <div>
                                    <span class="font-label-md font-bold text-on-surface"><%# Eval("TargetType") %> #<%# Eval("TargetID") %></span>
                                    <span class="text-on-surface-variant ml-2"><%# Eval("Reason") %></span>
                                    <p class="text-sm text-on-surface-variant mt-1"><%# Eval("Description") %></p>
                                </div>
                                <span class="font-badge-cap text-badge-cap"><%# Eval("Status") %></span>
                            </div>
                            <p class="text-xs text-outline mt-2"><%# Eval("CreatedAt", "{0:MMM dd, yyyy}") %></p>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="text-on-surface-variant">
                    You have not submitted any flags yet.
                </asp:Panel>
            </section>
        </main>
    </div>
</asp:Content>
