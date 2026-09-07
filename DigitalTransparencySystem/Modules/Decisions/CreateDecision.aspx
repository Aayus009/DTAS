<%@ Page Title="Create Decision | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="CreateDecision.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Decisions.CreateDecision" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <uc:AdminTopbar runat="server" />

    <div class="flex min-h-screen">

        <uc:AdminSidebar ActivePage="Decisions" runat="server" />

        <!-- Main Content Area -->
        <main class="dashboard-main flex-1 ml-64 p-8">

            <!-- Breadcrumb -->
            <nav class="flex items-center gap-2 mb-6 font-label-md text-label-md text-on-surface-variant">
                <a href="<%= ResolveUrl("~/Modules/Dashboard/AdminDashboard.aspx") %>" class="hover:text-primary transition-colors">Dashboard</a>
                <span class="material-symbols-outlined text-[16px]">chevron_right</span>
                <a href="<%= ResolveUrl("~/Modules/Decisions/Decisions.aspx") %>" class="hover:text-primary transition-colors">Decisions</a>
                <span class="material-symbols-outlined text-[16px]">chevron_right</span>
                <span class="text-primary font-semibold">Create Decision</span>
            </nav>

            <!-- Header Section -->
            <header class="flex justify-between items-end mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Create New Decision</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">Record a new governance decision for tracking and accountability.</p>
                </div>
                <div class="flex gap-3">
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="px-6 py-2.5 border border-outline text-on-surface-variant rounded-lg font-label-md text-label-md font-bold hover:bg-surface-container-low transition-colors cursor-pointer" OnClick="btnCancel_Click" CausesValidation="false" />
                </div>
            </header>

            <!-- Error Message -->
            <asp:Panel ID="pnlError" runat="server" CssClass="mb-6 p-4 bg-error-container border border-error/30 rounded-xl flex items-center gap-3" Visible="false">
                <span class="material-symbols-outlined text-error">error</span>
                <asp:Label ID="lblError" runat="server" CssClass="font-label-md text-label-md text-on-error-container"></asp:Label>
            </asp:Panel>

            <!-- Form Card -->
            <div class="glass-card rounded-xl p-8">
                <div class="space-y-6">

                    <!-- Decision Title -->
                    <div>
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Decision Title <span class="text-error">*</span></label>
                        <asp:TextBox ID="txtDecisionTitle" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="Enter decision title" MaxLength="200"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvDecisionTitle" runat="server" ControlToValidate="txtDecisionTitle" ErrorMessage="Decision title is required." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="CreateDecision"></asp:RequiredFieldValidator>
                    </div>

                    <!-- Description -->
                    <div>
                        <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Description <span class="text-error">*</span></label>
                        <asp:TextBox ID="txtDescription" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="Enter decision description" TextMode="MultiLine" Rows="4"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvDescription" runat="server" ControlToValidate="txtDescription" ErrorMessage="Description is required." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="CreateDecision"></asp:RequiredFieldValidator>
                    </div>

                    <!-- Row: Priority + Responsible Person -->
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div>
                            <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Priority <span class="text-error">*</span></label>
                            <asp:DropDownList ID="ddlPriority" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none" ValidationGroup="CreateDecision">
                                <asp:ListItem Text="Select Priority" Value="" />
                                <asp:ListItem Text="Low" Value="Low" />
                                <asp:ListItem Text="Medium" Value="Medium" />
                                <asp:ListItem Text="High" Value="High" />
                                <asp:ListItem Text="Critical" Value="Critical" />
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="rfvPriority" runat="server" ControlToValidate="ddlPriority" InitialValue="" ErrorMessage="Priority is required." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="CreateDecision"></asp:RequiredFieldValidator>
                        </div>
                        <div>
                            <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Responsible Person <span class="text-error">*</span></label>
                            <asp:DropDownList ID="ddlResponsiblePerson" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none">
                                <asp:ListItem Text="Select Person" Value="" />
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="rfvResponsiblePerson" runat="server" ControlToValidate="ddlResponsiblePerson" InitialValue="" ErrorMessage="Responsible person is required." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="CreateDecision"></asp:RequiredFieldValidator>
                        </div>
                    </div>

                    <!-- Row: Related Meeting + Related Event -->
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div>
                            <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Related Meeting (Optional)</label>
                            <asp:DropDownList ID="ddlRelatedMeeting" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none">
                                <asp:ListItem Text="None" Value="" />
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Related Event (Optional)</label>
                            <asp:DropDownList ID="ddlRelatedEvent" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none">
                                <asp:ListItem Text="None" Value="" />
                            </asp:DropDownList>
                        </div>
                    </div>

                    <!-- Due Date -->
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div>
                            <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Due Date (Optional)</label>
                            <asp:TextBox ID="txtDueDate" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" TextMode="DateTimeLocal"></asp:TextBox>
                        </div>
                        <div></div>
                    </div>

                    <!-- Action Buttons -->
                    <div class="flex items-center gap-4 pt-4 border-t border-outline-variant/30">
                        <asp:Button ID="btnCreate" runat="server" Text="Create Decision" CssClass="px-8 py-3 bg-primary text-on-primary rounded-xl font-label-md text-label-md font-bold active:scale-95 transition-transform cursor-pointer" ValidationGroup="CreateDecision" OnClick="btnCreate_Click" />
                        <asp:Button ID="btnCancelBottom" runat="server" Text="Cancel" CssClass="px-8 py-3 border border-outline text-on-surface-variant rounded-xl font-label-md text-label-md font-bold hover:bg-surface-container-low transition-colors cursor-pointer" OnClick="btnCancel_Click" CausesValidation="false" />
                    </div>
                </div>
            </div>

        </main>
    </div>

    <!-- Dashboard Footer -->
    <footer class="dashboard-footer bg-on-secondary-fixed text-on-primary py-20 px-8 ml-64">
        <div class="grid grid-cols-1 md:grid-cols-4 gap-12 max-w-7xl mx-auto">
            <div class="md:col-span-1">
                <h2 class="font-headline-md text-headline-md font-bold mb-4">DTAS</h2>
                <p class="font-body-md text-surface-container-high/60">The authoritative platform for educational accountability and data transparency.</p>
            </div>
            <div>
                <h4 class="font-title-lg text-title-lg mb-4 text-on-primary-fixed-variant">Navigation</h4>
                <ul class="space-y-2 font-body-md text-surface-container-high/80">
                    <li><a class="hover:text-white transition-colors" href="#">Institutional Records</a></li>
                    <li><a class="hover:text-white transition-colors" href="#">Governance Portal</a></li>
                    <li><a class="hover:text-white transition-colors" href="#">Public Dashboard</a></li>
                    <li><a class="hover:text-white transition-colors" href="#">Ethics Guidelines</a></li>
                </ul>
            </div>
            <div>
                <h4 class="font-title-lg text-title-lg mb-4 text-on-primary-fixed-variant">Support</h4>
                <ul class="space-y-2 font-body-md text-surface-container-high/80">
                    <li><a class="hover:text-white transition-colors" href="#">Help Center</a></li>
                    <li><a class="hover:text-white transition-colors" href="#">API Documentation</a></li>
                    <li><a class="hover:text-white transition-colors" href="#">Status Page</a></li>
                    <li><a class="hover:text-white transition-colors" href="#">Submit Request</a></li>
                </ul>
            </div>
            <div>
                <h4 class="font-title-lg text-title-lg mb-4 text-on-primary-fixed-variant">Trust</h4>
                <div class="flex flex-wrap gap-4">
                    <div class="h-12 w-12 bg-white/10 rounded flex items-center justify-center">
                        <span class="material-symbols-outlined text-white">verified_user</span>
                    </div>
                    <div class="h-12 w-12 bg-white/10 rounded flex items-center justify-center">
                        <span class="material-symbols-outlined text-white">gpp_maybe</span>
                    </div>
                    <div class="h-12 w-12 bg-white/10 rounded flex items-center justify-center">
                        <span class="material-symbols-outlined text-white">policy</span>
                    </div>
                </div>
            </div>
        </div>
        <div class="border-t border-white/10 mt-16 pt-8 text-center text-surface-container-high/40 font-label-md text-label-md">
            &copy; 2023 DTAS. All rights reserved. Encrypted and audited.
        </div>
    </footer>

</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="<%= ResolveUrl("~/Assets/js/dashboard.js") %>"></script>
</asp:Content>
