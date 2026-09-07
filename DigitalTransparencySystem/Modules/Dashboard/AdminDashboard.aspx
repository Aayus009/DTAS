<%@ Page Title="Dashboard | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="AdminDashboard.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Dashboard.AdminDashboard" %>
<%@ Import Namespace="DigitalTransparencySystem.Helpers" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>?v=6" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <uc:AdminTopbar runat="server" />

    <div class="flex min-h-screen">
        
        <!-- Sidebar -->
        <uc:AdminSidebar ActivePage="Dashboard" runat="server" />

        <!-- Main Content Area -->
        <main class="dashboard-main flex-1 ml-64 p-8">
            
            <!-- Header Section -->
            <header class="flex justify-between items-end mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Institutional Dashboard</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">Monitor real-time institutional metrics, active governance decisions, and pending administrative tasks.</p>
                </div>
                <div class="flex gap-3">
                    <div class="flex flex-col items-end">
                        <span class="font-badge-cap text-badge-cap uppercase text-outline">Last Updated</span>
                        <asp:Literal ID="litLastUpdated" runat="server" Text="MMM dd, yyyy - hh:mm tt"></asp:Literal>
                    </div>
                </div>
            </header>

            <!-- Metrics Bento Grid -->
            <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-6">
                
                <!-- Accountability -->
                <div class="md:col-span-1 standard-card p-6 flex flex-col justify-between rounded-xl">
                    <div>
                        <span class="font-badge-cap text-badge-cap text-tertiary uppercase tracking-widest block mb-4">Accountability</span>
                        <span class="font-headline-lg text-headline-lg text-primary font-bold leading-none">
                            <asp:Literal ID="litTrustIndex" runat="server" Text="0%"></asp:Literal>
                        </span>
                    </div>
                    <div class="mt-4">
                        <div class="w-full bg-surface-container-high h-2 rounded-full overflow-hidden">
                            <div id="barAccountability" runat="server" class="bg-on-tertiary-container h-full transition-all duration-1000" style="width: 0%"></div>
                        </div>
                        <p class="mt-2 font-label-md text-label-md text-on-surface-variant flex items-center gap-1">
                            <span class="material-symbols-outlined text-on-tertiary-container text-[18px]">task_alt</span>
                            Task completion across the institution
                        </p>
                    </div>
                </div>

                <!-- Active Decisions -->
                <div class="md:col-span-1 standard-card p-6 flex flex-col justify-between rounded-xl">
                    <div>
                        <span class="font-badge-cap text-badge-cap text-on-secondary-fixed-variant uppercase tracking-widest block mb-4">Active Decisions</span>
                        <span class="font-headline-lg text-headline-lg text-primary font-bold leading-none">
                            <asp:Literal ID="litActiveDecisions" runat="server" Text="12"></asp:Literal>
                        </span>
                    </div>
                    <div class="flex -space-x-2 overflow-hidden mt-4">
                        <div class="inline-block h-8 w-8 rounded-full ring-2 ring-white bg-primary-container"></div>
                        <div class="inline-block h-8 w-8 rounded-full ring-2 ring-white bg-secondary-container"></div>
                        <div class="inline-block h-8 w-8 rounded-full ring-2 ring-white bg-surface-variant"></div>
                        <div class="inline-block h-8 w-8 rounded-full ring-2 ring-white bg-outline flex items-center justify-center text-[10px] text-white">+5</div>
                    </div>
                </div>

                <!-- Operations Queue -->
                <div class="md:col-span-2 glass-card p-6 rounded-xl relative overflow-hidden flex flex-col justify-center">
                    <div class="relative z-10">
                        <h4 class="font-title-lg text-title-lg text-primary mb-2">Operations Queue</h4>
                        <div class="flex items-baseline gap-3 mb-4 flex-wrap">
                            <span class="font-headline-lg text-headline-lg text-primary font-bold leading-none">
                                <asp:Literal ID="litRecordRequests" runat="server" Text="0"></asp:Literal>
                            </span>
                            <span class="text-on-surface font-semibold text-[1.05rem]">items needing attention</span>
                        </div>
                        <div class="grid grid-cols-2 gap-4">
                            <div class="bg-surface/50 p-3 rounded-lg border border-outline-variant/30">
                                <span class="font-badge-cap text-badge-cap uppercase text-outline block">Pending Invites</span>
                                <span class="font-title-lg text-title-lg text-on-surface font-bold">
                                    <asp:Literal ID="litResponseTime" runat="server" Text="0"></asp:Literal>
                                </span>
                            </div>
                            <div class="bg-surface/50 p-3 rounded-lg border border-outline-variant/30">
                                <span class="font-badge-cap text-badge-cap uppercase text-outline block">Open Feedback</span>
                                <span class="font-title-lg text-title-lg text-on-surface font-bold">
                                    <asp:Literal ID="litFulfillmentRate" runat="server" Text="0"></asp:Literal>
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <asp:Panel ID="pnlIdentityQueue" runat="server" Visible="false" CssClass="standard-card rounded-xl p-5 mb-6">
                <div class="flex items-center justify-between gap-4">
                    <div>
                        <h3 class="font-title-lg text-title-lg text-primary mb-1">Identity documents to review</h3>
                        <asp:Label ID="lblIdentityQueue" runat="server" CssClass="font-body-md text-on-surface-variant"></asp:Label>
                    </div>
                    <asp:HyperLink ID="lnkIdentityQueue" runat="server" NavigateUrl="~/Modules/Users/IdentityQueue.aspx"
                        CssClass="py-2.5 px-5 bg-primary text-on-primary rounded-xl font-label-md font-bold whitespace-nowrap">Open queue</asp:HyperLink>
                </div>
            </asp:Panel>

            <section class="grid grid-cols-2 md:grid-cols-6 gap-4 mb-6">
                <div class="standard-card rounded-xl p-4">
                    <span class="font-badge-cap text-badge-cap uppercase text-outline block mb-2">Users</span>
                    <asp:Literal ID="litStatUsers" runat="server" Text="0"></asp:Literal>
                </div>
                <div class="standard-card rounded-xl p-4">
                    <span class="font-badge-cap text-badge-cap uppercase text-outline block mb-2">Pending IDs</span>
                    <asp:Literal ID="litStatPendingId" runat="server" Text="0"></asp:Literal>
                </div>
                <div class="standard-card rounded-xl p-4">
                    <span class="font-badge-cap text-badge-cap uppercase text-outline block mb-2">Assignments</span>
                    <asp:Literal ID="litStatAssignments" runat="server" Text="0"></asp:Literal>
                </div>
                <div class="standard-card rounded-xl p-4">
                    <span class="font-badge-cap text-badge-cap uppercase text-outline block mb-2">Clubs</span>
                    <asp:Literal ID="litStatGroups" runat="server" Text="0"></asp:Literal>
                </div>
                <div class="standard-card rounded-xl p-4">
                    <span class="font-badge-cap text-badge-cap uppercase text-outline block mb-2">Flags</span>
                    <asp:Literal ID="litStatFlags" runat="server" Text="0"></asp:Literal>
                </div>
                <div class="standard-card rounded-xl p-4">
                    <span class="font-badge-cap text-badge-cap uppercase text-outline block mb-2">Suspended</span>
                    <asp:Literal ID="litStatSuspended" runat="server" Text="0"></asp:Literal>
                </div>
            </section>

            <asp:Panel ID="pnlFlagsQueue" runat="server" Visible="false" CssClass="standard-card rounded-xl p-5 mb-6">
                <div class="flex items-center justify-between gap-4">
                    <div>
                        <h3 class="font-title-lg text-title-lg text-primary mb-1">Content flags to review</h3>
                        <asp:Label ID="lblFlagsQueue" runat="server" CssClass="font-body-md text-on-surface-variant"></asp:Label>
                    </div>
                    <asp:HyperLink ID="lnkFlagsQueue" runat="server" NavigateUrl="~/Modules/Users/Flags.aspx"
                        CssClass="py-2.5 px-5 bg-primary text-on-primary rounded-xl font-label-md font-bold whitespace-nowrap">Open flags</asp:HyperLink>
                </div>
            </asp:Panel>

            <!-- Pending Event Proposals Queue -->
            <asp:Panel ID="pnlProposals" runat="server" CssClass="standard-card rounded-xl overflow-hidden mb-6" Visible="false">
                <div class="px-6 py-4 border-b border-surface-container-high flex justify-between items-center">
                    <h3 class="font-title-lg text-title-lg text-primary flex items-center gap-2">
                        <span class="material-symbols-outlined text-[#e67e00]">hourglass_top</span>
                        Pending Event Proposals
                        <asp:Literal ID="litProposalCount" runat="server"></asp:Literal>
                    </h3>
                    <asp:HyperLink ID="lnkAllEvents" runat="server" NavigateUrl="~/Modules/Events/Events.aspx" CssClass="text-primary font-label-md text-label-md hover:underline">View All Events</asp:HyperLink>
                </div>
                <div class="overflow-x-auto">
                    <table class="w-full text-left font-body-md">
                        <thead>
                            <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                <th class="px-6 py-3 font-semibold">Proposed Event</th>
                                <th class="px-6 py-3 font-semibold">Type</th>
                                <th class="px-6 py-3 font-semibold">Proposed By</th>
                                <th class="px-6 py-3 font-semibold">Dates</th>
                                <th class="px-6 py-3 font-semibold text-right">Decide</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-surface-container-high">
                            <asp:Repeater ID="rptProposals" runat="server" OnItemCommand="rptProposals_ItemCommand">
                                <ItemTemplate>
                                    <tr class="hover:bg-surface-container-lowest transition-colors group">
                                        <td class="px-6 py-4">
                                            <div class="flex items-center gap-3">
                                                <div class="h-10 w-10 rounded-lg bg-[rgba(230,126,0,0.1)] flex items-center justify-center shrink-0">
                                                    <span class="material-symbols-outlined text-[#e67e00] text-[20px]">event</span>
                                                </div>
                                                <div>
                                                    <span class="font-semibold text-on-surface"><%# Eval("EventName") %></span>
                                                    <p class="text-xs text-on-surface-variant truncate max-w-[240px]"><%# Eval("Description") %></p>
                                                </div>
                                            </div>
                                        </td>
                                        <td class="px-6 py-4 text-on-surface-variant"><%# Eval("EventType") %></td>
                                        <td class="px-6 py-4 font-medium"><%# Eval("ProposedByName") %></td>
                                        <td class="px-6 py-4 text-on-surface-variant whitespace-nowrap">
                                            <%# Eval("StartDate") != DBNull.Value ? Eval("StartDate", "{0:MMM dd}") : "TBD" %> - <%# Eval("EndDate") != DBNull.Value ? Eval("EndDate", "{0:MMM dd}") : "TBD" %>
                                        </td>
                                        <td class="px-6 py-4 text-right">
                                            <div class="flex items-center justify-end gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                                                <asp:LinkButton ID="btnApproveProp" runat="server" CommandName="Approve" CommandArgument='<%# Eval("EventID") %>'
                                                    CssClass="px-3 py-1.5 bg-tertiary-container/30 text-on-tertiary-container rounded-lg font-label-md text-label-md font-bold hover:scale-105 active:scale-95 transition-transform"
                                                    ToolTip="Approve Proposal">
                                                    <span class="material-symbols-outlined text-[18px] align-middle">check_circle</span> Approve
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="btnRejectProp" runat="server" CommandName="Reject" CommandArgument='<%# Eval("EventID") %>'
                                                    CssClass="px-3 py-1.5 bg-error-container/30 text-error rounded-lg font-label-md text-label-md font-bold hover:scale-105 active:scale-95 transition-transform"
                                                    ToolTip="Reject Proposal" OnClientClick="return promptRejectReason(event, this);">
                                                    <span class="material-symbols-outlined text-[18px] align-middle">cancel</span> Reject
                                                </asp:LinkButton>
                                            </div>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>
            </asp:Panel>
            <asp:HiddenField ID="hfRejectReason" runat="server" />

            <!-- Live event progress -->
            <section class="standard-card rounded-xl p-4 mb-4" id="dashEventProgressRoot"
                data-api="<%= ResolveUrl("~/Modules/Events/EventsProgressApi.ashx") %>">
                <div class="dash-equal-head">
                    <div class="flex items-center gap-3 min-w-0">
                        <h3 class="font-title-lg text-title-lg text-primary">Live event progress</h3>
                        <span class="inline-flex items-center gap-1.5 text-[11px] font-bold uppercase tracking-widest text-primary shrink-0">
                            <span class="h-1.5 w-1.5 rounded-full bg-[#2e7d32] animate-pulse"></span>
                            Task data
                        </span>
                    </div>
                    <a href="<%= ResolveUrl("~/Modules/Events/Events.aspx") %>" class="text-primary font-label-md text-sm hover:underline shrink-0">Manage events</a>
                </div>
                <div class="dash-live-progress-list space-y-2">
                    <asp:Repeater ID="rptLiveEventProgress" runat="server">
                        <ItemTemplate>
                            <a href='<%# Eval("DetailsUrl") %>' class="block px-3 py-2.5 bg-surface-container-low rounded-lg border border-outline-variant/30 hover:border-primary/30 transition-colors"
                                data-event-id='<%# Eval("EventID") %>'>
                                <div class="flex justify-between items-baseline gap-3 mb-1">
                                    <p class="font-semibold text-on-surface text-sm truncate"><%# Eval("EventName") %></p>
                                    <span class="js-dash-pct font-bold text-sm text-on-surface tabular-nums shrink-0"><%# Eval("Percent") %>%</span>
                                </div>
                                <div class="flex justify-between text-[11px] text-on-surface-variant mb-1">
                                    <span class="js-dash-status"><%# Eval("StatusLabel") %></span>
                                    <span class="js-dash-meta"><%# Eval("CompletionLabel") %></span>
                                </div>
                                <div class="h-1.5 bg-surface-container-high rounded-full overflow-hidden">
                                    <div class="js-progress-fill js-dash-bar h-full rounded-full" style='<%# "width:" + Eval("Percent") + "%;background-color:#2e7d32;" %>'></div>
                                </div>
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                    <asp:Panel ID="pnlNoLiveEvents" runat="server" Visible="false" CssClass="text-center py-4">
                        <p class="font-body-md text-on-surface-variant">No active events to track yet.</p>
                    </asp:Panel>
                </div>
            </section>

            <!-- Charts Section -->
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
                <div class="standard-card dash-equal-card rounded-xl">
                    <h3 class="font-title-lg text-title-lg text-primary dash-equal-title">Task Completion</h3>
                    <div class="chart-container dash-equal-chart">
                        <canvas id="taskCompletionChart"></canvas>
                    </div>
                </div>
                <div class="standard-card dash-equal-card rounded-xl">
                    <h3 class="font-title-lg text-title-lg text-primary dash-equal-title">Decision Status</h3>
                    <div class="chart-container dash-equal-chart">
                        <canvas id="decisionStatusChart"></canvas>
                    </div>
                </div>
                <div class="standard-card dash-equal-card rounded-xl">
                    <h3 class="font-title-lg text-title-lg text-primary dash-equal-title">Events Overview</h3>
                    <div class="chart-container dash-equal-chart">
                        <canvas id="eventsOverviewChart"></canvas>
                    </div>
                </div>
            </div>

            <!-- Equal dashboard cards -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">

                <!-- Pending Reviews -->
                <div class="standard-card dash-equal-card rounded-xl">
                    <div class="dash-equal-head">
                        <h3 class="font-title-lg text-title-lg text-primary">Pending Reviews</h3>
                        <span class="bg-error-container text-on-error-container px-2 py-0.5 rounded font-badge-cap text-badge-cap">Urgent</span>
                    </div>
                    <div class="dash-equal-body space-y-2">
                        <asp:Repeater ID="rptPendingTasks" runat="server">
                            <ItemTemplate>
                                <div class="px-3 py-2 bg-surface-container-low rounded-lg border border-outline-variant/30 flex items-center gap-3">
                                    <span class="material-symbols-outlined text-error text-[18px] shrink-0">assignment_late</span>
                                    <div class="min-w-0">
                                        <p class="font-label-md text-sm font-bold truncate"><%# Eval("TaskTitle") %></p>
                                        <p class='<%# "text-[11px] " + TimelineService.DueClass(Eval("DueDate"), Eval("Status")) %>'>
                                            <%# TimelineService.ShortDueLabel(Eval("DueDate"), Eval("Status")) %>
                                        </p>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>

                <!-- Institution Health -->
                <div class="standard-card dash-equal-card rounded-xl">
                    <h3 class="font-title-lg text-title-lg text-primary dash-equal-title">Institution Health</h3>
                    <div class="dash-equal-body dash-equal-fill">
                        <div class="space-y-4">
                            <div>
                                <div class="flex justify-between font-label-md text-sm mb-1">
                                    <span>Task Completion</span>
                                    <asp:Literal ID="litComplianceRate" runat="server" Text="0%"></asp:Literal>
                                </div>
                                <div class="h-1.5 bg-surface-container-high rounded-full overflow-hidden">
                                    <div id="barTaskCompletion" runat="server" class="h-full bg-primary js-progress-fill" style="width: 0%"></div>
                                </div>
                            </div>
                            <div>
                                <div class="flex justify-between font-label-md text-sm mb-1">
                                    <span>Decision Progress</span>
                                    <asp:Literal ID="litAccessibilityRate" runat="server" Text="0%"></asp:Literal>
                                </div>
                                <div class="h-1.5 bg-surface-container-high rounded-full overflow-hidden">
                                    <div id="barDecisionProgress" runat="server" class="h-full bg-on-tertiary-container js-progress-fill" style="width: 0%"></div>
                                </div>
                            </div>
                        </div>
                        <asp:Button ID="btnFullAudit" runat="server" Text="Full Audit Report" CssClass="w-full py-2 border border-primary text-primary rounded-lg font-label-md text-sm font-bold hover:bg-primary-fixed-dim/10 transition-colors cursor-pointer" OnClick="btnFullAudit_Click" />
                    </div>
                </div>

                <!-- System Health -->
                <section class="standard-card dash-equal-card rounded-xl">
                    <h3 class="font-title-lg text-title-lg text-primary dash-equal-title">System Health</h3>
                    <div class="dash-equal-body space-y-3">
                        <asp:Repeater ID="rptSystemHealth" runat="server">
                            <ItemTemplate>
                                <div class="flex items-start gap-3">
                                    <div class="h-8 w-8 shrink-0 rounded flex items-center justify-center" style='<%# "background-color: " + Eval("IconBgColor") %>'>
                                        <span class="material-symbols-outlined text-[18px]" style='<%# "color: " + Eval("IconColor") %>'><%# Eval("IconName") %></span>
                                    </div>
                                    <div class="min-w-0">
                                        <h5 class="font-label-md text-sm font-bold"><%# Eval("Title") %></h5>
                                        <p class="text-[12px] text-on-surface-variant leading-snug"><%# Eval("Description") %></p>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </section>

                <!-- Audit Trail -->
                <section class="standard-card dash-equal-card rounded-xl">
                    <h3 class="font-title-lg text-title-lg text-primary dash-equal-title">Audit Trail</h3>
                    <div class="dash-equal-body space-y-3 relative before:absolute before:left-3 before:top-2 before:bottom-2 before:w-[1px] before:bg-outline-variant/30">
                        <asp:Repeater ID="rptAuditTrail" runat="server">
                            <ItemTemplate>
                                <div class="relative pl-8">
                                    <div class="absolute left-1.5 top-1.5 w-2.5 h-2.5 rounded-full ring-4 ring-surface" style='<%# "background-color: " + Eval("DotColor") %>'></div>
                                    <p class="font-label-md text-sm font-bold"><%# Eval("Title") %></p>
                                    <p class="text-[11px] text-on-surface-variant mb-0.5"><%# Eval("TimeAgo") %> - <%# Eval("Department") %></p>
                                    <p class="text-[12px] text-on-surface-variant italic truncate"><%# Eval("Description") %></p>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <asp:Button ID="btnSeeAllActivity" runat="server" Text="See All Activity" CssClass="dash-equal-foot w-full py-2 text-primary font-label-md text-sm font-bold hover:bg-primary-fixed-dim/10 rounded-lg transition-colors cursor-pointer" OnClick="btnSeeAllActivity_Click" />
                </section>
            </div>
        </main>
    </div>

    <!-- Dashboard Footer -->
    <footer class="dashboard-footer bg-on-secondary-fixed text-on-primary py-20 px-8 ml-64">
        <div class="grid grid-cols-1 md:grid-cols-4 gap-12 max-w-7xl mx-auto">
            <div class="md:col-span-1">
                <h2 class="font-headline-md text-headline-md font-bold mb-4">DTAS</h2>
                <p class="font-body-md text-surface-container-high/60">The authoritative platform for educational accountability and data transparency.</p>
            </div>
            <div>
                <h4 class="font-title-lg text-title-lg mb-4 text-on-primary-fixed-variant">Navigation</h4>
                <ul class="space-y-2 font-body-md text-surface-container-high/80">
                    <li><a class="hover:text-white transition-colors" href="#">Institutional Records</a></li>
                    <li><a class="hover:text-white transition-colors" href="#">Governance Portal</a></li>
                    <li><a class="hover:text-white transition-colors" href="#">Public Dashboard</a></li>
                    <li><a class="hover:text-white transition-colors" href="#">Ethics Guidelines</a></li>
                </ul>
            </div>
            <div>
                <h4 class="font-title-lg text-title-lg mb-4 text-on-primary-fixed-variant">Support</h4>
                <ul class="space-y-2 font-body-md text-surface-container-high/80">
                    <li><a class="hover:text-white transition-colors" href="#">Help Center</a></li>
                    <li><a class="hover:text-white transition-colors" href="#">API Documentation</a></li>
                    <li><a class="hover:text-white transition-colors" href="#">Status Page</a></li>
                    <li><a class="hover:text-white transition-colors" href="#">Submit Request</a></li>
                </ul>
            </div>
            <div>
                <h4 class="font-title-lg text-title-lg mb-4 text-on-primary-fixed-variant">Trust</h4>
                <div class="flex flex-wrap gap-4">
                    <div class="h-12 w-12 bg-white/10 rounded flex items-center justify-center">
                        <span class="material-symbols-outlined text-white">verified_user</span>
                    </div>
                    <div class="h-12 w-12 bg-white/10 rounded flex items-center justify-center">
                        <span class="material-symbols-outlined text-white">gpp_maybe</span>
                    </div>
                    <div class="h-12 w-12 bg-white/10 rounded flex items-center justify-center">
                        <span class="material-symbols-outlined text-white">policy</span>
                    </div>
                </div>
            </div>
        </div>
        <div class="border-t border-white/10 mt-16 pt-8 text-center text-surface-container-high/40 font-label-md text-label-md">
            © 2026 DTAS. All rights reserved. Encrypted and audited.
        </div>
    </footer>

</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Assets/js/dashboard.js") %>"></script>
    <script type="text/javascript">
        function promptRejectReason(event, link) {
            if (event) event.preventDefault();
            return dtasPromptPostback(link, '<%= hfRejectReason.ClientID %>', 'Please enter a reason for rejection.', {
                title: 'Reject proposal',
                required: true,
                requiredText: 'A rejection reason is required.',
                okText: 'Reject'
            });
        }
    </script>
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
                var errorContainer = themeColor('--dt-error-container', '#ffdad6');
                var outline = themeColor('--dt-outline', '#757682');
                var tickColor = onSurfaceVariant();
                var grid = gridColor();

                var legendFont = { family: 'Inter', size: 12, color: tickColor };

                // Task Completion Doughnut
                var taskCtx = document.getElementById('taskCompletionChart');
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
                var decisionCtx = document.getElementById('decisionStatusChart');
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
                                backgroundColor: [secondaryContainer, '#f59e0b', tertiaryContainer, errorContainer, tertiary],
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

                // Events Overview Pie
                var eventsCtx = document.getElementById('eventsOverviewChart');
                if (eventsCtx) {
                    chartInstances.push(new Chart(eventsCtx, {
                        type: 'pie',
                        data: {
                            labels: ['Planned', 'In Progress', 'Completed', 'Cancelled'],
                            datasets: [{
                                data: [<asp:Literal ID="litEventsPlanned" runat="server" Text="0"></asp:Literal>,
                                       <asp:Literal ID="litEventsInProgress" runat="server" Text="0"></asp:Literal>,
                                       <asp:Literal ID="litEventsCompleted" runat="server" Text="0"></asp:Literal>,
                                       <asp:Literal ID="litEventsCancelled" runat="server" Text="0"></asp:Literal>],
                                backgroundColor: [secondaryContainer, '#f59e0b', tertiary, outline],
                                borderWidth: 0
                            }]
                        },
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            plugins: {
                                legend: { position: 'bottom', labels: { padding: 12, usePointStyle: true, pointStyleWidth: 8, font: legendFont } }
                            }
                        }
                    }));
                }
            }

            initCharts();
            window.addEventListener('themechange', initCharts);

            (function () {
                var root = document.getElementById('dashEventProgressRoot');
                if (!root) return;
                var api = root.getAttribute('data-api');
                function poll() {
                    var xhr = new XMLHttpRequest();
                    xhr.open('GET', api, true);
                    xhr.onreadystatechange = function () {
                        if (xhr.readyState !== 4 || xhr.status !== 200) return;
                        try {
                            var data = JSON.parse(xhr.responseText);
                            if (!data || !data.ok) return;
                            var list = data.events || [];
                            for (var i = 0; i < list.length; i++) {
                                var row = list[i];
                                var card = root.querySelector('[data-event-id="' + row.eventId + '"]');
                                if (!card) continue;
                                var pct = row.percent || 0;
                                var p = card.querySelector('.js-dash-pct');
                                var s = card.querySelector('.js-dash-status');
                                var m = card.querySelector('.js-dash-meta');
                                var b = card.querySelector('.js-dash-bar');
                                if (p) p.textContent = pct + '%';
                                if (s && row.statusLabel) s.textContent = row.statusLabel;
                                if (m) m.textContent = row.label || '';
                                if (b) {
                                    b.style.width = pct + '%';
                                    b.style.backgroundColor = '#2e7d32';
                                }
                            }
                        } catch (err) { }
                    };
                    xhr.send();
                }
                setInterval(poll, 5000);
            })();
        });
    </script>
</asp:Content>