namespace IvyOneBusinessOnePlatform.Apps.Planning;

[App(icon: Icons.Calendar, title: "Planning")]
public class PlanningApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Planning & Scheduling")
                .Add("Plan events, meetings, and resource allocation")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Create Plan", _ => client.Toast("Planning tools coming soon!")))
                    .Add(new Button("Schedule Events", _ => client.Toast("Event scheduling coming soon!")))
                    .Add(new Button("Resource Calendar", _ => client.Toast("Resource management coming soon!")))
                )
        );
    }
}
