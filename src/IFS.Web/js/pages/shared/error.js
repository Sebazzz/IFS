document.addEventListener('click', function (event) {
    if (event.target.id === 'reload-button') {
        event.preventDefault();
        document.location.reload(true);
    } else if (event.target.id === 'back-button') {
        event.preventDefault();
        history.back();
    }
});