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

    // send gps data to users with the same glasses id
    public async Task SendGPSData(GpsDto gpsData)
    {
        await Clients.Group(gpsData.GlassesId).SendAsync("ReceiveGPSData", gpsData.Latitude, gpsData.Longitude);
    }
}