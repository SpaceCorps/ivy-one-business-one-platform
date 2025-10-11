namespace IvyOneBusinessOnePlatform.Apps.SocialMarketing;

[App(icon: Icons.Heart, title: "Social Marketing", path: new[] { "Marketing & Communication" })]
public class SocialMarketingApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new SocialMarketingRootBlade(), "Social Marketing");
    }
}

public class SocialMarketingRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        var platforms = new[]
        {
            new { Name = "Facebook", Status = "Connected", Followers = "12.5K", Engagement = "8.2%" },
            new { Name = "Twitter", Status = "Connected", Followers = "8.3K", Engagement = "6.1%" },
            new { Name = "Instagram", Status = "Not Connected", Followers = "0", Engagement = "0%" }
        };
        
        var listItems = platforms.Select(platform => new ListItem(
            title: platform.Name,
            subtitle: $"{platform.Status} - {platform.Followers} followers - {platform.Engagement} engagement",
            icon: Icons.Heart,
            badge: platform.Status,
            onClick: _ => { client.Toast($"{platform.Name} management"); return default; }
        ));
        
        return BladeHelper.WithHeader(
            new Button("Create Post", _ => client.Toast("Create post"))
                .Icon(Icons.Plus)
                .Variant(ButtonVariant.Primary),
            new List(listItems)
        );
    }
}
