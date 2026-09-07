<%@ Page Title="Connect | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ConnectRoom.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Connect.ConnectRoom" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>?v=connect-wh3" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <uc:UserTopbar runat="server" />

    <div class="connect-shell">
        <uc:UserSidebar ActivePage="Connect" runat="server" />

        <main class="dashboard-main connect-main">
            <asp:Panel ID="pnlDenied" runat="server" Visible="false" CssClass="connect-app">
                <div class="connect-empty-chat">
                    <a href="<%= ResolveUrl("~/Modules/Connect/Connect.aspx") %>" class="connect-icon-btn" title="Back"><span class="material-symbols-outlined">arrow_back</span></a>
                    <p>You do not have access to this group.</p>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlRoom" runat="server" CssClass="connect-app">
                <aside class="connect-threads">
                    <div class="connect-threads-head">
                        <a href="<%= ResolveUrl("~/Modules/Connect/Connect.aspx?filter=" + Filter) %>" class="connect-icon-btn" title="Back">
                            <span class="material-symbols-outlined">arrow_back</span>
                        </a>
                        <h1>Connect</h1>
                    </div>
                    <div class="connect-filters">
                        <a class="<%= FilterClass("all") %>" href="<%= FilterUrl("all") %>">All messages</a>
                        <a class="<%= FilterClass("unread") %>" href="<%= FilterUrl("unread") %>">Unread</a>
                        <a class="<%= FilterClass("archive") %>" href="<%= FilterUrl("archive") %>">Archive</a>
                    </div>
                    <div class="connect-thread-scroll">
                        <asp:Repeater ID="rptThreads" runat="server" OnItemCommand="rptThreads_ItemCommand">
                            <ItemTemplate>
                                <div class="connect-thread-row">
                                    <a class='<%# "connect-thread" + (Convert.ToInt32(Eval("GroupID")) == GroupID ? " is-active" : "") %>'
                                        href='<%# RoomUrl(Eval("GroupID")) %>'>
                                        <span class="connect-avatar"><%# AvatarMarkup(Eval("GroupName"), Eval("PhotoPath")) %></span>
                                        <span class="connect-thread-body">
                                            <span class="connect-thread-top">
                                                <strong><%# Eval("GroupName") %></strong>
                                                <time><%# TimeLabel(Eval("LastMessageAt")) %></time>
                                            </span>
                                            <span class="connect-thread-preview-row">
                                                <span class="connect-thread-preview"><%# Preview(Eval("LastPreview")) %></span>
                                                <%# UnreadBadge(Eval("UnreadCount")) %>
                                            </span>
                                        </span>
                                    </a>
                                    <asp:LinkButton ID="btnArchiveThread" runat="server" CausesValidation="false"
                                        CssClass="connect-thread-archive connect-icon-btn"
                                        CommandName='<%# IsArchiveView ? "Unarchive" : "Archive" %>'
                                        CommandArgument='<%# Eval("GroupID") %>'
                                        ToolTip='<%# IsArchiveView ? "Unarchive" : "Archive" %>'>
                                        <span class="material-symbols-outlined"><%# IsArchiveView ? "unarchive" : "archive" %></span>
                                    </asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </aside>

                <section class="connect-chat">
                    <header class="connect-chat-head">
                        <span class="connect-avatar">
                            <asp:PlaceHolder ID="phPhoto" runat="server" Visible="false">
                                <asp:Image ID="imgGroupPhoto" runat="server" AlternateText="" />
                            </asp:PlaceHolder>
                            <asp:Literal ID="litInitial" runat="server"></asp:Literal>
                        </span>
                        <div class="connect-chat-title">
                            <strong><asp:Literal ID="litName" runat="server"></asp:Literal></strong>
                            <span><asp:Literal ID="litMemberCount" runat="server"></asp:Literal> members</span>
                        </div>
                        <button type="button" class="connect-icon-btn" id="btnToggleMembers" title="Members">
                            <span class="material-symbols-outlined">group</span>
                        </button>
                        <button type="button" class="connect-icon-btn" id="btnToggleMenu" title="More">
                            <span class="material-symbols-outlined">more_horiz</span>
                        </button>
                        <div id="connectMenu" class="connect-menu hidden">
                            <button type="button" id="btnShowEdit">Edit group</button>
                            <asp:LinkButton ID="btnArchive" runat="server" OnClick="btnArchive_Click" CausesValidation="false">
                                <asp:Literal ID="litArchiveAction" runat="server" Text="Archive chat"></asp:Literal>
                            </asp:LinkButton>
                            <button type="button" id="btnCopyCode" data-code="">Copy invite code</button>
                            <asp:LinkButton ID="btnLeave" runat="server" OnClick="btnLeave_Click"
                                OnClientClick="return dtasConfirm(this, 'Leave this Connect group?');">Leave group</asp:LinkButton>
                        </div>
                    </header>

                    <div id="connectFeed" class="connect-feed" data-group-id="<%= GroupID %>" data-api="<%= ResolveUrl("~/Modules/Connect/ConnectApi.ashx") %>" data-user-id="<%= CurrentUserID %>"></div>

                    <div class="connect-composer-dock">
                        <div class="connect-composer">
                            <div id="connectEmojiBar" class="connect-emoji-pop hidden"></div>
                            <div id="connectTypeMenu" class="connect-menu connect-type-menu hidden">
                                <button type="button" class="connect-type-opt is-active" data-type="Message">Message</button>
                                <button type="button" class="connect-type-opt" data-type="Alert">Alert</button>
                                <button type="button" class="connect-type-opt" data-type="Important">Important</button>
                                <button type="button" class="connect-type-opt" data-type="Announcement">Announcement</button>
                            </div>
                            <button type="button" id="btnTypeToggle" class="connect-icon-btn" title="Message type">
                                <span class="material-symbols-outlined">campaign</span>
                            </button>
                            <button type="button" id="btnEmojiToggle" class="connect-icon-btn" title="Emoji">
                                <span class="material-symbols-outlined">mood</span>
                            </button>
                            <label class="connect-icon-btn" title="Photo">
                                <span class="material-symbols-outlined">image</span>
                                <input type="file" id="connectImage" accept=".jpg,.jpeg,.png,.gif,.webp" class="sr-only" />
                            </label>
                            <textarea id="connectInput" rows="1" maxlength="2000" placeholder="Message..."></textarea>
                            <button type="button" id="btnConnectSend" class="connect-send" title="Send">
                                <span class="material-symbols-outlined">send</span>
                            </button>
                        </div>
                        <p id="connectImageName" class="connect-attach-name"></p>
                        <p id="connectStatus" class="connect-status"></p>
                    </div>

                    <div id="connectMembersBackdrop" class="connect-pop-backdrop<%= MembersOpen ? "" : " hidden" %>"></div>
                    <aside id="connectDrawer" class="connect-members-pop<%= MembersOpen ? "" : " hidden" %>">
                        <div class="connect-drawer-head">
                            <h3>Members</h3>
                            <button type="button" class="connect-icon-btn" id="btnCloseMembers" title="Close">
                                <span class="material-symbols-outlined">close</span>
                            </button>
                        </div>
                        <div class="connect-drawer-block">
                            <span class="connect-kicker">Invite code</span>
                            <div class="connect-code-row">
                                <strong id="litCodeText"><asp:Literal ID="litCode" runat="server"></asp:Literal></strong>
                                <button type="button" class="connect-icon-btn" id="btnCopyCode2" title="Copy">
                                    <span class="material-symbols-outlined">content_copy</span>
                                </button>
                            </div>
                        </div>
                        <asp:Panel ID="pnlAddMember" runat="server" CssClass="connect-drawer-block">
                            <span class="connect-kicker">Add member</span>
                            <asp:TextBox ID="txtInviteEmail" runat="server" TextMode="Email" CssClass="connect-field" placeholder="Email"></asp:TextBox>
                            <div class="connect-drawer-actions">
                                <asp:Button ID="btnLookup" runat="server" Text="Look up" CssClass="connect-ghost" OnClick="btnLookup_Click" />
                                <asp:Button ID="btnAdd" runat="server" Text="Add" CssClass="connect-popup-submit" OnClick="btnAdd_Click" />
                            </div>
                            <asp:Label ID="lblLookup" runat="server" CssClass="connect-inline-msg"></asp:Label>
                        </asp:Panel>
                        <div class="connect-drawer-block">
                            <span class="connect-kicker">People</span>
                            <asp:Repeater ID="rptMembers" runat="server">
                                <ItemTemplate>
                                    <div class="connect-member">
                                        <span class="connect-avatar sm"><%# Initial(Eval("FullName")) %></span>
                                        <span>
                                            <strong><%# Eval("FullName") %></strong>
                                            <em><%# Eval("Role") %></em>
                                        </span>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </aside>
                </section>
            </asp:Panel>

            <asp:Panel ID="pnlEditGroup" runat="server" CssClass="connect-overlay hidden" ClientIDMode="Static">
                <div class="connect-popup">
                    <div class="connect-popup-head">
                        <h3>Edit group</h3>
                        <button type="button" class="connect-icon-btn" id="btnCloseEdit" title="Close">
                            <span class="material-symbols-outlined">close</span>
                        </button>
                    </div>
                    <asp:TextBox ID="txtEditName" runat="server" MaxLength="200" CssClass="connect-field" placeholder="Group name"></asp:TextBox>
                    <label class="connect-kicker">Group photo</label>
                    <asp:FileUpload ID="fuGroupPhoto" runat="server" CssClass="connect-field" accept=".jpg,.jpeg,.png,.gif,.webp" />
                    <asp:Label ID="lblEditGroup" runat="server" CssClass="connect-inline-msg"></asp:Label>
                    <asp:Button ID="btnSaveGroup" runat="server" Text="Save" CssClass="connect-popup-submit" OnClick="btnSaveGroup_Click" />
                </div>
            </asp:Panel>
        </main>
    </div>
</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script src="<%= ResolveUrl("~/Assets/js/connect-room.js") %>"></script>
</asp:Content>
