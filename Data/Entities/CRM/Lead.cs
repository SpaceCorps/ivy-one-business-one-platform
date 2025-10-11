namespace IvyOneBusinessOnePlatform.Data;

public class Lead
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty; // Website, Referral, Cold Call, etc.
    public string Status { get; set; } = "New"; // New, Qualified, Contacted, Converted, Lost
    public decimal EstimatedValue { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
