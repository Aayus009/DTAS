<%@ Page Title="Create Event | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="CreateEvent.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Events.CreateEvent" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
    <style>
        .create-section { margin-bottom: 1.25rem; }
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
    <uc:AdminTopbar ID="adminTop" runat="server" Visible="false" />
    <uc:UserTopbar ID="userTop" runat="server" />

    <div class="flex min-h-screen">
        <uc:AdminSidebar ID="adminSide" ActivePage="Events" runat="server" Visible="false" />
        <uc:UserSidebar ID="userSide" ActivePage="CreateEvent" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            <a href="<%= ResolveUrl("~/Modules/Events/MyEvents.aspx") %>" class="inline-flex items-center gap-1 text-primary font-bold text-sm mb-4">
                <span class="material-symbols-outlined text-[20px]">arrow_back</span>
                Back to My Events
            </a>

            <header class="mb-6">
                <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Create event</h1>
                <p class="text-on-surface-variant max-w-2xl">Name a lead, set the public gist, then open the workspace to add meetings, decisions, and tasks.</p>
            </header>

            <asp:Panel ID="pnlError" runat="server" CssClass="mb-6 p-4 bg-error-container border border-error/30 rounded-xl flex items-center gap-3" Visible="false">
                <span class="material-symbols-outlined text-error">error</span>
                <asp:Label ID="lblError" runat="server" CssClass="font-label-md text-on-error-container"></asp:Label>
            </asp:Panel>

            <div class="max-w-4xl space-y-5">
                <section class="standard-card rounded-xl p-6 create-section">
                    <h3 class="font-title-lg text-title-lg text-primary">Basics</h3>
                    <p class="create-hint">This is what people see first. Keep the description short if the event is public.</p>
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
                        <div class="create-field">
                            <label>Event name <span class="text-error">*</span></label>
                            <asp:TextBox ID="txtEventName" runat="server" CssClass="create-input" placeholder="Annual orientation" MaxLength="200"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvEventName" runat="server" ControlToValidate="txtEventName" ErrorMessage="Event name is required." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="CreateEvent"></asp:RequiredFieldValidator>
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
                        <label>Public description</label>
                        <asp:TextBox ID="txtDescription" runat="server" CssClass="create-input resize-none" placeholder="What the community may see if this event is public" TextMode="MultiLine" Rows="3"></asp:TextBox>
                    </div>
                </section>

                <section class="standard-card rounded-xl p-6 create-section">
                    <h3 class="font-title-lg text-title-lg text-primary">When and where</h3>
                    <p class="create-hint">Use the real venue. Online events can use a meeting link or "Online".</p>
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
                        <div class="create-field">
                            <label>Start <span class="text-error">*</span></label>
                            <asp:TextBox ID="txtStartDate" runat="server" CssClass="create-input" TextMode="DateTimeLocal"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvStartDate" runat="server" ControlToValidate="txtStartDate" ErrorMessage="Start date is required." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="CreateEvent"></asp:RequiredFieldValidator>
                        </div>
                        <div class="create-field">
                            <label>End <span class="text-error">*</span></label>
                            <asp:TextBox ID="txtEndDate" runat="server" CssClass="create-input" TextMode="DateTimeLocal"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvEndDate" runat="server" ControlToValidate="txtEndDate" ErrorMessage="End date is required." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="CreateEvent"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="create-field mt-5">
                        <label>Venue <span class="text-error">*</span></label>
                        <asp:TextBox ID="txtVenue" runat="server" CssClass="create-input" placeholder="Main auditorium" MaxLength="200"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvVenue" runat="server" ControlToValidate="txtVenue" ErrorMessage="Venue is required." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="CreateEvent"></asp:RequiredFieldValidator>
                    </div>
                </section>

                <section class="standard-card rounded-xl p-6 create-section">
                    <h3 class="font-title-lg text-title-lg text-primary">Who is responsible</h3>
                    <p class="create-hint">Pick a verified faculty or staff member as lead. Students who create an event stay on it as event manager.</p>
                    <div class="create-field">
                        <label>Event lead <span class="text-error">*</span></label>
                        <asp:DropDownList ID="ddlEventLead" runat="server" CssClass="create-input appearance-none"></asp:DropDownList>
                        <asp:RequiredFieldValidator ID="rfvEventLead" runat="server" ControlToValidate="ddlEventLead" InitialValue="0" ErrorMessage="Choose an event lead." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="CreateEvent"></asp:RequiredFieldValidator>
                    </div>
                </section>

                <section class="standard-card rounded-xl p-6 create-section">
                    <h3 class="font-title-lg text-title-lg text-primary">Visibility and first invites</h3>
                    <p class="create-hint">Public events appear on Campus events with progress and who is leading. Anyone can request to join; the event lead or manager must accept them. Private events and their tasks stay visible only to members.</p>
                    <div class="create-field mb-5">
                        <label>Visibility</label>
                        <asp:DropDownList ID="ddlVisibility" runat="server" CssClass="create-input appearance-none">
                            <asp:ListItem Text="Private - invite or code required" Value="Private" Selected="True" />
                            <asp:ListItem Text="Public - discoverable to verified members" Value="Public" />
                        </asp:DropDownList>
                    </div>
                    <div class="create-field">
                        <label>First invites (optional)</label>
                        <asp:TextBox ID="txtInvites" runat="server" CssClass="create-input resize-none" TextMode="MultiLine" Rows="3"
                            placeholder="one email per line, or separated by commas"></asp:TextBox>
                        <p class="create-hint mt-2 mb-0">Invited people join as participants. Change roles in the workspace.</p>
                    </div>
                </section>

                <section class="standard-card rounded-xl p-6 create-section">
                    <h3 class="font-title-lg text-title-lg text-primary">Club roster (optional)</h3>
                    <p class="create-hint">Use a club you created or joined. Club members are added to this event. You can also open a Connect group and invite them there.</p>
                    <div class="create-field mb-5">
                        <label>Your clubs</label>
                        <asp:DropDownList ID="ddlClub" runat="server" CssClass="create-input appearance-none"></asp:DropDownList>
                    </div>
                    <div class="create-field mb-5">
                        <label>Or club ID / invitation code</label>
                        <asp:TextBox ID="txtClubId" runat="server" CssClass="create-input" placeholder="12 or DTAS-CLB-8X29K" MaxLength="50"></asp:TextBox>
                    </div>
                    <asp:CheckBox ID="chkCreateConnect" runat="server" Checked="true"
                        Text=" Create a Connect group and invite those club members"
                        CssClass="font-label-md" />
                </section>

                <section class="standard-card rounded-xl p-6 create-section">
                    <h3 class="font-title-lg text-title-lg text-primary">Internal</h3>
                    <div class="internal-note">
                        <span class="material-symbols-outlined text-[18px] text-primary">lock</span>
                        <span>Budget stays inside DTAS. It is not shown on the public transparency page.</span>
                    </div>
                    <div class="create-field">
                        <label>Budget (optional)</label>
                        <asp:TextBox ID="txtBudget" runat="server" CssClass="create-input" placeholder="5000.00" MaxLength="20"></asp:TextBox>
                    </div>
                </section>

                <div class="flex flex-wrap items-center gap-3 pt-1 pb-8">
                    <asp:Button ID="btnCreate" runat="server" Text="Create event" CssClass="btn-primary" ValidationGroup="CreateEvent" OnClick="btnCreate_Click" />
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="px-6 py-2.5 border border-outline rounded-xl font-label-md font-bold" OnClick="btnCancel_Click" CausesValidation="false" />
                    <asp:Button ID="btnCancelBottom" runat="server" Visible="false" />
                </div>
            </div>
        </main>
    </div>
</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript" src="<%= ResolveUrl("~/Assets/js/dashboard.js") %>"></script>
</asp:Content>
