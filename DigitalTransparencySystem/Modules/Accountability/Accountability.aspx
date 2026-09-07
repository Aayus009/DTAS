<%@ Page Title="Accountability Dashboard | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Accountability.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Accountability.Accountability" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <uc:AdminTopbar runat="server" />

    <div class="flex min-h-screen">

        <!-- Sidebar -->
        <uc:AdminSidebar ActivePage="Accountability" runat="server" />

        <!-- Main Content Area -->
        <main class="dashboard-main flex-1 ml-64 p-8">

            <!-- Header Section -->
            <header class="flex justify-between items-end mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Accountability Dashboard</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">Track who did what, when, and why. Full audit trail of institutional actions and community engagement.</p>
                </div>
                <div class="flex flex-col items-end">
                    <span class="font-badge-cap text-badge-cap uppercase text-outline">Last Updated</span>
                    <asp:Literal ID="litLastUpdated" runat="server" Text="MMM dd, yyyy - hh:mm tt"></asp:Literal>
                </div>
            </header>

            <!-- KPI Grid -->
            <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-6 mb-6">

                <!-- Total Logins -->
                <div class="standard-card p-6 rounded-xl">
                    <span class="font-badge-cap text-badge-cap text-tertiary uppercase tracking-widest block mb-4">Total Logins</span>
                    <div class="flex items-baseline gap-3">
                        <span class="text-headline-lg font-headline-lg text-primary"><asp:Literal ID="litTotalLogins" runat="server" Text="0"></asp:Literal></span>
                        <span class="font-badge-cap text-badge-cap uppercase text-outline">sessions</span>
                    </div>
                    <div class="mt-4 flex items-center gap-1 font-label-md text-label-md text-on-surface-variant">
                        <span class="material-symbols-outlined text-tertiary text-[18px]">login</span>
                        Every sign-in is logged with IP address
                    </div>
                </div>

                <!-- Active Users -->
                <div class="standard-card p-6 rounded-xl">
                    <span class="font-badge-cap text-badge-cap text-tertiary uppercase tracking-widest block mb-4">Active Users</span>
                    <div class="flex items-baseline gap-3">
                        <span class="text-headline-lg font-headline-lg text-primary"><asp:Literal ID="litActiveUsers" runat="server" Text="0"></asp:Literal></span>
                        <span class="font-badge-cap text-badge-cap uppercase text-outline">this month</span>
                    </div>
                    <div class="mt-4 flex items-center gap-1 font-label-md text-label-md text-on-surface-variant">
                        <span class="material-symbols-outlined text-on-tertiary-container text-[18px]">group</span>
                        Distinct users who signed in
                    </div>
                </div>

                <!-- Audit Events -->
                <div class="standard-card p-6 rounded-xl">
                    <span class="font-badge-cap text-badge-cap text-tertiary uppercase tracking-widest block mb-4">Audit Events</span>
                    <div class="flex items-baseline gap-3">
                        <span class="text-headline-lg font-headline-lg text-primary"><asp:Literal ID="litAuditEvents" runat="server" Text="0"></asp:Literal></span>
                        <span class="font-badge-cap text-badge-cap uppercase text-outline">tracked</span>
                    </div>
                    <div class="mt-4 flex items-center gap-1 font-label-md text-label-md text-on-surface-variant">
                        <span class="material-symbols-outlined text-tertiary text-[18px]">history_edu</span>
                        Status changes recorded in DecisionHistory
                    </div>
                </div>

                <!-- Community Responses -->
                <div class="standard-card p-6 rounded-xl">
                    <span class="font-badge-cap text-badge-cap text-tertiary uppercase tracking-widest block mb-4">Community Engaged</span>
                    <div class="flex items-baseline gap-3">
                        <span class="text-headline-lg font-headline-lg text-primary"><asp:Literal ID="litCommunityEngaged" runat="server" Text="0"></asp:Literal></span>
                        <span class="font-badge-cap text-badge-cap uppercase text-outline">responses</span>
                    </div>
                    <div class="mt-4 w-full bg-surface-container-high h-2 rounded-full overflow-hidden">
                        <div class="bg-on-tertiary-container h-full transition-all duration-1000" id="barEngagement" runat="server" style="width:0%"></div>
                    </div>
                    <p class="mt-2 font-label-md text-label-md text-on-surface-variant">feedback items with an official response</p>
                </div>
            </div>

            <!-- Two Column: Audit Trail + Recent Sign-ins -->
            <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">

                <!-- Audit Trail -->
                <section class="lg:col-span-2 standard-card rounded-xl overflow-hidden">
                    <div class="px-6 py-4 border-b border-surface-container-high">
                        <h3 class="font-title-lg text-title-lg text-primary">Decision Audit Trail</h3>
                        <p class="font-body-md text-body-md text-on-surface-variant mt-1">Immutable log of every status change on institutional decisions.</p>
                    </div>
                    <div class="overflow-x-auto">
                        <table class="w-full text-left font-body-md">
                            <thead>
                                <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                    <th class="px-6 py-3 font-semibold">Decision</th>
                                    <th class="px-6 py-3 font-semibold">Change</th>
                                    <th class="px-6 py-3 font-semibold">By</th>
                                    <th class="px-6 py-3 font-semibold">When</th>
                                </tr>
                            </thead>
                            <tbody class="divide-y divide-surface-container-high">
                                <asp:Repeater ID="rptAuditTrail" runat="server">
                                    <ItemTemplate>
                                        <tr class="hover:bg-surface-container-lowest transition-colors">
                                            <td class="px-6 py-4">
                                                <p class="font-semibold max-w-[280px] truncate"><%# Eval("DecisionTitle") %></p>
                                            </td>
                                            <td class="px-6 py-4">
                                                <span class="font-badge-cap text-badge-cap px-2 py-1 rounded bg-surface-variant text-on-surface-variant"><%# Eval("OldStatus") %></span>
                                                <span class="material-symbols-outlined text-outline text-[16px] align-middle mx-1">arrow_forward</span>
                                                <span class='font-badge-cap text-badge-cap px-2 py-1 rounded <%# GetStatusClass(Eval("NewStatus").ToString()) %>'><%# Eval("NewStatus") %></span>
                                            </td>
                                            <td class="px-6 py-4 text-on-surface-variant"><%# Eval("ChangedByName") %></td>
                                            <td class="px-6 py-4 text-on-surface-variant whitespace-nowrap"><%# Eval("ChangedAt", "{0:MMM dd, HH:mm}") %></td>
                                        </tr>
                                    </ItemTemplate>
                                    <EmptyDataTemplate>
                                        <tr><td colspan="4" class="px-6 py-10 text-center text-on-surface-variant">No audit events recorded yet.</td></tr>
                                    </EmptyDataTemplate>
                                </asp:Repeater>
                            </tbody>
                        </table>
                    </div>
                </section>

                <!-- Recent Sign-ins -->
                <section class="lg:col-span-1 standard-card rounded-xl overflow-hidden">
                    <div class="px-6 py-4 border-b border-surface-container-high">
                        <h3 class="font-title-lg text-title-lg text-primary">Recent Sign-ins</h3>
                    </div>
                    <div class="p-4 space-y-4">
                        <asp:Repeater ID="rptLogins" runat="server">
                            <ItemTemplate>
                                <div class="p-4 bg-surface-container-low rounded-lg border border-outline-variant/30 flex items-center gap-4">
                                    <div class="w-10 h-10 rounded-full bg-primary-container flex items-center justify-center flex-shrink-0">
                                        <span class="material-symbols-outlined text-on-primary-container">person</span>
                                    </div>
                                    <div class="min-w-0">
                                        <p class="font-label-md text-label-md font-bold truncate"><%# Eval("FullName") %></p>
                                        <p class="text-[12px] text-on-surface-variant truncate"><%# Eval("Username") %></p>
                                        <p class="text-[12px] text-on-surface-variant mt-0.5"><%# Eval("LoginTime", "{0:MMM dd, yyyy hh:mm tt}") %></p>
                                    </div>
                                    <span class="ml-auto font-badge-cap text-badge-cap px-2 py-1 rounded bg-tertiary-container text-on-tertiary-container flex-shrink-0"><%# Eval("IPAddress") %></span>
                                </div>
                            </ItemTemplate>
                            <EmptyDataTemplate>
                                <p class="text-on-surface-variant font-body-md text-center py-10">No sign-ins recorded yet.</p>
                            </EmptyDataTemplate>
                        </asp:Repeater>
                        <asp:HyperLink ID="lnkUserActivity" runat="server" NavigateUrl="~/Modules/Users/UserActivity.aspx" CssClass="block w-full mt-2 py-2 border border-primary text-primary rounded-lg font-label-md text-label-md font-bold text-center hover:bg-primary-fixed-dim/10 transition-colors">View Full Activity</asp:HyperLink>
                    </div>
                </section>
            </div>
        </main>
    </div>
</asp:Content>