document.addEventListener('DOMContentLoaded', function () {
    var detailsElements = document.querySelectorAll('.faq-accordion-item');

    detailsElements.forEach(function (targetDetail) {
        targetDetail.addEventListener('click', function () {
            detailsElements.forEach(function (detail) {
                if (detail !== targetDetail)
                    detail.removeAttribute('open');
            });
        });
    });

    var categoryButtons = document.querySelectorAll('.category-btn');
    var faqItems = document.querySelectorAll('.faq-accordion-item');

    categoryButtons.forEach(function (btn) {
        btn.addEventListener('click', function () {
            categoryButtons.forEach(function (b) {
                b.classList.remove('active', 'bg-pacific-blue', 'bg-primary', 'text-white', 'text-on-primary');
                b.classList.add('text-on-surface-variant');
            });
            btn.classList.remove('text-on-surface-variant');
            btn.classList.add('active');

            var category = btn.getAttribute('data-category');
            faqItems.forEach(function (item) {
                var itemCategory = item.getAttribute('data-category');
                var show = category === 'general' || itemCategory === category;
                item.style.display = show ? 'block' : 'none';
                item.style.opacity = show ? '1' : '0';
            });
        });
    });
});
