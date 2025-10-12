using System.ComponentModel.DataAnnotations;

namespace IvyOneBusinessOnePlatform.Data;

public class Transaction
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string TransactionNumber { get; set; } = string.Empty;
    
    public DateTime TransactionDate { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue, ErrorMessage = "Debit amount cannot be negative")]
    public decimal DebitAmount { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Credit amount cannot be negative")]
    public decimal CreditAmount { get; set; }
    
    [MaxLength(100)]
    public string Reference { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
