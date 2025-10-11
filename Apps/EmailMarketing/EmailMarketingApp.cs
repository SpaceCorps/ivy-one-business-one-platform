using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.EmailMarketing;

[App(icon: Icons.Mail, title: "Email Marketing", path: new[] { "Marketing & Communication" })]
public class EmailMarketingApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new EmailMarketingRootBlade(), "Email Marketing");
    }
}

public class EmailMarketingRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var campaigns = context.Campaigns.OrderByDescending(c => c.CreatedAt).Take(10).ToList();
        
        var listItems = campaigns.Select(camp => new ListItem(
            title: camp.Name,
            subtitle: $"{camp.Type} - {camp.Status} - ${camp.Budget:N0}",
            icon: Icons.Mail,
            badge: camp.Status,
            onClick: _ => { blades.Push(this, new CampaignDetailBlade(camp.Id), camp.Name); return default; }
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(Text.H3("Email Campaigns"))
                .Add(new Button("New Campaign", _ => client.Toast("Create campaign"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)))
            .Add(campaigns.Count == 0 
                ? Text.Block("No campaigns yet.")
                : new List(listItems));
    }
}

public class CampaignDetailBlade(int campaignId) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var campaign = context.Campaigns.FirstOrDefault(c => c.Id == campaignId);
        
        if (campaign == null)
            return "Campaign not found";
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(campaign.Name)
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add($"Type: {campaign.Type}")
                    .Add($"Budget: ${campaign.Budget:N0}")
                    .Add($"Start: {campaign.StartDate:MMM dd, yyyy}")
                    .Add($"Target: {campaign.TargetAudience}")
                    .Add(new Badge(campaign.Status)
                        .Variant(campaign.Status == "Active" ? BadgeVariant.Success : BadgeVariant.Secondary)))
                .Add($"Description: {campaign.Description}")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Edit", _ => client.Toast("Edit campaign"))
                        .Variant(ButtonVariant.Primary))
                    .Add(new Button("Send", _ => client.Toast("Send campaign"))
                        .Variant(ButtonVariant.Success)))
        );
    }
}
