/* ============================================
   Dashboard Page Scripts
   ============================================ */

(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        if (window.__dtasDashboardInit) return;
        window.__dtasDashboardInit = true;

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
     * Live header search across tasks, meetings, and events
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
        var panel = box.querySelector('.js-dtas-search-results');
        var url = box.getAttribute('data-search-url');
        if (!input || !panel || !url) return;

        var timer = null;
        var active = -1;
        var hits = [];

        input.addEventListener('input', function () {
            active = -1;
            var q = (input.value || '').trim();
            if (timer) window.clearTimeout(timer);
            if (q.length < 2) {
                hidePanel();
                return;
            }
            timer = window.setTimeout(function () { runSearch(q); }, 180);
        });

        input.addEventListener('keydown', function (e) {
            if (panel.classList.contains('hidden')) return;
            if (e.key === 'ArrowDown') {
                e.preventDefault();
                move(1);
            } else if (e.key === 'ArrowUp') {
                e.preventDefault();
                move(-1);
            } else if (e.key === 'Enter') {
                if (active >= 0 && hits[active]) {
                    e.preventDefault();
                    window.location.href = hits[active].url;
                }
            } else if (e.key === 'Escape') {
                hidePanel();
                input.blur();
            }
        });

        document.addEventListener('click', function (e) {
            if (!box.contains(e.target)) hidePanel();
        });

        function runSearch(q) {
            fetch(url + '?q=' + encodeURIComponent(q), {
                credentials: 'same-origin',
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            })
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    if ((input.value || '').trim() !== q) return;
                    hits = (data && data.items) ? data.items : [];
                    render();
                })
                .catch(function () {
                    hits = [];
                    panel.innerHTML = '<div class="dtas-search-empty">Search is unavailable right now.</div>';
                    panel.classList.remove('hidden');
                });
        }

        function render() {
            if (!hits.length) {
                panel.innerHTML = '<div class="dtas-search-empty">No matching tasks, meetings, or events.</div>';
                panel.classList.remove('hidden');
                return;
            }

            var html = '';
            var lastType = '';
            hits.forEach(function (hit, i) {
                if (hit.type !== lastType) {
                    lastType = hit.type;
                    html += '<div class="dtas-search-group">' + escapeHtml(hit.type) + 's</div>';
                }
                html += '<a class="dtas-search-hit' + (i === active ? ' is-active' : '') + '" href="' + escapeAttr(hit.url) + '" data-i="' + i + '">'
                    + '<span class="dtas-search-hit-icon material-symbols-outlined">' + escapeHtml(hit.icon || 'search') + '</span>'
                    + '<span><span class="dtas-search-hit-title">' + escapeHtml(hit.title || '') + '</span>'
                    + '<span class="dtas-search-hit-sub">' + escapeHtml(hit.subtitle || '') + '</span></span></a>';
            });
            panel.innerHTML = html;
            panel.classList.remove('hidden');
        }

        function move(delta) {
            if (!hits.length) return;
            active = (active + delta + hits.length) % hits.length;
            var nodes = panel.querySelectorAll('.dtas-search-hit');
            for (var i = 0; i < nodes.length; i++) {
                if (i === active) nodes[i].classList.add('is-active');
                else nodes[i].classList.remove('is-active');
            }
            if (nodes[active]) nodes[active].scrollIntoView({ block: 'nearest' });
        }

        function hidePanel() {
            panel.classList.add('hidden');
            panel.innerHTML = '';
            active = -1;
        }
    }

    function escapeHtml(value) {
        return String(value || '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function escapeAttr(value) {
        return escapeHtml(value);
    }

    /**
     * Animate progress bars on page load
     */
    function initProgressBars() {
        var root = document.querySelector('.dashboard-main');
        if (!root) return;
        var progressBars = root.querySelectorAll('.js-progress-fill');

        progressBars.forEach(function (bar) {
            var targetWidth = bar.style.width;
            bar.style.width = '0%';
            bar.style.transition = 'width 1s cubic-bezier(0.4, 0, 0.2, 1)';

            setTimeout(function () {
                bar.style.width = targetWidth;
            }, 200);
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

    /**
     * Auto-hide toast notifications after 5 seconds
     */
    var toasts = document.querySelectorAll('.toast-notification');
    toasts.forEach(function (toast) {
        setTimeout(function () {
            toast.style.opacity = '0';
            toast.style.transform = 'translateY(16px)';
            setTimeout(function () {
                toast.remove();
            }, 300);
        }, 5000);
    });

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
