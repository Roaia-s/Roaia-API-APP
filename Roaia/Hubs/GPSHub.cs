namespace Roaia.Hubs;

public sealed class GPSHub : Hub
{
    // join group with glasses id
    public async Task JoinGroup(string glassesId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, glassesId);
    }

    // leave group with glasses id
    public async Task LeaveGroup(string glassesId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, glassesId);
    }
}