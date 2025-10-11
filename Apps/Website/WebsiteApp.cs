namespace IvyOneBusinessOnePlatform.Apps.Website;

[App(icon: Icons.Globe, title: "Website", path: new[] { "Marketing & Communication" })]
public class WebsiteApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new WebsiteRootBlade(), "Website", Size.Units(80));
    }
}

public class WebsiteRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var blades = this.UseContext<IBladeController>();
        
        var pages = new[]
        {
            new { Name = "Home", Status = "Published", Views = 1234 },
            new { Name = "About Us", Status = "Published", Views = 567 },
            new { Name = "Services", Status = "Published", Views = 890 },
            new { Name = "Contact", Status = "Draft", Views = 0 }
        };
        
        var listItems = pages.Select(page => new ListItem(
            title: page.Name,
            subtitle: $"{page.Status} - {page.Views} views",
            icon: Icons.FileText,
            badge: page.Status,
            onClick: _ => { blades.Push(this, new PageDetailBlade(page.Name, page.Status, page.Views), page.Name); return default; }
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(Text.H3("Website Pages"))
                .Add(new Button("Add Page", _ => client.Toast("Create page"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)))
            .Add(new List(listItems));
    }
}

public class PageDetailBlade(string name, string status, int views) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(name)
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add(new Badge(status).Variant(status == "Published" ? BadgeVariant.Success : BadgeVariant.Secondary))
                    .Add($"Views: {views}"))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Edit", _ => client.Toast("Edit page"))
                        .Variant(ButtonVariant.Primary))
                    .Add(new Button("Publish", _ => client.Toast("Publish page"))
                        .Variant(ButtonVariant.Success)))
        );
    }
}
