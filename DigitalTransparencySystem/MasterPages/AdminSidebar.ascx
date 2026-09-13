<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AdminSidebar.ascx.cs" Inherits="DigitalTransparencySystem.MasterPages.AdminSidebar" %>

<aside class="dashboard-sidebar h-screen w-64 fixed left-0 top-0 overflow-y-auto bg-surface-container border-r border-outline-variant/30 flex flex-col p-4 gap-2 pt-24 z-40">
    <div class="flex items-center gap-2 px-2 mb-6">
        <button type="button" id="btnSidebarCollapse" class="sidebar-collapse-btn material-symbols-outlined shrink-0" aria-label="Collapse sidebar" title="Collapse/expand sidebar">menu</button>
        <p class="sidebar-heading sidebar-portal-title">Admin Portal</p>
    </div>
    <nav class="flex flex-col gap-1">
        <asp:HyperLink ID="lnkDashboard" runat="server" NavigateUrl="~/Modules/Dashboard/AdminDashboard.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">dashboard</span>
            <span class="font-label-md text-label-md">Dashboard</span>
        </asp:HyperLink>
        <asp:HyperLink ID="lnkEvents" runat="server" NavigateUrl="~/Modules/Events/Events.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">calendar_today</span>
            <span class="font-label-md text-label-md">Events</span>
            <asp:Label ID="lblEventsCount" runat="server" Visible="false"
                CssClass="js-unread-badge ml-auto min-w-[20px] h-5 px-1.5 bg-error text-on-error text-[11px] font-bold rounded-full flex items-center justify-center"></asp:Label>
        </asp:HyperLink>
        <asp:HyperLink ID="lnkDecisions" runat="server" NavigateUrl="~/Modules/Decisions/Decisions.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">gavel</span>
            <span class="font-label-md text-label-md">Decisions</span>
            <asp:Label ID="lblDecisionsCount" runat="server" Visible="false"
                CssClass="js-unread-badge ml-auto min-w-[20px] h-5 px-1.5 bg-error text-on-error text-[11px] font-bold rounded-full flex items-center justify-center"></asp:Label>
        </asp:HyperLink>
        <asp:HyperLink ID="lnkTasks" runat="server" NavigateUrl="~/Modules/Tasks/Tasks.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">assignment</span>
            <span class="font-label-md text-label-md">Tasks</span>
            <asp:Label ID="lblTasksCount" runat="server" Visible="false"
                CssClass="js-unread-badge ml-auto min-w-[20px] h-5 px-1.5 bg-error text-on-error text-[11px] font-bold rounded-full flex items-center justify-center"></asp:Label>
        </asp:HyperLink>
        <asp:HyperLink ID="lnkAssignments" runat="server" Visible="false" NavigateUrl="~/Modules/Assignments/Assignments.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">school</span>
            <span class="font-label-md text-label-md">Assignments</span>
            <asp:Label ID="lblAssignmentsCount" runat="server" Visible="false"
                CssClass="js-unread-badge ml-auto min-w-[20px] h-5 px-1.5 bg-error text-on-error text-[11px] font-bold rounded-full flex items-center justify-center"></asp:Label>
        </asp:HyperLink>
        <asp:HyperLink ID="lnkClubs" runat="server" NavigateUrl="~/Modules/Clubs/Clubs.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">groups</span>
            <span class="font-label-md text-label-md">Clubs</span>
            <asp:Label ID="lblClubsCount" runat="server" Visible="false"
                CssClass="js-unread-badge ml-auto min-w-[20px] h-5 px-1.5 bg-error text-on-error text-[11px] font-bold rounded-full flex items-center justify-center"></asp:Label>
        </asp:HyperLink>
        <asp:HyperLink ID="lnkPolls" runat="server" NavigateUrl="~/Modules/Polls/Polls.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">how_to_vote</span>
            <span class="font-label-md text-label-md">Polls</span>
            <asp:Label ID="lblPollsCount" runat="server" Visible="false"
                CssClass="js-unread-badge ml-auto min-w-[20px] h-5 px-1.5 bg-error text-on-error text-[11px] font-bold rounded-full flex items-center justify-center"></asp:Label>
        </asp:HyperLink>
        <asp:HyperLink ID="lnkReports" runat="server" NavigateUrl="~/Modules/Reports/Reports.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">analytics</span>
            <span class="font-label-md text-label-md">Reports</span>
        </asp:HyperLink>
        <asp:HyperLink ID="lnkTransparency" runat="server" NavigateUrl="~/Modules/Transparency/Transparency.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">visibility</span>
            <span class="font-label-md text-label-md">Transparency</span>
        </asp:HyperLink>
        <asp:HyperLink ID="lnkUsers" runat="server" NavigateUrl="~/Modules/Users/Users.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">manage_accounts</span>
            <span class="font-label-md text-label-md">Users</span>
            <asp:Label ID="lblUsersCount" runat="server" Visible="false"
                CssClass="js-unread-badge ml-auto min-w-[20px] h-5 px-1.5 bg-error text-on-error text-[11px] font-bold rounded-full flex items-center justify-center"></asp:Label>
        </asp:HyperLink>
        <asp:HyperLink ID="lnkIdentity" runat="server" NavigateUrl="~/Modules/Users/IdentityQueue.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">badge</span>
            <span class="font-label-md text-label-md">ID Queue</span>
            <asp:Label ID="lblIdentityCount" runat="server" Visible="false"
                CssClass="js-unread-badge ml-auto min-w-[20px] h-5 px-1.5 bg-error text-on-error text-[11px] font-bold rounded-full flex items-center justify-center"></asp:Label>
        </asp:HyperLink>
        <asp:HyperLink ID="lnkFlags" runat="server" NavigateUrl="~/Modules/Users/Flags.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">flag</span>
            <span class="font-label-md text-label-md">Flags</span>
            <asp:Label ID="lblFlagsCount" runat="server" Visible="false"
                CssClass="js-unread-badge ml-auto min-w-[20px] h-5 px-1.5 bg-error text-on-error text-[11px] font-bold rounded-full flex items-center justify-center"></asp:Label>
        </asp:HyperLink>
        <asp:HyperLink ID="lnkFeedback" runat="server" NavigateUrl="~/Modules/Participation/Feedback.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">feedback</span>
            <span class="font-label-md text-label-md">Feedback</span>
            <asp:Label ID="lblFeedbackCount" runat="server" Visible="false"
                CssClass="js-unread-badge ml-auto min-w-[20px] h-5 px-1.5 bg-error text-on-error text-[11px] font-bold rounded-full flex items-center justify-center"></asp:Label>
        </asp:HyperLink>
        <asp:HyperLink ID="lnkNotifications" runat="server" NavigateUrl="~/Modules/Notifications/Notifications.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">notifications</span>
            <span class="font-label-md text-label-md">Notifications</span>
            <asp:Label ID="lblNotifCount" runat="server" Visible="false"
                CssClass="js-unread-badge ml-auto min-w-[20px] h-5 px-1.5 bg-error text-on-error text-[11px] font-bold rounded-full flex items-center justify-center"></asp:Label>
        </asp:HyperLink>
    </nav>
    <div class="mt-auto pt-4">
        <asp:Button ID="btnCreateTask" runat="server" Visible="false" Text="&#43; Create New Task"
            CssClass="w-full py-3 px-4 bg-primary text-on-primary rounded-xl font-label-md text-label-md font-bold flex items-center justify-center gap-2 active:scale-95 transition-transform cursor-pointer"
            OnClick="btnCreateTask_Click" />
    </div>
</aside>

<script type="text/javascript">
    (function () {
        function initCollapse() {
            try {
                if (localStorage.getItem('sidebar-collapsed') === 'true') {
                    document.body.classList.add('sidebar-collapsed');
                }
            } catch (e) { }

            var btn = document.querySelector('#btnSidebarCollapse');
            if (btn) {
                btn.addEventListener('click', function (e) {
                    e.stopPropagation();
                    document.body.classList.toggle('sidebar-collapsed');
                    try {
                        localStorage.setItem('sidebar-collapsed', document.body.classList.contains('sidebar-collapsed'));
                    } catch (err) { }
                });
            }
        }
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', initCollapse);
        } else {
            initCollapse();
        }
    })();
</script>
