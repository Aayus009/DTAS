(function (window, document) {
    var overlay;
    var titleEl;
    var messageEl;
    var inputEl;
    var errorEl;
    var okBtn;
    var cancelBtn;
    var active = null;

    function ensure() {
        if (overlay) return;
        overlay = document.createElement("div");
        overlay.id = "dtas-dialog";
        overlay.className = "dtas-dialog-overlay";
        overlay.setAttribute("hidden", "hidden");
        overlay.innerHTML =
            '<div class="dtas-dialog-card" role="dialog" aria-modal="true" aria-labelledby="dtas-dialog-title">' +
            '  <h3 id="dtas-dialog-title" class="dtas-dialog-title"></h3>' +
            '  <p class="dtas-dialog-message"></p>' +
            '  <textarea class="dtas-dialog-input" rows="3" hidden></textarea>' +
            '  <p class="dtas-dialog-error" hidden></p>' +
            '  <div class="dtas-dialog-actions">' +
            '    <button type="button" class="dtas-dialog-cancel">Cancel</button>' +
            '    <button type="button" class="dtas-dialog-ok">OK</button>' +
            '  </div>' +
            "</div>";
        document.body.appendChild(overlay);
        titleEl = overlay.querySelector(".dtas-dialog-title");
        messageEl = overlay.querySelector(".dtas-dialog-message");
        inputEl = overlay.querySelector(".dtas-dialog-input");
        errorEl = overlay.querySelector(".dtas-dialog-error");
        okBtn = overlay.querySelector(".dtas-dialog-ok");
        cancelBtn = overlay.querySelector(".dtas-dialog-cancel");

        overlay.addEventListener("click", function (e) {
            if (e.target === overlay && active && active.allowCancel) finish(null);
        });
        cancelBtn.addEventListener("click", function () {
            if (active && active.allowCancel) finish(null);
        });
        okBtn.addEventListener("click", accept);
        inputEl.addEventListener("keydown", function (e) {
            if (e.key === "Enter" && !e.shiftKey) {
                e.preventDefault();
                accept();
            }
        });
        document.addEventListener("keydown", function (e) {
            if (!active) return;
            if (e.key === "Escape" && active.allowCancel) finish(null);
        });
    }

    function accept() {
        if (!active) return;
        if (active.mode === "prompt") {
            var value = (inputEl.value || "").trim();
            if (active.required && !value) {
                errorEl.textContent = active.requiredText || "This field is required.";
                errorEl.hidden = false;
                inputEl.focus();
                return;
            }
            finish(value);
            return;
        }
        finish(true);
    }

    function finish(result) {
        var cb = active && active.resolve;
        overlay.setAttribute("hidden", "hidden");
        overlay.classList.remove("is-open");
        inputEl.value = "";
        errorEl.hidden = true;
        active = null;
        if (cb) cb(result);
    }

    function open(options) {
        ensure();
        if (active && active.resolve) active.resolve(null);
        active = options;
        titleEl.textContent = options.title || "DTAS";
        messageEl.textContent = options.message || "";
        okBtn.textContent = options.okText || "OK";
        cancelBtn.textContent = options.cancelText || "Cancel";
        cancelBtn.hidden = !options.allowCancel;
        inputEl.hidden = options.mode !== "prompt";
        inputEl.placeholder = options.placeholder || "";
        inputEl.value = options.value || "";
        errorEl.hidden = true;
        overlay.removeAttribute("hidden");
        overlay.classList.add("is-open");
        setTimeout(function () {
            if (options.mode === "prompt") inputEl.focus();
            else okBtn.focus();
        }, 20);
    }

    function dialog(options) {
        return new Promise(function (resolve) {
            options.resolve = resolve;
            open(options);
        });
    }

    function fireControl(el) {
        if (!el) return;
        el.setAttribute("data-dtas-ok", "1");
        if (typeof el.click === "function") {
            el.click();
            return;
        }
        var href = el.getAttribute("href") || "";
        var match = href.match(/__doPostBack\('([^']*)','([^']*)'\)/);
        if (match && typeof window.__doPostBack === "function")
            window.__doPostBack(match[1], match[2]);
    }

    window.dtasAlert = function (message, title) {
        return dialog({
            mode: "alert",
            title: title || "Notice",
            message: message,
            okText: "OK",
            allowCancel: false
        });
    };

    window.dtasConfirm = function (elOrMessage, message) {
        var el = null;
        var text = message;
        if (typeof elOrMessage === "string" && typeof message === "undefined") {
            text = elOrMessage;
        } else if (elOrMessage && elOrMessage.nodeType) {
            el = elOrMessage;
            if (el.getAttribute("data-dtas-ok") === "1") {
                el.removeAttribute("data-dtas-ok");
                return true;
            }
        } else {
            text = elOrMessage;
        }

        dialog({
            mode: "confirm",
            title: "Please confirm",
            message: text || "Continue?",
            okText: "Confirm",
            cancelText: "Cancel",
            allowCancel: true
        }).then(function (ok) {
            if (ok && el) fireControl(el);
        });
        return false;
    };

    window.dtasPrompt = function (message, options) {
        options = options || {};
        return dialog({
            mode: "prompt",
            title: options.title || "Enter a value",
            message: message || "",
            okText: options.okText || "Continue",
            cancelText: options.cancelText || "Cancel",
            allowCancel: true,
            required: !!options.required,
            requiredText: options.requiredText || "This field is required.",
            placeholder: options.placeholder || "",
            value: options.value || ""
        });
    };

    window.dtasPromptPostback = function (el, fieldId, message, options) {
        if (el && el.getAttribute("data-dtas-ok") === "1") {
            el.removeAttribute("data-dtas-ok");
            return true;
        }
        options = options || {};
        window.dtasPrompt(message, options).then(function (value) {
            if (value === null) return;
            var field = document.getElementById(fieldId);
            if (field) field.value = value;
            fireControl(el);
        });
        return false;
    };
})(window, document);
