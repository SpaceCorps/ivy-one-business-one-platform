namespace IvyOneBusinessOnePlatform.Apps.CRM;

[App(icon: Icons.Users, title: "CRM", path: new[] { "Business Operations" })]
public class CRMApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Customer Relationship Management")
                .Add("Manage customer interactions, leads, and sales pipeline")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("View Contacts", _ => client.Toast("Contact management coming soon!")))
                    .Add(new Button("Sales Pipeline", _ => client.Toast("Sales pipeline coming soon!")))
                    .Add(new Button("Lead Management", _ => client.Toast("Lead management coming soon!")))
                )
        );
    }
}
