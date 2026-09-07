<%@ Page Title="Connect | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Connect.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Connect.ConnectHome" %>
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
            <div class="connect-app">
                <aside class="connect-threads">
                    <div class="connect-threads-head">
                        <h1>Connect</h1>
                        <div class="connect-icon-row">
                            <asp:LinkButton ID="btnShowJoin" runat="server" CausesValidation="false" CssClass="connect-icon-btn" ToolTip="Join with code" OnClick="btnShowJoin_Click">
                                <span class="material-symbols-outlined">login</span>
                            </asp:LinkButton>
                            <asp:LinkButton ID="btnShowCreate" runat="server" CausesValidation="false" CssClass="connect-icon-btn" ToolTip="New group" OnClick="btnShowCreate_Click">
                                <span class="material-symbols-outlined">add</span>
                            </asp:LinkButton>
                        </div>
                    </div>
                    <div class="connect-filters">
                        <a class="<%= FilterClass("all") %>" href="<%= FilterUrl("all") %>">All messages</a>
                        <a class="<%= FilterClass("unread") %>" href="<%= FilterUrl("unread") %>">Unread</a>
                        <a class="<%= FilterClass("archive") %>" href="<%= FilterUrl("archive") %>">Archive</a>
                    </div>
                    <asp:Label ID="lblMessage" runat="server" CssClass="connect-inline-msg"></asp:Label>
                    <div class="connect-thread-scroll">
                        <asp:Repeater ID="rptGroups" runat="server" OnItemCommand="rptGroups_ItemCommand">
                            <ItemTemplate>
                                <div class="connect-thread-row">
                                    <a class="connect-thread" href='<%# RoomUrl(Eval("GroupID")) %>'>
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
                        <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="connect-empty-list">
                            <span class="material-symbols-outlined">chat_bubble</span>
                            <p><asp:Literal ID="litEmpty" runat="server" Text="No conversations yet"></asp:Literal></p>
                        </asp:Panel>
                    </div>
                </aside>

                <section class="connect-chat connect-chat-empty">
                    <div class="connect-empty-chat">
                        <span class="material-symbols-outlined">forum</span>
                        <h2>Your messages</h2>
                        <p>Pick a group or start a new one.</p>
                    </div>
                </section>
            </div>

            <asp:Panel ID="pnlCreate" runat="server" Visible="false" CssClass="connect-overlay">
                <div class="connect-popup">
                    <div class="connect-popup-head">
                        <h3>New group</h3>
                        <asp:LinkButton ID="btnCancelCreate" runat="server" CausesValidation="false" CssClass="connect-icon-btn" OnClick="btnCancelCreate_Click">
                            <span class="material-symbols-outlined">close</span>
                        </asp:LinkButton>
                    </div>
                    <asp:TextBox ID="txtName" runat="server" MaxLength="200" CssClass="connect-field" placeholder="Group name"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtName"
                        ValidationGroup="CreateConnect" CssClass="field-error" Display="Dynamic"
                        ErrorMessage="Group name is required."></asp:RequiredFieldValidator>
                    <asp:TextBox ID="txtDescription" runat="server" MaxLength="300" CssClass="connect-field" placeholder="Description (optional)"></asp:TextBox>
                    <label class="connect-kicker">Group photo (optional)</label>
                    <asp:FileUpload ID="fuCreatePhoto" runat="server" CssClass="connect-field" accept=".jpg,.jpeg,.png,.gif,.webp" />
                    <asp:Button ID="btnCreate" runat="server" Text="Create" CssClass="connect-popup-submit" OnClick="btnCreate_Click" ValidationGroup="CreateConnect" />
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlJoin" runat="server" Visible="false" CssClass="connect-overlay">
                <div class="connect-popup">
                    <div class="connect-popup-head">
                        <h3>Join group</h3>
                        <asp:LinkButton ID="btnCancelJoin" runat="server" CausesValidation="false" CssClass="connect-icon-btn" OnClick="btnCancelJoin_Click">
                            <span class="material-symbols-outlined">close</span>
                        </asp:LinkButton>
                    </div>
                    <asp:TextBox ID="txtCode" runat="server" MaxLength="50" CssClass="connect-field" placeholder="DTAS-CNN-XXXXX"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvCode" runat="server" ControlToValidate="txtCode"
                        ValidationGroup="JoinConnect" CssClass="field-error" Display="Dynamic"
                        ErrorMessage="Enter an invite code."></asp:RequiredFieldValidator>
                    <asp:Button ID="btnJoin" runat="server" Text="Join" CssClass="connect-popup-submit" OnClick="btnJoin_Click" ValidationGroup="JoinConnect" />
                </div>
            </asp:Panel>
        </main>
    </div>
</asp:Content>
