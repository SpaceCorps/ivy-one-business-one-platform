namespace IvyOneBusinessOnePlatform.Apps.Knowledge;

[App(icon: Icons.Bookmark, title: "Knowledge", path: new[] { "Customer Service" })]
public class KnowledgeApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Knowledge Base")
                .Add("Access documentation, FAQs, and support resources")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Browse Articles", _ => client.Toast("Knowledge articles coming soon!")))
                    .Add(new Button("Search Knowledge", _ => client.Toast("Search functionality coming soon!")))
                    .Add(new Button("Submit Question", _ => client.Toast("Question submission coming soon!")))
                )
        );
    }
}
