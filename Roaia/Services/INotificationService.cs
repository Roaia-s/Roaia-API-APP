namespace Roaia.Services;

public interface INotificationService
{
    Task<NotificationDto> SendMessageAsync(NotificationDto request);
    Task<IEnumerable<AppNotification>?> GetNotificationsByGlassesIdAsync(string glassesId);
    Task<NotificationDto> DeleteNotificationsAsync(string glassesId);
}
