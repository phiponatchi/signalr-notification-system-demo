namespace signalR_demo.hub;

public class Notification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string CreatedBy { get; set; } = null!;
    public object? Payload { get; set; }
}