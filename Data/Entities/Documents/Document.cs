using System.ComponentModel.DataAnnotations;

namespace IvyOneBusinessOnePlatform.Data;

public class Document
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string FilePath { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string FileType { get; set; } = string.Empty;
    
    [Range(0, long.MaxValue, ErrorMessage = "File size cannot be negative")]
    public long FileSize { get; set; }
    
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Tags { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string Author { get; set; } = string.Empty;
    
    public bool IsPublic { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
