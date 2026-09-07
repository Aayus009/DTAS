<%@ Page Title="Event Workspace | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EventWorkspace.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Events.EventWorkspace" %>
<%@ Import Namespace="DigitalTransparencySystem.Helpers" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>?v=board7" />
    <style>
        .work-attach { position: relative; overflow-x: hidden; }
        .work-type-btn {
            display: inline-flex; flex-direction: column; align-items: center; justify-content: center;
            gap: 0.3rem; min-width: 4.75rem; padding: 0.75rem 0.65rem;
            border: 1px solid rgb(var(--dt-outline-variant-rgb, 197 197 211) / 0.6);
            border-radius: 0.9rem; background: rgb(var(--dt-surface-rgb, 255 255 255) / 1);
            cursor: pointer; color: inherit; font-size: 11px; font-weight: 700;
            box-shadow: 0 1px 2px rgb(0 0 0 / 0.04);
        }
        .work-type-btn:hover { border-color: rgb(var(--dt-primary-rgb, 0 93 144) / 0.35); background: rgb(0 0 0 / 0.03); }
        .work-type-btn.is-active {
            border-color: rgb(var(--dt-primary-rgb, 0 93 144) / 1);
            background: rgb(var(--dt-primary-container-rgb, 0 119 182) / 0.12);
        }
        .work-type-btn .work-type-icon { font-size: 24px; line-height: 1; }
        .work-type-btn[data-work-type="pdf"] .work-type-icon { color: #c62828; }
        .work-type-btn[data-work-type="word"] .work-type-icon { color: #1565c0; }
        .work-type-btn[data-work-type="image"] .work-type-icon { color: #7b1fa2; }
        .work-type-btn[data-work-type="zip"] .work-type-icon { color: #e67e00; }
        .work-file-drop {
            display: flex; flex-direction: column; align-items: center; justify-content: center;
            gap: 0.35rem; min-height: 6.5rem; padding: 1rem;
            border: 1.5px dashed rgb(var(--dt-outline-variant-rgb, 197 197 211) / 0.9);
            border-radius: 0.85rem; background: rgb(var(--dt-surface-rgb, 255 255 255) / 1);
            cursor: pointer; text-align: center;
        }
        .work-file-drop:hover { border-color: rgb(var(--dt-primary-rgb, 0 93 144) / 0.5); }
        .work-file-drop input[type="file"] { position: absolute; width: 1px; height: 1px; opacity: 0; overflow: hidden; }
        .work-type-panel:not(.is-open) {
            position: absolute !important;
            left: -9999px !important;
            width: 1px !important;
            height: 0 !important;
            overflow: hidden !important;
            padding: 0 !important;
            margin: 0 !important;
            border: 0 !important;
        }
        .file-action { text-decoration: none; }
        .file-action:hover { opacity: 0.9; }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <uc:AdminTopbar ID="adminTop" runat="server" Visible="false" />
    <uc:UserTopbar ID="userTop" runat="server" Visible="false" />

    <div class="event-board-page min-h-screen">
        <uc:AdminSidebar ID="adminSide" ActivePage="Events" runat="server" Visible="false" />
        <uc:UserSidebar ID="userSide" ActivePage="Workspaces" runat="server" Visible="false" />

        <main class="dashboard-main event-board-main">
            <asp:Panel ID="pnlDenied" runat="server" Visible="false" CssClass="standard-card rounded-xl p-8">
                <h1 class="font-headline-lg text-primary mb-2">Private event</h1>
                <p class="text-on-surface-variant">You need an invitation or invitation code to open this event.</p>
                <a href="<%= ResolveUrl("~/Modules/TaskWorkspaces/TaskWorkspaces.aspx") %>" class="text-primary font-bold mt-4 inline-block">Back to workspaces</a>
            </asp:Panel>

            <asp:Panel ID="pnlWorkspace" runat="server" CssClass="jira-workspace">
                <asp:Panel ID="pnlDisabled" runat="server" Visible="false" CssClass="p-4 rounded-xl bg-error-container/20 text-error font-label-md">
                    This event is restricted. Members cannot open workspaces or perform tasks until the system administrator restores it.
                </asp:Panel>

                <header class="jira-top">
                    <div class="jira-header min-w-0">
                        <div>
                            <a href="<%= ResolveUrl("~/Modules/TaskWorkspaces/TaskWorkspaces.aspx") %>" class="inline-flex items-center gap-1 text-primary font-bold text-sm mb-3">
                                <span class="material-symbols-outlined text-[20px]">arrow_back</span>
                                Back to workspaces
                            </a>
                            <p class="jira-kicker">
                                <span class="material-symbols-outlined text-[16px]">view_kanban</span>
                                Shared board
                            </p>
                            <h1 class="jira-title font-headline-lg text-headline-lg text-primary"><asp:Literal ID="litName" runat="server"></asp:Literal></h1>
                            <p class="text-sm text-on-surface-variant mb-3 line-clamp-2"><asp:Literal ID="litDescription" runat="server"></asp:Literal></p>
                            <div class="jira-meta-strip">
                                <span class="jira-chip"><span class="material-symbols-outlined">category</span><asp:Literal ID="litMeta" runat="server"></asp:Literal></span>
                                <span class="jira-chip"><span class="material-symbols-outlined">event</span><asp:Literal ID="litStart" runat="server"></asp:Literal> – <asp:Literal ID="litEnd" runat="server"></asp:Literal></span>
                                <span class="jira-chip"><span class="material-symbols-outlined">location_on</span><asp:Literal ID="litVenue" runat="server"></asp:Literal></span>
                                <span class="jira-chip"><span class="material-symbols-outlined">badge</span><asp:Literal ID="litMyRole" runat="server"></asp:Literal></span>
                                <asp:Panel ID="pnlCode" runat="server" Visible="false" CssClass="jira-chip">
                                    <span class="material-symbols-outlined">key</span>
                                    <asp:Literal ID="litCode" runat="server"></asp:Literal>
                                </asp:Panel>
                            </div>
                            <div class="flex flex-wrap gap-2 mt-4">
                                <asp:Button ID="btnJoinPublic" runat="server" Text="Request to join" CssClass="btn-primary" Visible="false" OnClick="btnJoinPublic_Click" />
                                <asp:Panel ID="pnlJoinPending" runat="server" Visible="false" CssClass="px-4 py-2 bg-surface-container-high rounded-xl font-label-md text-on-surface-variant">Join request pending</asp:Panel>
                                <asp:Button ID="btnAccept" runat="server" Text="Accept invitation" CssClass="btn-primary" Visible="false" OnClick="btnAccept_Click" />
                                <asp:Button ID="btnLeave" runat="server" Text="Leave" CssClass="px-4 py-2 border border-outline rounded-xl font-label-md" Visible="false" OnClick="btnLeave_Click" />
                                <asp:HyperLink ID="lnkEditEvent" runat="server" Visible="false" Text="Edit event"
                                    CssClass="px-4 py-2 bg-primary text-on-primary rounded-xl font-label-md font-bold" />
                                <asp:HyperLink ID="lnkScheduleMeeting" runat="server" Visible="false" Text="Schedule meeting"
                                    CssClass="px-4 py-2 border border-outline rounded-xl font-label-md font-bold" />
                                <asp:Button ID="btnDisable" runat="server" Text="Disable event" CssClass="px-4 py-2 border border-error text-error rounded-xl font-label-md" Visible="false" OnClick="btnDisable_Click" />
                                <asp:Button ID="btnEnable" runat="server" Text="Enable event" CssClass="btn-primary" Visible="false" OnClick="btnEnable_Click" />
                            </div>
                        </div>
                    </div>
                    <asp:Panel ID="pnlMemberAside" runat="server" CssClass="jira-members">
                        <details class="jira-members-box" open>
                            <summary>
                                <span class="material-symbols-outlined text-[20px] text-primary">group</span>
                                <span class="font-title-md text-primary">Members</span>
                                <span class="jira-count"><asp:Literal ID="litMemberCount" runat="server"></asp:Literal></span>
                                <span class="material-symbols-outlined jira-members-chevron ml-auto">expand_more</span>
                            </summary>
                            <div class="jira-members-body">
                                <asp:Repeater ID="rptMembers" runat="server" OnItemCommand="rptMembers_ItemCommand" OnItemDataBound="rptMembers_ItemDataBound">
                                    <ItemTemplate>
                                        <div class="mb-3 pb-3 border-b border-surface-container-high last:border-0">
                                            <p class="font-semibold"><%# Eval("FullName") %></p>
                                            <p class="text-xs text-on-surface-variant"><%# DigitalTransparencySystem.Helpers.EventService.DisplayRole(Convert.ToString(Eval("RoleInEvent"))) %> - <%# Eval("RoleName") %></p>
                                            <asp:Panel ID="pnlMemberActions" runat="server" Visible="false" CssClass="flex flex-wrap gap-2 mt-2">
                                                <asp:LinkButton runat="server" Visible='<%# CanManageMembers %>' CommandName="MakeAdmin" CommandArgument='<%# Eval("UserID") %>' CssClass="text-xs text-primary font-bold">Admin</asp:LinkButton>
                                                <asp:LinkButton runat="server" Visible='<%# CanManageMembers %>' CommandName="MakeManager" CommandArgument='<%# Eval("UserID") %>' CssClass="text-xs text-primary font-bold">Manager</asp:LinkButton>
                                                <asp:LinkButton runat="server" Visible='<%# CanManageMembers %>' CommandName="MakeParticipant" CommandArgument='<%# Eval("UserID") %>' CssClass="text-xs text-primary font-bold">Participant</asp:LinkButton>
                                                <asp:LinkButton ID="btnRemoveMember" runat="server" Visible="false" CommandName="Remove" CommandArgument='<%# Eval("UserID") %>' CssClass="text-xs text-error font-bold">Remove</asp:LinkButton>
                                            </asp:Panel>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <asp:Panel ID="pnlInvite" runat="server" Visible="false" CssClass="mt-4 pt-4 border-t border-outline-variant/40">
                                    <p class="font-label-md text-on-surface mb-2">Assign members</p>
                                    <p class="text-xs text-on-surface-variant mb-3">Add or reassign people. The manager then assigns them to tasks.</p>
                                    <asp:TextBox ID="txtInviteEmail" runat="server" TextMode="Email"
                                        CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md mb-3"
                                        placeholder="name@institution.edu"></asp:TextBox>
                                    <asp:DropDownList ID="ddlInviteRole" runat="server" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md mb-3">
                                        <asp:ListItem Text="Participant" Value="Participant" />
                                        <asp:ListItem Text="Event Manager" Value="EventManager" />
                                        <asp:ListItem Text="Event Administrator" Value="EventAdmin" />
                                    </asp:DropDownList>
                                    <div class="flex gap-2">
                                        <asp:Button ID="btnLookup" runat="server" Text="Look up" CssClass="px-4 py-2 border border-outline rounded-xl font-label-md" OnClick="btnLookup_Click" />
                                        <asp:Button ID="btnInvite" runat="server" Text="Send invite" CssClass="btn-primary" OnClick="btnInvite_Click" />
                                    </div>
                                    <asp:Label ID="lblLookup" runat="server" CssClass="font-label-md block mt-3"></asp:Label>
                                </asp:Panel>
                                <asp:Panel ID="pnlClubLink" runat="server" Visible="false" CssClass="mt-4 pt-4 border-t border-outline-variant/40">
                                    <p class="font-label-md text-on-surface mb-1">Linked club</p>
                                    <p class="text-xs text-on-surface-variant mb-3"><asp:Literal ID="litClubLink" runat="server"></asp:Literal></p>
                                    <div class="flex flex-wrap gap-2">
                                        <asp:Button ID="btnAddClubMembers" runat="server" Text="Add club members" CssClass="px-3 py-1.5 border border-outline rounded-lg font-label-md font-bold" OnClick="btnAddClubMembers_Click" />
                                        <asp:HyperLink ID="lnkEventConnect" runat="server" Visible="false" CssClass="px-3 py-1.5 bg-primary text-on-primary rounded-lg font-label-md font-bold">Open Connect</asp:HyperLink>
                                        <asp:Button ID="btnCreateEventConnect" runat="server" Text="Create Connect group" CssClass="btn-primary" Visible="false" OnClick="btnCreateEventConnect_Click" />
                                    </div>
                                </asp:Panel>
                                <asp:Panel ID="pnlJoinRequests" runat="server" Visible="false" CssClass="mt-4 pt-3 border-t border-outline-variant/40">
                                    <p class="font-label-md text-on-surface mb-2">Join requests</p>
                                    <p class="text-xs text-on-surface-variant mb-3">Accept people who asked to join this public event.</p>
                                    <asp:Repeater ID="rptJoinRequests" runat="server" OnItemCommand="rptJoinRequests_ItemCommand">
                                        <ItemTemplate>
                                            <div class="mb-3 pb-3 border-b border-surface-container-high last:border-0">
                                                <p class="font-semibold"><%# Eval("FullName") %></p>
                                                <p class="text-xs text-on-surface-variant mb-2"><%# Eval("Email") %></p>
                                                <asp:LinkButton runat="server" CommandName="AcceptJoin" CommandArgument='<%# Eval("UserID") %>' CssClass="text-xs text-primary font-bold mr-3">Accept</asp:LinkButton>
                                                <asp:LinkButton runat="server" CommandName="DeclineJoin" CommandArgument='<%# Eval("UserID") %>' CssClass="text-xs text-error font-bold">Decline</asp:LinkButton>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </asp:Panel>
                                <asp:Panel ID="pnlPending" runat="server" Visible="false" CssClass="mt-4 pt-3 border-t border-outline-variant/40">
                                    <p class="font-label-md text-on-surface mb-2">Pending invites</p>
                                    <asp:Repeater ID="rptPending" runat="server">
                                        <ItemTemplate>
                                            <p class="text-sm mb-2"><%# Eval("Email") %> - <%# DigitalTransparencySystem.Helpers.EventService.DisplayRole(Convert.ToString(Eval("InviteRole"))) %></p>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </asp:Panel>
                            </div>
                        </details>
                    </asp:Panel>
                </header>

                <asp:Label ID="lblMessage" runat="server" CssClass="font-label-md block"></asp:Label>

                <asp:Panel ID="pnlPublicPreview" runat="server" Visible="false" CssClass="standard-card rounded-xl p-6 mb-6">
                    <h2 class="font-title-lg text-title-lg text-primary mb-1">Public event details</h2>
                    <p class="text-sm text-on-surface-variant mb-4">Progress and who is leading this event. Request to join; the event lead or manager must accept you before you can work here.</p>
                    <div class="mb-4">
                        <div class="flex justify-between text-xs text-on-surface-variant mb-1">
                            <span>Team progress</span>
                            <span><asp:Literal ID="litPublicProgressLabel" runat="server"></asp:Literal></span>
                        </div>
                        <div class="h-2 bg-surface-container-high rounded-full overflow-hidden">
                            <div id="publicProgressFill" runat="server" class="h-full bg-[#2e7d32] rounded-full" style="width: 0%"></div>
                        </div>
                    </div>
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                        <p><span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest">Leading</span><br />
                            <span class="font-semibold"><asp:Literal ID="litPublicLead" runat="server"></asp:Literal></span></p>
                        <p><span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest">Handling</span><br />
                            <span class="font-semibold"><asp:Literal ID="litPublicHandler" runat="server"></asp:Literal></span></p>
                    </div>
                    <p class="text-xs text-outline mt-4"><asp:Literal ID="litPublicTeam" runat="server"></asp:Literal></p>
                </asp:Panel>

                <asp:Panel ID="pnlMemberWork" runat="server">
                <asp:Panel ID="pnlConcluded" runat="server" Visible="false" CssClass="p-4 rounded-xl bg-tertiary-container/30 text-on-surface">
                    Faculty approved the last task. This event is concluded.
                </asp:Panel>
                <asp:Panel ID="pnlReviewQueue" runat="server" Visible="false" CssClass="standard-card rounded-xl p-4">
                    <p class="font-label-md text-on-surface mb-2">Needs your review</p>
                    <div class="jira-review">
                        <asp:Repeater ID="rptReviewQueue" runat="server">
                            <ItemTemplate>
                                <a href='<%# "#task-" + Eval("TaskID") %>'><%# Eval("TaskTitle") %></a>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlEventMeetings" runat="server" CssClass="standard-card rounded-xl p-4 mb-6">
                    <div class="flex items-center justify-between gap-3 mb-3">
                        <div>
                            <p class="font-label-md text-on-surface">Event meetings</p>
                            <p class="text-xs text-on-surface-variant">Zoom invites go to accepted members of this event.</p>
                        </div>
                        <asp:HyperLink ID="lnkScheduleMeetingCard" runat="server" Visible="false" Text="Schedule meeting"
                            CssClass="text-primary font-label-md font-bold" />
                    </div>
                    <asp:Repeater ID="rptEventMeetings" runat="server">
                        <ItemTemplate>
                            <div class="flex flex-wrap items-center justify-between gap-2 py-2 border-b border-surface-container-high last:border-0">
                                <div>
                                    <p class="font-semibold"><%# Eval("MeetingTitle") %></p>
                                    <p class="text-xs text-on-surface-variant">
                                        <%# Eval("ScheduledDate", "{0:MMM dd, yyyy hh:mm tt}") %>
                                        · <%# Convert.ToInt32(Eval("HasZoom")) == 1 ? "Zoom" : Eval("Venue") %>
                                    </p>
                                </div>
                                <asp:HyperLink runat="server"
                                    NavigateUrl='<%# ResolveUrl("~/Modules/UserMeetings/UserMeetingDetails.aspx?MeetingID=" + Eval("MeetingID")) %>'
                                    CssClass="text-primary font-label-md font-bold">Open</asp:HyperLink>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <asp:Panel ID="pnlNoEventMeetings" runat="server" Visible="false" CssClass="text-sm text-on-surface-variant">
                        No meetings scheduled for this event yet.
                    </asp:Panel>
                </asp:Panel>

                <section class="jira-board-shell">
                    <div class="jira-board-toolbar">
                        <div>
                            <h2 class="font-title-lg text-title-lg text-primary">Board</h2>
                            <p class="text-xs text-on-surface-variant"><asp:Literal ID="litTaskCount" runat="server"></asp:Literal> issues · click a card to open it</p>
                        </div>
                    </div>

                    <asp:Panel ID="pnlCreateTask" runat="server" Visible="false" CssClass="jira-create">
                        <details id="createIssueDetails">
                            <summary>+ Create issue</summary>
                            <p class="text-xs text-on-surface-variant mb-4">Search a member by username or email, then assign the work.</p>
                            <div class="jira-create-grid">
                                <div>
                                    <label class="font-label-md text-on-surface-variant block mb-2">Title</label>
                                    <asp:TextBox ID="txtTaskTitle" runat="server"
                                        CssClass="w-full p-3 bg-surface border border-outline rounded-xl font-body-md"
                                        placeholder="What needs to be done?"></asp:TextBox>
                                </div>
                                <div>
                                    <label class="font-label-md text-on-surface-variant block mb-2">Due date</label>
                                    <asp:TextBox ID="txtTaskDue" runat="server" TextMode="Date"
                                        CssClass="w-full p-3 bg-surface border border-outline rounded-xl font-body-md"></asp:TextBox>
                                </div>
                                <div class="jira-span-2">
                                    <label class="font-label-md text-on-surface-variant block mb-2">Instructions</label>
                                    <asp:TextBox ID="txtTaskDescription" runat="server" TextMode="MultiLine" Rows="2"
                                        CssClass="w-full p-3 bg-surface border border-outline rounded-xl font-body-md resize-none"
                                        placeholder="What should the assigned members do?"></asp:TextBox>
                                </div>
                                <div class="jira-span-2">
                                    <label class="font-label-md text-on-surface-variant block mb-2" for="<%= txtMemberSearch.ClientID %>">Assign members</label>
                                    <asp:HiddenField ID="hfMemberDirectory" runat="server" />
                                    <asp:HiddenField ID="hfAssigneeIds" runat="server" />
                                    <asp:TextBox ID="txtMemberSearch" runat="server" autocomplete="off"
                                        CssClass="w-full p-3 bg-surface border border-outline rounded-xl font-body-md"
                                        placeholder="Type a username or email"></asp:TextBox>
                                    <div id="memberSearchResults" class="mt-2 max-h-64 overflow-y-auto"></div>
                                    <p id="memberSearchHint" class="text-xs text-on-surface-variant mt-2 mb-2">Matching members appear as you type.</p>
                                    <div id="memberSelectedWrap" class="mb-3 hidden">
                                        <p class="text-xs font-label-md text-on-surface-variant mb-2">Assigned to this issue</p>
                                        <div id="memberSelectedChips"></div>
                                    </div>
                                    <asp:Button ID="btnCreateTask" runat="server" Text="Create and assign" CssClass="btn-primary" OnClick="btnCreateTask_Click" />
                                </div>
                            </div>
                        </details>
                    </asp:Panel>

                    <asp:HiddenField ID="hfOpenTask" runat="server" />
                    <div id="jiraTaskSource" class="hidden">
                        <asp:Repeater ID="rptTasks" runat="server" OnItemCommand="rptTasks_ItemCommand" OnItemDataBound="rptTasks_ItemDataBound">
                            <ItemTemplate>
                                <article id='<%# "task-" + Eval("TaskID") %>' class="jira-card" data-col='<%# BoardColumn(Eval("Status")) %>' data-task-id='<%# Eval("TaskID") %>' tabindex="0">
                                    <div class="jira-card-face">
                                        <div class="jira-card-top">
                                            <span class="jira-key">EVT-<%# Eval("TaskID") %></span>
                                            <span class='<%# TimelineService.StatusBadgeClass(Eval("Status"), Eval("DueDate")) %>'><%# TimelineService.StatusLabel(Eval("Status"), Eval("DueDate")) %></span>
                                        </div>
                                        <h4 class="jira-card-title"><%# Eval("TaskTitle") %></h4>
                                        <p class="jira-card-desc"><%# Eval("Description") %></p>
                                        <div class="jira-card-foot">
                                            <%# AvatarStack(Eval("AssigneesShort")) %>
                                            <span class="jira-card-meta">
                                                <asp:Label runat="server" Visible='<%# Convert.ToBoolean(Eval("IsAssigned")) %>' CssClass="jira-yours" Text="Yours" />
                                                <span class="material-symbols-outlined text-[14px]">attach_file</span>
                                                <%# Eval("AttachmentCount") %>
                                                <span class="material-symbols-outlined text-[14px]">chat_bubble</span>
                                                <%# Eval("CommentCount") %>
                                                <span class='<%# TimelineService.DueClass(Eval("DueDate"), Eval("Status")) %>'><%# TimelineService.ShortDueLabel(Eval("DueDate"), Eval("Status")) %></span>
                                            </span>
                                        </div>
                                    </div>
                                    <div class="jira-issue-detail">
                                        <p class="jira-issue-kicker"><%# TimelineService.StatusLabel(Eval("Status"), Eval("DueDate")) %> · <%# Eval("Priority") %> priority</p>
                                        <h3 class="jira-issue-title"><%# Eval("TaskTitle") %></h3>
                                        <p class="jira-issue-desc"><%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("Description"))) ? "No description." : Eval("Description") %></p>
                                        <p class="text-sm text-on-surface-variant mb-4">Assigned to: <%# Eval("Assignees") %></p>
                                        <asp:Label runat="server" Visible='<%# Convert.ToBoolean(Eval("IsAssigned")) %>' CssClass="text-xs font-bold text-primary block mb-3" Text="Your assigned work — update progress here." />
                                        <asp:Label runat="server" Visible='<%# !Convert.ToBoolean(Eval("IsAssigned")) %>' CssClass="text-xs text-outline block mb-3" Text="You can comment. You cannot edit this issue." />

                                        <div class="mb-4">
                                            <p class="text-xs font-label-md text-on-surface-variant mb-2">Work files</p>
                                            <asp:Repeater ID="rptTaskFiles" runat="server">
                                                <ItemTemplate>
                                                    <div class="flex items-start gap-2 p-2 mb-1.5 rounded-lg bg-surface-container-low border border-outline-variant/40">
                                                        <span class="material-symbols-outlined text-[18px] text-primary shrink-0"><%# FileIcon(Eval("FileName")) %></span>
                                                        <div class="min-w-0 flex-1">
                                                            <p class="text-xs font-semibold text-on-surface break-all"><%# Eval("FileName") %></p>
                                                            <p class="text-[11px] text-outline"><%# Eval("FullName") %> · <%# Eval("UploadedAt", "{0:MMM dd, yyyy}") %></p>
                                                            <div class="flex flex-wrap gap-2 mt-1.5">
                                                                <asp:HyperLink runat="server"
                                                                    NavigateUrl='<%# "~/Modules/Events/ViewEventAttachment.aspx?FileID=" + Eval("AttachmentID") %>'
                                                                    CssClass="file-action inline-flex items-center gap-1 px-2.5 py-1 rounded-lg border border-outline text-[11px] font-bold text-on-surface"
                                                                    Target="_blank">
                                                                    <span class="material-symbols-outlined text-[14px]">visibility</span>
                                                                    Open
                                                                </asp:HyperLink>
                                                                <asp:HyperLink runat="server"
                                                                    NavigateUrl='<%# "~/Modules/Events/ViewEventAttachment.aspx?FileID=" + Eval("AttachmentID") + "&download=1" %>'
                                                                    CssClass="file-action inline-flex items-center gap-1 px-2.5 py-1 rounded-lg bg-primary text-on-primary text-[11px] font-bold">
                                                                    <span class="material-symbols-outlined text-[14px]">download</span>
                                                                    Download
                                                                </asp:HyperLink>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                            <asp:Label ID="lblNoFiles" runat="server" CssClass="text-xs text-outline block mb-2" Text="No files uploaded yet." />
                                        </div>

                                        <asp:Panel ID="pnlUploadWork" runat="server" Visible="false" CssClass="mb-4 p-4 bg-surface-container-low rounded-xl">
                                            <p class="font-label-md text-on-surface mb-2">Add note, files, and ZIP</p>
                                            <p class="text-xs text-on-surface-variant mb-3">Choose what to add</p>
                                            <div class="work-attach space-y-3">
                                                <div class="flex flex-wrap gap-2">
                                                    <button type="button" class="work-type-btn" data-work-type="pdf">
                                                        <span class="material-symbols-outlined work-type-icon">picture_as_pdf</span>
                                                        PDF
                                                    </button>
                                                    <button type="button" class="work-type-btn" data-work-type="word">
                                                        <span class="material-symbols-outlined work-type-icon">description</span>
                                                        Word
                                                    </button>
                                                    <button type="button" class="work-type-btn" data-work-type="image">
                                                        <span class="material-symbols-outlined work-type-icon">image</span>
                                                        Image
                                                    </button>
                                                    <button type="button" class="work-type-btn" data-work-type="zip">
                                                        <span class="material-symbols-outlined work-type-icon">folder_zip</span>
                                                        ZIP
                                                    </button>
                                                </div>
                                                <div class="work-type-panel space-y-2 p-3 rounded-xl bg-surface" data-work-panel="pdf">
                                                    <p class="text-xs font-semibold">Add PDF</p>
                                                    <label class="work-file-drop relative">
                                                        <span class="material-symbols-outlined text-[28px] text-primary">upload_file</span>
                                                        <span class="text-sm font-semibold">Choose PDF document</span>
                                                        <span class="text-xs text-outline">PDF only · up to 1000 MB each</span>
                                                        <asp:FileUpload ID="fuPdf" runat="server" AllowMultiple="true" accept=".pdf,application/pdf" />
                                                    </label>
                                                    <p class="work-file-names text-xs text-on-surface-variant">No file selected</p>
                                                </div>
                                                <div class="work-type-panel space-y-2 p-3 rounded-xl bg-surface" data-work-panel="word">
                                                    <p class="text-xs font-semibold">Add Word document</p>
                                                    <label class="work-file-drop relative">
                                                        <span class="material-symbols-outlined text-[28px] text-primary">upload_file</span>
                                                        <span class="text-sm font-semibold">Choose DOC or DOCX</span>
                                                        <span class="text-xs text-outline">Microsoft Word · up to 1000 MB each</span>
                                                        <asp:FileUpload ID="fuWord" runat="server" AllowMultiple="true" accept=".doc,.docx,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document" />
                                                    </label>
                                                    <p class="work-file-names text-xs text-on-surface-variant">No file selected</p>
                                                </div>
                                                <div class="work-type-panel space-y-2 p-3 rounded-xl bg-surface" data-work-panel="image">
                                                    <p class="text-xs font-semibold">Add image</p>
                                                    <label class="work-file-drop relative">
                                                        <span class="material-symbols-outlined text-[28px] text-primary">add_photo_alternate</span>
                                                        <span class="text-sm font-semibold">Choose JPG or PNG</span>
                                                        <span class="text-xs text-outline">Images · up to 1000 MB each</span>
                                                        <asp:FileUpload ID="fuImage" runat="server" AllowMultiple="true" accept=".jpg,.jpeg,.png,image/jpeg,image/png" />
                                                    </label>
                                                    <p class="work-file-names text-xs text-on-surface-variant">No file selected</p>
                                                </div>
                                                <div class="work-type-panel space-y-2 p-3 rounded-xl bg-surface" data-work-panel="zip">
                                                    <p class="text-xs font-semibold">Add ZIP</p>
                                                    <label class="work-file-drop relative">
                                                        <span class="material-symbols-outlined text-[28px] text-primary">folder_zip</span>
                                                        <span class="text-sm font-semibold">Choose a ZIP archive</span>
                                                        <span class="text-xs text-outline">ZIP only · up to 1000 MB each</span>
                                                        <asp:FileUpload ID="fuZip" runat="server" AllowMultiple="true" accept=".zip,application/zip" />
                                                    </label>
                                                    <p class="work-file-names text-xs text-on-surface-variant">No file selected</p>
                                                </div>
                                                <asp:Button ID="btnUploadWork" runat="server" Text="Save files" CssClass="btn-primary"
                                                    CommandName="Upload" CommandArgument='<%# Eval("TaskID") %>' CausesValidation="false" />
                                            </div>
                                        </asp:Panel>

                                        <asp:Panel ID="pnlMyProgress" runat="server" Visible="false" CssClass="mb-4 p-4 bg-surface-container-low rounded-xl">
                                            <p class="font-label-md text-on-surface mb-2">Update your progress</p>
                                            <asp:DropDownList ID="ddlTaskStatus" runat="server" CssClass="w-full p-2 bg-surface border border-outline rounded-lg font-body-md mb-2">
                                                <asp:ListItem Text="Not started" Value="NotStarted" />
                                                <asp:ListItem Text="In progress" Value="InProgress" />
                                                <asp:ListItem Text="Submit for review" Value="UnderReview" />
                                            </asp:DropDownList>
                                            <asp:TextBox ID="txtProgressNote" runat="server" TextMode="MultiLine" Rows="3"
                                                CssClass="w-full p-2 bg-surface border border-outline rounded-lg font-body-md mb-2 resize-none"
                                                placeholder="What did you do? Faculty sees this when you submit for review."></asp:TextBox>
                                            <asp:Button ID="btnSaveProgress" runat="server" Text="Save progress" CssClass="btn-primary"
                                                CommandName="Progress" CommandArgument='<%# Eval("TaskID") %>' CausesValidation="false" />
                                        </asp:Panel>

                                        <asp:Panel ID="pnlFacultyReview" runat="server" Visible="false" CssClass="mb-4 p-4 bg-surface-container-low rounded-xl">
                                            <p class="font-label-md text-on-surface mb-2">Faculty review</p>
                                            <asp:TextBox ID="txtReviewNote" runat="server" TextMode="MultiLine" Rows="3"
                                                CssClass="w-full p-2 bg-surface border border-outline rounded-lg font-body-md mb-2 resize-none"
                                                placeholder="Comment if the work needs to be done again..."></asp:TextBox>
                                            <div class="flex flex-wrap gap-2">
                                                <asp:Button ID="btnApproveTask" runat="server" Text="Approve" CssClass="btn-primary"
                                                    CommandName="Approve" CommandArgument='<%# Eval("TaskID") %>' CausesValidation="false" />
                                                <asp:Button ID="btnReviseTask" runat="server" Text="Send back to redo" CssClass="px-4 py-2 border border-outline rounded-lg font-label-md font-bold"
                                                    CommandName="Revise" CommandArgument='<%# Eval("TaskID") %>' CausesValidation="false" />
                                            </div>
                                        </asp:Panel>

                                        <div class="pt-2">
                                            <p class="text-xs font-label-md text-on-surface-variant mb-2">Comments</p>
                                            <asp:Repeater ID="rptTaskComments" runat="server">
                                                <ItemTemplate>
                                                    <div class="mb-3">
                                                        <p class="text-xs font-semibold"><%# Eval("FullName") %>
                                                            <span class="font-normal text-outline"><%# Eval("CreatedAt", "{0:MMM dd, HH:mm}") %></span>
                                                        </p>
                                                        <p class="text-sm text-on-surface"><%# Eval("Comment") %></p>
                                                    </div>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                            <asp:Label ID="lblNoComments" runat="server" CssClass="text-xs text-outline block mb-2" Text="No comments yet." />
                                            <asp:Panel ID="pnlAddComment" runat="server" Visible="false">
                                                <asp:TextBox ID="txtTaskComment" runat="server" TextMode="MultiLine" Rows="3"
                                                    CssClass="w-full p-2 bg-surface-container-low border border-outline rounded-lg font-body-md mb-2 resize-none"
                                                    placeholder="Comment on this work..."></asp:TextBox>
                                                <asp:Button ID="btnAddTaskComment" runat="server" Text="Comment" CssClass="px-3 py-1.5 border border-outline rounded-lg font-label-md font-bold"
                                                    CommandName="Comment" CommandArgument='<%# Eval("TaskID") %>' CausesValidation="false" />
                                            </asp:Panel>
                                        </div>
                                    </div>
                                </article>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <asp:Panel ID="pnlNoTasks" runat="server" Visible="false" CssClass="text-on-surface-variant text-sm mb-3">No issues on this board yet.</asp:Panel>

                    <div class="jira-board-scroll">
                        <div class="jira-board">
                            <section class="jira-col jira-col-todo">
                                <div class="jira-col-head"><h3>To Do</h3><span class="jira-count" id="cnt-todo">0</span></div>
                                <div class="jira-col-body" id="col-todo"><p class="jira-empty">No issues</p></div>
                            </section>
                            <section class="jira-col jira-col-progress">
                                <div class="jira-col-head"><h3>In Progress</h3><span class="jira-count" id="cnt-progress">0</span></div>
                                <div class="jira-col-body" id="col-progress"><p class="jira-empty">No issues</p></div>
                            </section>
                            <section class="jira-col jira-col-review">
                                <div class="jira-col-head"><h3>In Review</h3><span class="jira-count" id="cnt-review">0</span></div>
                                <div class="jira-col-body" id="col-review"><p class="jira-empty">No issues</p></div>
                            </section>
                            <section class="jira-col jira-col-changes">
                                <div class="jira-col-head"><h3>Changes</h3><span class="jira-count" id="cnt-changes">0</span></div>
                                <div class="jira-col-body" id="col-changes"><p class="jira-empty">No issues</p></div>
                            </section>
                            <section class="jira-col jira-col-done">
                                <div class="jira-col-head"><h3>Done</h3><span class="jira-count" id="cnt-done">0</span></div>
                                <div class="jira-col-body" id="col-done"><p class="jira-empty">No issues</p></div>
                            </section>
                        </div>
                    </div>
                </section>

                <section class="standard-card rounded-xl p-6 jira-notes">
                    <h3 class="font-title-lg text-title-lg text-primary mb-1">Team notes</h3>
                    <p class="text-xs text-on-surface-variant mb-4">Use this thread to communicate with everyone in the event.</p>
                    <asp:Panel ID="pnlAddNote" runat="server" Visible="false" CssClass="mb-4">
                        <asp:TextBox ID="txtNote" runat="server" TextMode="MultiLine" Rows="3"
                            CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md resize-none"
                            placeholder="Write a note for the team..."></asp:TextBox>
                        <div class="flex gap-2 mt-2">
                            <asp:Button ID="btnAddNote" runat="server" Text="Add note" CssClass="btn-primary" OnClick="btnAddNote_Click" />
                            <asp:CheckBox ID="chkPin" runat="server" Text=" Pin" CssClass="font-label-md" />
                        </div>
                    </asp:Panel>
                    <asp:Repeater ID="rptNotes" runat="server">
                        <ItemTemplate>
                            <div class="p-3 bg-surface-container-low rounded-lg mb-2">
                                <p class="font-semibold text-sm"><%# Eval("FullName") %>
                                    <asp:Label runat="server" Visible='<%# Convert.ToBoolean(Eval("IsPinned")) %>' CssClass="text-primary text-xs ml-2">Pinned</asp:Label>
                                </p>
                                <p class="text-on-surface-variant"><%# Eval("NoteText") %></p>
                                <p class="text-xs text-outline mt-1"><%# Eval("CreatedAt", "{0:MMM dd, yyyy HH:mm}") %></p>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <asp:Panel ID="pnlNoNotes" runat="server" Visible="false" CssClass="text-on-surface-variant">No notes yet. Start the conversation here.</asp:Panel>
                </section>
                </asp:Panel>
            </asp:Panel>
            <asp:Panel ID="pnlIssueOverlay" runat="server">
            <div id="jiraIssueOverlay" class="jira-issue-overlay" aria-hidden="true">
                <div class="jira-issue-backdrop" data-close-issue="1"></div>
                <aside class="jira-issue-drawer" role="dialog" aria-modal="true" aria-labelledby="jiraIssueKey">
                    <header class="jira-issue-head">
                        <div>
                            <p class="jira-key" id="jiraIssueKey">Issue</p>
                            <p class="text-xs text-on-surface-variant">Click outside or press Esc to return to the board</p>
                        </div>
                        <button type="button" id="jiraIssueClose" class="px-3 py-1.5 border border-outline rounded-lg font-label-md font-bold">Close</button>
                    </header>
                    <div id="jiraIssueMount" class="jira-issue-mount"></div>
                </aside>
            </div>
            </asp:Panel>
        </main>
    </div>
    <script>
    (function () {
        document.querySelectorAll('#jiraTaskSource .jira-card').forEach(function (card) {
            var col = document.getElementById('col-' + card.getAttribute('data-col'));
            if (col)
                col.appendChild(card);
        });
        ['todo', 'progress', 'review', 'changes', 'done'].forEach(function (key) {
            var col = document.getElementById('col-' + key);
            var count = document.getElementById('cnt-' + key);
            if (col && count)
                count.textContent = col.querySelectorAll('.jira-card').length;
        });

        var overlay = document.getElementById('jiraIssueOverlay');
        var mount = document.getElementById('jiraIssueMount');
        var keyEl = document.getElementById('jiraIssueKey');
        var home = null;
        var current = null;

        function closeIssue() {
            if (current && home)
                home.appendChild(current);
            current = null;
            home = null;
            if (overlay) {
                overlay.classList.remove('is-open');
                overlay.setAttribute('aria-hidden', 'true');
            }
            document.body.classList.remove('jira-issue-open');
        }

        function openIssue(card) {
            if (!card || !overlay || !mount)
                return;
            var detail = card.querySelector('.jira-issue-detail');
            if (!detail)
                return;
            if (current && home)
                home.appendChild(current);
            home = card;
            current = detail;
            mount.appendChild(detail);
            keyEl.textContent = 'EVT-' + card.getAttribute('data-task-id');
            overlay.classList.add('is-open');
            overlay.setAttribute('aria-hidden', 'false');
            document.body.classList.add('jira-issue-open');
        }

        document.querySelectorAll('.jira-card').forEach(function (card) {
            card.addEventListener('click', function () { openIssue(card); });
            card.addEventListener('keydown', function (e) {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    openIssue(card);
                }
            });
        });
        var closeBtn = document.getElementById('jiraIssueClose');
        if (closeBtn)
            closeBtn.addEventListener('click', closeIssue);
        var backdrop = overlay ? overlay.querySelector('.jira-issue-backdrop') : null;
        if (backdrop)
            backdrop.addEventListener('click', closeIssue);
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape')
                closeIssue();
        });
        document.querySelectorAll('a[href^="#task-"]').forEach(function (link) {
            link.addEventListener('click', function (e) {
                var card = document.getElementById((link.getAttribute('href') || '').replace('#', ''));
                if (!card)
                    return;
                e.preventDefault();
                openIssue(card);
            });
        });

        var openField = document.getElementById('<%= hfOpenTask.ClientID %>');
        var target = (openField && openField.value) ? document.getElementById('task-' + openField.value) : null;
        if (!target && location.hash)
            target = document.querySelector(location.hash);
        if (target)
            openIssue(target);

        if (<%= CreateIssueOpen ? "true" : "false" %>) {
            var create = document.getElementById('createIssueDetails');
            if (create)
                create.open = true;
        }
    })();
    (function () {
        var input = document.getElementById('<%= txtMemberSearch.ClientID %>');
        var directoryField = document.getElementById('<%= hfMemberDirectory.ClientID %>');
        var selectedField = document.getElementById('<%= hfAssigneeIds.ClientID %>');
        var resultsBox = document.getElementById('memberSearchResults');
        var hint = document.getElementById('memberSearchHint');
        var selectedWrap = document.getElementById('memberSelectedWrap');
        var chipsBox = document.getElementById('memberSelectedChips');
        if (!input || !directoryField || !selectedField || !resultsBox)
            return;

        function members() {
            try { return JSON.parse(directoryField.value || '[]'); }
            catch (e) { return []; }
        }

        function selectedIds() {
            return (selectedField.value || '').split(',').map(function (part) {
                return parseInt(part, 10);
            }).filter(function (id) { return id > 0; });
        }

        function setSelected(ids) {
            selectedField.value = ids.join(',');
        }

        function escapeHtml(value) {
            return String(value || '').replace(/[&<>"']/g, function (ch) {
                return ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[ch];
            });
        }

        function matches(member, term) {
            var needle = term.toLowerCase();
            return [member.name, member.username, member.email].some(function (value) {
                return String(value || '').toLowerCase().indexOf(needle) !== -1;
            });
        }

        function render() {
            var term = (input.value || '').trim();
            var chosen = selectedIds();
            var list = members();
            var html = '';

            if (term.length === 0) {
                resultsBox.innerHTML = '';
                hint.textContent = 'Matching members appear as you type. Username and email are shown so you can tell people apart.';
            } else {
                var hits = list.filter(function (member) {
                    return chosen.indexOf(member.id) === -1 && matches(member, term);
                });
                if (hits.length === 0) {
                    hint.textContent = 'No event member matches that username or email.';
                    resultsBox.innerHTML = '';
                } else {
                    hint.textContent = hits.length + ' match' + (hits.length === 1 ? '' : 'es') + '. Add the right person.';
                    hits.forEach(function (member) {
                        html += '<div class="flex items-start justify-between gap-3 p-3 mb-2 bg-surface rounded-xl border border-outline/30">'
                            + '<div class="min-w-0"><p class="font-semibold text-on-surface">' + escapeHtml(member.name) + '</p>'
                            + '<p class="text-xs text-on-surface-variant break-all">' + escapeHtml(member.username) + ' · ' + escapeHtml(member.email) + '</p></div>'
                            + '<button type="button" class="shrink-0 text-sm text-primary font-bold" data-add="' + member.id + '">Add</button></div>';
                    });
                    resultsBox.innerHTML = html;
                }
            }

            var selectedMembers = list.filter(function (member) { return chosen.indexOf(member.id) !== -1; });
            if (selectedMembers.length === 0) {
                selectedWrap.classList.add('hidden');
                chipsBox.innerHTML = '';
            } else {
                selectedWrap.classList.remove('hidden');
                chipsBox.innerHTML = selectedMembers.map(function (member) {
                    return '<div class="inline-flex items-center gap-2 mr-2 mb-2 px-3 py-2 bg-surface rounded-full border border-outline/40">'
                        + '<span class="text-sm">' + escapeHtml(member.name) + '</span>'
                        + '<span class="text-xs text-on-surface-variant">' + escapeHtml(member.username) + ' · ' + escapeHtml(member.email) + '</span>'
                        + '<button type="button" class="text-error font-bold text-sm" data-remove="' + member.id + '" title="Remove">×</button></div>';
                }).join('');
            }
        }

        input.addEventListener('input', render);
        input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter')
                e.preventDefault();
        });
        resultsBox.addEventListener('click', function (e) {
            var button = e.target.closest('[data-add]');
            if (!button) return;
            var id = parseInt(button.getAttribute('data-add'), 10);
            var ids = selectedIds();
            if (ids.indexOf(id) === -1)
                ids.push(id);
            setSelected(ids);
            input.value = '';
            input.focus();
            render();
        });
        chipsBox.addEventListener('click', function (e) {
            var button = e.target.closest('[data-remove]');
            if (!button) return;
            var id = parseInt(button.getAttribute('data-remove'), 10);
            setSelected(selectedIds().filter(function (value) { return value !== id; }));
            render();
        });
        render();
    })();
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('.work-type-btn');
        if (!btn) return;
        e.preventDefault();
        e.stopPropagation();
        var root = btn.closest('.work-attach');
        if (!root) return;
        var type = btn.getAttribute('data-work-type');
        var panel = root.querySelector('[data-work-panel="' + type + '"]');
        if (!panel) return;
        var open = !panel.classList.contains('is-open');
        panel.classList.toggle('is-open', open);
        btn.classList.toggle('is-active', open);
        if (!open) return;
        var file = panel.querySelector('input[type="file"]');
        if (file) file.click();
    });
    document.addEventListener('change', function (e) {
        if (!e.target.matches('input[type="file"]')) return;
        var panel = e.target.closest('.work-type-panel');
        if (!panel) return;
        var names = panel.querySelector('.work-file-names');
        if (!names) return;
        var files = e.target.files || [];
        if (!files.length)
            names.textContent = 'No file selected';
        else
            names.textContent = Array.prototype.map.call(files, function (f) { return f.name; }).join(', ');
    });
    </script>
</asp:Content>
