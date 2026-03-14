namespace signalR_demo.hub;

public interface IClientInterface
{
    Task ReceiveNotification(Notification notification);
    Task GetNotifications(List<Notification> notifications);
}