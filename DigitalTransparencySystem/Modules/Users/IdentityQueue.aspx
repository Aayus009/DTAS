<%@ Page Title="Identity Queue | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="IdentityQueue.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Users.IdentityQueue" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <uc:AdminTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:AdminSidebar ActivePage="Identity" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            <header class="flex justify-between items-end mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Identity verification</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">Review submitted ID documents. Approve to unlock core features, or reject with a reason.</p>
                </div>
                <asp:DropDownList ID="ddlStatus" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged"
                    CssClass="p-3 bg-surface-container-low border border-outline rounded-xl font-body-md">
                    <asp:ListItem Text="Pending" Value="Pending" Selected="True" />
                    <asp:ListItem Text="Rejected" Value="Rejected" />
                    <asp:ListItem Text="Verified" Value="Verified" />
                    <asp:ListItem Text="All current documents" Value="All" />
                </asp:DropDownList>
            </header>

            <asp:Label ID="lblMessage" runat="server" CssClass="font-label-md block mb-4"></asp:Label>
            <asp:HiddenField ID="hfRejectReason" runat="server" />

            <div class="standard-card rounded-xl overflow-hidden">
                <div class="overflow-x-auto">
                    <asp:Repeater ID="rptQueue" runat="server" OnItemCommand="rptQueue_ItemCommand">
                        <HeaderTemplate>
                            <table class="w-full text-left font-body-md">
                                <thead>
                                    <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                        <th class="px-6 py-3 font-semibold">Member</th>
                                        <th class="px-6 py-3 font-semibold">Role</th>
                                        <th class="px-6 py-3 font-semibold">Institutional ID</th>
                                        <th class="px-6 py-3 font-semibold">Document</th>
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
                                    <span class="font-semibold text-on-surface"><%# Eval("FullName") %></span>
                                    <p class="text-xs text-on-surface-variant"><%# Eval("Email") %></p>
                                </td>
                                <td class="px-6 py-4 text-on-surface-variant"><%# Eval("RoleName") %></td>
                                <td class="px-6 py-4 text-on-surface-variant"><%# string.IsNullOrEmpty(Convert.ToString(Eval("InstitutionalID"))) ? "-" : Eval("InstitutionalID") %></td>
                                <td class="px-6 py-4">
                                    <a class="text-primary font-bold hover:underline" target="_blank"
                                        href='<%# ResolveUrl("~/Modules/Users/ViewIdentityDocument.aspx?id=" + Eval("DocumentID")) %>'><%# Eval("OriginalFileName") %></a>
                                </td>
                                <td class="px-6 py-4 text-on-surface-variant whitespace-nowrap"><%# Eval("UploadDate", "{0:MMM dd, yyyy}") %></td>
                                <td class="px-6 py-4"><%# Eval("VerificationStatus") %></td>
                                <td class="px-6 py-4 text-right">
                                    <asp:Panel runat="server" Visible='<%# string.Equals(Convert.ToString(Eval("VerificationStatus")), "Pending", StringComparison.OrdinalIgnoreCase) %>'>
                                        <asp:LinkButton ID="btnApprove" runat="server" CommandName="Approve" CommandArgument='<%# Eval("UserID") %>'
                                            CssClass="px-3 py-1.5 bg-tertiary-container/30 text-on-tertiary-container rounded-lg font-label-md font-bold">Approve</asp:LinkButton>
                                        <asp:LinkButton ID="btnReject" runat="server" CommandName="Reject" CommandArgument='<%# Eval("UserID") %>'
                                            CssClass="px-3 py-1.5 bg-error-container/30 text-error rounded-lg font-label-md font-bold"
                                            OnClientClick="return promptIdentityReject(event, this);">Reject</asp:LinkButton>
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
                        No documents in this filter.
                    </asp:Panel>
                </div>
            </div>
        </main>
    </div>
</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript">
        function promptIdentityReject(event, link) {
            if (event) event.preventDefault();
            return dtasPromptPostback(link, '<%= hfRejectReason.ClientID %>', 'Rejection reason (required):', {
                title: 'Reject identity document',
                required: true,
                requiredText: 'A rejection reason is required.',
                okText: 'Reject'
            });
        }
    </script>
</asp:Content>
