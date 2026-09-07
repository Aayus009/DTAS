<%@ Page Title="Propose Event | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ProposeEvent.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Events.ProposeEvent" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
    <style>
        .create-section { margin-bottom: 0; }
        .create-section h3 { margin-bottom: 0.35rem; }
        .create-hint { font-size: 12px; color: rgb(var(--dt-on-surface-variant-rgb, 70 70 80) / 1); margin-bottom: 1rem; }
        .create-field label { display: block; font-weight: 600; font-size: 13px; margin-bottom: 0.4rem; }
        .create-input {
            width: 100%; padding: 0.75rem 1rem;
            border: 1px solid rgb(var(--dt-outline-rgb, 117 118 130) / 0.55);
            border-radius: 0.85rem; background: rgb(var(--dt-surface-container-low-rgb, 245 245 248) / 1);
            outline: none;
        }
        .create-input:focus { border-color: rgb(var(--dt-primary-rgb, 0 17 66) / 1); box-shadow: 0 0 0 1px rgb(var(--dt-primary-rgb, 0 17 66) / 0.2); }
        .internal-note {
            display: flex; align-items: flex-start; gap: 0.5rem;
            padding: 0.65rem 0.8rem; margin-bottom: 0.85rem;
            border-radius: 0.75rem;
            background: rgb(var(--dt-secondary-container-rgb, 212 227 255) / 0.35);
            font-size: 12px;
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <uc:UserTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:UserSidebar ActivePage="ProposeEvent" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            <a href="<%= ResolveUrl("~/Modules/Events/MyEvents.aspx") %>" class="inline-flex items-center gap-1 text-primary font-bold text-sm mb-4">
                <span class="material-symbols-outlined text-[20px]">arrow_back</span>
                Back to My Events
            </a>

            <header class="mb-6">
                <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Propose an event</h1>
                <p class="text-on-surface-variant max-w-2xl">Admin reviews this before it becomes a live event. Name a suggested lead and keep budget internal.</p>
            </header>

            <asp:Panel ID="pnlMessage" runat="server" CssClass="mb-6 p-4 rounded-xl flex items-center gap-3" Visible="false">
                <asp:Label ID="lblMessage" runat="server" CssClass="font-label-md text-label-md"></asp:Label>
            </asp:Panel>

            <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
                <div class="lg:col-span-2 space-y-5">
                    <section class="standard-card rounded-xl p-6 create-section">
                        <h3 class="font-title-lg text-title-lg text-primary">Basics</h3>
                        <p class="create-hint">Tell admin what this is and why it should run.</p>
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
                            <div class="create-field">
                                <label>Event name <span class="text-error">*</span></label>
                                <asp:TextBox ID="txtEventName" runat="server" CssClass="create-input" placeholder="Annual cultural fest 2026" MaxLength="200"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtEventName" ErrorMessage="Event name is required." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="ProposeEvent"></asp:RequiredFieldValidator>
                            </div>
                            <div class="create-field">
                                <label>Type</label>
                                <asp:DropDownList ID="ddlEventType" runat="server" CssClass="create-input appearance-none">
                                    <asp:ListItem Text="General" Value="General" Selected="True" />
                                    <asp:ListItem Text="Academic" Value="Academic" />
                                    <asp:ListItem Text="Sports" Value="Sports" />
                                    <asp:ListItem Text="Cultural" Value="Cultural" />
                                    <asp:ListItem Text="Technical" Value="Technical" />
                                    <asp:ListItem Text="Project" Value="Project" />
                                    <asp:ListItem Text="Custom" Value="Custom" />
                                </asp:DropDownList>
                            </div>
                        </div>
                        <div class="create-field mt-5">
                            <label>Why this event <span class="text-error">*</span></label>
                            <asp:TextBox ID="txtDescription" runat="server" CssClass="create-input resize-none" TextMode="MultiLine" Rows="4"
                                placeholder="What value it brings, and what the community should see if it is approved"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvDesc" runat="server" ControlToValidate="txtDescription" ErrorMessage="A description is required." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="ProposeEvent"></asp:RequiredFieldValidator>
                        </div>
                    </section>

                    <section class="standard-card rounded-xl p-6 create-section">
                        <h3 class="font-title-lg text-title-lg text-primary">When and where</h3>
                        <p class="create-hint">Tentative is fine. Admin can adjust after approval.</p>
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
                            <div class="create-field">
                                <label>Tentative start</label>
                                <asp:TextBox ID="txtStartDate" runat="server" CssClass="create-input" TextMode="Date"></asp:TextBox>
                            </div>
                            <div class="create-field">
                                <label>Tentative end</label>
                                <asp:TextBox ID="txtEndDate" runat="server" CssClass="create-input" TextMode="Date"></asp:TextBox>
                            </div>
                        </div>
                        <div class="create-field mt-5">
                            <label>Suggested venue</label>
                            <asp:TextBox ID="txtVenue" runat="server" CssClass="create-input" placeholder="Main auditorium" MaxLength="200"></asp:TextBox>
                        </div>
                    </section>

                    <section class="standard-card rounded-xl p-6 create-section">
                        <h3 class="font-title-lg text-title-lg text-primary">Who should lead</h3>
                        <p class="create-hint">Pick a verified faculty or staff member. Admin can change this if the proposal is approved.</p>
                        <div class="create-field">
                            <label>Suggested event lead <span class="text-error">*</span></label>
                            <asp:DropDownList ID="ddlEventLead" runat="server" CssClass="create-input appearance-none"></asp:DropDownList>
                            <asp:RequiredFieldValidator ID="rfvEventLead" runat="server" ControlToValidate="ddlEventLead" InitialValue="0" ErrorMessage="Choose a suggested event lead." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="ProposeEvent"></asp:RequiredFieldValidator>
                        </div>
                    </section>

                    <section class="standard-card rounded-xl p-6 create-section">
                        <h3 class="font-title-lg text-title-lg text-primary">Visibility</h3>
                        <p class="create-hint">This is your request. Admin sets the final visibility on approval.</p>
                        <div class="create-field">
                            <label>Requested visibility</label>
                            <asp:DropDownList ID="ddlVisibility" runat="server" CssClass="create-input appearance-none">
                                <asp:ListItem Text="Private - invite or code required" Value="Private" Selected="True" />
                                <asp:ListItem Text="Public - discoverable to verified members" Value="Public" />
                            </asp:DropDownList>
                        </div>
                    </section>

                    <section class="standard-card rounded-xl p-6 create-section">
                        <h3 class="font-title-lg text-title-lg text-primary">Club roster (optional)</h3>
                        <p class="create-hint">If you link a club you belong to, those members are added to the event after admin approves it. A Connect group is created then too, and club members are invited directly.</p>
                        <div class="create-field mb-5">
                            <label>Your clubs</label>
                            <asp:DropDownList ID="ddlClub" runat="server" CssClass="create-input appearance-none"></asp:DropDownList>
                        </div>
                        <div class="create-field">
                            <label>Or club ID / invitation code</label>
                            <asp:TextBox ID="txtClubId" runat="server" CssClass="create-input" placeholder="12 or DTAS-CLB-8X29K" MaxLength="50"></asp:TextBox>
                        </div>
                    </section>

                    <section class="standard-card rounded-xl p-6 create-section">
                        <h3 class="font-title-lg text-title-lg text-primary">Internal</h3>
                        <div class="internal-note">
                            <span class="material-symbols-outlined text-[18px] text-primary">lock</span>
                            <span>Budget stays inside DTAS. It is not shown on the public transparency page.</span>
                        </div>
                        <div class="create-field">
                            <label>Suggested budget (optional)</label>
                            <asp:TextBox ID="txtBudget" runat="server" CssClass="create-input" placeholder="5000.00" MaxLength="20"></asp:TextBox>
                        </div>
                    </section>

                    <div class="pb-4">
                        <asp:Button ID="btnSubmit" runat="server" Text="Submit proposal" CssClass="btn-primary" ValidationGroup="ProposeEvent" OnClick="btnSubmit_Click" />
                    </div>
                </div>

                <div class="space-y-6">
                    <section class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-4">What happens next</h3>
                        <ol class="space-y-3 text-sm text-on-surface-variant">
                            <li>1. Admin gets the proposal in the review queue.</li>
                            <li>2. They approve it or reject it with a reason.</li>
                            <li>3. You get a notification either way.</li>
                            <li>4. If you linked a club, members are added and invited to Connect after approval.</li>
                        </ol>
                    </section>

                    <section class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-4">My proposals</h3>
                        <asp:Repeater ID="rptMyProposals" runat="server">
                            <ItemTemplate>
                                <div class="p-4 bg-surface-container-low rounded-lg mb-4 last:mb-0">
                                    <div class="flex justify-between items-start mb-1 gap-2">
                                        <span class="font-label-md font-bold text-on-surface"><%# Eval("EventName") %></span>
                                        <span class='<%# "badge-status-" + Eval("Status").ToString().ToLower().Replace(" ", "") %>'><%# Eval("Status") %></span>
                                    </div>
                                    <p class="text-xs text-outline mb-2">Proposed on <%# Eval("CreatedAt", "{0:MMM dd, yyyy}") %></p>
                                    <%# Eval("RejectionReason") != DBNull.Value && !string.IsNullOrEmpty(Eval("RejectionReason").ToString()) ?
                                        "<div class='mt-2 p-3 bg-error-container/10 rounded-lg'><p class='text-sm font-bold text-error mb-1'>Rejection reason</p><p class='text-sm text-on-surface-variant'>" + Eval("RejectionReason") + "</p></div>" : "" %>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:Panel ID="pnlNoProposals" runat="server" Visible="false" CssClass="text-center py-8">
                            <span class="material-symbols-outlined text-5xl text-outline-variant mb-2 block">event_busy</span>
                            <p class="text-on-surface-variant">No proposals yet</p>
                        </asp:Panel>
                    </section>
                </div>
            </div>
        </main>
    </div>
</asp:Content>
