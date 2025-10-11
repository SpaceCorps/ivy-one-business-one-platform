using System.ComponentModel.DataAnnotations;

namespace IvyOneBusinessOnePlatform.Data;

public class PurchaseOrder
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Supplier { get; set; } = string.Empty;
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }
    
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Pending;
    
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? ExpectedDeliveryDate { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string PaymentTerms { get; set; } = "Net 30";
    
    [Required]
    [MaxLength(100)]
    public string Department { get; set; } = "Operations";
    
    [MaxLength(2000)]
    public string Notes { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum PurchaseOrderStatus
{
    Pending,
    Approved,
    Received,
    Cancelled
}

