namespace IvyOneBusinessOnePlatform.Data;

public class Timesheet
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal HoursWorked { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved, Rejected
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual Project Project { get; set; } = null!;
}
