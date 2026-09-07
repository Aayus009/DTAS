<%@ Page Title="Task Preview | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TaskPreview.aspx.cs" Inherits="DigitalTransparencySystem.Modules.UserTasks.TaskPreview" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <uc:UserTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:UserSidebar ActivePage="Tasks" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">

            <a href="<%= ResolveUrl("~/Modules/Dashboard/UsersDashboard.aspx") %>" class="inline-flex items-center gap-1 text-primary font-bold text-sm mb-3">
                <span class="material-symbols-outlined text-[20px]">arrow_back</span>
                Back
            </a>

            <!-- Task Info -->
            <section class="standard-card rounded-xl p-6 mb-6">
                <div class="flex justify-between items-start mb-6">
                    <div>
                        <h1 class="font-headline-md text-headline-md text-primary mb-2">Task Preview</h1>
                        <div class="flex gap-2 flex-wrap">
                            <span id="spanStatus" runat="server" class=""></span>
                        </div>
                    </div>
                </div>
                <div class="space-y-4">
                    <div>
                        <span class="font-label-md text-on-surface-variant block mb-1">Task Title</span>
                        <p class="text-title-md text-title-md text-on-surface"><asp:Literal ID="litTaskTitle" runat="server"></asp:Literal></p>
                    </div>
                    <div>
                        <span class="font-label-md text-on-surface-variant block mb-1">Description</span>
                        <p class="text-body-md text-on-surface"><asp:Literal ID="litDescription" runat="server"></asp:Literal></p>
                    </div>
                    <div>
                        <span class="font-label-md text-on-surface-variant block mb-1">Public Summary</span>
                        <p class="text-body-md text-on-surface"><asp:Literal ID="litPublicSummary" runat="server"></asp:Literal></p>
                    </div>
                    <div class="grid grid-cols-2 gap-4">
                        <div>
                            <span class="font-label-md text-on-surface-variant block mb-1">Priority</span>
                            <p class="text-body-md text-on-surface"><span id="spanPriority" runat="server" class=""></span></p>
                        </div>
                        <div>
                            <span class="font-label-md text-on-surface-variant block mb-1">Due Date</span>
                            <p class="text-body-md text-on-surface"><asp:Literal ID="litDueDate" runat="server"></asp:Literal></p>
                        </div>
                    </div>
                    <div>
                        <span class="font-label-md text-on-surface-variant block mb-1">Event</span>
                        <p class="text-body-md text-on-surface"><asp:Literal ID="litEvent" runat="server"></asp:Literal></p>
                    </div>
                </div>
            </section>

            <div class="flex gap-3">
                <a href="<%= ResolveUrl("~/Modules/UserTasks/UserTasks.aspx") %>" class="btn-outline px-4 py-2">Back to My Tasks</a>
            </div>

        </main>
    </div>
</asp:Content>
