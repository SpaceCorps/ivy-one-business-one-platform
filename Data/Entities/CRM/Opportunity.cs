namespace IvyOneBusinessOnePlatform.Data;

public class Opportunity
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Stage { get; set; } = "Prospecting"; // Prospecting, Qualification, Proposal, Negotiation, Closed Won, Closed Lost
    public int Probability { get; set; } // Percentage
    public DateTime ExpectedCloseDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual Contact Contact { get; set; } = null!;
}
