<%@ Page Title="Features | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Features.aspx.cs" Inherits="DigitalTransparencySystem.Features" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Features | DTAS
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="/Assets/css/features.css" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Hero Section -->
    <section class="features-hero relative pt-16 pb-20 px-container-padding-mobile md:px-container-padding-desktop overflow-hidden">
        <div class="max-w-7xl mx-auto text-center relative z-10">
            <span class="inline-block px-4 py-1.5 rounded-full bg-surface-container-low border border-border-light text-slate-deep font-badge-cap text-badge-cap uppercase mb-4">
                Platform Core
            </span>
            <h1 class="font-display-lg text-display-lg md:text-display-lg text-on-background mb-4 max-w-4xl mx-auto">
                Academic Precision, <span class="text-pacific-blue">Integrated Transparency.</span>
            </h1>
            <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl mx-auto mb-8 leading-relaxed">
                10 structural pillars designed to harmonize institutional workflows with radical public accountability.
            </p>
            <div class="flex flex-wrap justify-center gap-4">
                <a href="/Demo.aspx" class="bg-pacific-blue text-on-primary h-11 px-7 rounded-lg font-bold flex items-center gap-2 hover:bg-slate-deep transition-colors">
                    Try guest demo
                    <span class="material-symbols-outlined text-[18px]">play_circle</span>
                </a>
                <a href="/TechnicalSpec.pdf" class="border border-slate-deep text-slate-deep h-11 px-7 rounded-lg font-bold flex items-center gap-2 hover:bg-surface-container-low transition-colors">
                    Technical Spec
                    <span class="material-symbols-outlined text-[18px]">download</span>
                </a>
            </div>
        </div>
        <div class="absolute top-0 right-0 -translate-y-1/2 translate-x-1/4 w-[600px] h-[600px] bg-primary-fixed-dim/20 rounded-full blur-[120px]"></div>
    </section>

    <!-- Bento Grid Section -->
    <section class="py-12 px-container-padding-mobile md:px-container-padding-desktop max-w-7xl mx-auto">
        <div class="grid grid-cols-12 gap-4">
            <!-- Row 1 -->
            <div class="col-span-12 md:col-span-8 bg-surface-container-lowest border border-border-light rounded-2xl p-6 bento-card-hover flex flex-col md:flex-row gap-6 items-center overflow-hidden">
                <div class="flex-1">
                    <span class="material-symbols-outlined text-pacific-blue text-4xl mb-3 block">track_changes</span>
                    <h3 class="font-headline-md text-headline-md text-slate-deep mb-2">Decision Tracking</h3>
                    <p class="text-on-surface-variant font-body-md text-[14px] leading-relaxed">Full-lifecycle governance monitoring from proposal to final vote.</p>
                </div>
                <div class="flex-1 bg-surface-container rounded-lg p-5 w-full">
                    <div class="space-y-2">
                        <div class="h-2 w-3/4 bg-pacific-blue/15 rounded-full"></div>
                        <div class="h-2 w-1/2 bg-pacific-blue/10 rounded-full"></div>
                        <div class="h-2 w-full bg-pacific-blue/15 rounded-full"></div>
                        <div class="pt-3 flex justify-between items-center">
                            <span class="text-[11px] font-bold text-pacific-blue">Live Updates</span>
                            <span class="flex h-2 w-2 rounded-full bg-emerald-500 animate-pulse"></span>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-span-12 md:col-span-4 bg-gradient-to-br from-slate-deep to-pacific-blue text-white rounded-2xl p-6 bento-card-hover border-0">
                <span class="material-symbols-outlined text-white text-4xl mb-3 block">groups</span>
                <h3 class="font-headline-md text-headline-md mb-2">Meeting Management</h3>
                <p class="text-white/80 font-body-md text-[14px] leading-relaxed">Automated agenda generation and minute-taking for boards.</p>
            </div>
            <!-- Row 2 -->
            <div class="col-span-12 md:col-span-4 bg-secondary-container text-on-secondary-container rounded-2xl p-6 bento-card-hover border-0">
                <span class="material-symbols-outlined text-on-secondary-container text-4xl mb-3 block">assignment</span>
                <h3 class="font-headline-md text-headline-md mb-2">Task Assignment</h3>
                <p class="text-on-secondary-container/80 font-body-md text-[14px] leading-relaxed">Intelligent workload distribution across faculty tiers.</p>
            </div>
            <div class="col-span-12 md:col-span-8 bg-surface-container-lowest border border-border-light rounded-2xl p-6 bento-card-hover">
                <div class="flex flex-col md:flex-row justify-between items-start md:items-center mb-6 gap-4">
                    <div>
                        <span class="material-symbols-outlined text-pacific-blue text-4xl mb-2 block">fact_check</span>
                        <h3 class="font-headline-md text-headline-md text-slate-deep">Audit Reporting</h3>
                    </div>
                    <div class="text-right">
                        <span class="text-4xl font-bold text-pacific-blue">94%</span>
                        <span class="block text-badge-cap font-badge-cap uppercase text-on-surface-variant tracking-wider">Compliance</span>
                    </div>
                </div>
                <div class="w-full bg-surface-container-highest h-3 rounded-full overflow-hidden mb-4">
                    <div class="progress-bar bg-gradient-to-r from-pacific-blue to-slate-deep h-full rounded-full" style="width: 94%"></div>
                </div>
                <p class="text-on-surface-variant font-body-md text-[14px] max-w-xl leading-relaxed">Comprehensive compliance auditing engine monitoring institutional adherence in real-time.</p>
            </div>
            <!-- Row 3: 4 small cards -->
            <div class="col-span-12 md:col-span-3 bg-surface-container-lowest border border-border-light rounded-2xl p-5 bento-card-hover">
                <div class="w-10 h-10 rounded-lg bg-surface-container flex items-center justify-center mb-3">
                    <span class="material-symbols-outlined text-slate-deep">account_balance_wallet</span>
                </div>
                <h4 class="font-title-lg text-title-lg text-slate-deep mb-1">Resource Allocation</h4>
                <p class="text-on-surface-variant font-label-md text-[13px] leading-relaxed">Budgetary flow visualization.</p>
            </div>
            <div class="col-span-12 md:col-span-3 bg-surface-container-lowest border border-border-light rounded-2xl p-5 bento-card-hover">
                <div class="w-10 h-10 rounded-lg bg-surface-container flex items-center justify-center mb-3">
                    <span class="material-symbols-outlined text-pacific-blue">school</span>
                </div>
                <h4 class="font-title-lg text-title-lg text-slate-deep mb-1">Faculty Engagement</h4>
                <p class="text-on-surface-variant font-label-md text-[13px] leading-relaxed">Governance participation metrics.</p>
            </div>
            <div class="col-span-12 md:col-span-3 bg-surface-container-lowest border border-border-light rounded-2xl p-5 bento-card-hover">
                <div class="w-10 h-10 rounded-lg bg-surface-container flex items-center justify-center mb-3">
                    <span class="material-symbols-outlined text-slate-deep">security_update_good</span>
                </div>
                <h4 class="font-title-lg text-title-lg text-slate-deep mb-1">Compliance Monitor</h4>
                <p class="text-on-surface-variant font-label-md text-[13px] leading-relaxed">24/7 automated policy checks.</p>
            </div>
            <div class="col-span-12 md:col-span-3 bg-surface-container-lowest border border-border-light rounded-2xl p-5 bento-card-hover">
                <div class="w-10 h-10 rounded-lg bg-surface-container flex items-center justify-center mb-3">
                    <span class="material-symbols-outlined text-pacific-blue">cloud_done</span>
                </div>
                <h4 class="font-title-lg text-title-lg text-slate-deep mb-1">Document Vault</h4>
                <p class="text-on-surface-variant font-label-md text-[13px] leading-relaxed">Encrypted immutable versioning.</p>
            </div>
            <!-- Row 4 -->
            <div class="col-span-12 md:col-span-6 bg-surface-container-lowest border border-border-light rounded-2xl p-6 bento-card-hover overflow-hidden relative group">
                <div class="relative z-10">
                    <span class="material-symbols-outlined text-slate-deep text-4xl mb-3 block">public</span>
                    <h3 class="font-headline-md text-headline-md text-slate-deep mb-2">Public Portal</h3>
                    <p class="text-on-surface-variant font-body-md text-[14px] mb-4 leading-relaxed">Dedicated landing for stakeholders to view transparency reports.</p>
                    <ul class="space-y-2">
                        <li class="flex items-center gap-2 text-label-md font-label-md text-pacific-blue text-[13px]">
                            <span class="material-symbols-outlined text-[16px]">check_circle</span>
                            Verified Public Access
                        </li>
                        <li class="flex items-center gap-2 text-label-md font-label-md text-pacific-blue text-[13px]">
                            <span class="material-symbols-outlined text-[16px]">check_circle</span>
                            Interactive Data Visualization
                        </li>
                    </ul>
                </div>
                <div class="absolute -right-8 -bottom-8 w-40 h-40 bg-primary-fixed-dim/30 rounded-full group-hover:scale-110 transition-transform"></div>
            </div>
            <div class="col-span-12 md:col-span-6 bg-gradient-to-br from-slate-deep via-pacific-blue to-pacific-blue text-white rounded-2xl p-6 bento-card-hover relative overflow-hidden">
                <div class="relative z-10">
                    <span class="material-symbols-outlined text-white text-4xl mb-3 block">auto_graph</span>
                    <h3 class="font-headline-md text-headline-md mb-2">Predictive Governance</h3>
                    <p class="text-white/80 font-body-md text-[14px] mb-6 leading-relaxed">AI-driven forecasting for risk management and policy impact.</p>
                    <div class="flex gap-3">
                        <div class="h-16 flex-1 bg-white/10 rounded-lg flex items-end p-2 gap-1">
                            <div class="w-full bg-white/40 rounded-t h-[30%]"></div>
                            <div class="w-full bg-white/40 rounded-t h-[60%]"></div>
                            <div class="w-full bg-white h-[90%]"></div>
                            <div class="w-full bg-white/40 rounded-t h-[45%]"></div>
                        </div>
                        <div class="h-16 flex-1 bg-white/10 rounded-lg flex items-center justify-center">
                            <span class="text-[10px] font-bold uppercase tracking-widest opacity-60">Modeling...</span>
                        </div>
                    </div>
                </div>
                <div class="absolute top-0 right-0 w-48 h-48 bg-white/10 blur-[60px]"></div>
            </div>
        </div>
    </section>

    <!-- Final CTA Section -->
    <section class="py-12 px-container-padding-mobile md:px-container-padding-desktop bg-gradient-to-br from-slate-deep to-pacific-blue rounded-t-[48px] mx-4 md:mx-8">
        <div class="max-w-4xl mx-auto text-center">
            <h2 class="font-headline-lg text-headline-lg text-white mb-4">Ready to Elevate Your Standards?</h2>
            <p class="font-body-lg text-body-lg text-white/80 mb-8 leading-relaxed">Join 50+ leading institutions utilizing DTAS to foster trust.</p>
            <div class="flex flex-col md:flex-row justify-center gap-4">
                <a href="/Demo.aspx" class="bg-white text-slate-deep h-11 px-8 rounded-lg font-bold shadow-lg hover:-translate-y-0.5 transition-transform flex items-center justify-center">
                    Try guest demo
                </a>
                <a href="/TechnicalSpecs.aspx" class="border-2 border-white/40 text-white h-11 px-8 rounded-lg font-bold hover:bg-white/10 transition-all flex items-center justify-center">
                    Technical Specs
                </a>
            </div>
        </div>
    </section>

</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="/Assets/js/features.js"></script>
</asp:Content>