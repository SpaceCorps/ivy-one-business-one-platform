using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Sign;

[App(icon: Icons.Signature, title: "Sign", path: new[] { "Customer Service" })]
public class SignApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new SignRootBlade(), "Digital Signatures", Size.Units(110));
    }
}

public class SignRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var searchQuery = this.UseState("");
        var isNewSignatureOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        var query = context.Signatures.AsQueryable();
        
        if (!string.IsNullOrEmpty(searchQuery.Value))
        {
            var searchPattern = $"%{searchQuery.Value}%";
            query = query.Where(s => 
                EF.Functions.Like(s.DocumentTitle, searchPattern) ||
                EF.Functions.Like(s.SignerName, searchPattern) ||
                EF.Functions.Like(s.SignerEmail, searchPattern) ||
                EF.Functions.Like(s.Status, searchPattern));
        }
        
        var signatures = query.OrderByDescending(s => s.RequestDate).ToList();
        
        var listItems = signatures.Select(sig => new ListItem(
            title: sig.DocumentTitle,
            subtitle: $"{sig.SignerName} ({sig.SignerEmail}) - Requested: {sig.RequestDate:MMM dd, yyyy}",
            icon: Icons.Pen,
            badge: sig.Status,
            onClick: _ => blades.Push(this, new SignatureDetailBlade(sig.Id, () => refreshToken.Refresh()), sig.DocumentTitle)
        ));
        
        var mainContent = BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(4)
                .Add(searchQuery.ToSearchInput().Placeholder("Search by document, signer name, email, or status..."))
                .Add(new Button("New Signature Request")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => isNewSignatureOpen.Set(true))),
            signatures.Count == 0 
                ? Text.Block("No signature requests. Try a different search or create your first request!")
                : new List(listItems)
        );

        return isNewSignatureOpen.Value ? new Sheet(
            (Event<Sheet> _) => isNewSignatureOpen.Set(false),
            new SignatureFormSheet(null, () => {
                isNewSignatureOpen.Set(false);
                refreshToken.Refresh();
            }),
            title: "New Signature Request",
            description: "Create a new signature request"
        ).Width(Size.Fraction(1/3f)) : mainContent;
    }
}

public class SignatureDetailBlade(int signatureId, Action? onRefresh = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var initialSignature = context.Signatures.FirstOrDefault(s => s.Id == signatureId);
        
        if (initialSignature == null)
        {
            return Layout.Vertical()
                .Gap(4)
                .Add(Text.H3("Signature Request Not Found"))
                .Add(new Button("Go Back", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary));
        }
        
        var signatureData = this.UseState(initialSignature);
        var isEditOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        this.UseEffect(() =>
        {
            var updatedSignature = context.Signatures.FirstOrDefault(s => s.Id == signatureId);
            if (updatedSignature != null)
            {
                signatureData.Set(updatedSignature);
            }
        }, [refreshToken.ToTrigger()]);
        
        var statusBadge = new Badge(signatureData.Value.Status)
            .Variant(signatureData.Value.Status == "Signed" ? BadgeVariant.Success :
                   signatureData.Value.Status == "Declined" ? BadgeVariant.Destructive :
                   signatureData.Value.Status == "Expired" ? BadgeVariant.Warning :
                   BadgeVariant.Secondary);

        var signatureDetails = new
        {
            DocumentTitle = signatureData.Value.DocumentTitle,
            SignerName = signatureData.Value.SignerName,
            SignerEmail = signatureData.Value.SignerEmail,
            DocumentPath = signatureData.Value.DocumentPath,
            RequestDate = signatureData.Value.RequestDate.ToString("MMM dd, yyyy"),
            SignedDate = signatureData.Value.SignedDate?.ToString("MMM dd, yyyy HH:mm"),
            ExpiryDate = signatureData.Value.ExpiryDate?.ToString("MMM dd, yyyy"),
            IpAddress = signatureData.Value.IpAddress,
            Status = statusBadge,
            Notes = signatureData.Value.Notes
        };
        
        return Layout.Vertical()
            .Gap(4)
            .Add(Text.H3(signatureData.Value.DocumentTitle))
            .Add(signatureDetails.ToDetails().RemoveEmpty().MultiLine(x => x.Notes))
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(signatureData.Value.Status == "Pending" ? new Button("Mark as Signed", _ => {
                    var dbSignature = context.Signatures.FirstOrDefault(s => s.Id == signatureId);
                    if (dbSignature != null)
                    {
                        dbSignature.Status = "Signed";
                        dbSignature.SignedDate = DateTime.UtcNow;
                        dbSignature.UpdatedAt = DateTime.UtcNow;
                        context.SaveChanges();
                        client.Toast($"Document {signatureData.Value.DocumentTitle} marked as signed!");
                        refreshToken.Refresh();
                        onRefresh?.Invoke();
                    }
                })
                    .Variant(ButtonVariant.Success)
                    .Icon(Icons.Check)
                    : null)
                .Add(signatureData.Value.Status == "Pending" ? new Button("Decline", _ => {
                    var dbSignature = context.Signatures.FirstOrDefault(s => s.Id == signatureId);
                    if (dbSignature != null)
                    {
                        dbSignature.Status = "Declined";
                        dbSignature.UpdatedAt = DateTime.UtcNow;
                        context.SaveChanges();
                        client.Toast($"Signature request for {signatureData.Value.DocumentTitle} declined!");
                        refreshToken.Refresh();
                        onRefresh?.Invoke();
                    }
                })
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.X)
                    : null)
                .Add(new Button("Edit Request")
                    .Variant(ButtonVariant.Outline)
                    .Icon(Icons.Pencil)
                    .HandleClick(_ => isEditOpen.Set(true)))
                .Add(new Button("Delete Request")
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.Trash)
                    .HandleClick(_ => {
                        try
                        {
                            var signatureToDelete = context.Signatures.FirstOrDefault(s => s.Id == signatureId);
                            if (signatureToDelete != null)
                            {
                                context.Signatures.Remove(signatureToDelete);
                                context.SaveChanges();
                                client.Toast($"Signature request for {signatureData.Value.DocumentTitle} deleted successfully!");
                                refreshToken.Refresh();
                                onRefresh?.Invoke();
                                blades.Pop();
                            }
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error deleting signature: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary)))
            .Add(isEditOpen.Value ? new Sheet(
                (Event<Sheet> _) => isEditOpen.Set(false),
                new SignatureFormSheet(signatureId, () => {
                    isEditOpen.Set(false);
                    refreshToken.Refresh();
                    onRefresh?.Invoke();
                }),
                title: "Edit Signature Request",
                description: $"Edit signature request for {signatureData.Value.DocumentTitle}"
            ).Width(Size.Fraction(1/3f)) : null);
    }
}

public class SignatureFormSheet(int? signatureId = null, Action? onClose = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var isEdit = signatureId.HasValue;
        var existingSignature = isEdit ? context.Signatures.FirstOrDefault(s => s.Id == signatureId!.Value) : null;
        
        var signatureForm = this.UseState(existingSignature ?? new Signature
        {
            DocumentTitle = "",
            DocumentPath = "",
            SignerName = "",
            SignerEmail = "",
            Status = "Pending",
            RequestDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            Notes = ""
        });
        
        var statusOptions = new[] { "Pending", "Signed", "Declined", "Expired" };
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(signatureForm.Value.DocumentTitle))
                            {
                                client.Toast("Document Title is required", "Validation Error");
                                return;
                            }
                            
                            if (string.IsNullOrWhiteSpace(signatureForm.Value.DocumentPath))
                            {
                                client.Toast("Document Path is required", "Validation Error");
                                return;
                            }
                            
                            if (string.IsNullOrWhiteSpace(signatureForm.Value.SignerName))
                            {
                                client.Toast("Signer Name is required", "Validation Error");
                                return;
                            }
                            
                            if (string.IsNullOrWhiteSpace(signatureForm.Value.SignerEmail))
                            {
                                client.Toast("Signer Email is required", "Validation Error");
                                return;
                            }
                            
                            if (isEdit && existingSignature != null)
                            {
                                existingSignature.DocumentTitle = signatureForm.Value.DocumentTitle;
                                existingSignature.DocumentPath = signatureForm.Value.DocumentPath;
                                existingSignature.SignerName = signatureForm.Value.SignerName;
                                existingSignature.SignerEmail = signatureForm.Value.SignerEmail;
                                existingSignature.Status = signatureForm.Value.Status;
                                existingSignature.ExpiryDate = signatureForm.Value.ExpiryDate;
                                existingSignature.Notes = signatureForm.Value.Notes;
                                existingSignature.UpdatedAt = DateTime.UtcNow;
                            }
                            else
                            {
                                var newSignature = new Signature
                                {
                                    DocumentTitle = signatureForm.Value.DocumentTitle,
                                    DocumentPath = signatureForm.Value.DocumentPath,
                                    SignerName = signatureForm.Value.SignerName,
                                    SignerEmail = signatureForm.Value.SignerEmail,
                                    Status = signatureForm.Value.Status,
                                    RequestDate = signatureForm.Value.RequestDate,
                                    ExpiryDate = signatureForm.Value.ExpiryDate,
                                    Notes = signatureForm.Value.Notes,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                context.Signatures.Add(newSignature);
                            }
                            
                            context.SaveChanges();
                            client.Toast(isEdit ? "Signature request updated successfully!" : "Signature request created successfully!");
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
                        .Add(Text.Small("Document Title"))
                        .Add(new TextInput(signatureForm.Value.DocumentTitle, e => {
                            var updated = signatureForm.Value;
                            updated.DocumentTitle = e.Value;
                            signatureForm.Set(updated);
                        }).Placeholder("Contract Agreement"))
                        .Add(Text.Small("Document Path"))
                        .Add(new TextInput(signatureForm.Value.DocumentPath, e => {
                            var updated = signatureForm.Value;
                            updated.DocumentPath = e.Value;
                            signatureForm.Set(updated);
                        }).Placeholder("/documents/contract.pdf"))
                ).Title("Document"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Signer Information"))
                        .Add(Text.Small("Signer Name"))
                        .Add(new TextInput(signatureForm.Value.SignerName, e => {
                            var updated = signatureForm.Value;
                            updated.SignerName = e.Value;
                            signatureForm.Set(updated);
                        }).Placeholder("John Doe"))
                        .Add(Text.Small("Signer Email"))
                        .Add(new TextInput(signatureForm.Value.SignerEmail, e => {
                            var updated = signatureForm.Value;
                            updated.SignerEmail = e.Value;
                            signatureForm.Set(updated);
                        }).Placeholder("signer@example.com"))
                ).Title("Signer"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Request Settings"))
                        .Add(Text.Small("Status"))
                        .Add(new SelectInput<string>(signatureForm.Value.Status, e => {
                            var updated = signatureForm.Value;
                            updated.Status = e.Value;
                            signatureForm.Set(updated);
                        }, statusOptions.ToOptions()))
                        .Add(Text.Small("Expiry Date (Optional)"))
                        .Add(new DateTimeInput<DateTime?>(signatureForm.Value.ExpiryDate, e => {
                            var updated = signatureForm.Value;
                            updated.ExpiryDate = e.Value;
                            signatureForm.Set(updated);
                        }))
                        .Add(Text.Small("Notes"))
                        .Add(new TextInput(signatureForm.Value.Notes, e => {
                            var updated = signatureForm.Value;
                            updated.Notes = e.Value;
                            signatureForm.Set(updated);
                        }).Placeholder("Additional notes...").Variant(TextInputs.Textarea))
                ).Title("Settings"))
        );
    }
}
