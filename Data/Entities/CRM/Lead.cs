using System.ComponentModel.DataAnnotations;

namespace IvyOneBusinessOnePlatform.Data;

public class Lead
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string Company { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string JobTitle { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Source { get; set; } = string.Empty; // Website, Referral, Cold Call, etc.
    
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "New"; // New, Qualified, Contacted, Converted, Lost
    
    [Range(0, double.MaxValue, ErrorMessage = "Estimated value cannot be negative")]
    public decimal EstimatedValue { get; set; }
    
    [MaxLength(2000)]
    public string Notes { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
