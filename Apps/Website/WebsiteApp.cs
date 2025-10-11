namespace IvyOneBusinessOnePlatform.Apps.Website;

[App(icon: Icons.Globe, title: "Website", path: new[] { "Marketing & Communication" })]
public class WebsiteApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Website Builder")
                .Add("Create and manage your website")
                .Add(Layout.Grid()
                    .Columns(3)
                    .Gap(12)
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Pages").Add("12")))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Visitors Today").Add("1,234")))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Status").Add("Published"))))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Edit Pages", _ => client.Toast("Opening page editor"))
                        .Variant(ButtonVariant.Primary)
                        .Icon(Icons.Pen))
                    .Add(new Button("Add Page", _ => client.Toast("Creating new page"))
                        .Variant(ButtonVariant.Secondary)
                        .Icon(Icons.Plus))
                    .Add(new Button("View Live Site", _ => client.Toast("Opening website"))
                        .Variant(ButtonVariant.Outline)
                        .Icon(Icons.ExternalLink)))
                .Add(new Card(
                    Layout.Vertical()
                        .Gap(12)
                        .Padding(16)
                        .Add("Recent Pages")
                        .Add(BuildPagesList(client))))
        );
    }

    private object BuildPagesList(IClientProvider client)
    {
        var pages = new[] { "Home", "About Us", "Services", "Contact" };
        var pageCards = pages.Select(page => new Card(
            Layout.Horizontal()
                .Gap(12)
                .Padding(12)
                .Add(new Icon(Icons.FileText))
                .Add(page)
                .Add(new Badge("Published").Variant(BadgeVariant.Success))
                .Add(new Button("Edit", _ => client.Toast($"Editing: {page}"))
                    .Small()
                    .Variant(ButtonVariant.Outline))
        ));
        
        return Layout.Vertical().Gap(8).Add(pageCards);
    }
}
