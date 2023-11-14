import * as $ from 'jquery';

$(document).on('click', '#reload-button', function (ev) {
    ev.preventDefault();

    document.location.reload(true);
});

$(document).on('click', '#back-button', function (ev) {
    ev.preventDefault();

    history.back();
});