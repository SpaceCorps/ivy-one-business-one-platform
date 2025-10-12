using System.ComponentModel.DataAnnotations;

namespace IvyOneBusinessOnePlatform.Data;

public class StockMovement
{
    public int Id { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string MovementType { get; set; } = string.Empty; // In, Out, Transfer, Adjustment
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
    
    [MaxLength(200)]
    public string? Reference { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Notes { get; set; } = string.Empty;
    
    public DateTime MovementDate { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
