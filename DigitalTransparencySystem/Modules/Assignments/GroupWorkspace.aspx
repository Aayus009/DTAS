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
        .task-alloc-details { border: 1px solid rgb(var(--dt-outline-variant-rgb, 197 197 211) / 0.55); }
        .task-alloc-details > summary { list-style: none; }
        .task-alloc-details > summary::-webkit-details-marker { display: none; }
        .task-alloc-details[open] .task-alloc-chevron { transform: rotate(180deg); }
        .task-alloc-details > summary { cursor: pointer; }
        .alloc-members { list-style: none; margin: 0; padding: 0; display: flex; flex-wrap: wrap; gap: 0.5rem; }
        .alloc-members li {
            display: flex; align-items: center; gap: 0.45rem;
            padding: 0.4rem 0.8rem; border-radius: 999px;
            border: 1px solid rgb(var(--dt-outline-variant-rgb, 197 197 211) / 0.8);
            background: rgb(var(--dt-surface-rgb, 255 255 255) / 1);
            font-size: 13px; font-weight: 600; cursor: pointer;
        }
        .alloc-members input { margin: 0; }
        .alloc-members li:has(input:checked) {
            border-color: rgb(var(--dt-primary-rgb, 0 17 66) / 1);
            background: rgb(var(--dt-primary-container-rgb, 0 35 111) / 0.12);
        }
        .invite-search {
            display: flex; align-items: center; gap: 0.5rem;
            padding: 0.35rem 0.5rem 0.35rem 0.85rem;
            border: 1px solid rgb(var(--dt-outline-variant-rgb, 197 197 211) / 0.8);
            border-radius: 0.95rem;
            background: rgb(var(--dt-surface-container-low-rgb, 247 247 250) / 1);
        }
        .invite-search input { flex: 1; border: 0; background: transparent; outline: none; min-width: 0; padding: 0.55rem 0; }
        .invite-result {
            display: flex; align-items: center; gap: 0.75rem;
            padding: 0.7rem 0.75rem; border-radius: 0.85rem;
            border: 1px solid rgb(var(--dt-outline-variant-rgb, 197 197 211) / 0.55);
            background: rgb(var(--dt-surface-rgb, 255 255 255) / 1);
        }
        .invite-avatar {
            width: 2.25rem; height: 2.25rem; border-radius: 999px; flex-shrink: 0;
            display: flex; align-items: center; justify-content: center;
            background: rgb(var(--dt-primary-rgb, 0 17 66) / 1);
            color: #fff; font-size: 11px; font-weight: 800;
        }
        .invite-results { max-height: 18rem; overflow-y: auto; }
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
        .work-type-btn[data-work-type="zip"] .work-type-icon { color: #ef6c00; }
        .work-file-drop {
            display: flex; flex-direction: column; align-items: center; justify-content: center;
            gap: 0.35rem; min-height: 6.5rem; padding: 1rem;
            border: 1.5px dashed rgb(var(--dt-outline-variant-rgb, 197 197 211) / 0.9);
            border-radius: 0.85rem; background: rgb(var(--dt-surface-rgb, 255 255 255) / 1);
            cursor: pointer; text-align: center;
        }
        .work-file-drop:hover { border-color: rgb(var(--dt-primary-rgb, 0 17 66) / 0.5); }
        .work-file-drop input[type="file"] {
            position: absolute;
            left: 0;
            top: 0;
            width: 100%;
            height: 100%;
            opacity: 0;
            cursor: pointer;
        }
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
                        <asp:HyperLink ID="lnkGroupConnect" runat="server" Visible="false"
                            CssClass="inline-flex items-center gap-1 px-4 py-2 border border-outline rounded-xl font-label-md font-bold">
                            <span class="material-symbols-outlined text-[18px]">forum</span>
                            Open Connect
                        </asp:HyperLink>
                        <asp:Button ID="btnFinalize" runat="server" Text="Finalize group" CssClass="btn-primary" Visible="false" OnClick="btnFinalize_Click"
                            OnClientClick="return dtasConfirm(this, 'Finalize this group? Task structure and membership will lock.');" />
                    </div>
                </header>

                <asp:Label ID="lblMessage" runat="server" CssClass="dtas-notice"></asp:Label>

                <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-6">
                    <section class="standard-card rounded-xl p-6 lg:col-span-2">
                        <h3 class="font-title-lg text-title-lg text-primary mb-2">Progress</h3>
                        <p class="font-headline-md text-primary"><asp:Literal ID="litProgress" runat="server"></asp:Literal></p>
                        <div class="h-2 rounded-full bg-surface-container-high mt-3 overflow-hidden">
                            <div id="progressFill" runat="server" class="h-full rounded-full bg-primary" style="width:0%"></div>
                        </div>
                        <p class="text-xs text-on-surface-variant mt-2">Each task counts as it moves: In Progress 50%, Under Review 75%, Completed 100%.</p>
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
                            <h3 class="font-title-lg text-title-lg text-primary mb-1">Allocate task</h3>
                            <p class="text-xs text-on-surface-variant mb-4">Open a task to update status and upload work. Allocated tasks stay collapsed until you expand them.</p>
                            <asp:Panel ID="pnlAddTask" runat="server" Visible="false" CssClass="mb-4 p-4 bg-surface-container-low rounded-xl space-y-3">
                                <p class="font-label-md text-on-surface">Create one or more tasks and assign them to group members.</p>
                                <asp:TextBox ID="txtTaskTitles" runat="server" TextMode="MultiLine" Rows="3"
                                    placeholder="Task titles, one per line"
                                    CssClass="w-full p-3 bg-surface border border-outline rounded-xl font-body-md resize-y"></asp:TextBox>
                                <asp:DropDownList ID="ddlParent" runat="server" CssClass="w-full p-3 bg-surface border border-outline rounded-xl font-body-md"></asp:DropDownList>
                                <div>
                                    <div class="flex items-center justify-between gap-2 mb-2">
                                        <p class="text-xs font-semibold text-on-surface">Assign to</p>
                                        <button type="button" class="text-xs text-primary font-bold bg-transparent border-0 p-0 cursor-pointer" id="btnAllocSelectAll">Select all</button>
                                    </div>
                                    <asp:CheckBoxList ID="cblAssignees" runat="server" RepeatLayout="UnorderedList" CssClass="alloc-members"></asp:CheckBoxList>
                                    <p class="text-xs text-outline mt-2">Select several students to give each of them every title above. Leave empty to create unassigned tasks.</p>
                                </div>
                                <asp:TextBox ID="txtDue" runat="server" TextMode="Date" CssClass="w-full p-3 bg-surface border border-outline rounded-xl font-body-md"></asp:TextBox>
                                <asp:Button ID="btnAddTask" runat="server" Text="Allocate" CssClass="btn-primary" OnClick="btnAddTask_Click" />
                            </asp:Panel>
                            <asp:Repeater ID="rptTasks" runat="server" OnItemCommand="rptTasks_ItemCommand" OnItemDataBound="rptTasks_ItemDataBound">
                                <ItemTemplate>
                                    <details id="taskDetails" runat="server" class='<%# Eval("ParentTaskID") == DBNull.Value ? "task-alloc-details rounded-xl mb-2 bg-surface-container-low" : "task-alloc-details rounded-xl mb-2 ml-6 bg-surface-container-lowest" %>'>
                                        <summary class="flex items-center gap-3 px-3 py-2.5 select-none">
                                            <span class="material-symbols-outlined text-[22px] task-alloc-chevron transition-transform shrink-0 text-primary">expand_more</span>
                                            <div class="min-w-0 flex-1">
                                                <p class="font-semibold truncate"><%# Eval("Title") %></p>
                                                <p class='<%# "text-xs truncate " + TimelineService.DueClass(Eval("DueDate"), Eval("Status")) %>'>
                                                    <%# AssigneeLabel(Eval("AssigneeName")) %> · <%# TimelineService.DueLabel(Eval("DueDate"), Eval("Status")) %>
                                                </p>
                                            </div>
                                            <span class='<%# "shrink-0 " + TimelineService.StatusBadgeClass(Eval("Status"), Eval("DueDate")) %>'><%# TimelineService.StatusLabel(Eval("Status"), Eval("DueDate")) %></span>
                                        </summary>
                                        <div class="px-3 pb-3">
                                        <asp:Panel ID="pnlWork" runat="server" CssClass="mt-1 p-3 bg-surface rounded-lg">
                                            <p class="text-xs font-label-md text-on-surface-variant mb-2">Work files</p>
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
                                            <asp:Label ID="lblNoFiles" runat="server" CssClass="text-xs text-outline block" Text="No files uploaded yet." />
                                        </asp:Panel>
                                        <asp:Panel ID="pnlSubmit" runat="server" Visible="false" CssClass="mt-3 p-3 bg-surface rounded-lg work-attach space-y-3">
            <p class="font-label-md text-on-surface">Add note, files, and links</p>
            <p class="text-xs text-on-surface-variant">Choose a type, pick the file, then click Save at the bottom to keep the upload and status together.</p>
                                            <asp:TextBox ID="txtWorkNote" runat="server" TextMode="MultiLine" Rows="2"
                                                placeholder="Optional note"
                                                CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md resize-none"></asp:TextBox>
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
                                                <button type="button" class="work-type-btn" data-work-type="zip">
                                                    <span class="material-symbols-outlined work-type-icon">folder_zip</span>
                                                    ZIP
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
                                            <div class="work-type-panel space-y-2 p-3 rounded-xl bg-surface-container-low" data-work-panel="zip">
                                                <p class="text-xs font-semibold">Add ZIP</p>
                                                <label class="work-file-drop relative">
                                                    <span class="material-symbols-outlined text-[28px] text-primary">folder_zip</span>
                                                    <span class="text-sm font-semibold">Choose a ZIP archive</span>
                                                    <span class="text-xs text-outline">ZIP only - up to 1000 MB each</span>
                                                    <asp:FileUpload ID="fuZip" runat="server" AllowMultiple="true" accept=".zip,application/zip" />
                                                </label>
                                                <p class="work-file-names text-xs text-on-surface-variant">No file selected</p>
                                            </div>

                                        </asp:Panel>
                                        <div id="pnlTaskActions" runat="server" class="mt-3 flex flex-wrap gap-2 items-center justify-between p-3 rounded-xl bg-surface border border-outline-variant/40">
                                            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="p-2 border border-outline rounded-lg text-sm">
                                                <asp:ListItem Value="ToDo">To Do</asp:ListItem>
                                                <asp:ListItem Value="InProgress">In Progress</asp:ListItem>
                                                <asp:ListItem Value="UnderReview">Under Review</asp:ListItem>
                                                <asp:ListItem Value="Completed">Completed</asp:ListItem>
                                                <asp:ListItem Value="Blocked">Blocked</asp:ListItem>
                                            </asp:DropDownList>
                                            <asp:Button runat="server" CommandName="Save" CommandArgument='<%# Eval("TaskID") %>'
                                                Text="Save" CausesValidation="false"
                                                CssClass="btn-primary"
                                                ToolTip="Saves the status and any file, note, or link you added." />
                                        </div>
                                        <div class="mt-3 pt-3 border-t border-surface-container-high">
                                            <p class="text-xs font-label-md text-on-surface-variant mb-2">Comments</p>
                                            <asp:Repeater ID="rptTaskComments" runat="server">
                                                <ItemTemplate>
                                                    <div class="mb-3">
                                                        <p class="text-xs font-semibold"><%# Eval("FullName") %>
                                                            <span class="font-normal text-outline"><%# Eval("CreatedAt", "{0:MMM dd, HH:mm}") %></span>
                                                        </p>
                                                        <p class="text-sm text-on-surface"><%# EncodeComment(Eval("Comment")) %></p>
                                                    </div>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                            <asp:Label ID="lblNoComments" runat="server" CssClass="text-xs text-outline block mb-2" Text="No comments yet." />
                                            <asp:Panel ID="pnlAddComment" runat="server" Visible="false">
                                                <asp:TextBox ID="txtTaskComment" runat="server" TextMode="MultiLine" Rows="3"
                                                    CssClass="w-full p-2 bg-surface-container-low border border-outline rounded-lg font-body-md mb-2 resize-none"
                                                    placeholder="Comment on this work..."></asp:TextBox>
                                                <asp:Button runat="server" Text="Comment" CssClass="px-3 py-1.5 border border-outline rounded-lg font-label-md font-bold"
                                                    CommandName="Comment" CommandArgument='<%# Eval("TaskID") %>' CausesValidation="false" />
                                            </asp:Panel>
                                        </div>
                                        </div>
                                    </details>
                                </ItemTemplate>
                            </asp:Repeater>
                            <asp:Panel ID="pnlNoTasks" runat="server" Visible="false" CssClass="text-on-surface-variant">No tasks allocated yet.</asp:Panel>
                        </section>

                        <section class="standard-card rounded-xl p-6">
                            <h3 class="font-title-lg text-title-lg text-primary mb-2">Contribution</h3>
                            <p class="text-xs text-on-surface-variant mb-4">50% task progress (In Progress 50%, Under Review 75%, Completed 100%) - 20% timeliness - 20% activity - 10% recency.</p>
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
                        <asp:Panel ID="pnlInvite" runat="server" Visible="false" DefaultButton="btnInviteSearch" CssClass="standard-card rounded-xl p-6">
                            <h3 class="font-title-lg text-title-lg text-primary mb-1">Invite members</h3>
                            <p class="text-xs text-on-surface-variant mb-3">Search by name, email, username, or student ID. Only students not already on this assignment appear.</p>
                            <div class="invite-search mb-3">
                                <span class="material-symbols-outlined text-[20px] text-outline">search</span>
                                <asp:TextBox ID="txtInviteSearch" runat="server" MaxLength="120"
                                    placeholder="Search students"
                                    CssClass="font-body-md invite-search-input"></asp:TextBox>
                                <asp:Button ID="btnInviteSearch" runat="server" Text="Search" CssClass="btn-primary shrink-0 invite-search-btn" OnClick="btnInviteSearch_Click" CausesValidation="false" />
                            </div>
                            <asp:Label ID="lblLookup" runat="server" CssClass="font-label-md block mb-3"></asp:Label>
                            <div class="invite-results space-y-2">
                                <asp:Repeater ID="rptInviteResults" runat="server" OnItemCommand="rptInviteResults_ItemCommand">
                                    <ItemTemplate>
                                        <div class="invite-result">
                                            <span class="invite-avatar"><%# Initials(Eval("FullName")) %></span>
                                            <div class="min-w-0 flex-1">
                                                <p class="font-semibold text-sm truncate"><%# Eval("FullName") %></p>
                                                <p class="text-xs text-on-surface-variant truncate"><%# Eval("Email") %></p>
                                            </div>
                                            <asp:Label runat="server" Visible='<%# Convert.ToString(Eval("InviteStatus")) == "Pending" %>'
                                                CssClass="text-[11px] font-bold text-outline">Pending</asp:Label>
                                            <asp:Button runat="server" Visible='<%# Convert.ToString(Eval("InviteStatus")) != "Pending" %>'
                                                CommandName="Invite" CommandArgument='<%# Eval("UserID") %>'
                                                Text="Invite" CausesValidation="false"
                                                CssClass="btn-primary" />
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                            <asp:Panel ID="pnlInviteEmpty" runat="server" Visible="false" CssClass="text-xs text-outline">No matching students. Try another name or a complete email.</asp:Panel>
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
        (function () {
            var selectAll = document.getElementById('btnAllocSelectAll');
            if (selectAll) {
                selectAll.addEventListener('click', function () {
                    var boxes = document.querySelectorAll('.alloc-members input[type="checkbox"]');
                    var allOn = true;
                    for (var i = 0; i < boxes.length; i++) {
                        if (!boxes[i].checked) { allOn = false; break; }
                    }
                    for (var j = 0; j < boxes.length; j++)
                        boxes[j].checked = !allOn;
                    selectAll.textContent = allOn ? 'Select all' : 'Clear';
                });
            }
        })();
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
