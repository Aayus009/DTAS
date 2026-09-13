/* ============================================
   EDUTRANSPARENCY - EVENTS PAGE SCRIPTS
   ============================================ */

document.addEventListener('DOMContentLoaded', function () {

    // --- Filter Tabs Functionality ---
    const filterButtons = document.querySelectorAll('.filter-btn');
    const eventCards = document.querySelectorAll('.event-card');

    filterButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            // Update active button styling
            filterButtons.forEach(b => {
                b.classList.remove('bg-primary', 'text-on-primary');
                b.classList.add('bg-surface-container', 'text-secondary');
            });
            btn.classList.remove('bg-surface-container', 'text-secondary');
            btn.classList.add('bg-primary', 'text-on-primary');

            // Filter events
            const filter = btn.getAttribute('data-filter');

            eventCards.forEach(card => {
                if (filter === 'all') {
                    card.style.display = 'flex';
                    setTimeout(() => {
                        card.style.opacity = '1';
                        card.style.transform = 'translateY(0)';
                    }, 50);
                } else {
                    const categories = card.getAttribute('data-category');
                    if (categories && categories.includes(filter)) {
                        card.style.display = 'flex';
                        setTimeout(() => {
                            card.style.opacity = '1';
                            card.style.transform = 'translateY(0)';
                        }, 50);
                    } else {
                        card.style.opacity = '0';
                        card.style.transform = 'translateY(10px)';
                        setTimeout(() => {
                            card.style.display = 'none';
                        }, 300);
                    }
                }
            });
        });
    });

    // --- Event Card Entrance Animation ---
    if ('IntersectionObserver' in window) {
        const cardObserver = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry, index) {
                if (entry.isIntersecting) {
                    setTimeout(function () {
                        entry.target.style.opacity = '1';
                        entry.target.style.transform = 'translateY(0)';
                    }, index * 100);
                    cardObserver.unobserve(entry.target);
                }
            });
        }, { threshold: 0.1 });

        eventCards.forEach(function (card) {
            card.style.opacity = '1';
            card.style.transform = 'translateY(0)';
            card.style.transition = 'opacity 0.4s ease, transform 0.4s ease';
            cardObserver.observe(card);
        });
    }

    // --- Calendar Navigation ---
    const prevBtn = document.querySelector('aside button:first-child');
    const nextBtn = document.querySelector('aside button:nth-child(2)');

    if (prevBtn && nextBtn) {
        prevBtn.addEventListener('click', function () {
            // Navigate to previous month
            console.log('Previous month');
        });

        nextBtn.addEventListener('click', function () {
            // Navigate to next month
            console.log('Next month');
        });
    }

    // --- Date Picker ---
    const dateInput = document.querySelector('input[type="date"]');
    if (dateInput) {
        dateInput.addEventListener('change', function () {
            const selectedDate = this.value;
            console.log('Jump to date:', selectedDate);
            // Implement date jump logic
        });
    }

    // --- Calendar Day Click ---
    const calendarDays = document.querySelectorAll('.grid.grid-cols-7 > div:not(:first-child)');
    calendarDays.forEach(day => {
        if (!day.classList.contains('opacity-30')) {
            day.classList.add('calendar-day');
            day.addEventListener('click', function () {
                const dayNum = this.textContent;
                console.log('Selected day:', dayNum);
                // Implement day selection logic
            });
        }
    });

});