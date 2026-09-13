<%@ Page Title="Clubs | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MyClubs.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Clubs.MyClubs" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>?v=navfix2" />
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/clubs.css") %>?v=1" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <uc:UserTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:UserSidebar ActivePage="Clubs" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            <div class="club-page">
                <header class="club-hub-head">
                    <h1 class="club-title">Clubs</h1>
                    <p class="text-on-surface-variant max-w-2xl m-0">
                        Create a club, request to join a public one, or enter an invitation code. The club lead approves public join requests.
                    </p>
                </header>

                <asp:Label ID="lblMessage" runat="server" CssClass="dtas-notice"></asp:Label>

                <div class="club-hub-grid">
                    <asp:Panel ID="pnlCreate" runat="server" CssClass="club-card club-tile">
                        <h3 class="club-card-title mb-4">Create a club</h3>
                        <div class="space-y-4">
                            <asp:TextBox ID="txtName" runat="server" MaxLength="200"
                                CssClass="club-input" placeholder="Club name"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvClubName" runat="server" ControlToValidate="txtName"
                                ValidationGroup="CreateClub" CssClass="field-error" Display="Dynamic"
                                ErrorMessage="Club name is required."></asp:RequiredFieldValidator>
                            <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="3"
                                CssClass="club-input" placeholder="What is this club for?"></asp:TextBox>
                            <asp:CheckBox ID="chkPublic" runat="server" Text=" Public — others can discover and request to join" CssClass="club-check" />
                            <div>
                                <label class="club-kicker">Image (optional, JPG/PNG)</label>
                                <asp:FileUpload ID="fuImage" runat="server" />
                            </div>
                            <asp:Button ID="btnCreate" runat="server" Text="Create club" CssClass="btn-primary" OnClick="btnCreate_Click" ValidationGroup="CreateClub" />
                        </div>
                    </asp:Panel>

                    <section class="club-card club-tile">
                        <h3 class="club-card-title mb-3">Join with invitation code</h3>
                        <p class="club-hint mb-4">If a lead sent you a code, enter it here to join immediately.</p>
                        <div class="flex flex-wrap gap-3 items-end">
                            <asp:TextBox ID="txtCode" runat="server" MaxLength="50"
                                CssClass="club-input flex-1 min-w-[220px]"
                                placeholder="DTAS-CLB-8X29K"></asp:TextBox>
                            <asp:Button ID="btnJoinCode" runat="server" Text="Join" CssClass="btn-primary" OnClick="btnJoinCode_Click" ValidationGroup="JoinClub" />
                        </div>
                        <asp:RequiredFieldValidator ID="rfvClubCode" runat="server" ControlToValidate="txtCode"
                            ValidationGroup="JoinClub" CssClass="field-error" Display="Dynamic"
                            ErrorMessage="Enter an invitation code."></asp:RequiredFieldValidator>
                    </section>
                </div>

                <section class="club-card club-tile mb-4">
                    <h3 class="club-card-title mb-4">Pending invitations</h3>
                    <asp:Repeater ID="rptInvites" runat="server" OnItemCommand="rptInvites_ItemCommand">
                        <ItemTemplate>
                            <div class="club-list-card">
                                <p class="font-semibold m-0"><%# Eval("ClubName") %></p>
                                <div class="club-mini-actions">
                                    <asp:LinkButton runat="server" CommandName="Accept" CommandArgument='<%# Eval("InvitationID") %>'
                                        CssClass="club-pill club-pill-ok">Accept</asp:LinkButton>
                                    <asp:LinkButton runat="server" CommandName="Decline" CommandArgument='<%# Eval("InvitationID") %>'
                                        CssClass="club-pill club-pill-warn">Decline</asp:LinkButton>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <asp:Panel ID="pnlNoInvites" runat="server" Visible="false" CssClass="club-empty" style="padding: 1.5rem;">
                        <p class="m-0">No pending invitations.</p>
                    </asp:Panel>
                </section>

                <section class="club-card club-tile mb-4">
                    <h3 class="club-card-title mb-4">My clubs</h3>
                    <div class="club-list">
                        <asp:Repeater ID="rptMine" runat="server">
                            <ItemTemplate>
                                <a class="club-list-card"
                                    href='<%# ResolveUrl("~/Modules/Clubs/ClubWorkspace.aspx?ClubID=" + Eval("ClubID")) %>'>
                                    <div class="club-list-main">
                                        <div class="club-msg-avatar"><%# Initials(Eval("ClubName")) %></div>
                                        <div class="min-w-0">
                                            <p class="font-semibold text-on-surface m-0">
                                                <%# Eval("ClubName") %>
                                                <asp:Label runat="server" Visible='<%# Convert.ToBoolean(Eval("IsRestricted")) %>'
                                                    CssClass="ml-2 px-2 py-0.5 rounded-full font-badge-cap text-badge-cap inline-block bg-[rgba(198,40,40,0.12)] text-[#c62828]">Restricted</asp:Label>
                                            </p>
                                            <p class="club-hint">Club ID <%# Eval("ClubID") %> · <%# Eval("Role") %> · <%# Convert.ToBoolean(Eval("IsPublic")) ? "Public" : "Private" %></p>
                                            <p class="club-code-value" style="font-size: 0.78rem;"><%# Convert.ToBoolean(Eval("IsRestricted")) ? "" : Eval("InviteCode") %></p>
                                        </div>
                                    </div>
                                    <span class="material-symbols-outlined text-outline">chevron_right</span>
                                </a>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <asp:Panel ID="pnlNoMine" runat="server" Visible="false" CssClass="club-empty" style="padding: 1.5rem;">
                        <p class="m-0">You have not joined a club yet.</p>
                    </asp:Panel>
                </section>

                <section class="club-card club-tile">
                    <h3 class="club-card-title mb-4">Public clubs</h3>
                    <div class="club-list">
                        <asp:Repeater ID="rptPublic" runat="server" OnItemCommand="rptPublic_ItemCommand">
                            <ItemTemplate>
                                <div class="club-list-card">
                                    <div class="club-list-main">
                                        <div class="club-msg-avatar"><%# Initials(Eval("ClubName")) %></div>
                                        <div>
                                            <p class="font-semibold m-0"><%# Eval("ClubName") %></p>
                                            <p class="club-hint"><%# Eval("Description") %></p>
                                        </div>
                                    </div>
                                    <asp:LinkButton runat="server" CommandName="Join" CommandArgument='<%# Eval("ClubID") %>'
                                        Visible='<%# Convert.ToInt32(Eval("HasPendingRequest")) == 0 %>'
                                        CssClass="btn-primary">Request to join</asp:LinkButton>
                                    <asp:Label runat="server" Visible='<%# Convert.ToInt32(Eval("HasPendingRequest")) == 1 %>'
                                        CssClass="club-chip">Request pending</asp:Label>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <asp:Panel ID="pnlNoPublic" runat="server" Visible="false" CssClass="club-empty" style="padding: 1.5rem;">
                        <p class="m-0">No public clubs are open right now.</p>
                    </asp:Panel>
                </section>
            </div>
        </main>
    </div>
</asp:Content>
