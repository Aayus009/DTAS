<%@ Page Title="Assignments | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MyAssignments.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Assignments.MyAssignments" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <uc:UserTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:UserSidebar ActivePage="Assignments" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            <header class="flex flex-wrap justify-between items-end gap-4 mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Assignments</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">
                        Open a group you are working on, or create a personal assignment group. College groups use a faculty assignment code.
                    </p>
                </div>
                <div class="flex flex-wrap gap-2">
                    <asp:HyperLink ID="lnkFaculty" runat="server" Visible="false" NavigateUrl="~/Modules/Assignments/Assignments.aspx"
                        CssClass="py-2.5 px-5 border border-outline rounded-xl font-label-md font-bold">Faculty assignments</asp:HyperLink>
                    <asp:LinkButton ID="btnShowCreate" runat="server" Visible="false"
                        CssClass="inline-flex items-center gap-2 py-2.5 px-6 bg-primary text-on-primary rounded-xl font-label-md font-bold"
                        OnClick="btnShowCreate_Click" CausesValidation="false">
                        <span class="material-symbols-outlined text-[20px]">add</span>
                        Create Assignment Group
                    </asp:LinkButton>
                </div>
            </header>

            <asp:Label ID="lblMessage" runat="server" CssClass="font-label-md block mb-4"></asp:Label>

            <asp:Panel ID="pnlCreate" runat="server" Visible="false" CssClass="standard-card rounded-xl p-6 mb-6">
                <div class="flex justify-between items-start gap-4 mb-4">
                    <h3 class="font-title-lg text-title-lg text-primary">Create Assignment Group</h3>
                    <asp:Button ID="btnCancelCreate" runat="server" Text="Cancel" CausesValidation="false"
                        CssClass="px-4 py-2 rounded-lg font-label-md text-on-surface-variant hover:bg-surface-variant"
                        OnClick="btnCancelCreate_Click" />
                </div>
                <div class="flex gap-2 flex-wrap mb-5">
                    <asp:Button ID="btnTypePersonal" runat="server" Text="Personal Assignment Group" CausesValidation="false"
                        CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-primary text-on-primary"
                        OnClick="btnTypePersonal_Click" />
                    <asp:Button ID="btnTypeCollege" runat="server" Text="College Assignment Group" CausesValidation="false"
                        CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors"
                        OnClick="btnTypeCollege_Click" />
                </div>

                <asp:Panel ID="pnlPersonalFields" runat="server" CssClass="space-y-4 max-w-2xl">
                    <p class="text-sm text-on-surface-variant">Students manage this group themselves. Invite classmates, assign tasks, update status, and attach work. Faculty does not oversee it.</p>
                    <asp:TextBox ID="txtPersonalName" runat="server" MaxLength="200"
                        CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md"
                        placeholder="Group name"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvPersonalName" runat="server" ControlToValidate="txtPersonalName"
                        ValidationGroup="CreatePersonal" CssClass="field-error" Display="Dynamic"
                        ErrorMessage="Group name is required."></asp:RequiredFieldValidator>
                    <asp:TextBox ID="txtPersonalDescription" runat="server" TextMode="MultiLine" Rows="3"
                        CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md resize-none"
                        placeholder="Description (optional)"></asp:TextBox>
                    <div>
                        <label class="font-label-md text-on-surface-variant block mb-2">Deadline</label>
                        <asp:TextBox ID="txtPersonalDeadline" runat="server" TextMode="DateTimeLocal"
                            CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md"></asp:TextBox>
                    </div>
                    <asp:RequiredFieldValidator ID="rfvPersonalDeadline" runat="server" ControlToValidate="txtPersonalDeadline"
                        ValidationGroup="CreatePersonal" CssClass="field-error" Display="Dynamic"
                        ErrorMessage="Deadline is required."></asp:RequiredFieldValidator>
                    <asp:Button ID="btnCreatePersonal" runat="server" Text="Create personal group" CssClass="btn-primary"
                        OnClick="btnCreatePersonal_Click" ValidationGroup="CreatePersonal" />
                </asp:Panel>

                <asp:Panel ID="pnlCollegeFields" runat="server" Visible="false" CssClass="space-y-4 max-w-2xl">
                    <p class="text-sm text-on-surface-variant">Enter the faculty assignment code to create your college group. Faculty can monitor progress.</p>
                    <asp:TextBox ID="txtCode" runat="server" MaxLength="50"
                        CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md"
                        placeholder="SNA-ASSIGN-0000"></asp:TextBox>
                    <asp:TextBox ID="txtGroupName" runat="server" MaxLength="200"
                        CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md"
                        placeholder="Group name, e.g. Group A"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvCode" runat="server" ControlToValidate="txtCode"
                        ValidationGroup="CreateCollege" CssClass="field-error" Display="Dynamic"
                        ErrorMessage="Enter the faculty assignment code."></asp:RequiredFieldValidator>
                    <asp:RequiredFieldValidator ID="rfvGroupName" runat="server" ControlToValidate="txtGroupName"
                        ValidationGroup="CreateCollege" CssClass="field-error" Display="Dynamic"
                        ErrorMessage="Group name is required."></asp:RequiredFieldValidator>
                    <asp:Button ID="btnCreateCollege" runat="server" Text="Create college group" CssClass="btn-primary"
                        OnClick="btnCreateCollege_Click" ValidationGroup="CreateCollege" />
                </asp:Panel>
            </asp:Panel>

            <asp:Panel ID="pnlPendingInvitations" runat="server" CssClass="standard-card rounded-xl p-6 mb-6">
                <div class="flex items-center gap-2 mb-4">
                    <span class="material-symbols-outlined text-primary">mail</span>
                    <h3 class="font-title-lg text-title-lg text-primary">Pending Invitations</h3>
                </div>
                <asp:Panel ID="pnlNoInvites" runat="server" Visible="false" CssClass="p-4 text-center">
                    <p class="text-sm text-on-surface-variant">No pending invitations.</p>
                </asp:Panel>
                <div class="space-y-3">
                    <asp:Repeater ID="rptInvites" runat="server" OnItemCommand="rptInvites_ItemCommand">
                        <ItemTemplate>
                            <div class="flex items-center justify-between gap-4 p-4 bg-surface-container-low rounded-xl border border-outline-variant/30">
                                <div class="flex-1 min-w-0">
                                    <p class="font-label-md text-label-md font-bold text-on-surface truncate"><%# Eval("GroupName") %></p>
                                    <p class="text-xs text-on-surface-variant mt-1"><%# Eval("AssignmentName") %></p>
                                    <div class="flex gap-2 mt-2 flex-wrap">
                                        <span class="badge-status-medium"><%# TypeLabel(Eval("AssignmentType")) %></span>
                                    </div>
                                </div>
                                <div class="flex gap-2 shrink-0">
                                    <asp:LinkButton runat="server" CommandName="Accept" CommandArgument='<%# Eval("InvitationID") %>'
                                        CssClass="px-4 py-2 bg-[rgba(46,125,50,0.1)] text-[#2e7d32] rounded-lg text-label-md hover:bg-[rgba(46,125,50,0.2)] transition-colors">Accept</asp:LinkButton>
                                    <asp:LinkButton runat="server" CommandName="Decline" CommandArgument='<%# Eval("InvitationID") %>'
                                        CssClass="px-4 py-2 bg-[rgba(211,47,47,0.1)] text-[#d32f2f] rounded-lg text-label-md hover:bg-[rgba(211,47,47,0.2)] transition-colors">Decline</asp:LinkButton>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </asp:Panel>

            <section class="standard-card rounded-xl p-6">
                <div class="flex flex-wrap justify-between items-center gap-3 mb-4">
                    <div class="flex items-center gap-2">
                        <span class="material-symbols-outlined text-primary">workspaces</span>
                        <h3 class="font-title-lg text-title-lg text-primary">My Groups</h3>
                    </div>
                    <div class="flex gap-2 flex-wrap">
                        <asp:Button ID="btnFilterAll" runat="server" Text="All" CausesValidation="false"
                            CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-primary text-on-primary"
                            OnClick="btnFilterAll_Click" />
                        <asp:Button ID="btnFilterPersonal" runat="server" Text="Personal Assignment Group" CausesValidation="false"
                            CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors"
                            OnClick="btnFilterPersonal_Click" />
                        <asp:Button ID="btnFilterCollege" runat="server" Text="College Assignment Group" CausesValidation="false"
                            CssClass="px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors"
                            OnClick="btnFilterCollege_Click" />
                    </div>
                </div>
                <asp:Panel ID="pnlNoMine" runat="server" Visible="false" CssClass="p-12 text-center">
                    <span class="material-symbols-outlined text-6xl text-outline-variant mb-4 block empty-state-icon">school</span>
                    <p class="font-title-lg text-title-lg text-on-surface-variant">No assignment groups yet</p>
                    <p class="text-body-md text-on-surface-variant mt-2">Create a personal group or join a college assignment with a faculty code.</p>
                </asp:Panel>
                <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                    <asp:Repeater ID="rptMine" runat="server">
                        <ItemTemplate>
                            <a href='<%# ResolveUrl("~/Modules/Assignments/GroupWorkspace.aspx?GroupID=" + Eval("GroupID")) %>' class="block group">
                                <div class="standard-card rounded-xl p-6 h-full hover:shadow-lg hover:-translate-y-0.5 transition-all cursor-pointer border border-outline-variant/30">
                                    <div class="flex justify-between items-start mb-4 gap-2">
                                        <span class="badge-status-medium"><%# TypeLabel(Eval("AssignmentType")) %></span>
                                        <span class='<%# Convert.ToBoolean(Eval("IsFinalized")) ? "badge-status-completed" : "badge-status-inprogress" %>'>
                                            <%# Convert.ToBoolean(Eval("IsFinalized")) ? "Finalized" : "Active" %>
                                        </span>
                                    </div>
                                    <h4 class="font-label-lg text-label-lg text-on-surface font-bold mb-2 group-hover:text-primary transition-colors"><%# Eval("GroupName") %></h4>
                                    <p class="text-sm text-on-surface-variant mb-4 line-clamp-2"><%# Eval("AssignmentName") %></p>
                                    <div class="flex justify-between items-center text-xs text-on-surface-variant">
                                        <span class="flex items-center gap-1"><span class="material-symbols-outlined text-[14px]">event</span> Due <%# Eval("Deadline", "{0:MMM dd, yyyy}") %></span>
                                        <span><%# Eval("CompletedTasks") %>/<%# Eval("TotalTasks") %> tasks</span>
                                    </div>
                                    <div class="mt-3 flex justify-between items-center text-xs text-on-surface-variant">
                                        <span><%# Eval("Responsibility") %></span>
                                        <span class="flex items-center gap-1 group-hover:text-primary transition-colors"><span class="material-symbols-outlined text-[14px]">arrow_forward</span> Open</span>
                                    </div>
                                </div>
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </section>
        </main>
    </div>
</asp:Content>
