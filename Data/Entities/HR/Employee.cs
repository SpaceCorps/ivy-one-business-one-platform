using System.ComponentModel.DataAnnotations;

namespace IvyOneBusinessOnePlatform.Data;

public class Employee
{
    public int Id { get; set; }
    
    public int DepartmentId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string EmployeeId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(150)]
    public string JobTitle { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive value")]
    public decimal Salary { get; set; }
    
    public DateTime HireDate { get; set; }
    
    public DateTime? TerminationDate { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Active"; // Active, Inactive, Terminated
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual Department Department { get; set; } = null!;
}
