<%@ Page Title="Meetings | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="Meetings.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Meetings.Meetings" %>
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

            <!-- Header Section -->
            <header class="flex justify-between items-end mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Meetings</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">Manage and track institutional meetings, agendas, and participation records.</p>
                </div>
                <div class="flex gap-3">
                    <asp:HyperLink ID="lnkScheduleMeeting" runat="server" NavigateUrl="~/Modules/Meetings/ScheduleMeeting.aspx" CssClass="flex items-center gap-2 bg-primary text-on-primary px-6 py-2.5 rounded-full font-label-md shadow-sm hover:scale-[1.02] active:scale-95 transition-all">
                        <span class="material-symbols-outlined text-[18px]">add</span>
                        Schedule Meeting
                    </asp:HyperLink>
                </div>
            </header>

            <!-- Filters -->
            <div class="flex flex-wrap items-center gap-4 mb-6">
                <div class="relative">
                    <span class="absolute left-3 top-1/2 -translate-y-1/2 material-symbols-outlined text-outline">search</span>
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="pl-10 pr-4 py-2 bg-surface-container-low border border-outline rounded-full font-body-md text-body-md w-72 focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="Search by meeting title..." AutoPostBack="true" OnTextChanged="txtSearch_TextChanged" />
                </div>
                <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="px-4 py-2 bg-surface-container-low border border-outline rounded-full font-label-md text-label-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all cursor-pointer" AutoPostBack="true" OnSelectedIndexChanged="ddlStatusFilter_SelectedIndexChanged">
                    <asp:ListItem Text="All Statuses" Value="All" />
                    <asp:ListItem Text="Scheduled" Value="Scheduled" />
                    <asp:ListItem Text="In Progress" Value="InProgress" />
                    <asp:ListItem Text="Completed" Value="Completed" />
                    <asp:ListItem Text="Cancelled" Value="Cancelled" />
                </asp:DropDownList>
            </div>

            <!-- Meetings Table -->
            <section class="standard-card rounded-xl overflow-hidden">
                <div class="overflow-x-auto">
                    <table class="w-full text-left font-body-md">
                        <thead>
                            <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                <th class="px-6 py-3 font-semibold">Meeting Title</th>
                                <th class="px-6 py-3 font-semibold">Type</th>
                                <th class="px-6 py-3 font-semibold">Date/Time</th>
                                <th class="px-6 py-3 font-semibold">Venue</th>
                                <th class="px-6 py-3 font-semibold">Status</th>
                                <th class="px-6 py-3 font-semibold text-right">Actions</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-surface-container-high">
                            <asp:Repeater ID="rptMeetings" runat="server" OnItemCommand="rptMeetings_ItemCommand">
                                <ItemTemplate>
                                    <tr class="hover:bg-surface-container-lowest transition-colors group">
                                        <td class="px-6 py-4">
                                            <span class="font-semibold text-on-surface"><%# Eval("MeetingTitle") %></span>
                                            <div class="flex gap-1 mt-1">
                                                <%# GetZoomIndicator(Eval("ZoomJoinUrl"), Eval("RecordingCount")) %>
                                            </div>
                                        </td>
                                        <td class="px-6 py-4 text-on-surface-variant"><%# Eval("MeetingType") %></td>
                                        <td class="px-6 py-4 whitespace-nowrap text-on-surface-variant"><%# Eval("ScheduledDate", "{0:MMM dd, yyyy - hh:mm tt}") %></td>
                                        <td class="px-6 py-4 text-on-surface-variant"><%# Eval("Venue") %></td>
                                        <td class="px-6 py-4">
                                            <span class='<%# GetStatusBadgeClass(Eval("Status").ToString()) %>'>
                                                <%# Eval("Status") %>
                                            </span>
                                        </td>
                                        <td class="px-6 py-4 text-right">
                                            <asp:HyperLink ID="lnkView" runat="server" NavigateUrl='<%# "~/Modules/Meetings/MeetingDetails.aspx?MeetingID=" + Eval("MeetingID") %>' CssClass="material-symbols-outlined text-outline hover:text-primary transition-colors" ToolTip="View Details">visibility</asp:HyperLink>
                                            <asp:LinkButton ID="btnArchive" runat="server" CommandName="Archive" CommandArgument='<%# Eval("MeetingID") %>' CssClass="material-symbols-outlined text-outline hover:text-error transition-colors ml-2" ToolTip="Archive">archive</asp:LinkButton>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                                <FooterTemplate>
                                    <asp:Panel ID="pnlNoResults" runat="server" Visible='<%# rptMeetings.Items.Count == 0 %>'>
                                        <tr>
                                            <td colspan="6" class="px-6 py-12 text-center">
                                                <span class="material-symbols-outlined text-4xl text-outline mb-2 block">groups</span>
                                                <p class="font-body-md text-on-surface-variant">No meetings found.</p>
                                            </td>
                                        </tr>
                                    </asp:Panel>
                                </FooterTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>
            </section>

        </main>
    </div>

</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="<%= ResolveUrl("~/Assets/js/dashboard.js") %>"></script>
</asp:Content>
