namespace IvyOneBusinessOnePlatform.Apps.FieldService;

[App(icon: Icons.Zap, title: "Field Service", path: new[] { "Project & Time Management" })]
public class FieldServiceApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Field Service Management")
                .Add("Manage field technicians, service calls, and maintenance schedules")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Service Requests", _ => client.Toast("Service requests coming soon!")))
                    .Add(new Button("Technician Schedule", _ => client.Toast("Scheduling coming soon!")))
                    .Add(new Button("Maintenance Logs", _ => client.Toast("Maintenance tracking coming soon!")))
                )
        );
    }
}
