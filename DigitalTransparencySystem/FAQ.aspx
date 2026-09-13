<%@ Page Title="FAQ | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="FAQ.aspx.cs" Inherits="DigitalTransparencySystem.FAQ" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    FAQ | DTAS
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="/Assets/css/faq.css?v=3" />
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
                    Support
                </span>
                <h1>Frequently asked questions</h1>
                <p>Short answers for people who are new to DTAS.</p>
                <div class="lp-hero-actions">
                    <a href="/Contact.aspx" class="lp-btn lp-btn-primary">
                        Contact us
                        <span class="material-symbols-outlined">mail</span>
                    </a>
                    <a href="/Features.aspx" class="lp-btn lp-btn-ghost">
                        See what DTAS does
                    </a>
                </div>
                <ul class="lp-trust">
                    <li>
                        <span class="material-symbols-outlined">menu_book</span>
                        Read the questions below
                    </li>
                    <li>
                        <span class="material-symbols-outlined">forum</span>
                        Write to us if you still need help
                    </li>
                    <li>
                        <span class="material-symbols-outlined">play_circle</span>
                        Try a guest demo anytime
                    </li>
                </ul>
            </div>
        </div>
    </section>

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
            </section>
        </div>
    </section>

    <section class="lp-cta-wrap">
        <div class="lp-shell">
            <div class="lp-cta">
                <h2>Who is behind DTAS?</h2>
                <p>
                    We work from Maitidevi, Kathmandu. About us is who we are and why DTAS exists.
                </p>
                <div class="lp-hero-actions">
                    <a href="/About.aspx" class="lp-btn lp-btn-primary">
                        About us
                        <span class="material-symbols-outlined">arrow_forward</span>
                    </a>
                </div>
            </div>
        </div>
    </section>

</div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="/Assets/js/faq.js?v=3"></script>
</asp:Content>