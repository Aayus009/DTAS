<%@ Page Title="Events | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Events.aspx.cs" Inherits="DigitalTransparencySystem.Events" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Events | DTAS
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="/Assets/css/events.css" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    
    <!-- Hero Section -->
    <section class="hero-gradient py-section-gap overflow-hidden relative rounded-b-[48px]">
        <div class="max-w-7xl mx-auto px-container-padding-mobile md:px-container-padding-desktop relative z-10">
            <div class="max-w-3xl">
                <div class="inline-flex items-center gap-2 px-4 py-1.5 bg-primary-container text-on-primary-container rounded-full mb-6 status-pulse">
                    <span class="material-symbols-outlined text-[14px]">calendar_today</span>
                    <span class="font-badge-cap text-badge-cap uppercase">Community Calendar</span>
                </div>
                <h1 class="font-display-lg text-display-lg text-primary mb-6 leading-tight">Educational Community Events & Assemblies</h1>
                <p class="font-body-lg text-body-lg text-secondary max-w-2xl leading-relaxed">
                    Monitor public discourse in real-time. Access live board meetings, PTA assemblies, and student council sessions. Your window into institutional decision-making.
                </p>
            </div>
        </div>
    </section>

    <!-- Stats Grid -->
    <section class="mt-[-40px] relative z-20 max-w-7xl mx-auto px-container-padding-mobile md:px-container-padding-desktop">
        <div class="grid grid-cols-2 md:grid-cols-4 gap-gutter">
            <div class="glass-card p-6 rounded-3xl flex flex-col items-center text-center hover:shadow-lg transition-shadow">
                <span class="text-primary font-headline-lg text-headline-lg"><asp:Literal ID="litUpcoming" runat="server" Text="0"></asp:Literal></span>
                <span class="text-secondary font-label-md text-label-md">Upcoming</span>
            </div>
            <div class="glass-card p-6 rounded-3xl flex flex-col items-center text-center hover:shadow-lg transition-shadow">
                <span class="text-primary font-headline-lg text-headline-lg"><asp:Literal ID="litPast" runat="server" Text="0"></asp:Literal></span>
                <span class="text-secondary font-label-md text-label-md">Past Events</span>
            </div>
            <div class="glass-card p-6 rounded-3xl flex flex-col items-center text-center hover:shadow-lg transition-shadow">
                <span class="text-primary font-headline-lg text-headline-lg"><asp:Literal ID="litParticipants" runat="server" Text="0"></asp:Literal></span>
                <span class="text-secondary font-label-md text-label-md">Participants</span>
            </div>
            <div class="glass-card p-6 rounded-3xl flex flex-col items-center text-center hover:shadow-lg transition-shadow">
                <span class="text-primary font-headline-lg text-headline-lg"><asp:Literal ID="litSessions" runat="server" Text="0"></asp:Literal></span>
                <span class="text-secondary font-label-md text-label-md">Total Sessions</span>
            </div>
        </div>
    </section>

    <!-- Main Content Area -->
    <section class="max-w-7xl mx-auto px-container-padding-mobile md:px-container-padding-desktop py-section-gap grid grid-cols-1 lg:grid-cols-12 gap-gutter">
        <!-- Filter and Event List -->
        <div class="lg:col-span-8">
            <!-- Filter Tabs -->
            <div class="flex flex-wrap gap-2 mb-8 overflow-x-auto pb-2 scrollbar-hide">
                <button class="filter-btn active bg-primary text-on-primary px-5 py-2 rounded-full font-label-md text-label-md transition-all" data-filter="all">All</button>
                <button class="filter-btn bg-surface-container text-secondary hover:bg-surface-container-high px-5 py-2 rounded-full transition-colors font-label-md text-label-md" data-filter="upcoming">Upcoming</button>
                <button class="filter-btn bg-surface-container text-secondary hover:bg-surface-container-high px-5 py-2 rounded-full transition-colors font-label-md text-label-md" data-filter="past">Past</button>
                <button class="filter-btn bg-surface-container text-secondary hover:bg-surface-container-high px-5 py-2 rounded-full transition-colors font-label-md text-label-md" data-filter="board">Board Meeting</button>
                <button class="filter-btn bg-surface-container text-secondary hover:bg-surface-container-high px-5 py-2 rounded-full transition-colors font-label-md text-label-md" data-filter="pta">PTA</button>
                <button class="filter-btn bg-surface-container text-secondary hover:bg-surface-container-high px-5 py-2 rounded-full transition-colors font-label-md text-label-md" data-filter="student">Student Council</button>
                <button class="filter-btn bg-surface-container text-secondary hover:bg-surface-container-high px-5 py-2 rounded-full transition-colors font-label-md text-label-md" data-filter="forum">Public Forum</button>
            </div>
            
            <!-- Event Cards Stack -->
            <div class="space-y-6">
                <asp:Repeater ID="rptEvents" runat="server">
                    <ItemTemplate>
                        <div class="event-card flex gap-6 p-6 bg-surface border border-outline-variant/30 rounded-3xl hover:border-primary/20 hover:shadow-md transition-all group" data-category='<%# Eval("Category") %>'>
                            <div class="flex-shrink-0 w-24 h-28 bg-surface-container-high text-primary rounded-2xl flex flex-col items-center justify-center">
                                <span class="font-headline-md text-headline-md"><%# Eval("Day") %></span>
                                <span class="font-badge-cap text-badge-cap uppercase"><%# Eval("Month") %></span>
                            </div>
                            <div class="flex-grow">
                                <div class="flex items-center gap-3 mb-2">
                                    <span class="bg-secondary-container text-on-secondary-container px-2 py-0.5 rounded-lg text-[10px] font-bold uppercase tracking-wider"><%# Eval("EventType") %></span>
                                    <%# Eval("LiveBadge") %>
                                </div>
                                <h3 class="font-headline-md text-headline-md text-primary mb-2 group-hover:text-surface-tint transition-colors"><%# Eval("EventName") %></h3>
                                <div class="flex flex-wrap gap-4 text-secondary font-label-md text-label-md">
                                    <span class="flex items-center gap-1"><span class="material-symbols-outlined text-[18px]">schedule</span> <%# Eval("TimeRange") %></span>
                                    <span class="flex items-center gap-1"><span class="material-symbols-outlined text-[18px]">location_on</span> <%# Eval("Venue") %></span>
                                </div>
                                <%# Eval("ProgressHtml") %>
                                <div class="mt-4">
                                    <a class="text-secondary hover:text-primary transition-colors font-bold flex items-center gap-1" href="EventDetails.aspx?id=<%# Eval("EventID") %>">
                                        View Details <span class="material-symbols-outlined text-[18px]">open_in_new</span>
                                    </a>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                    <EmptyDataTemplate>
                        <div class="bg-surface border border-outline-variant/30 rounded-3xl p-10 text-center">
                            <span class="material-symbols-outlined text-outline text-[40px] block mb-3">event_busy</span>
                            <p class="text-secondary font-body-md">No public events have been published yet.</p>
                            <a class="mt-4 inline-block text-primary font-bold hover:underline" href="/Modules/Participation/Feedback.aspx">Suggest an event</a>
                        </div>
                    </EmptyDataTemplate>
                </asp:Repeater>
            </div>
        </div>
        
        <!-- Sidebar -->
        <aside class="lg:col-span-4 space-y-gutter">
            <!-- Mini Calendar Widget -->
            <div class="bg-surface border border-outline-variant/30 rounded-3xl p-6 shadow-sm">
                <div class="flex justify-between items-center mb-6">
                    <h4 class="font-title-lg text-title-lg text-primary">Calendar</h4>
                    <div class="flex gap-2">
                        <button class="p-1.5 hover:bg-surface-container rounded-2xl transition-colors"><span class="material-symbols-outlined">chevron_left</span></button>
                        <button class="p-1.5 hover:bg-surface-container rounded-2xl transition-colors"><span class="material-symbols-outlined">chevron_right</span></button>
                    </div>
                </div>
                <div class="grid grid-cols-7 text-center gap-y-3">
                    <span class="text-[10px] font-bold text-outline uppercase">Mo</span>
                    <span class="text-[10px] font-bold text-outline uppercase">Tu</span>
                    <span class="text-[10px] font-bold text-outline uppercase">We</span>
                    <span class="text-[10px] font-bold text-outline uppercase">Th</span>
                    <span class="text-[10px] font-bold text-outline uppercase">Fr</span>
                    <span class="text-[10px] font-bold text-outline uppercase">Sa</span>
                    <span class="text-[10px] font-bold text-outline uppercase">Su</span>
                    <div class="py-2 text-label-md opacity-30">27</div>
                    <div class="py-2 text-label-md opacity-30">28</div>
                    <div class="py-2 text-label-md opacity-30">29</div>
                    <div class="py-2 text-label-md opacity-30">30</div>
                    <div class="py-2 text-label-md">1</div>
                    <div class="py-2 text-label-md">2</div>
                    <div class="py-2 text-label-md">3</div>
                    <div class="py-2 text-label-md">4</div>
                    <div class="py-2 text-label-md">5</div>
                    <div class="py-2 text-label-md">6</div>
                    <div class="py-2 text-label-md">7</div>
                    <div class="py-2 text-label-md">8</div>
                    <div class="py-2 text-label-md">9</div>
                    <div class="py-2 text-label-md">10</div>
                    <div class="py-2 text-label-md">11</div>
                    <div class="py-2 text-label-md">12</div>
                    <div class="py-2 text-label-md">13</div>
                    <div class="py-2 text-label-md">14</div>
                    <div class="py-2 text-label-md bg-primary-container text-on-primary-container rounded-full font-bold">15</div>
                    <div class="py-2 text-label-md">16</div>
                    <div class="py-2 text-label-md">17</div>
                    <div class="py-2 text-label-md bg-primary/5 text-primary rounded-full font-bold">18</div>
                    <div class="py-2 text-label-md">19</div>
                    <div class="py-2 text-label-md bg-tertiary-fixed text-on-tertiary-fixed-variant rounded-full font-bold">20</div>
                    <div class="py-2 text-label-md">21</div>
                    <div class="py-2 text-label-md">22</div>
                    <div class="py-2 text-label-md">23</div>
                    <div class="py-2 text-label-md">24</div>
                </div>
                <div class="mt-8">
                    <label class="block font-badge-cap text-badge-cap text-outline uppercase mb-2">Jump to Date</label>
                    <div class="relative">
                        <input class="w-full bg-surface-container-low border border-outline-variant rounded-2xl py-3 px-4 focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all" type="date" />
                    </div>
                </div>
            </div>
            
            <!-- My Registered Events Widget -->
            <div class="bg-surface-container-low border border-outline-variant/30 rounded-3xl p-6 shadow-sm">
                <h4 class="font-title-lg text-title-lg text-primary mb-4">My Registered Events</h4>
                <div class="space-y-4">
                    <div class="flex items-start gap-3 p-3 bg-surface rounded-2xl border border-outline-variant/20">
                        <span class="material-symbols-outlined text-primary mt-1">video_camera_back</span>
                        <div>
                            <p class="font-label-md text-label-md text-primary leading-tight mb-1">School Board Budget Review</p>
                            <p class="text-[12px] text-secondary">Dec 15 &bull; Virtual Join</p>
                        </div>
                    </div>
                    <div class="flex items-start gap-3 p-3 bg-surface rounded-2xl border border-outline-variant/20">
                        <span class="material-symbols-outlined text-primary mt-1">groups</span>
                        <div>
                            <p class="font-label-md text-label-md text-primary leading-tight mb-1">PTA Feedback Session</p>
                            <p class="text-[12px] text-secondary">Dec 18 &bull; In Person</p>
                        </div>
                    </div>
                </div>
                <a href="/Modules/UserMeetings/UserMeetings.aspx" class="block w-full mt-6 py-3 border-2 border-primary text-primary font-bold rounded-2xl hover:bg-primary hover:text-on-primary transition-all text-center">
                    View My Agenda
                </a>
            </div>
            
            <!-- CTA Banner -->
            <div class="relative rounded-3xl overflow-hidden group">
                <div class="absolute inset-0 bg-primary/80 group-hover:bg-primary/90 transition-all z-10"></div>
                <div class="absolute inset-0 z-0 bg-primary-container"></div>
                <div class="relative z-20 p-8 text-on-primary">
                    <h5 class="font-headline-md text-headline-md mb-4">Propose an Event</h5>
                    <p class="font-body-md text-body-md opacity-80 mb-6 leading-relaxed">Want to organize a community forum or public dialogue?</p>
                    <a href="/Modules/Participation/Feedback.aspx" class="inline-block bg-surface text-primary px-6 py-2.5 rounded-2xl font-bold hover:scale-105 transition-transform shadow-lg">
                        Submit Request
                    </a>
                </div>
            </div>
        </aside>
    </section>

</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="/Assets/js/events.js"></script>
</asp:Content>