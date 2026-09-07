/* ============================================
   EDUTRANSPARENCY - FAQ PAGE SCRIPTS
   ============================================ */

document.addEventListener('DOMContentLoaded', function () {

    // --- Accordion Toggle (Close Others) ---
    const detailsElements = document.querySelectorAll('.faq-accordion-item');

    detailsElements.forEach((targetDetail) => {
        targetDetail.addEventListener('click', () => {
            // Close other accordions
            detailsElements.forEach((detail) => {
                if (detail !== targetDetail) {
                    detail.removeAttribute('open');
                }
            });
        });
    });

    // --- Search Highlight Simulation ---
    const searchInput = document.querySelector('.faq-search');

    if (searchInput) {
        searchInput.addEventListener('input', (e) => {
            const term = e.target.value.toLowerCase().trim();
            const summaries = document.querySelectorAll('.faq-accordion-item h4');

            summaries.forEach(h4 => {
                const text = h4.textContent.toLowerCase();
                const parent = h4.closest('.faq-accordion-item');

                if (text.includes(term) || term === '') {
                    parent.style.display = 'block';
                    parent.style.opacity = '1';
                } else {
                    parent.style.opacity = '0';
                    setTimeout(() => {
                        parent.style.display = 'none';
                    }, 200);
                }
            });
        });
    }

    // --- Category Filter ---
    const categoryButtons = document.querySelectorAll('.category-btn');
    const faqItems = document.querySelectorAll('.faq-accordion-item');

    categoryButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            // Update active button
            categoryButtons.forEach(b => {
                b.classList.remove('bg-primary', 'text-on-primary');
                b.classList.add('text-on-surface-variant');
            });
            btn.classList.remove('text-on-surface-variant');
            btn.classList.add('bg-primary', 'text-on-primary');

            // Filter FAQ items
            const category = btn.getAttribute('data-category');

            faqItems.forEach(item => {
                const itemCategory = item.getAttribute('data-category');

                if (category === 'general' || itemCategory === category) {
                    item.style.display = 'block';
                    setTimeout(() => {
                        item.style.opacity = '1';
                    }, 50);
                } else {
                    item.style.opacity = '0';
                    setTimeout(() => {
                        item.style.display = 'none';
                    }, 200);
                }
            });
        });
    });

    // --- Smooth Scroll for Anchor Links ---
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