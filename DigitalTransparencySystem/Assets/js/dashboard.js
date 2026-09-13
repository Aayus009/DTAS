/* ============================================
   Dashboard Page Scripts
   ============================================ */

(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        if (window.__dtasDashboardInit) return;
        window.__dtasDashboardInit = true;

        initTransientNotices();
        if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initTransientNotices);
        }

        if (!document.querySelector('.dashboard-main') && !document.querySelector('.dashboard-sidebar')) {
            window.toggleUserSidebar = window.toggleAdminSidebar = function () { };
            return;
        }

        initCardHoverEffects();
        initGlobalSearch();
        initProgressBars();
        initMobileSidebar();
        initFormBusyState();
        initClientValidation();
    });

    /**
     * Card hover micro-interactions
     */
    function initCardHoverEffects() {
        document.querySelectorAll('.standard-card').forEach(function (card) {
            card.addEventListener('mouseenter', function () {
                this.style.transition = 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)';
            });
        });
    }

    /**
     * Header search: type a page or record name, press Enter, go there.
     */
    function initGlobalSearch() {
        var boxes = document.querySelectorAll('.dtas-search');
        if (!boxes.length) return;

        boxes.forEach(bindSearchBox);

        document.addEventListener('keydown', function (e) {
            if (e.key !== '/' || e.ctrlKey || e.metaKey || e.altKey) return;
            var tag = (e.target && e.target.tagName) ? e.target.tagName.toLowerCase() : '';
            if (tag === 'input' || tag === 'textarea' || (e.target && e.target.isContentEditable)) return;
            var input = document.querySelector('.dashboard-topbar:not([style*="display: none"]) .js-dtas-search')
                || document.querySelector('.js-dtas-search');
            if (!input) return;
            e.preventDefault();
            input.focus();
        });
    }

    function bindSearchBox(box) {
        var input = box.querySelector('.js-dtas-search');
        var status = box.querySelector('.js-dtas-search-status');
        var url = box.getAttribute('data-search-url');
        if (!input || !url) return;

        var pending = false;

        input.addEventListener('input', function () {
            hideStatus();
        });

        input.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                hideStatus();
                input.blur();
                return;
            }
            if (e.key !== 'Enter') return;
            e.preventDefault();
            e.stopPropagation();
            go((input.value || '').trim());
        });

        function go(q) {
            hideStatus();
            if (q.length < 2 || pending) {
                if (q.length < 2) showStatus('Type at least 2 characters, then press Enter.');
                return;
            }
            pending = true;
            input.setAttribute('aria-busy', 'true');
            fetch(url + '?q=' + encodeURIComponent(q), {
                credentials: 'same-origin',
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            })
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    if (data && data.url) {
                        window.location.href = data.url;
                        return;
                    }
                    showStatus((data && data.error) ? data.error : 'No matching page or record.');
                })
                .catch(function () {
                    showStatus('Search is unavailable right now.');
                })
                .then(function () {
                    pending = false;
                    input.removeAttribute('aria-busy');
                });
        }

        function showStatus(text) {
            if (!status) return;
            status.textContent = text;
            status.classList.remove('hidden');
        }

        function hideStatus() {
            if (!status) return;
            status.textContent = '';
            status.classList.add('hidden');
        }
    }

    /**
     * Animate progress bars on page load
     */
    function initProgressBars() {
        var root = document.querySelector('.dashboard-main');
        if (!root) return;
        var progressBars = root.querySelectorAll('.js-progress-fill');

        progressBars.forEach(function (bar) {
            if (bar.getAttribute('data-bar-ready') === '1')
                return;

            var painted = (bar.style.width || '').trim();
            var target = bar.getAttribute('data-width') || painted || '';
            if (!target || target === '0' || target === '0%') {
                bar.setAttribute('data-bar-ready', '1');
                return;
            }

            bar.setAttribute('data-width', target);
            bar.setAttribute('data-bar-ready', '1');

            // Already showing the real width (server-rendered). Do not collapse to 0%.
            if (painted && painted !== '0' && painted !== '0%')
                return;

            bar.style.width = '0%';
            bar.style.transition = 'width 1s cubic-bezier(0.4, 0, 0.2, 1)';

            window.requestAnimationFrame(function () {
                window.requestAnimationFrame(function () {
                    bar.style.width = target;
                });
            });
        });
    }

    /**
     * Mobile sidebar toggle
     */
    function initMobileSidebar() {
        var sidebar = document.querySelector('.dashboard-sidebar');
        var overlay = document.querySelector('.sidebar-overlay');

        if (!sidebar) return;

        // Create overlay if it doesn't exist
        if (!overlay) {
            overlay = document.createElement('div');
            overlay.className = 'sidebar-overlay';
            document.body.appendChild(overlay);
        }

        overlay.addEventListener('click', function () {
            closeMobileSidebar();
        });

        sidebar.querySelectorAll('a').forEach(function (link) {
            link.addEventListener('click', function () {
                closeMobileSidebar();
            });
        });

        // Close sidebar on Escape key
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && sidebar.classList.contains('is-open')) {
                sidebar.classList.remove('is-open');
                overlay.classList.remove('is-visible');
            }
        });
    }

    /**
     * Toggle user/admin sidebar (for mobile)
     */
    function closeMobileSidebar() {
        var sidebar = document.querySelector('.dashboard-sidebar');
        var overlay = document.querySelector('.sidebar-overlay');
        if (sidebar) sidebar.classList.remove('is-open');
        if (overlay) overlay.classList.remove('is-visible');
        document.body.classList.remove('sidebar-open');
    }

    window.toggleUserSidebar = window.toggleAdminSidebar = function () {
        var sidebar = document.querySelector('.dashboard-sidebar');
        var overlay = document.querySelector('.sidebar-overlay');

        if (!sidebar) return;

        if (!overlay) {
            overlay = document.createElement('div');
            overlay.className = 'sidebar-overlay';
            document.body.appendChild(overlay);
            overlay.addEventListener('click', closeMobileSidebar);
        }

        sidebar.classList.toggle('is-open');
        overlay.classList.toggle('is-visible');
        document.body.classList.toggle('sidebar-open', sidebar.classList.contains('is-open'));
    };

    function initFormBusyState() {
        var form = document.getElementById('form1');
        if (!form) return;

        form.addEventListener('submit', function () {
            var el = document.activeElement;
            if (!el || !el.classList || !el.classList.contains('btn-primary'))
                return;
            if (el.getAttribute('data-busy') === '1')
                return false;
            el.setAttribute('data-busy', '1');
            el.classList.add('is-busy');
            if (el.tagName === 'INPUT')
                el.value = 'Working...';
            else
                el.textContent = 'Working...';
        });
    }

    function initClientValidation() {
        document.querySelectorAll('[data-required="true"]').forEach(function (input) {
            input.addEventListener('invalid', function () {
                this.setCustomValidity(this.getAttribute('data-required-message') || 'This field is required.');
            });
            input.addEventListener('input', function () {
                this.setCustomValidity('');
            });
        });
    }

    function initTransientNotices() {
        document.querySelectorAll('.dtas-toast, .toast-notification').forEach(function (el) {
            var text = (el.textContent || '').replace(/\s+/g, '');
            if (!text)
                return;
            if (el.getAttribute('data-toast-sig') === text)
                return;
            el.setAttribute('data-toast-sig', text);
            el.classList.remove('is-leaving', 'is-gone');
            el.removeAttribute('aria-hidden');
            setTimeout(function () {
                if (el.getAttribute('data-toast-sig') !== text)
                    return;
                el.classList.add('is-leaving');
                setTimeout(function () {
                    if (el.getAttribute('data-toast-sig') !== text)
                        return;
                    el.classList.add('is-gone');
                    el.setAttribute('aria-hidden', 'true');
                }, 280);
            }, 5500);
        });
    }

    /**
     * Smooth scroll to anchor links
     */
    document.querySelectorAll('a[href^="#"]').forEach(function (anchor) {
        anchor.addEventListener('click', function (e) {
            var target = document.querySelector(this.getAttribute('href'));
            if (target) {
                e.preventDefault();
                target.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }
        });
    });
})();
