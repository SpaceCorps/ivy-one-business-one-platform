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
                .Add("Manage social media campaigns and engagement")
                .Add(Layout.Grid()
                    .Columns(4)
                    .Gap(12)
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Followers").Add("12.5K")))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Engagement").Add("8.2%")))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Posts").Add("234")))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Reach").Add("45.3K"))))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Create Post", _ => client.Toast("Creating social post"))
                        .Variant(ButtonVariant.Primary)
                        .Icon(Icons.Plus))
                    .Add(new Button("Schedule", _ => client.Toast("Schedule posts"))
                        .Variant(ButtonVariant.Secondary))
                    .Add(new Button("Analytics", _ => client.Toast("View analytics"))
                        .Variant(ButtonVariant.Outline)))
                .Add(new Card(
                    Layout.Vertical()
                        .Gap(12)
                        .Padding(16)
                        .Add("Connected Accounts")
                        .Add(Layout.Grid()
                            .Columns(3)
                            .Gap(12)
                            .Add(new Card(Layout.Vertical().Gap(8).Padding(12)
                                .Add(new Badge("Facebook").Variant(BadgeVariant.Primary))
                                .Add("Connected")))
                            .Add(new Card(Layout.Vertical().Gap(8).Padding(12)
                                .Add(new Badge("Twitter").Variant(BadgeVariant.Info))
                                .Add("Connected")))
                            .Add(new Card(Layout.Vertical().Gap(8).Padding(12)
                                .Add(new Badge("Instagram").Variant(BadgeVariant.Warning))
                                .Add("Not Connected"))))))
        );
    }
}
