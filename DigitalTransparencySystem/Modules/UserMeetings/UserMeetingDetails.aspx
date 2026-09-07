<%@ Page Title="Meeting | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UserMeetingDetails.aspx.cs" Inherits="DigitalTransparencySystem.Modules.UserMeetings.UserMeetingDetails" %>
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
            <a href="<%= ResolveUrl("~/Modules/UserMeetings/UserMeetings.aspx") %>" class="inline-flex items-center gap-1 text-primary font-bold text-sm mb-3">
                <span class="material-symbols-outlined text-[20px]">arrow_back</span>
                Back to meetings
            </a>

            <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="mb-6 p-4 rounded-xl">
                <asp:Literal ID="litMessage" runat="server"></asp:Literal>
            </asp:Panel>

            <header class="flex justify-between items-start mb-8 gap-4">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2"><asp:Literal ID="litTitle" runat="server"></asp:Literal></h1>
                    <p class="text-on-surface-variant"><asp:Literal ID="litLinked" runat="server"></asp:Literal></p>
                </div>
                <span class="font-body-md"><asp:Literal ID="litStatus" runat="server"></asp:Literal></span>
            </header>

            <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
                <div class="lg:col-span-2 space-y-6">
                    <div class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-4">Meeting details</h3>
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                                <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-1">When</span>
                                <span class="font-body-md text-on-surface"><asp:Literal ID="litWhen" runat="server"></asp:Literal></span>
                            </div>
                            <div>
                                <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-1">Duration</span>
                                <span class="font-body-md text-on-surface"><asp:Literal ID="litDuration" runat="server"></asp:Literal></span>
                            </div>
                            <div>
                                <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-1">Venue</span>
                                <span class="font-body-md text-on-surface"><asp:Literal ID="litVenue" runat="server"></asp:Literal></span>
                            </div>
                            <div>
                                <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-1">Type</span>
                                <span class="font-body-md text-on-surface"><asp:Literal ID="litType" runat="server"></asp:Literal></span>
                            </div>
                            <div class="md:col-span-2">
                                <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-1">Purpose</span>
                                <span class="font-body-md text-on-surface"><asp:Literal ID="litDescription" runat="server"></asp:Literal></span>
                            </div>
                            <div class="md:col-span-2">
                                <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-1">Agenda</span>
                                <span class="font-body-md text-on-surface-variant whitespace-pre-line"><asp:Literal ID="litAgenda" runat="server"></asp:Literal></span>
                            </div>
                        </div>
                    </div>

                    <asp:Panel ID="pnlZoom" runat="server" Visible="false">
                        <div class="standard-card rounded-xl p-6" style="border-left: 4px solid #2d8cff;">
                            <h3 class="font-title-lg text-title-lg text-primary flex items-center gap-2 mb-3">
                                <span class="material-symbols-outlined text-[#2d8cff]">videocam</span>
                                Zoom room
                            </h3>
                            <asp:Panel ID="pnlZoomOpen" runat="server" Visible="false">
                                <p class="text-sm text-on-surface-variant mb-2">Passcode: <strong><asp:Literal ID="litPasscode" runat="server"></asp:Literal></strong></p>
                                <p class="text-sm text-on-surface-variant mb-3 break-all">Join link: <asp:HyperLink ID="lnkJoinUrlText" runat="server" Target="_blank" CssClass="text-primary font-bold"></asp:HyperLink></p>
                                <p class="text-sm text-on-surface-variant mb-4">
                                    Event members already received this join link. The host should click <strong>Start as host</strong>. Everyone else uses <strong>Join</strong>.
                                    The room closes automatically when the scheduled duration ends.
                                </p>
                                <div id="txZoomActions" class="flex flex-wrap gap-3">
                                    <asp:HyperLink ID="lnkStart" runat="server" Target="_blank" Visible="false"
                                        CssClass="inline-flex items-center gap-2 bg-primary text-on-primary px-5 py-2.5 rounded-full font-label-md font-bold">
                                        <span class="material-symbols-outlined text-[18px]">play_circle</span> Start as host
                                    </asp:HyperLink>
                                    <asp:HyperLink ID="lnkJoin" runat="server" Target="_blank"
                                        CssClass="inline-flex items-center gap-2 border border-primary text-primary px-5 py-2.5 rounded-full font-label-md font-bold">
                                        <span class="material-symbols-outlined text-[18px]">videocam</span> Join
                                    </asp:HyperLink>
                                </div>
                                <asp:HiddenField ID="hidRoomEndsAt" runat="server" />
                            </asp:Panel>
                            <asp:Panel ID="pnlZoomClosed" runat="server" Visible="false">
                                <p class="text-sm text-on-surface-variant m-0">
                                    This meeting has ended. The Zoom room is closed, so nobody can start or join again.
                                </p>
                            </asp:Panel>
                        </div>
                    </asp:Panel>

                    <div class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-2">Minutes</h3>
                        <p class="text-xs text-on-surface-variant mb-4"><asp:Literal ID="litMinutesHint" runat="server"></asp:Literal></p>
                        <asp:TextBox ID="txtMinutes" runat="server" TextMode="MultiLine" Rows="7"
                            CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md min-h-[140px]" />
                        <div class="flex justify-end mt-4">
                            <asp:Button ID="btnSaveMinutes" runat="server" Text="Save minutes"
                                CssClass="px-6 py-2.5 bg-primary text-on-primary rounded-full font-label-md font-bold cursor-pointer"
                                OnClick="btnSaveMinutes_Click" />
                        </div>
                    </div>
                </div>

                <div class="space-y-6">
                    <div class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-4">Linked work</h3>
                        <asp:HyperLink ID="lnkEvent" runat="server" Visible="false" CssClass="block text-primary font-bold mb-2 hover:underline"></asp:HyperLink>
                        <asp:HyperLink ID="lnkWork" runat="server" Visible="false" CssClass="block text-primary font-bold mb-2 hover:underline"></asp:HyperLink>
                        <asp:HyperLink ID="lnkGroup" runat="server" Visible="false" CssClass="block text-primary font-bold mb-2 hover:underline"></asp:HyperLink>
                        <asp:Literal ID="litNoLink" runat="server" Text="This meeting is not linked to an event, workspace, or assignment."></asp:Literal>
                    </div>
                    <div class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-4">Participants</h3>
                        <p class="text-xs text-on-surface-variant mb-3">Accepted event members were added when this meeting was created. Resend if someone joined the event later.</p>
                        <asp:Button ID="btnSendInvites" runat="server" Visible="false" Text="Resend join link to event members"
                            CssClass="w-full mb-4 px-4 py-2.5 bg-primary text-on-primary rounded-xl font-label-md font-bold cursor-pointer"
                            OnClick="btnSendInvites_Click" CausesValidation="false" />
                        <asp:Repeater ID="rptParticipants" runat="server">
                            <ItemTemplate>
                                <div class="flex justify-between gap-2 py-2 border-b border-surface-container-high last:border-0">
                                    <span class="font-body-md"><%# Eval("FullName") %></span>
                                    <span class="text-xs text-on-surface-variant"><%# Eval("Role") %></span>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>
            </div>
        </main>
    </div>
</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript">
        (function () {
            var field = document.getElementById('<%= hidRoomEndsAt.ClientID %>');
            if (!field || !field.value) return;
            var ends = Date.parse(field.value);
            if (!ends) return;
            var closed = false;
            function check() {
                if (closed || Date.now() < ends) return;
                closed = true;
                window.location.reload();
            }
            setInterval(check, 10000);
            check();
        })();
    </script>
</asp:Content>
