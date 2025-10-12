using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Documents;

[App(icon: Icons.FileText, title: "Documents", path: new[] { "Development & Content" })]
public class DocumentsApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new DocumentsRootBlade(), "Documents", Size.Units(100));
    }
}

public class DocumentsRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var searchQuery = this.UseState("");
        var isNewDocumentOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        var query = context.Documents.AsQueryable();
        
        if (!string.IsNullOrEmpty(searchQuery.Value))
        {
            var searchPattern = $"%{searchQuery.Value}%";
            query = query.Where(d => 
                EF.Functions.Like(d.Title, searchPattern) ||
                EF.Functions.Like(d.Description, searchPattern) ||
                EF.Functions.Like(d.Category, searchPattern) ||
                EF.Functions.Like(d.Author, searchPattern) ||
                EF.Functions.Like(d.Tags, searchPattern));
        }
        
        var documents = query.OrderByDescending(d => d.CreatedAt).ToList();
        
        var listItems = documents.Select(doc => new ListItem(
            title: doc.Title,
            subtitle: $"{doc.Category} - {doc.FileType} - {doc.FileSize / 1024} KB",
            icon: Icons.FileText,
            badge: doc.IsPublic ? "Public" : "Private",
            onClick: _ => blades.Push(this, new DocumentDetailBlade(doc.Id, () => refreshToken.Refresh()), doc.Title)
        ));
        
        var mainContent = BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(4)
                .Add(searchQuery.ToSearchInput().Placeholder("Search by title, description, category, author, or tags..."))
                .Add(new Button("Upload Document")
                    .Icon(Icons.Upload)
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => isNewDocumentOpen.Set(true))),
            documents.Count == 0 
                ? Text.Block("No documents found. Try a different search or upload your first document!")
                : new List(listItems)
        );

        return isNewDocumentOpen.Value ? new Sheet(
            (Event<Sheet> _) => isNewDocumentOpen.Set(false),
            new DocumentFormSheet(null, () => {
                isNewDocumentOpen.Set(false);
                refreshToken.Refresh();
            }),
            title: "Upload Document",
            description: "Upload a new document"
        ).Width(Size.Fraction(1/3f)) : mainContent;
    }
}

public class DocumentDetailBlade(int documentId, Action? onRefresh = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var initialDocument = context.Documents.FirstOrDefault(d => d.Id == documentId);
        
        if (initialDocument == null)
        {
            return Layout.Vertical()
                .Gap(4)
                .Add(Text.H3("Document Not Found"))
                .Add(new Button("Go Back", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary));
        }
        
        var documentData = this.UseState(initialDocument);
        var isEditOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        this.UseEffect(() =>
        {
            var updatedDocument = context.Documents.FirstOrDefault(d => d.Id == documentId);
            if (updatedDocument != null)
            {
                documentData.Set(updatedDocument);
            }
        }, [refreshToken.ToTrigger()]);
        
        var visibilityBadge = new Badge(documentData.Value.IsPublic ? "Public" : "Private")
            .Variant(documentData.Value.IsPublic ? BadgeVariant.Success : BadgeVariant.Warning);

        var documentDetails = new
        {
            Title = documentData.Value.Title,
            Description = documentData.Value.Description,
            FileName = documentData.Value.FileName,
            FileType = documentData.Value.FileType,
            FileSize = $"{documentData.Value.FileSize / 1024} KB",
            Category = documentData.Value.Category,
            Tags = documentData.Value.Tags,
            Author = documentData.Value.Author,
            Visibility = visibilityBadge,
            CreatedAt = documentData.Value.CreatedAt.ToString("MMM dd, yyyy HH:mm")
        };
        
        return Layout.Vertical()
            .Gap(4)
            .Add(Text.H3(documentData.Value.Title))
            .Add(documentDetails.ToDetails().RemoveEmpty().MultiLine(x => x.Description))
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(new Button("Download")
                    .Variant(ButtonVariant.Primary)
                    .Icon(Icons.Download)
                    .HandleClick(_ => client.Toast($"Downloading {documentData.Value.FileName}")))
                .Add(new Button(documentData.Value.IsPublic ? "Make Private" : "Make Public", _ => {
                    var dbDocument = context.Documents.FirstOrDefault(d => d.Id == documentId);
                    if (dbDocument != null)
                    {
                        dbDocument.IsPublic = !dbDocument.IsPublic;
                        dbDocument.UpdatedAt = DateTime.UtcNow;
                        context.SaveChanges();
                        client.Toast($"Document visibility changed to {(dbDocument.IsPublic ? "Public" : "Private")}!");
                        refreshToken.Refresh();
                        onRefresh?.Invoke();
                    }
                })
                    .Variant(ButtonVariant.Secondary)
                    .Icon(documentData.Value.IsPublic ? Icons.Lock : Icons.Globe))
                .Add(new Button("Edit Document")
                    .Variant(ButtonVariant.Outline)
                    .Icon(Icons.Pencil)
                    .HandleClick(_ => isEditOpen.Set(true)))
                .Add(new Button("Delete Document")
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.Trash)
                    .HandleClick(_ => {
                        try
                        {
                            var documentToDelete = context.Documents.FirstOrDefault(d => d.Id == documentId);
                            if (documentToDelete != null)
                            {
                                context.Documents.Remove(documentToDelete);
                                context.SaveChanges();
                                client.Toast($"Document {documentData.Value.Title} deleted successfully!");
                                refreshToken.Refresh();
                                onRefresh?.Invoke();
                                blades.Pop();
                            }
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error deleting document: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary)))
            .Add(isEditOpen.Value ? new Sheet(
                (Event<Sheet> _) => isEditOpen.Set(false),
                new DocumentFormSheet(documentId, () => {
                    isEditOpen.Set(false);
                    refreshToken.Refresh();
                    onRefresh?.Invoke();
                }),
                title: "Edit Document",
                description: $"Edit document {documentData.Value.Title}"
            ).Width(Size.Fraction(1/3f)) : null);
    }
}

public class DocumentFormSheet(int? documentId = null, Action? onClose = null) : ViewBase
{
    private Document CloneDocument(Document source) => new Document
    {
        Id = source.Id,
        Title = source.Title,
        Description = source.Description,
        FileName = source.FileName,
        FilePath = source.FilePath,
        FileType = source.FileType,
        FileSize = source.FileSize,
        Category = source.Category,
        Tags = source.Tags,
        Author = source.Author,
        IsPublic = source.IsPublic,
        CreatedAt = source.CreatedAt,
        UpdatedAt = source.UpdatedAt
    };
    
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var isEdit = documentId.HasValue;
        var existingDocument = isEdit ? context.Documents.FirstOrDefault(d => d.Id == documentId!.Value) : null;
        
        var documentForm = this.UseState(existingDocument ?? new Document
        {
            Title = "",
            Description = "",
            FileName = "",
            FilePath = "",
            FileType = "",
            FileSize = 0,
            Category = "",
            Tags = "",
            Author = "",
            IsPublic = false
        });
        
        var categoryOptions = new[] { "Contract", "Invoice", "Report", "Proposal", "Legal", "Marketing", "HR", "Other" };
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(documentForm.Value.Title))
                            {
                                client.Toast("Title is required", "Validation Error");
                                return;
                            }
                            
                            if (string.IsNullOrWhiteSpace(documentForm.Value.FileName))
                            {
                                client.Toast("File Name is required", "Validation Error");
                                return;
                            }
                            
                            if (string.IsNullOrWhiteSpace(documentForm.Value.FilePath))
                            {
                                client.Toast("File Path is required", "Validation Error");
                                return;
                            }
                            
                            if (isEdit && existingDocument != null)
                            {
                                existingDocument.Title = documentForm.Value.Title;
                                existingDocument.Description = documentForm.Value.Description;
                                existingDocument.FileName = documentForm.Value.FileName;
                                existingDocument.FilePath = documentForm.Value.FilePath;
                                existingDocument.FileType = documentForm.Value.FileType;
                                existingDocument.FileSize = documentForm.Value.FileSize;
                                existingDocument.Category = documentForm.Value.Category;
                                existingDocument.Tags = documentForm.Value.Tags;
                                existingDocument.Author = documentForm.Value.Author;
                                existingDocument.IsPublic = documentForm.Value.IsPublic;
                                existingDocument.UpdatedAt = DateTime.UtcNow;
                            }
                            else
                            {
                                var newDocument = new Document
                                {
                                    Title = documentForm.Value.Title,
                                    Description = documentForm.Value.Description,
                                    FileName = documentForm.Value.FileName,
                                    FilePath = documentForm.Value.FilePath,
                                    FileType = documentForm.Value.FileType,
                                    FileSize = documentForm.Value.FileSize,
                                    Category = documentForm.Value.Category,
                                    Tags = documentForm.Value.Tags,
                                    Author = documentForm.Value.Author,
                                    IsPublic = documentForm.Value.IsPublic,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                context.Documents.Add(newDocument);
                            }
                            
                            context.SaveChanges();
                            client.Toast(isEdit ? "Document updated successfully!" : "Document created successfully!");
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
                        .Add(Text.Small("Document Information"))
                        .Add(Text.Small("Title"))
                        .Add(new TextInput(documentForm.Value.Title, e => {
                            var cloned = CloneDocument(documentForm.Value);
                            cloned.Title = e.Value;
                            documentForm.Set(cloned);
                        }).Placeholder("Document Title"))
                        .Add(Text.Small("Description"))
                        .Add(new TextInput(documentForm.Value.Description, e => {
                            var cloned = CloneDocument(documentForm.Value);
                            cloned.Description = e.Value;
                            documentForm.Set(cloned);
                        }).Placeholder("Document description...").Variant(TextInputs.Textarea))
                        .Add(Text.Small("File Name"))
                        .Add(new TextInput(documentForm.Value.FileName, e => {
                            var cloned = CloneDocument(documentForm.Value);
                            cloned.FileName = e.Value;
                            documentForm.Set(cloned);
                        }).Placeholder("document.pdf"))
                        .Add(Text.Small("File Path"))
                        .Add(new TextInput(documentForm.Value.FilePath, e => {
                            var cloned = CloneDocument(documentForm.Value);
                            cloned.FilePath = e.Value;
                            documentForm.Set(cloned);
                        }).Placeholder("/documents/file.pdf"))
                        .Add(Text.Small("File Type"))
                        .Add(new TextInput(documentForm.Value.FileType, e => {
                            var cloned = CloneDocument(documentForm.Value);
                            cloned.FileType = e.Value;
                            documentForm.Set(cloned);
                        }).Placeholder("PDF"))
                        .Add(Text.Small("File Size (bytes)"))
                        .Add(new NumberInput<long>(documentForm.Value.FileSize, v => {
                            var cloned = CloneDocument(documentForm.Value);
                            cloned.FileSize = v;
                            documentForm.Set(cloned);
                        }).Placeholder("0"))
                ).Title("Document Details"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Document Settings"))
                        .Add(Text.Small("Category"))
                        .Add(new SelectInput<string>(documentForm.Value.Category, e => {
                            var cloned = CloneDocument(documentForm.Value);
                            cloned.Category = e.Value;
                            documentForm.Set(cloned);
                        }, categoryOptions.ToOptions()))
                        .Add(Text.Small("Author"))
                        .Add(new TextInput(documentForm.Value.Author, e => {
                            var cloned = CloneDocument(documentForm.Value);
                            cloned.Author = e.Value;
                            documentForm.Set(cloned);
                        }).Placeholder("Author Name"))
                        .Add(Text.Small("Tags (comma separated)"))
                        .Add(new TextInput(documentForm.Value.Tags, e => {
                            var cloned = CloneDocument(documentForm.Value);
                            cloned.Tags = e.Value;
                            documentForm.Set(cloned);
                        }).Placeholder("tag1, tag2, tag3"))
                        .Add(Text.Small("Visibility"))
                        .Add(new SelectInput<bool>(documentForm.Value.IsPublic, e => {
                            var cloned = CloneDocument(documentForm.Value);
                            cloned.IsPublic = e.Value;
                            documentForm.Set(cloned);
                        }, new[] { (true, "Public"), (false, "Private") }.ToOptions()))
                ).Title("Settings"))
        );
    }
}
