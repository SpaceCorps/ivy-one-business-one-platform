namespace IvyOneBusinessOnePlatform.Apps.Dashboard;

[App(icon: Icons.Grid3x3, title: "Dashboard")]
public class DashboardApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Custom Dashboard")
                .Add("Create personalized dashboards with widgets and analytics")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Create Dashboard", _ => client.Toast("Dashboard builder coming soon!")))
                    .Add(new Button("Add Widgets", _ => client.Toast("Widget library coming soon!")))
                    .Add(new Button("Analytics View", _ => client.Toast("Analytics dashboard coming soon!")))
                )
        );
    }
}
