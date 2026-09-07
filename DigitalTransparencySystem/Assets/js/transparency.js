document.addEventListener('DOMContentLoaded', function () {
    var filterButtons = document.querySelectorAll('.tx-filter');
    var decisionCards = document.querySelectorAll('.tx-decision');

    filterButtons.forEach(function (btn) {
        btn.addEventListener('click', function () {
            filterButtons.forEach(function (item) {
                item.classList.toggle('is-on', item === btn);
            });

            var filter = btn.getAttribute('data-filter');
            decisionCards.forEach(function (card) {
                var show = filter === 'all' || card.getAttribute('data-status') === filter;
                card.style.display = show ? '' : 'none';
            });
        });
    });
});
