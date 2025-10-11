namespace IvyOneBusinessOnePlatform.Apps.Helpdesk;

[App(icon: Icons.Info, title: "Helpdesk", path: new[] { "Customer Service" })]
public class HelpdeskApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var selectedView = this.UseState("open");
        
        var tickets = new[]
        {
            new { Id = "HD-001", Customer = "John Doe", Subject = "Login Issue", Status = "Open", Priority = "High", Created = DateTime.Now.AddHours(-2) },
            new { Id = "HD-002", Customer = "Jane Smith", Subject = "Feature Request", Status = "In Progress", Priority = "Medium", Created = DateTime.Now.AddHours(-5) },
            new { Id = "HD-003", Customer = "Bob Johnson", Subject = "Bug Report", Status = "Resolved", Priority = "Low", Created = DateTime.Now.AddDays(-1) }
        };
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Helpdesk Support")
                .Add("Manage customer support tickets and inquiries")
                .Add(Layout.Grid()
                    .Columns(4)
                    .Gap(12)
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Total").Add(tickets.Length.ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Open").Add(tickets.Count(t => t.Status == "Open").ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("In Progress").Add(tickets.Count(t => t.Status == "In Progress").ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Resolved").Add(tickets.Count(t => t.Status == "Resolved").ToString()))))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Open", _ => selectedView.Set("open"))
                        .Variant(selectedView.Value == "open" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("In Progress", _ => selectedView.Set("progress"))
                        .Variant(selectedView.Value == "progress" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("All", _ => selectedView.Set("all"))
                        .Variant(selectedView.Value == "all" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("New Ticket", _ => client.Toast("Create support ticket"))
                        .Icon(Icons.Plus)
                        .Variant(ButtonVariant.Success)))
                .Add(BuildTicketsList(tickets, selectedView.Value, client))
        );
    }

    private object BuildTicketsList(dynamic[] tickets, string filter, IClientProvider client)
    {
        var filtered = filter switch
        {
            "open" => tickets.Where(t => t.Status == "Open").ToArray(),
            "progress" => tickets.Where(t => t.Status == "In Progress").ToArray(),
            _ => tickets
        };
        
        if (filtered.Length == 0)
            return new Card(Layout.Vertical().Padding(16).Add("No tickets found in this category."));
        
        var ticketCards = filtered.Select(ticket => new Card(
            Layout.Horizontal()
                .Gap(12)
                .Padding(16)
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add($"{ticket.Id} - {ticket.Subject}")
                    .Add($"Customer: {ticket.Customer}")
                    .Add($"Created: {ticket.Created:MMM dd, HH:mm}")
                    .Add(Layout.Horizontal()
                        .Gap(8)
                        .Add(new Badge(ticket.Status)
                            .Variant(ticket.Status == "Resolved" ? BadgeVariant.Success :
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
                    .Add(new Button("Assign", _ => client.Toast($"Assign agent to: {ticket.Id}"))
                        .Small()
                        .Variant(ButtonVariant.Outline)))
        ));
        
        return Layout.Vertical().Gap(8).Add(ticketCards);
    }
}
