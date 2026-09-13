<%@ Page Title="About Us | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="About.aspx.cs" Inherits="DigitalTransparencySystem.About" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    About Us | DTAS
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="/Assets/css/default.css?v=9" />
    <link rel="stylesheet" type="text/css" href="/Assets/css/about.css?v=6" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
<div class="lp">

    <section class="lp-hero">
        <div class="lp-hero-glow lp-hero-glow-a"></div>
        <div class="lp-hero-glow lp-hero-glow-b"></div>
        <div class="lp-shell">
            <div class="lp-hero-copy about-hero-copy lp-anim-rise">
                <span class="lp-pill">
                    <span class="lp-dot"></span>
                    About us
                </span>
                <h1>
                    We keep campus decisions<br />
                    <em>where the community can find them.</em>
                </h1>
                <p>
                    We are DTAS, the Digital Transparency and Accountability System.
                    We built a campus web record for educational community decision making.
                </p>
            </div>
        </div>
    </section>

    <section class="about-mission">
        <div class="lp-shell">
            <blockquote class="about-mission-card lp-reveal">
                <span class="lp-kicker">Our mission</span>
                <p>
                    To give students, faculty, and staff one honest place to see what was proposed,
                    what was decided, who is responsible, and whether the work was finished.
                </p>
            </blockquote>
        </div>
    </section>

    <section class="about-story">
        <div class="lp-shell about-story-grid">
            <article class="about-story-col lp-reveal">
                <span class="lp-kicker">Our story</span>
                <h2>Why we started</h2>
                <p>
                    Campus choices often split across email, slides, and private lists.
                    Later, people cannot say who approved a decision or whether the work closed.
                </p>
                <p>
                    We started DTAS so that chain stays together: the proposal, the meeting,
                    the decision, and the follow-through.
                </p>
            </article>
            <article class="about-story-col lp-reveal">
                <span class="lp-kicker">Who we serve</span>
                <h2>The educational community</h2>
                <p>
                    We serve the people who live the decision: students who propose and carry work,
                    faculty who teach and assign, staff who run events, and a system admin who
                    reviews identity and keeps the record in order.
                </p>
                <p>
                    Admin does not create events, clubs, or assignments. That work belongs to the campus,
                    not to a central operator.
                </p>
            </article>
        </div>
    </section>

    <section class="lp-roles about-bottom">
        <div class="lp-shell">
            <div class="lp-section-head lp-reveal">
                <span class="lp-kicker">What we believe</span>
                <h2>Four values we work by</h2>
                <p>These guide how we treat the campus record.</p>
            </div>
            <div class="lp-module-grid about-values">
                <article class="lp-module">
                    <span class="material-symbols-outlined">visibility</span>
                    <h3>Be visible</h3>
                    <p>A decision should be findable later, not buried in a thread only a few people saw.</p>
                </article>
                <article class="lp-module">
                    <span class="material-symbols-outlined">handshake</span>
                    <h3>Follow through</h3>
                    <p>Approval is not the end. The work after the decision is part of the same story.</p>
                </article>
                <article class="lp-module">
                    <span class="material-symbols-outlined">verified_user</span>
                    <h3>Know who is speaking</h3>
                    <p>People sign in with institutional email. Campus ID is reviewed before full access.</p>
                </article>
                <article class="lp-module">
                    <span class="material-symbols-outlined">history_edu</span>
                    <h3>Leave a trail</h3>
                    <p>Status changes and identity review are logged. Restricting work holds it. It does not erase it.</p>
                </article>
            </div>
            <p class="about-note">If you need to reach us about access or identity review, write through <a href="/Contact.aspx">Contact</a> or email edu.transparency@gmail.com.</p>
        </div>
    </section>

</div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="/Assets/js/default.js?v=4"></script>
</asp:Content>
