namespace IvyOneBusinessOnePlatform.Data;

public class Subscriber
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Status { get; set; } = "Active"; // Active, Unsubscribed, Bounced
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UnsubscribedAt { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
