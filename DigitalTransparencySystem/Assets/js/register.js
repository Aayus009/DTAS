/* ============================================
   Register Page Scripts
   DTAS - Digital Transparency and Accountability System
   ============================================ */

(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        initRegisterForm();
    });

    function initRegisterForm() {
        var form = document.getElementById('registerForm');
        if (!form) return;

        form.addEventListener('submit', function (e) {
            var btn = form.querySelector('button[type="submit"]');
            if (btn) {
                btn.disabled = true;
                btn.innerHTML = '<span class="material-symbols-outlined animate-spin">progress_activity</span> Creating Account...';
            }
        });
    }

})();

function validateCharter(source, args) {
    var box = document.querySelector('input[id$="chkCharter"]');
    args.IsValid = !!(box && box.checked);
}
