<%@ Page Title="Create Task | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="CreateTask.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Tasks.CreateTask" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <uc:AdminTopbar runat="server" />

    <div class="flex min-h-screen">

        <uc:AdminSidebar ActivePage="Tasks" runat="server" />

        <!-- Main Content Area -->
        <main class="dashboard-main flex-1 ml-64 p-8">

            <!-- Header Section -->
            <header class="flex justify-between items-end mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Create New Task</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">Define a task and assign it to staff members for tracking.</p>
                </div>
                <div class="flex gap-3">
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="px-6 py-2.5 border border-outline text-on-surface-variant rounded-lg font-label-md text-label-md font-bold hover:bg-surface-container-low transition-colors cursor-pointer" OnClick="btnCancel_Click" CausesValidation="false" />
                </div>
            </header>

            <!-- Error Message -->
            <asp:Panel ID="pnlError" runat="server" CssClass="mb-6 p-4 bg-error-container border border-error/30 rounded-xl flex items-center gap-3" Visible="false">
                <span class="material-symbols-outlined text-error">error</span>
                <asp:Label ID="lblError" runat="server" CssClass="font-label-md text-label-md text-on-error-container"></asp:Label>
            </asp:Panel>

            <!-- Form Card -->
            <div class="glass-card rounded-xl p-8">
                <div class="space-y-6">

                    <!-- Task Title -->
                    <div>
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Task Title <span class="text-error">*</span></label>
                        <asp:TextBox ID="txtTaskTitle" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="Enter task title" MaxLength="200"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvTaskTitle" runat="server" ControlToValidate="txtTaskTitle" ErrorMessage="Task title is required." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="CreateTask"></asp:RequiredFieldValidator>
                    </div>

                    <!-- Description -->
                    <div>
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Description</label>
                        <asp:TextBox ID="txtDescription" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="Enter task description" TextMode="MultiLine" Rows="4"></asp:TextBox>
                    </div>

                    <!-- Row: Priority + Due Date -->
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div>
                            <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Priority <span class="text-error">*</span></label>
                            <asp:DropDownList ID="ddlPriority" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none">
                                <asp:ListItem Text="Select Priority" Value="" />
                                <asp:ListItem Text="Low" Value="Low" />
                                <asp:ListItem Text="Medium" Value="Medium" Selected="True" />
                                <asp:ListItem Text="High" Value="High" />
                                <asp:ListItem Text="Critical" Value="Critical" />
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="rfvPriority" runat="server" ControlToValidate="ddlPriority" ErrorMessage="Priority is required." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="CreateTask" InitialValue=""></asp:RequiredFieldValidator>
                        </div>
                        <div>
                            <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Due Date <span class="text-error">*</span></label>
                            <asp:TextBox ID="txtDueDate" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" TextMode="DateTimeLocal"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvDueDate" runat="server" ControlToValidate="txtDueDate" ErrorMessage="Due date is required." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="CreateTask"></asp:RequiredFieldValidator>
                        </div>
                    </div>

                    <!-- Row: Related Decision + Related Event -->
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div>
                            <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Related Decision</label>
                            <asp:DropDownList ID="ddlRelatedDecision" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none">
                                <asp:ListItem Text="None" Value="" />
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Related Event</label>
                            <asp:DropDownList ID="ddlRelatedEvent" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none">
                                <asp:ListItem Text="None" Value="" />
                            </asp:DropDownList>
                        </div>
                    </div>

                    <!-- Related Meeting -->
                    <div>
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Related Meeting</label>
                        <asp:DropDownList ID="ddlRelatedMeeting" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none">
                            <asp:ListItem Text="None" Value="" />
                        </asp:DropDownList>
                    </div>

                    <!-- Club Selection (Option A) -->
                    <div class="pt-4 border-t border-outline-variant/30">
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2">
                            <span class="material-symbols-outlined text-[18px] align-middle mr-1">groups</span>
                            Pick an Existing Club (optional)
                        </label>
                        <p class="text-sm text-outline mb-4">Selecting a club pre-fills the team roster and suggests the club lead as team leader. You can still adjust both below.</p>
                        <asp:DropDownList ID="ddlClub" runat="server"
                            CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlClub_SelectedIndexChanged">
                            <asp:ListItem Text="- No Club -" Value="" />
                        </asp:DropDownList>
                    </div>

                    <!-- Team Leader Selection -->
                    <div class="pt-4 border-t border-outline-variant/30">
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2">
                            <span class="material-symbols-outlined text-[18px] align-middle mr-1">leaderboard</span>
                            Team Leader
                        </label>
                        <p class="text-sm text-outline mb-4">Select one user as the team leader for this task. The leader will build and manage the task team.</p>
                        <asp:DropDownList ID="ddlLeader" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none">
                            <asp:ListItem Text="No Leader (Individual Task)" Value="" />
                        </asp:DropDownList>
                    </div>

                    <!-- Assignment Section -->
                    <div class="pt-4 border-t border-outline-variant/30">
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2">
                            <span class="material-symbols-outlined text-[18px] align-middle mr-1">group_add</span>
                            Assign To
                        </label>
                        <p class="text-sm text-outline mb-4">Select one or more users to assign this task to.</p>
                        <asp:CheckBoxList ID="cblAssignUsers" runat="server" CssClass="grid grid-cols-1 md:grid-cols-2 gap-3" RepeatLayout="Flow">
                        </asp:CheckBoxList>
                        <asp:Panel ID="pnlNoUsers" runat="server" Visible="false" CssClass="p-4 bg-surface-container-low rounded-lg text-center">
                            <span class="material-symbols-outlined text-outline-variant text-[24px] align-middle">person_off</span>
                            <span class="font-body-md text-body-md text-on-surface-variant ml-2">No users available for assignment.</span>
                        </asp:Panel>
                    </div>

                    <!-- Action Buttons -->
                    <div class="flex items-center gap-4 pt-4 border-t border-outline-variant/30">
                        <asp:Button ID="btnCreate" runat="server" Text="Create &amp; Assign Task" CssClass="px-8 py-3 bg-primary text-on-primary rounded-xl font-label-md text-label-md font-bold active:scale-95 transition-transform cursor-pointer" ValidationGroup="CreateTask" OnClick="btnCreate_Click" />
                        <asp:Button ID="btnCancelBottom" runat="server" Text="Cancel" CssClass="px-8 py-3 border border-outline text-on-surface-variant rounded-xl font-label-md text-label-md font-bold hover:bg-surface-container-low transition-colors cursor-pointer" OnClick="btnCancel_Click" CausesValidation="false" />
                    </div>
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
    <script type="text/javascript" src="<%= ResolveUrl("~/Assets/js/dashboard.js") %>"></script>
</asp:Content>