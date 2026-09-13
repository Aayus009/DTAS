<%@ Page Title="Polls | DTAS Portal" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Polls.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Polls.Polls" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <uc:AdminTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:AdminSidebar ActivePage="Polls" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            <header class="flex justify-between items-end mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Community Polls</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">Create a poll on a planned event or decision so the community can vote before work is finalized.</p>
                </div>
                <asp:Button ID="btnNewPoll" runat="server" Text="New Poll"
                    CssClass="px-6 py-3 bg-primary text-on-primary rounded-xl font-label-md text-label-md font-bold hover:scale-[1.02] active:scale-95 transition-transform cursor-pointer"
                    OnClick="btnNewPoll_Click" />
            </header>

            <asp:Panel ID="pnlMessage" runat="server" CssClass="dtas-notice" Visible="false">
                <asp:Label ID="lblMessage" runat="server"></asp:Label>
            </asp:Panel>

            <asp:Panel ID="pnlForm" runat="server" CssClass="standard-card rounded-xl p-6 mb-6" Visible="false">
                <h3 class="font-title-lg text-title-lg text-primary mb-6">Create Poll</h3>
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div class="md:col-span-2">
                        <label class="font-label-md text-on-surface-variant block mb-2">Question / Title <span class="text-error">*</span></label>
                        <asp:TextBox ID="txtTitle" runat="server" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md" placeholder="e.g. Which venue should Convocation use?"></asp:TextBox>
                    </div>
                    <div class="md:col-span-2">
                        <label class="font-label-md text-on-surface-variant block mb-2">Why this vote matters</label>
                        <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="3" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md resize-none" placeholder="Short public reason the community can see."></asp:TextBox>
                    </div>
                    <div>
                        <label class="font-label-md text-on-surface-variant block mb-2">Link to Event</label>
                        <asp:DropDownList ID="ddlEvent" runat="server" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md"></asp:DropDownList>
                    </div>
                    <div>
                        <label class="font-label-md text-on-surface-variant block mb-2">Link to Decision</label>
                        <asp:DropDownList ID="ddlDecision" runat="server" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md"></asp:DropDownList>
                    </div>
                    <div>
                        <label class="font-label-md text-on-surface-variant block mb-2">Closes on</label>
                        <asp:TextBox ID="txtEndDate" runat="server" TextMode="Date" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md"></asp:TextBox>
                    </div>
                </div>

                <div class="mt-6">
                    <label class="font-label-md text-on-surface-variant block mb-2">Options <span class="text-error">*</span> (at least 2)</label>
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                        <asp:TextBox ID="txtOption1" runat="server" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md" placeholder="Option 1"></asp:TextBox>
                        <asp:TextBox ID="txtOption2" runat="server" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md" placeholder="Option 2"></asp:TextBox>
                        <asp:TextBox ID="txtOption3" runat="server" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md" placeholder="Option 3 (optional)"></asp:TextBox>
                        <asp:TextBox ID="txtOption4" runat="server" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md" placeholder="Option 4 (optional)"></asp:TextBox>
                        <asp:TextBox ID="txtOption5" runat="server" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md" placeholder="Option 5 (optional)"></asp:TextBox>
                        <asp:TextBox ID="txtOption6" runat="server" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md" placeholder="Option 6 (optional)"></asp:TextBox>
                    </div>
                </div>

                <div class="flex items-center gap-4 pt-6 mt-6 border-t border-outline-variant/30">
                    <asp:Button ID="btnSavePoll" runat="server" Text="Publish Poll"
                        CssClass="px-8 py-3 bg-primary text-on-primary rounded-xl font-label-md text-label-md font-bold active:scale-95 transition-transform cursor-pointer"
                        OnClick="btnSavePoll_Click" />
                    <asp:Button ID="btnCancelPoll" runat="server" Text="Cancel"
                        CssClass="px-8 py-3 border border-outline text-on-surface-variant rounded-xl font-label-md text-label-md font-bold hover:bg-surface-container-low transition-colors cursor-pointer"
                        OnClick="btnCancelPoll_Click" CausesValidation="false" />
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlDetails" runat="server" Visible="false" CssClass="standard-card rounded-xl p-6 mb-6">
                <div class="flex justify-between items-start gap-4 mb-6">
                    <div>
                        <p class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest mb-2">Poll details</p>
                        <h3 class="font-title-lg text-title-lg text-primary"><asp:Literal ID="litDetailTitle" runat="server"></asp:Literal></h3>
                        <p class="text-body-md text-on-surface-variant mt-2"><asp:Literal ID="litDetailDescription" runat="server"></asp:Literal></p>
                    </div>
                    <asp:Button ID="btnHideDetails" runat="server" Text="Hide" CausesValidation="false"
                        CssClass="px-4 py-2 border border-outline text-on-surface-variant rounded-xl font-label-md text-label-md font-bold hover:bg-surface-container-low transition-colors cursor-pointer"
                        OnClick="btnHideDetails_Click" />
                </div>
                <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
                    <div>
                        <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-1">Linked to</span>
                        <span class="font-body-md text-on-surface"><asp:Literal ID="litDetailLinked" runat="server"></asp:Literal></span>
                    </div>
                    <div>
                        <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-1">Status</span>
                        <span class="font-body-md text-on-surface"><asp:Literal ID="litDetailStatus" runat="server"></asp:Literal></span>
                    </div>
                    <div>
                        <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-1">Closes</span>
                        <span class="font-body-md text-on-surface"><asp:Literal ID="litDetailCloses" runat="server"></asp:Literal></span>
                    </div>
                    <div>
                        <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-1">Total votes</span>
                        <span class="font-body-md text-on-surface"><asp:Literal ID="litDetailVotes" runat="server"></asp:Literal></span>
                    </div>
                </div>

                <h4 class="font-title-lg text-title-lg text-primary mb-4">Results</h4>
                <asp:Panel ID="pnlNoResults" runat="server" Visible="false" CssClass="text-on-surface-variant mb-6">
                    No votes yet.
                </asp:Panel>
                <div class="space-y-4 mb-8">
                    <asp:Repeater ID="rptResults" runat="server">
                        <ItemTemplate>
                            <div>
                                <div class="flex justify-between text-label-md mb-1">
                                    <span class="text-on-surface font-semibold"><%# Eval("OptionText") %><%# Convert.ToBoolean(Eval("IsLeading")) ? " · leading" : "" %></span>
                                    <span class="text-on-surface-variant"><%# Eval("VoteCount") %> votes (<%# Eval("Percentage") %>%)</span>
                                </div>
                                <div class="w-full bg-surface-container-high h-2 rounded-full overflow-hidden">
                                    <div class="bg-primary h-full rounded-full" style='<%# "width: " + Eval("Percentage") + "%" %>'></div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <h4 class="font-title-lg text-title-lg text-primary mb-4">Who voted</h4>
                <asp:Panel ID="pnlNoVoters" runat="server" Visible="false" CssClass="text-on-surface-variant">
                    No ballots recorded yet.
                </asp:Panel>
                <asp:Repeater ID="rptVoters" runat="server">
                    <HeaderTemplate>
                        <table class="w-full text-left font-body-md">
                            <thead>
                                <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                    <th class="px-4 py-3 font-semibold">Voter</th>
                                    <th class="px-4 py-3 font-semibold">Choice</th>
                                    <th class="px-4 py-3 font-semibold">Voted at</th>
                                </tr>
                            </thead>
                            <tbody class="divide-y divide-surface-container-high">
                    </HeaderTemplate>
                    <ItemTemplate>
                                <tr>
                                    <td class="px-4 py-3">
                                        <p class="font-semibold"><%# Eval("FullName") %></p>
                                        <p class="text-xs text-on-surface-variant"><%# Eval("Email") %></p>
                                    </td>
                                    <td class="px-4 py-3 text-on-surface-variant"><%# Eval("OptionText") %></td>
                                    <td class="px-4 py-3 whitespace-nowrap"><%# Eval("VotedAt", "{0:MMM dd, yyyy hh:mm tt}") %></td>
                                </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                            </tbody>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
            </asp:Panel>

            <section class="standard-card rounded-xl overflow-hidden">
                <div class="px-6 py-4 border-b border-surface-container-high">
                    <h3 class="font-title-lg text-title-lg text-primary">All Polls</h3>
                </div>
                <asp:Panel ID="pnlNoPolls" runat="server" Visible="false" CssClass="p-8 text-on-surface-variant">
                    No polls yet. Create one to collect community input on a planned event or decision.
                </asp:Panel>
                <asp:Repeater ID="rptPolls" runat="server" OnItemCommand="rptPolls_ItemCommand">
                    <HeaderTemplate>
                        <table class="w-full text-left font-body-md">
                            <thead>
                                <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                    <th class="px-6 py-3 font-semibold">Poll</th>
                                    <th class="px-6 py-3 font-semibold">Linked to</th>
                                    <th class="px-6 py-3 font-semibold">Votes</th>
                                    <th class="px-6 py-3 font-semibold">Closes</th>
                                    <th class="px-6 py-3 font-semibold">Status</th>
                                    <th class="px-6 py-3 font-semibold text-right">Actions</th>
                                </tr>
                            </thead>
                            <tbody class="divide-y divide-surface-container-high">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td class="px-6 py-4">
                                <p class="font-semibold"><%# Eval("PollTitle") %></p>
                                <p class="text-xs text-on-surface-variant truncate max-w-[280px]"><%# Eval("Description") %></p>
                            </td>
                            <td class="px-6 py-4 text-on-surface-variant"><%# Eval("LinkedTo") %></td>
                            <td class="px-6 py-4"><%# Eval("VoteCount") %></td>
                            <td class="px-6 py-4 whitespace-nowrap"><%# Eval("EndDate") == DBNull.Value ? "Open" : Eval("EndDate", "{0:MMM dd, yyyy}") %></td>
                            <td class="px-6 py-4"><%# Eval("StatusLabel") %></td>
                            <td class="px-6 py-4 text-right whitespace-nowrap">
                                <asp:LinkButton ID="btnView" runat="server" CommandName="View" CommandArgument='<%# Eval("PollID") %>'
                                    CssClass="text-primary font-label-md text-label-md font-bold hover:underline">View results</asp:LinkButton>
                                <span class="text-outline mx-2">·</span>
                                <asp:LinkButton ID="btnToggle" runat="server"
                                    CommandName='<%# Convert.ToBoolean(Eval("IsOpen")) ? "Close" : "Reopen" %>'
                                    CommandArgument='<%# Eval("PollID") %>'
                                    CssClass="text-primary font-label-md text-label-md font-bold hover:underline">
                                    <%# Convert.ToBoolean(Eval("IsOpen")) ? "Close" : "Reopen" %>
                                </asp:LinkButton>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                            </tbody>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
            </section>
        </main>
    </div>
</asp:Content>
