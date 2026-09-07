<%@ Page Title="Profile Settings | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AdminProfile.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Settings.AdminProfile" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <uc:AdminTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:AdminSidebar ActivePage="Settings" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            <header class="flex justify-between items-end mb-8">
                <div>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Profile Settings</h1>
                    <p class="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">Manage your account information, profile image, and security settings.</p>
                </div>
            </header>

            <!-- Success/Error Messages -->
            <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="mb-6">
                <div id="divMessage" runat="server" class="px-4 py-3 rounded-xl flex items-center gap-3 text-label-md">
                    <span id="msgIcon" runat="server" class="material-symbols-outlined text-[20px]"></span>
                    <asp:Label ID="lblMessage" runat="server"></asp:Label>
                </div>
            </asp:Panel>

            <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
                <!-- Profile Image Card -->
                <div class="standard-card p-6 rounded-xl">
                    <div class="flex flex-col items-center text-center">
                        <div class="relative mb-4">
                            <div class="h-28 w-28 rounded-full bg-primary-container flex items-center justify-center overflow-hidden border-2 border-outline-variant">
                                <asp:Image ID="imgProfile" runat="server" CssClass="w-full h-full object-cover" AlternateText="Profile Image" />
                                <span id="lblProfileInitial" runat="server" class="text-on-primary-container font-bold text-3xl"></span>
                            </div>
                        </div>
                        <asp:Label ID="lblProfileName" runat="server" CssClass="font-title-lg text-title-lg text-on-surface block"></asp:Label>
                        <asp:Label ID="lblProfileRole" runat="server" CssClass="text-badge-cap uppercase text-primary block mt-1"></asp:Label>
                        <asp:Label ID="lblProfileEmail" runat="server" CssClass="text-label-md text-on-surface-variant block mt-1"></asp:Label>

                        <div class="mt-6 w-full">
                            <label class="btn-outline w-full text-center cursor-pointer block">
                                <span class="material-symbols-outlined text-[18px] align-middle mr-1">upload</span>
                                Change Photo
                                <asp:FileUpload ID="fuProfileImage" runat="server" CssClass="hidden" Accept="image/*" onchange="this.form.submit()" />
                            </label>
                            <asp:Button ID="btnRemoveImage" runat="server" Text="Remove Photo"
                                CssClass="mt-2 w-full py-2 px-4 border border-outline text-on-surface-variant rounded-xl font-label-md text-label-md hover:bg-surface-container-low transition-colors cursor-pointer"
                                OnClick="btnRemoveImage_Click" Visible="false" />
                        </div>
                    </div>
                </div>

                <!-- Profile Info Card -->
                <div class="lg:col-span-2 standard-card p-6 rounded-xl">
                    <h2 class="font-title-lg text-title-lg text-on-surface mb-6">Personal Information</h2>
                    <div class="space-y-6">
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div>
                                <label class="font-label-md text-label-md text-on-surface-variant block mb-2">Full Name</label>
                                <asp:TextBox ID="txtFullName" runat="server"
                                    CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" />
                            </div>
                            <div>
                                <label class="font-label-md text-label-md text-on-surface-variant block mb-2">Username</label>
                                <asp:TextBox ID="txtUsername" runat="server" Enabled="false"
                                    CssClass="w-full px-4 py-3 bg-surface-container border border-outline-variant rounded-xl font-body-md text-body-md opacity-60 cursor-not-allowed" />
                            </div>
                        </div>
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div>
                                <label class="font-label-md text-label-md text-on-surface-variant block mb-2">Email Address</label>
                                <asp:TextBox ID="txtEmail" runat="server" TextMode="SingleLine"
                                    CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" />
                            </div>
                            <div>
                                <label class="font-label-md text-label-md text-on-surface-variant block mb-2">Phone Number</label>
                                <asp:TextBox ID="txtPhone" runat="server"
                                    CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" />
                            </div>
                        </div>
                        <div>
                            <label class="font-label-md text-label-md text-on-surface-variant block mb-2">Department</label>
                            <asp:DropDownList ID="ddlDepartment" runat="server"
                                CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all">
                            </asp:DropDownList>
                        </div>
                        <div class="flex justify-end">
                            <asp:Button ID="btnSaveProfile" runat="server" Text="Save Changes"
                                CssClass="btn-primary px-8 py-3"
                                OnClick="btnSaveProfile_Click" />
                        </div>
                    </div>
                </div>
            </div>

            <!-- Change Password Section -->
            <div class="standard-card p-6 rounded-xl mt-6">
                <h2 class="font-title-lg text-title-lg text-on-surface mb-6">Change Password</h2>
                <div class="space-y-6 max-w-xl">
                    <div>
                        <label class="font-label-md text-label-md text-on-surface-variant block mb-2">Current Password</label>
                        <asp:TextBox ID="txtCurrentPassword" runat="server" TextMode="Password"
                            CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" />
                    </div>
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div>
                            <label class="font-label-md text-label-md text-on-surface-variant block mb-2">New Password</label>
                            <asp:TextBox ID="txtNewPassword" runat="server" TextMode="Password"
                                CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" />
                        </div>
                        <div>
                            <label class="font-label-md text-label-md text-on-surface-variant block mb-2">Confirm New Password</label>
                            <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password"
                                CssClass="w-full px-4 py-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all" />
                        </div>
                    </div>
                    <div class="flex justify-end">
                        <asp:Button ID="btnChangePassword" runat="server" Text="Update Password"
                            CssClass="btn-secondary px-8 py-3"
                            OnClick="btnChangePassword_Click" />
                    </div>
                </div>
            </div>

            <!-- Danger Zone -->
            <div class="border border-error/30 bg-error-container/10 p-6 rounded-xl mt-6">
                <h2 class="font-title-lg text-title-lg text-error mb-2">Account Actions</h2>
                <p class="text-body-md text-on-surface-variant mb-4">Manage your account session and access.</p>
                <div class="flex gap-4">
                    <asp:HyperLink ID="lnkLogoutProfile" runat="server" NavigateUrl="~/Modules/Authentication/Logout.aspx"
                        CssClass="inline-flex items-center gap-2 px-6 py-3 border border-error text-error rounded-xl font-label-md text-label-md hover:bg-error-container/30 transition-colors">
                        <span class="material-symbols-outlined text-[18px]">logout</span>
                        Logout from Account
                    </asp:HyperLink>
                </div>
            </div>

        </main>
    </div>

</asp:Content>
