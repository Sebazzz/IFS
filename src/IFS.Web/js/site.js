import * as bootstrap from 'bootstrap';
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
        const elements = document.querySelectorAll(selector);

        for (const element of elements) {
            const tooltip = new bootstrap.Tooltip(element, {});
            console.debug('Initialized tooltip %s', tooltip.tip);
        }
    };
})(window.app = (window.app || {}), localStorage);