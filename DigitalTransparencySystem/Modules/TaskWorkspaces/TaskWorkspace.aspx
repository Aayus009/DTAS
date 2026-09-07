<%@ Page Title="Task Workspace | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TaskWorkspace.aspx.cs" Inherits="DigitalTransparencySystem.Modules.TaskWorkspaces.TaskWorkspace" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <uc:UserTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:UserSidebar ActivePage="Workspaces" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            <a href="<%= ResolveUrl("~/Modules/TaskWorkspaces/TaskWorkspaces.aspx") %>" class="inline-flex items-center gap-1 text-primary font-bold text-sm mb-3">
                <span class="material-symbols-outlined text-[20px]">arrow_back</span>
                Back
            </a>

            <asp:Panel ID="pnlInternal" runat="server">

                <!-- Task Header -->
                <section class="standard-card rounded-xl p-6 mb-6">
                    <div class="flex justify-between items-start mb-4">
                        <div>
                            <h1 class="font-headline-lg text-headline-lg text-primary mb-2"><asp:Literal ID="litTaskTitle" runat="server"></asp:Literal></h1>
                            <div class="flex gap-2 flex-wrap">
                                <span id="spanStatus" runat="server" class=""></span>
                                <span id="spanPriority" runat="server" class=""></span>
                            </div>
                        </div>
                        <asp:HyperLink ID="lnkScheduleMeeting" runat="server"
                            CssClass="inline-flex items-center gap-1 px-4 py-2 bg-primary text-on-primary rounded-xl font-label-md font-bold">
                            <span class="material-symbols-outlined text-[18px]">videocam</span>
                            Schedule meeting
                        </asp:HyperLink>
                    </div>
                    <asp:Panel ID="pnlViewOnly" runat="server" Visible="false" CssClass="mb-4 p-4 rounded-xl bg-surface-container-low">
                        <p class="font-label-md text-on-surface">This assignment is visible to the event. You can see who is responsible, but only the assigned members can update the work.</p>
                    </asp:Panel>
                    <p class="text-body-md text-on-surface"><asp:Literal ID="litDescription" runat="server"></asp:Literal></p>
                    <asp:Panel ID="pnlDeadlineRisk" runat="server" Visible="false" CssClass="mt-4 p-4 rounded-xl bg-surface-container-low">
                        <span id="spanRiskBadge" runat="server" class="inline-block px-2.5 py-1 rounded-full font-badge-cap text-badge-cap mb-2"></span>
                        <p class="font-semibold text-on-surface mb-1"><asp:Literal ID="litRiskTitle" runat="server"></asp:Literal></p>
                        <p class="text-sm text-on-surface-variant mb-3"><asp:Literal ID="litRiskMessage" runat="server"></asp:Literal></p>
                        <asp:Panel ID="pnlDeadlineActions" runat="server" Visible="false" CssClass="flex flex-wrap gap-2 items-end">
                            <asp:TextBox ID="txtExtendDue" runat="server" TextMode="Date"
                                CssClass="p-2 border border-outline rounded-lg font-body-md"></asp:TextBox>
                            <asp:Button ID="btnExtendDue" runat="server" Text="Extend due date" CssClass="px-4 py-2 bg-primary text-on-primary rounded-lg font-label-md font-bold" OnClick="btnExtendDue_Click" />
                            <asp:HyperLink ID="lnkRedistribute" runat="server"
                                CssClass="inline-flex items-center gap-1 px-4 py-2 border border-outline rounded-lg font-label-md font-bold">
                                Redistribute / add members
                            </asp:HyperLink>
                        </asp:Panel>
                    </asp:Panel>
                    <div class="grid grid-cols-2 gap-4 mt-4">
                        <div>
                            <span class="font-label-md text-on-surface-variant block mb-1">Due Date</span>
                            <p class="text-body-md text-on-surface"><asp:Literal ID="litDueDate" runat="server"></asp:Literal></p>
                        </div>
                        <div>
                            <span class="font-label-md text-on-surface-variant block mb-1">Event</span>
                            <p class="text-body-md text-on-surface"><asp:Literal ID="litEvent" runat="server"></asp:Literal></p>
                        </div>
                    </div>
                </section>

                <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">

                    <asp:Label ID="lblWorkflowMessage" runat="server" CssClass="font-label-md block mb-4" Visible="false"></asp:Label>

                    <!-- Left column -->
                    <div class="lg:col-span-2 space-y-6">

                        <asp:Panel ID="pnlReview" runat="server" Visible="false" CssClass="standard-card rounded-xl p-6">
                            <h3 class="font-title-lg text-title-lg text-primary mb-2">Faculty review</h3>
                            <p class="text-sm text-on-surface-variant mb-4">Comment on the member's work, then approve or send it back. The event concludes after you approve the last task.</p>
                            <asp:TextBox ID="txtReviewComment" runat="server" TextMode="MultiLine" Rows="4"
                                placeholder="Comments for the member..."
                                CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md resize-none mb-4"></asp:TextBox>
                            <div class="flex flex-wrap gap-3">
                                <asp:Button ID="btnApprove" runat="server" Text="Approve and complete" CssClass="btn-primary" OnClick="btnApprove_Click" />
                                <asp:Button ID="btnRequestChanges" runat="server" Text="Send back with comments" CssClass="px-4 py-2 border border-outline rounded-xl font-label-md font-bold" OnClick="btnRequestChanges_Click" />
                            </div>
                        </asp:Panel>

                        <!-- Post Update -->
                        <asp:Panel ID="pnlWorkerUpdate" runat="server" CssClass="standard-card rounded-xl p-6">
                            <h3 class="font-title-lg text-title-lg text-primary mb-6">Post Update</h3>
                            <div class="space-y-6">
                                <div>
                                    <label class="font-label-md text-on-surface-variant block mb-2">New Status</label>
                                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md">
                                        <asp:ListItem Text="Select status..." Value="" />
                                        <asp:ListItem Text="Not Started" Value="NotStarted" />
                                        <asp:ListItem Text="In Progress" Value="InProgress" />
                                        <asp:ListItem Text="Completed" Value="Completed" />
                                        <asp:ListItem Text="Delayed" Value="Delayed" />
                                    </asp:DropDownList>
                                </div>
                                <div>
                                    <label class="font-label-md text-on-surface-variant block mb-2">Comment / Notes</label>
                                    <asp:TextBox ID="txtComment" runat="server" TextMode="MultiLine" Rows="4" placeholder="Describe your progress or any issues..."
                                        CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md resize-none"></asp:TextBox>
                                </div>
                                <div id="pnlPublicSummaryField" runat="server">
                                    <label class="font-label-md text-on-surface-variant block mb-2">Public Summary (visible to the wider community)</label>
                                    <asp:TextBox ID="txtPublicSummary" runat="server" TextMode="MultiLine" Rows="2" placeholder="Short public-facing summary of progress..."
                                        CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md resize-none"></asp:TextBox>
                                    <p class="text-xs text-outline mt-1">You are the team Leader. This summary is shown on the My Tasks preview and public views.</p>
                                </div>
                                <div>
                                    <label class="font-label-md text-on-surface-variant block mb-2">Upload Evidence (optional)</label>
                                    <div class="border-2 border-dashed border-outline-variant rounded-xl p-6 text-center hover:border-primary transition-colors">
                                        <span class="material-symbols-outlined text-4xl text-outline-variant mb-2 block">cloud_upload</span>
                                        <p class="text-label-md text-on-surface-variant mb-2">Drag & drop files or click to browse</p>
                                        <asp:FileUpload ID="fuEvidence" runat="server" CssClass="w-full" />
                                        <p class="text-xs text-outline mt-2">PDF, Images, Documents up to 10MB</p>
                                    </div>
                                </div>
                                <div class="flex gap-3">
                                    <asp:Button ID="btnSubmitUpdate" runat="server" Text="Submit Update" CssClass="btn-primary" OnClick="btnSubmitUpdate_Click" />
                                    <asp:Label ID="lblSubmitMessage" runat="server" CssClass="text-body-md self-center" Visible="false"></asp:Label>
                                </div>
                            </div>
                        </asp:Panel>

                        <asp:Panel ID="pnlSharedComments" runat="server" Visible="false" CssClass="standard-card rounded-xl p-6">
                            <h3 class="font-title-lg text-title-lg text-primary mb-2">Shared comments</h3>
                            <p class="text-sm text-on-surface-variant mb-4">Anyone in this event can comment. Only assigned members can change the task.</p>
                            <asp:Repeater ID="rptSharedComments" runat="server">
                                <ItemTemplate>
                                    <div class="mb-3">
                                        <p class="text-sm font-semibold"><%# Eval("FullName") %>
                                            <span class="font-normal text-xs text-outline"><%# Eval("CreatedAt", "{0:MMM dd, yyyy HH:mm}") %></span>
                                        </p>
                                        <p class="text-sm text-on-surface"><%# Eval("Comment") %></p>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            <asp:Panel ID="pnlNoSharedComments" runat="server" Visible="false" CssClass="text-sm text-on-surface-variant mb-3">No comments yet.</asp:Panel>
                            <asp:TextBox ID="txtSharedComment" runat="server" TextMode="MultiLine" Rows="3"
                                CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md resize-none mb-2"
                                placeholder="Comment on this work..."></asp:TextBox>
                            <asp:Button ID="btnAddSharedComment" runat="server" Text="Post comment" CssClass="btn-primary" OnClick="btnAddSharedComment_Click" />
                        </asp:Panel>

                        <!-- Update History -->
                        <section class="standard-card rounded-xl p-6">
                            <div class="flex items-center justify-between mb-6">
                                <h3 class="font-title-lg text-title-lg text-primary">Update History</h3>
                                <span class="font-label-md text-label-md text-on-surface-variant"><asp:Literal ID="litUpdateCount" runat="server"></asp:Literal></span>
                            </div>
                            <asp:Repeater ID="rptHistory" runat="server">
                                <ItemTemplate>
                                    <div class="timeline-item">
                                        <div class='<%# "timeline-dot " + GetTimelineDotClass(Eval("NewStatus").ToString()) %>'></div>
                                        <div>
                                            <div class="flex items-center gap-2 mb-1">
                                                <span class='<%# "badge-status-" + Eval("NewStatus").ToString().ToLower().Replace(" ", "") %>'><%# Eval("NewStatus") %></span>
                                                <span class="text-xs text-on-surface-variant"><%# Eval("UpdatedAt", "{0:MMM dd, yyyy hh:mm tt}") %></span>
                                            </div>
                                            <p class="text-body-md text-on-surface font-semibold"><%# Eval("UserName") %></p>
                                            <p class="text-sm text-on-surface-variant mt-1"><%# Eval("Comment") %></p>
                                            <asp:Repeater ID="rptHistAttachments" runat="server" DataSource='<%# Eval("Attachments") %>'>
                                                <ItemTemplate>
                                                    <a href='<%# Eval("FilePath") %>' target="_blank" class="inline-flex items-center gap-1 text-sm text-primary hover:underline mt-1 mr-3">
                                                        <span class="material-symbols-outlined text-[14px]">attachment</span><%# Eval("FileName") %>
                                                    </a>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            <asp:Panel ID="pnlNoUpdates" runat="server" Visible="false" CssClass="p-8 text-center">
                                <p class="text-sm text-on-surface-variant">No updates posted yet.</p>
                            </asp:Panel>
                        </section>
                    </div>

                    <!-- Right column -->
                    <div class="space-y-6">

                        <asp:Panel ID="pnlAssign" runat="server" Visible="false" CssClass="standard-card rounded-xl p-6">
                            <h3 class="font-title-lg text-title-lg text-primary mb-2">Assign members</h3>
                            <p class="text-xs text-on-surface-variant mb-3">Only the event manager assigns who does this task.</p>
                            <asp:CheckBoxList ID="cblAssignees" runat="server" RepeatLayout="UnorderedList" CssClass="mb-3 space-y-1 font-body-md"></asp:CheckBoxList>
                            <asp:Button ID="btnSaveAssignees" runat="server" Text="Save assignments" CssClass="btn-primary" OnClick="btnSaveAssignees_Click" />
                        </asp:Panel>

                        <!-- Team Roster -->
                        <section class="standard-card rounded-xl p-6">
                            <h3 class="font-title-lg text-title-lg text-primary mb-4">Team Roster</h3>
                            <div class="space-y-3">
                                <asp:Repeater ID="rptRoster" runat="server">
                                    <ItemTemplate>
                                        <div class="flex items-center gap-3 p-2 bg-surface-container-low rounded-lg">
                                            <span class="material-symbols-outlined text-primary">person</span>
                                            <div class="flex-1 min-w-0">
                                                <p class="text-label-md text-on-surface truncate"><%# Eval("FullName") %></p>
                                                <p class="text-xs text-on-surface-variant"><%# Eval("Status") %></p>
                                            </div>
                                            <%# IsLeader(Eval("IsLeader")) %>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <asp:Panel ID="pnlNoRoster" runat="server" Visible="false">
                                    <p class="text-sm text-on-surface-variant text-center py-4">Team leader hasn't assigned a team yet.</p>
                                </asp:Panel>
                            </div>
                        </section>

                        <!-- Current Public Summary -->
                        <section class="standard-card rounded-xl p-6">
                            <h3 class="font-title-lg text-title-lg text-primary mb-4">Public Summary</h3>
                            <p class="text-body-md text-on-surface-variant"><asp:Literal ID="litCurrentPublicSummary" runat="server"></asp:Literal></p>
                        </section>

                        <!-- Attachments -->
                        <section class="standard-card rounded-xl p-6">
                            <h3 class="font-title-lg text-title-lg text-primary mb-4">All Attachments</h3>
                            <div class="space-y-3">
                                <asp:Repeater ID="rptAttachments" runat="server">
                                    <ItemTemplate>
                                        <a href='<%# Eval("FilePath") %>' target="_blank" class="flex items-center gap-3 p-2 bg-surface-container-low rounded-lg hover:bg-surface-container-high transition-colors">
                                            <span class="material-symbols-outlined text-primary">description</span>
                                            <div class="flex-1 min-w-0">
                                                <p class="text-label-md text-on-surface truncate"><%# Eval("FileName") %></p>
                                                <p class="text-xs text-outline"><%# Eval("UploadedAt", "{0:MMM dd, yyyy}") %></p>
                                            </div>
                                        </a>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <asp:Panel ID="pnlNoAttachments" runat="server" Visible="false">
                                    <p class="text-sm text-on-surface-variant text-center py-4">No attachments yet</p>
                                </asp:Panel>
                            </div>
                        </section>
                    </div>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlAccessDenied" runat="server" Visible="false" CssClass="standard-card rounded-xl p-12 text-center">
                <span class="material-symbols-outlined text-6xl text-outline-variant mb-4 block">lock</span>
                <h2 class="font-headline-md text-headline-md text-on-surface mb-2">Access Restricted</h2>
                <p class="text-body-md text-on-surface-variant mb-6">You need an accepted membership to this task to view its workspace.</p>
                <a href="<%= ResolveUrl("~/Modules/TaskWorkspaces/TaskWorkspaces.aspx") %>" class="btn-primary px-4 py-2">Back to Task Workspaces</a>
            </asp:Panel>

        </main>
    </div>
</asp:Content>
