<%@ Page Title="Transparency Portal | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Transparency.aspx.cs" Inherits="DigitalTransparencySystem.Transparency" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Transparency Portal | DTAS
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>?v=navfix2" />
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/transparency.css") %>?v=2" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">

    <uc:AdminTopbar ID="adminTop" runat="server" Visible="false" />
    <uc:UserTopbar ID="userTop" runat="server" />

    <div class="flex min-h-screen">
        <uc:AdminSidebar ID="adminSide" ActivePage="Transparency" runat="server" Visible="false" />
        <uc:UserSidebar ID="userSide" ActivePage="Transparency" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            <div class="tx-portal">
                <asp:Panel ID="pnlAdminPreview" runat="server" Visible="false" CssClass="tx-banner">
                    <div>
                        <p class="tx-banner-title">Community portal</p>
                        <p class="tx-banner-copy">This is the same gist faculty, staff, and students see. Live counts stay on the admin dashboard.</p>
                    </div>
                    <a class="tx-link" href="<%= ResolveUrl("~/Modules/Transparency/Transparency.aspx") %>">Back to live dashboard</a>
                </asp:Panel>

                <header class="tx-hero">
                    <div>
                        <p class="tx-kicker">Public gist</p>
                        <h1 class="tx-title">Transparency</h1>
                        <p class="tx-sub">What was proposed, what was approved, who is responsible, and how far the work has moved. Private workspace notes stay off this page.</p>
                    </div>
                    <a class="tx-link" href="<%= ResolveUrl("~/Modules/UserFeedback/UserFeedback.aspx") %>">Send feedback</a>
                </header>

                <section class="tx-stats">
                    <article class="tx-card tx-stat">
                        <span class="tx-kicker">Decisions</span>
                        <span class="tx-num"><asp:Literal ID="litTotalDecisions" runat="server" Text="0"></asp:Literal></span>
                        <p class="tx-hint">Recorded and still in public view</p>
                    </article>
                    <article class="tx-card tx-stat">
                        <span class="tx-kicker">Implemented</span>
                        <span class="tx-num"><asp:Literal ID="litImplemented" runat="server" Text="0"></asp:Literal></span>
                        <p class="tx-hint">Approved decisions that reached delivery</p>
                    </article>
                    <article class="tx-card tx-stat">
                        <span class="tx-kicker">Task follow-through</span>
                        <span class="tx-num"><asp:Literal ID="litAccountability" runat="server" Text="0"></asp:Literal>%</span>
                        <p class="tx-hint">Completed tasks across campus work</p>
                    </article>
                    <article class="tx-card tx-stat">
                        <span class="tx-kicker">Active members</span>
                        <span class="tx-num"><asp:Literal ID="litMembers" runat="server" Text="0"></asp:Literal></span>
                        <p class="tx-hint">People currently on the platform</p>
                    </article>
                </section>

                <section class="tx-split">
                    <article class="tx-card">
                        <div class="tx-card-head">
                            <h2>Ongoing works &amp; events</h2>
                        </div>
                        <asp:Repeater ID="rptWorks" runat="server">
                            <ItemTemplate>
                                <div class="tx-row">
                                    <div class="tx-icon">
                                        <span class="material-symbols-outlined"><%# Eval("TypeIcon") %></span>
                                    </div>
                                    <div class="tx-row-body">
                                        <div class="tx-chips">
                                            <%# Eval("TypeBadge") %>
                                            <%# Eval("StatusBadge") %>
                                        </div>
                                        <p class="tx-row-title"><%# Eval("ItemTitle") %></p>
                                        <p class="tx-row-copy"><%# Eval("ReasonGist") %></p>
                                        <%# Eval("ProgressHtml") %>
                                        <div class="tx-meta">
                                            <span><span class="material-symbols-outlined">person</span> <%# Eval("HandledBy") %></span>
                                            <span><span class="material-symbols-outlined">event</span> Due <%# Eval("DueDate", "{0:MMM dd, yyyy}") %></span>
                                            <span runat="server" visible='<%# !String.IsNullOrEmpty(Eval("RelatedDecision") as string) %>'>
                                                <span class="material-symbols-outlined">fact_check</span> <%# Eval("RelatedDecision") %>
                                            </span>
                                        </div>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        <div id="pnlWorksEmpty" runat="server" class="tx-empty" visible="false">
                            <span class="material-symbols-outlined">task_alt</span>
                            <p>No ongoing works or events right now.</p>
                        </div>
                    </article>

                    <article class="tx-card">
                        <div class="tx-card-head">
                            <h2>Ongoing decisions</h2>
                        </div>
                        <asp:Repeater ID="rptActiveDecisions" runat="server">
                            <ItemTemplate>
                                <div class="tx-row">
                                    <div class="tx-avatar"><%# Eval("ResponsibleInitials") %></div>
                                    <div class="tx-row-body">
                                        <div class="tx-chips">
                                            <%# Eval("StatusBadge") %>
                                            <span class="tx-chip">DEC-<%# Eval("DecisionID") %></span>
                                        </div>
                                        <p class="tx-row-title"><%# Eval("DecisionTitle") %></p>
                                        <p class="tx-row-copy"><%# Eval("ReasonGist") %></p>
                                        <%# Eval("ProgressHtml") %>
                                        <div class="tx-meta">
                                            <span><span class="material-symbols-outlined">person</span> <%# Eval("ResponsibleName") %></span>
                                            <span><span class="material-symbols-outlined">groups</span> <%# Eval("ContextName") %></span>
                                            <span><span class="material-symbols-outlined">event</span> Due <%# Eval("DueDate", "{0:MMM dd, yyyy}") %></span>
                                        </div>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        <div id="pnlActiveDecisionsEmpty" runat="server" class="tx-empty" visible="false">
                            <span class="material-symbols-outlined">fact_check</span>
                            <p>No ongoing decisions right now.</p>
                        </div>
                    </article>
                </section>

                <section class="tx-block">
                    <div class="tx-block-head">
                        <div>
                            <h2>Decision registry</h2>
                            <p>Track the public lifecycle of institutional changes.</p>
                        </div>
                    </div>
                    <div class="tx-filters" id="txFilters">
                        <button type="button" class="tx-filter is-on" data-filter="all">All</button>
                        <button type="button" class="tx-filter" data-filter="proposed">Proposed</button>
                        <button type="button" class="tx-filter" data-filter="deliberating">In review</button>
                        <button type="button" class="tx-filter" data-filter="approved">Approved</button>
                        <button type="button" class="tx-filter" data-filter="implemented">Implemented</button>
                        <button type="button" class="tx-filter" data-filter="completed">Completed</button>
                        <button type="button" class="tx-filter" data-filter="rejected">Rejected</button>
                    </div>
                    <div class="tx-cards">
                        <asp:Repeater ID="rptDecisions" runat="server">
                            <ItemTemplate>
                                <article class="tx-card tx-decision" data-status='<%# Eval("FilterKey") %>'>
                                    <div class="tx-chips">
                                        <span class="tx-chip">DEC-<%# Eval("DecisionID") %></span>
                                        <%# Eval("StatusBadge") %>
                                    </div>
                                    <h3><%# Eval("DecisionTitle") %></h3>
                                    <span class="tx-chip"><%# Eval("Priority") %></span>
                                    <%# Eval("ProgressHtml") %>
                                    <div class="tx-decision-foot">
                                        <div class="tx-avatar"><%# Eval("CommitteeInitials") %></div>
                                        <div>
                                            <span class="tx-kicker"><%# Eval("ContextLabel") %></span>
                                            <strong><%# Eval("ResponsibleName") %></strong>
                                        </div>
                                    </div>
                                    <a class="tx-link" href='<%# ResolveUrl("~/Modules/UserDecisions/UserDecisionPreview.aspx?id=" + Eval("DecisionID")) %>'>
                                        View record <span class="material-symbols-outlined">arrow_forward</span>
                                    </a>
                                </article>
                            </ItemTemplate>
                        </asp:Repeater>
                        <div id="pnlDecisionsEmpty" runat="server" class="tx-empty tx-empty-wide" visible="false">
                            <span class="material-symbols-outlined">fact_check</span>
                            <p>No approved decisions published yet.</p>
                        </div>
                    </div>
                </section>

                <section class="tx-block">
                    <div class="tx-block-head">
                        <div>
                            <h2>Community polls</h2>
                            <p>Vote on planned events, projects, and decisions before they are finalized.</p>
                        </div>
                        <a class="tx-link" href="<%= ResolveUrl("~/Modules/UserPolls/UserPolls.aspx") %>">Open polls</a>
                    </div>
                    <div class="tx-cards">
                        <asp:Repeater ID="rptOpenPolls" runat="server">
                            <ItemTemplate>
                                <article class="tx-card tx-poll">
                                    <span class="tx-kicker"><%# Eval("ContextLabel") %></span>
                                    <h3><%# Eval("PollTitle") %></h3>
                                    <p><%# Eval("Description") %></p>
                                    <a class="tx-link" href="<%= ResolveUrl("~/Modules/UserPolls/UserPolls.aspx") %>">Participate</a>
                                </article>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <div id="pnlPollsEmpty" runat="server" class="tx-empty tx-empty-wide" visible="false">
                        <span class="material-symbols-outlined">how_to_vote</span>
                        <p>No open polls right now. When an event or decision needs community input, it will appear here.</p>
                    </div>
                </section>

                <section class="tx-block">
                    <div class="tx-block-head">
                        <div>
                            <h2>Accountability tracker</h2>
                            <p>Public progress only: lead, deadline, and status. Workspace notes are not shown.</p>
                        </div>
                    </div>
                    <div class="tx-table-wrap">
                        <table class="tx-table">
                            <thead>
                                <tr>
                                    <th>Work</th>
                                    <th>Lead</th>
                                    <th>Deadline</th>
                                    <th>Status</th>
                                    <th>Days left</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptTracker" runat="server">
                                    <ItemTemplate>
                                        <tr>
                                            <td><%# Eval("ItemName") %></td>
                                            <td><%# Eval("Department") %></td>
                                            <td><%# Eval("DueDate", "{0:MMM dd, yyyy}") %></td>
                                            <td><%# Eval("StatusBadge") %></td>
                                            <td><%# Eval("DaysLabel") %></td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <tr id="trTrackerEmpty" runat="server" style="display:none;">
                                    <td colspan="5">No tracked commitments yet.</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </section>
            </div>
        </main>
    </div>

</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="<%= ResolveUrl("~/Assets/js/transparency.js") %>?v=2"></script>
</asp:Content>
