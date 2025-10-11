using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Documents;

[App(icon: Icons.FileText, title: "Documents", path: new[] { "Development & Content" })]
public class DocumentsApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new DocumentsRootBlade(), "Documents");
    }
}

public class DocumentsRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var documents = context.Documents.OrderByDescending(d => d.CreatedAt).ToList();
        
        var listItems = documents.Select(doc => new ListItem(
            title: doc.Title,
            subtitle: $"{doc.Category} - {doc.FileType} - {doc.FileSize / 1024} KB",
            icon: Icons.FileText,
            badge: doc.IsPublic ? "Public" : "Private",
            onClick: _ => { blades.Push(this, new DocumentDetailBlade(doc.Id), doc.Title); }
        ));
        
        return BladeHelper.WithHeader(
            Layout.Vertical()
                .Gap(12)
                .Add(Layout.Grid()
                    .Columns(2)
                    .Gap(12)
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Total").Add(documents.Count.ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Public").Add(documents.Count(d => d.IsPublic).ToString()))))
                .Add(new Button("Upload Document", _ => client.Toast("Upload document"))
                    .Icon(Icons.Upload)
                    .Variant(ButtonVariant.Primary)),
            documents.Count == 0 
                ? "No documents found."
                : new List(listItems)
        );
    }
}

public class DocumentDetailBlade(int documentId) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var document = context.Documents.FirstOrDefault(d => d.Id == documentId);
        
        if (document == null)
            return "Document not found";
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(document.Title)
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add($"Description: {document.Description}")
                    .Add($"Category: {document.Category}")
                    .Add($"File: {document.FileName}")
                    .Add($"Type: {document.FileType}")
                    .Add($"Size: {document.FileSize / 1024} KB")
                    .Add($"Author: {document.Author}")
                    .Add(new Badge(document.IsPublic ? "Public" : "Private")
                        .Variant(document.IsPublic ? BadgeVariant.Success : BadgeVariant.Warning)))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Download", _ => client.Toast($"Downloading {document.Title}"))
                        .Variant(ButtonVariant.Primary)
                        .Icon(Icons.Download))
                    .Add(new Button("Share", _ => client.Toast("Share document"))
                        .Variant(ButtonVariant.Secondary)))
        );
    }
}
