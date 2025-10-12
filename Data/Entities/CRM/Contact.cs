using System.ComponentModel.DataAnnotations;

namespace IvyOneBusinessOnePlatform.Data;

public class Contact
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
    
    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string State { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string ZipCode { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string Notes { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
}
