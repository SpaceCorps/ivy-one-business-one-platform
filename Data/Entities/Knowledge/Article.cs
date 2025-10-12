using System.ComponentModel.DataAnnotations;

namespace IvyOneBusinessOnePlatform.Data;

public enum ArticleStatus
{
    Draft,
    Published,
    Archived
}

public class Article
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Summary { get; set; } = string.Empty;
    
    [Required]
    public int CategoryId { get; set; }
    
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
    
    [Range(0, int.MaxValue, ErrorMessage = "View count cannot be negative")]
    public int ViewCount { get; set; }
    
    [MaxLength(200)]
    public string Author { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual Category Category { get; set; } = null!;
}
