(function (global) {
    let $tracker = document.getElementById('uploadTracker'),
        $progress = $tracker.querySelector('.progress .progress-bar'),
        $textBlock = $tracker.querySelector('#uploadProgress'),
        $performance = $tracker.querySelector('#uploadPerformance'),
        firstTime = true;

    function updateProgress(incoming) {
        if (incoming.total === 0) {
            return;
        }

        if (incoming.total < 0) {
            $textBlock.textContent = 'Uploading ...';
            $performance.textContent = '(unable to determine speed)';
            return;
        }

        const percent = incoming.percent,
            currentKilobytes = Math.round(incoming.current / 1024),
            totalKilobytes = Math.round(incoming.total / 1024);

        $progress.style.width = percent + '%';
        $progress.querySelector('.sr-only').textContent = percent + '%';

        $textBlock.textContent = 'Uploading ' + currentKilobytes + '/' + totalKilobytes + ' KB (' + percent + '%)';
        $performance.textContent = incoming.performance;
    }

    function isTrackerRemoved() {
        return !$tracker.closest('body');
    }

    function triggerAjax() {
        if (isTrackerRemoved()) {
            return;
        }

        fetch(global.uploadParameters.uploadApi)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                updateProgress(data);
                global.setTimeout(triggerAjax, 250);
                firstTime = false;
            })
            .catch(_ => {
                if (firstTime) {
                    $textBlock.textContent = 'Waiting for progress information...';
                    $progress.style.width = '0%';
                } else {
                    $textBlock.textContent = 'Error updating progress...';
                }
                global.setTimeout(triggerAjax, 1000);
            });
    }

    triggerAjax();
})(window);
