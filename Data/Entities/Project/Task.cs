using System.ComponentModel.DataAnnotations;

namespace IvyOneBusinessOnePlatform.Data;

public class Task
{
    public int Id { get; set; }
    
    [Required]
    public int ProjectId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "To Do"; // To Do, In Progress, Review, Done
    
    [Range(1, 4, ErrorMessage = "Priority must be between 1 and 4")]
    public int Priority { get; set; } = 1; // 1 = Low, 2 = Medium, 3 = High, 4 = Critical
    
    public DateTime? DueDate { get; set; }
    
    [MaxLength(200)]
    public string Assignee { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual Project Project { get; set; } = null!;
}
