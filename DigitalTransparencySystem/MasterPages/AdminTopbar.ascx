<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AdminTopbar.ascx.cs" Inherits="DigitalTransparencySystem.MasterPages.AdminTopbar" %>

<nav class="dashboard-topbar dtas-topbar w-full z-50 bg-surface/90 backdrop-blur-md border-b border-surface-container-high">
    <div class="dtas-topbar-left">
        <button type="button" id="btnAdminSidebarToggle" class="sidebar-toggle material-symbols-outlined text-on-surface-variant" onclick="toggleAdminSidebar()" aria-label="Open menu">menu</button>
        <a href="/Default.aspx" class="brand-link gap-2 font-bold hidden sm:inline-flex" aria-label="DTAS - Home">
            <span class="dtas-wordmark">DTAS</span>
        </a>
    </div>
    <div class="dtas-search" data-search-url="<%= ResolveUrl("~/Modules/Search/GlobalSearch.ashx") %>">
        <label class="dtas-search-field">
            <span class="material-symbols-outlined text-outline">search</span>
            <input type="search" class="js-dtas-search dtas-search-input" placeholder="Search events, tasks, meetings..." autocomplete="off" />
            <kbd class="dtas-search-kbd">/</kbd>
        </label>
        <div class="js-dtas-search-results dtas-search-results hidden" role="listbox"></div>
    </div>
    <div class="dtas-topbar-right">
        <!-- Theme Toggle -->
        <button type="button" class="theme-toggle relative p-2 hover:bg-surface-container-low rounded-full transition-colors" aria-label="Toggle dark mode" title="Toggle dark mode">
            <span class="material-symbols-outlined theme-icon-light text-on-surface-variant">light_mode</span>
            <span class="material-symbols-outlined theme-icon-dark hidden text-on-surface-variant">dark_mode</span>
        </button>

        <!-- Notifications -->
        <div class="relative">
            <button type="button" id="btnNotifications" class="relative p-2 hover:bg-surface-container-low rounded-full transition-colors material-symbols-outlined text-on-surface-variant">notifications</button>
            <span id="notifBadge" runat="server" class="js-unread-badge absolute -top-0.5 -right-0.5 hidden min-w-[18px] h-[18px] bg-error text-on-error text-[10px] font-bold rounded-full flex items-center justify-center px-1">0</span>
            <!-- Notifications Dropdown -->
            <div id="notifDropdown" class="hidden absolute right-0 mt-2 w-80 bg-surface-container-lowest border border-outline-variant rounded-xl shadow-lg overflow-hidden z-50">
                <div class="px-4 py-3 border-b border-surface-container-high flex justify-between items-center">
                    <span class="font-title-lg text-title-lg text-on-surface">Notifications</span>
                    <asp:LinkButton ID="lnkMarkAllRead" runat="server" CssClass="text-primary text-label-md hover:underline" OnClick="lnkMarkAllRead_Click">Mark all read</asp:LinkButton>
                </div>
                <div id="notifList" runat="server" class="max-h-80 overflow-y-auto">
                    <div class="px-4 py-8 text-center text-on-surface-variant text-label-md">No notifications yet</div>
                </div>
                <div class="px-4 py-2 border-t border-surface-container-high text-center">
                    <asp:HyperLink ID="lnkAllNotifications" runat="server" NavigateUrl="~/Modules/Notifications/Notifications.aspx"
                        CssClass="text-primary text-label-md font-bold hover:underline">View all notifications</asp:HyperLink>
                </div>
            </div>
        </div>

        <!-- Help -->
        <a href="/FAQ.aspx" class="p-2 hover:bg-surface-container-low rounded-full transition-colors material-symbols-outlined text-on-surface-variant">help</a>

        <!-- Settings -->
        <asp:HyperLink ID="lnkSettings" runat="server" NavigateUrl="~/Modules/Settings/AdminProfile.aspx"
            CssClass="p-2 hover:bg-surface-container-low rounded-full transition-colors material-symbols-outlined text-on-surface-variant">settings</asp:HyperLink>

        <!-- Profile Avatar + Dropdown -->
        <div class="relative">
            <button type="button" id="btnProfile" class="flex items-center gap-2 p-1 hover:bg-surface-container-low rounded-full transition-colors">
                <div class="h-10 w-10 rounded-full bg-primary-container flex items-center justify-center overflow-hidden border border-outline-variant">
                    <asp:Image ID="imgUserAvatar" runat="server" CssClass="w-full h-full object-cover" AlternateText="User Avatar" />
                    <span id="lblAvatarInitial" runat="server" class="text-on-primary-container font-bold text-sm"></span>
                </div>
            </button>
            <!-- Profile Dropdown -->
            <div id="profileDropdown" class="hidden absolute right-0 mt-2 w-64 bg-surface-container-lowest border border-outline-variant rounded-xl shadow-lg overflow-hidden z-50">
                <div class="px-4 py-3 border-b border-surface-container-high">
                    <asp:Label ID="lblUserName" runat="server" CssClass="font-title-lg text-title-lg text-on-surface block"></asp:Label>
                    <asp:Label ID="lblUserEmail" runat="server" CssClass="text-label-md text-on-surface-variant block mt-0.5"></asp:Label>
                    <asp:Label ID="lblUserRole" runat="server" CssClass="text-badge-cap uppercase text-primary block mt-1"></asp:Label>
                </div>
                <div class="py-1">
                    <asp:HyperLink ID="lnkProfile" runat="server" NavigateUrl="~/Modules/Settings/AdminProfile.aspx"
                        CssClass="flex items-center gap-3 px-4 py-2.5 text-on-surface hover:bg-surface-container-low transition-colors text-label-md">
                        <span class="material-symbols-outlined text-[20px]">person</span>
                        My Profile
                    </asp:HyperLink>
                    <asp:HyperLink ID="lnkSettingsMenu" runat="server" NavigateUrl="~/Modules/Settings/AdminProfile.aspx"
                        CssClass="flex items-center gap-3 px-4 py-2.5 text-on-surface hover:bg-surface-container-low transition-colors text-label-md">
                        <span class="material-symbols-outlined text-[20px]">settings</span>
                        Settings
                    </asp:HyperLink>
                    <asp:HyperLink ID="lnkLoginHistory" runat="server" NavigateUrl="~/Modules/Reports/LoginHistory.aspx"
                        CssClass="flex items-center gap-3 px-4 py-2.5 text-on-surface hover:bg-surface-container-low transition-colors text-label-md">
                        <span class="material-symbols-outlined text-[20px]">history</span>
                        Login History
                    </asp:HyperLink>
                </div>
                <div class="border-t border-surface-container-high py-1">
                    <asp:HyperLink ID="lnkLogout" runat="server" NavigateUrl="~/Modules/Authentication/Logout.aspx"
                        CssClass="flex items-center gap-3 px-4 py-2.5 text-error hover:bg-error-container/30 transition-colors text-label-md">
                        <span class="material-symbols-outlined text-[20px]">logout</span>
                        Logout
                    </asp:HyperLink>
                </div>
            </div>
        </div>
    </div>
</nav>

<script type="text/javascript">
    (function () {
        var btnNotif = document.getElementById('btnNotifications');
        var notifDrop = document.getElementById('notifDropdown');
        var btnProfile = document.getElementById('btnProfile');
        var profileDrop = document.getElementById('profileDropdown');

        function closeAll() {
            if (notifDrop) notifDrop.classList.add('hidden');
            if (profileDrop) profileDrop.classList.add('hidden');
        }

        if (btnNotif && notifDrop) {
            btnNotif.addEventListener('click', function (e) {
                e.stopPropagation();
                var isOpen = !notifDrop.classList.contains('hidden');
                closeAll();
                if (!isOpen) {
                    notifDrop.classList.remove('hidden');
                    var badges = document.querySelectorAll('.js-unread-badge');
                    for (var i = 0; i < badges.length; i++) {
                        badges[i].style.display = 'none';
                        badges[i].classList.add('hidden');
                    }
                    fetch('<%= ResolveUrl("~/Modules/Notifications/Notifications.aspx") %>?seen=1', {
                        credentials: 'same-origin',
                        headers: { 'X-Requested-With': 'XMLHttpRequest' }
                    });
                }
            });
        }

        if (btnProfile && profileDrop) {
            btnProfile.addEventListener('click', function (e) {
                e.stopPropagation();
                var isOpen = !profileDrop.classList.contains('hidden');
                closeAll();
                if (!isOpen) profileDrop.classList.remove('hidden');
            });
        }

        document.addEventListener('click', function () {
            closeAll();
        });

        if (notifDrop) {
            notifDrop.addEventListener('click', function (e) {
                e.stopPropagation();
            });
        }
        if (profileDrop) {
            profileDrop.addEventListener('click', function (e) {
                e.stopPropagation();
            });
        }
    })();
</script>
