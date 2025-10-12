using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Knowledge;

[App(icon: Icons.Bookmark, title: "Knowledge", path: new[] { "Customer Service" })]
public class KnowledgeApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new KnowledgeRootBlade(), "Knowledge Base", Size.Units(110));
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
        var isNewArticleOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        var query = context.Articles.Include(a => a.Category).Where(a => a.Status == ArticleStatus.Published).AsQueryable();

        if (!string.IsNullOrEmpty(searchQuery.Value))
        {
            var searchPattern = $"%{searchQuery.Value}%";
            query = query.Where(a => 
                EF.Functions.Like(a.Title, searchPattern) ||
                EF.Functions.Like(a.Content, searchPattern) ||
                EF.Functions.Like(a.Summary, searchPattern) ||
                EF.Functions.Like(a.Author, searchPattern));
        }

        var articles = query.OrderByDescending(a => a.CreatedAt).ToList();
        
        var listItems = articles.Select(article => new ListItem(
            title: article.Title,
            subtitle: $"{article.Category?.Name ?? "Uncategorized"} - {article.ViewCount} views - By {article.Author}",
            icon: Icons.FileText,
            badge: article.Status.ToString(),
            onClick: _ => blades.Push(this, new ArticleDetailBlade(article.Id, () => refreshToken.Refresh()), article.Title)
        ));
        
        var mainContent = BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(4)
                .Add(searchQuery.ToSearchInput().Placeholder("Search articles by title, content, summary, or author..."))
                .Add(new Button("New Article")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => isNewArticleOpen.Set(true)))
                .Add(new Button("Categories")
                    .Variant(ButtonVariant.Secondary)
                    .HandleClick(_ => blades.Push(this, new CategoriesBlade(), "Categories"))),
            articles.Count == 0 
                ? Text.Block("No articles found. Try a different search or create your first article!")
                : new List(listItems)
        );

        return isNewArticleOpen.Value ? new Sheet(
            (Event<Sheet> _) => isNewArticleOpen.Set(false),
            new ArticleFormSheet(null, () => {
                isNewArticleOpen.Set(false);
                refreshToken.Refresh();
            }),
            title: "New Article",
            description: "Create a new knowledge base article"
        ).Width(Size.Fraction(1/3f)) : mainContent;
    }
}

public class ArticleDetailBlade(int articleId, Action? onRefresh = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var initialArticle = context.Articles.Include(a => a.Category).FirstOrDefault(a => a.Id == articleId);
        
        if (initialArticle == null)
        {
            return Layout.Vertical()
                .Gap(4)
                .Add(Text.H3("Article Not Found"))
                .Add(new Button("Go Back", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary));
        }
        
        var articleData = this.UseState(initialArticle);
        var isEditOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        this.UseEffect(() =>
        {
            // Atomically increment view count
            context.Database.ExecuteSqlRaw("UPDATE Articles SET ViewCount = ViewCount + 1 WHERE Id = {0}", articleId);
            
            // Fetch updated article with category
            var updatedArticle = context.Articles.Include(a => a.Category).FirstOrDefault(a => a.Id == articleId);
            if (updatedArticle != null)
            {
                articleData.Set(updatedArticle);
            }
        }, [refreshToken.ToTrigger()]);
        
        var statusBadge = new Badge(articleData.Value.Status.ToString())
            .Variant(articleData.Value.Status == ArticleStatus.Published ? BadgeVariant.Success :
                   articleData.Value.Status == ArticleStatus.Archived ? BadgeVariant.Secondary :
                   BadgeVariant.Warning);

        var articleDetails = new
        {
            Title = articleData.Value.Title,
            Category = articleData.Value.Category?.Name ?? "Uncategorized",
            Author = articleData.Value.Author,
            ViewCount = $"{articleData.Value.ViewCount} views",
            Status = statusBadge,
            Summary = articleData.Value.Summary,
            Content = articleData.Value.Content,
            CreatedAt = articleData.Value.CreatedAt.ToString("MMM dd, yyyy")
        };
        
        return Layout.Vertical()
            .Gap(4)
            .Add(Text.H3(articleData.Value.Title))
            .Add(articleDetails.ToDetails().RemoveEmpty().MultiLine(x => x.Content).MultiLine(x => x.Summary))
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(articleData.Value.Status == ArticleStatus.Draft ? new Button("Publish Article", _ => {
                    var dbArticle = context.Articles.FirstOrDefault(a => a.Id == articleId);
                    if (dbArticle != null)
                    {
                        dbArticle.Status = ArticleStatus.Published;
                        dbArticle.UpdatedAt = DateTime.UtcNow;
                        context.SaveChanges();
                        client.Toast($"Article {articleData.Value.Title} published!");
                        refreshToken.Refresh();
                        onRefresh?.Invoke();
                    }
                })
                    .Variant(ButtonVariant.Success)
                    .Icon(Icons.Check)
                    : null)
                .Add(articleData.Value.Status == ArticleStatus.Published ? new Button("Archive Article", _ => {
                    var dbArticle = context.Articles.FirstOrDefault(a => a.Id == articleId);
                    if (dbArticle != null)
                    {
                        dbArticle.Status = ArticleStatus.Archived;
                        dbArticle.UpdatedAt = DateTime.UtcNow;
                        context.SaveChanges();
                        client.Toast($"Article {articleData.Value.Title} archived!");
                        refreshToken.Refresh();
                        onRefresh?.Invoke();
                    }
                })
                    .Variant(ButtonVariant.Secondary)
                    .Icon(Icons.Archive)
                    : null)
                .Add(new Button("Edit Article")
                    .Variant(ButtonVariant.Outline)
                    .Icon(Icons.Pencil)
                    .HandleClick(_ => isEditOpen.Set(true)))
                .Add(new Button("Delete Article")
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.Trash)
                    .HandleClick(_ => {
                        try
                        {
                            var articleToDelete = context.Articles.FirstOrDefault(a => a.Id == articleId);
                            if (articleToDelete != null)
                            {
                                context.Articles.Remove(articleToDelete);
                                context.SaveChanges();
                                client.Toast($"Article {articleData.Value.Title} deleted successfully!");
                                refreshToken.Refresh();
                                onRefresh?.Invoke();
                                blades.Pop();
                            }
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error deleting article: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary)))
            .Add(isEditOpen.Value ? new Sheet(
                (Event<Sheet> _) => isEditOpen.Set(false),
                new ArticleFormSheet(articleId, () => {
                    isEditOpen.Set(false);
                    refreshToken.Refresh();
                    onRefresh?.Invoke();
                }),
                title: "Edit Article",
                description: $"Edit article {articleData.Value.Title}"
            ).Width(Size.Fraction(1/3f)) : null);
    }
}

public class CategoriesBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var isNewCategoryOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
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
            onClick: _ => blades.Push(this, new CategoryDetailBlade(category.Id, () => refreshToken.Refresh()), category.Name)
        ));
        
        var mainContent = BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(4)
                .Add(new Button("New Category")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => isNewCategoryOpen.Set(true))),
            categories.Count == 0 
                ? Text.Block("No categories found. Create your first category!")
                : new List(listItems)
        );

        return isNewCategoryOpen.Value ? new Sheet(
            (Event<Sheet> _) => isNewCategoryOpen.Set(false),
            new CategoryFormSheet(null, () => {
                isNewCategoryOpen.Set(false);
                refreshToken.Refresh();
            }),
            title: "New Category",
            description: "Create a new category"
        ).Width(Size.Fraction(1/3f)) : mainContent;
    }
}

public class CategoryDetailBlade(int categoryId, Action? onRefresh = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var initialCategory = context.Categories.Include(c => c.Articles).FirstOrDefault(c => c.Id == categoryId);
        
        if (initialCategory == null)
        {
            return Layout.Vertical()
                .Gap(4)
                .Add(Text.H3("Category Not Found"))
                .Add(new Button("Go Back", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary));
        }
        
        var categoryData = this.UseState(initialCategory);
        var isEditOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        this.UseEffect(() =>
        {
            var updatedCategory = context.Categories.Include(c => c.Articles).FirstOrDefault(c => c.Id == categoryId);
            if (updatedCategory != null)
            {
                categoryData.Set(updatedCategory);
            }
        }, [refreshToken.ToTrigger()]);
        
        var categoryDetails = new
        {
            Name = categoryData.Value.Name,
            Description = categoryData.Value.Description,
            ArticlesCount = $"{categoryData.Value.Articles.Count} articles",
            Status = categoryData.Value.IsActive ? "Active" : "Inactive"
        };
        
        var articleItems = categoryData.Value.Articles.Select(article => new ListItem(
            title: article.Title,
            subtitle: $"{article.ViewCount} views - By {article.Author}",
            icon: Icons.FileText,
            badge: article.Status.ToString(),
            onClick: _ => blades.Push(this, new ArticleDetailBlade(article.Id, () => refreshToken.Refresh()), article.Title)
        ));
        
        return Layout.Vertical()
            .Gap(4)
            .Add(Text.H3(categoryData.Value.Name))
            .Add(categoryDetails.ToDetails().RemoveEmpty().MultiLine(x => x.Description))
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(new Button("Edit Category")
                    .Variant(ButtonVariant.Outline)
                    .Icon(Icons.Pencil)
                    .HandleClick(_ => isEditOpen.Set(true)))
                .Add(new Button("Delete Category")
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.Trash)
                    .HandleClick(_ => {
                        try
                        {
                            var categoryToDelete = context.Categories.FirstOrDefault(c => c.Id == categoryId);
                            if (categoryToDelete != null)
                            {
                                context.Categories.Remove(categoryToDelete);
                                context.SaveChanges();
                                client.Toast($"Category {categoryData.Value.Name} deleted successfully!");
                                refreshToken.Refresh();
                                onRefresh?.Invoke();
                                blades.Pop();
                            }
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error deleting category: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary)))
            .Add(Text.H3("Articles"))
            .Add(categoryData.Value.Articles.Any() 
                ? new List(articleItems)
                : Text.Block("No articles in this category yet."))
            .Add(isEditOpen.Value ? new Sheet(
                (Event<Sheet> _) => isEditOpen.Set(false),
                new CategoryFormSheet(categoryId, () => {
                    isEditOpen.Set(false);
                    refreshToken.Refresh();
                    onRefresh?.Invoke();
                }),
                title: "Edit Category",
                description: $"Edit category {categoryData.Value.Name}"
            ).Width(Size.Fraction(1/3f)) : null);
    }
}

public record ArticleFormModel(
    [Required] string Title,
    [Required] string Content,
    string Summary,
    [Required] string CategoryId, // Changed to string to display names
    ArticleStatus Status,
    string Author
);

public class ArticleFormSheet(int? articleId = null, Action? onClose = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var isEdit = articleId.HasValue;
        var existingArticle = isEdit ? context.Articles.FirstOrDefault(a => a.Id == articleId!.Value) : null;
        var categories = context.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();
        
        var articleForm = this.UseState(() => existingArticle != null 
            ? new ArticleFormModel(
                existingArticle.Title,
                existingArticle.Content,
                existingArticle.Summary,
                existingArticle.Category?.Name ?? "", // Convert int ID to string Name
                existingArticle.Status,
                existingArticle.Author
            )
            : new ArticleFormModel(
                "",
                "",
                "",
                categories.FirstOrDefault()?.Name ?? "", // Convert int ID to string Name
                ArticleStatus.Draft,
                ""
            ));
        
        var formBuilder = articleForm.ToForm()
            .Group("Article Information", m => m.Title, m => m.Summary, m => m.Content)
            .Group("Article Settings", m => m.CategoryId, m => m.Author, m => m.Status)
            .Label(m => m.Title, "Title")
            .Label(m => m.Summary, "Summary")
            .Label(m => m.Content, "Content")
            .Label(m => m.CategoryId, "Category")
            .Label(m => m.Author, "Author")
            .Label(m => m.Status, "Status")
            .Builder(m => m.Summary, s => s.ToTextInput().Variant(TextInputs.Textarea).Placeholder("Brief summary..."))
            .Builder(m => m.Content, s => s.ToTextInput().Variant(TextInputs.Textarea).Placeholder("Article content..."))
            .Builder(m => m.CategoryId, s => s.ToSelectInput(categories.Select(c => c.Name).ToOptions()))
            .Builder(m => m.Author, s => s.ToTextInput().Placeholder("Author Name"))
            .Builder(m => m.Status, s => s.ToSelectInput(typeof(ArticleStatus).ToOptions()));
        
        var (onSubmit, formView, validationView, loading) = formBuilder.UseForm(this.Context);
        
        async ValueTask HandleSubmit()
        {
            if (await onSubmit())
            {
                try
                {
                    var formData = articleForm.Value;
                    
                    // Convert string CategoryId back to int
                    var categoryId = categories.FirstOrDefault(c => c.Name == formData.CategoryId)?.Id ?? 1;
                    
                    if (isEdit && existingArticle != null)
                    {
                        existingArticle.Title = formData.Title;
                        existingArticle.Content = formData.Content;
                        existingArticle.Summary = formData.Summary;
                        existingArticle.CategoryId = categoryId; // Convert string Name back to int ID
                        existingArticle.Status = formData.Status;
                        existingArticle.Author = formData.Author;
                        existingArticle.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        var newArticle = new Data.Article
                        {
                            Title = formData.Title,
                            Content = formData.Content,
                            Summary = formData.Summary,
                            CategoryId = categoryId, // Convert string Name back to int ID
                            Status = formData.Status,
                            Author = formData.Author,
                            ViewCount = 0,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        context.Articles.Add(newArticle);
                    }
                    
                    await context.SaveChangesAsync();
                    client.Toast(isEdit ? "Article updated successfully!" : "Article created successfully!");
                    onClose?.Invoke();
                }
                catch (Exception ex)
                {
                    client.Toast($"Error: {ex.Message}", "Error");
                }
            }
        }
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => HandleSubmit()))
                .Add(new Button("Cancel")
                    .Variant(ButtonVariant.Outline)
                    .HandleClick(_ => onClose?.Invoke())),
            formView
        );
    }
}

public class CategoryFormSheet(int? categoryId = null, Action? onClose = null) : ViewBase
{
    private Category CloneCategory(Category source) => new Category
    {
        Id = source.Id,
        Name = source.Name,
        Description = source.Description,
        ParentCategoryId = source.ParentCategoryId,
        IsActive = source.IsActive,
        CreatedAt = source.CreatedAt,
        UpdatedAt = source.UpdatedAt
    };
    
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var isEdit = categoryId.HasValue;
        var existingCategory = isEdit ? context.Categories.FirstOrDefault(c => c.Id == categoryId!.Value) : null;
        
        var categoryForm = this.UseState(existingCategory ?? new Category
        {
            Name = "",
            Description = "",
            IsActive = true
        });
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(categoryForm.Value.Name))
                            {
                                client.Toast("Category Name is required", "Validation Error");
                                return;
                            }
                            
                            if (isEdit && existingCategory != null)
                            {
                                existingCategory.Name = categoryForm.Value.Name;
                                existingCategory.Description = categoryForm.Value.Description;
                                existingCategory.IsActive = categoryForm.Value.IsActive;
                                existingCategory.UpdatedAt = DateTime.UtcNow;
                            }
                            else
                            {
                                var newCategory = new Category
                                {
                                    Name = categoryForm.Value.Name,
                                    Description = categoryForm.Value.Description,
                                    IsActive = categoryForm.Value.IsActive,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                context.Categories.Add(newCategory);
                            }
                            
                            context.SaveChanges();
                            client.Toast(isEdit ? "Category updated successfully!" : "Category created successfully!");
                            onClose?.Invoke();
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel")
                    .Variant(ButtonVariant.Outline)
                    .HandleClick(_ => onClose?.Invoke())),
            
            Layout.Vertical().Gap(4)
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Category Information"))
                        .Add(Text.Small("Category Name"))
                        .Add(new TextInput(categoryForm.Value.Name, e => {
                            var cloned = CloneCategory(categoryForm.Value);
                            cloned.Name = e.Value;
                            categoryForm.Set(cloned);
                        }).Placeholder("Category Name"))
                        .Add(Text.Small("Description"))
                        .Add(new TextInput(categoryForm.Value.Description, e => {
                            var cloned = CloneCategory(categoryForm.Value);
                            cloned.Description = e.Value;
                            categoryForm.Set(cloned);
                        }).Placeholder("Category description...").Variant(TextInputs.Textarea))
                        .Add(Text.Small("Status"))
                        .Add(new SelectInput<bool>(categoryForm.Value.IsActive, e => {
                            var cloned = CloneCategory(categoryForm.Value);
                            cloned.IsActive = e.Value;
                            categoryForm.Set(cloned);
                        }, new[] { (true, "Active"), (false, "Inactive") }.ToOptions()))
                ).Title("Category Details"))
        );
    }
}
