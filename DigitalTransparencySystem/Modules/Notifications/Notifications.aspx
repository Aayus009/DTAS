<%@ Page Title="Notifications | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Notifications.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Notifications.NotificationsPage" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <uc:AdminTopbar ID="adminTop" runat="server" Visible="false" />
    <uc:UserTopbar ID="userTop" runat="server" Visible="false" />

    <div class="flex min-h-screen">
        <uc:AdminSidebar ID="adminSide" ActivePage="Notifications" runat="server" Visible="false" />
        <uc:UserSidebar ID="userSide" ActivePage="Notifications" runat="server" Visible="false" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            <header class="flex justify-between items-center mb-8">
                <div>
                    <a href="<%= ResolveUrl("~/Modules/Dashboard/UsersDashboard.aspx") %>" class="inline-flex items-center gap-1 text-primary font-bold text-sm mb-3">
                        <span class="material-symbols-outlined text-[20px]">arrow_back</span>
                        Back
                    </a>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Notifications</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant">Unread count, filters, and mark as read. The bell in the top bar shows the same list.</p>
                </div>
                <div class="flex gap-3">
                    <asp:Button ID="btnMarkAllRead" runat="server" Text="Mark all as read" CssClass="btn-outline" OnClick="btnMarkAllRead_Click" />
                    <asp:Button ID="btnClearAll" runat="server" Text="Clear all" CssClass="btn-outline text-error border-error hover:bg-error-container/30" OnClick="btnClearAll_Click" />
                </div>
            </header>

            <div class="flex gap-2 mb-6 flex-wrap">
                <asp:Button ID="btnAll" runat="server" Text="All" CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-primary text-on-primary" OnClick="btnAll_Click" />
                <asp:Button ID="btnUnread" runat="server" Text="Unread" CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors" OnClick="btnUnread_Click" />
                <asp:Button ID="btnTasks" runat="server" Text="Tasks" CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors" OnClick="btnTasks_Click" />
                <asp:Button ID="btnAssignments" runat="server" Text="Assignments" CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors" OnClick="btnAssignments_Click" />
                <asp:Button ID="btnEvents" runat="server" Text="Events" CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors" OnClick="btnEvents_Click" />
                <asp:Button ID="btnClubs" runat="server" Text="Clubs" CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors" OnClick="btnClubs_Click" />
                <asp:Button ID="btnIdentity" runat="server" Text="Identity" CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors" OnClick="btnIdentity_Click" />
                <asp:Button ID="btnModeration" runat="server" Text="Moderation" CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors" OnClick="btnModeration_Click" />
            </div>

            <section class="standard-card rounded-xl overflow-hidden">
                <asp:Repeater ID="rptNotifications" runat="server" OnItemCommand="rptNotifications_ItemCommand">
                    <ItemTemplate>
                        <div class='px-6 py-4 border-b border-surface-container-high hover:bg-surface-container-low transition-colors <%# Eval("IsRead").ToString() == "False" ? "bg-primary-container/10" : "" %>'>
                            <div class="flex items-start gap-4">
                                <div class="h-10 w-10 shrink-0 rounded-full flex items-center justify-center bg-surface-container">
                                    <span class="material-symbols-outlined text-primary"><%# Eval("IconName") %></span>
                                </div>
                                <div class="flex-1 min-w-0">
                                    <div class="flex items-start justify-between gap-4">
                                        <div>
                                            <p class="font-label-md text-label-md text-on-surface <%# Eval("IsRead").ToString() == "False" ? "font-bold" : "" %>"><%# Eval("Title") %></p>
                                            <p class="text-body-md text-on-surface-variant mt-0.5"><%# Eval("Message") %></p>
                                        </div>
                                        <div class="flex items-center gap-2 shrink-0">
                                            <span class="text-badge-cap text-outline"><%# Eval("TimeAgo") %></span>
                                            <asp:Button ID="btnMarkRead" runat="server" CommandName="MarkRead" CommandArgument='<%# Eval("NotificationID") %>'
                                                CssClass="material-symbols-outlined text-[18px] text-outline hover:text-primary transition-colors" ToolTip="Mark as read"
                                                Text="mark_email_read" Visible='<%# Eval("IsRead").ToString() == "False" %>' />
                                            <asp:Button ID="btnDelete" runat="server" CommandName="Delete" CommandArgument='<%# Eval("NotificationID") %>'
                                                CssClass="material-symbols-outlined text-[18px] text-outline hover:text-error transition-colors" ToolTip="Delete"
                                                Text="delete" />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlNoNotifications" runat="server" Visible="false" CssClass="p-12 text-center">
                    <span class="material-symbols-outlined text-6xl text-outline-variant mb-4 block empty-state-icon">notifications_off</span>
                    <p class="font-title-lg text-title-lg text-on-surface-variant">No notifications</p>
                    <p class="text-body-md text-on-surface-variant mt-2">You're all caught up!</p>
                </asp:Panel>
            </section>
        </main>
    </div>
</asp:Content>
