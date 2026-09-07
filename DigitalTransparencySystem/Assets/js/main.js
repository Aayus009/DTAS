/* ============================================
   EDUTRANSPARENCY - GLOBAL SCRIPTS (main.js)
   ============================================ */

(function () {
    'use strict';

    var THEME_KEY = 'dtas-theme';

    /**
     * Read a resolved DT theme token (hex) from CSS custom properties.
     * Useful for canvas/chart colors that cannot consume CSS vars directly.
     */
    window.dtCss = function (name, fallback) {
        var value = getComputedStyle(document.documentElement).getPropertyValue(name).trim();
        return value || fallback || '';
    };

    /**
     * Resolve the current active theme.
     */
    function getTheme() {
        return document.documentElement.classList.contains('dark') ? 'dark' : 'light';
    }

    /**
     * Apply a theme ('light' | 'dark' | 'system') to the document.
     */
    function isPublicSite() {
        var path = (location.pathname || '/').toLowerCase();
        return path === '/' || path === ''
            || /\/(default|features|about|faq|contact)\.aspx$/i.test(path)
            || /\/(loginpopup|registerpopup)\.aspx$/i.test(path);
    }

    function applyTheme(theme) {
        var resolved = theme;
        if (theme === 'system') {
            resolved = (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) ? 'dark' : 'light';
        }
        if (isPublicSite()) {
            resolved = 'light';
        }
        document.documentElement.classList.toggle('dark', resolved === 'dark');
        syncToggleIcons();
        try { localStorage.setItem(THEME_KEY, theme); } catch (e) { }
        document.dispatchEvent(new CustomEvent('themechange', { detail: { theme: resolved } }));
    }

    /**
     * Flip the light/dark icons on every theme toggle button.
     */
    function syncToggleIcons() {
        var dark = getTheme() === 'dark';
        document.querySelectorAll('.theme-toggle').forEach(function (btn) {
            var lightIcon = btn.querySelector('.theme-icon-light');
            var darkIcon = btn.querySelector('.theme-icon-dark');
            /* Show the destination: sun while dark (switch to light), moon while light. */
            if (lightIcon) lightIcon.classList.toggle('hidden', !dark);
            if (darkIcon) darkIcon.classList.toggle('hidden', dark);
        });
    }

    /**
     * Wire up theme toggle buttons + system preference changes.
     */
    function initTheme() {
        var saved = null;
        try { saved = localStorage.getItem(THEME_KEY); } catch (e) { }
        applyTheme(saved || 'system');

        document.addEventListener('click', function (e) {
            var btn = e.target.closest ? e.target.closest('.theme-toggle') : null;
            if (btn) {
                var next = getTheme() === 'dark' ? 'light' : 'dark';
                document.documentElement.classList.add('theme-transition');
                applyTheme(next);
                setTimeout(function () {
                    document.documentElement.classList.remove('theme-transition');
                }, 300);
            }
        });

        if (window.matchMedia) {
            window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function (e) {
                var saved = null;
                try { saved = localStorage.getItem(THEME_KEY); } catch (err) { }
                if (!saved || saved === 'system') applyTheme('system');
            });
        }
    }

    /**
     * Wire up scroll-triggered reveal animations for common card elements.
     * Classes are added dynamically so nothing is hidden if JS is disabled.
     */
    function initScrollReveal() {
        if (!('IntersectionObserver' in window)) return;

        var selector = '.glass-card, .standard-card, .decision-card, .vault-card, .event-card, .bento-card-hover, .feature-card, .stat-card';
        var els = [];

        document.querySelectorAll('main ' + selector).forEach(function (el) {
            if (el.classList.contains('animate-in') || el.classList.contains('section-enter')) return;
            if (el.closest('.step-card')) return;
            el.classList.add('reveal-up');
            els.push(el);
        });

        // Stagger siblings so grids cascade nicely.
        var buckets = {};
        els.forEach(function (el) {
            var key = el.parentNode;
            if (!buckets[key]) buckets[key] = [];
            buckets[key].push(el);
        });
        Object.keys(buckets).forEach(function (parent) {
            buckets[parent].forEach(function (el, i) {
                el.style.transitionDelay = Math.min(i * 90, 450) + 'ms';
            });
        });

        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('revealed');
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.1, rootMargin: '0px 0px -30px 0px' });

        els.forEach(function (el) { observer.observe(el); });
    }


    document.addEventListener('DOMContentLoaded', function () {

        initTheme();
        initScrollReveal();

        // --- Mobile Menu Toggle ---
    const navToggle = document.getElementById('navToggle');
    const mobileMenu = document.getElementById('mobileMenu');

    if (navToggle && mobileMenu) {
        navToggle.addEventListener('click', function () {
            mobileMenu.classList.toggle('hidden');
            const icon = navToggle.querySelector('.material-symbols-outlined');
            if (icon) {
                icon.textContent = mobileMenu.classList.contains('hidden') ? 'menu' : 'close';
            }
        });
    }

    // --- Active Navigation Link Highlighter ---
    const currentPath = window.location.pathname.toLowerCase();
    const navLinks = document.querySelectorAll('nav a[id^="nav"]');

    navLinks.forEach(function (link) {
        const href = link.getAttribute('href');
        if (href) {
            const pageName = href.toLowerCase().replace('.aspx', '').replace('/', '');
            const currentPage = currentPath.replace('.aspx', '').replace('/', '');

            if (currentPage === pageName || (currentPage === '' && pageName === 'default') || (currentPage === 'default' && pageName === '')) {
                navLinks.forEach(function (l) { l.classList.remove('is-active'); });
                link.classList.add('is-active');
            }
        }
    });

    // --- Close mobile menu on window resize ---
    window.addEventListener('resize', function () {
        if (window.innerWidth >= 768 && mobileMenu) {
            mobileMenu.classList.add('hidden');
        }
    });

    // --- Smooth scroll for anchor links ---
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            if (href !== '#') {
                const target = document.querySelector(href);
                if (target) {
                    e.preventDefault();
                    target.scrollIntoView({ behavior: 'smooth' });
                }
            }
        });
    });

    });

})();
