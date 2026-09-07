<%@ Page Title="Reports | DTAS"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Reports.aspx.cs"
    Inherits="DigitalTransparencySystem.Modules.Reports.Reports" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <uc:AdminTopbar runat="server" />

    <div class="flex min-h-screen">

        <uc:AdminSidebar ActivePage="Reports" runat="server" />

        <!-- Main Content Area -->
        <main class="dashboard-main flex-1 ml-64 p-8">

            <!-- Header Section -->
            <header class="flex justify-between items-end mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Reports Hub</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">Generate, manage, and review institutional reports for full transparency and accountability.</p>
                </div>
                <div class="flex gap-3">
                    <div class="flex flex-col items-end">
                        <span class="font-badge-cap text-badge-cap uppercase text-outline">Last Updated</span>
                        <asp:Literal ID="litLastUpdated" runat="server"></asp:Literal>
                    </div>
                </div>
            </header>

            <section class="standard-card rounded-xl p-6 mb-6">
                <h3 class="font-title-lg text-title-lg text-primary mb-4">System snapshot</h3>
                <p class="text-xs text-on-surface-variant mb-4">Live counts from <code>sp_GetAdminDashboardStats</code>. Generated file reports below are unchanged.</p>
                <div class="grid grid-cols-2 md:grid-cols-6 gap-4 mb-4">
                    <div>
                        <span class="font-badge-cap text-badge-cap uppercase text-outline block">Users</span>
                        <asp:Literal ID="litSnapUsers" runat="server" Text="0"></asp:Literal>
                    </div>
                    <div>
                        <span class="font-badge-cap text-badge-cap uppercase text-outline block">Pending IDs</span>
                        <asp:Literal ID="litSnapPendingId" runat="server" Text="0"></asp:Literal>
                    </div>
                    <div>
                        <span class="font-badge-cap text-badge-cap uppercase text-outline block">Assignments</span>
                        <asp:Literal ID="litSnapAssignments" runat="server" Text="0"></asp:Literal>
                    </div>
                    <div>
                        <span class="font-badge-cap text-badge-cap uppercase text-outline block">Clubs</span>
                        <asp:Literal ID="litSnapGroups" runat="server" Text="0"></asp:Literal>
                    </div>
                    <div>
                        <span class="font-badge-cap text-badge-cap uppercase text-outline block">Flags</span>
                        <asp:Literal ID="litSnapFlags" runat="server" Text="0"></asp:Literal>
                    </div>
                    <div>
                        <span class="font-badge-cap text-badge-cap uppercase text-outline block">Banned</span>
                        <asp:Literal ID="litSnapBanned" runat="server" Text="0"></asp:Literal>
                    </div>
                </div>
                <div class="flex flex-wrap gap-4">
                    <a class="text-primary font-bold" href="<%= ResolveUrl("~/Modules/Reports/AuditTrail.aspx") %>">Audit trail</a>
                    <a class="text-primary font-bold" href="<%= ResolveUrl("~/Modules/Users/Flags.aspx") %>">Content flags</a>
                    <a class="text-primary font-bold" href="<%= ResolveUrl("~/Modules/Users/IdentityQueue.aspx") %>">ID queue</a>
                </div>
            </section>

            <!-- Stats Cards -->
            <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-6">

                <!-- Total Reports Generated -->
                <div class="standard-card p-6 flex flex-col justify-between rounded-xl">
                    <div>
                        <span class="font-badge-cap text-badge-cap text-tertiary uppercase tracking-widest block mb-4">Total Reports Generated</span>
                        <asp:Literal ID="litTotalReports" runat="server" Text="0"></asp:Literal>
                    </div>
                    <div class="mt-4">
                        <div class="flex items-center gap-1">
                            <span class="material-symbols-outlined text-on-tertiary-container text-[18px]">description</span>
                            <p class="font-label-md text-label-md text-on-surface-variant">All time</p>
                        </div>
                    </div>
                </div>

                <!-- Events Summary -->
                <div class="standard-card p-6 flex flex-col justify-between rounded-xl">
                    <div>
                        <span class="font-badge-cap text-badge-cap text-on-secondary-fixed-variant uppercase tracking-widest block mb-4">Events Summary</span>
                        <asp:Literal ID="litEventsSummary" runat="server" Text="0"></asp:Literal>
                    </div>
                    <div class="mt-4">
                        <div class="flex items-center gap-1">
                            <span class="material-symbols-outlined text-secondary text-[18px]">calendar_today</span>
                            <p class="font-label-md text-label-md text-on-surface-variant">Tracked events</p>
                        </div>
                    </div>
                </div>

                <!-- Tasks Completion Rate -->
                <div class="standard-card p-6 flex flex-col justify-between rounded-xl">
                    <div>
                        <span class="font-badge-cap text-badge-cap text-on-surface-variant uppercase tracking-widest block mb-4">Tasks Completion Rate</span>
                        <asp:Literal ID="litTasksRate" runat="server" Text="0%"></asp:Literal>
                    </div>
                    <div class="mt-4">
                        <div class="w-full bg-surface-container-high h-2 rounded-full overflow-hidden">
                            <asp:Literal ID="litTasksRateBar" runat="server" Text="<div class='bg-primary h-full' style='width: 0%'></div>"></asp:Literal>
                        </div>
                    </div>
                </div>

                <!-- Decision Outcomes -->
                <div class="standard-card p-6 flex flex-col justify-between rounded-xl">
                    <div>
                        <span class="font-badge-cap text-badge-cap text-tertiary uppercase tracking-widest block mb-4">Decision Outcomes</span>
                        <asp:Literal ID="litDecisionOutcomes" runat="server" Text="0"></asp:Literal>
                    </div>
                    <div class="mt-4">
                        <div class="flex items-center gap-1">
                            <span class="material-symbols-outlined text-on-tertiary-container text-[18px]">gavel</span>
                            <p class="font-label-md text-label-md text-on-surface-variant">Resolved decisions</p>
                        </div>
                    </div>
                </div>

            </div>

            <!-- Report Type Cards -->
            <section class="mb-8">
                <h2 class="font-title-lg text-title-lg text-primary mb-4">Generate Reports</h2>
                <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">

                    <!-- Event Report -->
                    <div class="standard-card p-6 rounded-xl flex flex-col">
                        <div class="flex items-center gap-3 mb-4">
                            <div class="h-12 w-12 rounded-lg bg-primary-container flex items-center justify-center">
                                <span class="material-symbols-outlined text-primary">calendar_today</span>
                            </div>
                            <div>
                                <h3 class="font-title-md text-title-md text-on-surface">Event Report</h3>
                                <p class="font-body-sm text-sm text-on-surface-variant">Summary of all institutional events</p>
                            </div>
                        </div>
                        <p class="font-body-md text-sm text-on-surface-variant mb-6 flex-1">Generate a comprehensive report covering scheduled, completed, and cancelled events with attendance data.</p>
                        <asp:Button ID="btnGenerateEventReport" runat="server" Text="Generate Report" CssClass="w-full py-2.5 bg-primary text-on-primary rounded-lg font-label-md text-label-md font-bold hover:opacity-90 transition-opacity cursor-pointer" OnClick="btnGenerateEventReport_Click" />
                    </div>

                    <!-- Decision Report -->
                    <div class="standard-card p-6 rounded-xl flex flex-col">
                        <div class="flex items-center gap-3 mb-4">
                            <div class="h-12 w-12 rounded-lg bg-secondary-container flex items-center justify-center">
                                <span class="material-symbols-outlined text-on-secondary-fixed">gavel</span>
                            </div>
                            <div>
                                <h3 class="font-title-md text-title-md text-on-surface">Decision Report</h3>
                                <p class="font-body-sm text-sm text-on-surface-variant">Governance decision outcomes</p>
                            </div>
                        </div>
                        <p class="font-body-md text-sm text-on-surface-variant mb-6 flex-1">Compile all institutional decisions, their statuses, rationale, and outcomes for public accountability.</p>
                        <asp:Button ID="btnGenerateDecisionReport" runat="server" Text="Generate Report" CssClass="w-full py-2.5 bg-primary text-on-primary rounded-lg font-label-md text-label-md font-bold hover:opacity-90 transition-opacity cursor-pointer" OnClick="btnGenerateDecisionReport_Click" />
                    </div>

                    <!-- Accountability Report -->
                    <div class="standard-card p-6 rounded-xl flex flex-col">
                        <div class="flex items-center gap-3 mb-4">
                            <div class="h-12 w-12 rounded-lg bg-tertiary-container flex items-center justify-center">
                                <span class="material-symbols-outlined text-on-tertiary-container">verified_user</span>
                            </div>
                            <div>
                                <h3 class="font-title-md text-title-md text-on-surface">Accountability Report</h3>
                                <p class="font-body-sm text-sm text-on-surface-variant">Staff & compliance overview</p>
                            </div>
                        </div>
                        <p class="font-body-md text-sm text-on-surface-variant mb-6 flex-1">Produce an accountability audit covering task completions, decision adherence, and staff compliance metrics.</p>
                        <asp:Button ID="btnGenerateAccountabilityReport" runat="server" Text="Generate Report" CssClass="w-full py-2.5 bg-primary text-on-primary rounded-lg font-label-md text-label-md font-bold hover:opacity-90 transition-opacity cursor-pointer" OnClick="btnGenerateAccountabilityReport_Click" />
                    </div>

                    <!-- Login History -->
                    <div class="standard-card p-6 rounded-xl flex flex-col">
                        <div class="flex items-center gap-3 mb-4">
                            <div class="h-12 w-12 rounded-lg bg-surface-container-high flex items-center justify-center">
                                <span class="material-symbols-outlined text-on-surface-variant">login</span>
                            </div>
                            <div>
                                <h3 class="font-title-md text-title-md text-on-surface">Login History</h3>
                                <p class="font-body-sm text-sm text-on-surface-variant">User access records</p>
                            </div>
                        </div>
                        <p class="font-body-md text-sm text-on-surface-variant mb-6 flex-1">View detailed login and logout records for all users including timestamps and IP addresses.</p>
                        <asp:HyperLink ID="lnkLoginHistory" runat="server" NavigateUrl="~/Modules/Reports/LoginHistory.aspx" CssClass="block w-full py-2.5 bg-surface-variant text-on-surface rounded-lg font-label-md text-label-md font-bold text-center hover:bg-surface-container-high transition-colors">View Login History</asp:HyperLink>
                    </div>

                    <!-- Audit Trail -->
                    <div class="standard-card p-6 rounded-xl flex flex-col">
                        <div class="flex items-center gap-3 mb-4">
                            <div class="h-12 w-12 rounded-lg bg-secondary-container flex items-center justify-center">
                                <span class="material-symbols-outlined text-on-secondary-fixed">history</span>
                            </div>
                            <div>
                                <h3 class="font-title-md text-title-md text-on-surface">Audit Trail</h3>
                                <p class="font-body-sm text-sm text-on-surface-variant">System activity log</p>
                            </div>
                        </div>
                        <p class="font-body-md text-sm text-on-surface-variant mb-6 flex-1">View comprehensive activity logs across decisions, tasks, and user sessions with filters and export.</p>
                        <asp:HyperLink ID="lnkAuditTrail" runat="server" NavigateUrl="~/Modules/Reports/AuditTrail.aspx" CssClass="block w-full py-2.5 bg-surface-variant text-on-surface rounded-lg font-label-md text-label-md font-bold text-center hover:bg-surface-container-high transition-colors">View Audit Trail</asp:HyperLink>
                    </div>

                    <!-- Users & Feedback -->
                    <div class="standard-card p-6 rounded-xl flex flex-col">
                        <div class="flex items-center gap-3 mb-4">
                            <div class="h-12 w-12 rounded-lg bg-tertiary-container flex items-center justify-center">
                                <span class="material-symbols-outlined text-on-tertiary-container">group</span>
                            </div>
                            <div>
                                <h3 class="font-title-md text-title-md text-on-surface">Users & Feedback</h3>
                                <p class="font-body-sm text-sm text-on-surface-variant">User management overview</p>
                            </div>
                        </div>
                        <p class="font-body-md text-sm text-on-surface-variant mb-6 flex-1">Generate reports on user registrations, role distributions, and feedback summaries for institutional review.</p>
                        <asp:Button ID="btnGenerateUsersReport" runat="server" Text="Users Report" CssClass="w-full py-2.5 bg-primary text-on-primary rounded-lg font-label-md text-label-md font-bold hover:opacity-90 transition-opacity cursor-pointer" OnClick="btnGenerateUsersReport_Click" />
                        <asp:Button ID="btnGenerateFeedbackReport" runat="server" Text="Feedback Report" CssClass="w-full py-2.5 bg-surface-variant text-on-surface rounded-lg font-label-md text-label-md font-bold hover:bg-surface-container-high transition-colors cursor-pointer mt-2" OnClick="btnGenerateFeedbackReport_Click" />
                    </div>

                    <!-- Task Completion Report -->
                    <div class="standard-card p-6 rounded-xl flex flex-col">
                        <div class="flex items-center gap-3 mb-4">
                            <div class="h-12 w-12 rounded-lg bg-error-container flex items-center justify-center">
                                <span class="material-symbols-outlined text-on-error-container">assignment</span>
                            </div>
                            <div>
                                <h3 class="font-title-md text-title-md text-on-surface">Task Completion Report</h3>
                                <p class="font-body-sm text-sm text-on-surface-variant">Assignment progress tracking</p>
                            </div>
                        </div>
                        <p class="font-body-md text-sm text-on-surface-variant mb-6 flex-1">Track task assignments, completion rates, overdue items, and staff performance across departments.</p>
                        <asp:Button ID="btnGenerateTaskCompletionReport" runat="server" Text="Generate Report" CssClass="w-full py-2.5 bg-primary text-on-primary rounded-lg font-label-md text-label-md font-bold hover:opacity-90 transition-opacity cursor-pointer" OnClick="btnGenerateTaskCompletionReport_Click" />
                    </div>

                </div>
            </section>

            <!-- Recent Reports -->
            <section class="standard-card rounded-xl overflow-hidden">
                <div class="px-6 py-4 border-b border-surface-container-high flex justify-between items-center">
                    <h3 class="font-title-lg text-title-lg text-primary">Recent Reports</h3>
                    <span class="font-badge-cap text-badge-cap uppercase text-outline"><asp:Literal ID="litRecentCount" runat="server" Text="0"></asp:Literal> reports</span>
                </div>
                <div class="overflow-x-auto">
                    <table class="w-full text-left font-body-md">
                        <thead>
                            <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                <th class="px-6 py-3 font-semibold">Report Title</th>
                                <th class="px-6 py-3 font-semibold">Type</th>
                                <th class="px-6 py-3 font-semibold">Generated Date</th>
                                <th class="px-6 py-3 font-semibold">Generated By</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-surface-container-high">
                            <asp:Repeater ID="rptRecentReports" runat="server">
                                <ItemTemplate>
                                    <tr class="hover:bg-surface-container-lowest transition-colors">
                                        <td class="px-6 py-4">
                                            <div class="flex items-center gap-3">
                                                <span class="material-symbols-outlined text-primary">description</span>
                                                <span class="font-semibold"><%# Eval("ReportTitle") %></span>
                                            </div>
                                        </td>
                                        <td class="px-6 py-4">
                                            <span class="bg-primary-container text-on-primary-container px-2 py-1 rounded font-badge-cap text-badge-cap"><%# Eval("ReportType") %></span>
                                        </td>
                                        <td class="px-6 py-4 text-on-surface-variant"><%# Eval("CreatedAt", "{0:MMM dd, yyyy - hh:mm tt}") %></td>
                                        <td class="px-6 py-4 text-on-surface-variant"><%# Eval("GeneratedBy") %></td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>
                <asp:Panel ID="pnlNoReports" runat="server" Visible="false" CssClass="p-8 text-center">
                    <span class="material-symbols-outlined text-outline text-4xl mb-2">inventory_2</span>
                    <p class="font-body-md text-on-surface-variant">No reports have been generated yet.</p>
                </asp:Panel>
            </section>

        </main>
    </div>

</asp:Content>
