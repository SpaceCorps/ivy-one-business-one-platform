using System.ComponentModel.DataAnnotations;

namespace IvyOneBusinessOnePlatform.Data;

public class Timesheet
{
    public int Id { get; set; }
    
    [Required]
    public int ProjectId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string EmployeeName { get; set; } = string.Empty;
    
    public DateTime Date { get; set; }
    
    [Required]
    [Range(0.1, 24, ErrorMessage = "Hours worked must be between 0.1 and 24")]
    public decimal HoursWorked { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved, Rejected
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual Project Project { get; set; } = null!;
}
