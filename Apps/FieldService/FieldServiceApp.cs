namespace IvyOneBusinessOnePlatform.Apps.FieldService;

[App(icon: Icons.Zap, title: "Field Service", path: new[] { "Project & Time Management" })]
public class FieldServiceApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var selectedView = this.UseState("tickets");
        
        var tickets = new[]
        {
            new { Id = "FS-001", Customer = "ABC Corp", Location = "123 Main St", Status = "In Progress", Priority = "High" },
            new { Id = "FS-002", Customer = "XYZ Ltd", Location = "456 Oak Ave", Status = "Scheduled", Priority = "Medium" },
            new { Id = "FS-003", Customer = "Tech Inc", Location = "789 Elm Rd", Status = "Completed", Priority = "Low" }
        };
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Field Service Management")
                .Add("Manage service tickets and field technicians")
                .Add(Layout.Grid()
                    .Columns(3)
                    .Gap(12)
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Total Tickets").Add(tickets.Length.ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("In Progress").Add(tickets.Count(t => t.Status == "In Progress").ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Completed").Add(tickets.Count(t => t.Status == "Completed").ToString()))))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Tickets", _ => selectedView.Set("tickets"))
                        .Variant(selectedView.Value == "tickets" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("Technicians", _ => selectedView.Set("techs"))
                        .Variant(selectedView.Value == "techs" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("New Ticket", _ => client.Toast("Create service ticket"))
                        .Icon(Icons.Plus)
                        .Variant(ButtonVariant.Success)))
                .Add(BuildView(selectedView.Value, tickets, client))
        );
    }

    private object BuildView(string view, dynamic[] tickets, IClientProvider client)
    {
        if (view == "techs")
        {
            return new Card(Layout.Vertical().Padding(16).Add("Technician management coming soon!"));
        }
        
        var ticketCards = tickets.Select(ticket => new Card(
            Layout.Horizontal()
                .Gap(12)
                .Padding(16)
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add(ticket.Id)
                    .Add(ticket.Customer)
                    .Add(ticket.Location)
                    .Add(Layout.Horizontal()
                        .Gap(8)
                        .Add(new Badge(ticket.Status)
                            .Variant(ticket.Status == "Completed" ? BadgeVariant.Success :
                                   ticket.Status == "In Progress" ? BadgeVariant.Primary :
                                   BadgeVariant.Secondary))
                        .Add(new Badge(ticket.Priority)
                            .Variant(ticket.Priority == "High" ? BadgeVariant.Destructive :
                                   ticket.Priority == "Medium" ? BadgeVariant.Warning :
                                   BadgeVariant.Outline))))
                .Add(Layout.Horizontal()
                    .Gap(4)
                    .Add(new Button("View", _ => client.Toast($"Viewing: {ticket.Id}"))
                        .Small()
                        .Variant(ButtonVariant.Primary))
                    .Add(new Button("Assign", _ => client.Toast($"Assign technician to: {ticket.Id}"))
                        .Small()
                        .Variant(ButtonVariant.Outline)))
        ));
        
        return Layout.Vertical().Gap(8).Add(ticketCards);
    }
}
