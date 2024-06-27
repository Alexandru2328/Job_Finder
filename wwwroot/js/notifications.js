
    function fetchNotificationsCount() {
        $.get('/Notifications/Notification', function (data) {
            $('#notificationsCount').text(data.count);
        });
    }

    setInterval(fetchNotificationsCount, 1000);
