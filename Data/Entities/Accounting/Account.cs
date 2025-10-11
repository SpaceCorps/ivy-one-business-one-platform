namespace IvyOneBusinessOnePlatform.Data;

public class Account
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty; // Asset, Liability, Equity, Revenue, Expense
    public decimal Balance { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
