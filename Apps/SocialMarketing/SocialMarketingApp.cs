namespace IvyOneBusinessOnePlatform.Apps.SocialMarketing;

[App(icon: Icons.Heart, title: "Social Marketing", path: new[] { "Marketing & Communication" })]
public class SocialMarketingApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Social Media Marketing")
                .Add("Manage social media campaigns and content across platforms")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Campaigns", _ => client.Toast("Campaign management coming soon!")))
                    .Add(new Button("Content Calendar", _ => client.Toast("Content planning coming soon!")))
                    .Add(new Button("Analytics", _ => client.Toast("Social analytics coming soon!")))
                )
        );
    }
}
