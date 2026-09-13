<%@ Page Title="Features | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Features.aspx.cs" Inherits="DigitalTransparencySystem.Features" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Features | DTAS
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="/Assets/css/features.css?v=3" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
<div class="lp">

    <section class="lp-hero">
        <div class="lp-hero-glow lp-hero-glow-a"></div>
        <div class="lp-hero-glow lp-hero-glow-b"></div>
        <div class="lp-shell">
            <div class="lp-hero-copy lp-hero-center lp-anim-rise">
                <span class="lp-pill">
                    <span class="lp-dot"></span>
                    What DTAS does
                </span>
                <h1>
                    Transparent decisions.<br />
                    <em>Accountable actions.</em>
                </h1>
                <p>
                    DTAS keeps proposals, meetings, decisions, and follow-through in one campus record
                    that students, faculty, and staff can actually follow.
                </p>
                <div class="lp-hero-actions">
                    <a href="/Demo.aspx" class="lp-btn lp-btn-primary">
                        Try guest demo
                        <span class="material-symbols-outlined">play_circle</span>
                    </a>
                    <a href="/FAQ.aspx" class="lp-btn lp-btn-ghost">
                        Read the FAQ
                    </a>
                </div>
            </div>
        </div>
    </section>

    <section class="py-12 px-container-padding-mobile md:px-container-padding-desktop max-w-7xl mx-auto">
        <div class="grid grid-cols-12 gap-4">

            <div class="col-span-12 md:col-span-8 bg-surface-container-lowest border border-border-light rounded-2xl p-6 bento-card-hover flex flex-col md:flex-row gap-6 items-center overflow-hidden">
                <div class="flex-1">
                    <span class="material-symbols-outlined text-pacific-blue text-4xl mb-3 block">fact_check</span>
                    <h3 class="font-headline-md text-headline-md text-slate-deep mb-2">Decisions</h3>
                    <p class="text-on-surface-variant font-body-md text-[14px] leading-relaxed">
                        Record what was chosen, who is responsible, and the status from proposed through completed.
                        The history stays with the decision.
                    </p>
                </div>
                <div class="flex-1 bg-surface-container rounded-lg p-5 w-full">
                    <div class="space-y-3 font-label-md text-[13px] text-on-surface-variant">
                        <div class="flex justify-between"><span>Propose</span><span class="font-bold text-pacific-blue">Proposed</span></div>
                        <div class="flex justify-between"><span>Deliberate</span><span class="font-bold text-pacific-blue">Under review</span></div>
                        <div class="flex justify-between"><span>Approve</span><span class="font-bold text-pacific-blue">Approved</span></div>
                        <div class="flex justify-between"><span>Follow through</span><span class="font-bold text-pacific-blue">Completed</span></div>
                    </div>
                </div>
            </div>
            <div class="col-span-12 md:col-span-4 bg-gradient-to-br from-slate-deep to-pacific-blue text-white rounded-2xl p-6 bento-card-hover border-0">
                <span class="material-symbols-outlined text-white text-4xl mb-3 block">groups</span>
                <h3 class="font-headline-md text-headline-md mb-2">Meetings</h3>
                <p class="text-white/80 font-body-md text-[14px] leading-relaxed">
                    Minutes, attendance, and Zoom rooms stay with the decision they inform. A room closes when the scheduled time is over.
                </p>
            </div>

            <div class="col-span-12 md:col-span-4 bg-secondary-container text-on-secondary-container rounded-2xl p-6 bento-card-hover border-0">
                <span class="material-symbols-outlined text-on-secondary-container text-4xl mb-3 block">campaign</span>
                <h3 class="font-headline-md text-headline-md mb-2">Events</h3>
                <p class="text-on-secondary-container/80 font-body-md text-[14px] leading-relaxed">
                    Faculty and staff create events. Students propose them. Joining a public event is a request, not an automatic join.
                </p>
            </div>
            <div class="col-span-12 md:col-span-8 bg-surface-container-lowest border border-border-light rounded-2xl p-6 bento-card-hover">
                <div class="flex flex-col md:flex-row justify-between items-start md:items-center mb-4 gap-4">
                    <div>
                        <span class="material-symbols-outlined text-pacific-blue text-4xl mb-2 block">task_alt</span>
                        <h3 class="font-headline-md text-headline-md text-slate-deep">Tasks and workspaces</h3>
                    </div>
                </div>
                <p class="text-on-surface-variant font-body-md text-[14px] max-w-xl leading-relaxed">
                    Action items stay linked to the decision they serve. Teams update progress, attach proof, and keep overdue work visible.
                </p>
            </div>

            <div class="col-span-12 md:col-span-3 bg-surface-container-lowest border border-border-light rounded-2xl p-5 bento-card-hover">
                <div class="w-10 h-10 rounded-lg bg-surface-container flex items-center justify-center mb-3">
                    <span class="material-symbols-outlined text-slate-deep">school</span>
                </div>
                <h4 class="font-title-lg text-title-lg text-slate-deep mb-1">Assignments</h4>
                <p class="text-on-surface-variant font-label-md text-[13px] leading-relaxed">Faculty create groups and track contribution. Percent is calculated, not typed in.</p>
            </div>
            <div class="col-span-12 md:col-span-3 bg-surface-container-lowest border border-border-light rounded-2xl p-5 bento-card-hover">
                <div class="w-10 h-10 rounded-lg bg-surface-container flex items-center justify-center mb-3">
                    <span class="material-symbols-outlined text-pacific-blue">groups</span>
                </div>
                <h4 class="font-title-lg text-title-lg text-slate-deep mb-1">Clubs</h4>
                <p class="text-on-surface-variant font-label-md text-[13px] leading-relaxed">Campus clubs with membership requests and a shared workspace.</p>
            </div>
            <div class="col-span-12 md:col-span-3 bg-surface-container-lowest border border-border-light rounded-2xl p-5 bento-card-hover">
                <div class="w-10 h-10 rounded-lg bg-surface-container flex items-center justify-center mb-3">
                    <span class="material-symbols-outlined text-slate-deep">forum</span>
                </div>
                <h4 class="font-title-lg text-title-lg text-slate-deep mb-1">Connect</h4>
                <p class="text-on-surface-variant font-label-md text-[13px] leading-relaxed">Group conversation that stays inside the same workspace.</p>
            </div>
            <div class="col-span-12 md:col-span-3 bg-surface-container-lowest border border-border-light rounded-2xl p-5 bento-card-hover">
                <div class="w-10 h-10 rounded-lg bg-surface-container flex items-center justify-center mb-3">
                    <span class="material-symbols-outlined text-pacific-blue">how_to_vote</span>
                </div>
                <h4 class="font-title-lg text-title-lg text-slate-deep mb-1">Polls</h4>
                <p class="text-on-surface-variant font-label-md text-[13px] leading-relaxed">Collect a campus vote and keep the result with the record.</p>
            </div>

            <div class="col-span-12 md:col-span-6 bg-surface-container-lowest border border-border-light rounded-2xl p-6 bento-card-hover overflow-hidden relative group">
                <div class="relative z-10">
                    <span class="material-symbols-outlined text-slate-deep text-4xl mb-3 block">public</span>
                    <h3 class="font-headline-md text-headline-md text-slate-deep mb-2">Transparency portal</h3>
                    <p class="text-on-surface-variant font-body-md text-[14px] mb-4 leading-relaxed">
                        A signed-in view of events, decisions, and progress. Guests see the public site. The portal itself requires an account.
                    </p>
                    <ul class="space-y-2">
                        <li class="flex items-center gap-2 text-label-md font-label-md text-pacific-blue text-[13px]">
                            <span class="material-symbols-outlined text-[16px]">check_circle</span>
                            Role-based access
                        </li>
                        <li class="flex items-center gap-2 text-label-md font-label-md text-pacific-blue text-[13px]">
                            <span class="material-symbols-outlined text-[16px]">check_circle</span>
                            Email and campus ID before full access
                        </li>
                    </ul>
                </div>
                <div class="absolute -right-8 -bottom-8 w-40 h-40 bg-primary-fixed-dim/30 rounded-full group-hover:scale-110 transition-transform"></div>
            </div>
            <div class="col-span-12 md:col-span-6 bg-gradient-to-br from-slate-deep via-pacific-blue to-pacific-blue text-white rounded-2xl p-6 bento-card-hover relative overflow-hidden">
                <div class="relative z-10">
                    <span class="material-symbols-outlined text-white text-4xl mb-3 block">verified_user</span>
                    <h3 class="font-headline-md text-headline-md mb-2">Identity and audit</h3>
                    <p class="text-white/80 font-body-md text-[14px] mb-6 leading-relaxed">
                        Sign in with institutional email or Google. Campus ID is reviewed. Admins restrict or restore work. The log of those actions is kept.
                    </p>
                    <div class="flex flex-wrap gap-2">
                        <span class="px-3 py-1 rounded-full bg-white/15 text-[12px] font-bold">OTP email</span>
                        <span class="px-3 py-1 rounded-full bg-white/15 text-[12px] font-bold">Campus ID</span>
                        <span class="px-3 py-1 rounded-full bg-white/15 text-[12px] font-bold">Append-only log</span>
                    </div>
                </div>
                <div class="absolute top-0 right-0 w-48 h-48 bg-white/10 blur-[60px]"></div>
            </div>
        </div>
    </section>

    <section class="lp-cta-wrap">
        <div class="lp-shell">
            <div class="lp-cta">
                <h2>Ready to see the record, not the rumor?</h2>
                <p>
                    Join the people using DTAS to keep campus decisions visible and follow-through honest.
                </p>
                <div class="lp-hero-actions">
                    <a href="/Demo.aspx" class="lp-btn lp-btn-primary">Try guest demo</a>
                    <a href="/FAQ.aspx" class="lp-btn lp-btn-ghost">Read the FAQ</a>
                </div>
            </div>
        </div>
    </section>

</div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="/Assets/js/features.js"></script>
</asp:Content>
