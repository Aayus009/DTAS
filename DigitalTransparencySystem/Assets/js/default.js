document.addEventListener('DOMContentLoaded', function () {
    var reduce = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    function reveal(selector, extraClass) {
        var nodes = document.querySelectorAll(selector);
        if (!nodes.length) return;
        if (reduce || !('IntersectionObserver' in window)) {
            nodes.forEach(function (n) { n.classList.add('is-in', extraClass || 'is-visible'); });
            return;
        }
        var obs = new IntersectionObserver(function (entries) {
            entries.forEach(function (e) {
                if (!e.isIntersecting) return;
                e.target.classList.add('is-in');
                if (extraClass) e.target.classList.add(extraClass);
                obs.unobserve(e.target);
            });
        }, { threshold: 0.16, rootMargin: '0px 0px -40px 0px' });
        nodes.forEach(function (n) { obs.observe(n); });
    }

    reveal('.lp-reveal');
    reveal('.step-card-sm', 'is-visible');
    reveal('.lp-role, .lp-module');
    reveal('.lp-snapshot');

    var statsSection = document.querySelector('.lp-metrics');
    var statNums = document.querySelectorAll('.metric-stat');
    var counted = false;

    function countUp() {
        if (counted) return;
        counted = true;
        statNums.forEach(function (el) {
            var raw = el.textContent;
            var target = parseInt(raw.replace(/\D/g, ''), 10) || 0;
            var suffix = raw.replace(/[\d,]/g, '');
            if (reduce) {
                el.textContent = target + suffix;
                return;
            }
            var t0 = performance.now();
            (function tick(now) {
                var p = Math.min((now - t0) / 1600, 1);
                var ease = 1 - Math.pow(1 - p, 3);
                el.textContent = Math.floor(target * ease) + suffix;
                if (p < 1) requestAnimationFrame(tick);
                else el.textContent = target + suffix;
            })(t0);
        });
    }

    if (statsSection && 'IntersectionObserver' in window && !reduce) {
        var sObs = new IntersectionObserver(function (entries) {
            entries.forEach(function (e) {
                if (e.isIntersecting) {
                    countUp();
                    sObs.unobserve(e.target);
                }
            });
        }, { threshold: 0.4 });
        sObs.observe(statsSection);
    } else {
        countUp();
    }
});
