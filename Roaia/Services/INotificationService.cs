namespace Roaia.Services;

public interface INotificationService
{
    Task<MessageDto> SendMessageAsync(MessageDto request);
}
