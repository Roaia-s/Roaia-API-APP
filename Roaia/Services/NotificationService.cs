using FirebaseAdmin.Messaging;

namespace Roaia.Services;

public class NotificationService(ApplicationDbContext context) : INotificationService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<MessageDto> SendMessageAsync(MessageDto request)
    {
        // Retrieve all device tokens
        var tokens = await _context.DeviceTokens
                                    .Where(t => t.GlassesId == request.GlassesId)
                                    .Select(t => t.Token)
                                    .ToListAsync();
        //Filter out invalid tokens
        tokens = tokens.Where(t => !string.IsNullOrEmpty(t)).ToList();

        if (!tokens.Any())
            return new MessageDto { Message = "No Device Tokens Found" };

        // Validate the ImageUrl
        if (!string.IsNullOrEmpty(request.ImageUrl) && !Uri.TryCreate(request.ImageUrl, UriKind.Absolute, out var validatedImageUrl))
            return new MessageDto { Message = "Invalid Image URL" };

        // Validate the AudioUrl
        if (!string.IsNullOrEmpty(request.AudioUrl) && !Uri.TryCreate(request.AudioUrl, UriKind.Absolute, out var validatedAudioUrl))
            return new MessageDto { Message = "Invalid Audio URL" };

        //send messages
        var message = new MulticastMessage()
        {
            Tokens = tokens,
            Notification = new Notification
            {
                Title = request.Title,
                Body = request.Body,
                ImageUrl = request.ImageUrl
            },

            Android = new AndroidConfig
            {
                Notification = new AndroidNotification
                {
                    ImageUrl = request.ImageUrl,
                    Sound = request.AudioUrl ?? "default",
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

            return new MessageDto { Message = $"Error in sending Notification, There are often invalid tokens that are now being deleted! Success Count: {response.SuccessCount} | Failure Count: {response.FailureCount}" };
        }

        return new MessageDto();
    }

}
