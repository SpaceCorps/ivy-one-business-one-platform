using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Sign;

[App(icon: Icons.Signature, title: "Sign", path: new[] { "Customer Service" })]
public class SignApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var selectedView = this.UseState("pending");
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Digital Signature Management")
                .Add("Sign documents electronically and manage signature requests")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Pending", _ => selectedView.Set("pending"))
                        .Variant(selectedView.Value == "pending" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("Completed", _ => selectedView.Set("completed"))
                        .Variant(selectedView.Value == "completed" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("All Requests", _ => selectedView.Set("all"))
                        .Variant(selectedView.Value == "all" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("New Request", _ => selectedView.Set("create"))
                        .Variant(selectedView.Value == "create" ? ButtonVariant.Primary : ButtonVariant.Secondary)
                        .Icon(Icons.Plus)))
                .Add(BuildSelectedView(selectedView.Value, context, client))
        );
    }

    private object BuildSelectedView(string view, ApplicationDbContext context, IClientProvider client)
    {
        return view switch
        {
            "pending" => BuildPendingSignatures(context, client),
            "completed" => BuildCompletedSignatures(context, client),
            "all" => BuildAllSignatures(context, client),
            "create" => BuildCreateRequest(context, client),
            _ => "Select a view"
        };
    }

    private object BuildPendingSignatures(ApplicationDbContext context, IClientProvider client)
    {
        var signatures = context.Signatures
            .Where(s => s.Status == "Pending")
            .OrderByDescending(s => s.RequestDate)
            .ToList();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add($"Pending Signatures ({signatures.Count})"))
                .Add(signatures.Count == 0 
                    ? "No pending signature requests. All caught up!"
                    : BuildSignaturesList(signatures, client, showActions: true))
        );
    }

    private object BuildCompletedSignatures(ApplicationDbContext context, IClientProvider client)
    {
        var signatures = context.Signatures
            .Where(s => s.Status == "Signed")
            .OrderByDescending(s => s.SignedDate)
            .ToList();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add($"Completed Signatures ({signatures.Count})"))
                .Add(signatures.Count == 0 
                    ? "No completed signatures yet."
                    : BuildSignaturesList(signatures, client, showActions: false))
        );
    }

    private object BuildAllSignatures(ApplicationDbContext context, IClientProvider client)
    {
        var signatures = context.Signatures
            .OrderByDescending(s => s.RequestDate)
            .ToList();
        
        var stats = new
        {
            Total = signatures.Count,
            Pending = signatures.Count(s => s.Status == "Pending"),
            Signed = signatures.Count(s => s.Status == "Signed"),
            Declined = signatures.Count(s => s.Status == "Declined"),
            Expired = signatures.Count(s => s.Status == "Expired")
        };
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add("All Signature Requests")
                .Add(Layout.Grid()
                    .Columns(5)
                    .Gap(12)
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12)
                        .Add("Total").Add(stats.Total.ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12)
                        .Add("Pending").Add(stats.Pending.ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12)
                        .Add("Signed").Add(stats.Signed.ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12)
                        .Add("Declined").Add(stats.Declined.ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12)
                        .Add("Expired").Add(stats.Expired.ToString()))))
                .Add(signatures.Count == 0 
                    ? "No signature requests found."
                    : BuildSignaturesList(signatures, client, showActions: true))
        );
    }

    private object BuildCreateRequest(ApplicationDbContext context, IClientProvider client)
    {
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add("Create Signature Request")
                .Add(Layout.Vertical()
                    .Gap(12)
                    .Add(new TextInput(UseState(""))
                        .Placeholder("Document title..."))
                    .Add(new TextInput(UseState(""))
                        .Placeholder("Signer name..."))
                    .Add(new TextInput(UseState(""))
                        .Placeholder("Signer email...")
                        .Variant(TextInputs.Email))
                    .Add(new TextInput(UseState(""))
                        .Placeholder("Document path or URL..."))
                    .Add(new TextInput(UseState(""))
                        .Placeholder("Notes (optional)...")
                        .Variant(TextInputs.Textarea))
                    .Add(Layout.Horizontal()
                        .Gap(12)
                        .Add(new Button("Send Request", _ => client.Toast("Signature request sent!"))
                            .Variant(ButtonVariant.Primary)
                            .Icon(Icons.Send))
                        .Add(new Button("Save Draft", _ => client.Toast("Request saved as draft!"))
                            .Variant(ButtonVariant.Secondary))
                        .Add(new Button("Cancel", _ => client.Toast("Request cancelled!"))
                            .Variant(ButtonVariant.Outline))))
        );
    }

    private object BuildSignaturesList(List<Signature> signatures, IClientProvider client, bool showActions)
    {
        var signatureCards = signatures.Select(signature => new Card(
            Layout.Horizontal()
                .Gap(12)
                .Padding(16)
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add(signature.DocumentTitle)
                    .Add($"Signer: {signature.SignerName} ({signature.SignerEmail})")
                    .Add($"Requested: {signature.RequestDate:MMM dd, yyyy}")
                    .Add(Layout.Horizontal()
                        .Gap(8)
                        .Add(new Badge(signature.Status)
                            .Variant(signature.Status == "Signed" ? BadgeVariant.Success : 
                                   signature.Status == "Declined" ? BadgeVariant.Destructive :
                                   signature.Status == "Expired" ? BadgeVariant.Warning :
                                   BadgeVariant.Secondary))
                        .Add(signature.SignedDate.HasValue 
                            ? new Badge($"Signed: {signature.SignedDate.Value:MMM dd, yyyy}")
                                .Variant(BadgeVariant.Info)
                            : null)))
                .Add(showActions
                    ? Layout.Vertical()
                        .Gap(4)
                        .Add(Layout.Horizontal()
                            .Gap(4)
                            .Add(signature.Status == "Pending"
                                ? new Button("Sign Now", _ => client.Toast($"Signing: {signature.DocumentTitle}"))
                                    .Small()
                                    .Variant(ButtonVariant.Primary)
                                    .Icon(Icons.Pen)
                                : null)
                            .Add(new Button("View", _ => client.Toast($"Viewing: {signature.DocumentTitle}"))
                                .Small()
                                .Variant(ButtonVariant.Outline))
                            .Add(signature.Status == "Pending"
                                ? new Button("Decline", _ => client.Toast($"Declined: {signature.DocumentTitle}"))
                                    .Small()
                                    .Variant(ButtonVariant.Destructive)
                                : null))
                    : Layout.Vertical()
                        .Gap(4)
                        .Add(new Button("Download", _ => client.Toast($"Downloading: {signature.DocumentTitle}"))
                            .Small()
                            .Variant(ButtonVariant.Primary)
                            .Icon(Icons.Download)))
        ));

        return Layout.Vertical().Gap(8).Add(signatureCards);
    }
}
