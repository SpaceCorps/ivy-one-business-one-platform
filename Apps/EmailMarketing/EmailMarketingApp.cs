using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.EmailMarketing;

[App(icon: Icons.Mail, title: "Email Marketing", path: new[] { "Marketing & Communication" })]
public class EmailMarketingApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var campaigns = context.Campaigns.OrderByDescending(c => c.CreatedAt).Take(5).ToList();
        var subscribers = context.Subscribers.Count();
        var activeSubscribers = context.Subscribers.Count(s => s.Status == "Active");
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Email Marketing")
                .Add("Create and manage email campaigns")
                .Add(Layout.Grid()
                    .Columns(3)
                    .Gap(12)
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Subscribers").Add(subscribers.ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Active").Add(activeSubscribers.ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Campaigns").Add(campaigns.Count.ToString()))))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("New Campaign", _ => client.Toast("Creating email campaign"))
                        .Variant(ButtonVariant.Primary)
                        .Icon(Icons.Plus))
                    .Add(new Button("Templates", _ => client.Toast("View email templates"))
                        .Variant(ButtonVariant.Secondary))
                    .Add(new Button("Subscribers", _ => client.Toast("Manage subscribers"))
                        .Variant(ButtonVariant.Outline)))
                .Add(BuildCampaignsList(campaigns, client))
        );
    }

    private object BuildCampaignsList(List<Campaign> campaigns, IClientProvider client)
    {
        if (campaigns.Count == 0)
            return new Card(Layout.Vertical().Padding(16).Add("No campaigns yet. Create your first campaign!"));
        
        var cards = campaigns.Select(camp => new Card(
            Layout.Horizontal()
                .Gap(12)
                .Padding(16)
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add(camp.Name)
                    .Add(camp.Description)
                    .Add(Layout.Horizontal()
                        .Gap(8)
                        .Add(new Badge(camp.Status)
                            .Variant(camp.Status == "Active" ? BadgeVariant.Success :
                                   camp.Status == "Draft" ? BadgeVariant.Secondary :
                                   BadgeVariant.Warning))
                        .Add(new Badge(camp.Type).Variant(BadgeVariant.Info))
                        .Add(new Badge($"${camp.Budget:N0}").Variant(BadgeVariant.Outline))))
                .Add(Layout.Horizontal()
                    .Gap(4)
                    .Add(new Button("View", _ => client.Toast($"Viewing: {camp.Name}"))
                        .Small()
                        .Variant(ButtonVariant.Primary))
                    .Add(new Button("Edit", _ => client.Toast($"Editing: {camp.Name}"))
                        .Small()
                        .Variant(ButtonVariant.Outline)))
        ));
        
        return Layout.Vertical().Gap(8).Add(cards);
    }
}
