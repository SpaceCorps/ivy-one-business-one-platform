namespace IvyOneBusinessOnePlatform.Data;

public class StockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string MovementType { get; set; } = string.Empty; // In, Out, Transfer, Adjustment
    public int Quantity { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime MovementDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
