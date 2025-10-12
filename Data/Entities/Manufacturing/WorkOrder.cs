using System.ComponentModel.DataAnnotations;

namespace IvyOneBusinessOnePlatform.Data;

public enum WorkOrderStatus
{
    Scheduled,
    InProduction,
    Completed,
    Cancelled
}

public enum WorkOrderPriority
{
    Low,
    Normal,
    High,
    Urgent
}

public class WorkOrder
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string WorkOrderNumber { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
    
    [Required]
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Scheduled;
    
    [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100")]
    public int Progress { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? CompletionDate { get; set; }
    
    public DateTime PlannedStartDate { get; set; }
    
    public DateTime PlannedEndDate { get; set; }
    
    [Required]
    public WorkOrderPriority Priority { get; set; } = WorkOrderPriority.Normal;
    
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    public int? ProductId { get; set; }
    
    public Product? Product { get; set; }
}

