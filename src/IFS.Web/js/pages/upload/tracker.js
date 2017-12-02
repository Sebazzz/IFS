import * as $ from 'jquery';
(function () {
    var $tracker = $('#uploadTracker'),
        $progress = $tracker.find('.progress .progress-bar'),
        $textBlock = $tracker.find('#uploadProgress'),
        $performance = $tracker.find('#uploadPerformance'),
        firstTime = true;

    function updateProgress(incoming) {
        if (incoming.total === 0) {
            return;
        }

        if (incoming.total < 0) {
            $textBlock.text('Uploading ...');
            $performance.text('(unable to determine speed)');
            return;
        }

        var percent = incoming.percent,
            currentkb = Math.round(incoming.current / 1024),
            totalkb = Math.round(incoming.total / 1024);

        $progress
            .css('width', percent + '%')
            .find('.sr-only').text(percent + '%');

        $textBlock
            .text('Uploading ' + currentkb + '/' + totalkb + ' KB (' + percent + '%)');

        $performance.text(incoming.performance);
    }

    function isTrackerRemoved() {
        return $tracker.closest('body').length === 0;
    }

    function triggerAjax() {
        if (isTrackerRemoved()) {
            return;
        }

        $.getJSON(global.scriptParams.uploadApi, {})
            .success(function(data) {
                if (firstTime) {
                    $progress.stop();

                    firstTime = false;
                }

                updateProgress(data);

                global.setTimeout(triggerAjax, 250);
            })
            .fail(function() {
                if (firstTime) {
                    $textBlock.text('Waiting for progress information...');

                    $progress.animate({ width: '0%' }, 100);
                } else {
                    $textBlock.text('Error updating progress...');
                }

                global.setTimeout(triggerAjax, 1000);
            });
    }

    triggerAjax();
})(window);
