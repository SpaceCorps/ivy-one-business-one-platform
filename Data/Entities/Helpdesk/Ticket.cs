using System.ComponentModel.DataAnnotations;

namespace IvyOneBusinessOnePlatform.Data;

public class Ticket
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string TicketNumber { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string CustomerEmail { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(300)]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Open"; // Open, In Progress, Resolved, Closed
    
    [Required]
    [MaxLength(50)]
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
    
    [MaxLength(200)]
    public string AssignedTo { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ResolvedAt { get; set; }
}

