<%@ Page Title="Clubs | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Clubs.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Clubs.Clubs" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <uc:AdminTopbar runat="server" />

    <div class="flex min-h-screen">

        <uc:AdminSidebar ActivePage="Clubs" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">

            <header class="flex justify-between items-end mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Clubs Management</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">
                        Review clubs created by faculty, staff, and students. Restrict a club after a flag review if it is violating the rules. Administrators do not create clubs.
                    </p>
                </div>
            </header>

            <asp:Panel ID="pnlMessage" runat="server" CssClass="dtas-notice" Visible="false">
                <asp:Label ID="lblMessage" runat="server"></asp:Label>
            </asp:Panel>

            <!-- Clubs List -->
            <section class="standard-card rounded-xl overflow-hidden">
                <div class="px-6 py-4 border-b border-surface-container-high flex justify-between items-center">
                    <h3 class="font-title-lg text-title-lg text-primary">All Clubs</h3>
                    <asp:Literal ID="litClubCount" runat="server"></asp:Literal>
                </div>
                <div class="overflow-x-auto">
                    <table class="w-full text-left font-body-md">
                        <thead>
                            <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                <th class="px-6 py-3 font-semibold">Club</th>
                                <th class="px-6 py-3 font-semibold">Lead</th>
                                <th class="px-6 py-3 font-semibold">Members</th>
                                <th class="px-6 py-3 font-semibold">Flags</th>
                                <th class="px-6 py-3 font-semibold text-right">Actions</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-surface-container-high">
                            <asp:Repeater ID="rptClubs" runat="server" OnItemDataBound="rptClubs_ItemDataBound" OnItemCommand="rptClubs_ItemCommand">
                                <ItemTemplate>
                                    <tr class="hover:bg-surface-container-lowest transition-colors group">
                                        <td class="px-6 py-4">
                                            <div class="flex items-center gap-3">
                                                <div class="h-10 w-10 rounded-lg bg-primary-container flex items-center justify-center shrink-0">
                                                    <span class="material-symbols-outlined text-on-primary-container text-[20px]">groups</span>
                                                </div>
                                                <div>
                                                    <span class="font-semibold text-on-surface"><%# Eval("ClubName") %></span>
                                                    <asp:Label runat="server" Visible='<%# Convert.ToBoolean(Eval("IsRestricted")) %>'
                                                        CssClass="ml-2 px-3 py-1 rounded-full font-badge-cap text-badge-cap inline-block bg-[rgba(198,40,40,0.12)] text-[#c62828]">Restricted</asp:Label>
                                                    <p class="text-xs text-on-surface-variant truncate max-w-[240px]"><%# Eval("Description") %></p>
                                                </div>
                                            </div>
                                        </td>
                                        <td class="px-6 py-4">
                                            <asp:Label ID="lblLead" runat="server"></asp:Label>
                                        </td>
                                        <td class="px-6 py-4">
                                            <asp:LinkButton ID="btnViewMembers" runat="server" CommandName="ViewMembers" CommandArgument='<%# Eval("ClubID") %>' CssClass="text-primary font-label-md text-label-md hover:underline">
                                                <asp:Literal ID="litMemberCount" runat="server"></asp:Literal> members
                                            </asp:LinkButton>
                                        </td>
                                        <td class="px-6 py-4">
                                            <asp:HyperLink runat="server" NavigateUrl="~/Modules/Users/Flags.aspx"
                                                Visible='<%# Convert.ToInt32(Eval("FlagCount")) > 0 %>'
                                                CssClass="font-label-md text-label-md text-[#c62828] font-bold hover:underline">
                                                <%# Eval("FlagCount") %> open
                                            </asp:HyperLink>
                                            <asp:Label runat="server" Visible='<%# Convert.ToInt32(Eval("FlagCount")) == 0 %>'
                                                CssClass="text-sm text-on-surface-variant">None</asp:Label>
                                        </td>
                                        <td class="px-6 py-4 text-right">
                                            <div class="flex items-center justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                                                <a href='<%# ResolveUrl("~/Modules/Clubs/ClubWorkspace.aspx?ClubID=" + Eval("ClubID")) %>'
                                                    class="p-2 hover:bg-surface-container-low rounded-lg transition-colors" title="Workspace">
                                                    <span class="material-symbols-outlined text-outline hover:text-primary transition-colors text-[20px]">forum</span>
                                                </a>
                                                <asp:LinkButton ID="btnRestrict" runat="server" CommandName="Restrict" CommandArgument='<%# Eval("ClubID") %>'
                                                    Visible='<%# !Convert.ToBoolean(Eval("IsRestricted")) %>'
                                                    CssClass="p-2 hover:bg-error-container/50 rounded-lg transition-colors" ToolTip="Restrict club"
                                                    OnClientClick="return dtasConfirm(this, 'Restrict this club? Members will not be able to message, invite, join, or link it to events.');">
                                                    <span class="material-symbols-outlined text-outline hover:text-error transition-colors text-[20px]">block</span>
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="btnRestore" runat="server" CommandName="Restore" CommandArgument='<%# Eval("ClubID") %>'
                                                    Visible='<%# Convert.ToBoolean(Eval("IsRestricted")) %>'
                                                    CssClass="p-2 hover:bg-surface-container-low rounded-lg transition-colors" ToolTip="Restore club"
                                                    OnClientClick="return dtasConfirm(this, 'Restore this club so members can use it again?');">
                                                    <span class="material-symbols-outlined text-outline hover:text-primary transition-colors text-[20px]">lock_open</span>
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="btnDeleteClub" runat="server" CommandName="Delete" CommandArgument='<%# Eval("ClubID") %>' CssClass="p-2 hover:bg-error-container/50 rounded-lg transition-colors" ToolTip="Archive Club" OnClientClick="return dtasConfirm(this, 'Archive this club and its roster?');">
                                                    <span class="material-symbols-outlined text-outline hover:text-error transition-colors text-[20px]">delete</span>
                                                </asp:LinkButton>
                                            </div>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>
                <asp:Panel ID="pnlNoClubs" runat="server" Visible="false" CssClass="text-center py-16">
                    <span class="material-symbols-outlined text-5xl text-outline-variant mb-2 block">groups</span>
                    <p class="text-body-md text-on-surface-variant">No clubs have been created by faculty, staff, or students yet</p>
                </asp:Panel>
            </section>

            <!-- Members List Panel -->
            <asp:Panel ID="pnlMembers" runat="server" CssClass="standard-card rounded-xl mt-6 p-6" Visible="false">
                <div class="flex justify-between items-center mb-4">
                    <h3 class="font-title-lg text-title-lg text-primary"><asp:Literal ID="litMembersTitle" runat="server"></asp:Literal></h3>
                    <asp:Button ID="btnCloseMembers" runat="server" Text="Close" CssClass="px-4 py-2 border border-outline text-on-surface-variant rounded-lg font-label-md text-label-md cursor-pointer" OnClick="btnCloseMembers_Click" CausesValidation="false" />
                </div>
                <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                    <asp:Repeater ID="rptMembers" runat="server">
                        <ItemTemplate>
                            <div class="p-3 bg-surface-container-low rounded-lg flex items-center justify-between">
                                <div>
                                    <span class="font-label-md text-label-md font-bold"><%# Eval("FullName") %></span>
                                    <p class="text-xs text-on-surface-variant"><%# Eval("RoleName") %><%# Eval("IsLead") %></p>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                <asp:Panel ID="pnlNoMembers" runat="server" Visible="false" CssClass="text-center py-8 text-on-surface-variant">No members in this club.</asp:Panel>
            </asp:Panel>

        </main>
    </div>

</asp:Content>
