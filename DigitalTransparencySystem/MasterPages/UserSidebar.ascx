<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserSidebar.ascx.cs" Inherits="DigitalTransparencySystem.MasterPages.UserSidebar" %>

<aside class="dashboard-sidebar h-screen w-64 fixed left-0 top-0 overflow-y-auto bg-surface-container border-r border-outline-variant/30 flex flex-col p-4 gap-2 pt-24 z-40">
    <div class="px-4 mb-6">
        <p class="font-label-md text-label-md text-on-surface-variant">Community Portal</p>
    </div>
    <nav class="flex flex-col gap-1">
        <asp:HyperLink ID="lnkDashboard" runat="server" NavigateUrl="~/Modules/Dashboard/UsersDashboard.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">dashboard</span>
            <span class="font-label-md text-label-md">Dashboard</span>
        </asp:HyperLink>
        <asp:HyperLink ID="lnkIdentity" runat="server" NavigateUrl="~/Modules/Settings/IdentityVerification.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">badge</span>
            <span class="font-label-md text-label-md">Verify ID</span>
        </asp:HyperLink>

        <asp:HyperLink ID="lnkTaskWorkspaces" runat="server" NavigateUrl="~/Modules/TaskWorkspaces/TaskWorkspaces.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">workspaces</span>
            <span class="font-label-md text-label-md">Task Workspaces</span>
        </asp:HyperLink>
        <asp:HyperLink ID="lnkAssignments" runat="server" NavigateUrl="~/Modules/Assignments/MyAssignments.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">school</span>
            <span class="font-label-md text-label-md">Assignments</span>
            <asp:Label ID="lblAssignmentCount" runat="server" Visible="false"
                CssClass="js-unread-badge ml-auto min-w-[20px] h-5 px-1.5 bg-error text-on-error text-[11px] font-bold rounded-full flex items-center justify-center"></asp:Label>
        </asp:HyperLink>
        <asp:HyperLink ID="lnkConnect" runat="server" NavigateUrl="~/Modules/Connect/Connect.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">forum</span>
            <span class="font-label-md text-label-md">Connect</span>
        </asp:HyperLink>

        <asp:HyperLink ID="lnkMyEvents" runat="server" NavigateUrl="~/Modules/Events/MyEvents.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">event</span>
            <span class="font-label-md text-label-md">Events</span>
        </asp:HyperLink>
        <asp:Panel ID="pnlEventActions" runat="server" CssClass="sidebar-submenu">
            <asp:HyperLink ID="lnkCampusEvents" runat="server" NavigateUrl="~/Modules/Events/CampusEvents.aspx"
                CssClass="sidebar-sublink group cursor-pointer flex items-center gap-3 px-4 py-2 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all">
                <span class="material-symbols-outlined text-[18px]">campaign</span>
                <span class="font-label-md text-label-md">Campus Events</span>
            </asp:HyperLink>
            <asp:HyperLink ID="lnkCreateEvent" runat="server" NavigateUrl="~/Modules/Events/CreateEvent.aspx" Visible="false"
                CssClass="sidebar-sublink group cursor-pointer flex items-center gap-3 px-4 py-2 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all">
                <span class="material-symbols-outlined text-[18px]">add_circle</span>
                <span class="font-label-md text-label-md">Create Event</span>
            </asp:HyperLink>
            <asp:HyperLink ID="lnkProposeEvent" runat="server" NavigateUrl="~/Modules/Events/ProposeEvent.aspx"
                CssClass="sidebar-sublink group cursor-pointer flex items-center gap-3 px-4 py-2 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all">
                <span class="material-symbols-outlined text-[18px]">add_circle</span>
                <span class="font-label-md text-label-md">Propose Event</span>
            </asp:HyperLink>
        </asp:Panel>

        <asp:HyperLink ID="lnkClubs" runat="server" NavigateUrl="~/Modules/Clubs/MyClubs.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">groups</span>
            <span class="font-label-md text-label-md">Clubs</span>
        </asp:HyperLink>
        <asp:HyperLink ID="lnkMyMeetings" runat="server" NavigateUrl="~/Modules/UserMeetings/UserMeetings.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">groups</span>
            <span class="font-label-md text-label-md">My Meetings</span>
        </asp:HyperLink>
        <asp:HyperLink ID="lnkPolls" runat="server" NavigateUrl="~/Modules/UserPolls/UserPolls.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">how_to_vote</span>
            <span class="font-label-md text-label-md">Polls</span>
        </asp:HyperLink>

        <asp:HyperLink ID="lnkFeedback" runat="server" NavigateUrl="~/Modules/UserFeedback/UserFeedback.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">feedback</span>
            <span class="font-label-md text-label-md">Feedback</span>
        </asp:HyperLink>
        <asp:HyperLink ID="lnkFlag" runat="server" NavigateUrl="~/Modules/Settings/SubmitReport.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">flag</span>
            <span class="font-label-md text-label-md">Flag content</span>
        </asp:HyperLink>

        <asp:HyperLink ID="lnkReports" runat="server" NavigateUrl="~/Modules/UserReports/UserReports.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">analytics</span>
            <span class="font-label-md text-label-md">Reports</span>
        </asp:HyperLink>
        <asp:HyperLink ID="lnkTransparency" runat="server" NavigateUrl="~/Transparency.aspx"
            CssClass="group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1">
            <span class="material-symbols-outlined">visibility</span>
            <span class="font-label-md text-label-md">Transparency</span>
        </asp:HyperLink>
    </nav>
</aside>
