<%@ Page Title="Club | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ClubWorkspace.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Clubs.ClubWorkspace" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>?v=notices1" />
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/clubs.css") %>?v=1" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <uc:AdminTopbar ID="adminTop" runat="server" Visible="false" />
    <uc:UserTopbar ID="userTop" runat="server" Visible="false" />

    <div class="flex min-h-screen">
        <uc:AdminSidebar ID="adminSide" ActivePage="Clubs" runat="server" Visible="false" />
        <uc:UserSidebar ID="userSide" ActivePage="Clubs" runat="server" Visible="false" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            <div class="club-page">
                <asp:Panel ID="pnlDenied" runat="server" Visible="false" CssClass="club-card p-8">
                    <p class="text-on-surface-variant">This club is private. You need an invitation or invitation code.</p>
                    <a class="club-back mt-4" href="<%= ClubsHomeUrl %>">Back to Clubs</a>
                </asp:Panel>

                <asp:Panel ID="pnlWorkspace" runat="server">
                    <header class="club-hero">
                        <div class="club-hero-main">
                            <asp:Image ID="imgClub" runat="server" Visible="false" CssClass="club-avatar" />
                            <asp:Panel ID="pnlAvatarFallback" runat="server" CssClass="club-avatar-fallback">
                                <asp:Literal ID="litAvatarInitial" runat="server"></asp:Literal>
                            </asp:Panel>
                            <div class="min-w-0">
                                <a href="<%= ClubsHomeUrl %>" class="club-back">
                                    <span class="material-symbols-outlined text-[20px]">arrow_back</span>
                                    Back
                                </a>
                                <h1 class="club-title"><asp:Literal ID="litName" runat="server"></asp:Literal></h1>
                                <div class="club-chips">
                                    <span class="club-chip">
                                        <span class="material-symbols-outlined">public</span>
                                        <asp:Literal ID="litVisibility" runat="server"></asp:Literal>
                                    </span>
                                    <span class="club-chip club-chip-lead">
                                        <span class="material-symbols-outlined">badge</span>
                                        <asp:Literal ID="litRoleChip" runat="server"></asp:Literal>
                                    </span>
                                </div>
                            </div>
                        </div>
                        <div class="club-actions">
                            <asp:Button ID="btnRestrict" runat="server" Text="Restrict club" Visible="false"
                                CssClass="py-2 px-5 border border-error text-error rounded-lg font-label-md font-bold hover:bg-error-container/30 cursor-pointer"
                                OnClick="btnRestrict_Click"
                                OnClientClick="return dtasConfirm(this, 'Restrict this club? Members will not be able to message, invite, join, or link it to events.');" />
                            <asp:Button ID="btnRestore" runat="server" Text="Restore club" Visible="false"
                                CssClass="btn-primary" OnClick="btnRestore_Click"
                                OnClientClick="return dtasConfirm(this, 'Restore this club so members can use it again?');" />
                            <asp:Button ID="btnJoinPublic" runat="server" Text="Request to join" CssClass="btn-primary" Visible="false" OnClick="btnJoinPublic_Click" />
                            <asp:Panel ID="pnlJoinPending" runat="server" Visible="false" CssClass="club-chip">Join request pending</asp:Panel>
                            <asp:Button ID="btnAccept" runat="server" Text="Accept invitation" CssClass="btn-primary" Visible="false" OnClick="btnAccept_Click" />
                            <asp:Button ID="btnLeave" runat="server" Text="Leave" CssClass="club-btn-ghost" Visible="false" OnClick="btnLeave_Click"
                                OnClientClick="return dtasConfirm(this, 'Leave this club?');" />
                        </div>
                    </header>

                    <asp:Panel ID="pnlRestricted" runat="server" Visible="false" CssClass="dtas-notice dtas-notice-danger dtas-notice-sticky dtas-notice-rich">
                        <span class="material-symbols-outlined dtas-notice-icon">block</span>
                        <div>
                            <p class="dtas-notice-kicker">Restricted</p>
                            <p class="dtas-notice-title">This club is restricted</p>
                            <p class="dtas-notice-copy">Messages, invites, joining, and linking to events are paused until the system administrator restores it.</p>
                        </div>
                    </asp:Panel>

                    <asp:Label ID="lblMessage" runat="server" CssClass="dtas-notice"></asp:Label>

                    <div class="club-grid">
                        <section class="club-card club-feed">
                            <div class="club-card-head">
                                <h3>Messages</h3>
                            </div>
                            <div class="club-messages">
                                <asp:Repeater ID="rptMessages" runat="server">
                                    <ItemTemplate>
                                        <div class='<%# MessageCss(Eval("MessageType")) %>'>
                                            <div class="club-msg-avatar"><%# Initials(Eval("FullName")) %></div>
                                            <div class="club-msg-body">
                                                <div class="club-msg-meta">
                                                    <strong><%# Eval("FullName") %></strong>
                                                    <span><%# Eval("CreatedAt", "{0:MMM dd HH:mm}") %></span>
                                                    <asp:Label runat="server" Visible='<%# IsUrgent(Eval("MessageType")) %>' CssClass="club-tag">Urgent</asp:Label>
                                                </div>
                                                <p class="text-on-surface m-0"><%# Eval("MessageContent") %></p>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <asp:Panel ID="pnlNoMessages" runat="server" Visible="false" CssClass="club-empty">
                                    <span class="material-symbols-outlined">forum</span>
                                    <p class="m-0 font-semibold">No messages yet</p>
                                    <p class="m-0 text-sm">Start the conversation with your club.</p>
                                </asp:Panel>
                            </div>
                            <asp:Panel ID="pnlCompose" runat="server" Visible="false" CssClass="club-compose">
                                <asp:TextBox ID="txtMessage" runat="server" TextMode="MultiLine" Rows="3"
                                    CssClass="club-input" placeholder="Write a message..."></asp:TextBox>
                                <div class="club-compose-bar">
                                    <asp:Button ID="btnSend" runat="server" Text="Send" CssClass="btn-primary" OnClick="btnSend_Click" />
                                    <asp:CheckBox ID="chkUrgent" runat="server" Visible="false" Text=" Urgent" CssClass="club-check" />
                                    <asp:CheckBox ID="chkAnnounce" runat="server" Visible="false" Text=" Announcement" CssClass="club-check" />
                                </div>
                            </asp:Panel>
                        </section>

                        <aside class="club-rail">
                            <asp:Panel ID="pnlClubId" runat="server" CssClass="club-card">
                                <span class="club-kicker">Club ID</span>
                                <p class="club-id-value"><asp:Literal ID="litClubId" runat="server"></asp:Literal></p>
                                <p class="club-hint">Use this ID when you create or propose an event so club members can be added directly.</p>
                                <asp:Panel ID="pnlCode" runat="server" Visible="false" CssClass="club-code-row">
                                    <div>
                                        <span class="club-kicker">Invitation code</span>
                                        <div class="club-code-value"><asp:Literal ID="litCode" runat="server"></asp:Literal></div>
                                    </div>
                                    <button type="button" class="club-icon-btn" title="Copy code" onclick="copyClubCode();">
                                        <span class="material-symbols-outlined">content_copy</span>
                                    </button>
                                </asp:Panel>
                                <asp:HiddenField ID="hfInviteCode" runat="server" />
                            </asp:Panel>

                            <asp:Panel ID="pnlInvite" runat="server" Visible="false" CssClass="club-card">
                                <h3 class="club-card-title mb-2">Invite by email</h3>
                                <p class="club-hint mb-3">Enter a complete email. The directory is not listed.</p>
                                <asp:TextBox ID="txtInviteEmail" runat="server" TextMode="Email" CssClass="club-input" placeholder="name@institution.edu"></asp:TextBox>
                                <div class="club-inline-actions">
                                    <asp:Button ID="btnLookup" runat="server" Text="Look up" CssClass="club-btn-ghost" OnClick="btnLookup_Click" />
                                    <asp:Button ID="btnInvite" runat="server" Text="Invite" CssClass="btn-primary" OnClick="btnInvite_Click" />
                                </div>
                                <asp:Label ID="lblLookup" runat="server" CssClass="font-label-md block mt-3"></asp:Label>
                            </asp:Panel>

                            <asp:Panel ID="pnlJoinRequests" runat="server" Visible="false" CssClass="club-card">
                                <div class="club-card-head" style="padding: 0 0 0.7rem;">
                                    <h3>Join requests</h3>
                                    <span class="club-count"><asp:Literal ID="litJoinCount" runat="server"></asp:Literal></span>
                                </div>
                                <p class="club-hint mb-2">Accept people who asked to join this public club.</p>
                                <asp:Repeater ID="rptJoinRequests" runat="server" OnItemCommand="rptJoinRequests_ItemCommand">
                                    <ItemTemplate>
                                        <div class="club-person">
                                            <div class="club-msg-avatar"><%# Initials(Eval("FullName")) %></div>
                                            <div class="club-person-copy">
                                                <p><%# Eval("FullName") %></p>
                                                <span><%# Eval("Email") %></span>
                                            </div>
                                            <div class="club-mini-actions">
                                                <asp:LinkButton runat="server" CommandName="AcceptJoin" CommandArgument='<%# Eval("UserID") %>' CssClass="club-pill club-pill-ok">Accept</asp:LinkButton>
                                                <asp:LinkButton runat="server" CommandName="DeclineJoin" CommandArgument='<%# Eval("UserID") %>' CssClass="club-pill club-pill-warn">Decline</asp:LinkButton>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </asp:Panel>

                            <section class="club-card">
                                <div class="club-card-head" style="padding: 0 0 0.7rem;">
                                    <h3>Members</h3>
                                    <span class="club-count"><asp:Literal ID="litMemberCount" runat="server"></asp:Literal></span>
                                </div>
                                <asp:Repeater ID="rptMembers" runat="server" OnItemCommand="rptMembers_ItemCommand">
                                    <ItemTemplate>
                                        <div class="club-person">
                                            <div class="club-msg-avatar"><%# Initials(Eval("FullName")) %></div>
                                            <div class="club-person-copy">
                                                <p><%# Eval("FullName") %></p>
                                                <span><%# Eval("Role") %></span>
                                            </div>
                                            <asp:LinkButton runat="server" Visible='<%# CanTransferLead(Eval("Role")) %>' CommandName="MakeLead" CommandArgument='<%# Eval("UserID") %>'
                                                CssClass="club-pill club-pill-soft">Make lead</asp:LinkButton>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </section>

                            <asp:Panel ID="pnlAnnouncements" runat="server" CssClass="club-card">
                                <h3 class="club-card-title mb-3">Announcements</h3>
                                <asp:Repeater ID="rptAnnouncements" runat="server">
                                    <ItemTemplate>
                                        <div class="club-announce">
                                            <p class="font-semibold m-0"><%# Eval("Title") %></p>
                                            <p class="text-sm text-on-surface-variant mt-1 mb-0"><%# Eval("Message") %></p>
                                            <p class="club-hint"><%# Eval("FullName") %> · <%# Eval("CreatedAt", "{0:MMM dd}") %></p>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </asp:Panel>
                        </aside>
                    </div>
                </asp:Panel>
            </div>
        </main>
    </div>
</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript">
        function copyClubCode() {
            var field = document.getElementById('<%= hfInviteCode.ClientID %>');
            if (!field || !field.value) return;
            if (navigator.clipboard && navigator.clipboard.writeText) {
                navigator.clipboard.writeText(field.value);
            }
        }
    </script>
</asp:Content>
