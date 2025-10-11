namespace IvyOneBusinessOnePlatform.Data;

public class Payment
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // Cash, Credit Card, Bank Transfer
    public string Reference { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Invoice Invoice { get; set; } = null!;
}
