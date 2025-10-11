namespace IvyOneBusinessOnePlatform.Apps.Helpdesk;

[App(icon: Icons.Info, title: "Helpdesk", path: new[] { "Customer Service" })]
public class HelpdeskApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new HelpdeskRootBlade(), "Helpdesk", Size.Units(80));
    }
}

public class HelpdeskRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var blades = this.UseContext<IBladeController>();
        
        var tickets = new[]
        {
            new { Id = "HD-001", Customer = "John Doe", Subject = "Login Issue", Status = "Open", Priority = "High" },
            new { Id = "HD-002", Customer = "Jane Smith", Subject = "Feature Request", Status = "In Progress", Priority = "Medium" },
            new { Id = "HD-003", Customer = "Bob Johnson", Subject = "Bug Report", Status = "Resolved", Priority = "Low" }
        };
        
        var listItems = tickets.Select(ticket => new ListItem(
            title: $"{ticket.Id} - {ticket.Subject}",
            subtitle: $"{ticket.Customer} - {ticket.Priority} priority",
            icon: Icons.Info,
            badge: ticket.Status,
            onClick: _ => { blades.Push(this, new TicketDetailBlade(ticket.Id, ticket.Customer, ticket.Subject, ticket.Status, ticket.Priority), ticket.Id); return default; }
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(Text.H3("Support Tickets"))
                .Add(new Button("New Ticket", _ => client.Toast("Create ticket"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)))
            .Add(new List(listItems));
    }
}

public class TicketDetailBlade(string id, string customer, string subject, string status, string priority) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add($"{id} - {subject}")
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add($"Customer: {customer}")
                    .Add(new Badge(status)
                        .Variant(status == "Resolved" ? BadgeVariant.Success :
                               status == "In Progress" ? BadgeVariant.Primary :
                               BadgeVariant.Secondary))
                    .Add(new Badge(priority)
                        .Variant(priority == "High" ? BadgeVariant.Destructive :
                               priority == "Medium" ? BadgeVariant.Warning :
                               BadgeVariant.Outline)))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Assign", _ => client.Toast("Assign agent"))
                        .Variant(ButtonVariant.Primary))
                    .Add(status != "Resolved" 
                        ? new Button("Resolve", _ => client.Toast("Mark resolved"))
                            .Variant(ButtonVariant.Success)
                        : null))
        );
    }
}
