<%@ Page Title="My Meetings | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UserMeetings.aspx.cs" Inherits="DigitalTransparencySystem.Modules.UserMeetings.UserMeetings" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <uc:UserTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:UserSidebar ActivePage="Meetings" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">

            <header class="flex justify-between items-end mb-8 gap-4">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">My Meetings</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl"><asp:Literal ID="litIntro" runat="server" Text="Schedule a meeting for an event. DTAS creates the Zoom room and sends the join link to that event’s accepted members."></asp:Literal></p>
                </div>
                <asp:Button ID="btnNew" runat="server" Text="Schedule meeting"
                    CssClass="px-6 py-3 bg-primary text-on-primary rounded-xl font-label-md text-label-md font-bold hover:scale-[1.02] active:scale-95 transition-transform cursor-pointer"
                    OnClick="btnNew_Click" CausesValidation="false" />
            </header>

            <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="mb-6 p-4 rounded-xl">
                <asp:Literal ID="litMessage" runat="server"></asp:Literal>
            </asp:Panel>

            <asp:Panel ID="pnlForm" runat="server" Visible="false" CssClass="standard-card rounded-xl p-6 mb-8">
                <h3 class="font-title-lg text-title-lg text-primary mb-6">Schedule a meeting</h3>
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div class="md:col-span-2">
                        <label class="block font-label-md text-on-surface-variant mb-2 font-semibold">Title <span class="text-error">*</span></label>
                        <asp:TextBox ID="txtTitle" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md" placeholder="Team check-in" />
                        <asp:RequiredFieldValidator ID="rfvTitle" runat="server" ControlToValidate="txtTitle" ErrorMessage="Title is required." CssClass="text-error text-xs mt-1 block" Display="Dynamic" ValidationGroup="UserMeeting" />
                    </div>
                    <div class="md:col-span-2">
                        <label class="block font-label-md text-on-surface-variant mb-2 font-semibold">Purpose</label>
                        <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="3" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md resize-none" placeholder="What this meeting is for." />
                    </div>
                    <div>
                        <label class="block font-label-md text-on-surface-variant mb-2 font-semibold">Type</label>
                        <asp:DropDownList ID="ddlMeetingType" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md cursor-pointer">
                            <asp:ListItem Text="General" Value="General" Selected="True" />
                            <asp:ListItem Text="Planning" Value="Planning" />
                            <asp:ListItem Text="Review" Value="Review" />
                            <asp:ListItem Text="Event" Value="Event" />
                            <asp:ListItem Text="Workspace" Value="Workspace" />
                            <asp:ListItem Text="Assignment" Value="Assignment" />
                            <asp:ListItem Text="Follow-up" Value="Follow-up" />
                        </asp:DropDownList>
                    </div>
                    <div>
                        <label class="block font-label-md text-on-surface-variant mb-2 font-semibold">Duration (minutes)</label>
                        <asp:TextBox ID="txtDuration" runat="server" Text="60" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md" />
                    </div>
                    <div>
                        <label class="block font-label-md text-on-surface-variant mb-2 font-semibold">Date &amp; time <span class="text-error">*</span></label>
                        <asp:TextBox ID="txtDateTime" runat="server" TextMode="DateTimeLocal" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md" />
                        <asp:RequiredFieldValidator ID="rfvDateTime" runat="server" ControlToValidate="txtDateTime" ErrorMessage="Date and time are required." CssClass="text-error text-xs mt-1 block" Display="Dynamic" ValidationGroup="UserMeeting" />
                    </div>
                    <div>
                        <label class="block font-label-md text-on-surface-variant mb-2 font-semibold">Venue</label>
                        <asp:TextBox ID="txtVenue" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md" placeholder="Leave blank for Zoom (online)" />
                    </div>
                    <asp:HiddenField ID="hidGroupId" runat="server" />
                    <asp:HiddenField ID="hidAssignmentId" runat="server" />
                    <asp:Panel ID="pnlEventLink" runat="server">
                        <label class="block font-label-md text-on-surface-variant mb-2 font-semibold">Event <span class="text-error">*</span></label>
                        <asp:DropDownList ID="ddlEvent" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md cursor-pointer"></asp:DropDownList>
                        <asp:RequiredFieldValidator ID="rfvEvent" runat="server" ControlToValidate="ddlEvent" InitialValue="" ErrorMessage="Choose the event this meeting is for." CssClass="text-error text-xs mt-1 block" Display="Dynamic" ValidationGroup="UserMeeting" />
                        <p class="text-xs text-on-surface-variant mt-1">Accepted members of this event get the Zoom join link automatically.</p>
                    </asp:Panel>
                    <asp:Panel ID="pnlWorkLink" runat="server">
                        <label class="block font-label-md text-on-surface-variant mb-2 font-semibold">Workspace / work</label>
                        <asp:DropDownList ID="ddlWork" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md cursor-pointer"></asp:DropDownList>
                    </asp:Panel>
                    <asp:Panel ID="pnlGroupLink" runat="server">
                        <label class="block font-label-md text-on-surface-variant mb-2 font-semibold">Assignment group</label>
                        <asp:DropDownList ID="ddlGroup" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md cursor-pointer"></asp:DropDownList>
                        <p class="text-xs text-on-surface-variant mt-1"><asp:Literal ID="litGroupHint" runat="server"></asp:Literal></p>
                    </asp:Panel>
                    <asp:Panel ID="pnlAssignment" runat="server" CssClass="md:col-span-2">
                        <label class="block font-label-md text-on-surface-variant mb-2 font-semibold">Faculty assignment</label>
                        <asp:DropDownList ID="ddlAssignment" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md cursor-pointer"></asp:DropDownList>
                    </asp:Panel>
                    <div class="md:col-span-2">
                        <label class="block font-label-md text-on-surface-variant mb-2 font-semibold">Agenda</label>
                        <asp:TextBox ID="txtAgenda" runat="server" TextMode="MultiLine" Rows="3" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md resize-none" />
                    </div>
                    <div class="md:col-span-2">
                        <asp:Panel ID="pnlZoomHint" runat="server" Visible="false" CssClass="p-3 mb-3 rounded-xl bg-error-container/20 text-sm text-on-surface">
                            Zoom credentials are missing in Web.config, so DTAS cannot create the room. Add Zoom_AccountId, Zoom_ClientId, and Zoom_ClientSecret.
                        </asp:Panel>
                        <asp:Panel ID="pnlZoomReady" runat="server" CssClass="p-3 rounded-xl bg-surface-container-low text-sm text-on-surface">
                            <asp:Literal ID="litZoomReady" runat="server" Text="DTAS will create a Zoom room and email the people who should join. Members cannot enter until the host starts the room and admits them from the waiting room."></asp:Literal>
                        </asp:Panel>
                    </div>
                </div>
                <div class="flex items-center gap-4 pt-6 mt-6 border-t border-outline-variant/30">
                    <asp:Button ID="btnSave" runat="server" Text="Create meeting" ValidationGroup="UserMeeting"
                        CssClass="px-8 py-3 bg-primary text-on-primary rounded-xl font-label-md font-bold cursor-pointer"
                        OnClick="btnSave_Click" />
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" CausesValidation="false"
                        CssClass="px-8 py-3 border border-outline text-on-surface-variant rounded-xl font-label-md font-bold cursor-pointer"
                        OnClick="btnCancel_Click" />
                </div>
            </asp:Panel>

            <div class="flex gap-2 mb-6 flex-wrap">
                <asp:Button ID="btnAll" runat="server" Text="All" CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-primary text-on-primary" OnClick="btnAll_Click" CausesValidation="false" />
                <asp:Button ID="btnUpcoming" runat="server" Text="Upcoming" CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors" OnClick="btnUpcoming_Click" CausesValidation="false" />
                <asp:Button ID="btnPast" runat="server" Text="Past" CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors" OnClick="btnPast_Click" CausesValidation="false" />
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                <asp:Repeater ID="rptMeetings" runat="server">
                    <ItemTemplate>
                        <div class="standard-card rounded-xl overflow-hidden transition-all hover:translate-y-[-2px]">
                            <div class="p-6">
                                <div class="flex items-start justify-between mb-3 gap-3">
                                    <h3 class="font-title-lg text-title-lg text-primary"><%# Eval("MeetingTitle") %></h3>
                                    <span class='<%# "badge-status-" + Eval("Status").ToString().ToLower().Replace(" ", "") %>'><%# FormatStatus(Eval("Status")) %></span>
                                </div>
                                <p class="text-body-md text-on-surface-variant mb-4 line-clamp-2"><%# Eval("Description") %></p>
                                <div class="space-y-2">
                                    <div class="flex items-center gap-2 text-label-md text-on-surface-variant">
                                        <span class="material-symbols-outlined text-[18px]">calendar_today</span>
                                        <span><%# Eval("ScheduledDate", "{0:MMM dd, yyyy}") %></span>
                                    </div>
                                    <div class="flex items-center gap-2 text-label-md text-on-surface-variant">
                                        <span class="material-symbols-outlined text-[18px]">schedule</span>
                                        <span><%# Eval("ScheduledDate", "{0:hh:mm tt}") %> (<%# Eval("Duration") %> min)</span>
                                    </div>
                                    <div class="flex items-center gap-2 text-label-md text-on-surface-variant">
                                        <span class="material-symbols-outlined text-[18px]"><%# Convert.ToInt32(Eval("HasZoom")) == 1 ? "videocam" : "location_on" %></span>
                                        <span><%# Convert.ToInt32(Eval("HasZoom")) == 1 ? "Zoom online" : Eval("Venue") %></span>
                                    </div>
                                    <div class="flex items-center gap-2 text-label-md text-on-surface-variant">
                                        <span class="material-symbols-outlined text-[18px]">link</span>
                                        <span><%# Eval("LinkedLabel") %></span>
                                    </div>
                                    <div class="flex items-center gap-2 text-label-md text-on-surface-variant">
                                        <span class="material-symbols-outlined text-[18px]">send</span>
                                        <span><%# FormatRoom(Eval("Status"), Eval("HasZoom")) %></span>
                                    </div>
                                </div>
                            </div>
                            <div class="px-6 py-4 border-t border-surface-container-high bg-surface-container-low flex justify-between items-center">
                                <span class="text-label-md text-on-surface-variant"><%# Eval("RoleLabel") %></span>
                                <asp:HyperLink ID="lnkDetails" runat="server" NavigateUrl='<%# "~/Modules/UserMeetings/UserMeetingDetails.aspx?MeetingID=" + Eval("MeetingID") %>'
                                    CssClass="text-primary font-label-md text-label-md font-bold hover:underline">Open meeting</asp:HyperLink>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <asp:Panel ID="pnlNoMeetings" runat="server" Visible="false" CssClass="text-center py-16">
                <span class="material-symbols-outlined text-7xl text-outline-variant mb-4 block empty-state-icon">groups</span>
                <p class="font-title-lg text-title-lg text-on-surface-variant">No meetings yet</p>
                <p class="text-body-md text-on-surface-variant mt-2">Schedule a meeting for an event. Members receive the Zoom join link.</p>
            </asp:Panel>

        </main>
    </div>
</asp:Content>
