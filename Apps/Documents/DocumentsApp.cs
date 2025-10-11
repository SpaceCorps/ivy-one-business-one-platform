using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Documents;

[App(icon: Icons.FileText, title: "Documents", path: new[] { "Development & Content" })]
public class DocumentsApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var selectedView = this.UseState("all");
        
        var documents = context.Documents.OrderByDescending(d => d.CreatedAt).ToList();
        var categories = documents.Select(d => d.Category).Distinct().ToList();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Document Management")
                .Add("Store, organize, and share documents")
                .Add(Layout.Grid()
                    .Columns(4)
                    .Gap(12)
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Total").Add(documents.Count.ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Categories").Add(categories.Count.ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Public").Add(documents.Count(d => d.IsPublic).ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Private").Add(documents.Count(d => !d.IsPublic).ToString()))))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("All Documents", _ => selectedView.Set("all"))
                        .Variant(selectedView.Value == "all" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("By Category", _ => selectedView.Set("categories"))
                        .Variant(selectedView.Value == "categories" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("Upload", _ => client.Toast("Upload document"))
                        .Icon(Icons.Upload)
                        .Variant(ButtonVariant.Success)))
                .Add(BuildDocumentsList(documents, client))
        );
    }

    private object BuildDocumentsList(List<Document> documents, IClientProvider client)
    {
        if (documents.Count == 0)
            return new Card(Layout.Vertical().Padding(16).Add("No documents found. Upload your first document!"));
        
        var docCards = documents.Select(doc => new Card(
            Layout.Horizontal()
                .Gap(12)
                .Padding(16)
                .Add(new Icon(Icons.FileText))
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add(doc.Title)
                    .Add(doc.Description)
                    .Add(Layout.Horizontal()
                        .Gap(8)
                        .Add(new Badge(doc.Category).Variant(BadgeVariant.Secondary))
                        .Add(new Badge(doc.FileType).Variant(BadgeVariant.Outline))
                        .Add(new Badge($"{doc.FileSize / 1024} KB").Variant(BadgeVariant.Info))
                        .Add(doc.IsPublic ? new Badge("Public").Variant(BadgeVariant.Success) : new Badge("Private").Variant(BadgeVariant.Warning))))
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add(doc.CreatedAt.ToString("MMM dd, yyyy"))
                    .Add(Layout.Horizontal()
                        .Gap(4)
                        .Add(new Button("Download", _ => client.Toast($"Downloading: {doc.Title}"))
                            .Small()
                            .Variant(ButtonVariant.Primary)
                            .Icon(Icons.Download))
                        .Add(new Button("Share", _ => client.Toast($"Sharing: {doc.Title}"))
                            .Small()
                            .Variant(ButtonVariant.Outline))))
        ));
        
        return Layout.Vertical().Gap(8).Add(docCards);
    }
}
