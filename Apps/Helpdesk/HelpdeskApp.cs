namespace IvyOneBusinessOnePlatform.Apps.Helpdesk;

[App(icon: Icons.Info, title: "Helpdesk", path: new[] { "Customer Service" })]
public class HelpdeskApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Helpdesk Support")
                .Add("Manage support tickets and customer service requests")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("View Tickets", _ => client.Toast("Ticket management coming soon!")))
                    .Add(new Button("Create Ticket", _ => client.Toast("Ticket creation coming soon!")))
                    .Add(new Button("Support Chat", _ => client.Toast("Live chat coming soon!")))
                )
        );
    }
}
