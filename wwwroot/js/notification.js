function fetchNotificationsCount(url, elementId) {
    console.log("Fetching data from: ", url); // Debug
    $.get(url, function (data) {
        console.log("Received data: ", data); // Debug
        if (data && typeof data.count !== 'undefined') {
            $('#' + elementId).text(data.count);
        } else {
            console.error("Invalid data structure: ", data); // Debug
        }
    }).fail(function () {
        console.error("Request failed for: ", url); // Debug
    });
}

$(document).ready(function () {
    fetchNotificationsCount('/Notifications/Notification', 'notificationsCount');
    fetchNotificationsCount('/Notifications/NotificationJobs', 'notificationsCountJobs');
    fetchNotificationsCount('/Notifications/NotificationApply', 'notificationsCountApply');

    setInterval(function () {
        fetchNotificationsCount('/Notifications/Notification', 'notificationsCount');
    }, 55500);

    setInterval(function () {
        fetchNotificationsCount('/Notifications/NotificationJobs', 'notificationsCountJobs');
    }, 55500);

    setInterval(function () {
        fetchNotificationsCount('/Notifications/NotificationApply', 'notificationsCountApply');
    }, 55500);
});
