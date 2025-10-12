using System.ComponentModel.DataAnnotations;

namespace IvyOneBusinessOnePlatform.Data;

public class Invoice
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;
    
    public DateTime IssueDate { get; set; }
    
    public DateTime DueDate { get; set; }
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Tax amount cannot be negative")]
    public decimal TaxAmount { get; set; }
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than zero")]
    public decimal TotalAmount { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Draft"; // Draft, Sent, Paid, Overdue
    
    [Required]
    [MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string CustomerEmail { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
