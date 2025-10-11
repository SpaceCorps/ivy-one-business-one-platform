namespace IvyOneBusinessOnePlatform.Apps.Studio;

[App(icon: Icons.Wrench, title: "Studio")]
public class StudioApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Development Studio")
                .Add("Build and customize applications with visual tools")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Create App", _ => client.Toast("App creation coming soon!")))
                    .Add(new Button("Design Interface", _ => client.Toast("Interface design coming soon!")))
                    .Add(new Button("Manage Projects", _ => client.Toast("Project management coming soon!")))
                )
        );
    }
}
