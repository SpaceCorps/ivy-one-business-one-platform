namespace IvyOneBusinessOnePlatform.Data;

public class WorkOrder
{
    public int Id { get; set; }
    public string WorkOrderNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Status { get; set; } = "Scheduled"; // Scheduled, In Production, Completed, Cancelled
    public int Progress { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent
    public string? Notes { get; set; }
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
}

