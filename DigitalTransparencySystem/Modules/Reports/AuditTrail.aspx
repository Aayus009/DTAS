<%@ Page Title="Audit Trail | DTAS"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="AuditTrail.aspx.cs"
    Inherits="DigitalTransparencySystem.Modules.Reports.AuditTrail" %>
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
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Audit Trail</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">Comprehensive log of all system activities across decisions, tasks, and user sessions.</p>
                </div>
                <div class="flex gap-3">
                    <asp:Button ID="btnExport" runat="server" Text="Export CSV"
                        CssClass="py-2 px-4 bg-primary text-on-primary rounded-lg font-label-md text-label-md font-bold hover:opacity-90 transition-opacity cursor-pointer"
                        OnClick="btnExport_Click" />
                </div>
            </header>

            <section class="standard-card rounded-xl overflow-hidden mb-6">
                <div class="px-6 py-4 border-b border-surface-container-high">
                    <h3 class="font-title-lg text-title-lg text-primary">System audit log</h3>
                    <p class="text-xs text-on-surface-variant">Actions written to AuditLogs (login, identity, moderation, events, assignments, clubs).</p>
                </div>
                <asp:Repeater ID="rptSystemAudit" runat="server">
                    <HeaderTemplate>
                        <table class="w-full text-left font-body-md">
                            <thead>
                                <tr class="bg-surface-container-low text-on-surface-variant">
                                    <th class="px-6 py-3">When</th>
                                    <th class="px-6 py-3">User</th>
                                    <th class="px-6 py-3">Action</th>
                                    <th class="px-6 py-3">Entity</th>
                                    <th class="px-6 py-3">Detail</th>
                                </tr>
                            </thead>
                            <tbody class="divide-y divide-surface-container-high">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td class="px-6 py-3 whitespace-nowrap"><%# Eval("Timestamp", "{0:MMM dd, yyyy HH:mm}") %></td>
                            <td class="px-6 py-3"><%# Eval("FullName") %></td>
                            <td class="px-6 py-3"><%# Eval("Action") %></td>
                            <td class="px-6 py-3"><%# Eval("EntityType") %> <%# Eval("EntityID") %></td>
                            <td class="px-6 py-3 text-on-surface-variant"><%# Eval("Description") %></td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                            </tbody>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlNoSystemAudit" runat="server" Visible="false" CssClass="px-6 py-8 text-on-surface-variant">No system audit rows yet.</asp:Panel>
            </section>

            <!-- Filters -->
            <section class="standard-card rounded-xl p-6 mb-6">
                <h3 class="font-title-lg text-title-lg text-primary mb-4">Filters</h3>
                <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
                    <div>
                        <label class="font-label-md text-label-md text-on-surface-variant block mb-1">Date From</label>
                        <asp:TextBox ID="txtDateFrom" runat="server" TextMode="Date"
                            CssClass="w-full px-3 py-2 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" />
                    </div>
                    <div>
                        <label class="font-label-md text-label-md text-on-surface-variant block mb-1">Date To</label>
                        <asp:TextBox ID="txtDateTo" runat="server" TextMode="Date"
                            CssClass="w-full px-3 py-2 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" />
                    </div>
                    <div>
                        <label class="font-label-md text-label-md text-on-surface-variant block mb-1">User</label>
                        <asp:DropDownList ID="ddlUser" runat="server"
                            CssClass="w-full px-3 py-2 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all">
                            <asp:ListItem Value="" Text="All Users" />
                        </asp:DropDownList>
                    </div>
                    <div>
                        <label class="font-label-md text-label-md text-on-surface-variant block mb-1">Action Type</label>
                        <asp:DropDownList ID="ddlActionType" runat="server"
                            CssClass="w-full px-3 py-2 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all">
                            <asp:ListItem Value="" Text="All Actions" />
                            <asp:ListItem Value="Decision" Text="Decision" />
                            <asp:ListItem Value="Task" Text="Task" />
                            <asp:ListItem Value="Login" Text="Login" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="mt-4 flex gap-3">
                    <asp:Button ID="btnApplyFilters" runat="server" Text="Apply Filters"
                        CssClass="py-2 px-6 bg-primary text-on-primary rounded-lg font-label-md text-label-md font-bold hover:opacity-90 transition-opacity cursor-pointer"
                        OnClick="btnApplyFilters_Click" />
                    <asp:Button ID="btnClearFilters" runat="server" Text="Clear"
                        CssClass="py-2 px-6 border border-outline text-on-surface-variant rounded-lg font-label-md text-label-md font-bold hover:bg-surface-container-low transition-colors cursor-pointer"
                        OnClick="btnClearFilters_Click" />
                </div>
            </section>

            <!-- Audit Trail Table -->
            <section class="standard-card rounded-xl overflow-hidden">
                <div class="px-6 py-4 border-b border-surface-container-high flex justify-between items-center">
                    <h3 class="font-title-lg text-title-lg text-primary">Activity Log</h3>
                    <span class="font-badge-cap text-badge-cap uppercase text-outline"><asp:Literal ID="litTotalEntries" runat="server" Text="0"></asp:Literal> entries</span>
                </div>
                <div class="overflow-x-auto">
                    <table class="w-full text-left font-body-md">
                        <thead>
                            <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                <th class="px-6 py-3 font-semibold">Timestamp</th>
                                <th class="px-6 py-3 font-semibold">User</th>
                                <th class="px-6 py-3 font-semibold">Action Type</th>
                                <th class="px-6 py-3 font-semibold">Description</th>
                                <th class="px-6 py-3 font-semibold">Related Item</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-surface-container-high">
                            <asp:Repeater ID="rptAuditTrail" runat="server">
                                <ItemTemplate>
                                    <tr class="hover:bg-surface-container-lowest transition-colors">
                                        <td class="px-6 py-4 whitespace-nowrap text-on-surface-variant">
                                            <%# Eval("Timestamp", "{0:MMM dd, yyyy hh:mm tt}") %>
                                        </td>
                                        <td class="px-6 py-4">
                                            <div class="flex items-center gap-2">
                                                <div class="h-7 w-7 rounded-full bg-primary-container flex items-center justify-center">
                                                    <span class="material-symbols-outlined text-on-primary-container text-[14px]">person</span>
                                                </div>
                                                <span class="font-semibold"><%# Eval("UserName") %></span>
                                            </div>
                                        </td>
                                        <td class="px-6 py-4">
                                            <span class="px-2 py-1 rounded font-badge-cap text-badge-cap" style='<%# "background-color: " + Eval("TypeBgColor") + "; color: " + Eval("TypeColor") %>'><%# Eval("ActionType") %></span>
                                        </td>
                                        <td class="px-6 py-4 text-on-surface-variant max-w-md"><%# Eval("Description") %></td>
                                        <td class="px-6 py-4 text-on-surface-variant"><%# Eval("RelatedItem") %></td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>
                <asp:Panel ID="pnlNoResults" runat="server" Visible="false" CssClass="p-8 text-center">
                    <span class="material-symbols-outlined text-outline text-4xl mb-2">search_off</span>
                    <p class="font-body-md text-on-surface-variant">No audit trail entries match your filters.</p>
                </asp:Panel>

                <!-- Pagination -->
                <div class="px-6 py-4 border-t border-surface-container-high flex justify-between items-center">
                    <asp:Button ID="btnPrev" runat="server" Text="Previous" CssClass="py-2 px-4 border border-outline text-on-surface-variant rounded-lg font-label-md text-label-md font-bold hover:bg-surface-container-low transition-colors cursor-pointer" OnClick="btnPrev_Click" />
                    <span class="font-label-md text-label-md text-on-surface-variant">Page <asp:Literal ID="litPageNumber" runat="server" Text="1"></asp:Literal> of <asp:Literal ID="litTotalPages" runat="server" Text="1"></asp:Literal></span>
                    <asp:Button ID="btnNext" runat="server" Text="Next" CssClass="py-2 px-4 border border-outline text-on-surface-variant rounded-lg font-label-md text-label-md font-bold hover:bg-surface-container-low transition-colors cursor-pointer" OnClick="btnNext_Click" />
                </div>
            </section>

        </main>
    </div>

</asp:Content>
