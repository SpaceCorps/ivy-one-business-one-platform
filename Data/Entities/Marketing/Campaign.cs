namespace IvyOneBusinessOnePlatform.Data;

public class Campaign
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Email, Social Media, Print, etc.
    public string Status { get; set; } = "Draft"; // Draft, Active, Paused, Completed
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal Budget { get; set; }
    public string TargetAudience { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
