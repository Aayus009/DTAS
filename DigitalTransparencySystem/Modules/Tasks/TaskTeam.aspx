<%@ Page Title="Manage Task Team | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="TaskTeam.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Tasks.TaskTeam" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <uc:AdminTopbar runat="server" />

    <div class="flex min-h-screen">

        <uc:AdminSidebar ActivePage="Tasks" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">

            <nav class="flex items-center gap-2 mb-6 font-label-md text-label-md text-on-surface-variant">
                <a href="<%= ResolveUrl("~/Modules/Dashboard/AdminDashboard.aspx") %>" class="hover:text-primary transition-colors">Dashboard</a>
                <span class="material-symbols-outlined text-[16px]">chevron_right</span>
                <a href="<%= ResolveUrl("~/Modules/Tasks/Tasks.aspx") %>" class="hover:text-primary transition-colors">Tasks</a>
                <span class="material-symbols-outlined text-[16px]">chevron_right</span>
                <span class="text-primary font-semibold">Manage Team</span>
            </nav>

            <header class="flex justify-between items-start mb-8">
                <div>
                    <div class="flex items-center gap-3 mb-2">
                        <span class="material-symbols-outlined text-[28px] text-primary">group</span>
                        <h1 class="font-headline-lg text-headline-lg text-primary">Manage Task Team</h1>
                    </div>
                    <p class="font-body-md text-body-lg text-on-surface-variant">
                        Task: <asp:Literal ID="litTaskTitle" runat="server"></asp:Literal>
                    </p>
                    <p class="font-body-sm text-body-sm text-on-surface-variant mt-1">
                        As the team leader, you can invite members and manage the team.
                    </p>
                </div>
                <div class="flex gap-3">
                    <asp:Button ID="btnBackToTask" runat="server" Text="Back to Task" CssClass="py-2 px-5 border border-outline text-on-surface-variant rounded-lg font-label-md text-label-md font-bold hover:bg-surface-container-low transition-colors cursor-pointer" OnClick="btnBackToTask_Click" />
                </div>
            </header>

            <asp:Panel ID="pnlError" runat="server" CssClass="mb-6 p-4 bg-error-container border border-error/30 rounded-xl flex items-center gap-3" Visible="false">
                <span class="material-symbols-outlined text-error">error</span>
                <asp:Label ID="lblError" runat="server" CssClass="font-label-md text-label-md text-on-error-container"></asp:Label>
            </asp:Panel>

            <asp:Panel ID="pnlSuccess" runat="server" CssClass="mb-6 p-4 bg-[rgba(46,125,50,0.1)] border border-[rgba(46,125,50,0.3)] rounded-xl flex items-center gap-3" Visible="false">
                <span class="material-symbols-outlined text-[#2e7d32]">check_circle</span>
                <asp:Label ID="lblSuccess" runat="server" CssClass="font-label-md text-label-md text-[#2e7d32]"></asp:Label>
            </asp:Panel>

            <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">

                <!-- Left: Add Member -->
                <div class="lg:col-span-1 space-y-6">

                    <section class="standard-card rounded-xl overflow-hidden">
                        <div class="px-6 py-4 border-b border-surface-container-high">
                            <h3 class="font-title-lg text-title-lg text-primary">Invite Member</h3>
                        </div>
                        <div class="p-6 space-y-4">
                            <div>
                                <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Search User</label>
                                <asp:TextBox ID="txtSearchUser" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="Search by name or department..." AutoPostBack="true" OnTextChanged="txtSearchUser_TextChanged"></asp:TextBox>
                            </div>
                            <div>
                                <asp:ListBox ID="lstAvailableUsers" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" Rows="8" SelectionMode="Single"></asp:ListBox>
                            </div>
                            <asp:Button ID="btnInviteMember" runat="server" Text="Invite Selected User" CssClass="w-full px-6 py-3 bg-primary text-on-primary rounded-xl font-label-md text-label-md font-bold active:scale-95 transition-transform cursor-pointer" OnClick="btnInviteMember_Click" />
                            <asp:Panel ID="pnlNoUsers" runat="server" Visible="false" CssClass="p-4 bg-surface-container-low rounded-lg text-center">
                                <span class="font-body-md text-body-md text-on-surface-variant">No available users found.</span>
                            </asp:Panel>
                        </div>
                    </section>

                    <!-- Task Progress -->
                    <section class="standard-card rounded-xl overflow-hidden">
                        <div class="px-6 py-4 border-b border-surface-container-high">
                            <h3 class="font-title-lg text-title-lg text-primary">Team Progress</h3>
                        </div>
                        <div class="p-6 space-y-4">
                            <div>
                                <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Overall Progress (%)</label>
                                <asp:TextBox ID="txtProgress" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" TextMode="Number" min="0" max="100" placeholder="0-100"></asp:TextBox>
                            </div>
                            <asp:Button ID="btnUpdateProgress" runat="server" Text="Update Progress" CssClass="w-full px-6 py-3 bg-[#2e7d32] text-white rounded-xl font-label-md text-label-md font-bold active:scale-95 transition-transform cursor-pointer" OnClick="btnUpdateProgress_Click" />
                            <asp:Panel ID="pnlProgressBar" runat="server" Visible="false" class="mt-2">
                                <div class="flex items-center gap-3 mb-2">
                                    <div class="flex-1 h-3 bg-surface-container-high rounded-full overflow-hidden">
                                        <asp:Literal ID="litProgressBar" runat="server"></asp:Literal>
                                    </div>
                                    <span class="font-label-md text-label-md text-on-surface-variant whitespace-nowrap"><asp:Literal ID="litProgressText" runat="server"></asp:Literal></span>
                                </div>
                            </asp:Panel>
                        </div>
                    </section>
                </div>

                <!-- Right: Team Members -->
                <div class="lg:col-span-2 space-y-6">

                    <section class="standard-card rounded-xl overflow-hidden">
                        <div class="px-6 py-4 border-b border-surface-container-high flex justify-between items-center">
                            <h3 class="font-title-lg text-title-lg text-primary">Team Members</h3>
                            <asp:Literal ID="litMemberCount" runat="server"></asp:Literal>
                        </div>
                        <div class="p-6">
                            <div class="space-y-3">
                                <asp:Repeater ID="rptTeamMembers" runat="server" OnItemCommand="rptTeamMembers_ItemCommand">
                                    <ItemTemplate>
                                        <div class="flex items-center gap-4 p-4 bg-surface-container-low rounded-lg border border-outline-variant/30 hover:bg-surface-container-high transition-colors">
                                            <div class="h-10 w-10 rounded-full bg-primary-container flex items-center justify-center shrink-0">
                                                <span class="material-symbols-outlined text-on-primary-container text-[20px]">person</span>
                                            </div>
                                            <div class="flex-1 min-w-0">
                                                <p class="font-label-md text-label-md font-bold"><%# Eval("FullName") %></p>
                                                <p class="text-xs text-on-surface-variant"><%# Eval("Role") %></p>
                                            </div>
                                            <div class="flex items-center gap-2">
                                                <span class="font-badge-cap text-badge-cap px-2.5 py-1 rounded-full" style='<%# Eval("StatusStyle") %>'><%# Eval("StatusText") %></span>
                                                <asp:LinkButton ID="btnRemove" runat="server" CommandName="RemoveMember" CommandArgument='<%# Eval("MemberID") %>'
                                                    CssClass="p-2 hover:bg-error-container/50 rounded-lg transition-colors" ToolTip="Remove from team"
                                                    OnClientClick="return dtasConfirm(this, 'Remove this member from the team?');">
                                                    <span class="material-symbols-outlined text-outline hover:text-error transition-colors text-[18px]">person_remove</span>
                                                </asp:LinkButton>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                            <asp:Panel ID="pnlNoMembers" runat="server" CssClass="p-8 text-center" Visible="false">
                                <span class="material-symbols-outlined text-[40px] text-outline-variant block mb-2">group_off</span>
                                <p class="font-body-md text-body-md text-on-surface-variant">No team members yet. Invite users to join this task team.</p>
                            </asp:Panel>
                        </div>
                    </section>

                    <!-- Team Member Updates -->
                    <section class="standard-card rounded-xl overflow-hidden">
                        <div class="px-6 py-4 border-b border-surface-container-high flex justify-between items-center">
                            <h3 class="font-title-lg text-title-lg text-primary">Member Updates</h3>
                        </div>
                        <div class="p-6">
                            <div class="space-y-4">
                                <asp:Repeater ID="rptMemberUpdates" runat="server">
                                    <ItemTemplate>
                                        <div class="p-4 bg-surface-container-low rounded-lg border border-outline-variant/30">
                                            <div class="flex items-center gap-3 mb-2">
                                                <div class="h-8 w-8 rounded-full bg-primary-container flex items-center justify-center shrink-0">
                                                    <span class="material-symbols-outlined text-on-primary-container text-[16px]">person</span>
                                                </div>
                                                <div>
                                                    <p class="font-label-md text-label-md font-bold"><%# Eval("UserName") %></p>
                                                    <p class="text-xs text-on-surface-variant"><%# Eval("UpdatedAt", "{0:MMM dd, yyyy - hh:mm tt}") %></p>
                                                </div>
                                            </div>
                                            <div class="flex items-center gap-2 mb-2 ml-11">
                                                <span class="font-badge-cap text-badge-cap px-2 py-0.5 rounded" style='<%# Eval("OldStatusStyle") %>'><%# Eval("OldStatus") %></span>
                                                <span class="material-symbols-outlined text-[14px] text-outline">arrow_forward</span>
                                                <span class="font-badge-cap text-badge-cap px-2 py-0.5 rounded" style='<%# Eval("NewStatusStyle") %>'><%# Eval("NewStatus") %></span>
                                            </div>
                                            <p class="text-sm text-on-surface-variant italic ml-11"><%# Eval("Comment") %></p>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                            <asp:Panel ID="pnlNoUpdates" runat="server" CssClass="p-8 text-center" Visible="false">
                                <span class="material-symbols-outlined text-[40px] text-outline-variant block mb-2">history</span>
                                <p class="font-body-md text-body-md text-on-surface-variant">No member updates yet.</p>
                            </asp:Panel>
                        </div>
                    </section>
                </div>
            </div>
        </main>
    </div>

</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="<%= ResolveUrl("~/Assets/js/dashboard.js") %>"></script>
</asp:Content>
