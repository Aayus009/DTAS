<%@ Page Title="Edit Event | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="EditEvent.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Events.EditEvent" %>
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

        <uc:AdminSidebar ID="adminSide" ActivePage="Events" runat="server" Visible="false" />
        <uc:UserSidebar ID="userSide" ActivePage="MyEvents" runat="server" />

        <!-- Main Content Area -->
        <main class="dashboard-main flex-1 ml-64 p-8">
            <a href="<%= ResolveUrl("~/Modules/Events/EventWorkspace.aspx") %>?EventID=<%= Request.QueryString["EventID"] %>" class="inline-flex items-center gap-1 text-primary font-bold text-sm mb-3">
                <span class="material-symbols-outlined text-[20px]">arrow_back</span>
                Back
            </a>

            <!-- Breadcrumb -->
            <nav class="flex items-center gap-2 mb-6 font-label-md text-label-md text-on-surface-variant">
                <a href="<%= ResolveUrl("~/Modules/Dashboard/AdminDashboard.aspx") %>" class="hover:text-primary transition-colors">Dashboard</a>
                <span class="material-symbols-outlined text-[16px]">chevron_right</span>
                <a href="<%= ResolveUrl("~/Modules/Events/Events.aspx") %>" class="hover:text-primary transition-colors">Events</a>
                <span class="material-symbols-outlined text-[16px]">chevron_right</span>
                <span class="text-primary font-semibold">Edit Event</span>
            </nav>

            <!-- Header Section -->
            <header class="flex justify-between items-end mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Edit Event</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">Update event details and manage the event lifecycle.</p>
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

            <!-- Success Message -->
            <asp:Panel ID="pnlSuccess" runat="server" CssClass="mb-6 p-4 bg-tertiary-container border border-tertiary/30 rounded-xl flex items-center gap-3" Visible="false">
                <span class="material-symbols-outlined text-tertiary">check_circle</span>
                <asp:Label ID="lblSuccess" runat="server" CssClass="font-label-md text-label-md text-on-tertiary-container"></asp:Label>
            </asp:Panel>

            <!-- Form Card -->
            <div class="glass-card rounded-xl p-8">
                <div class="space-y-6">

                    <!-- Row: Event Name + Event Type -->
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div>
                            <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Event Name <span class="text-error">*</span></label>
                            <asp:TextBox ID="txtEventName" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="Enter event name" MaxLength="200"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvEventName" runat="server" ControlToValidate="txtEventName" ErrorMessage="Event name is required." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="EditEvent"></asp:RequiredFieldValidator>
                        </div>
                        <div>
                            <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Event Type</label>
                            <asp:DropDownList ID="ddlEventType" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none">
                                <asp:ListItem Text="General" Value="General" />
                                <asp:ListItem Text="Academic" Value="Academic" />
                                <asp:ListItem Text="Sports" Value="Sports" />
                                <asp:ListItem Text="Cultural" Value="Cultural" />
                                <asp:ListItem Text="Technical" Value="Technical" />
                                <asp:ListItem Text="Project" Value="Project" />
                                <asp:ListItem Text="Custom" Value="Custom" />
                            </asp:DropDownList>
                        </div>
                    </div>

                    <!-- Description -->
                    <div>
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Description</label>
                        <asp:TextBox ID="txtDescription" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="Enter event description" TextMode="MultiLine" Rows="4"></asp:TextBox>
                    </div>

                    <!-- Row: Start Date + End Date -->
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div>
                            <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Start Date <span class="text-error">*</span></label>
                            <asp:TextBox ID="txtStartDate" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" TextMode="DateTimeLocal"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvStartDate" runat="server" ControlToValidate="txtStartDate" ErrorMessage="Start date is required." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="EditEvent"></asp:RequiredFieldValidator>
                        </div>
                        <div>
                            <label class="block font-label-md text-label-md text-on-surface-variant mb-2">End Date <span class="text-error">*</span></label>
                            <asp:TextBox ID="txtEndDate" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" TextMode="DateTimeLocal"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvEndDate" runat="server" ControlToValidate="txtEndDate" ErrorMessage="End date is required." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="EditEvent"></asp:RequiredFieldValidator>
                            <asp:CompareValidator ID="cvEndDate" runat="server" ControlToValidate="txtEndDate" ControlToCompare="txtStartDate" Operator="GreaterThanEqual" ErrorMessage="End date must be on or after start date." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="EditEvent"></asp:CompareValidator>
                        </div>
                    </div>

                    <!-- Row: Venue + Budget -->
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div>
                            <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Venue <span class="text-error">*</span></label>
                            <asp:TextBox ID="txtVenue" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="Enter event venue" MaxLength="200"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvVenue" runat="server" ControlToValidate="txtVenue" ErrorMessage="Venue is required." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="EditEvent"></asp:RequiredFieldValidator>
                        </div>
                        <div>
                            <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Budget</label>
                            <asp:TextBox ID="txtBudget" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="e.g. 5000.00" MaxLength="20"></asp:TextBox>
                        </div>
                    </div>

                    <!-- Row: Organizer + Status -->
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div>
                            <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Organizer</label>
                            <asp:TextBox ID="txtOrganizer" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="Enter organizer name" MaxLength="200"></asp:TextBox>
                        </div>
                        <div>
                            <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Status</label>
                            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none">
                                <asp:ListItem Text="Planned" Value="Planned" />
                                <asp:ListItem Text="In Progress" Value="InProgress" />
                                <asp:ListItem Text="Completed" Value="Completed" />
                                <asp:ListItem Text="Cancelled" Value="Cancelled" />
                            </asp:DropDownList>
                        </div>
                    </div>

                    <div>
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Visibility</label>
                        <asp:DropDownList ID="ddlVisibility" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none">
                            <asp:ListItem Text="Private - invite or code required" Value="Private" />
                            <asp:ListItem Text="Public - discoverable to verified members" Value="Public" />
                        </asp:DropDownList>
                    </div>

                    <!-- Action Buttons -->
                    <div class="flex items-center gap-4 pt-4 border-t border-outline-variant/30">
                        <asp:Button ID="btnUpdate" runat="server" Text="Update Event" CssClass="px-8 py-3 bg-primary text-on-primary rounded-xl font-label-md text-label-md font-bold active:scale-95 transition-transform cursor-pointer" ValidationGroup="EditEvent" OnClick="btnUpdate_Click" />
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
