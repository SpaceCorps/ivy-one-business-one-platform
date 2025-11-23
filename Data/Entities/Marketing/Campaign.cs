using System.ComponentModel.DataAnnotations;

namespace IvyOneBusinessOnePlatform.Data;

public class Campaign
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty; // Email, Social Media, Print, etc.
    
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Draft"; // Draft, Active, Paused, Completed
    
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Budget cannot be negative")]
    public decimal Budget { get; set; }
    
    [MaxLength(500)]
    public string TargetAudience { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
