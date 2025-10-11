namespace IvyOneBusinessOnePlatform.Data;

public class Signature
{
    public int Id { get; set; }
    public string DocumentTitle { get; set; } = string.Empty;
    public string DocumentPath { get; set; } = string.Empty;
    public string SignerName { get; set; } = string.Empty;
    public string SignerEmail { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Signed, Declined, Expired
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public DateTime? SignedDate { get; set; }
    public string SignatureData { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

