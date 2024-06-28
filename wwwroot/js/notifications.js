
    function fetchNotificationsCount() {
        $.get('/Notifications/Notification', function (data) {
            $('#notificationsCount').text(data.count);
        });
    }
    fetchNotificationsCount();

    setInterval(fetchNotificationsCount, 100);
