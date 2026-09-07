<%@ Page Title="Flags | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Flags.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Users.Flags" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <uc:AdminTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:AdminSidebar ActivePage="Flags" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            <header class="flex justify-between items-end mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Content flags</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">
                        Review community reports of users, events, tasks, and clubs. Generated file reports stay under Reports.
                    </p>
                </div>
                <asp:DropDownList ID="ddlStatus" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged"
                    CssClass="p-3 bg-surface-container-low border border-outline rounded-xl font-body-md">
                    <asp:ListItem Text="Pending" Value="Pending" Selected="True" />
                    <asp:ListItem Text="Reviewing" Value="Reviewing" />
                    <asp:ListItem Text="Resolved" Value="Resolved" />
                    <asp:ListItem Text="Rejected" Value="Rejected" />
                    <asp:ListItem Text="Escalated" Value="Escalated" />
                    <asp:ListItem Text="All" Value="All" />
                </asp:DropDownList>
            </header>

            <asp:Label ID="lblMessage" runat="server" CssClass="font-label-md block mb-4"></asp:Label>
            <asp:HiddenField ID="hfResolution" runat="server" />

            <div class="standard-card rounded-xl overflow-hidden">
                <div class="overflow-x-auto">
                    <asp:Repeater ID="rptFlags" runat="server" OnItemCommand="rptFlags_ItemCommand">
                        <HeaderTemplate>
                            <table class="w-full text-left font-body-md">
                                <thead>
                                    <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                        <th class="px-6 py-3 font-semibold">Reporter</th>
                                        <th class="px-6 py-3 font-semibold">Target</th>
                                        <th class="px-6 py-3 font-semibold">Reason</th>
                                        <th class="px-6 py-3 font-semibold">Submitted</th>
                                        <th class="px-6 py-3 font-semibold">Status</th>
                                        <th class="px-6 py-3 font-semibold text-right">Action</th>
                                    </tr>
                                </thead>
                                <tbody class="divide-y divide-surface-container-high">
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr class="hover:bg-surface-container-lowest">
                                <td class="px-6 py-4">
                                    <span class="font-semibold text-on-surface"><%# Eval("ReporterName") %></span>
                                    <p class="text-xs text-on-surface-variant"><%# Eval("ReporterEmail") %></p>
                                </td>
                                <td class="px-6 py-4">
                                    <span class="font-semibold"><%# Eval("TargetType") %> #<%# Eval("TargetID") %></span>
                                    <asp:HyperLink runat="server" Visible='<%# IsClubTarget(Eval("TargetType")) %>'
                                        NavigateUrl='<%# ResolveUrl("~/Modules/Clubs/ClubWorkspace.aspx?ClubID=" + Eval("TargetID")) %>'
                                        CssClass="block text-xs text-primary font-bold mt-1">Open club</asp:HyperLink>
                                    <p class="text-xs text-on-surface-variant mt-1"><%# Eval("Description") %></p>
                                </td>
                                <td class="px-6 py-4 text-on-surface-variant"><%# Eval("Reason") %></td>
                                <td class="px-6 py-4 text-on-surface-variant whitespace-nowrap"><%# Eval("CreatedAt", "{0:MMM dd, yyyy}") %></td>
                                <td class="px-6 py-4"><%# Eval("Status") %></td>
                                <td class="px-6 py-4 text-right whitespace-nowrap">
                                    <asp:Panel runat="server" Visible='<%# IsOpen(Eval("Status")) %>'>
                                        <asp:LinkButton ID="btnReview" runat="server" CommandName="Reviewing" CommandArgument='<%# Eval("ReportID") %>'
                                            CssClass="px-3 py-1.5 bg-surface-variant text-on-surface rounded-lg font-label-md font-bold">Review</asp:LinkButton>
                                        <asp:LinkButton ID="btnResolve" runat="server" CommandName="Resolved" CommandArgument='<%# Eval("ReportID") %>'
                                            CssClass="px-3 py-1.5 bg-tertiary-container/30 text-on-tertiary-container rounded-lg font-label-md font-bold"
                                            OnClientClick="return promptFlagNote(event, this, 'Resolution note (required):');">Resolve</asp:LinkButton>
                                        <asp:LinkButton ID="btnReject" runat="server" CommandName="Rejected" CommandArgument='<%# Eval("ReportID") %>'
                                            CssClass="px-3 py-1.5 bg-error-container/30 text-error rounded-lg font-label-md font-bold"
                                            OnClientClick="return promptFlagNote(event, this, 'Rejection note (required):');">Reject</asp:LinkButton>
                                        <asp:LinkButton ID="btnEscalate" runat="server" CommandName="Escalated" CommandArgument='<%# Eval("ReportID") %>'
                                            CssClass="px-3 py-1.5 border border-outline text-on-surface-variant rounded-lg font-label-md font-bold"
                                            OnClientClick="return promptFlagNote(event, this, 'Escalation note (optional):');">Escalate</asp:LinkButton>
                                        <asp:LinkButton ID="btnRestrictClub" runat="server" CommandName="RestrictClub" CommandArgument='<%# Eval("ReportID") %>'
                                            Visible='<%# IsClubTarget(Eval("TargetType")) %>'
                                            CssClass="px-3 py-1.5 bg-error-container/30 text-error rounded-lg font-label-md font-bold"
                                            OnClientClick="return promptFlagNote(event, this, 'Restrict this club after review? Add a note (required):');">Restrict club</asp:LinkButton>
                                    </asp:Panel>
                                    <asp:Panel runat="server" Visible='<%# !IsOpen(Eval("Status")) %>'>
                                        <span class="text-xs text-on-surface-variant"><%# Eval("Resolution") %></span>
                                    </asp:Panel>
                                </td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                                </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                    <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="px-6 py-12 text-center text-on-surface-variant">
                        No flags in this filter.
                    </asp:Panel>
                </div>
            </div>
        </main>
    </div>
</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript">
        function promptFlagNote(event, link, message) {
            if (event) event.preventDefault();
            return dtasPromptPostback(link, '<%= hfResolution.ClientID %>', message, {
                title: 'Flag action',
                required: /required/i.test(message || ''),
                requiredText: 'A note is required.',
                okText: 'Save'
            });
        }
    </script>
</asp:Content>
