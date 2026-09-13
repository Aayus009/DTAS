<%@ Page Title="Events | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="Events.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Events.Events" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>?v=navfix1" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <uc:AdminTopbar runat="server" />
    <asp:HiddenField ID="hfRejectReason" runat="server" />

    <div class="flex min-h-screen">
        
        <!-- Sidebar -->
        <uc:AdminSidebar ActivePage="Events" runat="server" />

        <!-- Main Content Area -->
        <main class="dashboard-main flex-1 ml-64 p-8">
            
            <!-- Header Section -->
            <header class="flex justify-between items-end mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Events Management</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">Review proposals and restrict events. System admin can view and block; organizers and managers edit their events.</p>
                </div>
            </header>

            <!-- Filters Row -->
            <div class="standard-card rounded-xl p-6 mb-6">
                <div class="flex flex-col md:flex-row gap-4 items-end">
                    <div class="flex-1 w-full">
                        <label class="font-badge-cap text-badge-cap text-on-surface-variant uppercase tracking-widest block mb-2">Search</label>
                        <div class="relative">
                            <span class="absolute left-3 top-1/2 -translate-y-1/2 material-symbols-outlined text-outline">search</span>
                            <asp:TextBox ID="txtSearch" runat="server" CssClass="js-event-search pl-10 pr-4 py-2.5 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md w-full focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="Search events by name..." autocomplete="off" />
                        </div>
                    </div>
                    <div class="w-full md:w-48">
                        <label class="font-badge-cap text-badge-cap text-on-surface-variant uppercase tracking-widest block mb-2">Status</label>
                        <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="js-event-status-filter w-full py-2.5 px-4 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none cursor-pointer">
                            <asp:ListItem Text="All Statuses" Value="" />
                            <asp:ListItem Text="Proposed" Value="Proposed" />
                            <asp:ListItem Text="Planned" Value="Planned" />
                            <asp:ListItem Text="In Progress" Value="InProgress" />
                            <asp:ListItem Text="Completed" Value="Completed" />
                            <asp:ListItem Text="Cancelled" Value="Cancelled" />
                            <asp:ListItem Text="Rejected" Value="Rejected" />
                        </asp:DropDownList>
                    </div>
                    <div class="w-full md:w-auto">
                        <asp:Button ID="btnSearch" runat="server" Text="Search" UseSubmitBehavior="false" OnClientClick="filterEventsLive(); return false;" CssClass="w-full md:w-auto px-6 py-2.5 bg-primary text-on-primary rounded-lg font-label-md text-label-md font-bold hover:scale-[1.02] active:scale-95 transition-transform cursor-pointer" />
                    </div>
                    <div class="w-full md:w-auto">
                        <asp:Button ID="btnReset" runat="server" Text="Reset" UseSubmitBehavior="false" OnClientClick="resetEventsLive(); return false;" CssClass="w-full md:w-auto px-6 py-2.5 border border-outline text-on-surface-variant rounded-lg font-label-md text-label-md font-bold hover:bg-surface-variant/50 transition-colors cursor-pointer" />
                    </div>
                </div>
            </div>

            <!-- Stats Summary -->
            <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-5 gap-4 mb-6">
                <div class="standard-card p-5 rounded-xl flex items-center gap-4">
                    <div class="h-12 w-12 rounded-lg bg-primary-container flex items-center justify-center">
                        <span class="material-symbols-outlined text-on-primary-container">event</span>
                    </div>
                    <div>
                        <p class="font-badge-cap text-badge-cap text-outline uppercase tracking-widest">Total Events</p>
                        <span id="statTotalEvents" class="font-headline-lg text-headline-lg text-primary font-bold leading-none">
                            <asp:Literal ID="litTotalEvents" runat="server" Text="0"></asp:Literal>
                        </span>
                    </div>
                </div>
                <div class="standard-card p-5 rounded-xl flex items-center gap-4">
                    <div class="h-12 w-12 rounded-lg bg-[rgba(0,81,55,0.1)] flex items-center justify-center">
                        <span class="material-symbols-outlined text-[#005137]">upcoming</span>
                    </div>
                    <div>
                        <p class="font-badge-cap text-badge-cap text-outline uppercase tracking-widest">Planned</p>
                        <span id="statPlanned" class="font-headline-lg text-headline-lg text-primary font-bold leading-none">
                            <asp:Literal ID="litPlanned" runat="server" Text="0"></asp:Literal>
                        </span>
                    </div>
                </div>
                <div class="standard-card p-5 rounded-xl flex items-center gap-4">
                    <div class="h-12 w-12 rounded-lg bg-[rgba(130,110,20,0.1)] flex items-center justify-center">
                        <span class="material-symbols-outlined text-[#826e14]">pending</span>
                    </div>
                    <div>
                        <p class="font-badge-cap text-badge-cap text-outline uppercase tracking-widest">In Progress</p>
                        <span id="statInProgress" class="font-headline-lg text-headline-lg text-primary font-bold leading-none">
                            <asp:Literal ID="litInProgress" runat="server" Text="0"></asp:Literal>
                        </span>
                    </div>
                </div>
                <div class="standard-card p-5 rounded-xl flex items-center gap-4">
                    <div class="h-12 w-12 rounded-lg bg-[rgba(26,94,48,0.1)] flex items-center justify-center">
                        <span class="material-symbols-outlined text-[#1a5e30]">check_circle</span>
                    </div>
                    <div>
                        <p class="font-badge-cap text-badge-cap text-outline uppercase tracking-widest">Completed</p>
                        <span id="statCompleted" class="font-headline-lg text-headline-lg text-primary font-bold leading-none">
                            <asp:Literal ID="litCompleted" runat="server" Text="0"></asp:Literal>
                        </span>
                    </div>
                </div>
                <div class="standard-card p-5 rounded-xl flex items-center gap-4">
                    <div class="h-12 w-12 rounded-lg bg-[rgba(230,126,0,0.1)] flex items-center justify-center">
                        <span class="material-symbols-outlined text-[#e67e00]">hourglass_top</span>
                    </div>
                    <div>
                        <p class="font-badge-cap text-badge-cap text-outline uppercase tracking-widest">Pending Proposals</p>
                        <span id="statPendingProposals" class="font-headline-lg text-headline-lg text-primary font-bold leading-none">
                            <asp:Literal ID="litPendingProposals" runat="server" Text="0"></asp:Literal>
                        </span>
                    </div>
                </div>
            </div>

            <!-- Events Table -->
            <section class="standard-card rounded-xl overflow-hidden">
                <div class="px-6 py-4 border-b border-surface-container-high flex justify-between items-center">
                    <div class="flex items-center gap-3">
                        <h3 class="font-title-lg text-title-lg text-primary">All Events</h3>
                        <span class="inline-flex items-center gap-1.5 text-[11px] font-bold uppercase tracking-widest text-primary">
                            <span class="h-1.5 w-1.5 rounded-full bg-[#2e7d32] animate-pulse"></span>
                            Live completion
                        </span>
                    </div>
                    <span id="eventVisibleCount" class="font-label-md text-label-md text-on-surface-variant"></span>
                </div>
                <div class="overflow-x-auto">
                    <table class="w-full text-left font-body-md">
                        <thead>
                            <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                <th class="px-6 py-3 font-semibold">Event Name</th>
                                <th class="px-6 py-3 font-semibold">Type</th>
                                <th class="px-6 py-3 font-semibold">Start Date</th>
                                <th class="px-6 py-3 font-semibold">End Date</th>
                                <th class="px-6 py-3 font-semibold">Status</th>
                                <th class="px-6 py-3 font-semibold min-w-[160px]">Completion</th>
                                <th class="px-6 py-3 font-semibold">Visibility</th>
                                <th class="px-6 py-3 font-semibold">Proposed By</th>
                                <th class="px-6 py-3 font-semibold text-right">Actions</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-surface-container-high">
                            <asp:Repeater ID="rptEvents" runat="server" OnItemDataBound="rptEvents_ItemDataBound" OnItemCommand="rptEvents_ItemCommand">
                                <ItemTemplate>
                                    <tr class="js-event-row hover:bg-surface-container-lowest transition-colors group" data-event-id="<%# Eval("EventID") %>" data-event-name="<%# System.Web.HttpUtility.HtmlAttributeEncode(Convert.ToString(Eval("EventName"))) %>" data-event-status="<%# System.Web.HttpUtility.HtmlAttributeEncode(Convert.ToString(Eval("Status")).Replace(" ", "")) %>">
                                        <td class="px-6 py-4">
                                            <div class="flex items-center gap-3">
                                                <div class="h-10 w-10 rounded-lg bg-primary-container flex items-center justify-center shrink-0">
                                                    <span class="material-symbols-outlined text-on-primary-container text-[20px]">calendar_today</span>
                                                </div>
                                                <div>
                                                    <span class="font-semibold text-on-surface"><%# Eval("EventName") %></span>
                                                    <p class="text-xs text-on-surface-variant truncate max-w-[200px]"><%# Eval("Venue") %></p>
                                                </div>
                                            </div>
                                        </td>
                                        <td class="px-6 py-4">
                                            <span class="text-on-surface-variant"><%# Eval("EventType") %></span>
                                        </td>
                                        <td class="px-6 py-4 whitespace-nowrap">
                                            <span class="text-on-surface-variant"><%# Eval("StartDate", "{0:MMM dd, yyyy}") %></span>
                                        </td>
                                        <td class="px-6 py-4 whitespace-nowrap">
                                            <span class="text-on-surface-variant"><%# Eval("EndDate", "{0:MMM dd, yyyy}") %></span>
                                        </td>
                                        <td class="px-6 py-4">
                                            <asp:Label ID="lblStatus" runat="server" CssClass="js-event-status px-3 py-1 rounded-full font-badge-cap text-badge-cap inline-block"></asp:Label>
                                            <asp:Label ID="lblRestricted" runat="server" Visible='<%# Convert.ToBoolean(Eval("IsRestricted")) %>'
                                                CssClass="ml-2 px-3 py-1 rounded-full font-badge-cap text-badge-cap inline-block bg-[rgba(198,40,40,0.12)] text-[#c62828]">Restricted</asp:Label>
                                        </td>
                                        <td class="px-6 py-4 min-w-[180px]">
                                            <div class="event-completion">
                                                <div class="flex justify-between items-baseline gap-2 mb-1">
                                                    <span class="js-completion-meta text-[11px] text-on-surface-variant"><%# CompletionMeta(Eval("TaskTotal"), Eval("TaskCompleted")) %></span>
                                                    <span class="js-completion-pct font-bold text-sm text-on-surface tabular-nums"><%# Eval("CompletionPercent") %>%</span>
                                                </div>
                                                <div class="h-2 bg-surface-container-high rounded-full overflow-hidden">
                                                    <div class="js-completion-bar h-full bg-[#2e7d32] rounded-full transition-all duration-500" style='width: <%# Eval("CompletionPercent") %>%'></div>
                                                </div>
                                            </div>
                                        </td>
                                        <td class="px-6 py-4 text-on-surface-variant"><%# Eval("Visibility") %></td>
                                        <td class="px-6 py-4">
                                            <asp:Label ID="lblProposedBy" runat="server"></asp:Label>
                                        </td>
                                        <td class="px-6 py-4 text-right">
                                            <div class="flex items-center justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                                                <asp:LinkButton ID="btnApprove" runat="server" CommandName="ApproveEvent" CommandArgument='<%# Eval("EventID") %>'
                                                    CssClass="p-2 hover:bg-tertiary-container/30 rounded-lg transition-colors" ToolTip="Approve Proposal"
                                                    Visible='<%# Eval("Status").ToString() == "Proposed" %>'>
                                                    <span class="material-symbols-outlined text-tertiary text-[20px]">check_circle</span>
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="btnReject" runat="server" CommandName="RejectEvent" CommandArgument='<%# Eval("EventID") %>'
                                                    CssClass="p-2 hover:bg-error-container/30 rounded-lg transition-colors" ToolTip="Reject Proposal"
                                                    Visible='<%# Eval("Status").ToString() == "Proposed" %>'
                                                    OnClientClick="return promptRejectReason(event, this);">
                                                    <span class="material-symbols-outlined text-error text-[20px]">cancel</span>
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="btnView" runat="server" CommandName="ViewEvent" CommandArgument='<%# Eval("EventID") %>'
                                                    CssClass="p-2 hover:bg-surface-container-low rounded-lg transition-colors" ToolTip="View Event">
                                                    <span class="material-symbols-outlined text-outline hover:text-primary transition-colors text-[20px]">visibility</span>
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="btnRestrict" runat="server" CommandName="RestrictEvent" CommandArgument='<%# Eval("EventID") %>'
                                                    Visible='<%# !Convert.ToBoolean(Eval("IsRestricted")) %>'
                                                    CssClass="p-2 hover:bg-error-container/50 rounded-lg transition-colors" ToolTip="Restrict event"
                                                    OnClientClick="return dtasConfirm(this, 'Restrict this event? Members will not be able to open it or work on its tasks.');">
                                                    <span class="material-symbols-outlined text-outline hover:text-error transition-colors text-[20px]">block</span>
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="btnRestore" runat="server" CommandName="RestoreEvent" CommandArgument='<%# Eval("EventID") %>'
                                                    Visible='<%# Convert.ToBoolean(Eval("IsRestricted")) %>'
                                                    CssClass="p-2 hover:bg-tertiary-container/30 rounded-lg transition-colors" ToolTip="Restore event"
                                                    OnClientClick="return dtasConfirm(this, 'Restore this event so members can use it again?');">
                                                    <span class="material-symbols-outlined text-tertiary text-[20px]">lock_open</span>
                                                </asp:LinkButton>
                                            </div>
                                        </td>
                                    </tr>
                                    <asp:PlaceHolder ID="phRejectRow" runat="server"
                                        Visible='<%# !string.IsNullOrEmpty(Eval("RejectionReason") != DBNull.Value ? Eval("RejectionReason").ToString() : "") %>'>
                                        <tr class="js-event-reject-row bg-error-container/5" data-event-id='<%# Eval("EventID") %>'>
                                            <td colspan="9" class="px-6 py-3 text-sm text-error">
                                                <span class="font-semibold">Rejection Reason:</span> <%# Eval("RejectionReason") %>
                                            </td>
                                        </tr>
                                    </asp:PlaceHolder>
                                </ItemTemplate>
                            </asp:Repeater>
                            <tr id="eventSearchEmpty" class="js-event-search-empty" style="display:none">
                                <td colspan="9" class="px-6 py-16 text-center">
                                    <p class="font-title-lg text-title-lg text-on-surface-variant mb-1">No matching events</p>
                                    <p class="font-body-md text-body-md text-outline">No event name matches what you typed.</p>
                                </td>
                            </tr>
                            <asp:Panel ID="pnlNoEvents" runat="server" Visible="false">
                                <div class="px-6 py-16 text-center">
                                    <div class="flex flex-col items-center gap-4">
                                        <div class="h-16 w-16 rounded-full bg-surface-container-high flex items-center justify-center">
                                            <span class="material-symbols-outlined text-outline text-[32px]">event_busy</span>
                                        </div>
                                        <div>
                                            <p class="font-title-lg text-title-lg text-on-surface-variant mb-1">No events found</p>
                                            <p class="font-body-md text-body-md text-outline">Try adjusting your search or filter criteria.</p>
                                        </div>
                                    </div>
                                </div>
                            </asp:Panel>
                        </tbody>
                    </table>
                </div>
            </section>
        </main>
    </div>

    <asp:SqlDataSource ID="dsLiveEventStats" runat="server"
        ConnectionString="<%$ ConnectionStrings:DBConnection %>"
        SelectCommand="SELECT COUNT(*) AS TotalEvents,
            ISNULL(SUM(CASE WHEN REPLACE(LTRIM(RTRIM(ISNULL(LiveStatus, N''))), N' ', N'') = N'Planned' THEN 1 ELSE 0 END), 0) AS Planned,
            ISNULL(SUM(CASE WHEN REPLACE(LTRIM(RTRIM(ISNULL(LiveStatus, N''))), N' ', N'') = N'InProgress' THEN 1 ELSE 0 END), 0) AS InProgress,
            ISNULL(SUM(CASE WHEN REPLACE(LTRIM(RTRIM(ISNULL(LiveStatus, N''))), N' ', N'') = N'Completed' THEN 1 ELSE 0 END), 0) AS Completed,
            ISNULL(SUM(CASE WHEN REPLACE(LTRIM(RTRIM(ISNULL(LiveStatus, N''))), N' ', N'') = N'Proposed' THEN 1 ELSE 0 END), 0) AS Proposed
            FROM (
                SELECT CASE
                    WHEN REPLACE(LTRIM(RTRIM(ISNULL(e.Status, N''))), N' ', N'') IN (N'Proposed', N'Rejected', N'Cancelled', N'Archived')
                        THEN LTRIM(RTRIM(e.Status))
                    WHEN ts.Total > 0 AND ts.Completed = ts.Total THEN N'Completed'
                    WHEN ISNULL(ts.InWork, 0) > 0 OR (ISNULL(ts.Completed, 0) > 0 AND ISNULL(ts.Completed, 0) &lt; ts.Total) THEN N'InProgress'
                    ELSE LTRIM(RTRIM(ISNULL(e.Status, N'Planned')))
                END AS LiveStatus
                FROM Events e
                OUTER APPLY (
                    SELECT COUNT(*) AS Total,
                        SUM(CASE WHEN REPLACE(LTRIM(RTRIM(ISNULL(t.Status, N''))), N' ', N'') = N'Completed' THEN 1 ELSE 0 END) AS Completed,
                        SUM(CASE WHEN REPLACE(LTRIM(RTRIM(ISNULL(t.Status, N''))), N' ', N'') IN
                            (N'InProgress', N'UnderReview', N'Delayed', N'RevisionNeeded', N'Submitted', N'ChangesRequested')
                            THEN 1 ELSE 0 END) AS InWork
                    FROM Tasks t
                    WHERE t.EventID = e.EventID
                      AND ISNULL(t.IsDeleted, 0) = 0
                      AND REPLACE(LTRIM(RTRIM(ISNULL(t.Status, N''))), N' ', N'') NOT IN (N'Archived', N'Cancelled')
                ) ts
                WHERE ISNULL(e.IsDeleted, 0) = 0
            ) live" />
    <asp:FormView ID="fvLiveEventStats" runat="server" DataSourceID="dsLiveEventStats">
        <ItemTemplate>
            <span id="liveStatTotalEvents" hidden="hidden"><%# Eval("TotalEvents") %></span>
            <span id="liveStatPlanned" hidden="hidden"><%# Eval("Planned") %></span>
            <span id="liveStatInProgress" hidden="hidden"><%# Eval("InProgress") %></span>
            <span id="liveStatCompleted" hidden="hidden"><%# Eval("Completed") %></span>
            <span id="liveStatPendingProposals" hidden="hidden"><%# Eval("Proposed") %></span>
        </ItemTemplate>
    </asp:FormView>

</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
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

        function filterEventsLive() {
            var searchEl = document.querySelector('.js-event-search');
            var statusEl = document.querySelector('.js-event-status-filter');
            var query = (searchEl && searchEl.value ? searchEl.value : '').trim().toLowerCase();
            var status = (statusEl && statusEl.value ? statusEl.value : '').replace(/\s+/g, '').toLowerCase();
            var rows = document.querySelectorAll('.js-event-row');
            var visible = 0;
            for (var i = 0; i < rows.length; i++) {
                var row = rows[i];
                var name = (row.getAttribute('data-event-name') || '').toLowerCase();
                var rowStatus = (row.getAttribute('data-event-status') || '').replace(/\s+/g, '').toLowerCase();
                var show = (!query || name.indexOf(query) !== -1) && (!status || rowStatus === status);
                row.style.display = show ? '' : 'none';
                if (show) visible++;
                var eventId = row.getAttribute('data-event-id');
                var rejectRows = document.querySelectorAll('.js-event-reject-row[data-event-id="' + eventId + '"]');
                for (var j = 0; j < rejectRows.length; j++) {
                    rejectRows[j].style.display = show ? '' : 'none';
                }
            }
            var empty = document.getElementById('eventSearchEmpty');
            if (empty) empty.style.display = (rows.length > 0 && visible === 0) ? '' : 'none';
            var count = document.getElementById('eventVisibleCount');
            if (count) {
                count.textContent = rows.length === 0 ? '' : (visible + (visible === 1 ? ' event' : ' events'));
            }
        }

        function resetEventsLive() {
            var searchEl = document.querySelector('.js-event-search');
            var statusEl = document.querySelector('.js-event-status-filter');
            if (searchEl) searchEl.value = '';
            if (statusEl) statusEl.selectedIndex = 0;
            filterEventsLive();
        }

        (function () {
            var searchEl = document.querySelector('.js-event-search');
            var statusEl = document.querySelector('.js-event-status-filter');
            if (searchEl) {
                searchEl.addEventListener('input', filterEventsLive);
                searchEl.addEventListener('keydown', function (e) {
                    if (e.key === 'Enter' || e.keyCode === 13) {
                        e.preventDefault();
                        filterEventsLive();
                    }
                });
            }
            if (statusEl) statusEl.addEventListener('change', filterEventsLive);
            filterEventsLive();
        })();

        (function () {
            function applyLiveStat(sourceId, destId) {
                var source = document.getElementById(sourceId);
                var dest = document.getElementById(destId);
                if (!source || !dest) return;
                dest.textContent = (source.textContent || '').trim();
            }
            applyLiveStat('liveStatTotalEvents', 'statTotalEvents');
            applyLiveStat('liveStatPlanned', 'statPlanned');
            applyLiveStat('liveStatInProgress', 'statInProgress');
            applyLiveStat('liveStatCompleted', 'statCompleted');
            applyLiveStat('liveStatPendingProposals', 'statPendingProposals');
        })();

        (function () {
            var api = '<%= ResolveUrl("~/Modules/Events/EventsProgressApi.ashx") %>';
            function setText(id, value) {
                var el = document.getElementById(id);
                if (el) el.textContent = String(value);
            }
            function statusClass(key) {
                var map = {
                    planned: 'badge-status-planned',
                    inprogress: 'badge-status-inprogress',
                    completed: 'badge-status-completed',
                    cancelled: 'badge-status-cancelled',
                    proposed: 'badge-status-proposed',
                    rejected: 'badge-status-rejected',
                    archived: 'badge-status-cancelled'
                };
                return 'js-event-status px-3 py-1 rounded-full font-badge-cap text-badge-cap inline-block ' +
                    (map[(key || '').toLowerCase()] || 'badge-status-planned');
            }
            function applyEvent(row) {
                var tr = document.querySelector('tr[data-event-id="' + row.eventId + '"]');
                if (!tr) return;
                var pct = row.percent || 0;
                var bar = tr.querySelector('.js-completion-bar');
                var label = tr.querySelector('.js-completion-pct');
                var meta = tr.querySelector('.js-completion-meta');
                var status = tr.querySelector('.js-event-status');
                if (bar) bar.style.width = pct + '%';
                if (label) label.textContent = pct + '%';
                if (meta) meta.textContent = row.label || 'No tasks yet';
                if (status && row.statusLabel) {
                    status.textContent = row.statusLabel;
                    status.className = statusClass(row.status);
                }
                if (row.status) {
                    tr.setAttribute('data-event-status', String(row.status).replace(/\s+/g, '').toLowerCase());
                }
            }
            function poll() {
                var xhr = new XMLHttpRequest();
                xhr.open('GET', api, true);
                xhr.onreadystatechange = function () {
                    if (xhr.readyState !== 4 || xhr.status !== 200) return;
                    try {
                        var data = JSON.parse(xhr.responseText);
                        if (!data || !data.ok) return;
                        var list = data.events || [];
                        for (var i = 0; i < list.length; i++) applyEvent(list[i]);
                        if (typeof filterEventsLive === 'function') filterEventsLive();
                        if (data.stats) {
                            setText('statTotalEvents', data.stats.total);
                            setText('statPlanned', data.stats.planned);
                            setText('statInProgress', data.stats.inProgress);
                            setText('statCompleted', data.stats.completed);
                            setText('statPendingProposals', data.stats.proposed);
                        }
                    } catch (err) { }
                };
                xhr.send();
            }
            setInterval(poll, 5000);
            if (document.visibilityState === 'visible') poll();
            document.addEventListener('visibilitychange', function () {
                if (document.visibilityState === 'visible') poll();
            });
        })();
    </script>
</asp:Content>
