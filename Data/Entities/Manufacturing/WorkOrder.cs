using System.ComponentModel.DataAnnotations;

namespace IvyOneBusinessOnePlatform.Data;

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
    [MaxLength(50)]
    public string Status { get; set; } = "Scheduled"; // Scheduled, In Production, Completed, Cancelled
    
    [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100")]
    public int Progress { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? CompletionDate { get; set; }
    
    public DateTime PlannedStartDate { get; set; }
    
    public DateTime PlannedEndDate { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent
    
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    public int? ProductId { get; set; }
    
    public Product? Product { get; set; }
}

