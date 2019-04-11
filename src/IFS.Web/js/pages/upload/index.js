import * as $ from 'jquery';

(function (global) {
    var uploadParameters = global.uploadParameters,
        $uploadRoot = $('#upload-root'),
        errorIntervalTimerHandle = 0,
        trackerUrl = uploadParameters.trackerUrl;

    $uploadRoot.find('form').submit(function () {
        if (!this.checkValidity()) {
            return;
        }

        $uploadRoot.load(trackerUrl);

        // If the iframe loads, and we weren't called back within a certain time,
        // we can assume there is some kind of server error.
        $('#uploadFrame').on('load', function () {
            console.info('Upload frame has loaded...');

            errorIntervalTimerHandle = window.setTimeout(global.uploadCoordinator.assumeServerUploadError, 2000);
        });
    });

    $(document).on('click', '.reload-button', function (ev) {
        ev.preventDefault();

        document.location.reload(true);
    });

    $('.upload-control input[type=file]').change(function () {
        $(this).parent().next('[data-valmsg-for]')
            .removeClass('text-danger')
            .text('')
            .addClass('text-success')
            .addClass('fas')
            .addClass('fa-check');

        var fileInput = this;
        var files = fileInput.files;
        var firstFile = (files || [])[0];

        if (firstFile && firstFile.size) {
            $('input[name="'+uploadParameters.names.suggestedFileSize+'"]').val(firstFile.size);
        }
    });

    (function() {
        const pPasswordProtect = document.getElementById('pPasswordProtect'),
              cbPasswordProtect = document.getElementById('cbPasswordProtect');

        let isFirstTimeShowingPanel = true;

        function setState() {
            const showPanel = cbPasswordProtect.checked,
                  removeClazzName = showPanel ? 'hidden' : 'visible',
                  addClazzName = showPanel ? 'visible' : 'hidden';

            pPasswordProtect.classList.add(addClazzName);
            pPasswordProtect.classList.remove(removeClazzName);

            if (showPanel && isFirstTimeShowingPanel) {
                isFirstTimeShowingPanel = false;

                const passwordBox = pPasswordProtect.querySelector('input[type=password]');
                if (passwordBox) {
                    passwordBox.value = ''; // Stop browsers from pre-filling it
                }
            }
        }

        setState();
        cbPasswordProtect.addEventListener('change', setState);
    })();

    global.uploadCoordinator = {
        validationError: function (text) {
            alert(text);

            document.location.reload(true);
        },

        assumeServerUploadError: function () {
            $uploadRoot.html($('#upload-error').html());
        },
        complete: function (targetUrl) {
            clearTimeout(errorIntervalTimerHandle);

            $uploadRoot.html($('#upload-done').html());

            document.location.href = targetUrl;
        }
    };

    global.app.contactInformationPersistence.initialize(
        uploadParameters.ids.nameField,
        uploadParameters.ids.emailAddressField
    );
})(window);