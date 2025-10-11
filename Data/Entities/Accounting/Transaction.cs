namespace IvyOneBusinessOnePlatform.Data;

public class Transaction
{
    public int Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string Reference { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
