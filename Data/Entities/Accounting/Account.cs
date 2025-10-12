using System.ComponentModel.DataAnnotations;

namespace IvyOneBusinessOnePlatform.Data;

public class Account
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string AccountNumber { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string AccountName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string AccountType { get; set; } = string.Empty; // Asset, Liability, Equity, Revenue, Expense
    
    [Range(0, double.MaxValue, ErrorMessage = "Balance cannot be negative")]
    public decimal Balance { get; set; }
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
