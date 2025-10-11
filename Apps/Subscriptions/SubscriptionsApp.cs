using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Subscriptions;

[App(icon: Icons.RefreshCw, title: "Subscriptions", path: new[] { "Commerce & Services" })]
public class SubscriptionsApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new SubscriptionsRootBlade(), "Subscriptions", Size.Units(80));
    }
}

public class SubscriptionsRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var subscriptions = context.Subscriptions.ToList();
        var activeCount = subscriptions.Count(s => s.Status == "Active");
        var monthlyRevenue = subscriptions.Where(s => s.Status == "Active").Sum(s => s.MonthlyAmount);
        
        var listItems = subscriptions.Select(sub => new ListItem(
            title: $"{sub.CustomerName} - {sub.PlanName}",
            subtitle: $"${sub.MonthlyAmount:N2}/{sub.BillingCycle} - Next: {sub.NextBillingDate:MMM dd}",
            icon: Icons.RefreshCw,
            badge: sub.Status,
            onClick: _ => blades.Push(this, new SubscriptionDetailBlade(sub.Id), sub.CustomerName)
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Text.H3("Subscriptions"))
            .Add(Layout.Grid()
                .Columns(3)
                .Gap(12)
                .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Active").Add(activeCount.ToString())))
                .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Monthly Revenue").Add($"${monthlyRevenue:N2}")))
                .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Total").Add(subscriptions.Count.ToString()))))
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(new Button("New Subscription", _ => client.Toast("Create subscription"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)))
            .Add(subscriptions.Count == 0 
                ? Text.Block("No subscriptions found.")
                : new List(listItems));
    }
}

public class SubscriptionDetailBlade(int subscriptionId) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var subscription = context.Subscriptions.FirstOrDefault(s => s.Id == subscriptionId);
        
        if (subscription == null)
            return "Subscription not found";
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add($"{subscription.CustomerName} - {subscription.PlanName}")
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add($"Customer: {subscription.CustomerName}")
                    .Add($"Email: {subscription.CustomerEmail}")
                    .Add($"Plan: {subscription.PlanName}")
                    .Add($"Amount: ${subscription.MonthlyAmount:N2}/{subscription.BillingCycle}")
                    .Add($"Started: {subscription.StartDate:MMM dd, yyyy}")
                    .Add(subscription.NextBillingDate.HasValue ? $"Next Billing: {subscription.NextBillingDate.Value:MMM dd, yyyy}" : "No next billing")
                    .Add(new Badge(subscription.Status)
                        .Variant(subscription.Status == "Active" ? BadgeVariant.Success :
                               subscription.Status == "Cancelled" ? BadgeVariant.Destructive :
                               BadgeVariant.Warning)))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Edit", _ => client.Toast("Edit subscription"))
                        .Variant(ButtonVariant.Primary))
                    .Add(subscription.Status == "Active" 
                        ? new Button("Cancel Subscription", _ => client.Toast("Subscription cancelled"))
                            .Variant(ButtonVariant.Destructive)
                        : null))
        );
    }
}
