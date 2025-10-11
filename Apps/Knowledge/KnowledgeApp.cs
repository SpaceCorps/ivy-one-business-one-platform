using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Knowledge;

[App(icon: Icons.Bookmark, title: "Knowledge", path: new[] { "Customer Service" })]
public class KnowledgeApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var selectedView = this.UseState("articles");
        var searchQuery = this.UseState("");
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Knowledge Base")
                .Add("Access and manage your knowledge articles and FAQs")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Articles", _ => selectedView.Set("articles"))
                        .Variant(selectedView.Value == "articles" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("Categories", _ => selectedView.Set("categories"))
                        .Variant(selectedView.Value == "categories" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("Create Article", _ => selectedView.Set("create"))
                        .Variant(selectedView.Value == "create" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                )
                .Add(selectedView.Value == "create" 
                    ? BuildCreateArticleView(context, client)
                    : Layout.Vertical()
                        .Gap(12)
                        .Add(BuildSearchBar(searchQuery, client))
                        .Add(BuildSelectedView(selectedView.Value, context, client, searchQuery.Value)))
        );
    }

    private object BuildSearchBar(IState<string> searchQuery, IClientProvider client)
    {
        return Layout.Horizontal()
            .Gap(12)
            .Add(new TextInput(searchQuery)
                .Placeholder("Search articles...")
                .Variant(TextInputs.Search))
            .Add(new Button("Clear", _ => searchQuery.Set(""))
                .Variant(ButtonVariant.Outline)
                .Small());
    }

    private object BuildSelectedView(string view, ApplicationDbContext context, IClientProvider client, string searchQuery)
    {
        return view switch
        {
            "articles" => BuildArticlesView(context, client, searchQuery),
            "categories" => BuildCategoriesView(context, client),
            _ => "Select a view"
        };
    }

    private object BuildArticlesView(ApplicationDbContext context, IClientProvider client, string searchQuery)
    {
        var query = context.Articles
            .Include(a => a.Category)
            .Where(a => a.Status == "Published");

        if (!string.IsNullOrEmpty(searchQuery))
        {
            query = query.Where(a => a.Title.Contains(searchQuery) || 
                                   a.Content.Contains(searchQuery) || 
                                   a.Summary.Contains(searchQuery));
        }

        var articles = query.OrderByDescending(a => a.CreatedAt).ToList();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add($"Articles ({articles.Count})")
                    .Add(new Button("New Article", _ => client.Toast("Create new article coming soon!"))
                        .Icon(Icons.Plus)
                        .Variant(ButtonVariant.Primary)))
                .Add(articles.Count == 0 
                    ? "No articles found. Create your first article!"
                    : BuildArticlesList(articles, client))
        );
    }

    private object BuildCategoriesView(ApplicationDbContext context, IClientProvider client)
    {
        var categories = context.Categories
            .Include(c => c.Articles)
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToList();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add("Categories")
                    .Add(new Button("New Category", _ => client.Toast("Create new category coming soon!"))
                        .Icon(Icons.Plus)
                        .Variant(ButtonVariant.Primary)))
                .Add(categories.Count == 0 
                    ? "No categories found. Create your first category!"
                    : BuildCategoriesList(categories, client))
        );
    }

    private object BuildCreateArticleView(ApplicationDbContext context, IClientProvider client)
    {
        var categories = context.Categories.Where(c => c.IsActive).ToList();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add("Create New Article")
                .Add(Layout.Vertical()
                    .Gap(12)
                    .Add(new TextInput(UseState(""))
                        .Placeholder("Enter article title..."))
                    .Add(new SelectInput<string>(UseState(""), categories.Select(c => c.Name).ToOptions())
                        .Placeholder("Select a category"))
                    .Add(new TextInput(UseState(""))
                        .Placeholder("Enter article summary...")
                        .Variant(TextInputs.Textarea))
                    .Add(new TextInput(UseState(""))
                        .Placeholder("Enter article content...")
                        .Variant(TextInputs.Textarea))
                    .Add(Layout.Horizontal()
                        .Gap(12)
                        .Add(new Button("Save Draft", _ => client.Toast("Article saved as draft!"))
                            .Variant(ButtonVariant.Secondary))
                        .Add(new Button("Publish", _ => client.Toast("Article published!"))
                            .Variant(ButtonVariant.Primary))
                        .Add(new Button("Cancel", _ => client.Toast("Creation cancelled!"))
                            .Variant(ButtonVariant.Outline))))
        );
    }

    private object BuildArticlesList(List<Data.Article> articles, IClientProvider client)
    {
        var articleCards = articles.Select(article => new Card(
            Layout.Vertical()
                .Gap(8)
                .Padding(16)
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(Layout.Vertical()
                        .Gap(4)
                        .Add(article.Title)
                        .Add(article.Summary)
                        .Add(Layout.Horizontal()
                            .Gap(8)
                            .Add(new Badge(article.Category?.Name ?? "Uncategorized")
                                .Variant(BadgeVariant.Secondary))
                            .Add(new Badge($"Views: {article.ViewCount}")
                                .Variant(BadgeVariant.Outline))
                            .Add(new Badge(article.Author)
                                .Variant(BadgeVariant.Info))))
                    .Add(Layout.Vertical()
                        .Gap(4)
                        .Add(article.CreatedAt.ToString("MMM dd, yyyy"))
                        .Add(Layout.Horizontal()
                            .Gap(4)
                            .Add(new Button("View", _ => client.Toast($"Viewing article: {article.Title}"))
                                .Small()
                                .Variant(ButtonVariant.Primary))
                            .Add(new Button("Edit", _ => client.Toast($"Editing article: {article.Title}"))
                                .Small()
                                .Variant(ButtonVariant.Outline)))))
        ));

        return Layout.Vertical().Gap(8).Add(articleCards);
    }

    private object BuildCategoriesList(List<Data.Category> categories, IClientProvider client)
    {
        var categoryCards = categories.Select(category => new Card(
            Layout.Horizontal()
                .Gap(12)
                .Padding(16)
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add(category.Name)
                    .Add(category.Description)
                    .Add(Layout.Horizontal()
                        .Gap(8)
                        .Add(new Badge($"{category.Articles.Count} articles")
                            .Variant(BadgeVariant.Secondary))
                        .Add(new Badge(category.IsActive ? "Active" : "Inactive")
                            .Variant(category.IsActive ? BadgeVariant.Success : BadgeVariant.Secondary))))
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add(category.CreatedAt.ToString("MMM dd, yyyy"))
                    .Add(Layout.Horizontal()
                        .Gap(4)
                        .Add(new Button("View Articles", _ => client.Toast($"Viewing articles in: {category.Name}"))
                            .Small()
                            .Variant(ButtonVariant.Primary))
                        .Add(new Button("Edit", _ => client.Toast($"Editing category: {category.Name}"))
                            .Small()
                            .Variant(ButtonVariant.Outline))))
        ));

        return Layout.Vertical().Gap(8).Add(categoryCards);
    }
}
