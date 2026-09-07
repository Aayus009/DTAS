<%@ Page Title="Contact - DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="Contact.aspx.cs" Inherits="DigitalTransparencySystem.Contact" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Contact - DTAS
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="/Assets/css/contact.css" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Hero Section -->
    <section class="relative overflow-hidden pt-12 pb-16 md:pt-16 md:pb-20 bg-surface-container-low border-b border-border-light">
        <div class="max-w-7xl mx-auto px-container-padding-mobile md:px-container-padding-desktop relative z-10">
            <div class="max-w-3xl">
                <span class="inline-block bg-surface-container-lowest text-slate-deep px-4 py-1.5 rounded-full border border-border-light font-badge-cap text-badge-cap mb-4">INSTITUTIONAL SUPPORT</span>
                <h1 class="font-display-lg text-display-lg text-slate-deep mb-4 leading-tight">Inquiry & Accountability</h1>
                <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl leading-relaxed">
                    Access formal communication protocols for auditing, research inquiries, and transparency reporting.
                </p>
            </div>
        </div>
    </section>

    <!-- Main Content Area -->
    <section class="py-12 max-w-7xl mx-auto px-container-padding-mobile md:px-container-padding-desktop">
        <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
            <div class="lg:col-span-7">
                <div class="bg-surface-container-lowest border border-border-light rounded-2xl p-6 md:p-8 shadow-sm hover:shadow-md transition-shadow">
                    <div class="flex items-center gap-3 mb-6">
                        <span class="material-symbols-outlined text-pacific-blue text-2xl">receipt_long</span>
                        <h2 class="font-headline-md text-headline-md text-slate-deep">Direct Inquiry Registry</h2>
                    </div>
                    <div class="space-y-5">
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
                            <div class="space-y-1.5">
                                <label class="block font-label-md text-label-md text-on-surface-variant" for="full_name">Full Name</label>
                                <asp:TextBox ID="txtFullName" runat="server" CssClass="w-full bg-surface-container-low border border-border-light rounded-lg px-4 py-2.5 text-on-surface focus:ring-0 transition-all form-input text-[14px]" placeholder="Dr. Julian Vane"></asp:TextBox>
                            </div>
                            <div class="space-y-1.5">
                                <label class="block font-label-md text-label-md text-on-surface-variant" for="email">Institutional Email</label>
                                <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" CssClass="w-full bg-surface-container-low border border-border-light rounded-lg px-4 py-2.5 text-on-surface focus:ring-0 transition-all form-input text-[14px]" placeholder="vane@edu-inst.org"></asp:TextBox>
                            </div>
                        </div>
                        <div class="space-y-1.5">
                            <label class="block font-label-md text-label-md text-on-surface-variant" for="category">Category</label>
                            <asp:DropDownList ID="ddlCategory" runat="server" CssClass="w-full bg-surface-container-low border border-border-light rounded-lg px-4 py-2.5 text-on-surface focus:ring-0 transition-all form-input text-[14px]">
                                <asp:ListItem Text="Select inquiry type" Value=""></asp:ListItem>
                                <asp:ListItem Text="Data Integrity Audit" Value="audit"></asp:ListItem>
                                <asp:ListItem Text="Academic Research Grant" Value="research"></asp:ListItem>
                                <asp:ListItem Text="Ethics Committee Inquiry" Value="ethics"></asp:ListItem>
                                <asp:ListItem Text="Technical Infrastructure Support" Value="support"></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="space-y-1.5">
                            <label class="block font-label-md text-label-md text-on-surface-variant" for="statement">Formal Statement</label>
                            <asp:TextBox ID="txtStatement" runat="server" TextMode="MultiLine" Rows="4" CssClass="w-full bg-surface-container-low border border-border-light rounded-lg px-4 py-2.5 text-on-surface focus:ring-0 transition-all form-input resize-none text-[14px]" placeholder="Provide a detailed description..."></asp:TextBox>
                        </div>
                        <div class="flex items-start gap-3">
                            <asp:CheckBox ID="chkConsent" runat="server" CssClass="mt-1 rounded border-border-light" />
                            <label class="text-[13px] text-on-surface-variant" for="chkConsent">
                                I consent to recording this communication for audit and compliance purposes.
                            </label>
                        </div>
                        <asp:Button ID="btnSubmit" runat="server" Text="Submit Inquiry" OnClick="btnSubmit_Click" CssClass="w-full md:w-auto bg-pacific-blue hover:bg-slate-deep text-on-primary px-8 py-3 rounded-lg font-title-lg text-title-lg flex items-center justify-center gap-2 transition-colors group" />
                    </div>
                </div>
            </div>

            <div class="lg:col-span-5 space-y-5">
                <div class="bg-surface-container-lowest border border-border-light rounded-2xl p-6 shadow-sm group hover:-translate-y-1 transition-all">
                    <h3 class="font-title-lg text-title-lg text-slate-deep mb-5">Communication Channels</h3>
                    <div class="space-y-5">
                        <div class="flex gap-4">
                            <div class="w-10 h-10 rounded-lg bg-pacific-blue flex items-center justify-center text-white">
                                <span class="material-symbols-outlined text-xl">mail</span>
                            </div>
                            <div>
                                <p class="font-label-md text-label-md text-on-surface-variant text-[12px]">General Registry</p>
                                <p class="font-body-md text-slate-deep font-bold text-[14px]">registry@dtas.org</p>
                            </div>
                        </div>
                        <div class="flex gap-4">
                            <div class="w-10 h-10 rounded-lg bg-slate-deep flex items-center justify-center text-white">
                                <span class="material-symbols-outlined text-xl">security</span>
                            </div>
                            <div>
                                <p class="font-label-md text-label-md text-on-surface-variant text-[12px]">Ethics Officer</p>
                                <p class="font-body-md text-slate-deep font-bold text-[14px]">compliance@dtas.org</p>
                            </div>
                        </div>
                        <div class="flex gap-4">
                            <div class="w-10 h-10 rounded-lg bg-gradient-to-br from-pacific-blue to-slate-deep flex items-center justify-center text-white">
                                <span class="material-symbols-outlined text-xl">phone_in_talk</span>
                            </div>
                            <div>
                                <p class="font-label-md text-label-md text-on-surface-variant text-[12px]">Institutional Liaison</p>
                                <p class="font-body-md text-slate-deep font-bold text-[14px]">+1 (800) 555-0192</p>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="bg-surface-container-lowest border border-border-light rounded-2xl overflow-hidden shadow-sm">
                    <div class="h-36 bg-surface-container-highest relative">
                        <div class="absolute inset-0 bg-cover bg-center" style="background-image: url('/Assets/images/headquarters.jpg')"></div>
                        <div class="absolute inset-0 bg-gradient-to-br from-slate-deep/20 to-pacific-blue/20"></div>
                    </div>
                    <div class="p-6">
                        <h3 class="font-title-lg text-title-lg text-slate-deep mb-3">Headquarters</h3>
                        <p class="font-body-md text-on-surface-variant text-[14px] leading-relaxed">
                            Transparency Plaza, Suite 400<br/>
                            1200 Innovation Drive<br/>
                            Cambridge, MA 02138, USA
                        </p>
                    </div>
                </div>

                <div class="bg-surface-container-low rounded-2xl p-6 border-l-4 border-l-pacific-blue shadow-sm">
                    <div class="flex items-center justify-between mb-5">
                        <div class="flex items-center gap-3">
                            <span class="relative flex h-2.5 w-2.5">
                                <span class="status-pulse absolute inline-flex h-full w-full rounded-full bg-pacific-blue opacity-75"></span>
                                <span class="relative inline-flex rounded-full h-2.5 w-2.5 bg-pacific-blue"></span>
                            </span>
                            <span class="font-label-md text-label-md text-slate-deep uppercase tracking-widest text-[11px]">System Status: Verified</span>
                        </div>
                        <span class="material-symbols-outlined text-pacific-blue">verified</span>
                    </div>
                    <div class="grid grid-cols-3 gap-3 pt-3 border-t border-border-light">
                        <div>
                            <p class="font-badge-cap text-badge-cap text-on-surface-variant text-[10px]">LATENCY</p>
                            <p class="font-title-lg text-title-lg text-slate-deep">12ms</p>
                        </div>
                        <div>
                            <p class="font-badge-cap text-badge-cap text-on-surface-variant text-[10px]">UPTIME</p>
                            <p class="font-title-lg text-title-lg text-pacific-blue">99.9%</p>
                        </div>
                        <div>
                            <p class="font-badge-cap text-badge-cap text-on-surface-variant text-[10px]">NODES</p>
                            <p class="font-title-lg text-title-lg text-slate-deep">1,402</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <!-- Newsletter Section -->
    <section class="py-8 bg-gradient-to-br from-slate-deep to-pacific-blue rounded-t-[48px] mx-4 md:mx-8">
        <div class="max-w-7xl mx-auto px-container-padding-mobile md:px-container-padding-desktop">
            <div class="flex flex-col md:flex-row items-center justify-between gap-5">
                <div>
                    <h2 class="font-headline-md text-headline-md text-white mb-1">Join the Transparency Registry</h2>
                    <p class="font-body-md text-white/80 text-[14px]">Receive monthly reports and system updates.</p>
                </div>
                <div class="w-full md:w-auto flex gap-2">
                    <asp:TextBox ID="txtNewsletter" runat="server" TextMode="Email" CssClass="flex-grow bg-white/15 backdrop-blur-md border border-white/30 rounded-lg px-5 py-2.5 focus:ring-0 transition-all text-white placeholder-white/60 text-[14px]" placeholder="Email Address"></asp:TextBox>
                    <asp:Button ID="btnSignUp" runat="server" Text="Sign Up" OnClick="btnSignUp_Click" CssClass="bg-white text-slate-deep px-6 py-2.5 rounded-lg font-title-lg whitespace-nowrap hover:bg-surface-container-low transition-colors font-bold" />
                </div>
            </div>
        </div>
    </section>

</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="/Assets/js/contact.js"></script>
</asp:Content>