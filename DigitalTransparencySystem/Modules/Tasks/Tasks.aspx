<%@ Page Title="Tasks | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="Tasks.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Tasks.Tasks" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>?v=2" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <uc:AdminTopbar runat="server" />

    <div class="flex min-h-screen">
        
        <uc:AdminSidebar ActivePage="Tasks" runat="server" />

        <!-- Main Content Area -->
        <main class="dashboard-main flex-1 ml-64 p-8">
            
            <!-- Header Section -->
            <header class="flex justify-between items-end mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Task Management</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">Review and restrict tasks. Leaders and event managers edit their own tasks.</p>
                </div>
                <asp:Panel ID="pnlCreateTask" runat="server" Visible="false" CssClass="flex gap-3">
                    <asp:HyperLink ID="lnkCreateTask" runat="server" NavigateUrl="~/Modules/Tasks/CreateTask.aspx"
                        CssClass="flex items-center gap-2 px-6 py-3 bg-primary text-on-primary rounded-xl font-label-md text-label-md font-bold hover:scale-[1.02] active:scale-95 transition-transform shadow-sm">
                        <span class="material-symbols-outlined">add</span>
                        Create Task
                    </asp:HyperLink>
                </asp:Panel>
            </header>

            <!-- Filters Row -->
            <asp:Panel ID="pnlFilters" runat="server">
            <div class="standard-card rounded-xl p-6 mb-6">
                <div class="flex flex-col md:flex-row gap-4 items-end">
                    <div class="flex-1 w-full">
                        <label class="font-badge-cap text-badge-cap text-on-surface-variant uppercase tracking-widest block mb-2">Search</label>
                        <div class="relative">
                            <span class="absolute left-3 top-1/2 -translate-y-1/2 material-symbols-outlined text-outline">search</span>
                            <asp:TextBox ID="txtSearch" runat="server" CssClass="pl-10 pr-4 py-2.5 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md w-full focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="Search tasks by title..." />
                        </div>
                    </div>
                    <div class="w-full md:w-48">
                        <label class="font-badge-cap text-badge-cap text-on-surface-variant uppercase tracking-widest block mb-2">Status</label>
                        <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="w-full py-2.5 px-4 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none cursor-pointer">
                            <asp:ListItem Text="All Statuses" Value="" />
                            <asp:ListItem Text="Pending" Value="Pending" />
                            <asp:ListItem Text="In Progress" Value="InProgress" />
                            <asp:ListItem Text="Submitted" Value="Submitted" />
                            <asp:ListItem Text="Under Review" Value="UnderReview" />
                            <asp:ListItem Text="Revision Needed" Value="RevisionNeeded" />
                            <asp:ListItem Text="Approved" Value="Approved" />
                            <asp:ListItem Text="Completed" Value="Completed" />
                            <asp:ListItem Text="Late / Overdue" Value="Overdue" />
                            <asp:ListItem Text="Delayed" Value="Delayed" />
                        </asp:DropDownList>
                    </div>
                    <div class="w-full md:w-48">
                        <label class="font-badge-cap text-badge-cap text-on-surface-variant uppercase tracking-widest block mb-2">Assigned To</label>
                        <asp:DropDownList ID="ddlAssignedUserFilter" runat="server" CssClass="w-full py-2.5 px-4 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none cursor-pointer">
                            <asp:ListItem Text="All Users" Value="" />
                        </asp:DropDownList>
                    </div>
                    <div class="w-full md:w-48">
                        <label class="font-badge-cap text-badge-cap text-on-surface-variant uppercase tracking-widest block mb-2">Event</label>
                        <asp:DropDownList ID="ddlEventFilter" runat="server" CssClass="w-full py-2.5 px-4 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none cursor-pointer">
                            <asp:ListItem Text="All Events" Value="" />
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
            </asp:Panel>

            <!-- Stats Summary -->
            <asp:Panel ID="pnlStats" runat="server">
            <div class="grid grid-cols-2 md:grid-cols-5 gap-4 mb-6">
                <div class="standard-card p-5 rounded-xl flex items-center gap-4">
                    <div class="h-12 w-12 rounded-lg bg-primary-container flex items-center justify-center">
                        <span class="material-symbols-outlined text-on-primary-container">assignment</span>
                    </div>
                    <div>
                        <p class="font-badge-cap text-badge-cap text-outline uppercase tracking-widest">Total</p>
                        <asp:Literal ID="litTotalTasks" runat="server" Text="0"></asp:Literal>
                    </div>
                </div>
                <div class="standard-card p-5 rounded-xl flex items-center gap-4">
                    <div class="h-12 w-12 rounded-lg bg-[rgba(117,117,117,0.1)] flex items-center justify-center">
                        <span class="material-symbols-outlined text-[#757575]">pending</span>
                    </div>
                    <div>
                        <p class="font-badge-cap text-badge-cap text-outline uppercase tracking-widest">Pending</p>
                        <asp:Literal ID="litPending" runat="server" Text="0"></asp:Literal>
                    </div>
                </div>
                <div class="standard-card p-5 rounded-xl flex items-center gap-4">
                    <div class="h-12 w-12 rounded-lg bg-[rgba(230,126,0,0.1)] flex items-center justify-center">
                        <span class="material-symbols-outlined text-[#e67e00]">autorenew</span>
                    </div>
                    <div>
                        <p class="font-badge-cap text-badge-cap text-outline uppercase tracking-widest">In Progress</p>
                        <asp:Literal ID="litInProgress" runat="server" Text="0"></asp:Literal>
                    </div>
                </div>
                <div class="standard-card p-5 rounded-xl flex items-center gap-4">
                    <div class="h-12 w-12 rounded-lg bg-[rgba(46,125,50,0.1)] flex items-center justify-center">
                        <span class="material-symbols-outlined text-[#2e7d32]">check_circle</span>
                    </div>
                    <div>
                        <p class="font-badge-cap text-badge-cap text-outline uppercase tracking-widest">Completed</p>
                        <asp:Literal ID="litCompleted" runat="server" Text="0"></asp:Literal>
                    </div>
                </div>
                <div class="standard-card p-5 rounded-xl flex items-center gap-4">
                    <div class="h-12 w-12 rounded-lg bg-[rgba(198,40,40,0.1)] flex items-center justify-center">
                        <span class="material-symbols-outlined text-[#c62828]">warning</span>
                    </div>
                    <div>
                        <p class="font-badge-cap text-badge-cap text-outline uppercase tracking-widest">Overdue</p>
                        <asp:Literal ID="litOverdue" runat="server" Text="0"></asp:Literal>
                    </div>
                </div>
            </div>
            </asp:Panel>

            <!-- Tasks Table -->
            <section class="standard-card rounded-xl overflow-hidden">
                <div class="px-6 py-4 border-b border-surface-container-high flex justify-between items-center">
                    <h3 class="font-title-lg text-title-lg text-primary">All Tasks</h3>
                    <asp:Literal ID="litTaskCount" runat="server"></asp:Literal>
                </div>
                <div class="overflow-x-auto">
                    <table class="w-full text-left font-body-md">
                        <thead>
                            <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                <th class="px-6 py-3 font-semibold">Task Title</th>
                                <th class="px-6 py-3 font-semibold">Event</th>
                                <th class="px-6 py-3 font-semibold">Leader</th>
                                <th class="px-6 py-3 font-semibold">Assigned To</th>
                                <th class="px-6 py-3 font-semibold">Priority</th>
                                <th class="px-6 py-3 font-semibold">Status</th>
                                <th class="px-6 py-3 font-semibold">Due Date</th>
                                <th class="px-6 py-3 font-semibold text-right">Actions</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-surface-container-high">
                            <asp:Repeater ID="rptTasks" runat="server" OnItemDataBound="rptTasks_ItemDataBound" OnItemCommand="rptTasks_ItemCommand">
                                <ItemTemplate>
                                    <tr class="hover:bg-surface-container-lowest transition-colors group" id="taskRow" runat="server">
                                        <td class="px-6 py-4">
                                            <div class="flex items-center gap-3">
                                                <div class="h-10 w-10 rounded-lg bg-primary-container flex items-center justify-center shrink-0">
                                                    <span class="material-symbols-outlined text-on-primary-container text-[20px]">assignment</span>
                                                </div>
                                                <div>
                                                    <span class="font-semibold text-on-surface"><%# Eval("TaskTitle") %></span>
                                                    <p class="text-xs text-on-surface-variant truncate max-w-[200px]"><%# Eval("ShortDescription") %></p>
                                                </div>
                                            </div>
                                        </td>
                                        <td class="px-6 py-4">
                                            <asp:HyperLink ID="lnkRelatedEvent" runat="server"
                                                NavigateUrl='<%# Eval("EventID") == DBNull.Value ? "" : ResolveUrl("~/Modules/Events/EventDetails.aspx?EventID=" + Eval("EventID")) %>'
                                                Text='<%# Eval("EventID") == DBNull.Value ? "—" : Eval("EventName") %>'
                                                CssClass='<%# Eval("EventID") == DBNull.Value ? "text-on-surface-variant" : "text-primary font-semibold hover:underline" %>'
                                                Enabled='<%# Eval("EventID") != DBNull.Value %>' />
                                        </td>
                                        <td class="px-6 py-4">
                                            <span class="text-on-surface-variant"><%# Eval("LeaderName") %></span>
                                        </td>
                                        <td class="px-6 py-4">
                                            <span class="text-on-surface-variant"><%# Eval("AssignedTo") %></span>
                                        </td>
                                        <td class="px-6 py-4">
                                            <asp:Label ID="lblPriority" runat="server" CssClass="px-3 py-1 rounded-full font-badge-cap text-badge-cap inline-block"></asp:Label>
                                        </td>
                                        <td class="px-6 py-4">
                                            <asp:Label ID="lblStatus" runat="server" CssClass="px-3 py-1 rounded-full font-badge-cap text-badge-cap inline-block"></asp:Label>
                                            <asp:Label runat="server" Visible='<%# Convert.ToInt32(Eval("IsRestricted")) == 1 %>'
                                                CssClass="ml-2 px-3 py-1 rounded-full font-badge-cap text-badge-cap inline-block bg-[rgba(198,40,40,0.12)] text-[#c62828]">Restricted</asp:Label>
                                        </td>
                                        <td class="px-6 py-4 whitespace-nowrap">
                                            <div class="flex items-center gap-2">
                                                <asp:Label ID="lblOverdue" runat="server" Visible="false" CssClass="material-symbols-outlined text-[16px] text-[#c62828]" ToolTip="Late">warning</asp:Label>
                                                <span id="spanDueLabel" runat="server" class="text-on-surface-variant"><%# Eval("DueDate", "{0:MMM dd, yyyy}") %></span>
                                            </div>
                                        </td>
                                        <td class="px-6 py-4 text-right">
                                            <div class="flex items-center justify-end gap-1">
                                                <asp:LinkButton ID="btnView" runat="server" CommandName="ViewTask" CommandArgument='<%# Eval("TaskID") %>'
                                                    CssClass="p-2 hover:bg-surface-container-low rounded-lg transition-colors" ToolTip="View Task">
                                                    <span class="material-symbols-outlined text-outline hover:text-primary transition-colors text-[20px]">visibility</span>
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="btnRestrict" runat="server" CommandName="RestrictTask" CommandArgument='<%# Eval("TaskID") %>'
                                                    Visible='<%# Convert.ToInt32(Eval("IsRestricted")) == 0 %>'
                                                    CssClass="p-2 hover:bg-error-container/50 rounded-lg transition-colors" ToolTip="Restrict task"
                                                    OnClientClick="return dtasConfirm(this, 'Restrict this task? Assigned members will not be able to work on it.');">
                                                    <span class="material-symbols-outlined text-outline hover:text-error transition-colors text-[20px]">block</span>
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="btnRestore" runat="server" CommandName="RestoreTask" CommandArgument='<%# Eval("TaskID") %>'
                                                    Visible='<%# Convert.ToInt32(Eval("IsRestricted")) == 1 %>'
                                                    CssClass="p-2 hover:bg-tertiary-container/30 rounded-lg transition-colors" ToolTip="Restore task"
                                                    OnClientClick="return dtasConfirm(this, 'Restore this task so members can work on it again?');">
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
                            <asp:Panel ID="pnlNoTasks" runat="server" Visible="false">
                                <tr>
                                    <td colspan="8" class="px-6 py-16 text-center">
                                        <div class="flex flex-col items-center gap-4">
                                            <div class="h-16 w-16 rounded-full bg-surface-container-high flex items-center justify-center">
                                                <span class="material-symbols-outlined text-outline text-[32px]">assignment</span>
                                            </div>
                                            <div>
                                                <p class="font-title-lg text-title-lg text-on-surface-variant mb-1">No tasks found</p>
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
        </main>
    </div>

</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="<%= ResolveUrl("~/Assets/js/dashboard.js") %>"></script>
</asp:Content>