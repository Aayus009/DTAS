<%@ Page Title="Login History | DTAS"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="LoginHistory.aspx.cs"
    Inherits="DigitalTransparencySystem.Modules.Reports.LoginHistory" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>?v=trailscroll1" />
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
                    <div class="flex items-center gap-3 mb-2">
                        <asp:HyperLink ID="lnkBackToReports" runat="server" NavigateUrl="~/Modules/Reports/Reports.aspx" CssClass="p-2 hover:bg-surface-container-low rounded-full transition-colors">
                            <span class="material-symbols-outlined text-on-surface-variant">arrow_back</span>
                        </asp:HyperLink>
                        <h1 class="font-headline-lg text-headline-lg text-primary">Login History</h1>
                    </div>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl ml-11">Detailed login and logout records for all users including timestamps and IP addresses.</p>
                </div>
                <div class="flex gap-3">
                    <asp:Button ID="btnExport" runat="server" Text="Export CSV"
                        CssClass="py-2 px-4 bg-primary text-on-primary rounded-lg font-label-md text-label-md font-bold hover:opacity-90 transition-opacity cursor-pointer"
                        OnClick="btnExport_Click" />
                </div>
            </header>

            <!-- Filters -->
            <section class="standard-card rounded-xl p-6 mb-6">
                <h3 class="font-title-lg text-title-lg text-primary mb-4">Filters</h3>
                <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
                    <div>
                        <label class="font-label-md text-label-md text-on-surface-variant block mb-1">Search</label>
                        <asp:TextBox ID="txtSearch" runat="server" placeholder="Search by name or username..."
                            CssClass="w-full px-3 py-2 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" />
                    </div>
                    <div>
                        <label class="font-label-md text-label-md text-on-surface-variant block mb-1">Date</label>
                        <asp:TextBox ID="txtDate" runat="server" TextMode="Date"
                            CssClass="w-full px-3 py-2 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" />
                    </div>
                    <div>
                        <label class="font-label-md text-label-md text-on-surface-variant block mb-1">Start Time</label>
                        <asp:TextBox ID="txtStartTime" runat="server" TextMode="Time"
                            CssClass="w-full px-3 py-2 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" />
                    </div>
                    <div>
                        <label class="font-label-md text-label-md text-on-surface-variant block mb-1">End Time</label>
                        <asp:TextBox ID="txtEndTime" runat="server" TextMode="Time"
                            CssClass="w-full px-3 py-2 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" />
                    </div>
                </div>
                <div class="mt-4 flex gap-3">
                    <asp:Button ID="btnSearch" runat="server" Text="Apply Filters"
                        CssClass="py-2 px-6 bg-primary text-on-primary rounded-lg font-label-md text-label-md font-bold hover:opacity-90 transition-opacity cursor-pointer"
                        OnClick="btnSearch_Click" />
                    <asp:Button ID="btnClear" runat="server" Text="Clear"
                        CssClass="py-2 px-6 border border-outline text-on-surface-variant rounded-lg font-label-md text-label-md font-bold hover:bg-surface-container-low transition-colors cursor-pointer"
                        OnClick="btnClear_Click" />
                </div>
            </section>

            <!-- Login History Table -->
            <section class="standard-card rounded-xl overflow-hidden">
                <div class="px-6 py-4 border-b border-surface-container-high flex justify-between items-center">
                    <h3 class="font-title-lg text-title-lg text-primary">Login Records</h3>
                    <span class="font-badge-cap text-badge-cap uppercase text-outline"><asp:Literal ID="litRecordCount" runat="server" Text="0"></asp:Literal> records</span>
                </div>
                <div class="report-table-scroll">
                    <table class="w-full text-left font-body-md">
                        <thead>
                            <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                <th class="px-6 py-3 font-semibold">User</th>
                                <th class="px-6 py-3 font-semibold">Username</th>
                                <th class="px-6 py-3 font-semibold">Login Time</th>
                                <th class="px-6 py-3 font-semibold">Logout Time</th>
                                <th class="px-6 py-3 font-semibold">IP Address</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-surface-container-high">
                            <asp:Repeater ID="rptLoginHistory" runat="server">
                                <ItemTemplate>
                                    <tr class="hover:bg-surface-container-lowest transition-colors">
                                        <td class="px-6 py-4">
                                            <div class="flex items-center gap-2">
                                                <div class="h-7 w-7 rounded-full bg-primary-container flex items-center justify-center">
                                                    <span class="material-symbols-outlined text-on-primary-container text-[14px]">person</span>
                                                </div>
                                                <span class="font-semibold"><%# Eval("FullName") %></span>
                                            </div>
                                        </td>
                                        <td class="px-6 py-4 text-on-surface-variant"><%# Eval("Username") %></td>
                                        <td class="px-6 py-4 text-on-surface-variant"><%# Eval("LoginTime", "{0:MMM dd, yyyy hh:mm tt}") %></td>
                                        <td class="px-6 py-4 text-on-surface-variant"><%# Eval("LogoutTime") != DBNull.Value ? Convert.ToDateTime(Eval("LogoutTime")).ToString("MMM dd, yyyy hh:mm tt") : "<span class=\"text-outline\">Active</span>" %></td>
                                        <td class="px-6 py-4 text-on-surface-variant"><%# Eval("IPAddress") %></td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>
                <asp:Panel ID="pnlNoResults" runat="server" Visible="false" CssClass="p-8 text-center">
                    <span class="material-symbols-outlined text-outline text-4xl mb-2">search_off</span>
                    <p class="font-body-md text-on-surface-variant">No login records match your filters.</p>
                </asp:Panel>
            </section>

        </main>
    </div>

</asp:Content>
