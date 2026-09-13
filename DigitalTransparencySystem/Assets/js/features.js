/* ============================================
   EDUTRANSPARENCY - FEATURES PAGE SCRIPTS
   ============================================ */

document.addEventListener('DOMContentLoaded', function () {

    // --- Progress Bar Animation on Scroll ---
    const progressBars = document.querySelectorAll('.progress-bar');

    if ('IntersectionObserver' in window) {
        const progressObserver = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    const bar = entry.target;
                    const targetWidth = bar.getAttribute('style').match(/width:\s*(\d+%)/);
                    if (targetWidth) {
                        bar.style.width = '0%';
                        setTimeout(function () {
                            bar.style.transition = 'width 1.5s cubic-bezier(0.65, 0, 0.35, 1)';
                            bar.style.width = targetWidth[1];
                        }, 300);
                    }
                    progressObserver.unobserve(bar);
                }
            });
        }, { threshold: 0.5 });

        progressBars.forEach(function (bar) {
            progressObserver.observe(bar);
        });
    }

    // --- Bento Card Entrance Animation ---
    const bentoCards = document.querySelectorAll('.bento-card-hover');

    if ('IntersectionObserver' in window) {
        const bentoObserver = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry, index) {
                if (entry.isIntersecting) {
                    setTimeout(function () {
                        entry.target.style.opacity = '1';
                        entry.target.style.transform = 'translateY(0)';
                    }, index * 80);
                    bentoObserver.unobserve(entry.target);
                }
            });
        }, { threshold: 0.1 });

        bentoCards.forEach(function (card, index) {
            card.style.opacity = '1';
            card.style.transform = 'translateY(0)';
            card.style.transition = 'opacity 0.5s ease, transform 0.5s ease';
            bentoObserver.observe(card);
        });
    }

    // --- Subtle Hover Tilt Effect for Bento Cards ---
    bentoCards.forEach(card => {
        card.addEventListener('mousemove', (e) => {
            const rect = card.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            const centerX = rect.width / 2;
            const centerY = rect.height / 2;
            const rotateX = (y - centerY) / 40;
            const rotateY = (centerX - x) / 40;

            card.style.transform = `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) translateY(-4px)`;
        });

        card.addEventListener('mouseleave', () => {
            card.style.transform = '';
        });
    });

});