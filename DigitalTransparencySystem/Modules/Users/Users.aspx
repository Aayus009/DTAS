<%@ Page Title="User Management | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="Users.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Users.Users" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <uc:AdminTopbar runat="server" />

    <div class="flex min-h-screen">
        
        <uc:AdminSidebar ActivePage="Users" runat="server" />

        <!-- Main Content Area -->
        <main class="dashboard-main flex-1 ml-64 p-8">
            
            <!-- Header Section -->
            <header class="flex justify-between items-end mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">User Management</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">Manage system users, assign roles, and monitor account activity across the institution.</p>
                </div>
                <div class="flex gap-3">
                    <asp:Button ID="btnShowCreateUser" runat="server" Text="Create User" CssClass="py-2.5 px-6 bg-primary text-on-primary rounded-xl font-label-md text-label-md font-bold flex items-center gap-2 active:scale-95 transition-transform cursor-pointer" OnClick="btnShowCreateUser_Click" />
                </div>
            </header>

            <!-- Stats Cards -->
            <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-6">
                
                <!-- Total Users -->
                <div class="standard-card p-6 flex flex-col justify-between rounded-xl">
                    <div>
                        <span class="font-badge-cap text-badge-cap text-primary uppercase tracking-widest block mb-4">Total Users</span>
                        <asp:Literal ID="litTotalUsers" runat="server" Text="0"></asp:Literal>
                    </div>
                    <div class="mt-4 flex items-center gap-2">
                        <span class="material-symbols-outlined text-primary text-[18px]">people</span>
                        <span class="font-label-md text-label-md text-on-surface-variant">All accounts</span>
                    </div>
                </div>

                <!-- Admins -->
                <div class="standard-card p-6 flex flex-col justify-between rounded-xl">
                    <div>
                        <span class="font-badge-cap text-badge-cap text-on-secondary-fixed-variant uppercase tracking-widest block mb-4">Admins</span>
                        <asp:Literal ID="litAdmins" runat="server" Text="0"></asp:Literal>
                    </div>
                    <div class="mt-4 flex items-center gap-2">
                        <span class="material-symbols-outlined text-on-secondary-fixed-variant text-[18px]">admin_panel_settings</span>
                        <span class="font-label-md text-label-md text-on-surface-variant">System admins</span>
                    </div>
                </div>

                <!-- Faculty/Staff -->
                <div class="standard-card p-6 flex flex-col justify-between rounded-xl">
                    <div>
                        <span class="font-badge-cap text-badge-cap text-tertiary uppercase tracking-widest block mb-4">Faculty / Staff</span>
                        <asp:Literal ID="litFacultyStaff" runat="server" Text="0"></asp:Literal>
                    </div>
                    <div class="mt-4 flex items-center gap-2">
                        <span class="material-symbols-outlined text-on-tertiary-container text-[18px]">school</span>
                        <span class="font-label-md text-label-md text-on-surface-variant">Teachers & staff</span>
                    </div>
                </div>

                <!-- Students -->
                <div class="standard-card p-6 flex flex-col justify-between rounded-xl">
                    <div>
                        <span class="font-badge-cap text-badge-cap text-on-tertiary-container uppercase tracking-widest block mb-4">Students</span>
                        <asp:Literal ID="litStudents" runat="server" Text="0"></asp:Literal>
                    </div>
                    <div class="mt-4 flex items-center gap-2">
                        <span class="material-symbols-outlined text-on-tertiary-container text-[18px]">person</span>
                        <span class="font-label-md text-label-md text-on-surface-variant">Enrolled students</span>
                    </div>
                </div>
            </div>

            <!-- Filters & User Table -->
            <section class="standard-card rounded-xl overflow-hidden">
                <div class="px-6 py-4 border-b border-surface-container-high flex flex-wrap justify-between items-center gap-4">
                    <h3 class="font-title-lg text-title-lg text-primary">All Users</h3>
                    <div class="flex items-center gap-3">
                        <div class="relative">
                            <span class="absolute left-3 top-1/2 -translate-y-1/2 material-symbols-outlined text-outline text-[18px]">search</span>
                            <asp:TextBox ID="txtSearch" runat="server" CssClass="pl-9 pr-4 py-2 bg-surface-container-low border border-outline rounded-full font-body-md text-body-md w-56 focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="Search name or email..." />
                        </div>
                        <asp:DropDownList ID="ddlRoleFilter" runat="server" CssClass="py-2 px-4 bg-surface-container-low border border-outline rounded-full font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" AutoPostBack="true" OnSelectedIndexChanged="ddlRoleFilter_SelectedIndexChanged">
                            <asp:ListItem Text="All Roles" Value="0" />
                            <asp:ListItem Text="Admin" Value="1" />
                            <asp:ListItem Text="Faculty" Value="2" />
                            <asp:ListItem Text="Student" Value="3" />
                            <asp:ListItem Text="Staff" Value="4" />
                        </asp:DropDownList>
                        <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="py-2 px-4 bg-surface-container-low border border-outline rounded-full font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" AutoPostBack="true" OnSelectedIndexChanged="ddlStatusFilter_SelectedIndexChanged">
                            <asp:ListItem Text="All statuses" Value="All" />
                            <asp:ListItem Text="Active" Value="Active" />
                            <asp:ListItem Text="Suspended" Value="Suspended" />
                            <asp:ListItem Text="Banned" Value="Banned" />
                            <asp:ListItem Text="Inactive" Value="Inactive" />
                        </asp:DropDownList>
                        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="py-2 px-5 bg-surface-variant text-on-surface rounded-full font-label-md text-label-md font-bold hover:bg-surface-variant/80 transition-colors cursor-pointer" OnClick="btnSearch_Click" />
                    </div>
                </div>
                <asp:Label ID="lblModeration" runat="server" CssClass="font-label-md block px-6 pt-4"></asp:Label>
                <asp:HiddenField ID="hfTargetUserId" runat="server" />
                <asp:Panel ID="pnlModerationForm" runat="server" Visible="false" CssClass="mx-6 mb-4 p-4 rounded-xl border border-outline-variant bg-surface-container-low">
                    <p class="font-title-lg text-title-lg text-on-surface mb-2"><asp:Literal ID="litModerationTitle" runat="server"></asp:Literal></p>
                    <p class="text-sm text-on-surface-variant mb-3">User: <asp:Literal ID="litModerationName" runat="server"></asp:Literal></p>
                    <asp:Panel ID="pnlSuspendDays" runat="server" CssClass="mb-3">
                        <label class="block text-sm font-semibold text-on-surface-variant mb-1">Days (1-365)</label>
                        <asp:TextBox ID="txtSuspendDays" runat="server" Text="7" CssClass="w-32 px-3 py-2 bg-surface border border-outline rounded-lg"></asp:TextBox>
                    </asp:Panel>
                    <label class="block text-sm font-semibold text-on-surface-variant mb-1">Reason</label>
                    <asp:TextBox ID="txtModerationReason" runat="server" TextMode="MultiLine" Rows="3" CssClass="w-full px-3 py-2 bg-surface border border-outline rounded-lg mb-3" placeholder="Required reason"></asp:TextBox>
                    <div class="flex gap-3">
                        <asp:Button ID="btnConfirmSuspend" runat="server" Text="Confirm suspension" CssClass="px-4 py-2 bg-primary text-on-primary rounded-lg font-bold" OnClick="btnConfirmSuspend_Click" />
                        <asp:Button ID="btnConfirmBan" runat="server" Text="Confirm ban" CssClass="px-4 py-2 bg-error text-on-error rounded-lg font-bold" OnClick="btnConfirmBan_Click" />
                        <asp:Button ID="btnCancelModeration" runat="server" Text="Cancel" CssClass="px-4 py-2 border border-outline rounded-lg" OnClick="btnCancelModeration_Click" CausesValidation="false" />
                    </div>
                </asp:Panel>
                <div class="overflow-x-auto">
                    <table class="w-full text-left font-body-md">
                        <thead>
                            <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                <th class="px-6 py-3 font-semibold">Full Name</th>
                                <th class="px-6 py-3 font-semibold">Email</th>
                                <th class="px-6 py-3 font-semibold">Username</th>
                                <th class="px-6 py-3 font-semibold">Role</th>
                                <th class="px-6 py-3 font-semibold">Department</th>
                                <th class="px-6 py-3 font-semibold">Status</th>
                                <th class="px-6 py-3 font-semibold">Joined</th>
                                <th class="px-6 py-3 font-semibold text-right">Actions</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-surface-container-high">
                            <asp:Repeater ID="rptUsers" runat="server" OnItemCommand="rptUsers_ItemCommand">
                                <ItemTemplate>
                                    <tr class="hover:bg-surface-container-lowest transition-colors group">
                                        <td class="px-6 py-4 font-semibold"><%# Eval("FullName") %></td>
                                        <td class="px-6 py-4 text-on-surface-variant"><%# Eval("Email") %></td>
                                        <td class="px-6 py-4 text-on-surface-variant"><%# Eval("Username") %></td>
                                        <td class="px-6 py-4">
                                            <span class='<%# GetRoleBadgeCss(Eval("RoleName").ToString()) %>'><%# Eval("RoleName") %></span>
                                        </td>
                                        <td class="px-6 py-4 text-on-surface-variant"><%# Eval("DepartmentName") ?? "N/A" %></td>
                                        <td class="px-6 py-4">
                                            <span class='<%# GetAccountStatusCss(Eval("AccountStatus"), Eval("IsActive")) %>'>
                                                <%# FormatUserStatus(Eval("AccountStatus"), Eval("IsActive"), Eval("SuspensionEnd")) %>
                                            </span>
                                        </td>
                                        <td class="px-6 py-4 whitespace-nowrap text-on-surface-variant"><%# Eval("CreatedAt", "{0:MMM dd, yyyy}") %></td>
                                        <td class="px-6 py-4 text-right whitespace-nowrap">
                                            <a href='<%# "UserActivity.aspx?UserID=" + Eval("UserID") %>' class="material-symbols-outlined text-outline hover:text-primary transition-colors" title="View Activity">visibility</a>
                                            <asp:LinkButton ID="btnToggleStatus" runat="server" Visible='<%# ShowToggle(Eval("UserID"), Eval("RoleID"), Eval("AccountStatus")) %>'
                                                CommandName="ToggleStatus" CommandArgument='<%# Eval("UserID") %>'
                                                CssClass="material-symbols-outlined text-outline hover:text-primary transition-colors ml-2"
                                                title='<%# Convert.ToBoolean(Eval("IsActive")) ? "Deactivate" : "Activate" %>'>
                                                <%# Convert.ToBoolean(Eval("IsActive")) ? "person_off" : "person_check" %>
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnSuspend" runat="server" Visible='<%# ShowSuspend(Eval("UserID"), Eval("RoleID"), Eval("AccountStatus")) %>'
                                                CommandName="BeginSuspend" CommandArgument='<%# Eval("UserID") %>'
                                                CssClass="ml-2 px-2 py-1 text-xs font-bold text-on-surface-variant hover:text-primary">Suspend</asp:LinkButton>
                                            <asp:LinkButton ID="btnUnsuspend" runat="server" Visible='<%# ShowUnsuspend(Eval("AccountStatus")) %>'
                                                CommandName="Unsuspend" CommandArgument='<%# Eval("UserID") %>'
                                                CssClass="ml-2 px-2 py-1 text-xs font-bold text-tertiary">Unsuspend</asp:LinkButton>
                                            <asp:LinkButton ID="btnBan" runat="server" Visible='<%# ShowBan(Eval("UserID"), Eval("RoleID"), Eval("AccountStatus")) %>'
                                                CommandName="BeginBan" CommandArgument='<%# Eval("UserID") %>'
                                                CssClass="ml-2 px-2 py-1 text-xs font-bold text-error">Ban</asp:LinkButton>
                                            <asp:LinkButton ID="btnUnban" runat="server" Visible='<%# ShowUnban(Eval("AccountStatus")) %>'
                                                CommandName="Unban" CommandArgument='<%# Eval("UserID") %>'
                                                CssClass="ml-2 px-2 py-1 text-xs font-bold text-tertiary">Unban</asp:LinkButton>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>
            </section>

            <!-- Create User Panel -->
            <asp:Panel ID="pnlCreateUser" runat="server" CssClass="mt-6" Visible="false">
                <section class="standard-card rounded-xl overflow-hidden">
                    <div class="px-6 py-4 border-b border-surface-container-high flex justify-between items-center">
                        <h3 class="font-title-lg text-title-lg text-primary">Create New User</h3>
                        <asp:Button ID="btnClosePanel" runat="server" Text="" CssClass="material-symbols-outlined text-outline hover:text-error transition-colors cursor-pointer" OnClick="btnClosePanel_Click" />
                    </div>
                    <div class="p-6">
                        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="mb-4 p-4 rounded-lg">
                            <asp:Label ID="lblMessage" runat="server" CssClass="font-body-md text-body-md"></asp:Label>
                        </asp:Panel>
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div>
                                <label class="font-label-md text-label-md text-on-surface-variant block mb-2">Full Name *</label>
                                <asp:TextBox ID="txtFullName" runat="server" CssClass="w-full px-4 py-2.5 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="e.g. Juan Dela Cruz" />
                                <asp:RequiredFieldValidator ID="rfvFullName" runat="server" ControlToValidate="txtFullName" ErrorMessage="Full name is required." CssClass="text-error text-xs mt-1 block" Display="Dynamic" ValidationGroup="CreateUser" />
                            </div>
                            <div>
                                <label class="font-label-md text-label-md text-on-surface-variant block mb-2">Email *</label>
                                <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" CssClass="w-full px-4 py-2.5 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="user@institution.edu" />
                                <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" ErrorMessage="Email is required." CssClass="text-error text-xs mt-1 block" Display="Dynamic" ValidationGroup="CreateUser" />
                            </div>
                            <div>
                                <label class="font-label-md text-label-md text-on-surface-variant block mb-2">Role *</label>
                                <asp:DropDownList ID="ddlCreateRole" runat="server" CssClass="w-full px-4 py-2.5 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all">
                                    <asp:ListItem Text="Select Role" Value="0" />
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator ID="rfvRole" runat="server" ControlToValidate="ddlCreateRole" InitialValue="0" ErrorMessage="Please select a role." CssClass="text-error text-xs mt-1 block" Display="Dynamic" ValidationGroup="CreateUser" />
                            </div>
                            <div>
                                <label class="font-label-md text-label-md text-on-surface-variant block mb-2">Department</label>
                                <asp:DropDownList ID="ddlDepartment" runat="server" CssClass="w-full px-4 py-2.5 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all">
                                    <asp:ListItem Text="Select Department" Value="0" />
                                </asp:DropDownList>
                            </div>
                            <div class="md:col-span-2">
                                <label class="font-label-md text-label-md text-on-surface-variant block mb-2">Password *</label>
                                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="w-full px-4 py-2.5 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="Minimum 6 characters" />
                                <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword" ErrorMessage="Password is required." CssClass="text-error text-xs mt-1 block" Display="Dynamic" ValidationGroup="CreateUser" />
                            </div>
                        </div>
                        <div class="flex gap-3 mt-6">
                            <asp:Button ID="btnCreateUser" runat="server" Text="Create User" CssClass="py-2.5 px-6 bg-primary text-on-primary rounded-xl font-label-md text-label-md font-bold active:scale-95 transition-transform cursor-pointer" OnClick="btnCreateUser_Click" ValidationGroup="CreateUser" />
                            <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="py-2.5 px-6 border border-outline text-on-surface-variant rounded-xl font-label-md text-label-md font-bold hover:bg-surface-variant/50 transition-colors cursor-pointer" OnClick="btnCancel_Click" CausesValidation="false" />
                        </div>
                    </div>
                </section>
            </asp:Panel>

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
            © 2023 DTAS. All rights reserved. Encrypted and audited.
        </div>
    </footer>

</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="<%= ResolveUrl("~/Assets/js/dashboard.js") %>"></script>
</asp:Content>
