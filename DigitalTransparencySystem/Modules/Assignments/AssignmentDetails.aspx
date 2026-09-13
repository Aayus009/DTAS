<%@ Page Title="Assignment | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AssignmentDetails.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Assignments.AssignmentDetails" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <uc:AdminTopbar ID="adminTop" runat="server" Visible="false" />
    <uc:UserTopbar ID="userTop" runat="server" Visible="false" />

    <div class="flex min-h-screen">
        <uc:AdminSidebar ID="adminSide" ActivePage="Assignments" runat="server" Visible="false" />
        <uc:UserSidebar ID="userSide" ActivePage="Assignments" runat="server" Visible="false" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            <header class="flex justify-between items-start mb-8 gap-4">
                <div>
                    <a href="<%= ResolveUrl("~/Modules/Assignments/Assignments.aspx") %>" class="inline-flex items-center gap-1 text-primary font-bold text-sm mb-3">
                        <span class="material-symbols-outlined text-[20px]">arrow_back</span>
                        Back
                    </a>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2"><asp:Literal ID="litName" runat="server"></asp:Literal></h1>
                    <p class="text-on-surface-variant">Code <span class="font-mono text-primary"><asp:Literal ID="litCode" runat="server"></asp:Literal></span>
                        - Deadline <asp:Literal ID="litDeadline" runat="server"></asp:Literal>
                        - <asp:Literal ID="litStatus" runat="server"></asp:Literal></p>
                    <asp:Panel ID="pnlNearDeadline" runat="server" Visible="false" CssClass="mt-3 p-3 rounded-xl bg-[rgba(230,126,0,0.12)] text-[#b86b00] font-label-md">
                        <asp:Literal ID="litNearDeadline" runat="server"></asp:Literal>
                    </asp:Panel>
                    <p class="mt-3 text-on-surface-variant"><asp:Literal ID="litDescription" runat="server"></asp:Literal></p>
                </div>
                <div class="flex flex-col gap-2">
                    <asp:HyperLink ID="lnkScheduleMeeting" runat="server"
                        CssClass="inline-flex items-center justify-center gap-1 px-4 py-2 bg-primary text-on-primary rounded-xl font-label-md font-bold">
                        <span class="material-symbols-outlined text-[18px]">videocam</span>
                        Schedule meeting
                    </asp:HyperLink>
                    <asp:HyperLink ID="lnkAssignmentConnect" runat="server" Visible="false"
                        CssClass="inline-flex items-center justify-center gap-1 px-4 py-2 border border-outline rounded-xl font-label-md font-bold">
                        <span class="material-symbols-outlined text-[18px]">forum</span>
                        Open Connect
                    </asp:HyperLink>
                    <asp:Button ID="btnClose" runat="server" Text="Close assignment" CssClass="px-4 py-2 border border-outline rounded-xl font-label-md" OnClick="btnClose_Click" />
                    <asp:Panel ID="pnlExtend" runat="server" CssClass="p-3 rounded-xl bg-surface-container-low space-y-2">
                        <p class="text-xs text-on-surface-variant">Faculty can change the deadline at any time. Students cannot extend it.</p>
                        <asp:TextBox ID="txtNewDeadline" runat="server" TextMode="DateTimeLocal"
                            CssClass="w-full p-2 border border-outline rounded-lg font-body-md"></asp:TextBox>
                        <asp:Button ID="btnExtendDeadline" runat="server" Text="Save deadline" CssClass="w-full px-4 py-2 bg-primary text-on-primary rounded-xl font-label-md font-bold" OnClick="btnExtendDeadline_Click" />
                    </asp:Panel>
                </div>
            </header>

            <asp:Label ID="lblMessage" runat="server" CssClass="dtas-notice"></asp:Label>

            <section class="standard-card rounded-xl overflow-hidden">
                <div class="px-6 py-4 border-b border-surface-container-high">
                    <h3 class="font-title-lg text-title-lg text-primary">Student subgroups</h3>
                    <p class="text-xs text-on-surface-variant">Progress is completed tasks / total tasks. Contribution scores are calculated by the database formula.</p>
                </div>
                <asp:Repeater ID="rptGroups" runat="server" OnItemCommand="rptGroups_ItemCommand">
                    <HeaderTemplate>
                        <table class="w-full text-left font-body-md">
                            <thead>
                                <tr class="bg-surface-container-low text-on-surface-variant">
                                    <th class="px-6 py-3">Group</th>
                                    <th class="px-6 py-3">Leader</th>
                                    <th class="px-6 py-3">Progress</th>
                                    <th class="px-6 py-3">Finalized</th>
                                    <th class="px-6 py-3 text-right">Action</th>
                                </tr>
                            </thead>
                            <tbody class="divide-y divide-surface-container-high">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td class="px-6 py-4 font-semibold"><%# Eval("GroupName") %></td>
                            <td class="px-6 py-4"><%# Eval("LeaderName") %></td>
                            <td class="px-6 py-4"><%# DigitalTransparencySystem.Helpers.AssignmentService.FormatPercent(Eval("CompletionPercentage")) %>
                                <span class="text-xs text-outline">(<%# Eval("TotalTasks") %> tasks)</span></td>
                            <td class="px-6 py-4"><%# Convert.ToBoolean(Eval("IsFinalized")) ? "Yes" : "No" %></td>
                            <td class="px-6 py-4 text-right">
                                <a class="text-primary font-bold mr-3" href='<%# ResolveUrl("~/Modules/Assignments/GroupWorkspace.aspx?GroupID=" + Eval("GroupID")) %>'>Open</a>
                                <a class="text-primary font-bold mr-3" href='<%# ResolveUrl("~/Modules/Assignments/ContributionReport.aspx?GroupID=" + Eval("GroupID")) %>'>Report</a>
                                <asp:LinkButton runat="server" CommandName="Remove" CommandArgument='<%# Eval("GroupID") %>'
                                    CssClass="text-error font-bold" OnClientClick="return dtasConfirm(this, 'Remove this subgroup?');">Remove</asp:LinkButton>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                            </tbody>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="px-6 py-12 text-on-surface-variant">No subgroups yet. Students create them with the assignment code.</asp:Panel>
            </section>
        </main>
    </div>
</asp:Content>
