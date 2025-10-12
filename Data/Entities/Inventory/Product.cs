using System.ComponentModel.DataAnnotations;

namespace IvyOneBusinessOnePlatform.Data;

public class Product
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string ProductCode { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative")]
    public decimal Price { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Cost cannot be negative")]
    public decimal Cost { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
    public int QuantityInStock { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Minimum stock level cannot be negative")]
    public int MinimumStockLevel { get; set; }
    
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string Supplier { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
