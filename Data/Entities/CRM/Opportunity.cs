using System.ComponentModel.DataAnnotations;

namespace IvyOneBusinessOnePlatform.Data;

public class Opportunity
{
    public int Id { get; set; }
    
    [Required]
    public int ContactId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue, ErrorMessage = "Amount cannot be negative")]
    public decimal Amount { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Stage { get; set; } = "Prospecting"; // Prospecting, Qualification, Proposal, Negotiation, Closed Won, Closed Lost
    
    [Range(0, 100, ErrorMessage = "Probability must be between 0 and 100")]
    public int Probability { get; set; } // Percentage
    
    public DateTime ExpectedCloseDate { get; set; }
    
    [MaxLength(2000)]
    public string Notes { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual Contact Contact { get; set; } = null!;
}
