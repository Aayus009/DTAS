<%@ Page Title="Decisions | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="Decisions.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Decisions.Decisions" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <uc:AdminTopbar runat="server" />

    <div class="flex min-h-screen">
        
        <uc:AdminSidebar ActivePage="Decisions" runat="server" />

        <!-- Main Content Area -->
        <main class="dashboard-main flex-1 ml-64 p-8">
            
            <!-- Header Section -->
            <header class="flex justify-between items-end mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Decisions Management</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">Review and restrict decisions. Responsible people and event managers edit their own decisions.</p>
                </div>
            </header>

            <!-- Filters Row -->
            <div class="standard-card rounded-xl p-6 mb-6">
                <div class="flex flex-col md:flex-row gap-4 items-end">
                    <div class="flex-1 w-full">
                        <label class="font-badge-cap text-badge-cap text-on-surface-variant uppercase tracking-widest block mb-2">Search</label>
                        <div class="relative">
                            <span class="absolute left-3 top-1/2 -translate-y-1/2 material-symbols-outlined text-outline">search</span>
                            <asp:TextBox ID="txtSearch" runat="server" CssClass="pl-10 pr-4 py-2.5 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md w-full focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="Search decisions by title..." />
                        </div>
                    </div>
                    <div class="w-full md:w-48">
                        <label class="font-badge-cap text-badge-cap text-on-surface-variant uppercase tracking-widest block mb-2">Status</label>
                        <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="w-full py-2.5 px-4 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none cursor-pointer">
                            <asp:ListItem Text="All Statuses" Value="" />
                            <asp:ListItem Text="Proposed" Value="Proposed" />
                            <asp:ListItem Text="Under Review" Value="UnderReview" />
                            <asp:ListItem Text="Approved" Value="Approved" />
                            <asp:ListItem Text="Rejected" Value="Rejected" />
                            <asp:ListItem Text="Completed" Value="Completed" />
                        </asp:DropDownList>
                    </div>
                    <div class="w-full md:w-48">
                        <label class="font-badge-cap text-badge-cap text-on-surface-variant uppercase tracking-widest block mb-2">Priority</label>
                        <asp:DropDownList ID="ddlPriorityFilter" runat="server" CssClass="w-full py-2.5 px-4 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none cursor-pointer">
                            <asp:ListItem Text="All Priorities" Value="" />
                            <asp:ListItem Text="Critical" Value="Critical" />
                            <asp:ListItem Text="High" Value="High" />
                            <asp:ListItem Text="Medium" Value="Medium" />
                            <asp:ListItem Text="Low" Value="Low" />
                        </asp:DropDownList>
                    </div>
                    <div class="w-full md:w-auto">
                        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="w-full md:w-auto px-6 py-2.5 bg-primary text-on-primary rounded-lg font-label-md text-label-md font-bold hover:scale-[1.02] active:scale-95 transition-transform cursor-pointer" OnClick="btnSearch_Click" />
                    </div>
                    <div class="w-full md:w-auto">
                        <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="w-full md:w-auto px-6 py-2.5 border border-outline text-on-surface-variant rounded-lg font-label-md text-label-md font-bold hover:bg-surface-variant/50 transition-colors cursor-pointer" OnClick="btnReset_Click" />
                    </div>
                </div>
            </div>

            <!-- Stats Summary -->
            <div class="grid grid-cols-1 md:grid-cols-5 gap-4 mb-6">
                <div class="standard-card p-5 rounded-xl flex items-center gap-4">
                    <div class="h-12 w-12 rounded-lg bg-primary-container flex items-center justify-center">
                        <span class="material-symbols-outlined text-on-primary-container">gavel</span>
                    </div>
                    <div>
                        <p class="font-badge-cap text-badge-cap text-outline uppercase tracking-widest">Total</p>
                        <asp:Literal ID="litTotalDecisions" runat="server" Text="0"></asp:Literal>
                    </div>
                </div>
                <div class="standard-card p-5 rounded-xl flex items-center gap-4">
                    <div class="h-12 w-12 rounded-lg bg-[rgba(1,87,155,0.1)] flex items-center justify-center">
                        <span class="material-symbols-outlined text-[#01579b]">new_releases</span>
                    </div>
                    <div>
                        <p class="font-badge-cap text-badge-cap text-outline uppercase tracking-widest">Proposed</p>
                        <asp:Literal ID="litProposed" runat="server" Text="0"></asp:Literal>
                    </div>
                </div>
                <div class="standard-card p-5 rounded-xl flex items-center gap-4">
                    <div class="h-12 w-12 rounded-lg bg-[rgba(230,126,0,0.1)] flex items-center justify-center">
                        <span class="material-symbols-outlined text-[#e67e00]">pending</span>
                    </div>
                    <div>
                        <p class="font-badge-cap text-badge-cap text-outline uppercase tracking-widest">Under Review</p>
                        <asp:Literal ID="litUnderReview" runat="server" Text="0"></asp:Literal>
                    </div>
                </div>
                <div class="standard-card p-5 rounded-xl flex items-center gap-4">
                    <div class="h-12 w-12 rounded-lg bg-[rgba(26,94,48,0.1)] flex items-center justify-center">
                        <span class="material-symbols-outlined text-[#1a5e30]">check_circle</span>
                    </div>
                    <div>
                        <p class="font-badge-cap text-badge-cap text-outline uppercase tracking-widest">Approved</p>
                        <asp:Literal ID="litApproved" runat="server" Text="0"></asp:Literal>
                    </div>
                </div>
                <div class="standard-card p-5 rounded-xl flex items-center gap-4">
                    <div class="h-12 w-12 rounded-lg bg-[rgba(46,125,50,0.1)] flex items-center justify-center">
                        <span class="material-symbols-outlined text-[#2e7d32]">task_alt</span>
                    </div>
                    <div>
                        <p class="font-badge-cap text-badge-cap text-outline uppercase tracking-widest">Completed</p>
                        <asp:Literal ID="litCompleted" runat="server" Text="0"></asp:Literal>
                    </div>
                </div>
            </div>

            <!-- Decisions Table -->
            <section class="standard-card rounded-xl overflow-hidden">
                <div class="px-6 py-4 border-b border-surface-container-high flex justify-between items-center">
                    <h3 class="font-title-lg text-title-lg text-primary">All Decisions</h3>
                    <asp:Literal ID="litDecisionCount" runat="server"></asp:Literal>
                </div>
                <div class="overflow-x-auto">
                    <table class="w-full text-left font-body-md">
                        <thead>
                            <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                <th class="px-6 py-3 font-semibold">Decision Title</th>
                                <th class="px-6 py-3 font-semibold">Priority</th>
                                <th class="px-6 py-3 font-semibold">Status</th>
                                <th class="px-6 py-3 font-semibold">Responsible Person</th>
                                <th class="px-6 py-3 font-semibold">Date</th>
                                <th class="px-6 py-3 font-semibold text-right">Actions</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-surface-container-high">
                            <asp:Repeater ID="rptDecisions" runat="server" OnItemDataBound="rptDecisions_ItemDataBound" OnItemCommand="rptDecisions_ItemCommand">
                                <ItemTemplate>
                                    <tr class="hover:bg-surface-container-lowest transition-colors group">
                                        <td class="px-6 py-4">
                                            <div class="flex items-center gap-3">
                                                <div class="h-10 w-10 rounded-lg bg-primary-container flex items-center justify-center shrink-0">
                                                    <span class="material-symbols-outlined text-on-primary-container text-[20px]">gavel</span>
                                                </div>
                                                <div>
                                                    <span class="font-semibold text-on-surface"><%# Eval("DecisionTitle") %></span>
                                                </div>
                                            </div>
                                        </td>
                                        <td class="px-6 py-4">
                                            <asp:Label ID="lblPriority" runat="server" CssClass="px-3 py-1 rounded-full font-badge-cap text-badge-cap inline-block"></asp:Label>
                                        </td>
                                        <td class="px-6 py-4">
                                            <asp:Label ID="lblStatus" runat="server" CssClass="px-3 py-1 rounded-full font-badge-cap text-badge-cap inline-block"></asp:Label>
                                            <asp:Label runat="server" Visible='<%# Convert.ToInt32(Eval("IsRestricted")) == 1 %>'
                                                CssClass="ml-2 px-3 py-1 rounded-full font-badge-cap text-badge-cap inline-block bg-[rgba(198,40,40,0.12)] text-[#c62828]">Restricted</asp:Label>
                                        </td>
                                        <td class="px-6 py-4">
                                            <span class="text-on-surface-variant"><%# Eval("ResponsiblePerson") %></span>
                                        </td>
                                        <td class="px-6 py-4 whitespace-nowrap">
                                            <span class="text-on-surface-variant"><%# Eval("CreatedAt", "{0:MMM dd, yyyy}") %></span>
                                        </td>
                                        <td class="px-6 py-4 text-right">
                                            <div class="flex items-center justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                                                <asp:LinkButton ID="btnView" runat="server" CommandName="ViewDecision" CommandArgument='<%# Eval("DecisionID") %>'
                                                    CssClass="p-2 hover:bg-surface-container-low rounded-lg transition-colors" ToolTip="View Details">
                                                    <span class="material-symbols-outlined text-outline hover:text-primary transition-colors text-[20px]">visibility</span>
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="btnRestrict" runat="server" CommandName="RestrictDecision" CommandArgument='<%# Eval("DecisionID") %>'
                                                    Visible='<%# Convert.ToInt32(Eval("IsRestricted")) == 0 %>'
                                                    CssClass="p-2 hover:bg-error-container/50 rounded-lg transition-colors" ToolTip="Restrict decision"
                                                    OnClientClick="return dtasConfirm(this, 'Restrict this decision? Linked work will be locked.');">
                                                    <span class="material-symbols-outlined text-outline hover:text-error transition-colors text-[20px]">block</span>
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="btnRestore" runat="server" CommandName="RestoreDecision" CommandArgument='<%# Eval("DecisionID") %>'
                                                    Visible='<%# Convert.ToInt32(Eval("IsRestricted")) == 1 %>'
                                                    CssClass="p-2 hover:bg-tertiary-container/30 rounded-lg transition-colors" ToolTip="Restore decision"
                                                    OnClientClick="return dtasConfirm(this, 'Restore this decision?');">
                                                    <span class="material-symbols-outlined text-tertiary text-[20px]">lock_open</span>
                                                </asp:LinkButton>
                                            </div>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                                <FooterTemplate>
                                    </tbody>
                                </FooterTemplate>
                            </asp:Repeater>
                            <asp:Panel ID="pnlNoDecisions" runat="server" Visible="false">
                                <tr>
                                    <td colspan="6" class="px-6 py-16 text-center">
                                        <div class="flex flex-col items-center gap-4">
                                            <div class="h-16 w-16 rounded-full bg-surface-container-high flex items-center justify-center">
                                                <span class="material-symbols-outlined text-outline text-[32px]">gavel</span>
                                            </div>
                                            <div>
                                                <p class="font-title-lg text-title-lg text-on-surface-variant mb-1">No decisions found</p>
                                                <p class="font-body-md text-body-md text-outline">Try adjusting your search or filter criteria.</p>
                                            </div>
                                        </div>
                                    </td>
                                </tr>
                            </asp:Panel>
                        </tbody>
                    </table>
                </div>
            </section>

            <!-- Pagination -->
            <asp:Panel ID="pnlPagination" runat="server" CssClass="mt-6 flex items-center justify-between" Visible="false">
                <div class="font-body-md text-body-md text-on-surface-variant">
                    <asp:Literal ID="litPageInfo" runat="server"></asp:Literal>
                </div>
                <div class="flex items-center gap-2">
                    <asp:Button ID="btnFirst" runat="server" Text="&laquo;" CssClass="px-3 py-2 border border-outline rounded-lg font-label-md text-label-md hover:bg-surface-container-low transition-colors cursor-pointer" OnClick="btnFirst_Click" CausesValidation="false" />
                    <asp:Button ID="btnPrev" runat="server" Text="&lsaquo;" CssClass="px-3 py-2 border border-outline rounded-lg font-label-md text-label-md hover:bg-surface-container-low transition-colors cursor-pointer" OnClick="btnPrev_Click" CausesValidation="false" />
                    <asp:Repeater ID="rptPager" runat="server" OnItemCommand="rptPager_ItemCommand">
                        <ItemTemplate>
                            <asp:LinkButton ID="lnkPage" runat="server" CommandName="Page" CommandArgument='<%# Eval("Page") %>'
                                CssClass='<%# Eval("Page").ToString() == litCurrentPage.Value ? "px-3 py-2 bg-primary text-on-primary rounded-lg font-label-md text-label-md font-bold" : "px-3 py-2 border border-outline rounded-lg font-label-md text-label-md hover:bg-surface-container-low transition-colors" %>'>
                                <%# Eval("Page") %>
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:Repeater>
                    <asp:Button ID="btnNext" runat="server" Text="&rsaquo;" CssClass="px-3 py-2 border border-outline rounded-lg font-label-md text-label-md hover:bg-surface-container-low transition-colors cursor-pointer" OnClick="btnNext_Click" CausesValidation="false" />
                    <asp:Button ID="btnLast" runat="server" Text="&raquo;" CssClass="px-3 py-2 border border-outline rounded-lg font-label-md text-label-md hover:bg-surface-container-low transition-colors cursor-pointer" OnClick="btnLast_Click" CausesValidation="false" />
                    <asp:HiddenField ID="litCurrentPage" runat="server" Value="1" />
                </div>
            </asp:Panel>

        </main>
    </div>

</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="<%= ResolveUrl("~/Assets/js/dashboard.js") %>"></script>
</asp:Content>
