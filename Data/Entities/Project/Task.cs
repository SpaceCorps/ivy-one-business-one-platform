namespace IvyOneBusinessOnePlatform.Data;

public class Task
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "To Do"; // To Do, In Progress, Review, Done
    public int Priority { get; set; } = 1; // 1 = Low, 2 = Medium, 3 = High, 4 = Critical
    public DateTime? DueDate { get; set; }
    public string Assignee { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual Project Project { get; set; } = null!;
}
