(function (global) {
    let uploadParameters = global.uploadParameters,
        uploadRoot = document.getElementById('upload-root'),
        errorIntervalTimerHandle = 0,
        trackerUrl = uploadParameters.trackerUrl;

    uploadRoot.querySelector('form').addEventListener('submit', function () {
        if (!this.checkValidity()) {
            return;
        }

        // If the iframe loads, and we weren't called back within a certain time,
        // we can assume there is some kind of server error.
        const uploadFrame = document.getElementById('uploadFrame');
        uploadFrame.addEventListener('load', function () {
            console.info('Upload frame has loaded...');
            errorIntervalTimerHandle = window.setTimeout(global.uploadCoordinator.assumeServerUploadError, 2000);
        });
        
        fetch(trackerUrl)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Bad network response');
                }

                return response.text();
            })
            .then(async html => {
                uploadRoot.innerHTML = html;
                await import('./tracker.js');
            })
            .catch(error => alert(error));
    });

    document.addEventListener('click', function (event) {
        if (event.target.classList.contains('reload-button')) {
            event.preventDefault();
            document.location.reload(true);
        }
    });

    document.querySelectorAll('.upload-control input[type=file]').forEach(function (input) {
        input.addEventListener('change', function () {
            let parent = input.parentElement;
            let nextElementWithDataValmsg = parent.nextElementSibling;

            if (nextElementWithDataValmsg) {
                nextElementWithDataValmsg.classList.remove('text-danger');
                nextElementWithDataValmsg.textContent = '';
                nextElementWithDataValmsg.classList.add('text-success', 'fa-solid', 'fa-check');
            }

            let files = this.files;
            let firstFile = files ? files[0] : undefined;

            if (firstFile && firstFile.size) {
                let suggestedFileSizeInput = document.querySelector('input[name="' + uploadParameters.names.suggestedFileSize + '"]');
                if (suggestedFileSizeInput) {
                    suggestedFileSizeInput.value = firstFile.size;
                }
            }
        });
    });

    (function () {
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
            uploadRoot.innerHTML = document.getElementById('upload-error').innerHTML;
        },
        complete: function (targetUrl) {
            clearTimeout(errorIntervalTimerHandle);

            uploadRoot.innerHTML = document.getElementById('upload-done').innerHTML;

            document.location.href = targetUrl;
        }
    };

    global.app.contactInformationPersistence.initialize(
        uploadParameters.ids.nameField,
        uploadParameters.ids.emailAddressField
    );
})(window);