/* ============================================
   EDUTRANSPARENCY - CONTACT PAGE SCRIPTS
   ============================================ */

document.addEventListener('DOMContentLoaded', function () {

    // --- Form Input Focus Effects ---
    const formInputs = document.querySelectorAll('input, select, textarea');

    formInputs.forEach(el => {
        el.addEventListener('focus', () => {
            const label = el.parentElement.querySelector('label');
            if (label) {
                label.classList.add('text-primary');
                label.classList.remove('text-secondary');
            }
        });

        el.addEventListener('blur', () => {
            const label = el.parentElement.querySelector('label');
            if (label) {
                label.classList.remove('text-primary');
                label.classList.add('text-secondary');
            }
        });
    });

    // --- Form Submission Simulation ---
    const form = document.querySelector('form');
    if (form) {
        form.addEventListener('submit', (e) => {
            e.preventDefault();
            const btn = form.querySelector('button[type="submit"], input[type="submit"]');

            if (btn) {
                const originalContent = btn.innerHTML;
                btn.innerHTML = '<span class="material-symbols-outlined animate-spin">sync</span> Validating...';
                btn.disabled = true;

                setTimeout(() => {
                    btn.innerHTML = '<span class="material-symbols-outlined">check_circle</span> Inquiry Submitted';
                    btn.classList.replace('bg-primary', 'bg-on-tertiary-container');
                    btn.classList.add('text-on-primary');
                    form.reset();
                }, 1500);
            }
        });
    }

    // --- Newsletter Signup ---
    const newsletterForm = document.querySelector('.newsletter-form');
    if (newsletterForm) {
        newsletterForm.addEventListener('submit', (e) => {
            e.preventDefault();
            const btn = newsletterForm.querySelector('button');
            const input = newsletterForm.querySelector('input');

            if (btn && input) {
                btn.textContent = 'Subscribed!';
                btn.classList.add('bg-on-tertiary-container');
                input.value = '';

                setTimeout(() => {
                    btn.textContent = 'Sign Up';
                    btn.classList.remove('bg-on-tertiary-container');
                }, 3000);
            }
        });
    }

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