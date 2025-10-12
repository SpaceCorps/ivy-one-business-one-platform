
using System.ComponentModel.DataAnnotations;

namespace IvyOneBusinessOnePlatform.Data;

public enum TicketStatus
{
    Open,
    InProgress,
    Resolved,
    Closed
}

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Critical
}

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
    
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    
    [MaxLength(200)]
    public string AssignedTo { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ResolvedAt { get; set; }
}

