<%@ Page Title="My Events | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MyEvents.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Events.MyEvents" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <uc:UserTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:UserSidebar ActivePage="MyEvents" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            <header class="flex justify-between items-end mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">My events</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">
                        Request to join public events, accept invitations, or enter an invitation code. Students and faculty can create events.
                    </p>
                </div>
                <asp:HyperLink ID="lnkCreate" runat="server" Visible="false" NavigateUrl="~/Modules/Events/CreateEvent.aspx"
                    CssClass="py-2.5 px-6 bg-primary text-on-primary rounded-xl font-label-md font-bold">Create event</asp:HyperLink>
            </header>

            <asp:Label ID="lblMessage" runat="server" CssClass="dtas-notice"></asp:Label>

            <section class="standard-card rounded-xl p-6 mb-6">
                <h3 class="font-title-lg text-title-lg text-primary mb-4">Join with invitation code</h3>
                <div class="flex flex-wrap gap-3 items-end">
                    <div class="flex-1 min-w-[220px]">
                        <asp:TextBox ID="txtCode" runat="server" MaxLength="50"
                            CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md"
                            placeholder="DTAS-EVT-8X29K"></asp:TextBox>
                    </div>
                    <asp:Button ID="btnJoinCode" runat="server" Text="Join" CssClass="btn-primary" OnClick="btnJoinCode_Click" />
                </div>
            </section>

            <section class="standard-card rounded-xl p-6 mb-6">
                <h3 class="font-title-lg text-title-lg text-primary mb-4">Pending invitations</h3>
                <asp:Repeater ID="rptInvites" runat="server" OnItemCommand="rptInvites_ItemCommand">
                    <ItemTemplate>
                        <div class="p-4 bg-surface-container-low rounded-lg mb-3 flex justify-between items-center gap-4">
                            <div>
                                <p class="font-semibold"><%# Eval("EventName") %></p>
                                <p class="text-xs text-on-surface-variant">Invited as <%# DigitalTransparencySystem.Helpers.EventService.DisplayRole(Convert.ToString(Eval("InviteRole"))) %></p>
                            </div>
                            <div class="flex gap-2">
                                <asp:LinkButton ID="btnAccept" runat="server" CommandName="Accept" CommandArgument='<%# Eval("InvitationID") %>'
                                    CssClass="px-3 py-1.5 bg-tertiary-container/30 text-on-tertiary-container rounded-lg font-label-md font-bold">Accept</asp:LinkButton>
                                <asp:LinkButton ID="btnDecline" runat="server" CommandName="Decline" CommandArgument='<%# Eval("InvitationID") %>'
                                    CssClass="px-3 py-1.5 bg-error-container/30 text-error rounded-lg font-label-md font-bold">Decline</asp:LinkButton>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlNoInvites" runat="server" Visible="false" CssClass="text-on-surface-variant">No pending invitations.</asp:Panel>
            </section>

            <section class="standard-card rounded-xl p-6 mb-6">
                <h3 class="font-title-lg text-title-lg text-primary mb-4">Events I belong to</h3>
                <asp:Repeater ID="rptMine" runat="server">
                    <ItemTemplate>
                        <asp:HyperLink runat="server"
                            NavigateUrl='<%# Convert.ToBoolean(Eval("IsDisabled")) ? "" : ResolveUrl("~/Modules/Events/EventWorkspace.aspx?EventID=" + Eval("EventID")) %>'
                            Enabled='<%# !Convert.ToBoolean(Eval("IsDisabled")) %>'
                            CssClass='<%# Convert.ToBoolean(Eval("IsDisabled"))
                                ? "block p-4 bg-surface-container-low rounded-lg mb-3 opacity-70 pointer-events-none cursor-not-allowed"
                                : "block p-4 bg-surface-container-low rounded-lg mb-3 hover:bg-surface-container-highest" %>'>
                            <div class="flex justify-between gap-4">
                                <div>
                                    <p class="font-semibold text-on-surface"><%# Eval("EventName") %></p>
                                    <p class="text-xs text-on-surface-variant"><%# Eval("EventType") %> - <%# Eval("Visibility") %> - <%# DigitalTransparencySystem.Helpers.EventService.DisplayRole(Convert.ToString(Eval("RoleInEvent"))) %></p>
                                </div>
                                <span class='<%# Convert.ToBoolean(Eval("IsDisabled")) ? "font-badge-cap text-badge-cap px-3 py-1 rounded-full bg-[rgba(198,40,40,0.12)] text-[#c62828]" : "font-badge-cap text-badge-cap" %>'><%# Convert.ToBoolean(Eval("IsDisabled")) ? "Restricted" : Eval("Status") %></span>
                            </div>
                        </asp:HyperLink>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlNoMine" runat="server" Visible="false" CssClass="text-on-surface-variant">You have not joined any events yet.</asp:Panel>
            </section>

            <section class="standard-card rounded-xl p-6">
                <h3 class="font-title-lg text-title-lg text-primary mb-4">Public events you can join</h3>
                <asp:Repeater ID="rptPublic" runat="server" OnItemCommand="rptPublic_ItemCommand">
                    <ItemTemplate>
                        <div class="p-4 bg-surface-container-low rounded-lg mb-3 flex justify-between items-center gap-4">
                            <div>
                                <p class="font-semibold"><%# Eval("EventName") %></p>
                                <p class="text-xs text-on-surface-variant"><%# Eval("EventType") %> - <%# Eval("Venue") %></p>
                            </div>
                            <asp:LinkButton ID="btnJoin" runat="server"
                                Visible='<%# Convert.ToInt32(Eval("HasJoinRequest")) == 0 && Convert.ToInt32(Eval("OpenToJoin")) == 1 %>'
                                CommandName="Join" CommandArgument='<%# Eval("EventID") %>'
                                CssClass="px-3 py-1.5 bg-primary text-on-primary rounded-lg font-label-md font-bold">Request to join</asp:LinkButton>
                            <span runat="server"
                                visible='<%# Convert.ToInt32(Eval("HasJoinRequest")) == 1 %>'
                                class="px-3 py-1.5 bg-surface-container-high text-on-surface-variant rounded-lg font-label-md">Request pending</span>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlNoPublic" runat="server" Visible="false" CssClass="text-on-surface-variant">No public events are open right now.</asp:Panel>
            </section>
        </main>
    </div>
</asp:Content>
