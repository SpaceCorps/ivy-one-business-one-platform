using System.ComponentModel.DataAnnotations;

namespace IvyOneBusinessOnePlatform.Data;

public class Subscription
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string CustomerEmail { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string PlanName { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue, ErrorMessage = "Monthly amount cannot be negative")]
    public decimal MonthlyAmount { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string BillingCycle { get; set; } = "Monthly"; // Monthly, Quarterly, Yearly
    
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Active"; // Active, Cancelled, Paused, Expired
    
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? EndDate { get; set; }
    
    public DateTime? NextBillingDate { get; set; }
    
    [MaxLength(100)]
    public string PaymentMethod { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

