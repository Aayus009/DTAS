<%@ Page Title="Schedule Meeting | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="ScheduleMeeting.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Meetings.ScheduleMeeting" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <uc:AdminTopbar runat="server" />

    <div class="flex min-h-screen">

        <uc:AdminSidebar ActivePage="Meetings" runat="server" />

        <!-- Main Content Area -->
        <main class="dashboard-main flex-1 ml-64 p-8">

            <!-- Header -->
            <header class="flex justify-between items-end mb-8">
                <div>
                    <div class="flex items-center gap-2 mb-2">
                        <asp:HyperLink ID="lnkBack" runat="server" NavigateUrl="~/Modules/Dashboard/AdminDashboard.aspx" CssClass="material-symbols-outlined text-on-surface-variant hover:text-primary transition-colors">arrow_back</asp:HyperLink>
                        <h1 class="font-headline-lg text-headline-lg text-primary">Schedule Meeting</h1>
                    </div>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">Create a new institutional meeting and assign participants.</p>
                </div>
            </header>

            <!-- Form Card -->
            <div class="standard-card rounded-xl p-8 max-w-4xl">
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">

                    <!-- Meeting Title -->
                    <div class="md:col-span-2">
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2 font-semibold">Meeting Title <span class="text-error">*</span></label>
                        <asp:TextBox ID="txtTitle" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="Enter meeting title" />
                        <asp:RequiredFieldValidator ID="rfvTitle" runat="server" ControlToValidate="txtTitle" ErrorMessage="Title is required." CssClass="text-error text-xs mt-1 block" Display="Dynamic" ValidationGroup="Schedule" />
                    </div>

                    <!-- Description -->
                    <div class="md:col-span-2">
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2 font-semibold">Description</label>
                        <asp:TextBox ID="txtDescription" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="Brief description of the meeting" TextMode="MultiLine" Rows="3" />
                    </div>

                    <!-- Meeting Type -->
                    <div>
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2 font-semibold">Meeting Type</label>
                        <asp:DropDownList ID="ddlMeetingType" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all cursor-pointer">
                            <asp:ListItem Text="General" Value="General" Selected="True" />
                            <asp:ListItem Text="Planning" Value="Planning" />
                            <asp:ListItem Text="Review" Value="Review" />
                            <asp:ListItem Text="Decision" Value="Decision" />
                            <asp:ListItem Text="Follow-up" Value="Follow-up" />
                        </asp:DropDownList>
                    </div>

                    <!-- Duration -->
                    <div>
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2 font-semibold">Duration (minutes)</label>
                        <asp:TextBox ID="txtDuration" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="e.g. 60" />
                    </div>

                    <!-- Date/Time -->
                    <div>
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2 font-semibold">Date &amp; Time <span class="text-error">*</span></label>
                        <asp:TextBox ID="txtDateTime" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" TextMode="DateTimeLocal" />
                        <asp:RequiredFieldValidator ID="rfvDateTime" runat="server" ControlToValidate="txtDateTime" ErrorMessage="Date/Time is required." CssClass="text-error text-xs mt-1 block" Display="Dynamic" ValidationGroup="Schedule" />
                    </div>

                    <!-- Venue -->
                    <div>
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2 font-semibold">Venue <span class="text-error">*</span></label>
                        <asp:TextBox ID="txtVenue" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="e.g. Conference Room A" />
                        <asp:RequiredFieldValidator ID="rfvVenue" runat="server" ControlToValidate="txtVenue" ErrorMessage="Venue is required." CssClass="text-error text-xs mt-1 block" Display="Dynamic" ValidationGroup="Schedule" />
                    </div>

                    <!-- Agenda -->
                    <div class="md:col-span-2">
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2 font-semibold">Agenda</label>
                        <asp:TextBox ID="txtAgenda" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="List agenda items..." TextMode="MultiLine" Rows="4" />
                    </div>

                    <!-- Participants -->
                    <div class="md:col-span-2">
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2 font-semibold">Add Participants</label>
                        <asp:ListBox ID="lstParticipants" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" SelectionMode="Multiple" Rows="6" />
                        <p class="text-xs text-on-surface-variant mt-1">Hold Ctrl (Windows) or Cmd (Mac) to select multiple participants.</p>
                    </div>

                    <!-- Zoom Meeting Toggle -->
                    <div class="md:col-span-2">
                        <label class="flex items-start gap-3 cursor-pointer">
                            <asp:CheckBox ID="chkZoom" runat="server" CssClass="mt-1 h-5 w-5 accent-primary" />
                            <span>
                                <span class="block font-label-md text-label-md text-on-surface font-semibold">Create a Zoom meeting</span>
                                <span class="block text-xs text-on-surface-variant mt-1">Creates a Zoom room. Cloud recording starts when the host clicks Start as host. After the meeting, sync recordings from the meeting page.</span>
                            </span>
                        </label>
                    </div>

                </div>

                <!-- Actions -->
                <div class="flex justify-end gap-4 mt-8 pt-6 border-t border-surface-container-high">
                    <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="w-full mb-4 p-4 rounded-xl bg-error-container/20 border border-error/30">
                        <div class="flex items-center gap-2">
                            <span class="material-symbols-outlined text-error">error</span>
                            <asp:Label ID="lblError" runat="server" CssClass="text-error text-sm font-medium"></asp:Label>
                        </div>
                    </asp:Panel>
                    <asp:HyperLink ID="lnkCancel" runat="server" NavigateUrl="~/Modules/Dashboard/AdminDashboard.aspx" CssClass="px-6 py-2.5 border border-outline text-on-surface-variant rounded-full font-label-md font-bold hover:bg-surface-container-low transition-colors">Cancel</asp:HyperLink>
                    <asp:Button ID="btnSchedule" runat="server" Text="Schedule Meeting" CssClass="px-6 py-2.5 bg-primary text-on-primary rounded-full font-label-md font-bold shadow-sm hover:scale-[1.02] active:scale-95 transition-all cursor-pointer" OnClick="btnSchedule_Click" ValidationGroup="Schedule" />
                </div>
            </div>

        </main>
    </div>

</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="<%= ResolveUrl("~/Assets/js/dashboard.js") %>"></script>
</asp:Content>
