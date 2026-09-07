<%@ Page Title="Task Workspaces | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TaskWorkspaces.aspx.cs" Inherits="DigitalTransparencySystem.Modules.TaskWorkspaces.TaskWorkspaces" %>
<%@ Import Namespace="DigitalTransparencySystem.Helpers" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <uc:UserTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:UserSidebar ActivePage="Workspaces" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">

            <!-- Header -->
            <header class="flex justify-between items-center mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Task Workspaces</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant">Open an event workplace to see every assigned task, comment, and faculty review in one shared space.</p>
                </div>
            </header>

            <!-- Pending Invitations -->
            <section class="standard-card rounded-xl p-6 mb-6" id="pnlPendingInvitations" runat="server">
                <div class="flex items-center gap-2 mb-4">
                    <span class="material-symbols-outlined text-primary">mail</span>
                    <h3 class="font-title-lg text-title-lg text-primary">Pending Invitations</h3>
                    <span id="pnlPendingCount" runat="server" class="ml-2 px-2 py-0.5 bg-[rgba(230,126,0,0.12)] text-[#e67e00] rounded-full font-badge-cap text-badge-cap">0</span>
                </div>
                <asp:Panel ID="pnlNoPending" runat="server" Visible="false" CssClass="p-4 text-center">
                    <p class="text-sm text-on-surface-variant">No pending invitations. You're all caught up.</p>
                </asp:Panel>
                <div class="space-y-3">
                    <asp:Repeater ID="rptPending" runat="server" OnItemCommand="rptPending_ItemCommand">
                        <ItemTemplate>
                            <div class="flex items-center justify-between gap-4 p-4 bg-surface-container-low rounded-xl border border-outline-variant/30">
                                <div class="flex-1 min-w-0">
                                    <p class="font-label-md text-label-md font-bold text-on-surface truncate"><%# Eval("TaskTitle") %></p>
                                    <p class="text-xs text-on-surface-variant mt-1"><%# Eval("Description") %></p>
                                    <div class="flex gap-2 mt-2 flex-wrap">
                                        <span class='<%# "badge-status-" + Eval("Priority").ToString().ToLower() %>'><%# Eval("Priority") %></span>
                                        <span class='<%# "text-xs " + TimelineService.DueClass(Eval("DueDate"), "") %>'><%# TimelineService.ShortDueLabel(Eval("DueDate"), "") %></span>
                                    </div>
                                </div>
                                <div class="flex gap-2 shrink-0">
                                    <asp:Button ID="btnAccept" runat="server" Text="Accept" CommandName="Accept" CommandArgument='<%# Eval("TaskID") %>'
                                        CssClass="px-4 py-2 bg-[rgba(46,125,50,0.1)] text-[#2e7d32] rounded-lg text-label-md hover:bg-[rgba(46,125,50,0.2)] transition-colors" />
                                    <asp:Button ID="btnDecline" runat="server" Text="Decline" CommandName="Decline" CommandArgument='<%# Eval("TaskID") %>'
                                        CssClass="px-4 py-2 bg-[rgba(211,47,47,0.1)] text-[#d32f2f] rounded-lg text-label-md hover:bg-[rgba(211,47,47,0.2)] transition-colors" />
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </section>

            <section class="standard-card rounded-xl p-6 mb-6">
                <div class="flex items-center gap-2 mb-4">
                    <span class="material-symbols-outlined text-primary">event</span>
                    <h3 class="font-title-lg text-title-lg text-primary">Event workplaces</h3>
                </div>
                <asp:Panel ID="pnlNoEvents" runat="server" Visible="false" CssClass="p-8 text-center">
                    <p class="text-body-md text-on-surface-variant">No event workplaces yet. After you join or create an event, it appears here like the cultural event workspace.</p>
                </asp:Panel>
                <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                    <asp:Repeater ID="rptEventWorkspaces" runat="server">
                        <ItemTemplate>
                            <a href='<%# Convert.ToBoolean(Eval("IsDisabled")) ? "#" : ResolveUrl("~/Modules/Events/EventWorkspace.aspx?EventID=" + Eval("EventID")) %>'
                               class='<%# Convert.ToBoolean(Eval("IsDisabled")) ? "block opacity-60 pointer-events-none" : "block group" %>'>
                                <div class="standard-card rounded-xl p-6 h-full hover:shadow-lg hover:-translate-y-0.5 transition-all cursor-pointer border border-outline-variant/30">
                                    <div class="flex justify-between items-start mb-4">
                                        <span class='<%# TimelineService.StatusBadgeClass(Eval("Status"), Eval("EndDate")) %>'><%# TimelineService.StatusLabel(Eval("Status"), Eval("EndDate")) %></span>
                                        <span class="badge-status-planned"><%# Eval("EventType") %></span>
                                    </div>
                                    <h4 class="font-label-lg text-label-lg text-on-surface font-bold mb-2 group-hover:text-primary transition-colors"><%# Eval("EventName") %></h4>
                                    <p class="text-sm text-on-surface-variant mb-4 line-clamp-2"><%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("Description"))) ? "Shared workplace for everyone in this event." : Eval("Description") %></p>
                                    <div class="flex justify-between items-center text-xs text-on-surface-variant">
                                        <span><%# Eval("TaskCount") %> task<%# Convert.ToInt32(Eval("TaskCount")) == 1 ? "" : "s" %></span>
                                        <span class="flex items-center gap-1 group-hover:text-primary transition-colors"><span class="material-symbols-outlined text-[14px]">arrow_forward</span> Open shared space</span>
                                    </div>
                                </div>
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </section>

            <!-- Accepted Workspaces -->
            <section class="standard-card rounded-xl p-6">
                <div class="flex items-center gap-2 mb-4">
                    <span class="material-symbols-outlined text-primary">workspaces</span>
                    <h3 class="font-title-lg text-title-lg text-primary">Other task workspaces</h3>
                </div>
                <asp:Panel ID="pnlNoAccepted" runat="server" Visible="false" CssClass="p-12 text-center">
                    <span class="material-symbols-outlined text-6xl text-outline-variant mb-4 block empty-state-icon">workspaces</span>
                    <p class="font-title-lg text-title-lg text-on-surface-variant">No other task workspaces</p>
                    <p class="text-body-md text-on-surface-variant mt-2">Event work is inside the event workplace above.</p>
                </asp:Panel>
                <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                    <asp:Repeater ID="rptWorkspaces" runat="server">
                        <ItemTemplate>
                            <a href='<%# ResolveUrl("~/Modules/TaskWorkspaces/TaskWorkspace.aspx?TaskID=" + Eval("TaskID")) %>' class="block group">
                                <div class="standard-card rounded-xl p-6 h-full hover:shadow-lg hover:-translate-y-0.5 transition-all cursor-pointer border border-outline-variant/30">
                                    <div class="flex justify-between items-start mb-4">
                                        <span class='<%# TimelineService.StatusBadgeClass(Eval("Status"), Eval("DueDate")) %>'><%# TimelineService.StatusLabel(Eval("Status"), Eval("DueDate")) %></span>
                                        <span class='<%# "badge-status-" + Eval("Priority").ToString().ToLower() %>'><%# Eval("Priority") %></span>
                                    </div>
                                    <h4 class="font-label-lg text-label-lg text-on-surface font-bold mb-2 group-hover:text-primary transition-colors"><%# Eval("TaskTitle") %></h4>
                                    <p class="text-sm text-on-surface-variant mb-4 line-clamp-2"><%# Eval("Description") %></p>
                                    <div class="flex justify-between items-center text-xs text-on-surface-variant">
                                        <span class='<%# "flex items-center gap-1 " + TimelineService.DueClass(Eval("DueDate"), Eval("Status")) %>'><span class="material-symbols-outlined text-[14px]">event</span> <%# TimelineService.ShortDueLabel(Eval("DueDate"), Eval("Status")) %></span>
                                        <span class="flex items-center gap-1 group-hover:text-primary transition-colors"><span class="material-symbols-outlined text-[14px]">arrow_forward</span> Open</span>
                                    </div>
                                </div>
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </section>

            <asp:Label ID="lblMessage" runat="server" CssClass="text-body-md text-on-surface-variant mt-4 block" Visible="false"></asp:Label>

        </main>
    </div>
</asp:Content>
