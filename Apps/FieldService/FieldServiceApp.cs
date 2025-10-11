namespace IvyOneBusinessOnePlatform.Apps.FieldService;

[App(icon: Icons.Zap, title: "Field Service", path: new[] { "Project & Time Management" })]
public class FieldServiceApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new FieldServiceRootBlade(), "Field Service", Size.Units(80));
    }
}

public class FieldServiceRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var blades = this.UseContext<IBladeController>();
        
        var tickets = new[]
        {
            new { Id = "FS-001", Customer = "ABC Corp", Location = "123 Main St", Status = "In Progress", Priority = "High" },
            new { Id = "FS-002", Customer = "XYZ Ltd", Location = "456 Oak Ave", Status = "Scheduled", Priority = "Medium" },
            new { Id = "FS-003", Customer = "Tech Inc", Location = "789 Elm Rd", Status = "Completed", Priority = "Low" }
        };
        
        var listItems = tickets.Select(ticket => new ListItem(
            title: $"{ticket.Id} - {ticket.Customer}",
            subtitle: $"{ticket.Location} - {ticket.Priority} priority",
            icon: Icons.Wrench,
            badge: ticket.Status,
            onClick: _ => { blades.Push(this, new TicketDetailBlade(ticket.Id, ticket.Customer, ticket.Location, ticket.Status, ticket.Priority), ticket.Id); }
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(Text.H3("Field Service Tickets"))
                .Add(new Button("New Ticket", _ => client.Toast("Create ticket"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)))
            .Add(new List(listItems));
    }
}

public class TicketDetailBlade(string id, string customer, string location, string status, string priority) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(id)
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add($"Customer: {customer}")
                    .Add($"Location: {location}")
                    .Add(new Badge(status)
                        .Variant(status == "Completed" ? BadgeVariant.Success :
                               status == "In Progress" ? BadgeVariant.Primary :
                               BadgeVariant.Secondary))
                    .Add(new Badge(priority)
                        .Variant(priority == "High" ? BadgeVariant.Destructive :
                               priority == "Medium" ? BadgeVariant.Warning :
                               BadgeVariant.Outline)))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Assign", _ => client.Toast("Assign technician"))
                        .Variant(ButtonVariant.Primary))
                    .Add(status != "Completed" 
                        ? new Button("Complete", _ => client.Toast("Mark complete"))
                            .Variant(ButtonVariant.Success)
                        : null))
        );
    }
}
