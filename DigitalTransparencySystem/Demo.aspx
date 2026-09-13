<%@ Page Title="Guest Demo | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Demo.aspx.cs" Inherits="DigitalTransparencySystem.Demo" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Guest Demo | DTAS
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="/Assets/css/demo.css?v=3" />
    <meta name="robots" content="noindex" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">

<div class="lp">

    <div class="demo-banner" role="status">
        <span class="demo-banner-tag">Guest demo</span>
        <p>Sample data only. No account, no database writes. Refresh to reset.</p>
        <span class="demo-banner-progress" id="demoProgress">Tried 0 of 6 guest actions</span>
    </div>

    <section class="lp-hero">
        <div class="lp-hero-glow lp-hero-glow-a"></div>
        <div class="lp-hero-glow lp-hero-glow-b"></div>
        <div class="lp-shell">
            <div class="lp-hero-copy lp-hero-center lp-anim-rise">
                <span class="lp-pill">Limited tour · no sign-in</span>
                <h1>Try a handful of DTAS flows <em>without creating an account.</em></h1>
                <p>
                    Browse sample events, leave a visitor note, move work on a mini board, RSVP to a meeting,
                    skim a transparency gist, and send one Connect message. Live Zoom, clubs, and real records stay locked.
                </p>
                <div class="lp-hero-actions">
                    <a href="#demo-event" class="lp-btn lp-btn-primary">
                        Start the tour
                        <span class="material-symbols-outlined">play_circle</span>
                    </a>
                    <a href="javascript:void(0)" onclick="openRegister('/Demo.aspx')" class="lp-btn lp-btn-ghost">
                        Create an account for full access
                    </a>
                </div>
                <ul class="demo-limits" id="demoTryList">
                    <li data-try="browse"><span class="material-symbols-outlined">check_circle</span> Browse 3 sample events</li>
                    <li data-try="note"><span class="material-symbols-outlined">check_circle</span> Leave a visitor note</li>
                    <li data-try="board"><span class="material-symbols-outlined">check_circle</span> Move tasks to In Review</li>
                    <li data-try="meeting"><span class="material-symbols-outlined">check_circle</span> RSVP and check agenda</li>
                    <li data-try="records"><span class="material-symbols-outlined">check_circle</span> Filter sample records</li>
                    <li data-try="connect"><span class="material-symbols-outlined">check_circle</span> Send one Connect message</li>
                </ul>
            </div>
        </div>
    </section>

    <section class="demo-wrap demo-grid">

        <article class="demo-card demo-card-wide" id="demo-event">
            <header class="demo-card-head">
                <div>
                    <span class="demo-kicker">1 · Campus events</span>
                    <h2>Public event gist</h2>
                </div>
                <span class="demo-badge demo-badge-public" id="demoEventBadge">Public · Planned</span>
            </header>
            <div class="demo-event-tabs" role="tablist" aria-label="Sample events">
                <button type="button" class="demo-chip is-on" data-event="forum" role="tab" aria-selected="true">Sustainability Forum</button>
                <button type="button" class="demo-chip" data-event="council" role="tab" aria-selected="false">Student Council</button>
                <button type="button" class="demo-chip" data-event="board" role="tab" aria-selected="false">Board briefing</button>
            </div>
            <div class="demo-event-meta" id="demoEventMeta"></div>
            <p class="demo-copy" id="demoEventCopy"></p>
            <dl class="demo-gist">
                <div>
                    <dt>Purpose</dt>
                    <dd id="demoEventPurpose"></dd>
                </div>
                <div>
                    <dt>Lead</dt>
                    <dd id="demoEventLead"></dd>
                </div>
            </dl>
            <div class="demo-notes">
                <h3>Visitor notes</h3>
                <div id="demoNoteList" class="demo-note-list"></div>
                <div class="demo-note-form">
                    <label for="demoNoteInput" class="sr-only">Visitor note</label>
                    <input id="demoNoteInput" type="text" maxlength="160" placeholder="Leave a short public note (sample, not saved)" />
                    <button type="button" class="demo-btn demo-btn-primary" id="demoNoteSend">Post note</button>
                </div>
                <p class="demo-hint" id="demoNoteHint">You can post up to 2 sample notes.</p>
            </div>
            <div class="demo-card-actions">
                <button type="button" class="demo-btn demo-btn-locked" onclick="openLogin()">
                    Request to join
                    <span class="material-symbols-outlined">lock</span>
                </button>
            </div>
        </article>

        <article class="demo-card demo-card-wide" id="demo-board">
            <header class="demo-card-head">
                <div>
                    <span class="demo-kicker">2 · Mini task board</span>
                    <h2>Sample event workspace</h2>
                </div>
                <button type="button" class="demo-reset" id="demoResetBoard">Reset board</button>
            </header>
            <p class="demo-copy">Move <strong>DEMO-1</strong> and <strong>DEMO-2</strong> as far as In Review. Marking Done, assigning people, and adding tasks stay locked.</p>
            <div class="demo-board demo-board-4" id="demoBoard">
                <section class="demo-col" data-col="todo">
                    <h3>To Do <span class="demo-count" data-count="todo">0</span></h3>
                    <div class="demo-col-body" id="col-todo">
                        <article class="demo-task" data-task="demo-1" data-status="todo">
                            <span class="demo-key">DEMO-1</span>
                            <p>Publish agenda draft</p>
                            <button type="button" class="demo-task-btn" data-advance="1">Start</button>
                        </article>
                        <article class="demo-task" data-task="demo-4" data-status="todo">
                            <span class="demo-key">DEMO-4</span>
                            <p>Print visitor badges</p>
                            <button type="button" class="demo-task-btn demo-task-btn-locked" onclick="openLogin()">Assign</button>
                        </article>
                    </div>
                </section>
                <section class="demo-col" data-col="progress">
                    <h3>In Progress <span class="demo-count" data-count="progress">0</span></h3>
                    <div class="demo-col-body" id="col-progress">
                        <article class="demo-task" data-task="demo-2" data-status="progress">
                            <span class="demo-key">DEMO-2</span>
                            <p>Book auditorium</p>
                            <button type="button" class="demo-task-btn" data-advance="1">Send to review</button>
                        </article>
                    </div>
                </section>
                <section class="demo-col" data-col="review">
                    <h3>In Review <span class="demo-count" data-count="review">0</span></h3>
                    <div class="demo-col-body" id="col-review"></div>
                </section>
                <section class="demo-col" data-col="done">
                    <h3>Done <span class="demo-count" data-count="done">0</span></h3>
                    <div class="demo-col-body" id="col-done">
                        <article class="demo-task demo-task-static" data-task="demo-3" data-status="done">
                            <span class="demo-key">DEMO-3</span>
                            <p>Invite student reps</p>
                            <span class="demo-done-tag">Sample complete</span>
                        </article>
                    </div>
                </section>
            </div>
            <p class="demo-hint" id="demoBoardHint" aria-live="polite"></p>
        </article>

        <article class="demo-card" id="demo-meeting">
            <header class="demo-card-head">
                <div>
                    <span class="demo-kicker">3 · Sample meeting</span>
                    <h2>Planning huddle</h2>
                </div>
                <span class="demo-badge" id="demoRsvpBadge">Scheduled</span>
            </header>
            <ul class="demo-meeting">
                <li><span class="material-symbols-outlined">schedule</span> 18 Sep 2026 · 10:00 Asia/Kolkata</li>
                <li><span class="material-symbols-outlined">videocam</span> Zoom is not live in guest demo</li>
                <li><span class="material-symbols-outlined">group</span> Host + 4 sample participants</li>
            </ul>
            <h3 class="demo-sub">Agenda checklist</h3>
            <ul class="demo-agenda">
                <li><label><input type="checkbox" data-agenda="1" /> Confirm venue and seating</label></li>
                <li><label><input type="checkbox" data-agenda="1" /> Share the public agenda gist</label></li>
                <li><label><input type="checkbox" data-agenda="1" /> Brief student representatives</label></li>
            </ul>
            <div class="demo-card-actions">
                <button type="button" class="demo-btn demo-btn-primary" data-rsvp="Going">RSVP: Going</button>
                <button type="button" class="demo-btn demo-btn-ghost" data-rsvp="Maybe">Maybe</button>
            </div>
            <div class="demo-card-actions">
                <button type="button" class="demo-btn demo-btn-locked" onclick="openLogin()">
                    Start as host
                    <span class="material-symbols-outlined">lock</span>
                </button>
                <button type="button" class="demo-btn demo-btn-ghost demo-btn-locked" onclick="openLogin()">
                    Join Zoom
                </button>
            </div>
            <p class="demo-hint" id="demoMeetingHint"></p>
        </article>

        <article class="demo-card" id="demo-records">
            <header class="demo-card-head">
                <div>
                    <span class="demo-kicker">4 · Transparency gist</span>
                    <h2>Sample public records</h2>
                </div>
            </header>
            <p class="demo-copy">Filter canned records. Full reports, downloads, and the live portal stay behind sign-in.</p>
            <div class="demo-event-tabs" role="tablist" aria-label="Record types">
                <button type="button" class="demo-chip is-on" data-record-filter="all">All</button>
                <button type="button" class="demo-chip" data-record-filter="decision">Decisions</button>
                <button type="button" class="demo-chip" data-record-filter="minutes">Minutes</button>
                <button type="button" class="demo-chip" data-record-filter="budget">Budget</button>
            </div>
            <ul class="demo-records" id="demoRecordList"></ul>
            <div class="demo-record-detail" id="demoRecordDetail" hidden></div>
            <div class="demo-card-actions">
                <button type="button" class="demo-btn demo-btn-locked" onclick="openLogin()">
                    Open live portal
                    <span class="material-symbols-outlined">lock</span>
                </button>
            </div>
        </article>

        <article class="demo-card" id="demo-connect">
            <header class="demo-card-head">
                <div>
                    <span class="demo-kicker">5 · Connect (sample)</span>
                    <h2>Sustainability Forum room</h2>
                </div>
                <span class="demo-badge">Read + 1 reply</span>
            </header>
            <div class="demo-chat" id="demoChat">
                <div class="demo-chat-msg">
                    <strong>Faculty lead</strong>
                    <p>Agenda draft is in the workspace. Please review DEMO-1 before Friday.</p>
                </div>
                <div class="demo-chat-msg">
                    <strong>Student rep</strong>
                    <p>Can we add a question on recycling bins in the hostels?</p>
                </div>
            </div>
            <div class="demo-note-form">
                <label for="demoChatInput" class="sr-only">Connect message</label>
                <input id="demoChatInput" type="text" maxlength="140" placeholder="Send one sample reply as Guest" />
                <button type="button" class="demo-btn demo-btn-primary" id="demoChatSend">Send</button>
            </div>
            <p class="demo-hint" id="demoChatHint">One guest message. Creating rooms and inviting members is locked.</p>
        </article>

        <article class="demo-card" id="demo-propose">
            <header class="demo-card-head">
                <div>
                    <span class="demo-kicker">6 · Proposal preview</span>
                    <h2>Draft a student proposal</h2>
                </div>
            </header>
            <p class="demo-copy">Fill a short sample form. It will not be submitted to faculty or stored.</p>
            <div class="demo-form">
                <label>
                    Title
                    <input id="demoProposeTitle" type="text" maxlength="80" placeholder="Hostel recycling stations" />
                </label>
                <label>
                    Type
                    <select id="demoProposeType">
                        <option>Student activity</option>
                        <option>Public forum</option>
                        <option>Workshop</option>
                    </select>
                </label>
                <label>
                    Gist
                    <textarea id="demoProposeGist" maxlength="220" rows="3" placeholder="What should the campus see in the public gist?"></textarea>
                </label>
                <button type="button" class="demo-btn demo-btn-primary" id="demoProposeSend">Preview proposal</button>
            </div>
            <div class="demo-propose-result" id="demoProposeResult" hidden></div>
            <p class="demo-hint">Creating clubs, invite codes, and real event records stay locked.</p>
        </article>

        <article class="demo-card demo-card-wide demo-card-locked-list">
            <header class="demo-card-head">
                <div>
                    <span class="demo-kicker">Not in this demo</span>
                    <h2>The rest of DTAS stays behind sign-in</h2>
                </div>
            </header>
            <ul class="demo-locked demo-locked-grid">
                <li><span class="material-symbols-outlined">lock</span> Create live events or approve proposals</li>
                <li><span class="material-symbols-outlined">lock</span> Clubs, invite codes, and member management</li>
                <li><span class="material-symbols-outlined">lock</span> Host Zoom or view recordings</li>
                <li><span class="material-symbols-outlined">lock</span> Live transparency portal and reports</li>
                <li><span class="material-symbols-outlined">lock</span> Assign people, mark tasks Done, add issues</li>
                <li><span class="material-symbols-outlined">lock</span> User, faculty, and admin dashboards</li>
            </ul>
            <div class="demo-card-actions">
                <a href="javascript:void(0)" onclick="openLogin()" class="demo-btn demo-btn-primary">Sign in</a>
                <a href="javascript:void(0)" onclick="openRegister('/Demo.aspx')" class="demo-btn demo-btn-ghost">Register</a>
            </div>
        </article>
    </section>

    <section class="lp-cta-wrap">
        <div class="lp-shell">
            <div class="lp-cta">
                <h2>Ready for the real workspace?</h2>
                <p>Create an account to run events, assign tasks, host meetings, and publish a live transparency record.</p>
                <div class="lp-hero-actions">
                    <a href="javascript:void(0)" onclick="openRegister('/Demo.aspx')" class="lp-btn lp-btn-primary">Create an account</a>
                    <a href="/Features.aspx" class="lp-btn lp-btn-ghost">Back to features</a>
                </div>
            </div>
        </div>
    </section>

</div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="/Assets/js/demo.js?v=2"></script>
</asp:Content>
