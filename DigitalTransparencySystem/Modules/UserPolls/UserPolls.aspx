<%@ Page Title="Polls | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UserPolls.aspx.cs" Inherits="DigitalTransparencySystem.Modules.UserPolls.UserPolls" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <uc:UserTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:UserSidebar ActivePage="Polls" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            
            <header class="mb-8">
                <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Polls & Voting</h1>
                <p class="font-body-lg text-body-lg text-on-surface-variant">Participate in active polls and help make community decisions.</p>
            </header>

            <asp:Panel ID="pnlVoteMessage" runat="server" Visible="false" CssClass="mb-6 p-4 rounded-xl">
                <asp:Literal ID="litVoteMessage" runat="server"></asp:Literal>
            </asp:Panel>

            <!-- Active Polls -->
            <section class="mb-8">
                <h2 class="font-title-lg text-title-lg text-primary mb-4">Active Polls</h2>
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <asp:Repeater ID="rptActivePolls" runat="server" OnItemCommand="rptActivePolls_ItemCommand">
                        <ItemTemplate>
                            <div class="standard-card rounded-xl p-6">
                                <div class="flex justify-between items-start mb-4">
                                    <h3 class="font-title-lg text-title-lg text-primary"><%# Eval("PollTitle") %></h3>
                                    <span class="badge-status-completed">Active</span>
                                </div>
                                <p class="text-xs text-primary font-semibold mb-2"><%# Eval("ContextLabel") %></p>
                                <p class="text-body-md text-on-surface-variant mb-4"><%# Eval("Description") %></p>
                                <p class="text-xs text-outline mb-4">Ends: <%# Eval("EndDate", "{0:MMM dd, yyyy}") %></p>
                                
                                <asp:Panel ID="pnlVoteForm" runat="server" Visible='<%# !UserHasVoted(Eval("HasVoted")) %>'>
                                    <div class="space-y-3 mb-4">
                                        <asp:Repeater ID="rptOptions" runat="server" DataSource='<%# Eval("Options") %>'>
                                            <ItemTemplate>
                                                <label class="flex items-center gap-3 p-3 bg-surface-container-low rounded-lg hover:bg-surface-variant/50 transition-colors cursor-pointer border border-transparent hover:border-outline-variant">
                                                    <input type="radio" name="poll_<%# Eval("PollID") %>" value='<%# Eval("OptionID") %>' class="w-4 h-4 accent-primary" />
                                                    <span class="text-body-md text-on-surface"><%# Eval("OptionText") %></span>
                                                </label>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </div>
                                    <asp:Button ID="btnVote" runat="server" Text="Vote" CssClass="btn-primary w-full" CommandName="Vote" CommandArgument='<%# Eval("PollID") %>' CausesValidation="false" />
                                </asp:Panel>

                                <asp:Panel ID="pnlResults" runat="server" Visible='<%# UserHasVoted(Eval("HasVoted")) %>'>
                                    <div class="space-y-3">
                                        <asp:Repeater ID="rptResults" runat="server" DataSource='<%# Eval("Options") %>'>
                                            <ItemTemplate>
                                                <div>
                                                    <div class="flex justify-between text-label-md mb-1">
                                                        <span class="text-on-surface font-semibold"><%# Eval("OptionText") %></span>
                                                        <span class="text-on-surface-variant"><%# Eval("VoteCount") %> votes (<%# Eval("Percentage") %>%)</span>
                                                    </div>
                                                    <div class="w-full bg-surface-container-high h-2 rounded-full overflow-hidden">
                                                        <div class="bg-primary h-full rounded-full animate-progress" style='<%# "width: " + Eval("Percentage") + "%" %>'></div>
                                                    </div>
                                                </div>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </div>
                                    <p class="text-xs text-outline mt-4 text-center">You have already voted</p>
                                </asp:Panel>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                <asp:Panel ID="pnlNoActivePolls" runat="server" Visible="false" CssClass="text-center py-12">
                    <span class="material-symbols-outlined text-6xl text-outline-variant mb-4 block empty-state-icon">how_to_vote</span>
                    <p class="font-title-lg text-title-lg text-on-surface-variant">No active polls</p>
                    <p class="text-body-md text-on-surface-variant mt-2">There are no polls available for voting right now.</p>
                </asp:Panel>
            </section>

            <!-- Past Polls -->
            <section>
                <h2 class="font-title-lg text-title-lg text-primary mb-4">Past Polls</h2>
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <asp:Repeater ID="rptPastPolls" runat="server">
                        <ItemTemplate>
                            <div class="standard-card rounded-xl p-6 opacity-75">
                                <div class="flex justify-between items-start mb-4">
                                    <h3 class="font-title-lg text-title-lg text-on-surface"><%# Eval("PollTitle") %></h3>
                                    <span class="badge-status-cancelled">Closed</span>
                                </div>
                                <p class="text-body-md text-on-surface-variant mb-4"><%# Eval("Description") %></p>
                                <div class="space-y-3">
                                    <asp:Repeater ID="rptPastResults" runat="server" DataSource='<%# Eval("Options") %>'>
                                        <ItemTemplate>
                                            <div>
                                                <div class="flex justify-between text-label-md mb-1">
                                                    <span class="text-on-surface"><%# Eval("OptionText") %></span>
                                                    <span class="text-on-surface-variant"><%# Eval("VoteCount") %> votes</span>
                                                </div>
                                                <div class="w-full bg-surface-container-high h-2 rounded-full overflow-hidden">
                                                    <div class="bg-on-surface-variant h-full rounded-full" style='<%# "width: " + Eval("Percentage") + "%" %>'></div>
                                                </div>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </section>

        </main>
    </div>
</asp:Content>
