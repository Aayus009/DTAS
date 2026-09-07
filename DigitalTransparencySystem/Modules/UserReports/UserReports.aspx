<%@ Page Title="Reports | DTAS Portal" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UserReports.aspx.cs" Inherits="DigitalTransparencySystem.Modules.UserReports.UserReports" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <uc:UserTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:UserSidebar ActivePage="Reports" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            <header class="flex justify-between items-end mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Accountability Reports</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">Scoped view of the events, tasks, and decisions you are connected to.</p>
                </div>
                <div class="flex flex-col items-end">
                    <span class="font-badge-cap text-badge-cap uppercase text-outline">Last Updated</span>
                    <asp:Literal ID="litLastUpdated" runat="server"></asp:Literal>
                </div>
            </header>

            <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-6">
                <div class="standard-card p-6 rounded-xl">
                    <span class="font-badge-cap text-badge-cap uppercase text-outline block mb-3">Linked Events</span>
                    <asp:Literal ID="litEvents" runat="server" Text="0"></asp:Literal>
                </div>
                <div class="standard-card p-6 rounded-xl">
                    <span class="font-badge-cap text-badge-cap uppercase text-outline block mb-3">My Tasks</span>
                    <asp:Literal ID="litTasks" runat="server" Text="0"></asp:Literal>
                </div>
                <div class="standard-card p-6 rounded-xl">
                    <span class="font-badge-cap text-badge-cap uppercase text-outline block mb-3">Completed</span>
                    <asp:Literal ID="litCompleted" runat="server" Text="0"></asp:Literal>
                </div>
                <div class="standard-card p-6 rounded-xl">
                    <span class="font-badge-cap text-badge-cap uppercase text-outline block mb-3">Overdue</span>
                    <asp:Literal ID="litOverdue" runat="server" Text="0"></asp:Literal>
                </div>
            </div>

            <section class="mb-8">
                <h2 class="font-title-lg text-title-lg text-primary mb-4">Generate Scoped Report</h2>
                <div class="standard-card p-6 rounded-xl flex flex-col md:flex-row md:items-center md:justify-between gap-4">
                    <div>
                        <h3 class="font-title-md text-on-surface mb-1">My Accountability Export</h3>
                        <p class="font-body-md text-sm text-on-surface-variant">Downloads a CSV of the tasks and decisions linked to your membership.</p>
                    </div>
                    <asp:Button ID="btnGenerate" runat="server" Text="Download CSV"
                        CssClass="px-6 py-2.5 bg-primary text-on-primary rounded-lg font-label-md text-label-md font-bold hover:opacity-90 transition-opacity cursor-pointer"
                        OnClick="btnGenerate_Click" />
                </div>
            </section>

            <section class="standard-card rounded-xl overflow-hidden mb-8">
                <div class="px-6 py-4 border-b border-surface-container-high">
                    <h3 class="font-title-lg text-title-lg text-primary">Assignment contribution reports</h3>
                    <p class="text-xs text-on-surface-variant">Scores come from the database formula (50% completion, 20% timeliness, 20% activity, 10% recency).</p>
                </div>
                <asp:Repeater ID="rptAssignmentGroups" runat="server">
                    <ItemTemplate>
                        <div class="px-6 py-4 border-b border-surface-container-high flex justify-between items-center gap-4">
                            <div>
                                <p class="font-semibold"><%# Eval("GroupName") %></p>
                                <p class="text-xs text-on-surface-variant"><%# Eval("AssignmentName") %> - Leader <%# Eval("LeaderName") %> - due <%# Eval("Deadline", "{0:MMM dd, yyyy}") %></p>
                            </div>
                            <a class="text-primary font-bold" href='<%# ResolveUrl("~/Modules/Assignments/ContributionReport.aspx?GroupID=" + Eval("GroupID")) %>'>Open report</a>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlNoAssignmentGroups" runat="server" Visible="false" CssClass="px-6 py-8 text-on-surface-variant">No assignment subgroups to report on yet.</asp:Panel>
            </section>

            <section class="standard-card rounded-xl overflow-hidden">
                <div class="px-6 py-4 border-b border-surface-container-high">
                    <h3 class="font-title-lg text-title-lg text-primary">Reports I Generated</h3>
                </div>
                <asp:Panel ID="pnlNoReports" runat="server" Visible="false" CssClass="p-8 text-on-surface-variant">
                    No reports generated yet.
                </asp:Panel>
                <asp:Repeater ID="rptReports" runat="server">
                    <HeaderTemplate>
                        <table class="w-full text-left font-body-md">
                            <thead>
                                <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                    <th class="px-6 py-3 font-semibold">Title</th>
                                    <th class="px-6 py-3 font-semibold">Type</th>
                                    <th class="px-6 py-3 font-semibold">Created</th>
                                </tr>
                            </thead>
                            <tbody class="divide-y divide-surface-container-high">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td class="px-6 py-4 font-semibold"><%# Eval("ReportTitle") %></td>
                            <td class="px-6 py-4 text-on-surface-variant"><%# Eval("ReportType") %></td>
                            <td class="px-6 py-4 whitespace-nowrap"><%# Eval("CreatedAt", "{0:MMM dd, yyyy - hh:mm tt}") %></td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                            </tbody>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
            </section>
        </main>
    </div>
</asp:Content>
