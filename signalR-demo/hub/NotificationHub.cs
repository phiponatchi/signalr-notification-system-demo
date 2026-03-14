using Microsoft.AspNetCore.SignalR;

namespace signalR_demo.hub;

public class NotificationHub : Hub<IClientInterface>
{
    private static readonly ConnectionMapping<string> Connections = new();

    private static readonly List<Notification> Notifications = [];


    public override Task OnConnectedAsync()
    {
        Connections.Add(Context.ConnectionId, Context.ConnectionId);
        // Check if client has undelivered messages
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Connections.Remove(Context.ConnectionId, Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task<Notification> SendNotification()
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid().ToString(), CreatedAt = DateTime.Now, CreatedBy = Context.ConnectionId, IsRead = false,
            Payload = Connections.GetConnections()
        };
        Notifications.Add(notification);
        await Clients.Others.ReceiveNotification(notification);
        return notification;
    }

    public async Task<Notification> Broadcast(Notification notification)
    {
        Notifications.Add(notification);
        await Clients.Others.ReceiveNotification(notification);
        return notification;
    }

    public IEnumerable<Notification> GetNotifications()
    {
        return Notifications;
    }

    public bool Ping()
    {
        return true;
    }

    public IEnumerable<string> GetConnections()
    {
        return Connections.GetConnections();
    }
}