<%@ Page Title="Contact - DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="Contact.aspx.cs" Inherits="DigitalTransparencySystem.Contact" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Contact - DTAS
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="/Assets/css/contact.css?v=3" />
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
                    Contact
                </span>
                <h1>Ask us about DTAS</h1>
                <p>
                    Use this page if you have a question, need help getting started, or want to reach the DTAS team.
                </p>
            </div>
        </div>
    </section>

    <section class="py-12 max-w-7xl mx-auto px-container-padding-mobile md:px-container-padding-desktop">
        <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
            <div class="lg:col-span-7">
                <div class="bg-surface-container-lowest border border-border-light rounded-2xl p-6 md:p-8 shadow-sm">
                    <div class="flex items-center gap-3 mb-6">
                        <span class="material-symbols-outlined text-pacific-blue text-2xl">mail</span>
                        <h2 class="font-headline-md text-headline-md text-slate-deep">System inquiry</h2>
                    </div>
                    <asp:Label ID="lblNotice" runat="server" CssClass="dtas-notice"></asp:Label>
                    <div class="space-y-5">
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
                            <div class="space-y-1.5">
                                <label class="block font-label-md text-label-md text-on-surface-variant" for="<%= txtFullName.ClientID %>">Full name</label>
                                <asp:TextBox ID="txtFullName" runat="server" CssClass="w-full bg-surface-container-low border border-border-light rounded-lg px-4 py-2.5 text-on-surface focus:ring-0 transition-all form-input text-[14px]" placeholder="Your name"></asp:TextBox>
                            </div>
                            <div class="space-y-1.5">
                                <label class="block font-label-md text-label-md text-on-surface-variant" for="<%= txtEmail.ClientID %>">Email</label>
                                <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" CssClass="w-full bg-surface-container-low border border-border-light rounded-lg px-4 py-2.5 text-on-surface focus:ring-0 transition-all form-input text-[14px]" placeholder="you@college.edu.np"></asp:TextBox>
                            </div>
                        </div>
                        <div class="space-y-1.5">
                            <label class="block font-label-md text-label-md text-on-surface-variant" for="<%= ddlCategory.ClientID %>">How can we help?</label>
                            <asp:DropDownList ID="ddlCategory" runat="server" CssClass="w-full bg-surface-container-low border border-border-light rounded-lg px-4 py-2.5 text-on-surface focus:ring-0 transition-all form-input text-[14px]">
                                <asp:ListItem Text="Select an option" Value=""></asp:ListItem>
                                <asp:ListItem Text="I want to know what DTAS is" Value="about"></asp:ListItem>
                                <asp:ListItem Text="I need help getting started" Value="start"></asp:ListItem>
                                <asp:ListItem Text="Something is not working" Value="problem"></asp:ListItem>
                                <asp:ListItem Text="I have a suggestion" Value="suggestion"></asp:ListItem>
                                <asp:ListItem Text="Something else" Value="other"></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="space-y-1.5">
                            <label class="block font-label-md text-label-md text-on-surface-variant" for="<%= txtStatement.ClientID %>">Your question</label>
                            <asp:TextBox ID="txtStatement" runat="server" TextMode="MultiLine" Rows="4" CssClass="w-full bg-surface-container-low border border-border-light rounded-lg px-4 py-2.5 text-on-surface focus:ring-0 transition-all form-input resize-none text-[14px]" placeholder="Write your question in your own words."></asp:TextBox>
                        </div>
                        <div class="flex items-start gap-3">
                            <asp:CheckBox ID="chkConsent" runat="server" CssClass="mt-1 rounded border-border-light" />
                            <label class="text-[13px] text-on-surface-variant" for="<%= chkConsent.ClientID %>">
                                DTAS may use this message to reply to my inquiry.
                            </label>
                        </div>
                        <asp:Button ID="btnSubmit" runat="server" Text="Send inquiry" OnClick="btnSubmit_Click" CssClass="w-full md:w-auto bg-pacific-blue hover:bg-slate-deep text-on-primary px-8 py-3 rounded-lg font-title-lg text-title-lg flex items-center justify-center gap-2 transition-colors group" />
                    </div>
                </div>
            </div>

            <div class="lg:col-span-5 space-y-5">
                <div class="bg-surface-container-lowest border border-border-light rounded-2xl p-6 shadow-sm">
                    <h3 class="font-title-lg text-title-lg text-slate-deep mb-5">Communication channels</h3>
                    <div class="space-y-5">
                        <div class="flex gap-4">
                            <div class="w-10 h-10 rounded-lg bg-pacific-blue flex items-center justify-center text-white">
                                <span class="material-symbols-outlined text-xl">mail</span>
                            </div>
                            <div>
                                <p class="font-label-md text-label-md text-on-surface-variant text-[12px]">Email</p>
                                <a class="font-body-md text-slate-deep font-bold text-[14px]" href="mailto:edu.transparency@gmail.com">edu.transparency@gmail.com</a>
                            </div>
                        </div>
                        <div class="flex gap-4">
                            <div class="w-10 h-10 rounded-lg bg-slate-deep flex items-center justify-center text-white">
                                <span class="material-symbols-outlined text-xl">phone_in_talk</span>
                            </div>
                            <div>
                                <p class="font-label-md text-label-md text-on-surface-variant text-[12px]">Phone</p>
                                <a class="font-body-md text-slate-deep font-bold text-[14px]" href="tel:+9779814524825">9814524825</a>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="bg-surface-container-lowest border border-border-light rounded-2xl p-6 shadow-sm">
                    <div class="flex gap-4">
                        <div class="w-10 h-10 rounded-lg bg-pacific-blue flex items-center justify-center text-white">
                            <span class="material-symbols-outlined text-xl">location_on</span>
                        </div>
                        <div>
                            <h3 class="font-title-lg text-title-lg text-slate-deep mb-2">Location</h3>
                            <p class="font-body-md text-on-surface-variant text-[14px] leading-relaxed">
                                Maitidevi<br />
                                Kathmandu, Nepal
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <section class="lp-cta-wrap">
        <div class="lp-shell">
            <div class="lp-cta">
                <h2>Prefer to write or call?</h2>
                <p>Reach DTAS at edu.transparency@gmail.com or 9814524825.</p>
                <div class="lp-hero-actions">
                    <a href="mailto:edu.transparency@gmail.com" class="lp-btn lp-btn-primary">Email us</a>
                    <a href="tel:+9779814524825" class="lp-btn lp-btn-ghost">Call 9814524825</a>
                </div>
            </div>
        </div>
    </section>

</div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="/Assets/js/contact.js?v=2"></script>
</asp:Content>
