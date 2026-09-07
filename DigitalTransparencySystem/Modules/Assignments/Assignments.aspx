<%@ Page Title="Assignments | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Assignments.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Assignments.Assignments" %>
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
            <header class="mb-8">
                <a href="<%= ResolveUrl("~/Modules/Dashboard/UsersDashboard.aspx") %>" class="inline-flex items-center gap-1 text-primary font-bold text-sm mb-3">
                    <span class="material-symbols-outlined text-[20px]">arrow_back</span>
                    Back
                </a>
                <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Academic assignments</h1>
                <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">
                    Faculty creates the assignment and deadline. Students create the subgroup, invite classmates, choose a leader, and enter their tasks. You monitor progress. This is separate from governance Tasks.
                </p>
            </header>

            <asp:Label ID="lblMessage" runat="server" CssClass="font-label-md block mb-4"></asp:Label>

            <section class="standard-card rounded-xl p-6 mb-6 max-w-2xl">
                <h3 class="font-title-lg text-title-lg text-primary mb-4">Create assignment</h3>
                <div class="space-y-4">
                    <asp:TextBox ID="txtName" runat="server" MaxLength="200"
                        CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md"
                        placeholder="Assignment name"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvAssignmentName" runat="server" ControlToValidate="txtName"
                        ValidationGroup="CreateAssignment" CssClass="field-error" Display="Dynamic"
                        ErrorMessage="Assignment name is required."></asp:RequiredFieldValidator>
                    <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="3"
                        CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md resize-none"
                        placeholder="Description"></asp:TextBox>
                    <div>
                        <label class="font-label-md text-on-surface-variant block mb-2">Deadline</label>
                        <asp:TextBox ID="txtDeadline" runat="server" TextMode="DateTimeLocal"
                            CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md"></asp:TextBox>
                    </div>
                    <asp:RequiredFieldValidator ID="rfvDeadline" runat="server" ControlToValidate="txtDeadline"
                        ValidationGroup="CreateAssignment" CssClass="field-error" Display="Dynamic"
                        ErrorMessage="Deadline is required."></asp:RequiredFieldValidator>
                    <asp:Button ID="btnCreate" runat="server" Text="Create assignment" CssClass="btn-primary" OnClick="btnCreate_Click" ValidationGroup="CreateAssignment" />
                </div>
            </section>

            <section class="standard-card rounded-xl overflow-hidden">
                <div class="px-6 py-4 border-b border-surface-container-high">
                    <h3 class="font-title-lg text-title-lg text-primary">Assignments</h3>
                </div>
                <asp:Repeater ID="rptAssignments" runat="server">
                    <HeaderTemplate>
                        <table class="w-full text-left font-body-md">
                            <thead>
                                <tr class="bg-surface-container-low text-on-surface-variant">
                                    <th class="px-6 py-3">Name</th>
                                    <th class="px-6 py-3">Code</th>
                                    <th class="px-6 py-3">Deadline</th>
                                    <th class="px-6 py-3">Groups</th>
                                    <th class="px-6 py-3">Status</th>
                                </tr>
                            </thead>
                            <tbody class="divide-y divide-surface-container-high">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr class="hover:bg-surface-container-lowest">
                            <td class="px-6 py-4">
                                <a class="font-semibold text-primary hover:underline"
                                    href='<%# ResolveUrl("~/Modules/Assignments/AssignmentDetails.aspx?AssignmentID=" + Eval("AssignmentID")) %>'><%# Eval("AssignmentName") %></a>
                            </td>
                            <td class="px-6 py-4 font-mono"><%# Eval("AssignmentCode") %></td>
                            <td class="px-6 py-4"><%# Eval("Deadline", "{0:MMM dd, yyyy}") %></td>
                            <td class="px-6 py-4"><%# Eval("GroupCount") %></td>
                            <td class="px-6 py-4"><%# Eval("Status") %></td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                            </tbody>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="px-6 py-12 text-on-surface-variant">No assignments yet.</asp:Panel>
            </section>
        </main>
    </div>
</asp:Content>
