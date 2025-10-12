using System.ComponentModel.DataAnnotations;

namespace IvyOneBusinessOnePlatform.Data;

public class Signature
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(300)]
    public string DocumentTitle { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string DocumentPath { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string SignerName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string SignerEmail { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Signed, Declined, Expired
    
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? SignedDate { get; set; }
    
    public string SignatureData { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string IpAddress { get; set; } = string.Empty;
    
    public DateTime? ExpiryDate { get; set; }
    
    [MaxLength(2000)]
    public string Notes { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

