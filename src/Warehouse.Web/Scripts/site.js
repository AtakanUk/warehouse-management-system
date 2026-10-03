/* Warehouse WMS – small progressive enhancements on top of ASP.NET Web Forms. */
(function () {
    'use strict';

    // Confirmation for destructive actions: <a data-confirm="Delete X?">.
    document.addEventListener('click', function (event) {
        var trigger = event.target.closest('[data-confirm]');
        if (trigger && !window.confirm(trigger.getAttribute('data-confirm'))) {
            event.preventDefault();
            event.stopImmediatePropagation();
        }
    }, true);

    // Mirror ASP.NET validator results on the inputs so Bootstrap can style them.
    if (typeof window.ValidatorUpdateDisplay === 'function') {
        var updateDisplay = window.ValidatorUpdateDisplay;
        window.ValidatorUpdateDisplay = function (validator) {
            updateDisplay(validator);
            if (validator.style.display === 'inline') {
                validator.style.display = 'block'; // messages sit below their field
            }
            var input = document.getElementById(validator.controltovalidate);
            if (input && input.Validators) {
                var invalid = Array.prototype.some.call(input.Validators, function (v) { return !v.isvalid; });
                input.classList.toggle('is-invalid', invalid);
                input.setAttribute('aria-invalid', invalid ? 'true' : 'false');
            }
        };
    }

    // Prevent double submissions: disable the clicked button once the form is really posting.
    var form = document.getElementById('MainForm');
    if (form) {
        form.addEventListener('submit', function (event) {
            var button = event.submitter;
            var pageIsValid = typeof window.Page_IsValid === 'undefined' || window.Page_IsValid;
            if (button && pageIsValid) {
                window.setTimeout(function () {
                    button.disabled = true;
                    button.classList.add('is-busy');
                }, 0);
            }
        });

        // Re-enable buttons when the page is restored from the back/forward cache.
        window.addEventListener('pageshow', function () {
            Array.prototype.forEach.call(form.querySelectorAll('.is-busy'), function (button) {
                button.disabled = false;
                button.classList.remove('is-busy');
            });
        });
    }
})();
