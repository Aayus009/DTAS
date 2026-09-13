<%@ Page Title="Transparency Dashboard | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Transparency.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Transparency.Transparency" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>?v=navfix2" />
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/transparency-admin.css") %>?v=2" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <uc:AdminTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:AdminSidebar ActivePage="Transparency" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            <div class="tx-page" id="txRoot" data-sync="<%= ResolveUrl("~/Modules/Transparency/TransparencySync.ashx") %>">
                <header class="tx-hero">
                    <div>
                        <h1 class="tx-title">Transparency Dashboard</h1>
                        <p class="tx-sub">Live public gist: decisions, campus events, and published meeting minutes. Private files, feedback, and workspace notes stay off this page.</p>
                    </div>
                    <div class="flex flex-col items-end gap-3">
                        <div class="tx-live">
                            <span class="tx-dot"></span>
                            <div>
                                <span class="tx-live-kicker">Realtime sync</span>
                                <strong id="txSyncedAt"><asp:Literal ID="litLastUpdated" runat="server"></asp:Literal></strong>
                            </div>
                        </div>
                        <a class="tx-link" href="<%= ResolveUrl("~/Transparency.aspx") %>">View community portal</a>
                    </div>
                </header>

                <section class="tx-stats">
                    <article class="tx-card tx-stat">
                        <span class="tx-kicker">Open decisions</span>
                        <span class="tx-num" id="txOpenDecisions"><asp:Literal ID="litOpenDecisions" runat="server" Text="0"></asp:Literal></span>
                        <p class="tx-hint">Still moving through review or approval</p>
                    </article>
                    <article class="tx-card tx-stat">
                        <span class="tx-kicker">Implemented</span>
                        <span class="tx-num" id="txImplemented"><asp:Literal ID="litImplemented" runat="server" Text="0"></asp:Literal></span>
                        <div class="tx-bar"><span id="txImplementedBar" runat="server" ClientIDMode="Static" style="width:0%"></span></div>
                        <p class="tx-hint"><span id="txImplementedPct"><asp:Literal ID="litImplementedPct" runat="server" Text="0"></asp:Literal></span>% of recorded decisions</p>
                    </article>
                    <article class="tx-card tx-stat">
                        <span class="tx-kicker">Public events</span>
                        <span class="tx-num" id="txPublicEvents"><asp:Literal ID="litPublicEvents" runat="server" Text="0"></asp:Literal></span>
                        <div class="tx-bar"><span id="txEventBar" runat="server" ClientIDMode="Static" style="width:0%"></span></div>
                        <p class="tx-hint">Live task completion <span id="txEventPct"><asp:Literal ID="litEventPct" runat="server" Text="0"></asp:Literal></span>%</p>
                    </article>
                    <article class="tx-card tx-stat">
                        <span class="tx-kicker">Minutes published</span>
                        <span class="tx-num" id="txMinutes"><asp:Literal ID="litMinutesPublished" runat="server" Text="0"></asp:Literal></span>
                        <div class="tx-bar"><span id="txMinutesBar" runat="server" ClientIDMode="Static" style="width:0%"></span></div>
                        <p class="tx-hint">of <span id="txMeetings"><asp:Literal ID="litMeetings" runat="server" Text="0"></asp:Literal></span> meetings · <span id="txPolls"><asp:Literal ID="litOpenPolls" runat="server" Text="0"></asp:Literal></span> open polls</p>
                    </article>
                </section>

                <div class="tx-grid">
                    <section class="tx-card">
                        <div class="tx-card-head">
                            <h3>Publication gap</h3>
                        </div>
                        <div class="tx-gap">
                            <div class="tx-gap-row">
                                <div class="tx-gap-top"><span>Meeting minutes</span><strong id="txGapMinutes"><asp:Literal ID="litGapMinutes" runat="server"></asp:Literal></strong></div>
                                <div class="tx-bar"><span id="txGapMinutesBar" runat="server" ClientIDMode="Static" style="width:0%"></span></div>
                            </div>
                            <div class="tx-gap-row">
                                <div class="tx-gap-top"><span>Decisions implemented</span><strong id="txGapDecisions"><asp:Literal ID="litGapDecisions" runat="server"></asp:Literal></strong></div>
                                <div class="tx-bar"><span id="txGapDecisionsBar" runat="server" ClientIDMode="Static" style="width:0%"></span></div>
                            </div>
                            <div class="tx-gap-row">
                                <div class="tx-gap-top"><span>Public event progress</span><strong id="txGapEvents"><asp:Literal ID="litGapEvents" runat="server"></asp:Literal></strong></div>
                                <div class="tx-bar"><span id="txGapEventsBar" runat="server" ClientIDMode="Static" style="width:0%"></span></div>
                            </div>
                        </div>
                    </section>

                    <section class="tx-card">
                        <div class="tx-card-head">
                            <h3>Recent decisions</h3>
                            <a class="tx-link" href="<%= ResolveUrl("~/Modules/Decisions/Decisions.aspx") %>">Open decisions</a>
                        </div>
                        <div id="txDecisionList">
                            <asp:Repeater ID="rptDecisions" runat="server">
                                <ItemTemplate>
                                    <div class="tx-row">
                                        <div>
                                            <p><%# Eval("Title") %></p>
                                            <span>Recorded <%# Eval("Recorded") %> · <%# Eval("Responsible") %></span>
                                        </div>
                                        <div class="tx-chips">
                                            <span class="tx-chip"><%# Eval("Priority") %></span>
                                            <span class="tx-chip"><%# Eval("Status") %></span>
                                            <span class="tx-chip">Due <%# Eval("Due") %></span>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            <asp:Panel ID="pnlDecisionsEmpty" runat="server" Visible="false" CssClass="tx-empty">No decisions recorded yet.</asp:Panel>
                        </div>
                    </section>
                </div>

                <section class="tx-card mb-4">
                    <div class="tx-card-head">
                        <h3>Live public events</h3>
                        <a class="tx-link" href="<%= ResolveUrl("~/Modules/Events/Events.aspx") %>">Events management</a>
                    </div>
                    <div id="txEventList" class="tx-event-scroll">
                        <asp:Repeater ID="rptEvents" runat="server">
                            <ItemTemplate>
                                <div class="tx-event">
                                    <div class="tx-event-top">
                                        <div>
                                            <p class="font-semibold m-0"><%# Eval("Name") %></p>
                                            <span class="tx-chip"><%# Eval("Status") %></span>
                                        </div>
                                        <strong><%# Eval("Percent") %>%</strong>
                                    </div>
                                    <div class="tx-bar"><span style="width:<%# Eval("Percent") %>%"></span></div>
                                    <p class="tx-hint"><%# Eval("Label") %></p>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:Panel ID="pnlEventsEmpty" runat="server" Visible="false" CssClass="tx-empty">No public campus events are live yet.</asp:Panel>
                    </div>
                </section>

                <section class="tx-card">
                    <div class="tx-card-head">
                        <h3>Meeting minutes</h3>
                        <a class="tx-link" href="<%= ResolveUrl("~/Modules/Meetings/Meetings.aspx") %>">Meetings</a>
                    </div>
                    <div id="txMeetingList">
                        <asp:Repeater ID="rptMeetings" runat="server">
                            <ItemTemplate>
                                <div class="tx-row">
                                    <div>
                                        <p><%# Eval("Title") %></p>
                                        <span><%# Eval("When") %></span>
                                    </div>
                                    <span class='<%# Convert.ToBoolean(Eval("HasMinutes")) ? "tx-chip tx-chip-ok" : "tx-chip tx-chip-wait" %>'><%# Eval("MinutesLabel") %></span>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:Panel ID="pnlMeetingsEmpty" runat="server" Visible="false" CssClass="tx-empty">No meetings have been scheduled yet.</asp:Panel>
                    </div>
                </section>
            </div>
        </main>
    </div>
</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="<%= ResolveUrl("~/Assets/js/transparency-admin.js") %>?v=1"></script>
</asp:Content>
