<%@ Page Title="Decision Preview | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UserDecisionPreview.aspx.cs" Inherits="DigitalTransparencySystem.Modules.UserDecisions.UserDecisionPreview" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
    <style>
        .timeline-vertical { position: relative; padding-left: 32px; }
        .timeline-vertical::before {
            content: '';
            position: absolute;
            left: 15px;
            top: 0;
            bottom: 0;
            width: 2px;
            background: linear-gradient(to bottom, var(--md-sys-color-primary), var(--md-sys-color-outline-variant));
        }
        .timeline-step { position: relative; padding-bottom: 32px; }
        .timeline-step:last-child { padding-bottom: 0; }
        .timeline-dot {
            position: absolute;
            left: -32px;
            top: 2px;
            width: 24px;
            height: 24px;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 1;
        }
        .timeline-dot-completed {
            background-color: var(--md-sys-color-primary);
            color: white;
        }
        .timeline-dot-current {
            background-color: var(--md-sys-color-primary);
            color: white;
            box-shadow: 0 0 0 4px rgba(0,35,111,0.2);
        }
        .timeline-dot-pending {
            background-color: var(--md-sys-color-surface-container-high);
            color: var(--md-sys-color-outline);
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <uc:UserTopbar runat="server" />

    <div class="flex min-h-screen">

        <uc:UserSidebar ActivePage="Decisions" runat="server" />

        <!-- Main Content Area -->
        <main class="dashboard-main flex-1 ml-64 p-8">

            <!-- Breadcrumb -->
            <nav class="flex items-center gap-2 mb-6 font-label-md text-label-md text-on-surface-variant">
                <a href="<%= ResolveUrl("~/Transparency.aspx") %>" class="hover:text-primary transition-colors">Transparency</a>
                <span class="material-symbols-outlined text-[16px]">chevron_right</span>
                <a href="<%= ResolveUrl("~/Modules/UserDecisions/UserDecisions.aspx") %>" class="hover:text-primary transition-colors">My Decisions</a>
                <span class="material-symbols-outlined text-[16px]">chevron_right</span>
                <span class="text-primary font-semibold">Decision Preview</span>
            </nav>

            <!-- Header Section -->
            <header class="flex justify-between items-start mb-8">
                <div>
                    <div class="flex items-center gap-4 mb-3 flex-wrap">
                        <asp:Literal ID="litTitle" runat="server" Text="Decision Title"></asp:Literal>
                        <asp:Literal ID="litStatusBadge" runat="server"></asp:Literal>
                        <asp:Literal ID="litPriorityBadge" runat="server"></asp:Literal>
                    </div>
                    <div class="flex items-center gap-3">
                        <span class="font-body-md text-body-md text-on-surface-variant">
                            <span class="material-symbols-outlined text-[16px] align-middle">person</span>
                            Responsible: <asp:Literal ID="litResponsiblePerson" runat="server"></asp:Literal>
                        </span>
                    </div>
                </div>
                <div class="flex gap-3">
                    <asp:HyperLink ID="lnkBackToList" runat="server" NavigateUrl="~/Transparency.aspx" CssClass="py-2 px-5 border border-outline text-on-surface-variant rounded-lg font-label-md text-label-md font-bold hover:bg-surface-container-low transition-colors cursor-pointer inline-flex items-center gap-2">
                        <span class="material-symbols-outlined text-[18px]">arrow_back</span> Back
                    </asp:HyperLink>
                </div>
            </header>

            <!-- Error Message -->
            <asp:Panel ID="pnlError" runat="server" CssClass="mb-6 p-4 bg-error-container border border-error/30 rounded-xl flex items-center gap-3" Visible="false">
                <span class="material-symbols-outlined text-error">error</span>
                <asp:Label ID="lblError" runat="server" CssClass="font-label-md text-label-md text-on-error-container"></asp:Label>
            </asp:Panel>

            <!-- Status Lifecycle Timeline -->
            <section class="standard-card rounded-xl p-6 mb-6">
                <h3 class="font-title-lg text-title-lg text-primary mb-6">Decision Lifecycle</h3>
                <div class="flex items-center justify-between relative">
                    <div class="absolute top-4 left-0 right-0 h-1 bg-surface-container-high rounded-full"></div>
                    <div class="absolute top-4 left-0 h-1 bg-primary rounded-full transition-all" style="width: <asp:Literal ID="litTimelineWidth" runat="server" Text="0%"></asp:Literal>"></div>
                    <asp:Repeater ID="rptTimeline" runat="server">
                        <ItemTemplate>
                            <div class="relative flex flex-col items-center z-10">
                                <div class="w-8 h-8 rounded-full flex items-center justify-center ring-4 ring-surface" style='<%# Eval("DotClass") %>'>
                                    <span class="material-symbols-outlined text-[18px]" style='<%# Eval("IconColor") %>'><%# Eval("Icon") %></span>
                                </div>
                                <span class="mt-2 font-label-sm text-label-sm text-on-surface-variant text-center"><%# Eval("Label") %></span>
                                <span class="text-xs text-outline mt-1"><%# Eval("Date") %></span>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </section>

            <!-- Execution Progress (auto-rolled-up from decision's tasks) -->
            <section class="standard-card rounded-xl p-6 mb-6">
                <div class="flex justify-between items-center mb-3">
                    <h3 class="font-title-lg text-title-lg text-primary">Execution Progress</h3>
                    <span class="font-label-lg text-label-lg font-bold text-on-surface-variant"><asp:Literal ID="litProgressPercent" runat="server" Text="0%"></asp:Literal></span>
                </div>
                <div class="h-3 bg-surface-container-high rounded-full overflow-hidden">
                    <div class="h-full bg-[#2e7d32] rounded-full transition-all duration-700" style="width: <asp:Literal ID="litProgressBar" runat="server" Text="0%"></asp:Literal>"></div>
                </div>
                <p class="mt-2 text-xs text-on-surface-variant">Auto-calculated from the tasks created under this decision.</p>
            </section>

            <!-- Main Layout: 2 Column Asymmetric -->
            <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">

                <!-- Left Column: Details (Wide) -->
                <div class="lg:col-span-2 space-y-6">

                    <!-- Decision Overview -->
                    <section class="standard-card rounded-xl overflow-hidden">
                        <div class="px-6 py-4 border-b border-surface-container-high">
                            <h3 class="font-title-lg text-title-lg text-primary">Decision Overview</h3>
                        </div>
                        <div class="p-6">
                            <div class="mb-6">
                                <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-2">Description</span>
                                <asp:Literal ID="litDescription" runat="server" Text="No description available."></asp:Literal>
                            </div>
                            <div class="grid grid-cols-2 gap-6">
                                <div>
                                    <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-2">Related Meeting</span>
                                    <p class="font-body-md text-body-md flex items-center gap-2">
                                        <span class="material-symbols-outlined text-[18px] text-primary">groups</span>
                                        <asp:Literal ID="litRelatedMeeting" runat="server" Text="N/A"></asp:Literal>
                                    </p>
                                </div>
                                <div>
                                    <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-2">Related Event</span>
                                    <p class="font-body-md text-body-md flex items-center gap-2">
                                        <span class="material-symbols-outlined text-[18px] text-primary">event</span>
                                        <asp:Literal ID="litRelatedEvent" runat="server" Text="N/A"></asp:Literal>
                                    </p>
                                </div>
                                <div>
                                    <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-2">Due Date</span>
                                    <p class="font-body-md text-body-md flex items-center gap-2">
                                        <span class="material-symbols-outlined text-[18px] text-primary">schedule</span>
                                        <asp:Literal ID="litDueDate" runat="server" Text="Not set"></asp:Literal>
                                    </p>
                                </div>
                                <div>
                                    <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-2">Created At</span>
                                    <p class="font-body-md text-body-md flex items-center gap-2">
                                        <span class="material-symbols-outlined text-[18px] text-primary">add_circle</span>
                                        <asp:Literal ID="litCreatedAt" runat="server"></asp:Literal>
                                    </p>
                                </div>
                            </div>
                        </div>
                    </section>

                    <!-- Status History Timeline -->
                    <section class="standard-card rounded-xl overflow-hidden">
                        <div class="px-6 py-4 border-b border-surface-container-high flex justify-between items-center">
                            <h3 class="font-title-lg text-title-lg text-primary">Status History</h3>
                            <span class="bg-surface-container-high px-2 py-1 rounded font-badge-cap text-badge-cap text-on-surface-variant">
                                <asp:Literal ID="litHistoryCount" runat="server" Text="0"></asp:Literal> updates
                            </span>
                        </div>
                        <div class="p-6">
                            <div class="timeline-vertical">
                                <asp:Repeater ID="rptHistory" runat="server">
                                    <ItemTemplate>
                                        <div class="timeline-step">
                                            <div class="timeline-dot timeline-dot-completed">
                                                <span class="material-symbols-outlined text-[14px]">check</span>
                                            </div>
                                            <div class="flex items-start justify-between">
                                                <div>
                                                    <p class="font-label-md text-label-md font-bold text-on-surface">
                                                        <%# Eval("OldStatusDisplay") %>
                                                        <%# Eval("OldStatus") != DBNull.Value && Eval("OldStatus") != null ? " → " : "" %>
                                                        <span class="text-primary"><%# Eval("NewStatus") %></span>
                                                    </p>
                                                    <%# Eval("Comments") != DBNull.Value && Eval("Comments") != null && Eval("Comments").ToString().Length > 0 ? "<p class='text-sm text-on-surface-variant mt-1 italic'>\"" + Server.HtmlEncode(Eval("Comments").ToString()) + "\"</p>" : "" %>
                                                </div>
                                                <div class="text-right shrink-0 ml-4">
                                                    <p class="text-xs text-on-surface-variant"><%# Eval("ChangedByName") %></p>
                                                    <p class="text-xs text-outline"><%# Eval("ChangedAt", "{0:MMM dd, yyyy - hh:mm tt}") %></p>
                                                </div>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <asp:Panel ID="pnlNoHistory" runat="server" Visible="false">
                                    <div class="text-center py-8">
                                        <span class="material-symbols-outlined text-[40px] text-outline-variant block mb-2">history</span>
                                        <p class="font-body-md text-body-md text-on-surface-variant">No status history recorded yet.</p>
                                    </div>
                                </asp:Panel>
                            </div>
                        </div>
                    </section>
                </div>

                <!-- Right Column: Related Information -->
                <div class="lg:col-span-1 space-y-6">

                    <!-- Audit Trail Summary -->
                    <section class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-4">Audit Trail</h3>
                        <div class="space-y-3">
                            <div class="flex items-center gap-3">
                                <span class="material-symbols-outlined text-[18px] text-on-surface-variant">person</span>
                                <div>
                                    <p class="font-label-md text-label-md">Created by</p>
                                    <p class="text-sm text-on-surface-variant"><asp:Literal ID="litCreatedBy" runat="server"></asp:Literal></p>
                                </div>
                            </div>
                            <div class="flex items-center gap-3">
                                <span class="material-symbols-outlined text-[18px] text-on-surface-variant">update</span>
                                <div>
                                    <p class="font-label-md text-label-md">Last Updated</p>
                                    <p class="text-sm text-on-surface-variant"><asp:Literal ID="litUpdatedAt" runat="server"></asp:Literal></p>
                                </div>
                            </div>
                            <div class="flex items-center gap-3">
                                <span class="material-symbols-outlined text-[18px] text-on-surface-variant">person</span>
                                <div>
                                    <p class="font-label-md text-label-md">Responsible Person</p>
                                    <p class="text-sm text-on-surface-variant"><asp:Literal ID="litResponsibleDetail" runat="server"></asp:Literal></p>
                                </div>
                            </div>
                        </div>
                    </section>
                </div>
            </div>
        </main>
    </div>

</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="<%= ResolveUrl("~/Assets/js/dashboard.js") %>"></script>
</asp:Content>
