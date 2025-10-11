using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Subscriptions;

[App(icon: Icons.RefreshCw, title: "Subscriptions", path: new[] { "Commerce & Services" })]
public class SubscriptionsApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var selectedView = this.UseState("active");
        
        var subscriptions = context.Subscriptions.ToList();
        var activeCount = subscriptions.Count(s => s.Status == "Active");
        var monthlyRevenue = subscriptions.Where(s => s.Status == "Active").Sum(s => s.MonthlyAmount);
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Subscription Management")
                .Add("Manage recurring billing and customer subscriptions")
                .Add(Layout.Grid()
                    .Columns(3)
                    .Gap(12)
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12)
                        .Add("Active Subscriptions").Add(activeCount.ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12)
                        .Add("Monthly Revenue").Add($"${monthlyRevenue:N2}")))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12)
                        .Add("Total Subscriptions").Add(subscriptions.Count.ToString()))))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Active", _ => selectedView.Set("active"))
                        .Variant(selectedView.Value == "active" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("Cancelled", _ => selectedView.Set("cancelled"))
                        .Variant(selectedView.Value == "cancelled" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("All", _ => selectedView.Set("all"))
                        .Variant(selectedView.Value == "all" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("New Subscription", _ => client.Toast("Create new subscription"))
                        .Icon(Icons.Plus)
                        .Variant(ButtonVariant.Success)))
                .Add(BuildSubscriptionsList(context, selectedView.Value, client))
        );
    }

    private object BuildSubscriptionsList(ApplicationDbContext context, string filter, IClientProvider client)
    {
        var query = context.Subscriptions.AsQueryable();
        
        if (filter == "active")
            query = query.Where(s => s.Status == "Active");
        else if (filter == "cancelled")
            query = query.Where(s => s.Status == "Cancelled");
            
        var subscriptions = query.OrderByDescending(s => s.StartDate).ToList();
        
        if (subscriptions.Count == 0)
            return new Card(Layout.Vertical().Padding(16).Add("No subscriptions found."));
        
        var cards = subscriptions.Select(sub => new Card(
            Layout.Horizontal()
                .Gap(12)
                .Padding(16)
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add(sub.CustomerName)
                    .Add(sub.CustomerEmail)
                    .Add($"Plan: {sub.PlanName}")
                    .Add(Layout.Horizontal()
                        .Gap(8)
                        .Add(new Badge(sub.Status)
                            .Variant(sub.Status == "Active" ? BadgeVariant.Success :
                                   sub.Status == "Cancelled" ? BadgeVariant.Destructive :
                                   BadgeVariant.Warning))
                        .Add(new Badge($"${sub.MonthlyAmount:N2}/{sub.BillingCycle}")
                            .Variant(BadgeVariant.Info))
                        .Add(sub.NextBillingDate.HasValue 
                            ? new Badge($"Next: {sub.NextBillingDate.Value:MMM dd}")
                                .Variant(BadgeVariant.Outline)
                            : null)))
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add($"Started: {sub.StartDate:MMM dd, yyyy}")
                    .Add(Layout.Horizontal()
                        .Gap(4)
                        .Add(new Button("View", _ => client.Toast($"Viewing: {sub.CustomerName}"))
                            .Small()
                            .Variant(ButtonVariant.Primary))
                        .Add(sub.Status == "Active" 
                            ? new Button("Cancel", _ => client.Toast($"Cancelling subscription"))
                                .Small()
                                .Variant(ButtonVariant.Destructive)
                            : null)))
        ));
        
        return Layout.Vertical().Gap(8).Add(cards);
    }
}
