using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Sign;

[App(icon: Icons.Signature, title: "Sign", path: new[] { "Customer Service" })]
public class SignApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new SignRootBlade(), "Digital Signatures", Size.Units(80));
    }
}

public class SignRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var signatures = context.Signatures.OrderByDescending(s => s.RequestDate).ToList();
        var pending = signatures.Count(s => s.Status == "Pending");
        var signed = signatures.Count(s => s.Status == "Signed");
        
        var listItems = signatures.Select(sig => new ListItem(
            title: sig.DocumentTitle,
            subtitle: $"{sig.SignerName} ({sig.SignerEmail}) - Requested: {sig.RequestDate:MMM dd, yyyy}",
            icon: Icons.Pen,
            badge: sig.Status,
            onClick: _ => blades.Push(this, new SignatureDetailBlade(sig.Id), sig.DocumentTitle)
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Text.H3("Digital Signatures"))
            .Add(Layout.Grid()
                .Columns(3)
                .Gap(12)
                .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Total").Add(signatures.Count.ToString())))
                .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Pending").Add(pending.ToString())))
                .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Signed").Add(signed.ToString()))))
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(new Button("New Signature Request", _ => client.Toast("Create request"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)))
            .Add(signatures.Count == 0 
                ? Text.Block("No signature requests. Create your first request!")
                : new List(listItems));
    }
}

public class SignatureDetailBlade(int signatureId) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var signature = context.Signatures.FirstOrDefault(s => s.Id == signatureId);
        
        if (signature == null)
            return "Signature request not found";
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(signature.DocumentTitle)
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add($"Signer: {signature.SignerName}")
                    .Add($"Email: {signature.SignerEmail}")
                    .Add($"Requested: {signature.RequestDate:MMM dd, yyyy}")
                    .Add(signature.SignedDate.HasValue ? $"Signed: {signature.SignedDate.Value:MMM dd, yyyy}" : "Not signed yet")
                    .Add($"Document: {signature.DocumentPath}")
                    .Add(new Badge(signature.Status)
                        .Variant(signature.Status == "Signed" ? BadgeVariant.Success :
                               signature.Status == "Declined" ? BadgeVariant.Destructive :
                               signature.Status == "Expired" ? BadgeVariant.Warning :
                               BadgeVariant.Secondary)))
                .Add(signature.Notes != string.Empty ? $"Notes: {signature.Notes}" : null)
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(signature.Status == "Pending" 
                        ? new Button("Sign Now", _ => client.Toast("Opening signature pad"))
                            .Variant(ButtonVariant.Primary)
                            .Icon(Icons.Pen)
                        : null)
                    .Add(new Button("Download", _ => client.Toast("Download document"))
                        .Variant(ButtonVariant.Secondary)
                        .Icon(Icons.Download))
                    .Add(signature.Status == "Pending" 
                        ? new Button("Decline", _ => client.Toast("Signature declined"))
                            .Variant(ButtonVariant.Destructive)
                        : null))
        );
    }
}
