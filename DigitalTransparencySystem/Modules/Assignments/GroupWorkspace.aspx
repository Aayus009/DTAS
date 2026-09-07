<%@ Page Title="Assignment Group | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GroupWorkspace.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Assignments.GroupWorkspace" %>
<%@ Import Namespace="DigitalTransparencySystem.Helpers" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
    <style>
        .task-submit-details > summary { list-style: none; }
        .task-submit-details > summary::-webkit-details-marker { display: none; }
        .task-submit-details[open] .task-submit-chevron { transform: rotate(180deg); }
        .member-updates-details > summary { list-style: none; }
        .member-updates-details > summary::-webkit-details-marker { display: none; }
        .member-updates-details[open] .member-updates-chevron { transform: rotate(180deg); }
        .member-updates-scroll {
            max-height: 16rem;
            overflow-y: auto;
            padding-right: 0.25rem;
        }
        .file-action { text-decoration: none; }
        .file-action:hover { opacity: 0.9; }
        .work-attach { position: relative; overflow-x: hidden; }
        .work-type-btn {
            display: inline-flex; flex-direction: column; align-items: center; justify-content: center;
            gap: 0.3rem; min-width: 4.75rem; padding: 0.75rem 0.65rem;
            border: 1px solid rgb(var(--dt-outline-variant-rgb, 197 197 211) / 0.6);
            border-radius: 0.9rem; background: rgb(var(--dt-surface-rgb, 255 255 255) / 1);
            cursor: pointer; color: inherit; font-size: 11px; font-weight: 700;
            box-shadow: 0 1px 2px rgb(0 0 0 / 0.04);
        }
        .work-type-btn:hover { border-color: rgb(var(--dt-primary-rgb, 0 17 66) / 0.35); background: rgb(0 0 0 / 0.03); }
        .work-type-btn.is-active {
            border-color: rgb(var(--dt-primary-rgb, 0 17 66) / 1);
            background: rgb(var(--dt-primary-container-rgb, 0 35 111) / 0.12);
        }
        .work-type-btn .work-type-icon { font-size: 24px; line-height: 1; }
        .work-type-btn[data-work-type="pdf"] .work-type-icon { color: #c62828; }
        .work-type-btn[data-work-type="word"] .work-type-icon { color: #1565c0; }
        .work-type-btn[data-work-type="github"] .work-type-icon { color: #24292f; }
        .work-type-btn[data-work-type="url"] .work-type-icon { color: #0b8043; }
        .work-type-btn[data-work-type="image"] .work-type-icon { color: #7b1fa2; }
        .work-file-drop {
            display: flex; flex-direction: column; align-items: center; justify-content: center;
            gap: 0.35rem; min-height: 6.5rem; padding: 1rem;
            border: 1.5px dashed rgb(var(--dt-outline-variant-rgb, 197 197 211) / 0.9);
            border-radius: 0.85rem; background: rgb(var(--dt-surface-rgb, 255 255 255) / 1);
            cursor: pointer; text-align: center;
        }
        .work-file-drop:hover { border-color: rgb(var(--dt-primary-rgb, 0 17 66) / 0.5); }
        .work-file-drop input[type="file"] { position: absolute; width: 1px; height: 1px; opacity: 0; overflow: hidden; }
        .github-path {
            display: flex; flex-wrap: wrap; align-items: center; gap: 0.4rem;
            padding: 0.65rem 0.75rem; border-radius: 0.85rem;
            background: rgb(var(--dt-surface-rgb, 255 255 255) / 1);
            border: 1px solid rgb(var(--dt-outline-variant-rgb, 197 197 211) / 0.7);
        }
        .github-path .github-prefix { font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace; font-size: 12px; color: rgb(var(--dt-on-surface-variant-rgb, 70 70 80) / 1); }
        .github-path input { min-width: 7rem; flex: 1; border: 0; background: transparent; padding: 0.35rem 0; outline: none; font-size: 14px; }
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
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <uc:AdminTopbar ID="adminTop" runat="server" Visible="false" />
    <uc:UserTopbar ID="userTop" runat="server" Visible="false" />

    <div class="flex min-h-screen">
        <uc:AdminSidebar ID="adminSide" ActivePage="Assignments" runat="server" Visible="false" />
        <uc:UserSidebar ID="userSide" ActivePage="Assignments" runat="server" Visible="false" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            <asp:Panel ID="pnlDenied" runat="server" Visible="false" CssClass="standard-card rounded-xl p-8">
                <asp:HyperLink ID="lnkDeniedBack" runat="server" NavigateUrl="~/Modules/Assignments/MyAssignments.aspx"
                    CssClass="inline-flex items-center gap-1 text-primary font-bold text-sm mb-3">
                    <span class="material-symbols-outlined text-[20px]">arrow_back</span>
                    Back
                </asp:HyperLink>
                <p class="text-on-surface-variant">You do not have access to this subgroup.</p>
            </asp:Panel>

            <asp:Panel ID="pnlWorkspace" runat="server">
                <asp:Panel ID="pnlFinalized" runat="server" Visible="false" CssClass="mb-4 p-4 rounded-xl bg-secondary-container/30 font-label-md">
                    This group is finalized. Members cannot change structure or reassign tasks.
                </asp:Panel>
                <asp:Panel ID="pnlDeadlineAlert" runat="server" Visible="false" CssClass="mb-4 p-4 rounded-xl bg-[rgba(230,126,0,0.12)] text-[#b86b00] font-label-md">
                    <asp:Literal ID="litDeadlineAlert" runat="server"></asp:Literal>
                </asp:Panel>
                <asp:Panel ID="pnlLocked" runat="server" Visible="false" CssClass="mb-4 p-4 rounded-xl bg-error-container/20 text-error font-label-md">
                    The deadline has passed and this assignment is closed for students. No updates, extensions, or new work unless faculty extends the deadline.
                </asp:Panel>

                <header class="flex justify-between items-start mb-8 gap-4">
                    <div>
                        <asp:HyperLink ID="lnkBack" runat="server" CssClass="inline-flex items-center gap-1 text-primary font-bold text-sm mb-3">
                            <span class="material-symbols-outlined text-[20px]">arrow_back</span>
                            Back
                        </asp:HyperLink>
                        <h1 class="font-headline-lg text-headline-lg text-primary mb-2"><asp:Literal ID="litGroup" runat="server"></asp:Literal></h1>
                        <p class="text-on-surface-variant"><asp:Literal ID="litMeta" runat="server"></asp:Literal></p>
                    </div>
                    <div class="flex gap-2 flex-wrap">
                        <asp:HyperLink ID="lnkScheduleMeeting" runat="server"
                            CssClass="inline-flex items-center gap-1 px-4 py-2 bg-primary text-on-primary rounded-xl font-label-md font-bold">
                            <span class="material-symbols-outlined text-[18px]">videocam</span>
                            Schedule meeting
                        </asp:HyperLink>
                        <asp:Button ID="btnFinalize" runat="server" Text="Finalize group" CssClass="btn-primary" Visible="false" OnClick="btnFinalize_Click"
                            OnClientClick="return dtasConfirm(this, 'Finalize this group? Task structure and membership will lock.');" />
                    </div>
                </header>

                <asp:Label ID="lblMessage" runat="server" CssClass="font-label-md block mb-4"></asp:Label>

                <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-6">
                    <section class="standard-card rounded-xl p-6 lg:col-span-2">
                        <h3 class="font-title-lg text-title-lg text-primary mb-2">Progress</h3>
                        <p class="font-headline-md text-primary"><asp:Literal ID="litProgress" runat="server"></asp:Literal></p>
                        <p class="text-xs text-on-surface-variant">Completed tasks / total tasks for this subgroup.</p>
                    </section>
                    <section class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-2">Deadline</h3>
                        <p><asp:Literal ID="litDeadline" runat="server"></asp:Literal></p>
                        <asp:Panel ID="pnlFacultyDeadline" runat="server" Visible="false" CssClass="mt-3 space-y-2">
                            <p class="text-xs text-on-surface-variant">Faculty can extend this deadline at any time.</p>
                            <asp:TextBox ID="txtFacultyDeadline" runat="server" TextMode="DateTimeLocal"
                                CssClass="w-full p-2 border border-outline rounded-lg font-body-md"></asp:TextBox>
                            <asp:Button ID="btnFacultyExtend" runat="server" Text="Extend deadline" CssClass="btn-primary" OnClick="btnFacultyExtend_Click" />
                        </asp:Panel>
                    </section>
                </div>

                <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
                    <div class="lg:col-span-2 space-y-6">
                        <section class="standard-card rounded-xl p-6">
                            <h3 class="font-title-lg text-title-lg text-primary mb-4">Your Tasks</h3>
                            <asp:Panel ID="pnlAddTask" runat="server" Visible="false" CssClass="mb-4 p-4 bg-surface-container-low rounded-lg space-y-3">
                                <asp:TextBox ID="txtTaskTitle" runat="server" placeholder="Section or task title"
                                    CssClass="w-full p-3 bg-surface border border-outline rounded-xl font-body-md"></asp:TextBox>
                                <asp:DropDownList ID="ddlParent" runat="server" CssClass="w-full p-3 bg-surface border border-outline rounded-xl font-body-md"></asp:DropDownList>
                                <asp:DropDownList ID="ddlAssignee" runat="server" CssClass="w-full p-3 bg-surface border border-outline rounded-xl font-body-md"></asp:DropDownList>
                                <asp:TextBox ID="txtDue" runat="server" TextMode="Date" CssClass="w-full p-3 bg-surface border border-outline rounded-xl font-body-md"></asp:TextBox>
                                <asp:Button ID="btnAddTask" runat="server" Text="Add task" CssClass="btn-primary" OnClick="btnAddTask_Click" />
                            </asp:Panel>
                            <asp:Repeater ID="rptTasks" runat="server" OnItemCommand="rptTasks_ItemCommand" OnItemDataBound="rptTasks_ItemDataBound">
                                <ItemTemplate>
                                    <div class='<%# Eval("ParentTaskID") == DBNull.Value ? "p-3 bg-surface-container-low rounded-lg mb-2" : "p-3 ml-6 bg-surface-container-lowest rounded-lg mb-2" %>'>
                                        <div class="flex justify-between gap-3 flex-wrap">
                                            <div>
                                                <p class="font-semibold"><%# Eval("Title") %></p>
                                                <p class='<%# "text-xs " + TimelineService.DueClass(Eval("DueDate"), Eval("Status")) %>'>
                                                    <%# Eval("AssigneeName") %> · <%# TimelineService.DueLabel(Eval("DueDate"), Eval("Status")) %>
                                                </p>
                                                <asp:Label runat="server" Visible='<%# TimelineService.IsOverdue(Eval("DueDate"), Eval("Status")) %>'
                                                    CssClass="badge-status-late mt-1 inline-block">Late</asp:Label>
                                            </div>
                                            <div id="pnlTaskActions" runat="server" class="flex gap-2 items-center">
                                                <asp:DropDownList ID="ddlStatus" runat="server" CssClass="p-2 border border-outline rounded-lg text-sm">
                                                    <asp:ListItem Value="ToDo">To Do</asp:ListItem>
                                                    <asp:ListItem Value="InProgress">In Progress</asp:ListItem>
                                                    <asp:ListItem Value="UnderReview">Under Review</asp:ListItem>
                                                    <asp:ListItem Value="Completed">Completed</asp:ListItem>
                                                    <asp:ListItem Value="Blocked">Blocked</asp:ListItem>
                                                </asp:DropDownList>
                                                <asp:Button runat="server" CommandName="Save" CommandArgument='<%# Eval("TaskID") %>'
                                                    Text="Save" CausesValidation="false"
                                                    CssClass="text-primary font-bold text-sm bg-transparent border-0 p-0 cursor-pointer" />
                                            </div>
                                        </div>
                                        <asp:Panel ID="pnlWork" runat="server" Visible="false" CssClass="mt-3 p-3 bg-surface rounded-lg">
                                            <p class="text-xs text-on-surface-variant mb-1">Submitted work</p>
                                            <asp:Literal ID="litWorkNote" runat="server"></asp:Literal>
                                            <asp:Repeater ID="rptWorkLinks" runat="server">
                                                <ItemTemplate>
                                                    <div class="flex items-start gap-2 p-2 mt-1.5 rounded-lg bg-surface-container-low border border-outline-variant/40">
                                                        <span class="material-symbols-outlined text-[18px] text-primary shrink-0">link</span>
                                                        <div class="min-w-0 flex-1">
                                                            <p class="text-xs font-semibold text-on-surface break-all"><%# Eval("DisplayLabel") %></p>
                                                            <a href='<%# Eval("Url") %>' target="_blank" rel="noopener noreferrer"
                                                                class="file-action inline-flex items-center gap-1 mt-1.5 px-2.5 py-1 rounded-lg bg-primary text-on-primary text-[11px] font-bold">
                                                                <span class="material-symbols-outlined text-[14px]">open_in_new</span>
                                                                Open link
                                                            </a>
                                                        </div>
                                                    </div>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                            <asp:Repeater ID="rptWorkFiles" runat="server">
                                                <ItemTemplate>
                                                    <div class="flex items-start gap-2 p-2 mt-1.5 rounded-lg bg-surface-container-low border border-outline-variant/40">
                                                        <span class="material-symbols-outlined text-[18px] text-primary shrink-0"><%# FileIcon(Eval("FileName")) %></span>
                                                        <div class="min-w-0 flex-1">
                                                            <p class="text-xs font-semibold text-on-surface break-all"><%# Eval("FileName") %></p>
                                                            <div class="flex flex-wrap gap-2 mt-1.5">
                                                                <asp:HyperLink runat="server"
                                                                    NavigateUrl='<%# "~/Modules/Assignments/ViewAssignmentSubmission.aspx?FileID=" + Eval("FileID") %>'
                                                                    CssClass="file-action inline-flex items-center gap-1 px-2.5 py-1 rounded-lg border border-outline text-[11px] font-bold text-on-surface"
                                                                    Target="_blank">
                                                                    <span class="material-symbols-outlined text-[14px]">visibility</span>
                                                                    Open
                                                                </asp:HyperLink>
                                                                <asp:HyperLink runat="server"
                                                                    NavigateUrl='<%# "~/Modules/Assignments/ViewAssignmentSubmission.aspx?FileID=" + Eval("FileID") + "&download=1" %>'
                                                                    CssClass="file-action inline-flex items-center gap-1 px-2.5 py-1 rounded-lg bg-primary text-on-primary text-[11px] font-bold">
                                                                    <span class="material-symbols-outlined text-[14px]">download</span>
                                                                    Download
                                                                </asp:HyperLink>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </asp:Panel>
                                        <asp:Panel ID="pnlSubmit" runat="server" Visible="false" CssClass="mt-3">
                                            <details class="task-submit-details p-3 bg-surface rounded-lg">
                                                <summary class="cursor-pointer select-none font-label-md text-primary font-bold flex items-center gap-2">
                                                    <span class="material-symbols-outlined text-[20px] task-submit-chevron transition-transform">expand_more</span>
                                                    Add note, links, and files
                                                </summary>
                                                <div class="work-attach mt-3 space-y-3">
                                                    <asp:TextBox ID="txtWorkNote" runat="server" TextMode="MultiLine" Rows="2"
                                                        placeholder="Optional note"
                                                        CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md resize-none"></asp:TextBox>
                                                    <p class="text-xs text-on-surface-variant">Choose what to add</p>
                                                    <div class="flex flex-wrap gap-2">
                                                        <button type="button" class="work-type-btn" data-work-type="pdf">
                                                            <span class="material-symbols-outlined work-type-icon">picture_as_pdf</span>
                                                            PDF
                                                        </button>
                                                        <button type="button" class="work-type-btn" data-work-type="word">
                                                            <span class="material-symbols-outlined work-type-icon">description</span>
                                                            Word
                                                        </button>
                                                        <button type="button" class="work-type-btn" data-work-type="github">
                                                            <span class="material-symbols-outlined work-type-icon">hub</span>
                                                            GitHub
                                                        </button>
                                                        <button type="button" class="work-type-btn" data-work-type="url">
                                                            <span class="material-symbols-outlined work-type-icon">link</span>
                                                            URL
                                                        </button>
                                                        <button type="button" class="work-type-btn" data-work-type="image">
                                                            <span class="material-symbols-outlined work-type-icon">image</span>
                                                            Image
                                                        </button>
                                                    </div>

                                                    <div class="work-type-panel space-y-2 p-3 rounded-xl bg-surface-container-low" data-work-panel="pdf">
                                                        <p class="text-xs font-semibold">Add PDF</p>
                                                        <label class="work-file-drop relative">
                                                            <span class="material-symbols-outlined text-[28px] text-primary">upload_file</span>
                                                            <span class="text-sm font-semibold">Choose PDF document</span>
                                                            <span class="text-xs text-outline">PDF only - up to 1000 MB each</span>
                                                            <asp:FileUpload ID="fuPdf" runat="server" AllowMultiple="true" accept=".pdf,application/pdf" />
                                                        </label>
                                                        <p class="work-file-names text-xs text-on-surface-variant">No file selected</p>
                                                    </div>
                                                    <div class="work-type-panel space-y-2 p-3 rounded-xl bg-surface-container-low" data-work-panel="word">
                                                        <p class="text-xs font-semibold">Add Word document</p>
                                                        <label class="work-file-drop relative">
                                                            <span class="material-symbols-outlined text-[28px] text-primary">upload_file</span>
                                                            <span class="text-sm font-semibold">Choose DOC or DOCX</span>
                                                            <span class="text-xs text-outline">Microsoft Word - up to 1000 MB each</span>
                                                            <asp:FileUpload ID="fuWord" runat="server" AllowMultiple="true" accept=".doc,.docx,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document" />
                                                        </label>
                                                        <p class="work-file-names text-xs text-on-surface-variant">No file selected</p>
                                                    </div>
                                                    <div class="work-type-panel space-y-2 p-3 rounded-xl bg-surface-container-low" data-work-panel="github">
                                                        <p class="text-xs font-semibold">GitHub repository</p>
                                                        <p class="text-xs text-on-surface-variant">Add the owner and repository, or paste a full github.com URL in Repository.</p>
                                                        <div class="github-path">
                                                            <span class="github-prefix">github.com /</span>
                                                            <asp:TextBox ID="txtGithubOwner" runat="server" placeholder="owner"
                                                                CssClass="github-owner"></asp:TextBox>
                                                            <span class="github-prefix">/</span>
                                                            <asp:TextBox ID="txtGithubRepo" runat="server" placeholder="repository"
                                                                CssClass="github-repo"></asp:TextBox>
                                                        </div>
                                                        <p class="text-xs text-on-surface-variant">Saved as <span class="github-preview font-mono text-primary">https://github.com/...</span></p>
                                                    </div>
                                                    <div class="work-type-panel space-y-2 p-3 rounded-xl bg-surface-container-low" data-work-panel="url">
                                                        <p class="text-xs font-semibold">Web link</p>
                                                        <asp:TextBox ID="txtWorkUrl" runat="server" placeholder="https://discord.gg/invite"
                                                            CssClass="work-url-input w-full p-3 bg-surface border border-outline rounded-xl font-body-md"></asp:TextBox>
                                                        <p class="text-xs text-outline">Discord, Drive, or any https link.</p>
                                                    </div>
                                                    <div class="work-type-panel space-y-2 p-3 rounded-xl bg-surface-container-low" data-work-panel="image">
                                                        <p class="text-xs font-semibold">Add image</p>
                                                        <label class="work-file-drop relative">
                                                            <span class="material-symbols-outlined text-[28px] text-primary">add_photo_alternate</span>
                                                            <span class="text-sm font-semibold">Choose JPG or PNG</span>
                                                            <span class="text-xs text-outline">Images - up to 1000 MB each</span>
                                                            <asp:FileUpload ID="fuImage" runat="server" AllowMultiple="true" accept=".jpg,.jpeg,.png,image/jpeg,image/png" />
                                                        </label>
                                                        <p class="work-file-names text-xs text-on-surface-variant">No file selected</p>
                                                    </div>

                                                    <asp:Button runat="server" CommandName="SubmitWork" CommandArgument='<%# Eval("TaskID") %>'
                                                        Text="Save" CausesValidation="false" CssClass="btn-primary" />
                                                </div>
                                            </details>
                                        </asp:Panel>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            <asp:Panel ID="pnlNoTasks" runat="server" Visible="false" CssClass="text-on-surface-variant">No tasks yet. Add sections, then nest tasks under them.</asp:Panel>
                        </section>

                        <section class="standard-card rounded-xl p-6">
                            <h3 class="font-title-lg text-title-lg text-primary mb-2">Contribution</h3>
                            <p class="text-xs text-on-surface-variant mb-4">50% completion - 20% timeliness - 20% activity - 10% recency. Calculated by the database, not estimated.</p>
                            <p class="mb-4"><a class="text-primary font-bold text-sm" href='<%= ResolveUrl("~/Modules/Assignments/ContributionReport.aspx?GroupID=" + Request.QueryString["GroupID"]) %>'>Open full contribution report</a></p>
                            <asp:Repeater ID="rptContribution" runat="server">
                                <HeaderTemplate>
                                    <table class="w-full text-left text-sm">
                                        <thead>
                                            <tr class="text-on-surface-variant">
                                                <th class="py-2">Member</th>
                                                <th class="py-2">Done</th>
                                                <th class="py-2">Score</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <tr>
                                        <td class="py-2"><%# Eval("FullName") %> <span class="text-outline"><%# Eval("Responsibility") %></span></td>
                                        <td class="py-2"><%# Eval("CompletedTasks") %>/<%# Eval("AssignedTasks") %></td>
                                        <td class="py-2 font-semibold"><%# DigitalTransparencySystem.Helpers.AssignmentService.FormatPercent(Eval("ContributionScore")) %></td>
                                    </tr>
                                </ItemTemplate>
                                <FooterTemplate>
                                        </tbody>
                                    </table>
                                </FooterTemplate>
                            </asp:Repeater>
                        </section>
                    </div>

                    <div class="space-y-6">
                        <asp:Panel ID="pnlInvite" runat="server" Visible="false" CssClass="standard-card rounded-xl p-6">
                            <h3 class="font-title-lg text-title-lg text-primary mb-3">Invite member</h3>
                            <p class="text-xs text-on-surface-variant mb-3">Enter a complete email. The directory is not listed.</p>
                            <asp:TextBox ID="txtInviteEmail" runat="server" TextMode="Email"
                                CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md mb-3"></asp:TextBox>
                            <div class="flex gap-2">
                                <asp:Button ID="btnLookup" runat="server" Text="Look up" CssClass="px-4 py-2 border border-outline rounded-xl font-label-md" OnClick="btnLookup_Click" />
                                <asp:Button ID="btnInvite" runat="server" Text="Invite" CssClass="btn-primary" OnClick="btnInvite_Click" />
                            </div>
                            <asp:Label ID="lblLookup" runat="server" CssClass="font-label-md block mt-3"></asp:Label>
                        </asp:Panel>

                        <section class="standard-card rounded-xl p-6">
                            <h3 class="font-title-lg text-title-lg text-primary mb-1">Members</h3>
                            <p class="text-xs text-on-surface-variant mb-4">Status changes, notes, and files each member posted.</p>
                            <asp:Repeater ID="rptMembers" runat="server" OnItemCommand="rptMembers_ItemCommand" OnItemDataBound="rptMembers_ItemDataBound">
                                <ItemTemplate>
                                    <div class="mb-4 pb-4 border-b border-surface-container-high last:border-0 last:mb-0 last:pb-0">
                                        <p class="font-semibold"><%# Eval("FullName") %></p>
                                        <p class="text-xs text-on-surface-variant"><%# Eval("Responsibility") %></p>
                                        <asp:LinkButton runat="server" Visible='<%# CanAssignLeader && Convert.ToString(Eval("Responsibility")) != "Leader" %>' CommandName="MakeLeader" CommandArgument='<%# Eval("UserID") %>'
                                            CssClass="text-xs text-primary font-bold">Make leader</asp:LinkButton>
                                        <details class="member-updates-details mt-3">
                                            <summary class="cursor-pointer select-none font-label-md text-primary font-bold flex items-center gap-2">
                                                <span class="material-symbols-outlined text-[20px] member-updates-chevron transition-transform">expand_more</span>
                                                Updates
                                                <asp:Literal ID="litUpdateCount" runat="server"></asp:Literal>
                                            </summary>
                                            <div class="member-updates-scroll mt-2">
                                        <asp:Repeater ID="rptUpdates" runat="server" OnItemDataBound="rptUpdates_ItemDataBound">
                                            <ItemTemplate>
                                                <div class="mb-3 p-3 rounded-xl bg-surface-container-low">
                                                    <p class="text-[11px] text-outline mb-1"><%# Eval("ChangedAt", "{0:MMM dd, yyyy HH:mm}") %></p>
                                                    <p class="text-xs text-on-surface mb-2"><%# Eval("Summary") %></p>
                                                    <asp:Repeater ID="rptUpdateLinks" runat="server">
                                                        <ItemTemplate>
                                                            <div class="flex items-start gap-2 p-2 mb-1.5 rounded-lg bg-surface border border-outline-variant/40">
                                                                <span class="material-symbols-outlined text-[18px] text-primary shrink-0">link</span>
                                                                <div class="min-w-0 flex-1">
                                                                    <p class="text-xs font-semibold text-on-surface break-all"><%# Eval("DisplayLabel") %></p>
                                                                    <a href='<%# Eval("Url") %>' target="_blank" rel="noopener noreferrer"
                                                                        class="file-action inline-flex items-center gap-1 mt-1.5 px-2.5 py-1 rounded-lg bg-primary text-on-primary text-[11px] font-bold">
                                                                        <span class="material-symbols-outlined text-[14px]">open_in_new</span>
                                                                        Open link
                                                                    </a>
                                                                </div>
                                                            </div>
                                                        </ItemTemplate>
                                                    </asp:Repeater>
                                                    <asp:Repeater ID="rptUpdateFiles" runat="server">
                                                        <ItemTemplate>
                                                            <div class="flex items-start gap-2 p-2 mb-1.5 rounded-lg bg-surface border border-outline-variant/40">
                                                                <span class="material-symbols-outlined text-[18px] text-primary shrink-0"><%# FileIcon(Eval("FileName")) %></span>
                                                                <div class="min-w-0 flex-1">
                                                                    <p class="text-xs font-semibold text-on-surface break-all"><%# Eval("FileName") %></p>
                                                                    <div class="flex flex-wrap gap-2 mt-1.5">
                                                                        <asp:HyperLink runat="server"
                                                                            NavigateUrl='<%# "~/Modules/Assignments/ViewAssignmentSubmission.aspx?FileID=" + Eval("FileID") %>'
                                                                            CssClass="file-action inline-flex items-center gap-1 px-2.5 py-1 rounded-lg border border-outline text-[11px] font-bold text-on-surface"
                                                                            Target="_blank">
                                                                            <span class="material-symbols-outlined text-[14px]">visibility</span>
                                                                            Open
                                                                        </asp:HyperLink>
                                                                        <asp:HyperLink runat="server"
                                                                            NavigateUrl='<%# "~/Modules/Assignments/ViewAssignmentSubmission.aspx?FileID=" + Eval("FileID") + "&download=1" %>'
                                                                            CssClass="file-action inline-flex items-center gap-1 px-2.5 py-1 rounded-lg bg-primary text-on-primary text-[11px] font-bold">
                                                                            <span class="material-symbols-outlined text-[14px]">download</span>
                                                                            Download
                                                                        </asp:HyperLink>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        </ItemTemplate>
                                                    </asp:Repeater>
                                                </div>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                        <asp:Panel ID="pnlNoUpdates" runat="server" Visible="false" CssClass="text-xs text-outline">No updates yet.</asp:Panel>
                                            </div>
                                        </details>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </section>
                    </div>
                </div>
            </asp:Panel>
        </main>
    </div>
    <script type="text/javascript">
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('.work-type-btn');
            if (!btn) return;
            e.preventDefault();
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
            var focus = panel.querySelector('.github-owner, .work-url-input');
            if (focus) focus.focus();
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
        document.addEventListener('input', function (e) {
            if (!e.target.classList.contains('github-owner') && !e.target.classList.contains('github-repo'))
                return;
            var root = e.target.closest('.work-attach');
            if (!root) return;
            var owner = (root.querySelector('.github-owner') || {}).value || '';
            var repo = (root.querySelector('.github-repo') || {}).value || '';
            var preview = root.querySelector('.github-preview');
            if (!preview) return;
            owner = owner.replace(/^\/+|\/+$/g, '');
            repo = repo.replace(/^\/+|\/+$/g, '');
            if (/github\.com|https?:/i.test(repo))
                preview.textContent = repo;
            else if (owner && repo)
                preview.textContent = 'https://github.com/' + owner + '/' + repo;
            else
                preview.textContent = 'https://github.com/...';
        });
    </script>
</asp:Content>
