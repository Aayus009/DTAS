<%@ Page Title="My Profile | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UserProfile.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Settings.UserProfile" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <uc:UserTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:UserSidebar ActivePage="Settings" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            
            <header class="mb-8">
                <a href="<%= ResolveUrl("~/Modules/Dashboard/UsersDashboard.aspx") %>" class="inline-flex items-center gap-1 text-primary font-bold text-sm mb-3">
                    <span class="material-symbols-outlined text-[20px]">arrow_back</span>
                    Back
                </a>
                <h1 class="font-headline-lg text-headline-lg text-primary mb-2">My Profile</h1>
                <p class="font-body-lg text-body-lg text-on-surface-variant">Manage your account settings and personal information.</p>
            </header>

            <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
                
                <!-- Profile Card -->
                <div class="space-y-6">
                    <section class="standard-card rounded-xl p-6 text-center">
                        <div class="h-24 w-24 rounded-full bg-primary-container flex items-center justify-center mx-auto mb-4 border-4 border-surface overflow-hidden">
                            <asp:Image ID="imgProfile" runat="server" CssClass="w-full h-full object-cover" AlternateText="Profile" />
                            <span id="lblInitials" runat="server" class="text-on-primary-container font-bold text-2xl"></span>
                        </div>
                        <h3 class="font-title-lg text-title-lg text-on-surface"><asp:Literal ID="litFullName" runat="server"></asp:Literal></h3>
                        <p class="text-label-md text-on-surface-variant"><asp:Literal ID="litEmail" runat="server"></asp:Literal></p>
                        <span id="spanRole" runat="server" class="badge-status-scheduled mt-2 inline-block"></span>
                        
                        <div class="mt-6 pt-4 border-t border-surface-container-high text-left space-y-3">
                            <div class="flex items-center gap-3">
                                <span class="material-symbols-outlined text-[18px] text-outline">business</span>
                                <span class="text-body-md text-on-surface-variant"><asp:Literal ID="litDepartment" runat="server" Text="Not assigned"></asp:Literal></span>
                            </div>
                            <div class="flex items-center gap-3">
                                <span class="material-symbols-outlined text-[18px] text-outline">phone</span>
                                <span class="text-body-md text-on-surface-variant"><asp:Literal ID="litPhone" runat="server" Text="Not provided"></asp:Literal></span>
                            </div>
                            <div class="flex items-center gap-3">
                                <span class="material-symbols-outlined text-[18px] text-outline">calendar_today</span>
                                <span class="text-body-md text-on-surface-variant">Joined <asp:Literal ID="litJoinDate" runat="server"></asp:Literal></span>
                            </div>
                        </div>
                    </section>
                </div>

                <!-- Edit Profile & Password -->
                <div class="lg:col-span-2 space-y-6">
                    
                    <!-- Profile Image Upload -->
                    <section class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-6">Profile Picture</h3>
                        <div class="flex items-center gap-6">
                            <div class="h-20 w-20 rounded-full bg-primary-container flex items-center justify-center overflow-hidden border-2 border-surface">
                                <asp:Image ID="imgProfileUpload" runat="server" CssClass="w-full h-full object-cover" />
                                <span id="lblUploadInitials" runat="server" class="text-on-primary-container font-bold text-xl"></span>
                            </div>
                            <div class="flex-1">
                                <asp:FileUpload ID="fuProfileImage" runat="server" CssClass="mb-2" />
                                <asp:Button ID="btnUploadImage" runat="server" Text="Upload Image" CssClass="btn-primary" OnClick="btnUploadImage_Click" />
                                <p class="text-xs text-outline mt-1">JPG, PNG up to 5MB</p>
                            </div>
                        </div>
                    </section>

                    <!-- Edit Profile Form -->
                    <section class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-6">Personal Information</h3>
                        <div class="space-y-6">
                            <div class="grid grid-cols-2 gap-4">
                                <div>
                                    <label class="font-label-md text-on-surface-variant block mb-2">Full Name</label>
                                    <asp:TextBox ID="txtFullName" runat="server" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md"></asp:TextBox>
                                </div>
                                <div>
                                    <label class="font-label-md text-on-surface-variant block mb-2">Email</label>
                                    <asp:TextBox ID="txtEmail" runat="server" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md" ReadOnly="true"></asp:TextBox>
                                </div>
                            </div>
                            <div class="grid grid-cols-2 gap-4">
                                <div>
                                    <label class="font-label-md text-on-surface-variant block mb-2">Phone</label>
                                    <asp:TextBox ID="txtPhone" runat="server" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md" placeholder="Enter phone number"></asp:TextBox>
                                </div>
                                <div>
                                    <label class="font-label-md text-on-surface-variant block mb-2">Department</label>
                                    <asp:DropDownList ID="ddlDepartment" runat="server" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md"></asp:DropDownList>
                                </div>
                            </div>
                            <asp:Button ID="btnSaveProfile" runat="server" Text="Save Changes" CssClass="btn-primary" OnClick="btnSaveProfile_Click" />
                            <asp:Label ID="lblProfileSuccess" runat="server" CssClass="text-tertiary font-label-md hidden" Text="Profile updated successfully!"></asp:Label>
                        </div>
                    </section>

                    <section class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-2">Change email</h3>
                        <p class="font-body-md text-body-md text-on-surface-variant mb-6">A one-time code is sent to the new address before it becomes active.</p>
                        <div class="space-y-4">
                            <div>
                                <label class="font-label-md text-on-surface-variant block mb-2">New email</label>
                                <asp:TextBox ID="txtNewEmail" runat="server" TextMode="Email" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md"></asp:TextBox>
                            </div>
                            <asp:Button ID="btnSendEmailCode" runat="server" Text="Send verification code" CssClass="btn-outline" OnClick="btnSendEmailCode_Click" />
                            <asp:Panel ID="pnlEmailOtp" runat="server" Visible="false" CssClass="space-y-4">
                                <asp:Label ID="lblEmailOtpSent" runat="server" CssClass="font-body-md text-on-surface-variant block"></asp:Label>
                                <asp:TextBox ID="txtEmailOtp" runat="server" MaxLength="6" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl text-center tracking-[8px]" placeholder="000000"></asp:TextBox>
                                <asp:Button ID="btnConfirmEmail" runat="server" Text="Confirm new email" CssClass="btn-primary" OnClick="btnConfirmEmail_Click" />
                            </asp:Panel>
                            <asp:Label ID="lblEmailChangeMessage" runat="server" CssClass="font-label-md"></asp:Label>
                        </div>
                    </section>

                    <!-- Change Password -->
                    <section class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-6">Change Password</h3>
                        <div class="space-y-6">
                            <div>
                                <label class="font-label-md text-on-surface-variant block mb-2">Current Password</label>
                                <asp:TextBox ID="txtCurrentPassword" runat="server" TextMode="Password" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md"></asp:TextBox>
                            </div>
                            <div class="grid grid-cols-2 gap-4">
                                <div>
                                    <label class="font-label-md text-on-surface-variant block mb-2">New Password</label>
                                    <asp:TextBox ID="txtNewPassword" runat="server" TextMode="Password" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md"></asp:TextBox>
                                </div>
                                <div>
                                    <label class="font-label-md text-on-surface-variant block mb-2">Confirm New Password</label>
                                    <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md"></asp:TextBox>
                                </div>
                            </div>
                            <asp:Button ID="btnChangePassword" runat="server" Text="Change Password" CssClass="btn-outline" OnClick="btnChangePassword_Click" />
                            <asp:Label ID="lblPasswordSuccess" runat="server" CssClass="text-tertiary font-label-md hidden" Text="Password changed successfully!"></asp:Label>
                            <asp:Label ID="lblPasswordError" runat="server" CssClass="text-error font-label-md hidden"></asp:Label>
                        </div>
                    </section>
                </div>
            </div>

        </main>
    </div>
</asp:Content>
