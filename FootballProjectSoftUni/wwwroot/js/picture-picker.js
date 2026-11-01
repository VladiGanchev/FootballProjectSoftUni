// Picture fields (_PicturePicker): shows the name of the chosen file, refuses a wrong type or a too large file
// right away (nothing is uploaded), and hands the picture to the page's preview with two events on the picker:
// 'picture:chosen' (detail.url = the picture as a data: URL) and 'picture:cleared'.
(function () {
    'use strict';

    var extensions = /\.(jpe?g|png|gif|webp)$/i;

    function setup(picker) {
        var input = picker.querySelector('input[type="file"]');
        var nameEl = picker.querySelector('[data-picture-name]');
        var form = picker.closest('form');
        var error = form ? form.querySelector('[data-picture-error]') : null;
        if (!input) { return; }

        var noFile = nameEl ? nameEl.textContent : '';
        var maxBytes = parseInt(picker.getAttribute('data-max-bytes'), 10) || 0;
        var types = (input.getAttribute('accept') || '').split(',');

        function message(text) {
            if (!error) { return; }
            error.textContent = text;
            error.classList.toggle('field-validation-error', !!text);
            error.classList.toggle('field-validation-valid', !text);
        }

        function clear() {
            if (nameEl) { nameEl.textContent = noFile; }
            picker.classList.remove('is-chosen');
            picker.dispatchEvent(new CustomEvent('picture:cleared', { bubbles: true }));
        }

        function refuse(template, file) {
            input.value = '';
            clear();
            message(template.replace('{0}', file.name).replace('{1}', Math.round(maxBytes / 1048576)));
        }

        input.addEventListener('change', function () {
            var file = input.files && input.files[0];
            if (!file) { message(''); clear(); return; }

            // Some systems don't know the type of a .webp file, so its name counts too (the server checks the content).
            if (types.indexOf(file.type) < 0 && !extensions.test(file.name)) {
                refuse(picker.getAttribute('data-msg-type') || '', file);
                return;
            }

            if (maxBytes && file.size > maxBytes) {
                refuse(picker.getAttribute('data-msg-size') || '', file);
                return;
            }

            message('');
            if (nameEl) { nameEl.textContent = file.name; }
            picker.classList.add('is-chosen');

            // A data: URL, not blob: - the site's security rules allow pictures only from itself and data:.
            var reader = new FileReader();
            reader.onload = function () {
                if (input.files && input.files[0] === file) {
                    picker.dispatchEvent(new CustomEvent('picture:chosen', { bubbles: true, detail: { url: reader.result } }));
                }
            };
            reader.readAsDataURL(file);
        });
    }

    Array.prototype.forEach.call(document.querySelectorAll('[data-picture-picker]'), setup);
})();
