<%@ Page Title="User Activity | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="UserActivity.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Users.UserActivity" %>
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
                    <a href="Users.aspx" class="font-label-md text-label-md text-primary hover:underline flex items-center gap-1 mb-2">
                        <span class="material-symbols-outlined text-[16px]">arrow_back</span> Back to Users
                    </a>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">User Activity</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">Detailed activity overview for <asp:Literal ID="litUserName" runat="server"></asp:Literal>.</p>
                </div>
            </header>

            <!-- User Profile Card -->
            <div class="standard-card rounded-xl p-6 mb-6">
                <div class="flex items-start gap-6">
                    <div class="h-20 w-20 rounded-full bg-primary-container flex items-center justify-center border-2 border-primary/20 shrink-0">
                        <span class="material-symbols-outlined text-primary text-[36px]">person</span>
                    </div>
                    <div class="flex-1">
                        <div class="flex items-center gap-3 mb-1">
                            <h2 class="font-headline-md text-headline-md text-primary"><asp:Literal ID="litProfileName" runat="server"></asp:Literal></h2>
                            <span id="lblProfileRole" runat="server" class="px-2 py-0.5 rounded font-badge-cap text-badge-cap"></span>
                        </div>
                        <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mt-4">
                            <div class="flex items-center gap-2 text-on-surface-variant">
                                <span class="material-symbols-outlined text-[18px]">email</span>
                                <span class="font-body-md text-body-md"><asp:Literal ID="litProfileEmail" runat="server"></asp:Literal></span>
                            </div>
                            <div class="flex items-center gap-2 text-on-surface-variant">
                                <span class="material-symbols-outlined text-[18px]">badge</span>
                                <span class="font-body-md text-body-md"><asp:Literal ID="litProfileUsername" runat="server"></asp:Literal></span>
                            </div>
                            <div class="flex items-center gap-2 text-on-surface-variant">
                                <span class="material-symbols-outlined text-[18px]">apartment</span>
                                <span class="font-body-md text-body-md"><asp:Literal ID="litProfileDepartment" runat="server"></asp:Literal></span>
                            </div>
                            <div class="flex items-center gap-2 text-on-surface-variant">
                                <span class="material-symbols-outlined text-[18px]">calendar_today</span>
                                <span class="font-body-md text-body-md">Joined <asp:Literal ID="litProfileJoined" runat="server"></asp:Literal></span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Activity Summary Stats -->
            <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-6">
                <div class="standard-card p-6 rounded-xl">
                    <span class="font-badge-cap text-badge-cap text-primary uppercase tracking-widest block mb-4">Tasks Assigned</span>
                    <asp:Literal ID="litTasksAssigned" runat="server" Text="0"></asp:Literal>
                </div>
                <div class="standard-card p-6 rounded-xl">
                    <span class="font-badge-cap text-badge-cap text-on-tertiary-container uppercase tracking-widest block mb-4">Tasks Completed</span>
                    <asp:Literal ID="litTasksCompleted" runat="server" Text="0"></asp:Literal>
                </div>
                <div class="standard-card p-6 rounded-xl">
                    <span class="font-badge-cap text-badge-cap text-on-secondary-fixed-variant uppercase tracking-widest block mb-4">Decisions Involved</span>
                    <asp:Literal ID="litDecisionsInvolved" runat="server" Text="0"></asp:Literal>
                </div>
                <div class="standard-card p-6 rounded-xl">
                    <span class="font-badge-cap text-badge-cap text-tertiary uppercase tracking-widest block mb-4">Meetings Attended</span>
                    <asp:Literal ID="litMeetingsAttended" runat="server" Text="0"></asp:Literal>
                </div>
            </div>

            <!-- Two Column Layout -->
            <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
                
                <!-- Recent Tasks -->
                <section class="standard-card rounded-xl overflow-hidden">
                    <div class="px-6 py-4 border-b border-surface-container-high">
                        <h3 class="font-title-lg text-title-lg text-primary">Recent Tasks</h3>
                    </div>
                    <div class="divide-y divide-surface-container-high max-h-[400px] overflow-y-auto">
                        <asp:Repeater ID="rptUserTasks" runat="server">
                            <ItemTemplate>
                                <div class="px-6 py-4 hover:bg-surface-container-lowest transition-colors">
                                    <div class="flex items-start gap-4">
                                        <span class="material-symbols-outlined text-primary mt-0.5"><%# Eval("Status").ToString() == "Completed" ? "check_circle" : Eval("Status").ToString() == "InProgress" ? "pending" : "radio_button_unchecked" %></span>
                                        <div class="flex-1">
                                            <p class="font-label-md text-label-md font-bold"><%# Eval("TaskTitle") %></p>
                                            <div class="flex items-center gap-3 mt-1">
                                                <span class='<%# GetStatusBadgeCss(Eval("Status").ToString()) %>'><%# Eval("Status") %></span>
                                                <span class="text-xs text-on-surface-variant">Due: <%# Eval("DueDate", "{0:MMM dd, yyyy}") %></span>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </ItemTemplate>
                            <FooterTemplate>
                                <asp:Panel ID="pnlNoTasks" runat="server" CssClass="px-6 py-8 text-center" Visible='<%# rptUserTasks.Items.Count == 0 %>'>
                                    <span class="material-symbols-outlined text-outline text-[36px] mb-2">assignment</span>
                                    <p class="font-body-md text-body-md text-on-surface-variant">No tasks assigned yet.</p>
                                </asp:Panel>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                </section>

                <!-- Recent Decisions -->
                <section class="standard-card rounded-xl overflow-hidden">
                    <div class="px-6 py-4 border-b border-surface-container-high">
                        <h3 class="font-title-lg text-title-lg text-primary">Recent Decisions</h3>
                    </div>
                    <div class="divide-y divide-surface-container-high max-h-[400px] overflow-y-auto">
                        <asp:Repeater ID="rptUserDecisions" runat="server">
                            <ItemTemplate>
                                <div class="px-6 py-4 hover:bg-surface-container-lowest transition-colors">
                                    <div class="flex items-start gap-4">
                                        <span class="material-symbols-outlined text-on-secondary-fixed-variant mt-0.5">gavel</span>
                                        <div class="flex-1">
                                            <p class="font-label-md text-label-md font-bold"><%# Eval("DecisionTitle") %></p>
                                            <div class="flex items-center gap-3 mt-1">
                                                <span class="font-body-md text-sm text-on-surface-variant"><%# Eval("Description") %></span>
                                            </div>
                                            <span class="text-xs text-on-surface-variant mt-1 block"><%# Eval("DecisionDate", "{0:MMM dd, yyyy}") %></span>
                                        </div>
                                    </div>
                                </div>
                            </ItemTemplate>
                            <FooterTemplate>
                                <asp:Panel ID="pnlNoDecisions" runat="server" CssClass="px-6 py-8 text-center" Visible='<%# rptUserDecisions.Items.Count == 0 %>'>
                                    <span class="material-symbols-outlined text-outline text-[36px] mb-2">gavel</span>
                                    <p class="font-body-md text-body-md text-on-surface-variant">No decisions involvement yet.</p>
                                </asp:Panel>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                </section>
            </div>

            <!-- Login History -->
            <section class="standard-card rounded-xl overflow-hidden mt-6">
                <div class="px-6 py-4 border-b border-surface-container-high">
                    <h3 class="font-title-lg text-title-lg text-primary">Login History</h3>
                </div>
                <div class="overflow-x-auto">
                    <table class="w-full text-left font-body-md">
                        <thead>
                            <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                <th class="px-6 py-3 font-semibold">Date & Time</th>
                                <th class="px-6 py-3 font-semibold">IP Address</th>
                                <th class="px-6 py-3 font-semibold">Status</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-surface-container-high">
                            <asp:Repeater ID="rptLoginHistory" runat="server">
                                <ItemTemplate>
                                    <tr class="hover:bg-surface-container-lowest transition-colors">
                                        <td class="px-6 py-4 whitespace-nowrap"><%# FormatLoginTime(Eval("LoginDate")) %></td>
                                        <td class="px-6 py-4 text-on-surface-variant"><%# string.IsNullOrEmpty(Convert.ToString(Eval("IPAddress"))) ? "N/A" : Eval("IPAddress") %></td>
                                        <td class="px-6 py-4">
                                            <span class='<%# GetLoginStatusCss(Eval("Status")) %>'>
                                                <%# Eval("Status") %>
                                            </span>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>
            </section>

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
