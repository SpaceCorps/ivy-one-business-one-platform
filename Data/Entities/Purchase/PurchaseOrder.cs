namespace IvyOneBusinessOnePlatform.Data;

public class PurchaseOrder
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Pending;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string PaymentTerms { get; set; } = "Net 30";
    public string Department { get; set; } = "Operations";
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

