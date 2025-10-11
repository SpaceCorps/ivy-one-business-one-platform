namespace IvyOneBusinessOnePlatform.Apps.Subscriptions;

[App(icon: Icons.RefreshCw, title: "Subscriptions")]
public class SubscriptionsApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Subscription Management")
                .Add("Manage recurring subscriptions and billing cycles")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("View Subscriptions", _ => client.Toast("Subscription list coming soon!")))
                    .Add(new Button("Billing History", _ => client.Toast("Billing history coming soon!")))
                    .Add(new Button("Manage Plans", _ => client.Toast("Plan management coming soon!")))
                )
        );
    }
}
