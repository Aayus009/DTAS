<%@ Page Title="Feedback Management" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Feedback.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Participation.Feedback" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <uc:AdminTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:AdminSidebar ActivePage="Feedback" runat="server" />

        <!-- Main Content Area -->
        <main class="dashboard-main flex-1 ml-64 p-8">

            <!-- Header Section -->
            <header class="flex justify-between items-end mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Feedback Management</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">Manage and respond to user feedback, complaints, suggestions and recommendations.</p>
                </div>
            </header>

            <!-- Stat Cards -->
            <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
                <!-- New Feedback -->
                <div class="standard-card p-6 rounded-xl">
                    <div class="flex items-center justify-between mb-4">
                        <div class="w-12 h-12 rounded-lg bg-primary/10 flex items-center justify-center">
                            <span class="material-symbols-outlined text-primary">new_releases</span>
                        </div>
                        <span class="text-xs font-medium text-primary bg-primary/10 px-2 py-1 rounded-full">Active</span>
                    </div>
                    <h3 class="text-2xl font-bold text-on-surface">
                        <asp:Label ID="lblNewCount" runat="server" Text="0"></asp:Label>
                    </h3>
                    <p class="text-sm text-on-surface-variant mt-1">New Feedback</p>
                </div>

                <!-- Under Review -->
                <div class="standard-card p-6 rounded-xl">
                    <div class="flex items-center justify-between mb-4">
                        <div class="w-12 h-12 rounded-lg bg-amber-500/10 flex items-center justify-center">
                            <span class="material-symbols-outlined text-amber-500">pending</span>
                        </div>
                        <span class="text-xs font-medium text-amber-600 bg-amber-500/10 px-2 py-1 rounded-full">Pending</span>
                    </div>
                    <h3 class="text-2xl font-bold text-on-surface">
                        <asp:Label ID="lblUnderReviewCount" runat="server" Text="0"></asp:Label>
                    </h3>
                    <p class="text-sm text-on-surface-variant mt-1">Under Review</p>
                </div>

                <!-- Resolved -->
                <div class="standard-card p-6 rounded-xl">
                    <div class="flex items-center justify-between mb-4">
                        <div class="w-12 h-12 rounded-lg bg-green-500/10 flex items-center justify-center">
                            <span class="material-symbols-outlined text-green-500">check_circle</span>
                        </div>
                        <span class="text-xs font-medium text-green-600 bg-green-500/10 px-2 py-1 rounded-full">Done</span>
                    </div>
                    <h3 class="text-2xl font-bold text-on-surface">
                        <asp:Label ID="lblResolvedCount" runat="server" Text="0"></asp:Label>
                    </h3>
                    <p class="text-sm text-on-surface-variant mt-1">Resolved</p>
                </div>
            </div>

            <!-- Filters -->
            <div class="standard-card p-6 rounded-xl mb-6">
                <div class="flex flex-col md:flex-row gap-4">
                    <!-- Search -->
                    <div class="flex-1 relative">
                        <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-on-surface-variant">search</span>
                        <asp:TextBox ID="txtSearchFilter" runat="server" placeholder="Search by title..."
                            CssClass="w-full pl-10 pr-4 py-2.5 rounded-lg border border-outline-variant/30 bg-surface text-on-surface placeholder-on-surface-variant/50 focus:outline-none focus:ring-2 focus:ring-primary/40 focus:border-primary transition-all"></asp:TextBox>
                    </div>

                    <!-- Category Filter -->
                    <div class="relative">
                        <asp:DropDownList ID="ddlCategory" runat="server"
                            CssClass="pl-4 pr-10 py-2.5 rounded-lg border border-outline-variant/30 bg-surface text-on-surface appearance-none cursor-pointer focus:outline-none focus:ring-2 focus:ring-primary/40 focus:border-primary transition-all min-w-[160px]"
                            AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                            <asp:ListItem Value="All" Text="All Categories" />
                            <asp:ListItem Value="Suggestion" Text="Suggestion" />
                            <asp:ListItem Value="Complaint" Text="Complaint" />
                            <asp:ListItem Value="Recommendation" Text="Recommendation" />
                        </asp:DropDownList>
                        <span class="material-symbols-outlined absolute right-3 top-1/2 -translate-y-1/2 text-on-surface-variant pointer-events-none text-sm">expand_more</span>
                    </div>

                    <!-- Status Filter -->
                    <div class="relative">
                        <asp:DropDownList ID="ddlStatus" runat="server"
                            CssClass="pl-4 pr-10 py-2.5 rounded-lg border border-outline-variant/30 bg-surface text-on-surface appearance-none cursor-pointer focus:outline-none focus:ring-2 focus:ring-primary/40 focus:border-primary transition-all min-w-[160px]"
                            AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                            <asp:ListItem Value="All" Text="All Status" />
                            <asp:ListItem Value="New" Text="New" />
                            <asp:ListItem Value="Under Review" Text="Under Review" />
                            <asp:ListItem Value="Resolved" Text="Resolved" />
                        </asp:DropDownList>
                        <span class="material-symbols-outlined absolute right-3 top-1/2 -translate-y-1/2 text-on-surface-variant pointer-events-none text-sm">expand_more</span>
                    </div>
                </div>
            </div>

            <!-- Feedback Table -->
            <div class="standard-card rounded-xl overflow-hidden">
                <div class="px-6 py-4 border-b border-surface-container-high">
                    <h3 class="font-title-lg text-title-lg text-primary">All Feedback</h3>
                </div>
                <div class="overflow-x-auto">
                    <table class="w-full text-left font-body-md">
                        <thead>
                            <tr class="bg-surface-container-low text-on-surface-variant border-b border-surface-container-high">
                                <th class="px-6 py-3 font-semibold">Title</th>
                                <th class="px-6 py-3 font-semibold">Category</th>
                                <th class="px-6 py-3 font-semibold">Submitter</th>
                                <th class="px-6 py-3 font-semibold">Rating</th>
                                <th class="px-6 py-3 font-semibold">Status</th>
                                <th class="px-6 py-3 font-semibold">Date</th>
                                <th class="px-6 py-3 font-semibold">Actions</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-surface-container-high">
                            <asp:Repeater ID="rptFeedback" runat="server" OnItemCommand="rptFeedback_ItemCommand">
                                <ItemTemplate>
                                    <tr class="hover:bg-surface-container-lowest transition-colors">
                                        <td class="px-6 py-4">
                                            <div class="font-semibold"><%# Eval("Title") %></div>
                                            <div class="text-sm text-on-surface-variant mt-0.5 line-clamp-1"><%# Eval("Description") %></div>
                                        </td>
                                        <td class="px-6 py-4">
                                            <span class='<%# GetCategoryBadgeClass(Eval("Category").ToString()) %>'><%# Eval("Category") %></span>
                                        </td>
                                        <td class="px-6 py-4">
                                            <div class="flex items-center gap-2">
                                                <div class="w-7 h-7 rounded-full bg-primary-container flex items-center justify-center text-xs font-semibold text-on-primary-container"><%# GetInitials(Eval("FullName").ToString()) %></div>
                                                <span><%# Eval("FullName") %></span>
                                            </div>
                                        </td>
                                        <td class="px-6 py-4">
                                            <div class="flex items-center gap-0.5">
                                                <%# GetStarRating(Eval("Rating")) %>
                                            </div>
                                        </td>
                                        <td class="px-6 py-4">
                                            <span class='<%# GetStatusBadgeClass(Eval("Status").ToString()) %>'><%# Eval("Status") %></span>
                                        </td>
                                        <td class="px-6 py-4 text-on-surface-variant"><%# Eval("CreatedAt", "{0:MMM dd, yyyy}") %></td>
                                        <td class="px-6 py-4">
                                            <div class="flex items-center gap-2">
                                                <asp:LinkButton ID="btnReview" runat="server" CommandName="Review" CommandArgument='<%# Eval("FeedbackID") %>'
                                                    CssClass="p-1.5 rounded-lg hover:bg-primary/10 text-primary transition-colors" ToolTip="Review">
                                                    <span class="material-symbols-outlined text-lg">rate_review</span>
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="btnResolve" runat="server" CommandName="Resolve" CommandArgument='<%# Eval("FeedbackID") %>'
                                                    CssClass="p-1.5 rounded-lg hover:bg-green-500/10 text-green-500 transition-colors" ToolTip="Resolve"
                                                    Visible='<%# Eval("Status").ToString() != "Resolved" %>'>
                                                    <span class="material-symbols-outlined text-lg">check_circle</span>
                                                </asp:LinkButton>
                                            </div>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>
                <asp:Panel ID="pnlNoFeedback" runat="server" Visible="false" CssClass="p-8 text-center">
                    <span class="material-symbols-outlined text-outline text-4xl mb-2">inbox</span>
                    <p class="font-body-md text-on-surface-variant">No feedback found matching your criteria.</p>
                </asp:Panel>
            </div>
        </main>
    </div>

    <!-- Review Modal -->
    <asp:Panel ID="pnlReviewModal" runat="server" Visible="false"
        CssClass="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm">
        <div class="bg-surface-container rounded-2xl shadow-2xl border border-outline-variant/30 w-full max-w-lg mx-4 overflow-hidden">
            <div class="px-6 py-4 border-b border-outline-variant/30 flex items-center justify-between">
                <h3 class="text-lg font-semibold text-on-surface">Review Feedback</h3>
                <asp:LinkButton ID="btnCloseModal" runat="server" OnClick="btnCloseModal_Click"
                    CssClass="p-1 rounded-lg hover:bg-surface-variant/50 text-on-surface-variant transition-colors">
                    <span class="material-symbols-outlined">close</span>
                </asp:LinkButton>
            </div>
            <div class="px-6 py-5 space-y-4">
                <asp:HiddenField ID="hfFeedbackID" runat="server" />

                <div>
                    <label class="block text-sm font-medium text-on-surface mb-1">Title</label>
                    <asp:Label ID="lblModalTitle" runat="server"
                        CssClass="block text-sm text-on-surface-variant bg-surface p-3 rounded-lg border border-outline-variant/20"></asp:Label>
                </div>

                <div>
                    <label class="block text-sm font-medium text-on-surface mb-1">Description</label>
                    <asp:Label ID="lblModalDescription" runat="server"
                        CssClass="block text-sm text-on-surface-variant bg-surface p-3 rounded-lg border border-outline-variant/20"></asp:Label>
                </div>

                <div>
                    <label class="block text-sm font-medium text-on-surface mb-1">Status</label>
                    <div class="relative">
                        <asp:DropDownList ID="ddlModalStatus" runat="server"
                            CssClass="w-full pl-4 pr-10 py-2.5 rounded-lg border border-outline-variant/30 bg-surface text-on-surface appearance-none cursor-pointer focus:outline-none focus:ring-2 focus:ring-primary/40 focus:border-primary transition-all">
                            <asp:ListItem Value="New" Text="New" />
                            <asp:ListItem Value="Under Review" Text="Under Review" />
                            <asp:ListItem Value="Resolved" Text="Resolved" />
                        </asp:DropDownList>
                        <span class="material-symbols-outlined absolute right-3 top-1/2 -translate-y-1/2 text-on-surface-variant pointer-events-none text-sm">expand_more</span>
                    </div>
                </div>

                <div>
                    <label class="block text-sm font-medium text-on-surface mb-1">Admin Response</label>
                    <asp:TextBox ID="txtAdminResponse" runat="server" TextMode="MultiLine" Rows="4"
                        CssClass="w-full px-4 py-2.5 rounded-lg border border-outline-variant/30 bg-surface text-on-surface placeholder-on-surface-variant/50 focus:outline-none focus:ring-2 focus:ring-primary/40 focus:border-primary transition-all resize-none"
                        placeholder="Enter your response..."></asp:TextBox>
                </div>
            </div>
            <div class="px-6 py-4 border-t border-outline-variant/30 flex items-center justify-end gap-3">
                <asp:LinkButton ID="btnCancel" runat="server" OnClick="btnCloseModal_Click"
                    CssClass="px-4 py-2 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 text-sm font-medium transition-colors">Cancel</asp:LinkButton>
                <asp:Button ID="btnUpdateFeedback" runat="server" Text="Update Feedback" OnClick="btnUpdateFeedback_Click"
                    CssClass="px-5 py-2 rounded-lg bg-primary text-on-primary text-sm font-medium hover:shadow-md transition-all" />
            </div>
        </div>
    </asp:Panel>
</asp:Content>
