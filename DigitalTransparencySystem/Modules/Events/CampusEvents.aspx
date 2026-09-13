<%@ Page Title="Campus Events | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CampusEvents.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Events.CampusEvents" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <uc:UserTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:UserSidebar ActivePage="CampusEvents" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            <header class="flex justify-between items-end mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Campus events</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">
                        Public events are visible to everyone. Request to join and wait for the event lead or manager to accept you. Private events stay members-only.
                    </p>
                </div>
                <asp:HyperLink ID="lnkCreate" runat="server" Visible="false" NavigateUrl="~/Modules/Events/CreateEvent.aspx"
                    CssClass="py-2.5 px-6 bg-primary text-on-primary rounded-xl font-label-md font-bold">Create event</asp:HyperLink>
            </header>

            <asp:Label ID="lblMessage" runat="server" CssClass="dtas-notice"></asp:Label>

            <section class="standard-card rounded-xl overflow-hidden">
                <div class="p-6">
                    <asp:Repeater ID="rptCampusEvents" runat="server" OnItemCommand="rptCampusEvents_ItemCommand">
                        <ItemTemplate>
                            <article class="p-5 mb-4 last:mb-0 rounded-xl border border-outline-variant/40 bg-surface-container-low">
                                <div class="flex flex-wrap items-start justify-between gap-3 mb-3">
                                    <div class="min-w-0">
                                        <div class="flex flex-wrap items-center gap-2 mb-1">
                                            <h4 class="font-title-md text-title-md text-on-surface"><%# Eval("EventNameDisplay") %></h4>
                                            <span class='<%# Convert.ToInt32(Eval("IsPublic")) == 1
                                                ? "px-2.5 py-0.5 rounded-full font-badge-cap text-badge-cap bg-[rgba(46,125,50,0.12)] text-[#2e7d32]"
                                                : "px-2.5 py-0.5 rounded-full font-badge-cap text-badge-cap bg-surface-container-high text-on-surface-variant" %>'><%# Eval("Visibility") %></span>
                                            <span class="px-2.5 py-0.5 rounded-full font-badge-cap text-badge-cap bg-primary-container text-on-primary-container"><%# Eval("Status") %></span>
                                        </div>
                                        <p class="text-sm text-on-surface-variant"><%# Eval("DescriptionGist") %></p>
                                    </div>
                                    <div class="flex flex-wrap gap-2 shrink-0">
                                        <asp:HyperLink runat="server"
                                            Visible='<%# Convert.ToInt32(Eval("IsMember")) == 1 %>'
                                            NavigateUrl='<%# ResolveUrl("~/Modules/Events/EventWorkspace.aspx?EventID=" + Eval("EventID")) %>'
                                            CssClass="px-4 py-2 bg-primary text-on-primary rounded-lg font-label-md font-bold">Open workplace</asp:HyperLink>
                                        <asp:HyperLink runat="server"
                                            Visible='<%# Convert.ToInt32(Eval("IsMember")) == 0 %>'
                                            NavigateUrl='<%# ResolveUrl("~/Modules/Events/EventWorkspace.aspx?EventID=" + Eval("EventID")) %>'
                                            CssClass="px-4 py-2 border border-outline rounded-lg font-label-md font-bold">View details</asp:HyperLink>
                                        <asp:LinkButton runat="server"
                                            Visible='<%# Convert.ToInt32(Eval("CanRequestJoin")) == 1 %>'
                                            CommandName="JoinPublic" CommandArgument='<%# Eval("EventID") %>'
                                            CssClass="px-4 py-2 bg-primary text-on-primary rounded-lg font-label-md font-bold">Request to join</asp:LinkButton>
                                        <span runat="server"
                                            visible='<%# Convert.ToInt32(Eval("IsMember")) == 0 && Convert.ToInt32(Eval("HasJoinRequest")) == 1 %>'
                                            class="px-4 py-2 bg-surface-container-high text-on-surface-variant rounded-lg font-label-md">Request pending</span>
                                    </div>
                                </div>
                                <p class="text-xs text-on-surface-variant mb-3">
                                    <%# Eval("WhenWhere") %>
                                </p>
                                <div class="mb-3">
                                    <div class="flex justify-between text-xs text-on-surface-variant mb-1">
                                        <span>Team progress</span>
                                        <span><%# Eval("ProgressLabel") %></span>
                                    </div>
                                    <div class="h-2 bg-surface-container-high rounded-full overflow-hidden">
                                        <div class="h-full bg-[#2e7d32] rounded-full" style='width: <%# Eval("ProgressPercent") %>%'></div>
                                    </div>
                                </div>
                                <div class="grid grid-cols-1 md:grid-cols-2 gap-3 text-sm">
                                    <p><span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest">Leading</span><br />
                                        <span class="font-semibold text-on-surface"><%# Eval("LeadDisplay") %></span></p>
                                    <p><span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest">Handling</span><br />
                                        <span class="font-semibold text-on-surface"><%# Eval("HandlerDisplay") %></span></p>
                                </div>
                                <p class="text-xs text-outline mt-3"><%# Eval("MemberCount") %> on the team<%# Convert.ToInt32(Eval("IsMember")) == 1 ? " · You are a member" : "" %></p>
                            </article>
                        </ItemTemplate>
                    </asp:Repeater>
                    <asp:Panel ID="pnlNoCampusEvents" runat="server" Visible="false" CssClass="p-6 text-center">
                        <span class="material-symbols-outlined text-5xl text-outline-variant mb-3 block">event</span>
                        <p class="font-title-lg text-title-lg text-on-surface-variant">No events to show</p>
                        <p class="text-body-md text-on-surface-variant mt-2">Public campus events will appear here. Private events show only after you join.</p>
                    </asp:Panel>
                </div>
            </section>
        </main>
    </div>
</asp:Content>
