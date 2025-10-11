namespace IvyOneBusinessOnePlatform.Data;

public class Subscription
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public decimal MonthlyAmount { get; set; }
    public string BillingCycle { get; set; } = "Monthly"; // Monthly, Quarterly, Yearly
    public string Status { get; set; } = "Active"; // Active, Cancelled, Paused, Expired
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

