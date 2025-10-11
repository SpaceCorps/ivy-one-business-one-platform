namespace IvyOneBusinessOnePlatform.Apps.Discuss;

[App(icon: Icons.MessageCircle, title: "Discuss", path: new[] { "Customer Service" })]
public class DiscussApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Discussion Forum")
                .Add("Collaborate and discuss topics with your team")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("View Topics", _ => client.Toast("Discussion topics coming soon!")))
                    .Add(new Button("Start Discussion", _ => client.Toast("New discussion coming soon!")))
                    .Add(new Button("Team Chat", _ => client.Toast("Team chat coming soon!")))
                )
        );
    }
}
