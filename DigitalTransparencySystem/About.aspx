<%@ Page Title="About Us | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="About.aspx.cs" Inherits="DigitalTransparencySystem.About" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    About Us | DTAS
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="/Assets/css/about.css" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Hero Section -->
    <section class="relative py-16 md:py-24 flex items-center justify-center overflow-hidden bg-surface-container-low border-b border-border-light px-container-padding-mobile">
        <div class="absolute inset-0 pointer-events-none opacity-40">
            <div class="absolute top-0 right-0 w-96 h-96 bg-primary-fixed-dim/30 rounded-full blur-[100px]"></div>
            <div class="absolute bottom-0 left-0 w-64 h-64 bg-primary-container/10 rounded-full blur-[80px]"></div>
        </div>
        <div class="max-w-4xl w-full text-center z-10">
            <div class="inline-flex items-center gap-2 px-4 py-1.5 rounded-full bg-surface-container-lowest border border-border-light mb-6">
                <span class="material-symbols-outlined text-[16px] text-pacific-blue">verified_user</span>
                <span class="font-badge-cap text-badge-cap text-slate-deep uppercase">Institutional Integrity</span>
            </div>
            <h1 class="font-display-lg text-display-lg md:text-[56px] text-slate-deep mb-6 tracking-tighter leading-tight">
                Precision and Accountability in <span class="text-pacific-blue">Higher Education.</span>
            </h1>
            <p class="font-body-lg text-body-lg text-on-surface-variant mb-8 max-w-2xl mx-auto leading-relaxed">
                DTAS provides the unified layer of truth for institutions to demonstrate fiduciary responsibility and academic excellence.
            </p>
            <div class="flex flex-col md:flex-row gap-4 justify-center">
                <a href="/Charter.aspx" class="bg-pacific-blue text-on-primary font-label-md px-8 py-3 rounded-lg shadow-sm hover:bg-slate-deep transition-colors inline-block text-center font-bold">
                    Review Charter
                </a>
                <a href="/Contact.aspx" class="border border-slate-deep text-slate-deep font-label-md px-8 py-3 rounded-lg hover:bg-surface-container-low transition-colors inline-block text-center font-bold">
                    Contact Advocacy
                </a>
            </div>
        </div>
    </section>

    <!-- Mission & Pillars Section -->
    <section class="py-12 px-container-padding-mobile md:px-container-padding-desktop max-w-7xl mx-auto">
        <div class="mb-10 text-center">
            <h2 class="font-headline-lg text-headline-lg-mobile md:text-headline-lg text-slate-deep mb-2">Mission & Legacy</h2>
            <div class="h-1.5 w-20 bg-pacific-blue mx-auto rounded-full"></div>
        </div>
        <div class="grid grid-cols-1 md:grid-cols-4 border border-border-light rounded-2xl overflow-hidden shadow-sm">
            <div class="bg-surface-container-lowest p-8 flex flex-col items-center text-center pill-border group hover:bg-surface-container-low transition-colors">
                <span class="material-symbols-outlined text-4xl text-pacific-blue mb-4">visibility</span>
                <h3 class="font-title-lg text-title-lg mb-2">Transparency</h3>
                <p class="font-body-md text-on-surface-variant text-[14px] leading-relaxed">Radical visibility into budgets and resource allocation.</p>
            </div>
            <div class="bg-gradient-to-br from-slate-deep to-pacific-blue p-8 flex flex-col items-center text-center pill-border group text-white">
                <span class="material-symbols-outlined text-4xl text-white mb-4">verified</span>
                <h3 class="font-title-lg text-title-lg mb-2">Veracity</h3>
                <p class="font-body-md text-white/80 text-[14px] leading-relaxed">Immutable and cryptographically sound data points.</p>
            </div>
            <div class="bg-surface-container-lowest p-8 flex flex-col items-center text-center pill-border group hover:bg-surface-container-low transition-colors">
                <span class="material-symbols-outlined text-4xl text-slate-deep mb-4">balance</span>
                <h3 class="font-title-lg text-title-lg mb-2">Equity</h3>
                <p class="font-body-md text-on-surface-variant text-[14px] leading-relaxed">Standardized metrics that reveal disparities.</p>
            </div>
            <div class="bg-surface-container p-8 flex flex-col items-center text-center group hover:bg-surface-container-low transition-colors">
                <span class="material-symbols-outlined text-4xl text-pacific-blue mb-4">gavel</span>
                <h3 class="font-title-lg text-title-lg mb-2">Ethics</h3>
                <p class="font-body-md text-on-surface-variant text-[14px] leading-relaxed">Moral framework governing data usage and conduct.</p>
            </div>
        </div>
    </section>

    <!-- Leadership Section -->
    <section class="py-12 bg-surface-container-low px-container-padding-mobile md:px-container-padding-desktop rounded-t-[48px] mx-4 md:mx-8">
        <div class="max-w-7xl mx-auto">
            <div class="flex flex-col md:flex-row justify-between items-end mb-10 gap-6">
                <div class="max-w-2xl">
                    <h2 class="font-headline-lg text-headline-lg-mobile md:text-headline-lg text-slate-deep mb-2">Leadership & Custodians</h2>
                    <p class="font-body-lg text-on-surface-variant text-[14px]">Our executive council dedicated to institutional reform.</p>
                </div>
                <div class="hidden md:block">
                    <span class="font-label-md text-outline">ESTABLISHED 2024</span>
                </div>
            </div>
            <div class="grid grid-cols-1 lg:grid-cols-12 gap-5">
                <div class="lg:col-span-8 bg-surface-container-lowest border border-border-light rounded-2xl p-8 shadow-sm hover:shadow-md transition-shadow group">
                    <div class="flex flex-col md:flex-row gap-8 items-center">
                        <div class="w-full md:w-1/3 aspect-[4/5] rounded-lg overflow-hidden">
                            <img class="w-full h-full object-cover grayscale-hover" src="/Assets/images/elena-rostrova.jpg" alt="Dr. Elena Rostova - Chief Executive Officer" />
                        </div>
                        <div class="w-full md:w-2/3">
                            <span class="font-badge-cap text-badge-cap text-white bg-slate-deep px-3 py-1 rounded-full mb-4 inline-block text-[11px]">CHIEF EXECUTIVE OFFICER</span>
                            <h3 class="font-headline-md text-headline-md text-slate-deep mb-3">Dr. Elena Rostova</h3>
                            <p class="font-body-md text-on-surface-variant text-[14px] mb-6 leading-relaxed">
                                A former university provost and Fulbright Scholar, pioneering frameworks that prioritize student outcomes.
                            </p>
                            <div class="flex gap-3">
                                <a href="#" class="text-pacific-blue hover:bg-surface-container-low transition-colors p-2 rounded-lg">
                                    <span class="material-symbols-outlined">link</span>
                                </a>
                                <a href="#" class="text-pacific-blue hover:bg-surface-container-low transition-colors p-2 rounded-lg">
                                    <span class="material-symbols-outlined">description</span>
                                </a>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="lg:col-span-4 flex flex-col gap-5">
                    <div class="bg-surface-container-lowest border border-border-light rounded-2xl p-6 shadow-sm flex-1 group hover:shadow-md transition-shadow">
                        <div class="flex items-center gap-4 mb-4">
                            <div class="w-16 h-16 rounded-full overflow-hidden shrink-0">
                                <img class="w-full h-full object-cover grayscale-hover" src="/Assets/images/marcus-thorne.jpg" alt="Marcus Thorne - Architecture" />
                            </div>
                            <div>
                                <h4 class="font-title-lg text-title-lg text-slate-deep text-[16px]">Marcus Thorne</h4>
                                <p class="font-label-md text-outline uppercase tracking-wider text-[11px]">Architecture</p>
                            </div>
                        </div>
                        <p class="font-body-md text-on-surface-variant text-[13px] leading-relaxed">Lead systems architect specialized in zero-knowledge proofs.</p>
                    </div>
                    <div class="bg-surface-container-lowest border border-border-light rounded-2xl p-6 shadow-sm flex-1 group hover:shadow-md transition-shadow">
                        <div class="flex items-center gap-4 mb-4">
                            <div class="w-16 h-16 rounded-full overflow-hidden shrink-0">
                                <img class="w-full h-full object-cover grayscale-hover" src="/Assets/images/sarah-chen.jpg" alt="Sarah Chen - Legal & Compliance" />
                            </div>
                            <div>
                                <h4 class="font-title-lg text-title-lg text-slate-deep text-[16px]">Sarah Chen</h4>
                                <p class="font-label-md text-outline uppercase tracking-wider text-[11px]">Legal & Compliance</p>
                            </div>
                        </div>
                        <p class="font-body-md text-on-surface-variant text-[13px] leading-relaxed">Expert in educational law and data privacy regulations.</p>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <!-- CTA Section -->
    <section class="py-12 px-container-padding-mobile">
        <div class="max-w-7xl mx-auto bg-gradient-to-br from-slate-deep to-pacific-blue rounded-2xl p-10 md:p-16 text-center relative overflow-hidden">
            <div class="absolute inset-0 opacity-10 pointer-events-none">
                <div class="absolute -top-1/2 -left-1/4 w-[100%] h-[200%] rotate-12 bg-gradient-to-r from-white to-transparent"></div>
            </div>
            <div class="relative z-10">
                <h2 class="font-display-lg text-white mb-6 tracking-tight text-[36px] md:text-[48px]">Empower Institutional Truth.</h2>
                <p class="font-body-lg text-white/70 mb-8 max-w-xl mx-auto leading-relaxed">
                    Join the growing network of transparent universities rebuilding trust.
                </p>
                <div class="flex flex-col md:flex-row gap-4 justify-center">
                    <a href="/Demo.aspx" class="bg-white text-slate-deep font-label-md px-10 py-3 rounded-lg shadow-lg hover:-translate-y-0.5 transition-all inline-block text-center font-bold">
                        Try guest demo
                    </a>
                    <a href="/TrustReport.aspx" class="border-2 border-white/30 text-white font-label-md px-10 py-3 rounded-lg hover:bg-white/10 transition-all inline-block text-center font-bold">
                        Trust Report
                    </a>
                </div>
            </div>
        </div>
    </section>

</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="/Assets/js/about.js"></script>
</asp:Content>