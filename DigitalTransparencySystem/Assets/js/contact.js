document.addEventListener('DOMContentLoaded', function () {
    var formInputs = document.querySelectorAll('.form-input');

    formInputs.forEach(function (el) {
        el.addEventListener('focus', function () {
            var label = el.parentElement.querySelector('label');
            if (label)
                label.classList.add('text-primary');
        });

        el.addEventListener('blur', function () {
            var label = el.parentElement.querySelector('label');
            if (label)
                label.classList.remove('text-primary');
        });
    });
});
