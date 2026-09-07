<%@ Page Title="FAQ | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="FAQ.aspx.cs" Inherits="DigitalTransparencySystem.FAQ" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    FAQ | DTAS
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="/Assets/css/faq.css" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Hero Section -->
    <header class="relative w-full py-16 overflow-hidden bg-surface-container-low border-b border-border-light">
        <div class="absolute inset-0 opacity-40 pointer-events-none">
            <div class="absolute top-0 right-0 w-96 h-96 bg-primary-fixed-dim/30 rounded-full blur-[120px] -translate-y-1/2 translate-x-1/2"></div>
            <div class="absolute bottom-0 left-0 w-64 h-64 bg-primary-container/10 rounded-full blur-[100px] translate-y-1/2 -translate-x-1/2"></div>
        </div>
        <div class="relative z-10 max-w-7xl mx-auto px-container-padding-mobile md:px-container-padding-desktop text-center">
            <span class="inline-block bg-surface-container-lowest text-slate-deep font-badge-cap text-badge-cap px-4 py-1.5 rounded-full mb-4 border border-border-light">SUPPORT CENTER</span>
            <h1 class="font-headline-lg-mobile md:font-display-lg text-slate-deep mb-4">Frequently Asked Questions</h1>
            <p class="font-body-lg text-on-surface-variant max-w-2xl mx-auto mb-8 leading-relaxed">
                Everything you need to know about DTAS and how to participate.
            </p>
            <div class="max-w-xl mx-auto relative">
                <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-pacific-blue">search</span>
                <asp:TextBox ID="txtSearch" runat="server" CssClass="w-full pl-12 pr-4 py-3 rounded-lg border border-border-light bg-surface-container-lowest text-on-surface shadow-sm text-body-md transition-all faq-search" placeholder="Search questions..."></asp:TextBox>
            </div>
        </div>
    </header>

    <!-- FAQ Body -->
    <section class="max-w-7xl mx-auto px-container-padding-mobile md:px-container-padding-desktop py-12">
        <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
            <aside class="lg:col-span-3 space-y-3">
                <h3 class="font-label-md text-on-surface-variant uppercase tracking-widest mb-4">Categories</h3>
                <nav class="flex flex-col gap-2">
                    <button class="category-btn active flex items-center gap-3 px-4 py-2.5 rounded-lg bg-pacific-blue text-white font-title-lg text-left transition-all" data-category="general">
                        <span class="material-symbols-outlined">grid_view</span>
                        <span class="text-body-md font-medium">General</span>
                    </button>
                    <button class="category-btn flex items-center gap-3 px-4 py-2.5 rounded-lg hover:bg-surface-container-low transition-all text-left text-on-surface-variant" data-category="participation">
                        <span class="material-symbols-outlined">group</span>
                        <span class="text-body-md">Participation</span>
                    </button>
                    <button class="category-btn flex items-center gap-3 px-4 py-2.5 rounded-lg hover:bg-surface-container-low transition-all text-left text-on-surface-variant" data-category="technical">
                        <span class="material-symbols-outlined">settings</span>
                        <span class="text-body-md">Technical</span>
                    </button>
                    <button class="category-btn flex items-center gap-3 px-4 py-2.5 rounded-lg hover:bg-surface-container-low transition-all text-left text-on-surface-variant" data-category="privacy">
                        <span class="material-symbols-outlined">lock</span>
                        <span class="text-body-md">Privacy</span>
                    </button>
                </nav>
            </aside>

            <section class="lg:col-span-9">
                <div class="space-y-3" id="faqAccordion">
                    <details class="faq-accordion-item group border border-border-light rounded-xl overflow-hidden transition-all duration-300" open="" data-category="general">
                        <summary class="flex items-center justify-between p-5 cursor-pointer hover:bg-surface-container/50 transition-colors list-none">
                            <h4 class="font-title-lg text-on-surface transition-all group-open:text-pacific-blue">What is DTAS?</h4>
                            <span class="material-symbols-outlined text-pacific-blue group-open:rotate-180 transition-transform">expand_more</span>
                        </summary>
                        <div class="px-5 pb-5 pt-0 font-body-md text-on-surface-variant leading-relaxed text-[14px]">
                            DTAS stands for Digital Transparency and Accountability System. It ensures educational data is immutable and verifiable by the entire community.
                        </div>
                    </details>

                    <details class="faq-accordion-item group border border-border-light rounded-xl overflow-hidden transition-all duration-300" data-category="general">
                        <summary class="flex items-center justify-between p-5 cursor-pointer hover:bg-surface-container/50 transition-colors list-none">
                            <h4 class="font-title-lg text-on-surface transition-all group-open:text-pacific-blue">How are decisions tracked?</h4>
                            <span class="material-symbols-outlined text-pacific-blue group-open:rotate-180 transition-transform">expand_more</span>
                        </summary>
                        <div class="px-5 pb-5 pt-0 font-body-md text-on-surface-variant leading-relaxed text-[14px]">
                            Every decision goes through a multi-stage tracking process including proposal, public comment, voting, and implementation with timestamped hashes.
                        </div>
                    </details>

                    <details class="faq-accordion-item group border border-border-light rounded-xl overflow-hidden transition-all duration-300" data-category="participation">
                        <summary class="flex items-center justify-between p-5 cursor-pointer hover:bg-surface-container/50 transition-colors list-none">
                            <h4 class="font-title-lg text-on-surface transition-all group-open:text-pacific-blue">Can I submit a proposal as a student?</h4>
                            <span class="material-symbols-outlined text-pacific-blue group-open:rotate-180 transition-transform">expand_more</span>
                        </summary>
                        <div class="px-5 pb-5 pt-0 font-body-md text-on-surface-variant leading-relaxed text-[14px]">
                            Yes, students are primary stakeholders. Once verified, you can draft "Student Initiatives" requiring peer signatures for formal review.
                        </div>
                    </details>

                    <details class="faq-accordion-item group border border-border-light rounded-xl overflow-hidden transition-all duration-300" data-category="privacy">
                        <summary class="flex items-center justify-between p-5 cursor-pointer hover:bg-surface-container/50 transition-colors list-none">
                            <h4 class="font-title-lg text-on-surface transition-all group-open:text-pacific-blue">Is my data shared with third parties?</h4>
                            <span class="material-symbols-outlined text-pacific-blue group-open:rotate-180 transition-transform">expand_more</span>
                        </summary>
                        <div class="px-5 pb-5 pt-0 font-body-md text-on-surface-variant leading-relaxed text-[14px]">
                            DTAS operates on a privacy-first model. We do not sell user data. Anonymized data may be used for research with zero-knowledge encryption.
                        </div>
                    </details>

                    <details class="faq-accordion-item group border border-border-light rounded-xl overflow-hidden transition-all duration-300" data-category="general">
                        <summary class="flex items-center justify-between p-5 cursor-pointer hover:bg-surface-container/50 transition-colors list-none">
                            <h4 class="font-title-lg text-on-surface transition-all group-open:text-pacific-blue">How do I report a discrepancy?</h4>
                            <span class="material-symbols-outlined text-pacific-blue group-open:rotate-180 transition-transform">expand_more</span>
                        </summary>
                        <div class="px-5 pb-5 pt-0 font-body-md text-on-surface-variant leading-relaxed text-[14px]">
                            Use the "Report Anomaly" button on any data card. This triggers an independent audit by the Transparency Board.
                        </div>
                    </details>
                </div>

                <div class="mt-8 bg-surface-container-low p-6 rounded-2xl flex flex-col md:flex-row items-center justify-between gap-6 border border-border-light">
                    <div class="space-y-1 text-center md:text-left">
                        <h2 class="font-headline-md text-slate-deep">Still need help?</h2>
                        <p class="font-body-md text-on-surface-variant text-[14px]">Our support team is available 24/7.</p>
                    </div>
                    <a href="/Contact.aspx" class="whitespace-nowrap bg-pacific-blue text-on-primary px-6 py-3 rounded-lg font-title-lg hover:bg-slate-deep transition-colors inline-block text-center">
                        Contact Support
                    </a>
                </div>
            </section>
        </div>
    </section>

    <!-- Illustration Section -->
    <section class="max-w-7xl mx-auto px-container-padding-mobile md:px-container-padding-desktop pb-12">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6 items-center bg-surface-container-lowest border border-border-light rounded-2xl overflow-hidden shadow-sm">
            <div class="p-8 md:p-12 space-y-4">
                <span class="font-badge-cap text-badge-cap text-slate-deep bg-surface-container px-3 py-1 rounded-full uppercase text-[11px]">Knowledge Base</span>
                <h2 class="font-headline-lg text-on-surface text-[28px]">Can't find what you need?</h2>
                <p class="font-body-lg text-on-surface-variant text-[14px] leading-relaxed">Our documentation covers advanced setups, governance frameworks, and data integration guides.</p>
                <div class="flex flex-wrap gap-4 pt-2">
                    <a class="flex items-center gap-2 font-label-md text-pacific-blue group" href="#">
                        View Documentation
                        <span class="material-symbols-outlined text-[18px] group-hover:translate-x-1 transition-transform">arrow_forward</span>
                    </a>
                </div>
            </div>
            <div class="h-[320px] w-full">
                <img class="w-full h-full object-cover" src="/Assets/images/knowledge-base.jpg" alt="Modern academic office with digital dashboards" />
            </div>
        </div>
    </section>

</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="/Assets/js/faq.js"></script>
</asp:Content>