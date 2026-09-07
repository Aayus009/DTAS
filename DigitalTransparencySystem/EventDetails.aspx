<%@ Page Title="Event Details | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EventDetails.aspx.cs" Inherits="DigitalTransparencySystem.EventDetails" %>

<asp:Content ID="TitleContent1" ContentPlaceHolderID="TitleContent" runat="server">
    Event Details | DTAS
</asp:Content>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="/Assets/css/events.css" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Event Found -->
    <asp:Panel ID="pnlEvent" runat="server">
        <!-- Hero Section -->
        <section class="hero-gradient py-section-gap overflow-hidden relative rounded-b-[48px]">
            <div class="max-w-7xl mx-auto px-container-padding-mobile md:px-container-padding-desktop relative z-10">
                <a href="/Default.aspx" class="inline-flex items-center gap-1.5 font-label-md text-label-md text-secondary hover:text-primary transition-colors mb-6">
                    <span class="material-symbols-outlined text-[18px]">arrow_back</span>
                    Back to Home
                </a>
                <div class="flex flex-col md:flex-row md:items-center gap-6">
                    <div class="flex-shrink-0 w-28 h-32 bg-surface-container-high text-primary rounded-2xl flex flex-col items-center justify-center shadow-lg">
                        <span class="font-display-md text-display-md leading-none"><asp:Literal ID="litDay" runat="server"></asp:Literal></span>
                        <span class="font-badge-cap text-badge-cap uppercase mt-1"><asp:Literal ID="litMonth" runat="server"></asp:Literal></span>
                    </div>
                    <div>
                        <div class="flex items-center gap-3 mb-3">
                            <span class="bg-secondary-container text-on-secondary-container px-3 py-1 rounded-lg text-[10px] font-bold uppercase tracking-wider"><asp:Literal ID="litEventType" runat="server"></asp:Literal></span>
                            <asp:Literal ID="litStatusBadge" runat="server"></asp:Literal>
                        </div>
                        <h1 class="font-display-lg text-display-lg text-primary mb-4 leading-tight"><asp:Literal ID="litEventName" runat="server"></asp:Literal></h1>
                        <div class="flex flex-wrap gap-4 text-secondary font-label-md text-label-md">
                            <span class="flex items-center gap-1.5"><span class="material-symbols-outlined text-[18px]">schedule</span> <asp:Literal ID="litDateRange" runat="server"></asp:Literal></span>
                            <span class="flex items-center gap-1.5"><span class="material-symbols-outlined text-[18px]">location_on</span> <asp:Literal ID="litVenue" runat="server"></asp:Literal></span>
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <!-- Details Grid -->
        <section class="max-w-7xl mx-auto px-container-padding-mobile md:px-container-padding-desktop py-section-gap grid grid-cols-1 lg:grid-cols-12 gap-gutter">

            <!-- Main Description -->
            <div class="lg:col-span-8 space-y-gutter">
                <div class="bg-surface border border-outline-variant/30 rounded-3xl p-8 shadow-sm">
                    <h2 class="font-headline-md text-headline-md text-primary mb-4">About This Session</h2>
                    <p class="font-body-lg text-body-lg text-secondary leading-relaxed"><asp:Literal ID="litDescription" runat="server"></asp:Literal></p>
                </div>

                <!-- Related Meetings -->
                <div class="bg-surface border border-outline-variant/30 rounded-3xl p-8 shadow-sm">
                    <h2 class="font-headline-md text-headline-md text-primary mb-4">Linked Meetings & Records</h2>
                    <div class="space-y-4">
                        <asp:Repeater ID="rptRelatedMeetings" runat="server">
                            <ItemTemplate>
                                <div class="flex items-start gap-4 p-4 bg-surface-container-low rounded-2xl border border-outline-variant/20">
                                    <span class="material-symbols-outlined text-primary mt-0.5">groups</span>
                                    <div class="min-w-0">
                                        <p class="font-label-md text-label-md text-primary font-bold leading-tight mb-1"><%# Eval("MeetingTitle") %></p>
                                        <p class="text-[12px] text-secondary"><%# Eval("MeetingType") %> &bull; <%# Eval("ScheduledDate", "{0:MMM dd, yyyy hh:mm tt}") %></p>
                                        <%# Eval("MinutesStatus") %>
                                    </div>
                                </div>
                            </ItemTemplate>
                            <EmptyDataTemplate>
                                <p class="text-secondary font-body-md">No meetings linked to this event yet.</p>
                            </EmptyDataTemplate>
                        </asp:Repeater>
                    </div>
                </div>
            </div>

            <!-- Sidebar Details -->
            <aside class="lg:col-span-4 space-y-gutter">
                <div class="glass-card p-6 rounded-3xl">
                    <h4 class="font-title-lg text-title-lg text-primary mb-4">Event Details</h4>
                    <dl class="space-y-4 font-label-md text-label-md">
                        <div class="flex justify-between gap-4">
                            <dt class="text-outline uppercase tracking-wider flex items-center gap-2"><span class="material-symbols-outlined text-[18px]">flag</span> Status</dt>
                            <dd class="font-medium text-on-surface text-right"><asp:Literal ID="litStatus" runat="server"></asp:Literal></dd>
                        </div>
                        <div class="flex justify-between gap-4">
                            <dt class="text-outline uppercase tracking-wider flex items-center gap-2"><span class="material-symbols-outlined text-[18px]">person</span> Organizer</dt>
                            <dd class="font-medium text-on-surface text-right"><asp:Literal ID="litOrganizer" runat="server"></asp:Literal></dd>
                        </div>
                        <div class="flex justify-between gap-4">
                            <dt class="text-outline uppercase tracking-wider flex items-center gap-2"><span class="material-symbols-outlined text-[18px]">payments</span> Budget</dt>
                            <dd class="font-medium text-on-surface text-right"><asp:Literal ID="litBudget" runat="server"></asp:Literal></dd>
                        </div>
                    </dl>
                </div>

                <!-- CTA -->
                <div class="relative rounded-3xl overflow-hidden group">
                    <div class="absolute inset-0 bg-primary/80 group-hover:bg-primary/90 transition-all z-10"></div>
                    <div class="absolute inset-0 z-0 bg-primary-container"></div>
                    <div class="relative z-20 p-8 text-on-primary">
                        <h5 class="font-headline-md text-headline-md mb-4">Want to Contribute?</h5>
                        <p class="font-body-md text-body-md opacity-80 mb-6 leading-relaxed">Have questions about this session? Share your feedback with the community.</p>
                        <a href="/Modules/Participation/Feedback.aspx" class="inline-block bg-surface text-primary px-6 py-2.5 rounded-2xl font-bold hover:scale-105 transition-transform shadow-lg">
                            Submit Feedback
                        </a>
                    </div>
                </div>
            </aside>
        </section>
    </asp:Panel>

    <!-- Not Found -->
    <asp:Panel ID="pnlNotFound" runat="server" Visible="false">
        <section class="hero-gradient py-section-gap rounded-b-[48px]">
            <div class="max-w-7xl mx-auto px-container-padding-mobile md:px-container-padding-desktop">
                <a href="/Default.aspx" class="inline-flex items-center gap-1.5 font-label-md text-label-md text-secondary hover:text-primary transition-colors mb-8">
                    <span class="material-symbols-outlined text-[18px]">arrow_back</span>
                    Back to Home
                </a>
            </div>
        </section>
        <section class="max-w-7xl mx-auto px-container-padding-mobile md:px-container-padding-desktop py-section-gap">
            <div class="bg-surface border border-outline-variant/30 rounded-3xl p-12 text-center max-w-2xl mx-auto">
                <span class="material-symbols-outlined text-outline text-[48px] block mb-3">event_busy</span>
                <h2 class="font-headline-lg text-headline-lg text-primary mb-3">Event Not Found</h2>
                <p class="text-secondary font-body-md">This event does not exist, has been removed, or the link is incorrect.</p>
                <a href="/Default.aspx" class="mt-6 inline-block bg-primary text-on-primary px-8 py-3 rounded-full font-bold hover:bg-primary-container transition-colors">Back to Home</a>
            </div>
        </section>
    </asp:Panel>
</asp:Content>