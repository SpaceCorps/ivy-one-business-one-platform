namespace IvyOneBusinessOnePlatform.Data;

public class Article
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Published, Archived
    public int ViewCount { get; set; }
    public string Author { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual Category Category { get; set; } = null!;
}
