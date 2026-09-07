<%@ Page Title="Edit Task | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="EditTask.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Tasks.EditTask" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <uc:AdminTopbar ID="adminTop" runat="server" Visible="false" />
    <uc:UserTopbar ID="userTop" runat="server" />

    <div class="flex min-h-screen">

        <uc:AdminSidebar ID="adminSide" ActivePage="Tasks" runat="server" Visible="false" />
        <uc:UserSidebar ID="userSide" ActivePage="Tasks" runat="server" />

        <!-- Main Content Area -->
        <main class="dashboard-main flex-1 ml-64 p-8">
            <a href="<%= ResolveUrl("~/Modules/Tasks/TaskDetails.aspx") %>?TaskID=<%= Request.QueryString["TaskID"] %>" class="inline-flex items-center gap-1 text-primary font-bold text-sm mb-3">
                <span class="material-symbols-outlined text-[20px]">arrow_back</span>
                Back
            </a>

            <!-- Breadcrumb -->
            <nav class="flex items-center gap-2 mb-6 font-label-md text-label-md text-on-surface-variant">
                <a href="<%= ResolveUrl("~/Modules/Dashboard/AdminDashboard.aspx") %>" class="hover:text-primary transition-colors">Dashboard</a>
                <span class="material-symbols-outlined text-[16px]">chevron_right</span>
                <a href="<%= ResolveUrl("~/Modules/Tasks/Tasks.aspx") %>" class="hover:text-primary transition-colors">Tasks</a>
                <span class="material-symbols-outlined text-[16px]">chevron_right</span>
                <span class="text-primary font-semibold">Edit Task</span>
            </nav>

            <!-- Header Section -->
            <header class="flex justify-between items-end mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Edit Task</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">Update task details and manage assignments.</p>
                </div>
                <div class="flex gap-3">
                    <asp:Button ID="btnCancelTop" runat="server" Text="Cancel" CssClass="px-6 py-2.5 border border-outline text-on-surface-variant rounded-lg font-label-md text-label-md font-bold hover:bg-surface-container-low transition-colors cursor-pointer" OnClick="btnCancel_Click" CausesValidation="false" />
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
                        <asp:RequiredFieldValidator ID="rfvTaskTitle" runat="server" ControlToValidate="txtTaskTitle" ErrorMessage="Task title is required." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="EditTask"></asp:RequiredFieldValidator>
                    </div>

                    <!-- Description -->
                    <div>
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Description</label>
                        <asp:TextBox ID="txtDescription" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="Enter task description" TextMode="MultiLine" Rows="4"></asp:TextBox>
                    </div>

                    <!-- Row: Priority + Status -->
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div>
                            <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Priority <span class="text-error">*</span></label>
                            <asp:DropDownList ID="ddlPriority" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none">
                                <asp:ListItem Text="Select Priority" Value="" />
                                <asp:ListItem Text="Low" Value="Low" />
                                <asp:ListItem Text="Medium" Value="Medium" />
                                <asp:ListItem Text="High" Value="High" />
                                <asp:ListItem Text="Critical" Value="Critical" />
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="rfvPriority" runat="server" ControlToValidate="ddlPriority" ErrorMessage="Priority is required." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="EditTask" InitialValue=""></asp:RequiredFieldValidator>
                        </div>
                        <div>
                            <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Status <span class="text-error">*</span></label>
                            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none">
                                <asp:ListItem Text="Select Status" Value="" />
                                <asp:ListItem Text="Pending" Value="Pending" />
                                <asp:ListItem Text="In Progress" Value="InProgress" />
                                <asp:ListItem Text="Submitted" Value="Submitted" />
                                <asp:ListItem Text="Under Review" Value="UnderReview" />
                                <asp:ListItem Text="Revision Needed" Value="RevisionNeeded" />
                                <asp:ListItem Text="Approved" Value="Approved" />
                                <asp:ListItem Text="Completed" Value="Completed" />
                                <asp:ListItem Text="Delayed" Value="Delayed" />
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="rfvStatus" runat="server" ControlToValidate="ddlStatus" ErrorMessage="Status is required." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="EditTask" InitialValue=""></asp:RequiredFieldValidator>
                        </div>
                    </div>

                    <!-- Due Date -->
                    <div>
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Due Date <span class="text-error">*</span></label>
                        <asp:TextBox ID="txtDueDate" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" TextMode="DateTimeLocal"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvDueDate" runat="server" ControlToValidate="txtDueDate" ErrorMessage="Due date is required." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="EditTask"></asp:RequiredFieldValidator>
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

                    <!-- Team Leader Selection -->
                    <div>
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2">
                            <span class="material-symbols-outlined text-[18px] align-middle mr-1">leaderboard</span>
                            Team Leader
                        </label>
                        <p class="text-sm text-outline mb-4">Assign a team leader who will manage the task team.</p>
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
                        <asp:Button ID="btnUpdate" runat="server" Text="Update Task" CssClass="px-8 py-3 bg-primary text-on-primary rounded-xl font-label-md text-label-md font-bold active:scale-95 transition-transform cursor-pointer" ValidationGroup="EditTask" OnClick="btnUpdate_Click" />
                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="px-8 py-3 border border-outline text-on-surface-variant rounded-xl font-label-md text-label-md font-bold hover:bg-surface-container-low transition-colors cursor-pointer" OnClick="btnCancel_Click" CausesValidation="false" />
                    </div>
                </div>
            </div>

        </main>
    </div>

</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="<%= ResolveUrl("~/Assets/js/dashboard.js") %>"></script>
</asp:Content>
