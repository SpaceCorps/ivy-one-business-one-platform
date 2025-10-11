using System.ComponentModel.DataAnnotations;

namespace IvyOneBusinessOnePlatform.Data;

public class Department
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string ManagerName { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue, ErrorMessage = "Budget must be a positive value")]
    public decimal Budget { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
