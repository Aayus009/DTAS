<%@ Page Title="Dashboard | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UsersDashboard.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Dashboard.UserDashboard" MaintainScrollPositionOnPostBack="true" %>
<%@ Import Namespace="DigitalTransparencySystem.Helpers" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>?v=notices1" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smUserDash" runat="server" EnablePartialRendering="true" />
    
    <uc:UserTopbar runat="server" />

    <div class="flex min-h-screen">
        
        <!-- Sidebar -->
        <uc:UserSidebar ActivePage="Dashboard" runat="server" />

        <!-- Main Content Area -->
        <main class="dashboard-main flex-1 ml-64 p-8">
            
            <!-- Header Section -->
            <header class="flex justify-between items-end mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Welcome Back, <asp:Literal ID="litFirstName" runat="server"></asp:Literal></h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl"><asp:Literal ID="litSubtitle" runat="server"></asp:Literal></p>
                </div>
                <div class="flex gap-3">
                    <div class="flex flex-col items-end">
                        <span class="font-badge-cap text-badge-cap uppercase text-outline">Last Updated</span>
                        <asp:Literal ID="litLastUpdated" runat="server"></asp:Literal>
                    </div>
                </div>
            </header>

            <asp:Panel ID="pnlSuspendedBanner" runat="server" Visible="false" CssClass="dtas-notice dtas-notice-danger dtas-notice-sticky dtas-notice-rich" role="status">
                <span class="material-symbols-outlined dtas-notice-icon">gavel</span>
                <div class="dtas-notice-body">
                    <p class="dtas-notice-kicker">Account</p>
                    <p class="dtas-notice-title">Account suspended</p>
                    <p class="dtas-notice-copy"><asp:Literal ID="litSuspendedBanner" runat="server"></asp:Literal></p>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlIdentityBanner" runat="server" Visible="false" CssClass="dtas-notice dtas-notice-warning dtas-notice-sticky dtas-notice-rich" role="status">
                <span class="material-symbols-outlined dtas-notice-icon">badge</span>
                <div class="dtas-notice-body">
                    <p class="dtas-notice-kicker">Required</p>
                    <p class="dtas-notice-title">Identity verification required</p>
                    <asp:Label ID="lblIdentityBanner" runat="server" CssClass="dtas-notice-copy"></asp:Label>
                </div>
                <asp:HyperLink ID="lnkVerifyIdentity" runat="server" NavigateUrl="~/Modules/Settings/IdentityVerification.aspx"
                    CssClass="dtas-notice-action">Verify ID</asp:HyperLink>
            </asp:Panel>

            <!-- Metrics Bento Grid -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
                
                <!-- Upcoming Meetings (Faculty/Staff only) -->
                <asp:Panel ID="pnlMeetingsMetric" runat="server" CssClass="standard-card p-6 flex flex-col justify-between rounded-xl">
                    <div>
                        <span class="font-badge-cap text-badge-cap text-on-tertiary-container uppercase tracking-widest block mb-4">Meetings</span>
                        <asp:Literal ID="litMyMeetings" runat="server" Text="0"></asp:Literal>
                    </div>
                    <div class="flex items-center gap-2 mt-4">
                        <span class="material-symbols-outlined text-on-tertiary-container text-[18px]">groups</span>
                        <p class="font-label-md text-label-md text-on-surface-variant">
                            <asp:Literal ID="litUpcomingMeetings" runat="server" Text="0"></asp:Literal> upcoming
                        </p>
                    </div>
                </asp:Panel>

                <!-- Notifications (Students - same slot as Meetings) -->
                <asp:Panel ID="pnlNotificationsMetric" runat="server" CssClass="standard-card p-6 flex flex-col justify-between rounded-xl" Visible="false">
                    <div>
                        <span class="font-badge-cap text-badge-cap text-on-tertiary-container uppercase tracking-widest block mb-4">Notifications</span>
                        <asp:Literal ID="litUnreadNotifications" runat="server" Text="0"></asp:Literal>
                    </div>
                    <div class="flex items-center gap-2 mt-4">
                        <span class="material-symbols-outlined text-on-tertiary-container text-[18px]">notifications</span>
                        <p class="font-label-md text-label-md text-on-surface-variant">unread</p>
                    </div>
                </asp:Panel>

                <!-- Feedback Status -->
                <div class="md:col-span-1 glass-card p-6 rounded-xl relative overflow-hidden flex flex-col justify-center">
                    <div class="relative z-10">
                        <h4 class="font-title-lg text-title-lg text-primary mb-2">Feedback</h4>
                        <div class="flex items-baseline gap-4 mb-4">
                            <asp:Literal ID="litTotalFeedback" runat="server" Text="0"></asp:Literal>
                            <span class="text-on-tertiary-container font-semibold">submissions</span>
                        </div>
                        <div class="grid grid-cols-2 gap-4">
                            <div class="bg-surface/50 p-3 rounded-lg border border-outline-variant/30">
                                <span class="font-badge-cap text-badge-cap uppercase text-outline block">Resolved</span>
                                <asp:Literal ID="litResolvedFeedback" runat="server" Text="0"></asp:Literal>
                            </div>
                            <div class="bg-surface/50 p-3 rounded-lg border border-outline-variant/30">
                                <span class="font-badge-cap text-badge-cap uppercase text-outline block">Pending</span>
                                <asp:Literal ID="litPendingFeedback" runat="server" Text="0"></asp:Literal>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Charts Section -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
                
                <!-- Task Completion Doughnut -->
                <div class="standard-card p-6 rounded-xl">
                    <h3 class="font-title-lg text-title-lg text-primary mb-4">My Task Completion</h3>
                    <div class="chart-container" style="height:220px">
                        <canvas id="myTaskCompletionChart"></canvas>
                    </div>
                </div>

                <!-- Decision Status Bar -->
                <div class="standard-card p-6 rounded-xl">
                    <h3 class="font-title-lg text-title-lg text-primary mb-4">Decision Overview</h3>
                    <div class="chart-container" style="height:220px">
                        <canvas id="myDecisionStatusChart"></canvas>
                    </div>
                </div>
            </div>

            <!-- Pending Team Invitations -->
            <asp:Panel ID="pnlInvitations" runat="server" CssClass="standard-card dash-panel rounded-xl p-6 mb-6" Visible="false">
                <div class="dash-panel-head flex items-center gap-3 mb-4">
                    <span class="material-symbols-outlined text-primary">mail</span>
                    <h3 class="font-title-lg text-title-lg text-primary">Pending Team Invitations</h3>
                </div>
                <div class="dash-scroll-invite">
                    <asp:Repeater ID="rptInvitations" runat="server" OnItemCommand="rptInvitations_ItemCommand">
                        <ItemTemplate>
                            <div class="flex flex-wrap items-center justify-between gap-3 p-4 bg-surface-container-low rounded-xl mb-3 border border-outline-variant/30">
                                <div class="flex items-center gap-3">
                                    <div class="h-10 w-10 rounded-lg bg-primary-container flex items-center justify-center shrink-0">
                                        <span class="material-symbols-outlined text-on-primary-container text-[20px]">assignment</span>
                                    </div>
                                    <div>
                                        <p class="font-label-md text-label-md font-bold text-on-surface"><%# Eval("TaskTitle") %></p>
                                        <p class="text-xs text-on-surface-variant">Invited as <%# Eval("TeamRole") %> by <%# Eval("LeaderName") %></p>
                                    </div>
                                </div>
                                <div class="flex items-center gap-2">
                                    <asp:LinkButton ID="btnAccept" runat="server" CommandName="Accept" CommandArgument='<%# Eval("MemberID") %>'
                                        CssClass="px-5 py-2 bg-primary text-on-primary rounded-lg font-label-md text-label-md font-bold active:scale-95 transition-transform">Accept</asp:LinkButton>
                                    <asp:LinkButton ID="btnDecline" runat="server" CommandName="Decline" CommandArgument='<%# Eval("MemberID") %>'
                                        CssClass="px-5 py-2 border border-outline text-on-surface-variant rounded-lg font-label-md text-label-md font-bold hover:bg-error-container/40 hover:text-error transition-colors">Decline</asp:LinkButton>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </asp:Panel>

            <!-- Main Layout: 2 Column Asymmetric -->
            <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
                
                <!-- Left Column: Tasks & Meetings (Wide) -->
                <div class="lg:col-span-2 space-y-6">
                    
                    <!-- My Active Tasks -->
                    <asp:UpdatePanel ID="upMyTasks" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                    <section class="standard-card dash-panel rounded-xl overflow-hidden">
                        <div class="dash-panel-head px-6 py-4 border-b border-surface-container-high flex flex-wrap justify-between items-center gap-3">
                            <h3 class="font-title-lg text-title-lg text-primary">My Active Tasks</h3>
                            <div class="flex gap-2 flex-wrap" role="group" aria-label="Task status filter">
                                <asp:Button ID="btnFilterAll" runat="server" Text="All" CausesValidation="false"
                                    UseSubmitBehavior="false" CssClass="dash-task-filter is-active"
                                    OnClick="btnFilterAll_Click" />
                                <asp:Button ID="btnFilterPending" runat="server" Text="Pending" CausesValidation="false"
                                    UseSubmitBehavior="false" CssClass="dash-task-filter"
                                    OnClick="btnFilterPending_Click" />
                                <asp:Button ID="btnFilterInProgress" runat="server" Text="In Progress" CausesValidation="false"
                                    UseSubmitBehavior="false" CssClass="dash-task-filter"
                                    OnClick="btnFilterInProgress_Click" />
                                <asp:Button ID="btnFilterCompleted" runat="server" Text="Completed" CausesValidation="false"
                                    UseSubmitBehavior="false" CssClass="dash-task-filter"
                                    OnClick="btnFilterCompleted_Click" />
                                <asp:Button ID="btnFilterLate" runat="server" Text="Late" CausesValidation="false"
                                    UseSubmitBehavior="false" CssClass="dash-task-filter"
                                    OnClick="btnFilterLate_Click" />
                            </div>
                        </div>
                        <asp:Panel ID="pnlTaskTable" runat="server" CssClass="dash-scroll">
                            <table class="w-full text-left font-body-md">
                                <thead>
                                    <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                        <th class="px-6 py-3 font-semibold">Task</th>
                                        <th class="px-6 py-3 font-semibold">Priority</th>
                                        <th class="px-6 py-3 font-semibold">Status</th>
                                        <th class="px-6 py-3 font-semibold">Due Date</th>
                                    </tr>
                                </thead>
                                <tbody class="divide-y divide-surface-container-high">
                                    <asp:Repeater ID="rptMyTasks" runat="server">
                                        <ItemTemplate>
                                            <tr class="hover:bg-surface-container-lowest transition-colors group">
                                                <td class="px-6 py-4">
                                                    <div class="flex items-center gap-3">
                                                        <div class="w-2 h-2 rounded-full" style='<%# GetPriorityDotColor(Eval("Priority").ToString()) %>'></div>
                                                        <div>
                                                            <a href='<%# ResolveUrl("~/Modules/UserTasks/TaskPreview.aspx?TaskID=" + Eval("TaskID")) %>' class="font-semibold hover:text-primary hover:underline"><%# Eval("TaskTitle") %></a>
                                                            <p class="text-xs text-on-surface-variant"><%# Eval("EventName") %></p>
                                                        </div>
                                                    </div>
                                                </td>
                                                <td class="px-6 py-4">
                                                    <span class='<%# "badge-status-" + Eval("Priority").ToString().ToLower() %>'><%# Eval("Priority") %></span>
                                                </td>
                                                <td class="px-6 py-4">
                                                    <span class='<%# TimelineService.StatusBadgeClass(Eval("Status"), Eval("DueDate")) %>'><%# TimelineService.StatusLabel(Eval("Status"), Eval("DueDate")) %></span>
                                                </td>
                                                <td class='<%# "px-6 py-4 whitespace-nowrap " + TimelineService.DueClass(Eval("DueDate"), Eval("Status")) %>'>
                                                    <%# TimelineService.DueLabel(Eval("DueDate"), Eval("Status")) %>
                                                </td>
                                            </tr>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </tbody>
                            </table>
                        </asp:Panel>
                        <asp:Panel ID="pnlNoTasks" runat="server" Visible="false" CssClass="p-10 text-center">
                            <span class="material-symbols-outlined text-5xl text-outline-variant mb-3 block">assignment</span>
                            <p class="font-title-lg text-title-lg text-on-surface-variant">No tasks found</p>
                            <p class="text-body-md text-on-surface-variant mt-2">You do not have any tasks matching this filter.</p>
                        </asp:Panel>
                        <asp:Panel ID="pnlTaskFooter" runat="server" CssClass="dash-panel-foot">
                            <a href="<%= ResolveUrl("~/Modules/UserTasks/UserTasks.aspx") %>" class="text-primary font-label-md text-label-md hover:underline">View all tasks</a>
                        </asp:Panel>
                    </section>
                        </ContentTemplate>
                    </asp:UpdatePanel>

                    <!-- Accountability Reports (Faculty/Staff only) -->
                    <asp:Panel ID="pnlReportsSection" runat="server" CssClass="standard-card rounded-xl overflow-hidden" Visible="false">
                        <div class="px-6 py-4 border-b border-surface-container-high flex justify-between items-center">
                            <h3 class="font-title-lg text-title-lg text-primary">Accountability Reports</h3>
                            <asp:HyperLink ID="lnkViewReports" runat="server" NavigateUrl="~/Modules/UserReports/UserReports.aspx" CssClass="text-primary font-label-md text-label-md hover:underline">Open Reports</asp:HyperLink>
                        </div>
                        <div class="grid grid-cols-1 md:grid-cols-3 gap-4 p-6">
                            <div class="bg-surface-container-low p-4 rounded-lg">
                                <span class="font-badge-cap text-badge-cap uppercase text-outline block mb-2">Linked Events</span>
                                <asp:Literal ID="litReportEvents" runat="server" Text="0"></asp:Literal>
                            </div>
                            <div class="bg-surface-container-low p-4 rounded-lg">
                                <span class="font-badge-cap text-badge-cap uppercase text-outline block mb-2">My Tasks</span>
                                <asp:Literal ID="litReportTasks" runat="server" Text="0"></asp:Literal>
                            </div>
                            <div class="bg-surface-container-low p-4 rounded-lg">
                                <span class="font-badge-cap text-badge-cap uppercase text-outline block mb-2">Overdue</span>
                                <asp:Literal ID="litReportOverdue" runat="server" Text="0"></asp:Literal>
                            </div>
                        </div>
                    </asp:Panel>

                    <!-- Upcoming Meetings (Faculty/Staff only) -->
                    <asp:Panel ID="pnlMeetingsSection" runat="server" CssClass="standard-card dash-panel rounded-xl overflow-hidden">
                        <div class="dash-panel-head px-6 py-4 border-b border-surface-container-high flex justify-between items-center">
                            <h3 class="font-title-lg text-title-lg text-primary">Upcoming Meetings</h3>
                            <asp:HyperLink ID="lnkViewAllMeetings" runat="server" NavigateUrl="~/Modules/UserMeetings/UserMeetings.aspx" CssClass="text-primary font-label-md text-label-md hover:underline">View All</asp:HyperLink>
                        </div>
                        <div class="dash-scroll">
                            <table class="w-full text-left font-body-md">
                                <thead>
                                    <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                        <th class="px-6 py-3 font-semibold">Meeting</th>
                                        <th class="px-6 py-3 font-semibold">Event</th>
                                        <th class="px-6 py-3 font-semibold">Date/Time</th>
                                        <th class="px-6 py-3 font-semibold">Status</th>
                                    </tr>
                                </thead>
                                <tbody class="divide-y divide-surface-container-high">
                                    <asp:Repeater ID="rptMyMeetings" runat="server">
                                        <ItemTemplate>
                                            <tr class="hover:bg-surface-container-lowest transition-colors group">
                                                <td class="px-6 py-4">
                                                    <div class="flex items-center gap-3">
                                                        <div class="w-2 h-2 rounded-full bg-error"></div>
                                                        <a href='<%# ResolveUrl("~/Modules/UserMeetings/UserMeetingDetails.aspx?MeetingID=" + Eval("MeetingID")) %>' class="font-semibold text-primary hover:underline"><%# Eval("MeetingTitle") %></a>
                                                    </div>
                                                </td>
                                                <td class="px-6 py-4 text-on-surface-variant"><%# Eval("EventName") %></td>
                                                <td class="px-6 py-4 whitespace-nowrap"><%# Eval("ScheduledDate", "{0:MMM dd, HH:mm tt}") %></td>
                                                <td class="px-6 py-4">
                                                    <span class='<%# "badge-status-" + Eval("Status").ToString().ToLower().Replace(" ", "") %>'><%# Eval("Status") %></span>
                                                </td>
                                            </tr>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </tbody>
                            </table>
                        </div>
                    </asp:Panel>
                </div>

                <!-- Right Column: Activity & Quick Actions -->
                <div class="lg:col-span-1 space-y-6">
                    
                    <!-- Quick Actions -->
                    <section class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-6">Quick Actions</h3>
                        <div class="space-y-3">
                            <asp:HyperLink ID="lnkTaskWorkspaces" runat="server" NavigateUrl="~/Modules/TaskWorkspaces/TaskWorkspaces.aspx"
                                CssClass="flex items-center gap-3 p-3 bg-surface-container-low rounded-lg hover:bg-surface-variant/50 transition-colors cursor-pointer">
                                <span class="material-symbols-outlined text-primary">workspaces</span>
                                <span class="font-label-md text-label-md">Open Task Workspaces</span>
                            </asp:HyperLink>
                            <asp:HyperLink ID="lnkSubmitFeedback" runat="server" NavigateUrl="~/Modules/UserFeedback/UserFeedback.aspx"
                                CssClass="flex items-center gap-3 p-3 bg-surface-container-low rounded-lg hover:bg-surface-variant/50 transition-colors cursor-pointer">
                                <span class="material-symbols-outlined text-primary">feedback</span>
                                <span class="font-label-md text-label-md">Submit Feedback</span>
                            </asp:HyperLink>
                            <asp:HyperLink ID="lnkViewPolls" runat="server" NavigateUrl="~/Modules/UserPolls/UserPolls.aspx"
                                CssClass="flex items-center gap-3 p-3 bg-surface-container-low rounded-lg hover:bg-surface-variant/50 transition-colors cursor-pointer">
                                <span class="material-symbols-outlined text-primary">how_to_vote</span>
                                <span class="font-label-md text-label-md">Vote on Polls</span>
                            </asp:HyperLink>
                            <asp:Panel ID="pnlQuickMeetings" runat="server">
                                <asp:HyperLink ID="lnkQuickMeetings" runat="server" NavigateUrl="~/Modules/UserMeetings/UserMeetings.aspx"
                                    CssClass="flex items-center gap-3 p-3 bg-surface-container-low rounded-lg hover:bg-surface-variant/50 transition-colors cursor-pointer">
                                    <span class="material-symbols-outlined text-primary">groups</span>
                                    <span class="font-label-md text-label-md">My Meetings</span>
                                </asp:HyperLink>
                            </asp:Panel>
                            <asp:Panel ID="pnlQuickReports" runat="server">
                                <asp:HyperLink ID="lnkQuickReports" runat="server" NavigateUrl="~/Modules/UserReports/UserReports.aspx"
                                    CssClass="flex items-center gap-3 p-3 bg-surface-container-low rounded-lg hover:bg-surface-variant/50 transition-colors cursor-pointer">
                                    <span class="material-symbols-outlined text-primary">analytics</span>
                                    <span class="font-label-md text-label-md">Accountability Reports</span>
                                </asp:HyperLink>
                            </asp:Panel>
                            <asp:HyperLink ID="lnkViewTransparency" runat="server" NavigateUrl="~/Transparency.aspx"
                                CssClass="flex items-center gap-3 p-3 bg-surface-container-low rounded-lg hover:bg-surface-variant/50 transition-colors cursor-pointer">
                                <span class="material-symbols-outlined text-primary">visibility</span>
                                <span class="font-label-md text-label-md">Transparency Portal</span>
                            </asp:HyperLink>
                            <asp:HyperLink ID="lnkMyProfile" runat="server" NavigateUrl="~/Modules/Settings/UserProfile.aspx"
                                CssClass="flex items-center gap-3 p-3 bg-surface-container-low rounded-lg hover:bg-surface-variant/50 transition-colors cursor-pointer">
                                <span class="material-symbols-outlined text-primary">person</span>
                                <span class="font-label-md text-label-md">Edit Profile</span>
                            </asp:HyperLink>
                        </div>
                    </section>

                    <!-- Recent Activity -->
                    <section class="standard-card dash-panel rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-4 dash-panel-head">Recent Activity</h3>
                        <div class="dash-scroll-feed space-y-6 relative before:absolute before:left-3 before:top-2 before:bottom-2 before:w-[1px] before:bg-outline-variant/30">
                            <asp:Repeater ID="rptRecentActivity" runat="server">
                                <ItemTemplate>
                                    <div class="relative pl-8">
                                        <div class="absolute left-1.5 top-1.5 w-3 h-3 rounded-full ring-4 ring-surface" style='<%# "background-color: " + Eval("DotColor") %>'></div>
                                        <p class="font-label-md text-label-md font-bold"><%# Eval("Title") %></p>
                                        <p class="text-xs text-on-surface-variant mb-1"><%# Eval("TimeAgo") %></p>
                                        <p class="text-sm text-on-surface-variant italic"><%# Eval("Description") %></p>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </section>
                </div>
            </div>
        </main>
    </div>

</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Assets/js/dashboard.js") %>"></script>
    <script type="text/javascript">
        document.addEventListener('DOMContentLoaded', function () {
            var chartInstances = [];

            function themeColor(name, fallback) {
                var value = (window.dtCss ? window.dtCss(name) : '') || fallback || '';
                return value;
            }

            function onSurfaceVariant() { return themeColor('--dt-on-surface-variant', '#444651'); }
            function gridColor() {
                return 'rgba(' + (themeColor('--dt-outline-variant-rgb', '197 197 211')) + ' / 0.25)';
            }

            function initCharts() {
                chartInstances.forEach(function (ch) { if (ch) ch.destroy(); });
                chartInstances = [];

                var primary = themeColor('--dt-primary', '#001142');
                var primaryContainer = themeColor('--dt-primary-container', '#00236f');
                var tertiary = themeColor('--dt-tertiary', '#005137');
                var tertiaryContainer = themeColor('--dt-tertiary-container', '#85f8c4');
                var secondaryContainer = themeColor('--dt-secondary-container', '#d4e3ff');
                var error = themeColor('--dt-error', '#ba1a1a');
                var outline = themeColor('--dt-outline', '#757682');
                var tickColor = onSurfaceVariant();
                var grid = gridColor();

                var legendFont = { family: 'Inter', size: 12, color: tickColor };

                // My Task Completion Doughnut
                var taskCtx = document.getElementById('myTaskCompletionChart');
                if (taskCtx) {
                    chartInstances.push(new Chart(taskCtx, {
                        type: 'doughnut',
                        data: {
                            labels: ['Completed', 'In Progress', 'Pending', 'Delayed'],
                            datasets: [{
                                data: [<asp:Literal ID="litTasksCompleted" runat="server" Text="0"></asp:Literal>,
                                       <asp:Literal ID="litTasksInProgress" runat="server" Text="0"></asp:Literal>,
                                       <asp:Literal ID="litTasksPending" runat="server" Text="0"></asp:Literal>,
                                       <asp:Literal ID="litTasksDelayed" runat="server" Text="0"></asp:Literal>],
                                backgroundColor: [tertiary, secondaryContainer, outline, error],
                                borderWidth: 0,
                                borderRadius: 4
                            }]
                        },
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            cutout: '65%',
                            plugins: {
                                legend: { position: 'bottom', labels: { padding: 12, usePointStyle: true, pointStyleWidth: 8, font: legendFont } }
                            }
                        }
                    }));
                }

                // Decision Status Bar
                var decisionCtx = document.getElementById('myDecisionStatusChart');
                if (decisionCtx) {
                    chartInstances.push(new Chart(decisionCtx, {
                        type: 'bar',
                        data: {
                            labels: ['Proposed', 'Under Review', 'Approved', 'Rejected', 'Completed'],
                            datasets: [{
                                label: 'Decisions',
                                data: [<asp:Literal ID="litDecisionsProposed" runat="server" Text="0"></asp:Literal>,
                                       <asp:Literal ID="litDecisionsUnderReview" runat="server" Text="0"></asp:Literal>,
                                       <asp:Literal ID="litDecisionsApproved" runat="server" Text="0"></asp:Literal>,
                                       <asp:Literal ID="litDecisionsRejected" runat="server" Text="0"></asp:Literal>,
                                       <asp:Literal ID="litDecisionsCompleted" runat="server" Text="0"></asp:Literal>],
                                backgroundColor: [secondaryContainer, '#f59e0b', tertiaryContainer, error, tertiary],
                                borderRadius: 6,
                                borderSkipped: false
                            }]
                        },
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            plugins: { legend: { display: false } },
                            scales: {
                                y: { beginAtZero: true, grid: { color: grid }, ticks: { color: tickColor, font: { family: 'Inter', size: 11 } } },
                                x: { grid: { display: false }, ticks: { color: tickColor, font: { family: 'Inter', size: 11 } } }
                            }
                        }
                    }));
                }
            }

            initCharts();
            window.addEventListener('themechange', initCharts);

            if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                    if (document.activeElement && document.activeElement.blur)
                        document.activeElement.blur();
                });
            }

            // Sidebar toggle for mobile
            var toggleBtn = document.getElementById('btnUserSidebarToggle');
            if (toggleBtn) {
                toggleBtn.addEventListener('click', function () {
                    var sidebar = document.querySelector('.dashboard-sidebar');
                    if (sidebar) sidebar.classList.toggle('is-open');
                });
            }
        });
    </script>
</asp:Content>
