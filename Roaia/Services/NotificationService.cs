using FirebaseAdmin.Messaging;
using Roaia.Settings;

namespace Roaia.Services;

public class NotificationService(ApplicationDbContext context, IOptions<NotificationSettings> notificationSettings) : INotificationService
{
    private readonly ApplicationDbContext _context = context;
    private readonly NotificationSettings _notificationSettings = notificationSettings.Value;

    public async Task<NotificationDto> SendNotificationAsync(NotificationDto request)
    {
        // Retrieve all device tokens
        var tokens = await _context.DeviceTokens
                                    .Where(t => t.GlassesId == request.GlassesId)
                                    .Select(t => t.Token)
                                    .ToListAsync();
        // SELECT FULL NAME OF GLASSES OWNER Note that the GlassesId is an string type
        var glasses = await _context.Glasses.FindAsync(request.GlassesId);

        //Filter out invalid tokens
        tokens = tokens.Where(t => !string.IsNullOrEmpty(t)).ToList();

        if (!tokens.Any())
            return new NotificationDto { Message = "No Device Tokens Found" };

        // Validate the ImageUrl
        if (!string.IsNullOrEmpty(request.ImageUrl) && !Uri.TryCreate(request.ImageUrl, UriKind.Absolute, out var validatedImageUrl))
            return new NotificationDto { Message = "Invalid Image URL" };

        // Validate the AudioUrl
        if (!string.IsNullOrEmpty(request.AudioUrl) && !Uri.TryCreate(request.AudioUrl, UriKind.Absolute, out var validatedAudioUrl))
            return new NotificationDto { Message = "Invalid Audio URL" };

        //switch request type is Critical,Normal or Warning
        var body = request.Type switch
        {
            "Critical" => $"{glasses.FullName} {request.Body}",
            "Normal" => request.Body.Replace("sama", glasses.FullName),
            "Warning" => request.Body,
            _ => throw new Exception("Invalid Notification Type")
        };


        //send messages
        var message = new MulticastMessage()
        {
            Tokens = tokens,
            Notification = new Notification
            {
                Title = request.Title,
                Body = body,
                //ImageUrl = request.ImageUrl ?? _notificationSettings.ImageUrl
            },
            Data = new Dictionary<string, string>
            {
                { "type", request.Type }
            },

            Android = new AndroidConfig
            {
                Notification = new AndroidNotification
                {
                    //ImageUrl = request.ImageUrl ?? _notificationSettings.ImageUrl,
                    Sound = request.AudioUrl ?? "default",
                    //ClickAction = "FLUTTER_NOTIFICATION_CLICK"
                }
            },
            Apns = new ApnsConfig
            {
                Aps = new Aps
                {
                    Sound = request.AudioUrl ?? "default"
                }
            }
        };

        var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);

        //check if message was sent successfully and save to database?
        if (response.SuccessCount > 0)
        {
            var notification = new AppNotification
            {
                Title = request.Title,
                Body = body,
                ImageUrl = request.ImageUrl,
                AudioUrl = request.AudioUrl,
                GlassesId = request.GlassesId,
                Type = request.Type,
                IsRead = false
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        if (response.FailureCount > 0)
        {
            var failedTokens = response.Responses
                                  .Where(r => !r.IsSuccess)
                                  .Select((r, idx) => tokens[idx])
                                  .ToList();
            // Optionally, remove or mark invalid tokens in the database
            foreach (var token in failedTokens)
            {
                var deviceToken = await _context.DeviceTokens.SingleOrDefaultAsync(t => t.Token == token);
                if (deviceToken is not null)
                    _context.DeviceTokens.Remove(deviceToken);
            }

            await _context.SaveChangesAsync();

            return new NotificationDto { Message = $"Error in sending Notification, There are often invalid tokens that are now being deleted! Success Count: {response.SuccessCount} | Failure Count: {response.FailureCount}" };
        }

        return new NotificationDto();
    }

    public async Task<IEnumerable<AppNotification>?> GetNotificationsByGlassesIdAsync(string glassesId)
    {
        if (!await _context.Glasses.AnyAsync(g => g.Id == glassesId))
            return null;

        return await _context.Notifications
                            .Where(n => n.GlassesId == glassesId)
                            .OrderByDescending(n => n.CreatedAt)
                            .ThenBy(n => n.Id)
                            .ToListAsync();
    }
    public async Task<NotificationDto> DeleteNotificationsAsync(string glassesId)
    {
        //if glassesId is not found
        if (!await _context.Glasses.AnyAsync(g => g.Id == glassesId))
            return new NotificationDto { Message = "Glasses Id Not Found" };

        var notifications = await _context.Notifications
                                        .Where(n => n.GlassesId == glassesId)
                                        .ToListAsync();

        if (!notifications.Any())
            return new NotificationDto { Message = "No Notifications Found" };

        _context.Notifications.RemoveRange(notifications);
        await _context.SaveChangesAsync();

        return new NotificationDto();
    }

    public async Task<NotificationDto> DeleteNotificationAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);

        if (notification is null)
            return new NotificationDto { Message = "Notification Not Found" };

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        return new NotificationDto();
    }

    public async Task<string> ReadNotificationAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);

        if (notification is null)
            return "Notification Not Found";

        notification.IsRead = true;
        await _context.SaveChangesAsync();

        return "Notification Read";
    }

    public async Task<string> ReadAllNotificationsAsync(string glassesId)
    {
        var notifications = await _context.Notifications
                                        .Where(n => n.GlassesId == glassesId)
                                        .ToListAsync();

        if (!notifications.Any())
            return "No Notifications Found";

        foreach (var notification in notifications)
            notification.IsRead = true;

        await _context.SaveChangesAsync();

        return "All Notifications Read";
    }
}
