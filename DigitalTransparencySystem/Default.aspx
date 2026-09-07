<%@ Page Title="DTAS | Home" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="DigitalTransparencySystem.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    DTAS | Home
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="/Assets/css/default.css?v=6" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
<div class="lp">

    <section class="lp-hero">
        <div class="lp-hero-glow lp-hero-glow-a"></div>
        <div class="lp-hero-glow lp-hero-glow-b"></div>
        <div class="lp-shell lp-hero-grid">
            <div class="lp-hero-copy lp-anim-rise">
                <span class="lp-pill">
                    <span class="lp-dot"></span>
                    Live campus governance
                </span>
                <h1>
                    Transparent decisions.<br />
                    <em>Accountable actions.</em>
                </h1>
                <p>
                    DTAS keeps proposals, meetings, decisions, and follow-through in one record
                    that students, faculty, and staff can actually follow.
                </p>
                <div class="lp-hero-actions">
                    <a href="javascript:void(0)" onclick="openLogin()" class="lp-btn lp-btn-primary">
                        Access dashboard
                        <span class="material-symbols-outlined">arrow_forward</span>
                    </a>
                    <a href="/Transparency.aspx" class="lp-btn lp-btn-ghost" onclick="requireAuth('/Transparency.aspx'); return false;">
                        <span class="material-symbols-outlined">visibility</span>
                        Public portal
                    </a>
                </div>
                <ul class="lp-trust">
                    <li><span class="material-symbols-outlined">verified_user</span> Role-based access</li>
                    <li><span class="material-symbols-outlined">lock</span> Append-only audit</li>
                    <li><span class="material-symbols-outlined">public</span> Open transparency</li>
                </ul>
            </div>

            <div class="lp-hero-visual lp-anim-rise lp-anim-delay">
                <div class="lp-snapshot">
                    <div class="lp-snapshot-bar">
                        <span>Governance snapshot</span>
                        <span class="lp-live"><i></i> Live</span>
                    </div>
                    <div class="lp-snapshot-card">
                        <div class="lp-snapshot-head">
                            <strong>Decision lifecycle</strong>
                            <span class="material-symbols-outlined">sync</span>
                        </div>
                        <div class="lp-bars">
                            <div class="lp-bar"><span>Propose</span><b>100%</b><span class="lp-track"><i style="width:100%"></i></span></div>
                            <div class="lp-bar"><span>Deliberate</span><b>82%</b><span class="lp-track"><i style="width:82%"></i></span></div>
                            <div class="lp-bar"><span>Approve</span><b>64%</b><span class="lp-track"><i class="is-navy" style="width:64%"></i></span></div>
                            <div class="lp-bar"><span>Implement</span><b>45%</b><span class="lp-track"><i class="is-navy" style="width:45%"></i></span></div>
                            <div class="lp-bar"><span>Report</span><b>38%</b><span class="lp-track"><i class="is-navy" style="width:38%"></i></span></div>
                        </div>
                    </div>
                    <div class="lp-snapshot-status">
                        <div>
                            <small>System status</small>
                            <strong><i></i> Fully operational</strong>
                        </div>
                        <span class="material-symbols-outlined">data_check</span>
                    </div>
                </div>
                <div class="lp-float lp-float-a">
                    <span class="material-symbols-outlined">fact_check</span>
                    <div>
                        <small>Decisions</small>
                        <strong><asp:Literal ID="litDecisions" runat="server" Text="0"></asp:Literal></strong>
                    </div>
                </div>
                <div class="lp-float lp-float-b">
                    <span class="material-symbols-outlined">task_alt</span>
                    <div>
                        <small>Completed</small>
                        <strong><asp:Literal ID="litCompleted" runat="server" Text="0"></asp:Literal></strong>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <section class="lp-metrics">
        <div class="lp-shell lp-ticker lp-reveal">
            <div>
                <small>Active people</small>
                <strong class="metric-stat"><asp:Literal ID="litParticipants" runat="server" Text="0"></asp:Literal></strong>
            </div>
            <div>
                <small>Task completion</small>
                <strong class="metric-stat"><asp:Literal ID="litAccountability" runat="server" Text="0"></asp:Literal>%</strong>
            </div>
            <div>
                <small>Campus reach</small>
                <strong class="metric-stat">100+</strong>
            </div>
            <div>
                <small>Trust score</small>
                <strong class="metric-stat">92%</strong>
            </div>
        </div>
    </section>

    <section class="lp-roles">
        <div class="lp-shell">
            <div class="lp-section-head lp-reveal">
                <span class="lp-kicker">Built for campus work</span>
                <h2>One record. Three ways in.</h2>
                <p>Everyone sees what they need. The trail stays the same.</p>
            </div>
            <div class="lp-role-grid">
                <article class="lp-role is-student">
                    <span class="lp-role-ico material-symbols-outlined">school</span>
                    <h3>Students</h3>
                    <p>Propose events, form groups, take on tasks, and keep work visible to the people who assigned it.</p>
                </article>
                <article class="lp-role is-faculty">
                    <span class="lp-role-ico material-symbols-outlined">menu_book</span>
                    <h3>Faculty</h3>
                    <p>Create assignments, follow progress, and keep decisions tied to the meetings that produced them.</p>
                </article>
                <article class="lp-role is-staff">
                    <span class="lp-role-ico material-symbols-outlined">badge</span>
                    <h3>Staff</h3>
                    <p>Run events, publish outcomes, and leave an audit path that does not depend on memory or chat history.</p>
                </article>
            </div>
        </div>
    </section>

    <section class="lp-cycle">
        <div class="lp-shell">
            <div class="lp-section-head lp-reveal">
                <span class="lp-kicker">How it works</span>
                <h2>The accountability cycle</h2>
                <p>Every decision follows the same five steps, from proposal to a signed-in report.</p>
            </div>
            <div class="lp-steps">
                <div class="lp-steps-line" aria-hidden="true"></div>

                <article class="step-card-sm lp-step">
                    <span class="lp-step-num">1</span>
                    <h3>Propose</h3>
                    <p>Submit timestamped proposals linked to events and meetings.</p>
                    <div class="lp-step-tag"><span class="material-symbols-outlined">event</span> Event-linked</div>
                </article>
                <article class="step-card-sm lp-step">
                    <span class="lp-step-num">2</span>
                    <h3>Deliberate</h3>
                    <p>Record minutes, attendance, and the discussion that shaped the call.</p>
                    <div class="lp-step-tag"><span class="material-symbols-outlined">groups</span> Attendance tracked</div>
                </article>
                <article class="step-card-sm lp-step">
                    <span class="lp-step-num">3</span>
                    <h3>Approve</h3>
                    <p>Move through a formal pipeline with a history that cannot be quietly rewritten.</p>
                    <div class="lp-step-tag"><span class="material-symbols-outlined">shield</span> Full history</div>
                </article>
                <article class="step-card-sm lp-step">
                    <span class="lp-step-num">4</span>
                    <h3>Implement</h3>
                    <p>Assign work, attach evidence, and track progress as it happens.</p>
                    <div class="lp-step-tag"><span class="material-symbols-outlined">backup</span> Proof attached</div>
                </article>
                <article class="step-card-sm lp-step is-last">
                    <span class="lp-step-num">5</span>
                    <h3>Report</h3>
                    <p>Publish approved decisions and the follow-through. Login is required to report.</p>
                    <div class="lp-step-tag"><span class="material-symbols-outlined">lock</span> Login required</div>
                </article>
            </div>
        </div>
    </section>

    <section class="lp-modules">
        <div class="lp-shell">
            <div class="lp-section-head lp-reveal">
                <span class="lp-kicker">The toolkit</span>
                <h2>Core modules</h2>
                <p>The pieces that keep campus work connected instead of scattered.</p>
            </div>
            <div class="lp-module-grid">
                <article class="lp-module">
                    <span class="material-symbols-outlined">login</span>
                    <h3>Sign in</h3>
                    <p>Institutional email access with one-time verification.</p>
                </article>
                <article class="lp-module">
                    <span class="material-symbols-outlined">dashboard</span>
                    <h3>Dashboard</h3>
                    <p>A clear home for work, alerts, and campus activity.</p>
                </article>
                <article class="lp-module">
                    <span class="material-symbols-outlined">groups</span>
                    <h3>Meetings</h3>
                    <p>Minutes and attendance kept with the decision they inform.</p>
                </article>
                <article class="lp-module">
                    <span class="material-symbols-outlined">fact_check</span>
                    <h3>Decisions</h3>
                    <p>The record of what was chosen, when, and by whom.</p>
                </article>
                <article class="lp-module">
                    <span class="material-symbols-outlined">task_alt</span>
                    <h3>Tasks</h3>
                    <p>Action items that stay linked to the decision they serve.</p>
                </article>
                <article class="lp-module">
                    <span class="material-symbols-outlined">forum</span>
                    <h3>Connect</h3>
                    <p>Group conversation that stays inside the same workspace.</p>
                </article>
                <article class="lp-module">
                    <span class="material-symbols-outlined">campaign</span>
                    <h3>Events</h3>
                    <p>Campus events with proposals, owners, and outcomes.</p>
                </article>
                <article class="lp-module">
                    <span class="material-symbols-outlined">verified_user</span>
                    <h3>Audit</h3>
                    <p>Immutable logs of the actions that matter.</p>
                </article>
            </div>
        </div>
    </section>

    <section class="lp-cta-wrap">
        <div class="lp-shell">
            <div class="lp-cta lp-reveal">
                <h2>Ready to see the record, not the rumor?</h2>
                <p>Join the people already using DTAS to keep campus decisions visible and follow-through honest.</p>
                <div class="lp-hero-actions">
                    <a href="javascript:void(0)" onclick="openRegister()" class="lp-btn lp-btn-light">Create an account</a>
                    <a href="/FAQ.aspx" class="lp-btn lp-btn-outline">Read the FAQ</a>
                </div>
            </div>
        </div>
    </section>

</div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="/Assets/js/default.js?v=3"></script>
</asp:Content>
