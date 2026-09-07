<%@ Page Title="My Tasks | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UserTasks.aspx.cs" Inherits="DigitalTransparencySystem.Modules.UserTasks.UserTasks" %>
<%@ Import Namespace="DigitalTransparencySystem.Helpers" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>?v=2" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <uc:UserTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:UserSidebar ActivePage="Tasks" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            
            <!-- Header -->
            <header class="flex justify-between items-center mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">My Tasks</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant">Read-only view of all tasks assigned to you.</p>
                </div>
            </header>

            <!-- Filter Tabs -->
            <div class="flex gap-2 mb-6 flex-wrap">
                <asp:Button ID="btnFilterAll" runat="server" Text="All" CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-primary text-on-primary" OnClick="btnFilterAll_Click" />
                <asp:Button ID="btnFilterPending" runat="server" Text="Pending" CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors" OnClick="btnFilterPending_Click" />
                <asp:Button ID="btnFilterInProgress" runat="server" Text="In Progress" CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors" OnClick="btnFilterInProgress_Click" />
                <asp:Button ID="btnFilterCompleted" runat="server" Text="Completed" CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors" OnClick="btnFilterCompleted_Click" />
                <asp:Button ID="btnFilterLate" runat="server" Text="Late" CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors" OnClick="btnFilterLate_Click" />
            </div>

            <!-- Stats -->
            <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
                <div class="standard-card p-4 rounded-xl text-center">
                    <span class="font-badge-cap text-badge-cap text-outline uppercase block mb-1">Total</span>
                    <span class="font-headline-md text-headline-md text-primary stat-number"><asp:Literal ID="litTotal" runat="server" Text="0"></asp:Literal></span>
                </div>
                <div class="standard-card p-4 rounded-xl text-center">
                    <span class="font-badge-cap text-badge-cap text-outline uppercase block mb-1">Pending</span>
                    <span class="font-headline-md text-headline-md text-on-secondary-fixed-variant stat-number"><asp:Literal ID="litPending" runat="server" Text="0"></asp:Literal></span>
                </div>
                <div class="standard-card p-4 rounded-xl text-center">
                    <span class="font-badge-cap text-badge-cap text-outline uppercase block mb-1">In Progress</span>
                    <span class="font-headline-md text-headline-md stat-number" style="color: #f59e0b"><asp:Literal ID="litInProgress" runat="server" Text="0"></asp:Literal></span>
                </div>
                <div class="standard-card p-4 rounded-xl text-center">
                    <span class="font-badge-cap text-badge-cap text-outline uppercase block mb-1">Completed</span>
                    <span class="font-headline-md text-headline-md stat-number" style="color: #005137"><asp:Literal ID="litCompleted" runat="server" Text="0"></asp:Literal></span>
                </div>
            </div>

            <!-- Tasks Table -->
            <section class="standard-card rounded-xl overflow-hidden">
                <div class="overflow-x-auto">
                    <table class="w-full text-left font-body-md">
                        <thead>
                            <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                <th class="px-6 py-3 font-semibold">Task</th>
                                <th class="px-6 py-3 font-semibold">Event</th>
                                <th class="px-6 py-3 font-semibold">Priority</th>
                                <th class="px-6 py-3 font-semibold">Status</th>
                                <th class="px-6 py-3 font-semibold">Due Date</th>
                                <th class="px-6 py-3 font-semibold text-right">Actions</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-surface-container-high">
                            <asp:Repeater ID="rptTasks" runat="server" OnItemCommand="rptTasks_ItemCommand">
                                <ItemTemplate>
                                    <tr class="hover:bg-surface-container-lowest transition-colors">
                                        <td class="px-6 py-4">
                                            <p class="font-semibold"><%# Eval("TaskTitle") %></p>
                                            <p class="text-xs text-on-surface-variant truncate max-w-xs"><%# Eval("Description") %></p>
                                        </td>
                                        <td class="px-6 py-4 text-on-surface-variant"><%# Eval("EventName") %></td>
                                        <td class="px-6 py-4">
                                            <span class='<%# "badge-status-" + Eval("Priority").ToString().ToLower() %>'><%# Eval("Priority") %></span>
                                        </td>
                                        <td class="px-6 py-4">
                                            <span class='<%# TimelineService.StatusBadgeClass(Eval("Status"), Eval("DueDate")) %>'><%# TimelineService.StatusLabel(Eval("Status"), Eval("DueDate")) %></span>
                                        </td>
                                        <td class='<%# "px-6 py-4 whitespace-nowrap " + TimelineService.DueClass(Eval("DueDate"), Eval("Status")) %>'>
                                            <%# TimelineService.DueLabel(Eval("DueDate"), Eval("Status")) %>
                                        </td>
                                        <td class="px-6 py-4 text-right">
                                            <asp:Label runat="server" Visible='<%# Convert.ToInt32(Eval("IsRestricted")) == 1 %>'
                                                CssClass="px-3 py-1.5 rounded-lg text-label-md bg-[rgba(198,40,40,0.12)] text-[#c62828] font-bold">Restricted</asp:Label>
                                            <asp:HyperLink ID="lnkView" runat="server" Visible='<%# Convert.ToInt32(Eval("IsRestricted")) == 0 %>'
                                                NavigateUrl='<%# "~/Modules/UserTasks/TaskPreview.aspx?TaskID=" + Eval("TaskID") %>'
                                                CssClass="inline-flex items-center gap-1 px-3 py-1.5 bg-[rgba(46,125,50,0.1)] text-[#2e7d32] rounded-lg text-label-md hover:bg-[rgba(46,125,50,0.2)] transition-colors">
                                                <span class="material-symbols-outlined text-[16px]">visibility</span> View
                                            </asp:HyperLink>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>
                <asp:Panel ID="pnlNoTasks" runat="server" Visible="false" CssClass="p-12 text-center">
                    <span class="material-symbols-outlined text-6xl text-outline-variant mb-4 block empty-state-icon">assignment</span>
                    <p class="font-title-lg text-title-lg text-on-surface-variant">No tasks found</p>
                    <p class="text-body-md text-on-surface-variant mt-2">You don't have any tasks matching this filter.</p>
                </asp:Panel>
            </section>

            <!-- Pagination -->
            <div class="flex justify-between items-center mt-6">
                <asp:Label ID="lblPageInfo" runat="server" CssClass="text-label-md text-on-surface-variant"></asp:Label>
                <div class="flex gap-2">
                    <asp:Button ID="btnPrev" runat="server" Text="Previous" CssClass="btn-outline px-4 py-2" OnClick="btnPrev_Click" />
                    <asp:Button ID="btnNext" runat="server" Text="Next" CssClass="btn-outline px-4 py-2" OnClick="btnNext_Click" />
                </div>
            </div>

        </main>
    </div>
</asp:Content>
