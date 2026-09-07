<%@ Page Title="Feedback | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UserFeedback.aspx.cs" Inherits="DigitalTransparencySystem.Modules.UserFeedback.UserFeedback" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <uc:UserTopbar runat="server" />

    <div class="flex min-h-screen">
        <uc:UserSidebar ActivePage="Feedback" runat="server" />

        <main class="dashboard-main flex-1 ml-64 p-8">
            
            <header class="mb-8">
                <h1 class="font-headline-lg text-headline-lg text-primary mb-2">Feedback & Suggestions</h1>
                <p class="font-body-lg text-body-lg text-on-surface-variant">Submit your suggestions, complaints, or recommendations to help improve the system.</p>
            </header>

            <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
                
                <!-- Submit Feedback Form -->
                <div class="lg:col-span-2">
                    <section class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-6">Submit New Feedback</h3>
                        <div class="space-y-6">
                            <div class="grid grid-cols-2 gap-4">
                                <div>
                                    <label class="font-label-md text-on-surface-variant block mb-2">Category</label>
                                    <asp:DropDownList ID="ddlCategory" runat="server" CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md">
                                        <asp:ListItem Text="Suggestion" Value="Suggestion" />
                                        <asp:ListItem Text="Complaint" Value="Complaint" />
                                        <asp:ListItem Text="Recommendation" Value="Recommendation" />
                                        <asp:ListItem Text="General Feedback" Value="General" />
                                    </asp:DropDownList>
                                </div>
                                <div>
                                    <label class="font-label-md text-on-surface-variant block mb-2">Rating</label>
                                    <div class="flex gap-1" id="ratingStars">
                                        <button type="button" class="star-btn material-symbols-outlined text-3xl text-outline-variant hover:text-primary transition-colors" data-rating="1">star</button>
                                        <button type="button" class="star-btn material-symbols-outlined text-3xl text-outline-variant hover:text-primary transition-colors" data-rating="2">star</button>
                                        <button type="button" class="star-btn material-symbols-outlined text-3xl text-outline-variant hover:text-primary transition-colors" data-rating="3">star</button>
                                        <button type="button" class="star-btn material-symbols-outlined text-3xl text-outline-variant hover:text-primary transition-colors" data-rating="4">star</button>
                                        <button type="button" class="star-btn material-symbols-outlined text-3xl text-outline-variant hover:text-primary transition-colors" data-rating="5">star</button>
                                    </div>
                                    <asp:HiddenField ID="hfRating" runat="server" Value="0" />
                                </div>
                            </div>
                            <div>
                                <label class="font-label-md text-on-surface-variant block mb-2">Title</label>
                                <asp:TextBox ID="txtTitle" runat="server" placeholder="Brief title for your feedback..."
                                    CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md"></asp:TextBox>
                            </div>
                            <div>
                                <label class="font-label-md text-on-surface-variant block mb-2">Description</label>
                                <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="6" placeholder="Describe your feedback in detail..."
                                    CssClass="w-full p-3 bg-surface-container-low border border-outline rounded-xl font-body-md text-body-md resize-none"></asp:TextBox>
                            </div>
                            <asp:Button ID="btnSubmit" runat="server" Text="Submit Feedback" CssClass="btn-primary" OnClick="btnSubmit_Click" />
                            <asp:Label ID="lblSuccess" runat="server" CssClass="text-tertiary font-label-md hidden" Text="Feedback submitted successfully!"></asp:Label>
                        </div>
                    </section>

                    <!-- My Feedback History -->
                    <section class="standard-card rounded-xl p-6 mt-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-6">My Submissions</h3>
                        <asp:Repeater ID="rptMyFeedback" runat="server">
                            <ItemTemplate>
                                <div class="p-4 bg-surface-container-low rounded-lg mb-4 last:mb-0">
                                    <div class="flex justify-between items-start mb-2">
                                        <div>
                                            <span class="font-label-md text-label-md font-bold text-on-surface"><%# Eval("Title") %></span>
                                            <span class="badge-status-pending ml-2"><%# Eval("Category") %></span>
                                        </div>
                                        <span class='<%# "badge-status-" + Eval("Status").ToString().ToLower().Replace(" ", "") %>'><%# Eval("Status") %></span>
                                    </div>
                                    <p class="text-sm text-on-surface-variant mb-2"><%# Eval("Description") %></p>
                                    <div class="flex items-center gap-4 text-xs text-outline">
                                        <span><%# Eval("CreatedAt", "{0:MMM dd, yyyy}") %></span>
                                        <%# Eval("Rating") != DBNull.Value ? "<span>Rating: " + Eval("Rating") + "/5</span>" : "" %>
                                    </div>
                                    <%# Eval("AdminResponse") != DBNull.Value && !string.IsNullOrEmpty(Eval("AdminResponse").ToString()) ? 
                                        "<div class='mt-3 p-3 bg-primary-container/10 rounded-lg border-l-3 border-primary'><p class='text-label-md font-bold text-primary mb-1'>Admin Response:</p><p class='text-sm text-on-surface-variant'>" + Eval("AdminResponse") + "</p></div>" : "" %>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:Panel ID="pnlNoFeedback" runat="server" Visible="false" CssClass="text-center py-8">
                            <span class="material-symbols-outlined text-5xl text-outline-variant mb-2 block empty-state-icon">feedback</span>
                            <p class="text-body-md text-on-surface-variant">No submissions yet</p>
                        </asp:Panel>
                    </section>
                </div>

                <!-- Sidebar Info -->
                <div class="space-y-6">
                    <section class="standard-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-4">Guidelines</h3>
                        <ul class="space-y-3 text-body-md text-on-surface-variant">
                            <li class="flex items-start gap-2">
                                <span class="material-symbols-outlined text-primary text-[18px] mt-0.5">check_circle</span>
                                <span>Be specific and constructive in your feedback</span>
                            </li>
                            <li class="flex items-start gap-2">
                                <span class="material-symbols-outlined text-primary text-[18px] mt-0.5">check_circle</span>
                                <span>Include examples where possible</span>
                            </li>
                            <li class="flex items-start gap-2">
                                <span class="material-symbols-outlined text-primary text-[18px] mt-0.5">check_circle</span>
                                <span>All feedback is reviewed by administrators</span>
                            </li>
                            <li class="flex items-start gap-2">
                                <span class="material-symbols-outlined text-primary text-[18px] mt-0.5">check_circle</span>
                                <span>You will receive a notification when your feedback is addressed</span>
                            </li>
                        </ul>
                    </section>

                    <section class="glass-card rounded-xl p-6">
                        <h3 class="font-title-lg text-title-lg text-primary mb-2">Transparency Score</h3>
                        <p class="text-body-md text-on-surface-variant mb-4">Your feedback helps improve institutional transparency.</p>
                        <div class="text-center">
                            <asp:Literal ID="litFeedbackCount" runat="server" Text="0"></asp:Literal>
                            <span class="text-body-md text-on-surface-variant block">total submissions</span>
                        </div>
                    </section>
                </div>
            </div>

        </main>
    </div>

    <script type="text/javascript">
        document.addEventListener('DOMContentLoaded', function () {
            var stars = document.querySelectorAll('.star-btn');
            var hfRating = document.getElementById('<%= hfRating.ClientID %>');
            var currentRating = parseInt(hfRating.value) || 0;

            function updateStars(rating) {
                stars.forEach(function (star) {
                    var starRating = parseInt(star.getAttribute('data-rating'));
                    if (starRating <= rating) {
                        star.classList.remove('text-outline-variant');
                        star.classList.add('text-primary');
                        star.style.fontVariationSettings = "'FILL' 1";
                    } else {
                        star.classList.add('text-outline-variant');
                        star.classList.remove('text-primary');
                        star.style.fontVariationSettings = "'FILL' 0";
                    }
                });
            }

            if (currentRating > 0) updateStars(currentRating);

            stars.forEach(function (star) {
                star.addEventListener('click', function () {
                    currentRating = parseInt(this.getAttribute('data-rating'));
                    hfRating.value = currentRating;
                    updateStars(currentRating);
                });
            });
        });
    </script>
</asp:Content>
