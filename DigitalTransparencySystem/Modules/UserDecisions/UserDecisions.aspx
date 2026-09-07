<%@ Page Title="My Decisions | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UserDecisions.aspx.cs" Inherits="DigitalTransparencySystem.Modules.UserDecisions.UserDecisions" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <uc:UserTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:UserSidebar ActivePage="Decisions" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            
            <header class="flex justify-between items-center mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">My Decisions</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant">Decisions you are responsible for or involved in.</p>
                </div>
            </header>

            <!-- Stats Row -->
            <div class="grid grid-cols-2 md:grid-cols-5 gap-4 mb-6">
                <div class="standard-card p-4 rounded-xl text-center">
                    <span class="font-badge-cap text-badge-cap text-outline uppercase block mb-1">Total</span>
                    <span class="font-headline-md text-headline-md text-primary stat-number"><asp:Literal ID="litTotal" runat="server" Text="0"></asp:Literal></span>
                </div>
                <div class="standard-card p-4 rounded-xl text-center">
                    <span class="font-badge-cap text-badge-cap text-outline uppercase block mb-1">Proposed</span>
                    <span class="font-headline-md text-headline-md stat-number" style="color: #d4e3ff"><asp:Literal ID="litProposed" runat="server" Text="0"></asp:Literal></span>
                </div>
                <div class="standard-card p-4 rounded-xl text-center">
                    <span class="font-badge-cap text-badge-cap text-outline uppercase block mb-1">Under Review</span>
                    <span class="font-headline-md text-headline-md stat-number" style="color: #01579b"><asp:Literal ID="litUnderReview" runat="server" Text="0"></asp:Literal></span>
                </div>
                <div class="standard-card p-4 rounded-xl text-center">
                    <span class="font-badge-cap text-badge-cap text-outline uppercase block mb-1">Approved</span>
                    <span class="font-headline-md text-headline-md stat-number" style="color: #005137"><asp:Literal ID="litApproved" runat="server" Text="0"></asp:Literal></span>
                </div>
                <div class="standard-card p-4 rounded-xl text-center">
                    <span class="font-badge-cap text-badge-cap text-outline uppercase block mb-1">Completed</span>
                    <span class="font-headline-md text-headline-md stat-number" style="color: #005137"><asp:Literal ID="litCompleted" runat="server" Text="0"></asp:Literal></span>
                </div>
            </div>

            <!-- Filter -->
            <div class="flex gap-2 mb-6 flex-wrap">
                <asp:Button ID="btnAll" runat="server" Text="All" CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-primary text-on-primary" OnClick="btnAll_Click" />
                <asp:Button ID="btnProposed" runat="server" Text="Proposed" CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors" OnClick="btnProposed_Click" />
                <asp:Button ID="btnUnderReview" runat="server" Text="Under Review" CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors" OnClick="btnUnderReview_Click" />
                <asp:Button ID="btnApproved" runat="server" Text="Approved" CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors" OnClick="btnApproved_Click" />
                <asp:Button ID="btnCompleted" runat="server" Text="Completed" CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors" OnClick="btnCompleted_Click" />
            </div>

            <!-- Decisions Table -->
            <section class="standard-card rounded-xl overflow-hidden">
                <div class="overflow-x-auto">
                    <table class="w-full text-left font-body-md">
                        <thead>
                            <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                <th class="px-6 py-3 font-semibold">Decision</th>
                                <th class="px-6 py-3 font-semibold">Event</th>
                                <th class="px-6 py-3 font-semibold">Priority</th>
                                <th class="px-6 py-3 font-semibold">Status</th>
                                <th class="px-6 py-3 font-semibold">Due Date</th>
                                <th class="px-6 py-3 font-semibold text-right">Actions</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-surface-container-high">
                            <asp:Repeater ID="rptDecisions" runat="server">
                                <ItemTemplate>
                                    <tr class="hover:bg-surface-container-lowest transition-colors">
                                        <td class="px-6 py-4">
                                            <p class="font-semibold"><%# Eval("DecisionTitle") %></p>
                                            <p class="text-xs text-on-surface-variant truncate max-w-xs"><%# Eval("Description") %></p>
                                        </td>
                                        <td class="px-6 py-4 text-on-surface-variant"><%# Eval("EventName") %></td>
                                        <td class="px-6 py-4">
                                            <span class='<%# "badge-status-" + Eval("Priority").ToString().ToLower() %>'><%# Eval("Priority") %></span>
                                        </td>
                                        <td class="px-6 py-4">
                                            <span class='<%# "badge-status-" + Eval("Status").ToString().ToLower().Replace(" ", "") %>'><%# Eval("Status") %></span>
                                        </td>
                                        <td class="px-6 py-4 whitespace-nowrap"><%# Eval("DueDate", "{0:MMM dd, yyyy}") %></td>
                                        <td class="px-6 py-4 text-right">
                                            <asp:HyperLink ID="lnkDetails" runat="server" NavigateUrl='<%# "~/Modules/Decisions/DecisionDetails.aspx?DecisionID=" + Eval("DecisionID") %>'
                                                CssClass="inline-flex items-center gap-1 px-3 py-1.5 bg-primary-container text-on-primary-container rounded-lg text-label-md hover:bg-primary-container/80 transition-colors">
                                                <span class="material-symbols-outlined text-[16px]">visibility</span> View
                                            </asp:HyperLink>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>
                <asp:Panel ID="pnlNoDecisions" runat="server" Visible="false" CssClass="p-12 text-center">
                    <span class="material-symbols-outlined text-6xl text-outline-variant mb-4 block empty-state-icon">gavel</span>
                    <p class="font-title-lg text-title-lg text-on-surface-variant">No decisions found</p>
                    <p class="text-body-md text-on-surface-variant mt-2">You don't have any decisions matching this filter.</p>
                </asp:Panel>
            </section>

        </main>
    </div>
</asp:Content>
