namespace IvyOneBusinessOnePlatform.Apps.Project;

[App(icon: Icons.Check, title: "Project", path: new[] { "Project & Time Management" })]
public class ProjectApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Project Management")
                .Add("Plan, track, and manage your projects and tasks")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("View Projects", _ => client.Toast("Project list coming soon!")))
                    .Add(new Button("Create Project", _ => client.Toast("Project creation coming soon!")))
                    .Add(new Button("Task Board", _ => client.Toast("Task management coming soon!")))
                )
        );
    }
}
