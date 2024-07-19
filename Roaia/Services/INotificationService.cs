namespace Roaia.Services;

public interface INotificationService
{
    Task<NotificationDto> SendNotificationAsync(NotificationDto request);
    Task<IEnumerable<AppNotification>?> GetNotificationsByGlassesIdAsync(string glassesId);
    Task<NotificationDto> DeleteNotificationsAsync(string glassesId);
    Task<NotificationDto> DeleteNotificationAsync(int notificationId);
    Task<string> ToggleNotificationReadStatusAsync(int notificationId);
    Task<string> MarkAllNotificationsReadAsync(string glassesId);
}
