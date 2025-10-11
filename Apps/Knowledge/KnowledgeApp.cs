using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Knowledge;

[App(icon: Icons.Bookmark, title: "Knowledge", path: new[] { "Customer Service" })]
public class KnowledgeApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new KnowledgeRootBlade(), "Knowledge Base", Size.Units(80));
    }
}

public class KnowledgeRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var searchQuery = this.UseState("");
        
        var query = context.Articles
            .Include(a => a.Category)
            .Where(a => a.Status == "Published");

        if (!string.IsNullOrEmpty(searchQuery.Value))
        {
            query = query.Where(a => a.Title.Contains(searchQuery.Value) || 
                                   a.Content.Contains(searchQuery.Value) || 
                                   a.Summary.Contains(searchQuery.Value));
        }

        var articles = query.OrderByDescending(a => a.CreatedAt).ToList();
        var categories = context.Categories.Include(c => c.Articles).Where(c => c.IsActive).ToList();
        
        var listItems = articles.Select(article => new ListItem(
            title: article.Title,
            subtitle: $"{article.Category?.Name ?? "Uncategorized"} - {article.ViewCount} views - By {article.Author}",
            icon: Icons.FileText,
            badge: article.Category?.Name ?? "Uncategorized",
            onClick: _ => blades.Push(this, new ArticleDetailBlade(article.Id), article.Title)
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(Text.H3("Knowledge Base"))
                .Add(new Button("New Article", _ => client.Toast("Create article"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary))
                .Add(new Button("Categories", _ => blades.Push(this, new CategoriesBlade(), "Categories"))
                    .Variant(ButtonVariant.Secondary)))
            .Add(searchQuery.ToSearchInput().Placeholder("Search articles..."))
            .Add(articles.Count == 0 
                ? Text.Block("No articles found. Create your first article!")
                : new List(listItems));
    }
}

public class ArticleDetailBlade(int articleId) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var article = context.Articles.Include(a => a.Category).FirstOrDefault(a => a.Id == articleId);
        
        if (article == null)
            return "Article not found";
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(article.Title)
                .Add(Layout.Horizontal()
                    .Gap(8)
                    .Add(new Badge(article.Category?.Name ?? "Uncategorized").Variant(BadgeVariant.Secondary))
                    .Add(new Badge($"{article.ViewCount} views").Variant(BadgeVariant.Outline))
                    .Add(new Badge(article.Author).Variant(BadgeVariant.Info))
                    .Add(new Badge(article.Status).Variant(BadgeVariant.Success)))
                .Add("Summary")
                .Add(article.Summary)
                .Add("Content")
                .Add(article.Content)
                .Add($"Created: {article.CreatedAt:MMM dd, yyyy}")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Edit", _ => client.Toast("Edit article"))
                        .Variant(ButtonVariant.Primary))
                    .Add(new Button("Share", _ => client.Toast("Share article"))
                        .Variant(ButtonVariant.Secondary)))
        );
    }
}

public class CategoriesBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var categories = context.Categories
            .Include(c => c.Articles)
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToList();
        
        var listItems = categories.Select(category => new ListItem(
            title: category.Name,
            subtitle: $"{category.Description} - {category.Articles.Count} articles",
            icon: Icons.Folder,
            badge: $"{category.Articles.Count}",
            onClick: _ => blades.Push(this, new CategoryDetailBlade(category.Id), category.Name)
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(Text.H3("Categories"))
                .Add(new Button("New Category", _ => client.Toast("Create category"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)))
            .Add(categories.Count == 0 
                ? Text.Block("No categories found. Create your first category!")
                : new List(listItems));
    }
}

public class CategoryDetailBlade(int categoryId) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var category = context.Categories.Include(c => c.Articles).FirstOrDefault(c => c.Id == categoryId);
        
        if (category == null)
            return "Category not found";
        
        var articleItems = category.Articles.Select(article => new ListItem(
            title: article.Title,
            subtitle: $"{article.ViewCount} views",
            icon: Icons.FileText
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Add(new Card(
                Layout.Vertical()
                    .Gap(12)
                    .Padding(16)
                    .Add(category.Name)
                    .Add(category.Description)
                    .Add(new Badge($"{category.Articles.Count} articles").Variant(BadgeVariant.Info))))
            .Add(category.Articles.Any() 
                ? new List(articleItems)
                : "No articles in this category yet.");
    }
}
