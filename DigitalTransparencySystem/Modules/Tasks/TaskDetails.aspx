<%@ Page Title="Task Details | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="TaskDetails.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Tasks.TaskDetails" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <asp:Panel ID="pnlAdminLayout" runat="server">
        <uc:AdminTopbar runat="server" />
    </asp:Panel>
    <asp:Panel ID="pnlUserLayout" runat="server" Visible="false">
        <uc:UserTopbar runat="server" />
    </asp:Panel>

    <div class="flex min-h-screen">

        <asp:Panel ID="pnlAdminSidebar" runat="server">
            <uc:AdminSidebar ActivePage="Tasks" runat="server" />
        </asp:Panel>
        <asp:Panel ID="pnlUserSidebar" runat="server" Visible="false">
            <uc:UserSidebar ActivePage="Tasks" runat="server" />
        </asp:Panel>

        <!-- Main Content Area -->
        <main class="dashboard-main flex-1 ml-64 p-8">

            <!-- Breadcrumb -->
            <nav class="flex items-center gap-2 mb-6 font-label-md text-label-md text-on-surface-variant">
                <asp:HyperLink ID="lnkBreadcrumbDashboard" runat="server" CssClass="hover:text-primary transition-colors">Dashboard</asp:HyperLink>
                <span class="material-symbols-outlined text-[16px]">chevron_right</span>
                <asp:HyperLink ID="lnkBreadcrumbTasks" runat="server" CssClass="hover:text-primary transition-colors">Tasks</asp:HyperLink>
                <span class="material-symbols-outlined text-[16px]">chevron_right</span>
                <span class="text-primary font-semibold">Task Details</span>
            </nav>

            <!-- Header Section -->
            <header class="flex justify-between items-start mb-8">
                <div>
                    <div class="flex items-center gap-4 mb-3">
                        <asp:Literal ID="litTaskTitle" runat="server" Text="Task Title"></asp:Literal>
                        <asp:Literal ID="litStatusBadge" runat="server"></asp:Literal>
                        <asp:Literal ID="litPriorityBadge" runat="server"></asp:Literal>
                    </div>
                    <div class="flex items-center gap-3">
                        <span class="font-body-md text-body-md text-on-surface-variant flex items-center gap-1">
                            <span class="material-symbols-outlined text-[16px] align-middle">calendar_today</span>
                            Due <asp:Literal ID="litDueDate" runat="server"></asp:Literal>
                        </span>
                        <span class="text-on-surface-variant">|</span>
                        <span class="font-body-md text-body-md text-on-surface-variant flex items-center gap-1">
                            <span class="material-symbols-outlined text-[16px] align-middle">schedule</span>
                            Created <asp:Literal ID="litCreatedAt" runat="server"></asp:Literal>
                        </span>
                    </div>
                </div>
                <div class="flex gap-3">
                    <asp:HyperLink ID="lnkEditTask" runat="server" CssClass="flex items-center gap-2 px-5 py-2 bg-primary text-on-primary rounded-lg font-label-md text-label-md font-bold hover:scale-[1.02] active:scale-95 transition-transform shadow-sm">
                        <span class="material-symbols-outlined text-[18px]">edit</span>
                        Edit Task
                    </asp:HyperLink>
                    <asp:Button ID="btnBackToList" runat="server" Text="Back to Tasks" CssClass="py-2 px-5 border border-outline text-on-surface-variant rounded-lg font-label-md text-label-md font-bold hover:bg-surface-container-low transition-colors cursor-pointer" OnClick="btnBackToList_Click" />
                </div>
            </header>

            <asp:Panel ID="pnlDeadlineRisk" runat="server" Visible="false" CssClass="mb-6 p-4 rounded-xl border border-outline-variant/40">
                <div class="flex flex-wrap items-start justify-between gap-4">
                    <div>
                        <span id="spanRiskBadge" runat="server" class="inline-block px-2.5 py-1 rounded-full font-badge-cap text-badge-cap mb-2"></span>
                        <h3 class="font-title-lg text-title-lg text-primary mb-1"><asp:Literal ID="litRiskTitle" runat="server"></asp:Literal></h3>
                        <p class="font-body-md text-on-surface-variant"><asp:Literal ID="litRiskMessage" runat="server"></asp:Literal></p>
                    </div>
                    <asp:Panel ID="pnlDeadlineActions" runat="server" Visible="false" CssClass="space-y-2 min-w-[16rem]">
                        <label class="font-label-md text-on-surface-variant block">Extend due date</label>
                        <asp:TextBox ID="txtExtendDue" runat="server" TextMode="Date"
                            CssClass="w-full p-2 border border-outline rounded-lg font-body-md"></asp:TextBox>
                        <asp:Button ID="btnExtendDue" runat="server" Text="Extend due date" CssClass="w-full py-2 px-4 bg-primary text-on-primary rounded-lg font-label-md font-bold" OnClick="btnExtendDue_Click" />
                        <asp:HyperLink ID="lnkRedistribute" runat="server"
                            CssClass="inline-flex items-center justify-center gap-1 w-full py-2 px-4 border border-outline rounded-lg font-label-md font-bold">
                            <span class="material-symbols-outlined text-[18px]">group_add</span>
                            Redistribute / add members
                        </asp:HyperLink>
                    </asp:Panel>
                </div>
            </asp:Panel>

            <!-- Error Message -->
            <asp:Panel ID="pnlError" runat="server" CssClass="mb-6 p-4 bg-error-container border border-error/30 rounded-xl flex items-center gap-3" Visible="false">
                <span class="material-symbols-outlined text-error">error</span>
                <asp:Label ID="lblError" runat="server" CssClass="font-label-md text-label-md text-on-error-container"></asp:Label>
            </asp:Panel>

            <!-- Main Layout: 2 Column Asymmetric -->
            <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">

                <!-- Left Column: Details & Updates (Wide) -->
                <div class="lg:col-span-2 space-y-6">

                    <!-- Task Overview -->
                    <section class="standard-card rounded-xl overflow-hidden">
                        <div class="px-6 py-4 border-b border-surface-container-high">
                            <h3 class="font-title-lg text-title-lg text-primary">Task Overview</h3>
                        </div>
                        <div class="p-6">
                            <div class="mb-6">
                                <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-2">Description</span>
                                <asp:Literal ID="litDescription" runat="server" Text="No description provided."></asp:Literal>
                            </div>
                            <div class="grid grid-cols-2 gap-6">
                                <div>
                                    <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-2">Created By</span>
                                    <p class="font-body-md text-body-md flex items-center gap-2">
                                        <span class="material-symbols-outlined text-[18px] text-primary">person</span>
                                        <asp:Literal ID="litCreatedBy" runat="server"></asp:Literal>
                                    </p>
                                </div>
                                <div>
                                    <span class="font-badge-cap text-badge-cap uppercase text-outline tracking-widest block mb-2">Last Updated</span>
                                    <p class="font-body-md text-body-md flex items-center gap-2">
                                        <span class="material-symbols-outlined text-[18px] text-primary">update</span>
                                        <asp:Literal ID="litUpdatedAt" runat="server"></asp:Literal>
                                    </p>
                                </div>
                            </div>
                        </div>
                    </section>

                    <asp:Panel ID="pnlRestricted" runat="server" Visible="false" CssClass="mb-6 p-4 rounded-xl bg-error-container/20 text-error font-label-md">
                        This task is restricted. Assigned members cannot work on it until the system administrator restores it.
                    </asp:Panel>
                    <asp:Panel ID="pnlStatusUpdate" runat="server">
                    <!-- Status Update Section -->
                    <section class="standard-card rounded-xl overflow-hidden">
                        <div class="px-6 py-4 border-b border-surface-container-high">
                            <h3 class="font-title-lg text-title-lg text-primary">Update Status</h3>
                        </div>
                        <div class="p-6 space-y-4">
                            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                                <div>
                                    <label class="block font-label-md text-label-md text-on-surface-variant mb-2">New Status</label>
                                    <asp:DropDownList ID="ddlNewStatus" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none">
                                        <asp:ListItem Text="Select Status" Value="" />
                                        <asp:ListItem Text="Pending" Value="Pending" />
                                        <asp:ListItem Text="In Progress" Value="InProgress" />
                                        <asp:ListItem Text="Submitted" Value="Submitted" />
                                        <asp:ListItem Text="Under Review" Value="UnderReview" />
                                        <asp:ListItem Text="Revision Needed" Value="RevisionNeeded" />
                                        <asp:ListItem Text="Approved" Value="Approved" />
                                        <asp:ListItem Text="Completed" Value="Completed" />
                                        <asp:ListItem Text="Delayed" Value="Delayed" />
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="rfvNewStatus" runat="server" ControlToValidate="ddlNewStatus" ErrorMessage="Please select a status." CssClass="text-error text-sm mt-1 block" Display="Dynamic" ValidationGroup="UpdateStatus" InitialValue=""></asp:RequiredFieldValidator>
                                </div>
                            </div>
                            <div>
                                <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Comment</label>
                                <asp:TextBox ID="txtStatusComment" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="Add a comment about this status change..." TextMode="MultiLine" Rows="3"></asp:TextBox>
                            </div>
                            <div>
                                <asp:Button ID="btnUpdateStatus" runat="server" Text="Submit Status Update" CssClass="px-6 py-3 bg-primary text-on-primary rounded-xl font-label-md text-label-md font-bold active:scale-95 transition-transform cursor-pointer" ValidationGroup="UpdateStatus" OnClick="btnUpdateStatus_Click" />
                            </div>
                        </div>
                    </section>
                    </asp:Panel>

                    <!-- Update History -->
                    <section class="standard-card rounded-xl overflow-hidden">
                        <div class="px-6 py-4 border-b border-surface-container-high flex justify-between items-center">
                            <h3 class="font-title-lg text-title-lg text-primary">Update History</h3>
                            <asp:Literal ID="litUpdateCount" runat="server"></asp:Literal>
                        </div>
                        <div class="p-6">
                            <div class="space-y-6 relative before:absolute before:left-5 before:top-2 before:bottom-2 before:w-[1px] before:bg-outline-variant/30">
                                <asp:Repeater ID="rptUpdates" runat="server">
                                    <ItemTemplate>
                                        <div class="relative pl-12">
                                            <div class="absolute left-2 top-1.5 w-5 h-5 rounded-full ring-4 ring-surface flex items-center justify-center" style='<%# Eval("DotStyle") %>'>
                                                <span class="material-symbols-outlined text-[14px]" style='<%# Eval("IconStyle") %>'><%# Eval("Icon") %></span>
                                            </div>
                                            <div class="flex items-start justify-between">
                                                <div>
                                                    <p class="font-label-md text-label-md font-bold"><%# Eval("UserName") %></p>
                                                    <p class="text-xs text-on-surface-variant mb-1"><%# Eval("UpdatedAt", "{0:MMM dd, yyyy - hh:mm tt}") %></p>
                                                    <div class="flex items-center gap-2 mb-2">
                                                        <span class="font-badge-cap text-badge-cap px-2 py-0.5 rounded" style='<%# Eval("OldStatusStyle") %>'><%# Eval("OldStatus") %></span>
                                                        <span class="material-symbols-outlined text-[14px] text-outline">arrow_forward</span>
                                                        <span class="font-badge-cap text-badge-cap px-2 py-0.5 rounded" style='<%# Eval("NewStatusStyle") %>'><%# Eval("NewStatus") %></span>
                                                    </div>
                                                    <p class="text-sm text-on-surface-variant italic"><%# Eval("Comment") %></p>
                                                </div>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                            <asp:Panel ID="pnlNoUpdates" runat="server" CssClass="p-8 text-center" Visible="false">
                                <span class="material-symbols-outlined text-[40px] text-outline-variant block mb-2">history</span>
                                <p class="font-body-md text-body-md text-on-surface-variant">No status updates recorded yet.</p>
                            </asp:Panel>
                        </div>
                    </section>

                    <!-- Attachments -->
                    <section class="standard-card rounded-xl overflow-hidden">
                        <div class="px-6 py-4 border-b border-surface-container-high flex justify-between items-center">
                            <h3 class="font-title-lg text-title-lg text-primary">Attachments</h3>
                            <asp:Literal ID="litAttachmentCount" runat="server"></asp:Literal>
                        </div>
                        <div class="p-6">
                            <asp:Panel ID="pnlUploadAttachment" runat="server">
                            <!-- Upload Form -->
                            <div class="mb-6 p-4 bg-surface-container-low rounded-lg border border-dashed border-outline-variant">
                                <div class="flex items-center gap-4">
                                    <asp:FileUpload ID="fuAttachment" runat="server" CssClass="flex-1 text-sm text-on-surface-variant file:mr-4 file:py-2 file:px-4 file:rounded-lg file:border-0 file:text-sm file:font-bold file:bg-primary-container file:text-on-primary-container hover:file:bg-primary/10 cursor-pointer" />
                                    <asp:Button ID="btnUpload" runat="server" Text="Upload" CssClass="px-5 py-2 bg-primary text-on-primary rounded-lg font-label-md text-label-md font-bold active:scale-95 transition-transform cursor-pointer" OnClick="btnUpload_Click" />
                                </div>
                                <p class="text-xs text-outline mt-2">Supported: PDF, Images, Documents (Max 10MB)</p>
                            </div>
                            </asp:Panel>
                            <asp:Panel ID="pnlAdminNoUpload" runat="server" Visible="false" CssClass="mb-6 p-3 rounded-lg bg-surface-container-low text-sm text-on-surface-variant">
                                System admin can review files here but cannot upload. Use a tagged comment to reach a specific member.
                            </asp:Panel>
                            <!-- File List -->
                            <div class="space-y-3">
                                <asp:Repeater ID="rptAttachments" runat="server" OnItemCommand="rptAttachments_ItemCommand" OnItemDataBound="rptAttachments_ItemDataBound">
                                    <ItemTemplate>
                                        <div class="flex items-center gap-3 p-3 bg-surface-container-low rounded-lg border border-outline-variant/30 hover:bg-surface-container-high transition-colors">
                                            <div class="h-10 w-10 rounded-lg bg-primary-container flex items-center justify-center shrink-0">
                                                <span class="material-symbols-outlined text-on-primary-container text-[20px]"><%# Eval("Icon") %></span>
                                            </div>
                                            <div class="flex-1 min-w-0">
                                                <p class="font-label-md text-label-md font-bold truncate"><%# Eval("FileName") %></p>
                                                <p class="text-xs text-on-surface-variant"><%# Eval("FileSizeText") %> &middot; <%# Eval("UploadedAt", "{0:MMM dd, yyyy}") %></p>
                                            </div>
                                            <asp:LinkButton ID="btnDownload" runat="server" CommandName="Download" CommandArgument='<%# Eval("AttachmentID") %>'
                                                CssClass="p-2 hover:bg-surface-container-low rounded-lg transition-colors" ToolTip="Download">
                                                <span class="material-symbols-outlined text-outline hover:text-primary transition-colors text-[20px]">download</span>
                                            </asp:LinkButton>
                                            <asp:Panel ID="pnlRemoveAttachment" runat="server">
                                                <asp:LinkButton ID="btnRemoveAttachment" runat="server" CommandName="RemoveAttachment" CommandArgument='<%# Eval("AttachmentID") %>'
                                                    CssClass="p-2 hover:bg-error-container/50 rounded-lg transition-colors" ToolTip="Remove"
                                                    OnClientClick="return dtasConfirm(this, 'Remove this attachment?');">
                                                    <span class="material-symbols-outlined text-outline hover:text-error transition-colors text-[20px]">close</span>
                                                </asp:LinkButton>
                                            </asp:Panel>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                            <asp:Panel ID="pnlNoAttachments" runat="server" CssClass="p-6 text-center" Visible="false">
                                <span class="material-symbols-outlined text-[40px] text-outline-variant block mb-2">attach_file</span>
                                <p class="font-body-md text-body-md text-on-surface-variant">No files attached yet.</p>
                            </asp:Panel>
                        </div>
                    </section>

                    <!-- Discussion / Comments -->
                    <section class="standard-card rounded-xl overflow-hidden">
                        <div class="px-6 py-4 border-b border-surface-container-high flex justify-between items-center">
                            <h3 class="font-title-lg text-title-lg text-primary">Discussion</h3>
                            <asp:Literal ID="litCommentCount" runat="server"></asp:Literal>
                        </div>
                        <div class="p-6">
                            <!-- Add Comment Form -->
                            <div class="mb-6 space-y-3">
                                <asp:TextBox ID="txtNewComment" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="Write a comment for this task..." TextMode="MultiLine" Rows="3"></asp:TextBox>
                                <div>
                                    <label class="block font-label-md text-label-md text-on-surface-variant mb-2">Tag a member</label>
                                    <asp:DropDownList ID="ddlTagUser" runat="server" CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all appearance-none">
                                        <asp:ListItem Text="Everyone on this task" Value="" />
                                    </asp:DropDownList>
                                    <p class="text-xs text-outline mt-2">Tag a specific user to send this comment to them and their event task.</p>
                                </div>
                                <asp:Button ID="btnAddComment" runat="server" Text="Post Comment" CssClass="px-6 py-2.5 bg-primary text-on-primary rounded-lg font-label-md text-label-md font-bold active:scale-95 transition-transform cursor-pointer" OnClick="btnAddComment_Click" />
                            </div>
                            <!-- Comments List -->
                            <div class="space-y-4">
                                <asp:Repeater ID="rptComments" runat="server" OnItemCommand="rptComments_ItemCommand">
                                    <ItemTemplate>
                                        <div class="p-4 bg-surface-container-low rounded-lg border border-outline-variant/30">
                                            <div class="flex items-center gap-3 mb-2">
                                                <div class="h-8 w-8 rounded-full bg-primary-container flex items-center justify-center shrink-0">
                                                    <span class="material-symbols-outlined text-on-primary-container text-[16px]">person</span>
                                                </div>
                                                <div>
                                                    <p class="font-label-md text-label-md font-bold"><%# Eval("UserName") %></p>
                                                    <p class="text-xs text-on-surface-variant"><%# Eval("CreatedAt", "{0:MMM dd, yyyy - hh:mm tt}") %></p>
                                                </div>
                                            </div>
                                            <asp:Panel ID="pnlTaggedUser" runat="server" Visible='<%# Convert.ToBoolean(Eval("HasMention")) %>' CssClass="ml-11 mb-2">
                                                <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-primary-container text-on-primary-container font-badge-cap text-badge-cap">
                                                    <span class="material-symbols-outlined text-[14px]">alternate_email</span>
                                                    <%# Eval("MentionedName") %>
                                                </span>
                                            </asp:Panel>
                                            <p class="font-body-md text-body-md text-on-surface ml-11"><%# Eval("CommentHtml") %></p>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                            <asp:Panel ID="pnlNoComments" runat="server" CssClass="p-6 text-center" Visible="false">
                                <span class="material-symbols-outlined text-[40px] text-outline-variant block mb-2">chat_bubble_outline</span>
                                <p class="font-body-md text-body-md text-on-surface-variant">No comments yet. Start the discussion!</p>
                            </asp:Panel>
                        </div>
                    </section>

                    <!-- Checklist / Sub-tasks -->
                    <section class="standard-card rounded-xl overflow-hidden">
                        <div class="px-6 py-4 border-b border-surface-container-high flex justify-between items-center">
                            <h3 class="font-title-lg text-title-lg text-primary">Checklist</h3>
                            <asp:Literal ID="litChecklistProgress" runat="server"></asp:Literal>
                        </div>
                        <div class="p-6">
                            <asp:Panel ID="pnlChecklistEdit" runat="server">
                            <!-- Add Item Form -->
                            <div class="mb-6 flex gap-3">
                                <asp:TextBox ID="txtNewChecklistItem" runat="server" CssClass="flex-1 px-4 py-3 bg-surface-container-low border border-outline rounded-lg font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" placeholder="Add a checklist item..." MaxLength="300"></asp:TextBox>
                                <asp:Button ID="btnAddChecklistItem" runat="server" Text="Add" CssClass="px-5 py-3 bg-primary text-on-primary rounded-lg font-label-md text-label-md font-bold active:scale-95 transition-transform cursor-pointer" OnClick="btnAddChecklistItem_Click" />
                            </div>
                            </asp:Panel>
                            <!-- Progress Bar -->
                            <asp:Panel ID="pnlProgressBar" runat="server" Visible="false" class="mb-4">
                                <div class="flex items-center gap-3 mb-2">
                                    <div class="flex-1 h-2 bg-surface-container-high rounded-full overflow-hidden">
                                        <asp:Literal ID="litProgressBar" runat="server"></asp:Literal>
                                    </div>
                                    <span class="font-label-md text-label-md text-on-surface-variant whitespace-nowrap"><asp:Literal ID="litProgressText" runat="server"></asp:Literal></span>
                                </div>
                            </asp:Panel>
                            <!-- Checklist Items -->
                            <div class="space-y-2">
                                <asp:Repeater ID="rptChecklist" runat="server" OnItemCommand="rptChecklist_ItemCommand" OnItemDataBound="rptChecklist_ItemDataBound">
                                    <ItemTemplate>
                                        <div class="flex items-center gap-3 p-3 bg-surface-container-low rounded-lg border border-outline-variant/30 hover:bg-surface-container-high transition-colors">
                                            <asp:LinkButton ID="btnToggleCheck" runat="server" CommandName="ToggleCheck" CommandArgument='<%# Eval("ItemID") %>'
                                                CssClass="shrink-0">
                                                <span class="material-symbols-outlined text-[22px]" style='<%# Eval("CheckStyle") %>'><%# Eval("CheckIcon") %></span>
                                            </asp:LinkButton>
                                            <span class="flex-1 font-body-md text-body-md" style='<%# Eval("TextStyle") %>'><%# Eval("ItemText") %></span>
                                            <asp:Panel ID="pnlDeleteChecklist" runat="server">
                                                <asp:LinkButton ID="btnDeleteItem" runat="server" CommandName="DeleteItem" CommandArgument='<%# Eval("ItemID") %>'
                                                    CssClass="p-1 hover:bg-error-container/50 rounded-lg transition-colors opacity-0 group-hover:opacity-100"
                                                    OnClientClick="return dtasConfirm(this, 'Delete this item?');">
                                                    <span class="material-symbols-outlined text-outline hover:text-error transition-colors text-[18px]">close</span>
                                                </asp:LinkButton>
                                            </asp:Panel>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                            <asp:Panel ID="pnlNoChecklist" runat="server" CssClass="p-6 text-center" Visible="false">
                                <span class="material-symbols-outlined text-[40px] text-outline-variant block mb-2">checklist</span>
                                <p class="font-body-md text-body-md text-on-surface-variant">No checklist items yet.</p>
                            </asp:Panel>
                        </div>
                    </section>
                </div>

                <!-- Right Column: Assigned Users & Related Items -->
                <div class="lg:col-span-1 space-y-6">

                    <asp:Panel ID="pnlEventTeamProgress" runat="server" Visible="false">
                        <section class="standard-card rounded-xl p-6" id="eventTeamProgressRoot"
                            data-api="<%= ResolveUrl("~/Modules/Tasks/TaskProgressApi.ashx") %>"
                            data-task-id="<%= Request.QueryString["TaskID"] %>">
                            <div class="flex items-center justify-between mb-2">
                                <h3 class="font-title-lg text-title-lg text-primary">Event team progress</h3>
                                <span id="eventProgressLabel" class="font-label-md text-label-md text-on-surface-variant whitespace-nowrap"><asp:Literal ID="litEventProgressPercent" runat="server"></asp:Literal></span>
                            </div>
                            <p class="text-sm text-on-surface-variant mb-3"><asp:Literal ID="litEventProgressEvent" runat="server"></asp:Literal></p>
                            <div class="flex items-center gap-3 mb-2">
                                <div class="flex-1 h-3 bg-surface-container-high rounded-full overflow-hidden">
                                    <div id="eventProgressFill" runat="server" ClientIDMode="Static" class="h-full bg-[#2e7d32] rounded-full transition-all duration-500" style="width: 0%"></div>
                                </div>
                            </div>
                            <p id="eventProgressMeta" class="text-xs text-outline mb-4"><asp:Literal ID="litEventProgressMeta" runat="server"></asp:Literal></p>
                            <div id="eventMemberProgress">
                                <asp:Repeater ID="rptMemberProgress" runat="server">
                                    <ItemTemplate>
                                        <div class="mb-3">
                                            <div class="flex justify-between text-xs text-on-surface-variant mb-1">
                                                <span class="font-semibold text-on-surface"><%# Eval("FullName") %></span>
                                                <span><%# Eval("CompletedTasks") %>/<%# Eval("AssignedTasks") %></span>
                                            </div>
                                            <div class="h-1.5 bg-surface-container-high rounded-full overflow-hidden">
                                                <div class="h-full bg-[#2e7d32] rounded-full" style='width: <%# Eval("Percent") %>%'></div>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <asp:Panel ID="pnlNoMemberProgress" runat="server" Visible="false">
                                    <p class="text-sm text-on-surface-variant">No members have event tasks yet.</p>
                                </asp:Panel>
                            </div>
                        </section>
                    </asp:Panel>

                    <!-- Team Progress (visible when task has a team) -->
                    <asp:Panel ID="pnlTeamProgress" runat="server" Visible="false">
                        <section class="standard-card rounded-xl p-6">
                            <div class="flex items-center justify-between mb-4">
                                <h3 class="font-title-lg text-title-lg text-primary">Team Progress</h3>
                                <asp:HyperLink ID="lnkManageTeam" runat="server" CssClass="flex items-center gap-1 px-3 py-1.5 bg-primary-container text-on-primary-container rounded-lg font-label-sm text-label-sm font-bold hover:bg-primary/10 transition-colors">
                                    <span class="material-symbols-outlined text-[16px]">group</span>
                                    Manage Team
                                </asp:HyperLink>
                            </div>
                            <div class="flex items-center gap-3 mb-2">
                                <div class="flex-1 h-3 bg-surface-container-high rounded-full overflow-hidden">
                                    <asp:Literal ID="litTeamProgress" runat="server"></asp:Literal>
                                </div>
                                <span class="font-label-md text-label-md text-on-surface-variant whitespace-nowrap"><asp:Literal ID="litTeamProgressText" runat="server"></asp:Literal></span>
                            </div>
                            <asp:Literal ID="litLeaderInfo" runat="server"></asp:Literal>
                        </section>
                    </asp:Panel>

                    <!-- Team Invite (visible to invited users who haven't responded) -->
                    <asp:Panel ID="pnlTeamInvite" runat="server" Visible="false">
                        <section class="standard-card rounded-xl p-6 border-2 border-[rgba(230,126,0,0.3)]">
                            <div class="flex items-center gap-3 mb-4">
                                <span class="material-symbols-outlined text-[28px] text-[#e67e00]">group_add</span>
                                <div>
                                    <h3 class="font-title-lg text-title-lg text-[#e67e00]">Team Invitation</h3>
                                    <p class="text-sm text-on-surface-variant">You've been invited to this task team.</p>
                                </div>
                            </div>
                            <div class="flex gap-3">
                                <asp:Button ID="btnAcceptInvite" runat="server" Text="Accept" CssClass="flex-1 px-4 py-2.5 bg-[#2e7d32] text-white rounded-lg font-label-md text-label-md font-bold active:scale-95 transition-transform cursor-pointer" OnClick="btnAcceptInvite_Click" />
                                <asp:Button ID="btnDeclineInvite" runat="server" Text="Decline" CssClass="flex-1 px-4 py-2.5 bg-[#c62828] text-white rounded-lg font-label-md text-label-md font-bold active:scale-95 transition-transform cursor-pointer" OnClick="btnDeclineInvite_Click" />
                            </div>
                        </section>
                    </asp:Panel>

                    <!-- Assigned Users -->
                    <section class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-6">Assigned Users</h3>
                        <div class="space-y-4">
                            <asp:Repeater ID="rptAssignedUsers" runat="server">
                                <ItemTemplate>
                                    <div class="flex items-center gap-3 p-3 bg-surface-container-low rounded-lg border border-outline-variant/30">
                                        <div class="h-10 w-10 rounded-full bg-primary-container flex items-center justify-center shrink-0">
                                            <span class="material-symbols-outlined text-on-primary-container text-[20px]">person</span>
                                        </div>
                                        <div>
                                            <p class="font-label-md text-label-md font-bold"><%# Eval("FullName") %></p>
                                            <p class="text-xs text-on-surface-variant"><%# Eval("Role") %></p>
                                        </div>
                                        <span class="ml-auto text-xs text-on-surface-variant"><%# Eval("AssignedAt", "{0:MMM dd}") %></span>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            <asp:Panel ID="pnlNoAssignees" runat="server" CssClass="p-6 text-center" Visible="false">
                                <span class="material-symbols-outlined text-[40px] text-outline-variant block mb-2">person_off</span>
                                <p class="font-body-md text-body-md text-on-surface-variant">No users assigned to this task.</p>
                            </asp:Panel>
                        </div>
                    </section>

                    <!-- Related Items -->
                    <section class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-6">Related Items</h3>
                        <div class="space-y-4">
                            <asp:Panel ID="pnlDecision" runat="server" Visible="false">
                                <div class="p-4 bg-surface-container-low rounded-lg border border-outline-variant/30">
                                    <div class="flex items-center gap-3">
                                        <span class="material-symbols-outlined text-primary">gavel</span>
                                        <div>
                                            <p class="font-badge-cap text-badge-cap uppercase text-outline mb-1">Decision</p>
                                            <asp:HyperLink ID="lnkDecision" runat="server" CssClass="font-label-md text-label-md font-bold text-primary hover:underline"></asp:HyperLink>
                                        </div>
                                    </div>
                                </div>
                            </asp:Panel>
                            <asp:Panel ID="pnlEvent" runat="server" Visible="false">
                                <div class="p-4 bg-surface-container-low rounded-lg border border-outline-variant/30">
                                    <div class="flex items-center gap-3">
                                        <span class="material-symbols-outlined text-primary">calendar_today</span>
                                        <div>
                                            <p class="font-badge-cap text-badge-cap uppercase text-outline mb-1">Event</p>
                                            <asp:HyperLink ID="lnkEvent" runat="server" CssClass="font-label-md text-label-md font-bold text-primary hover:underline"></asp:HyperLink>
                                        </div>
                                    </div>
                                </div>
                            </asp:Panel>
                            <asp:Panel ID="pnlMeeting" runat="server" Visible="false">
                                <div class="p-4 bg-surface-container-low rounded-lg border border-outline-variant/30">
                                    <div class="flex items-center gap-3">
                                        <span class="material-symbols-outlined text-primary">groups</span>
                                        <div>
                                            <p class="font-badge-cap text-badge-cap uppercase text-outline mb-1">Meeting</p>
                                            <asp:HyperLink ID="lnkMeeting" runat="server" CssClass="font-label-md text-label-md font-bold text-primary hover:underline"></asp:HyperLink>
                                        </div>
                                    </div>
                                </div>
                            </asp:Panel>
                            <asp:Panel ID="pnlNoRelated" runat="server" CssClass="p-6 text-center" Visible="false">
                                <span class="material-symbols-outlined text-[40px] text-outline-variant block mb-2">link_off</span>
                                <p class="font-body-md text-body-md text-on-surface-variant">No related items linked to this task.</p>
                            </asp:Panel>
                        </div>
                    </section>
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
    <script type="text/javascript">
        (function () {
            var root = document.getElementById('eventTeamProgressRoot');
            if (!root) return;
            var api = root.getAttribute('data-api');
            var taskId = root.getAttribute('data-task-id');
            function escapeHtml(value) {
                return String(value || '')
                    .replace(/&/g, '&amp;')
                    .replace(/</g, '&lt;')
                    .replace(/>/g, '&gt;')
                    .replace(/"/g, '&quot;');
            }
            function render(data) {
                if (!data || !data.ok) return;
                var fill = document.getElementById('eventProgressFill');
                var label = document.getElementById('eventProgressLabel');
                var meta = document.getElementById('eventProgressMeta');
                var list = document.getElementById('eventMemberProgress');
                var percent = data.percent || 0;
                if (fill) fill.style.width = percent + '%';
                if (label) label.textContent = percent + '%';
                if (meta) meta.textContent = (data.completed || 0) + ' of ' + (data.total || 0) + ' event tasks completed';
                if (!list) return;
                var html = '';
                var members = data.members || [];
                for (var i = 0; i < members.length; i++) {
                    var member = members[i];
                    html += '<div class="mb-3">' +
                        '<div class="flex justify-between text-xs text-on-surface-variant mb-1">' +
                        '<span class="font-semibold text-on-surface">' + escapeHtml(member.name) + '</span>' +
                        '<span>' + member.completed + '/' + member.assigned + '</span></div>' +
                        '<div class="h-1.5 bg-surface-container-high rounded-full overflow-hidden">' +
                        '<div class="h-full bg-[#2e7d32] rounded-full" style="width:' + (member.percent || 0) + '%"></div></div></div>';
                }
                if (!html) html = '<p class="text-sm text-on-surface-variant">No members have event tasks yet.</p>';
                list.innerHTML = html;
            }
            function poll() {
                var xhr = new XMLHttpRequest();
                xhr.open('GET', api + '?taskId=' + encodeURIComponent(taskId), true);
                xhr.onreadystatechange = function () {
                    if (xhr.readyState === 4 && xhr.status === 200) {
                        try { render(JSON.parse(xhr.responseText)); } catch (err) { }
                    }
                };
                xhr.send();
            }
            setInterval(poll, 8000);
        })();
    </script>
</asp:Content>