<%@ Page Title="Event Details | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="EventDetails.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Events.EventDetails" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <uc:AdminTopbar runat="server" />

    <div class="flex min-h-screen">

        <!-- Sidebar -->
        <uc:AdminSidebar ActivePage="Events" runat="server" />

        <!-- Main Content Area -->
        <main class="dashboard-main flex-1 ml-64 p-8">

            <!-- Breadcrumb -->
            <nav class="flex items-center gap-2 mb-6 font-label-md text-label-md text-on-surface-variant">
                <a href="<%= ResolveUrl("~/Modules/Dashboard/AdminDashboard.aspx") %>" class="hover:text-primary transition-colors">Dashboard</a>
                <span class="material-symbols-outlined text-[16px]">chevron_right</span>
                <a href="<%= ResolveUrl("~/Modules/Events/Events.aspx") %>" class="hover:text-primary transition-colors">Events</a>
                <span class="material-symbols-outlined text-[16px]">chevron_right</span>
                <span class="text-primary font-semibold">Event Details</span>
            </nav>

            <!-- Header Section -->
            <header class="flex justify-between items-start mb-8">
                <div>
                    <div class="flex items-center gap-4 mb-3">
                        <asp:Literal ID="litEventName" runat="server" Text="Event Name"></asp:Literal>
                        <span id="eventStatusBadge"><asp:Literal ID="litStatusBadge" runat="server"></asp:Literal></span>
                    </div>
                    <div class="flex items-center gap-3">
                        <asp:Literal ID="litEventType" runat="server"></asp:Literal>
                        <span class="text-on-surface-variant">-</span>
                        <span class="font-body-md text-body-md text-on-surface-variant">
                            <span class="material-symbols-outlined text-[16px] align-middle">person</span>
                            Created by <asp:Literal ID="litCreatedBy" runat="server"></asp:Literal>
                        </span>
                    </div>
                </div>
                <div class="flex gap-3">
                    <asp:Button ID="btnRestrict" runat="server" Text="Restrict event" CssClass="py-2 px-5 border border-error text-error rounded-lg font-label-md text-label-md font-bold hover:bg-error-container/30 transition-colors cursor-pointer" OnClick="btnRestrict_Click"
                        OnClientClick="return dtasConfirm(this, 'Restrict this event? Members will not be able to open it or work on its tasks.');" />
                    <asp:Button ID="btnRestore" runat="server" Text="Restore event" Visible="false" CssClass="py-2 px-5 bg-primary text-on-primary rounded-lg font-label-md text-label-md font-bold cursor-pointer" OnClick="btnRestore_Click"
                        OnClientClick="return dtasConfirm(this, 'Restore this event so members can use it again?');" />
                    <asp:Button ID="btnBackToList" runat="server" Text="Back to Events" CssClass="py-2 px-5 border border-outline text-on-surface-variant rounded-lg font-label-md text-label-md font-bold hover:bg-surface-container-low transition-colors cursor-pointer" OnClick="btnBackToList_Click" />
                </div>
            </header>

            <!-- Status Timeline -->
            <section class="standard-card rounded-xl p-6 mb-6" id="eventLifecycleRoot"
                data-api="<%= ResolveUrl("~/Modules/Events/EventsProgressApi.ashx") %>"
                data-event-id="<%= Request.QueryString["EventID"] %>"
                data-lifecycle-index="<%= CurrentLifecycleIndex %>">
                <h3 class="font-title-lg text-title-lg text-primary mb-6">Event Lifecycle</h3>
                <div class="flex items-center justify-between relative">
                    <div class="absolute top-4 left-0 right-0 h-1 bg-surface-container-high rounded-full"></div>
                    <div id="timelineFill" runat="server" class="absolute top-4 left-0 h-1 rounded-full z-[1]" style="width: 0%; background-color: #0077b6;"></div>
                    <asp:Repeater ID="rptTimeline" runat="server">
                        <ItemTemplate>
                            <div class="relative flex flex-col items-center z-10 js-lifecycle-step" data-stage-index="<%# Container.ItemIndex %>">
                                <div class="w-8 h-8 rounded-full flex items-center justify-center ring-4 ring-surface js-lifecycle-dot" style='<%# Eval("DotClass") %>'>
                                    <span class="material-symbols-outlined text-[18px] js-lifecycle-icon" style='<%# Eval("IconColor") %>'><%# Eval("Icon") %></span>
                                </div>
                                <span class="mt-2 font-label-sm text-label-sm text-on-surface-variant text-center"><%# Eval("Label") %></span>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </section>

            <!-- Overall Completion (auto-rolled-up from tasks) -->
            <section class="standard-card rounded-xl p-6 mb-6">
                <div class="flex justify-between items-center mb-3">
                    <h3 class="font-title-lg text-title-lg text-primary">Overall Completion</h3>
                    <span id="eventCompletionPercent" class="font-label-lg text-label-lg font-bold text-on-surface-variant"><asp:Literal ID="litCompletionPercent" runat="server" Text="0%"></asp:Literal></span>
                </div>
                <div class="h-3 bg-surface-container-high rounded-full overflow-hidden">
                    <div id="completionBarFill" runat="server" class="h-full rounded-full" style="width: 0%; background-color: #2e7d32;"></div>
                </div>
                <p id="eventCompletionMeta" class="mt-2 text-xs text-on-surface-variant"><asp:Literal ID="litCompletionMeta" runat="server" Text="No tasks yet"></asp:Literal></p>
            </section>

            <!-- Main Layout: 2 Column Asymmetric -->
            <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">

                <!-- Left Column: Details & Related Data (Wide) -->
                <div class="lg:col-span-2 space-y-6">

                    <!-- Event Overview -->
                    <section class="standard-card rounded-xl overflow-hidden">
                        <div class="px-6 py-4 border-b border-surface-container-high">
                            <h3 class="font-title-lg text-title-lg text-primary">Event Overview</h3>
                        </div>
                        <div class="p-6">
                            <div class="mb-6">
                                <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-2">Description</span>
                                <asp:Literal ID="litDescription" runat="server" Text="No description available."></asp:Literal>
                            </div>
                            <div class="grid grid-cols-2 gap-6">
                                <div>
                                    <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-2">Start Date</span>
                                    <p class="font-body-md text-body-md flex items-center gap-2">
                                        <span class="material-symbols-outlined text-[18px] text-primary">event</span>
                                        <asp:Literal ID="litStartDate" runat="server"></asp:Literal>
                                    </p>
                                </div>
                                <div>
                                    <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-2">End Date</span>
                                    <p class="font-body-md text-body-md flex items-center gap-2">
                                        <span class="material-symbols-outlined text-[18px] text-primary">event</span>
                                        <asp:Literal ID="litEndDate" runat="server"></asp:Literal>
                                    </p>
                                </div>
                                <div>
                                    <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-2">Venue</span>
                                    <p class="font-body-md text-body-md flex items-center gap-2">
                                        <span class="material-symbols-outlined text-[18px] text-primary">location_on</span>
                                        <asp:Literal ID="litVenue" runat="server" Text="TBD"></asp:Literal>
                                    </p>
                                </div>
                                <div>
                                    <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-2">Budget</span>
                                    <p class="font-body-md text-body-md flex items-center gap-2">
                                        <span class="material-symbols-outlined text-[18px] text-primary">payments</span>
                                        <asp:Literal ID="litBudget" runat="server" Text="$0.00"></asp:Literal>
                                    </p>
                                </div>
                                <div>
                                    <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-2">Organizer</span>
                                    <p class="font-body-md text-body-md flex items-center gap-2">
                                        <span class="material-symbols-outlined text-[18px] text-primary">corporate_fare</span>
                                        <asp:Literal ID="litOrganizer" runat="server" Text="N/A"></asp:Literal>
                                    </p>
                                </div>
                                <div>
                                    <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-2">Created Date</span>
                                    <p class="font-body-md text-body-md flex items-center gap-2">
                                        <span class="material-symbols-outlined text-[18px] text-primary">schedule</span>
                                        <asp:Literal ID="litCreatedDate" runat="server"></asp:Literal>
                                    </p>
                                </div>
                            </div>
                        </div>
                    </section>

                    <!-- Assigned Tasks -->
                    <section class="standard-card rounded-xl overflow-hidden">
                        <div class="px-6 py-4 border-b border-surface-container-high flex justify-between items-center">
                            <h3 class="font-title-lg text-title-lg text-primary">Assigned Tasks</h3>
                            <span class="bg-surface-container-high px-2 py-1 rounded font-badge-cap text-badge-cap text-on-surface-variant">
                                <asp:Literal ID="litTaskCount" runat="server" Text="0"></asp:Literal> tasks
                            </span>
                        </div>
                        <div class="overflow-x-auto">
                            <table class="w-full text-left font-body-md">
                                <thead>
                                    <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                        <th class="px-6 py-3 font-semibold">Task Title</th>
                                        <th class="px-6 py-3 font-semibold">Member</th>
                                        <th class="px-6 py-3 font-semibold">Status</th>
                                        <th class="px-6 py-3 font-semibold">Due Date</th>
                                    </tr>
                                </thead>
                                <tbody class="divide-y divide-surface-container-high">
                                    <asp:Repeater ID="rptTasks" runat="server">
                                        <ItemTemplate>
                                            <tr class="hover:bg-surface-container-lowest transition-colors">
                                                <td class="px-6 py-4 font-semibold"><%# Eval("TaskTitle") %></td>
                                                <td class="px-6 py-4">
                                                    <p class="text-sm font-semibold text-on-surface"><%# Eval("MemberNames") %></p>
                                                    <p class="text-[11px] text-on-surface-variant"><%# Eval("MemberRole") %></p>
                                                </td>
                                                <td class="px-6 py-4"><%# Eval("StatusBadge") %></td>
                                                <td class="px-6 py-4 whitespace-nowrap"><%# Eval("DueDateFormatted") %></td>
                                            </tr>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </tbody>
                            </table>
                            <asp:Panel ID="pnlNoTasks" runat="server" CssClass="p-8 text-center" Visible="false">
                                <span class="material-symbols-outlined text-[40px] text-outline-variant block mb-2">assignment</span>
                                <p class="font-body-md text-body-md text-on-surface-variant">No tasks assigned to this event yet.</p>
                            </asp:Panel>
                        </div>
                    </section>
                </div>

                <!-- Right Column: Decisions & Meetings -->
                <div class="lg:col-span-1 space-y-6">

                    <!-- Decisions Made -->
                    <section class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-6">Decisions Made</h3>
                        <div class="space-y-4">
                            <asp:Repeater ID="rptDecisions" runat="server">
                                <ItemTemplate>
                                    <div class="p-4 bg-surface-container-low rounded-lg border border-outline-variant/30 flex items-start gap-4">
                                        <span class="material-symbols-outlined text-primary mt-0.5">gavel</span>
                                        <div>
                                            <p class="font-label-md text-label-md font-bold"><%# Eval("DecisionTitle") %></p>
                                            <p class="text-xs text-on-surface-variant mt-1"><%# Eval("StatusBadge") %></p>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            <asp:Panel ID="pnlNoDecisions" runat="server" CssClass="p-6 text-center" Visible="false">
                                <span class="material-symbols-outlined text-[40px] text-outline-variant block mb-2">gavel</span>
                                <p class="font-body-md text-body-md text-on-surface-variant">No decisions recorded for this event.</p>
                            </asp:Panel>
                        </div>
                    </section>

                    <!-- Linked Meetings -->
                    <section class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-6">Linked Meetings</h3>
                        <div class="space-y-4">
                            <asp:Repeater ID="rptLinkedMeetings" runat="server">
                                <ItemTemplate>
                                    <div class="p-4 bg-surface-container-low rounded-lg border border-outline-variant/30">
                                        <div class="flex items-center gap-3 mb-2">
                                            <span class="material-symbols-outlined text-primary">groups</span>
                                            <p class="font-label-md text-label-md font-bold"><%# Eval("MeetingTitle") %></p>
                                        </div>
                                        <p class="text-xs text-on-surface-variant flex items-center gap-1">
                                            <span class="material-symbols-outlined text-[14px]">event</span>
                                            <%# Eval("ScheduledDate", "{0:MMM dd, yyyy - hh:mm tt}") %>
                                        </p>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            <asp:Panel ID="pnlNoMeetings" runat="server" CssClass="p-6 text-center" Visible="false">
                                <span class="material-symbols-outlined text-[40px] text-outline-variant block mb-2">groups</span>
                                <p class="font-body-md text-body-md text-on-surface-variant">No meetings linked to this event.</p>
                            </asp:Panel>
                        </div>
                    </section>
                </div>
            </div>
        </main>
    </div>

    <!-- Dashboard Footer -->
    <footer class="dashboard-footer bg-on-secondary-fixed text-on-primary py-20 px-8 ml-64">
        <div class="grid grid-cols-1 md:grid-cols-4 gap-12 max-w-7xl mx-auto">
            <div class="md:col-span-1">
                <h2 class="font-headline-md text-headline-md font-bold mb-4"><a href="<%= ResolveUrl("~/Default.aspx") %>" class="hover:text-white">DTAS</a></h2>
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
            &copy; 2023 DTAS. All rights reserved. Encrypted and audited.
        </div>
    </footer>

</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript">
        (function () {
            var root = document.getElementById('eventLifecycleRoot');
            if (!root) return;
            var api = root.getAttribute('data-api');
            var eventId = root.getAttribute('data-event-id');
            if (!api || !eventId) return;
            var lastLifecycle = parseInt(root.getAttribute('data-lifecycle-index'), 10);
            if (isNaN(lastLifecycle)) lastLifecycle = -1;

            function applyLifecycle(index) {
                if (typeof index !== 'number' || index < 0) return;
                if (index === lastLifecycle) return;
                lastLifecycle = index;
                var fill = document.getElementById('<%= timelineFill.ClientID %>');
                var steps = document.querySelectorAll('.js-lifecycle-step');
                var max = Math.max(steps.length - 1, 1);
                if (fill) fill.style.width = Math.round((index * 100) / max) + '%';
                for (var i = 0; i < steps.length; i++) {
                    var on = i <= index;
                    var current = i === index;
                    var dot = steps[i].querySelector('.js-lifecycle-dot');
                    var icon = steps[i].querySelector('.js-lifecycle-icon');
                    if (dot) dot.style.backgroundColor = on ? '#0077b6' : '#e0e0e0';
                    if (icon) {
                        icon.style.color = on ? '#fff' : '#999';
                        icon.textContent = i < index ? 'check' : (current ? 'radio_button_checked' : 'circle');
                    }
                }
            }

            function apply(row) {
                var pct = row.percent || 0;
                var percentEl = document.getElementById('eventCompletionPercent');
                var metaEl = document.getElementById('eventCompletionMeta');
                var bar = document.getElementById('<%= completionBarFill.ClientID %>');
                if (percentEl) percentEl.textContent = pct + '%';
                if (metaEl) metaEl.textContent = row.label || 'No tasks yet';
                if (bar) {
                    bar.style.width = pct + '%';
                    bar.style.backgroundColor = pct > 0 ? '#2e7d32' : '#c5d0c8';
                }
                if (typeof row.lifecycleIndex === 'number') applyLifecycle(row.lifecycleIndex);
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
                        for (var i = 0; i < list.length; i++) {
                            if (String(list[i].eventId) === String(eventId)) {
                                apply(list[i]);
                                break;
                            }
                        }
                    } catch (err) { }
                };
                xhr.send();
            }
            setInterval(poll, 5000);
            document.addEventListener('visibilitychange', function () {
                if (document.visibilityState === 'visible') poll();
            });
        })();
    </script>
</asp:Content>
