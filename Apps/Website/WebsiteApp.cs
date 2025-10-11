namespace IvyOneBusinessOnePlatform.Apps.Website;

[App(icon: Icons.Globe, title: "Website")]
public class WebsiteApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Website Management")
                .Add("Build and manage your website content and pages")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Page Builder", _ => client.Toast("Page builder coming soon!")))
                    .Add(new Button("Content Manager", _ => client.Toast("Content management coming soon!")))
                    .Add(new Button("SEO Tools", _ => client.Toast("SEO optimization coming soon!")))
                )
        );
    }
}
