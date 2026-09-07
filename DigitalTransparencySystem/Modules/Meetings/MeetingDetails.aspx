<%@ Page Title="Meeting Details | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="MeetingDetails.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Meetings.MeetingDetails" %>
<%@ Import Namespace="DigitalTransparencySystem.Helpers" %>
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
                        <h1 class="font-headline-lg text-headline-lg text-primary">Meeting Details</h1>
                    </div>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">
                        <asp:Literal ID="litMeetingTitle" runat="server" />
                    </p>
                </div>
                <div class="flex gap-3">
                    <asp:Button ID="btnStartMeeting" runat="server" Text="Start Meeting" CssClass="flex items-center gap-2 bg-amber-500 text-white px-6 py-2.5 rounded-full font-label-md shadow-sm hover:scale-[1.02] active:scale-95 transition-all cursor-pointer" OnClick="btnStartMeeting_Click" Visible="false" />
                    <asp:Button ID="btnEndMeeting" runat="server" Text="End Meeting" CssClass="flex items-center gap-2 bg-green-600 text-white px-6 py-2.5 rounded-full font-label-md shadow-sm hover:scale-[1.02] active:scale-95 transition-all cursor-pointer" OnClick="btnEndMeeting_Click" Visible="false" />
                    <asp:Button ID="btnRecordDecision" runat="server" Text="Record Decision" CssClass="flex items-center gap-2 bg-primary text-on-primary px-6 py-2.5 rounded-full font-label-md shadow-sm hover:scale-[1.02] active:scale-95 transition-all cursor-pointer" OnClick="btnRecordDecision_Click" Visible="false" />
                    <asp:Button ID="btnAssignTask" runat="server" Text="Assign Task" CssClass="flex items-center gap-2 border border-primary text-primary px-6 py-2.5 rounded-full font-label-md font-bold hover:bg-primary-fixed-dim/10 transition-colors cursor-pointer" OnClick="btnAssignTask_Click" Visible="false" />
                </div>
            </header>

            <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">

                <!-- Left Column: Meeting Info -->
                <div class="lg:col-span-2 space-y-6">

                    <!-- Meeting Details Card -->
                    <div class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-6">Meeting Information</h3>
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div>
                                <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-1">Meeting Type</span>
                                <span class="font-body-md text-on-surface"><asp:Literal ID="litMeetingType" runat="server" /></span>
                            </div>
                            <div>
                                <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-1">Status</span>
                                <asp:Literal ID="litStatus" runat="server" />
                            </div>
                            <div>
                                <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-1">Date &amp; Time</span>
                                <span class="font-body-md text-on-surface"><asp:Literal ID="litDate" runat="server" /></span>
                            </div>
                            <div>
                                <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-1">Duration</span>
                                <span class="font-body-md text-on-surface"><asp:Literal ID="litDuration" runat="server" /></span>
                            </div>
                            <div class="md:col-span-2">
                                <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-1">Venue</span>
                                <span class="font-body-md text-on-surface"><asp:Literal ID="litVenue" runat="server" /></span>
                            </div>
                            <div class="md:col-span-2">
                                <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-1">Description</span>
                                <span class="font-body-md text-on-surface-variant"><asp:Literal ID="litDescription" runat="server" /></span>
                            </div>
                            <div class="md:col-span-2">
                                <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-1">Agenda</span>
                                <span class="font-body-md text-on-surface-variant whitespace-pre-line"><asp:Literal ID="litAgenda" runat="server" /></span>
                            </div>
                        </div>
                    </div>

                    <!-- Zoom Meeting Card -->
                    <asp:Panel ID="pnlZoom" runat="server" Visible="false">
                        <div class="standard-card rounded-xl p-6 border-l-4" style="border-left-color: #2d8cff;">
                            <div class="flex items-center justify-between mb-4">
                                <h3 class="font-title-lg text-title-lg text-primary flex items-center gap-2">
                                    <span class="material-symbols-outlined text-[#2d8cff]">videocam</span> Zoom Meeting
                                </h3>
                                <asp:LinkButton ID="btnSyncRecordings" runat="server" OnClick="btnSyncRecordings_Click" CssClass="flex items-center gap-1 text-xs font-semibold text-primary hover:text-primary-dark transition-colors">
                                    <span class="material-symbols-outlined text-[16px]">sync</span> Sync Recordings
                                </asp:LinkButton>
                            </div>

                            <asp:Panel ID="pnlZoomError" runat="server" Visible="false" CssClass="mb-4 p-3 rounded-lg bg-error-container/20 border border-error/30">
                                <p class="text-error text-sm">
                                    <asp:Literal ID="litZoomError" runat="server" />
                                </p>
                            </asp:Panel>

                            <div class="space-y-3 mb-4">
                                <div class="flex items-center gap-3 p-3 bg-surface-container-low rounded-lg border border-outline-variant/30">
                                    <span class="material-symbols-outlined text-primary">link</span>
                                    <div class="flex-1 min-w-0">
                                        <p class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest text-xs">Join Link</p>
                                        <asp:HyperLink ID="lnkJoinUrl" runat="server" Target="_blank" CssClass="text-primary font-semibold text-sm truncate block word-break" />
                                    </div>
                                </div>
                                <div class="flex items-center gap-3 p-3 bg-surface-container-low rounded-lg border border-outline-variant/30">
                                    <span class="material-symbols-outlined text-tertiary">key</span>
                                    <div class="flex-1">
                                        <p class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest text-xs">Passcode</p>
                                        <span class="font-body-md font-semibold text-on-surface"><asp:Literal ID="litPasscode" runat="server" /></span>
                                    </div>
                                </div>
                            </div>

                            <p class="text-sm text-on-surface-variant mb-4">
                                Recording is not shown on the Join link. Use <strong>Start (Host)</strong> so Zoom can cloud-record this meeting.
                            </p>
                            <div class="flex flex-wrap gap-3">
                                <asp:HyperLink ID="lnkStartButton" runat="server" Target="_blank" CssClass="flex items-center gap-2 bg-primary text-on-primary px-5 py-2.5 rounded-full font-label-md font-bold hover:scale-[1.02] active:scale-95 transition-all">
                                    <span class="material-symbols-outlined text-[18px]">play_circle</span> Start (Host)
                                </asp:HyperLink>
                                <asp:HyperLink ID="lnkJoinButton" runat="server" Target="_blank" CssClass="flex items-center gap-2 border border-primary text-primary px-5 py-2.5 rounded-full font-label-md font-bold hover:bg-primary-fixed-dim/10 transition-colors">
                                    <span class="material-symbols-outlined text-[18px]">videocam</span> Join Meeting
                                </asp:HyperLink>
                                <asp:Button ID="btnEnableCloud" runat="server" Text="Enable cloud recording"
                                    CssClass="px-5 py-2.5 border border-outline text-on-surface rounded-full font-label-md font-bold bg-transparent cursor-pointer"
                                    CausesValidation="false" />
                            </div>
                        </div>
                    </asp:Panel>

                    <!-- Recordings Card -->
                    <asp:Panel ID="pnlRecordings" runat="server" Visible="false">
                    <div class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-4">Meeting Recordings</h3>
                        <asp:Repeater ID="rptRecordings" runat="server">
                            <ItemTemplate>
                                <div class="flex items-start gap-3 p-4 bg-surface-container-low rounded-lg border border-outline-variant/30 mb-3">
                                    <span class="material-symbols-outlined text-[#2d8cff] mt-0.5">movie</span>
                                    <div class="flex-1 min-w-0">
                                        <p class="font-label-md text-label-md font-bold truncate"><%# Eval("Title") %></p>
                                        <p class="text-xs text-on-surface-variant">
                                            <%# Eval("RecordingType") %>
                                            <%# GetRecordingMeta(Eval("DurationSeconds"), Eval("RecordedAt")) %>
                                        </p>
                                        <div class="flex gap-2 mt-2">
                                            <%# BuildRecordingLinks((string)Eval("PlayUrl"), (string)Eval("DownloadUrl")) %>
                                        </div>
                                    </div>
                                </div>
                            </ItemTemplate>
                            <FooterTemplate>
                                <asp:Panel ID="pnlNoRecordings" runat="server" Visible='<%# rptRecordings.Items.Count == 0 %>'>
                                    <p class="text-on-surface-variant font-body-md text-center py-4">No recordings available yet.</p>
                                </asp:Panel>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                    </asp:Panel>

                    <!-- Minutes of Meeting -->
                    <div class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-4">Minutes of Meeting</h3>
                        <asp:TextBox ID="txtMinutes" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all min-h-[160px]" TextMode="MultiLine" Rows="8" placeholder="Record meeting minutes here..." />
                        <div class="flex justify-end mt-4">
                            <asp:Button ID="btnSaveMinutes" runat="server" Text="Save Minutes" CssClass="px-6 py-2.5 bg-primary text-on-primary rounded-full font-label-md font-bold shadow-sm hover:scale-[1.02] active:scale-95 transition-all cursor-pointer" OnClick="btnSaveMinutes_Click" />
                        </div>
                    </div>

                    <!-- Decisions -->
                    <div class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-4">Decisions</h3>
                        <asp:Repeater ID="rptDecisions" runat="server">
                            <ItemTemplate>
                                <div class="flex items-center gap-4 p-4 bg-surface-container-low rounded-lg border border-outline-variant/30 mb-3">
                                    <span class="material-symbols-outlined text-primary">gavel</span>
                                    <div class="flex-1">
                                        <p class="font-label-md text-label-md font-bold"><%# Eval("DecisionTitle") %></p>
                                    </div>
                                    <span class='<%# GetDecisionStatusBadge(Eval("Status").ToString()) %>'><%# Eval("Status") %></span>
                                </div>
                            </ItemTemplate>
                            <FooterTemplate>
                                <asp:Panel ID="pnlNoDecisions" runat="server" Visible='<%# rptDecisions.Items.Count == 0 %>'>
                                    <p class="text-on-surface-variant font-body-md text-center py-4">No decisions recorded for this meeting.</p>
                                </asp:Panel>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>

                    <!-- Tasks -->
                    <div class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-4">Assigned Tasks</h3>
                        <asp:Repeater ID="rptTasks" runat="server">
                            <ItemTemplate>
                                <div class="flex items-center gap-4 p-4 bg-surface-container-low rounded-lg border border-outline-variant/30 mb-3">
                                    <span class="material-symbols-outlined text-tertiary">assignment</span>
                                    <div class="flex-1">
                                        <p class="font-label-md text-label-md font-bold"><%# Eval("TaskTitle") %></p>
                                        <p class='<%# "text-xs " + TimelineService.DueClass(Eval("DueDate"), Eval("Status")) %>'><%# TimelineService.DueLabel(Eval("DueDate"), Eval("Status")) %></p>
                                    </div>
                                    <span class='<%# TimelineService.StatusBadgeClass(Eval("Status"), Eval("DueDate")) %>'><%# TimelineService.StatusLabel(Eval("Status"), Eval("DueDate")) %></span>
                                </div>
                            </ItemTemplate>
                            <FooterTemplate>
                                <asp:Panel ID="pnlNoTasks" runat="server" Visible='<%# rptTasks.Items.Count == 0 %>'>
                                    <p class="text-on-surface-variant font-body-md text-center py-4">No tasks assigned for this meeting.</p>
                                </asp:Panel>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                </div>

                <!-- Right Column: Participants -->
                <div class="lg:col-span-1">
                    <div class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-6">Participants</h3>
                        <asp:Repeater ID="rptParticipants" runat="server">
                            <ItemTemplate>
                                <div class="flex items-center gap-3 p-3 mb-3 bg-surface-container-low rounded-lg border border-outline-variant/30">
                                    <div class="h-10 w-10 rounded-full bg-primary-container flex items-center justify-center text-on-primary-container font-bold text-sm shrink-0">
                                        <%# GetInitials(Eval("FullName").ToString()) %>
                                    </div>
                                    <div class="flex-1 min-w-0">
                                        <p class="font-label-md text-label-md font-bold truncate"><%# Eval("FullName") %></p>
                                        <p class="text-xs text-on-surface-variant"><%# Eval("Role") %></p>
                                    </div>
                                    <span class='<%# GetAttendanceBadge(Eval("AttendanceStatus").ToString()) %>'><%# Eval("AttendanceStatus") %></span>
                                </div>
                            </ItemTemplate>
                            <FooterTemplate>
                                <asp:Panel ID="pnlNoParticipants" runat="server" Visible='<%# rptParticipants.Items.Count == 0 %>'>
                                    <p class="text-on-surface-variant font-body-md text-center py-4">No participants added.</p>
                                </asp:Panel>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                </div>

            </div>
        </main>
    </div>

</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="<%= ResolveUrl("~/Assets/js/dashboard.js") %>"></script>
</asp:Content>
