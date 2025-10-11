namespace IvyOneBusinessOnePlatform.Apps.Sales;

[App(icon: Icons.TrendingUp, title: "Sales")]
public class SalesApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Sales Management")
                .Add("Track sales performance, manage leads, and analyze revenue")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Sales Dashboard", _ => client.Toast("Sales analytics coming soon!")))
                    .Add(new Button("Lead Tracking", _ => client.Toast("Lead management coming soon!")))
                    .Add(new Button("Revenue Reports", _ => client.Toast("Revenue analytics coming soon!")))
                )
        );
    }
}
