import 'bootstrap';
import 'jquery-validation';
import 'jquery-validation-unobtrusive';
import '../css/site.css';

(function (app, storage) {
    app.contactInformationPersistence = {
        initialize: function (nameFieldId, emailFieldId) {
            function initFromLocalStorage(key, id) {
                if (!storage) {
                    return;
                }

                let value = storage.getItem(key),
                    element = document.getElementById(id);

                if (!element) {
                    return;
                }

                if (value) {
                    element.value = value;
                }

                element.addEventListener('change', function () {
                    value = element.value;
                    storage.setItem(key, value);
                });
            }

            initFromLocalStorage('name', nameFieldId);
            initFromLocalStorage('type', emailFieldId);
        }
    };

    app.setTooltips = function (selector) {
        $(selector).tooltip();
    };
})(window.app = (window.app || {}), localStorage);