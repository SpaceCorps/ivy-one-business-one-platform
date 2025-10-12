using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Helpdesk;

[App(icon: Icons.Info, title: "Helpdesk", path: new[] { "Customer Service" })]
public class HelpdeskApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new HelpdeskRootBlade(), "Helpdesk", Size.Units(100));
    }
}

public class HelpdeskRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var searchQuery = this.UseState("");
        var isNewTicketOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        var query = context.Tickets.AsQueryable();
        
        if (!string.IsNullOrEmpty(searchQuery.Value))
        {
            var searchPattern = $"%{searchQuery.Value}%";
            query = query.Where(t => 
                EF.Functions.Like(t.TicketNumber, searchPattern) ||
                EF.Functions.Like(t.CustomerName, searchPattern) ||
                EF.Functions.Like(t.Subject, searchPattern) ||
                t.Status.ToString().Contains(searchQuery.Value, StringComparison.OrdinalIgnoreCase) ||
                t.Priority.ToString().Contains(searchQuery.Value, StringComparison.OrdinalIgnoreCase));
        }
        
        var tickets = query.OrderByDescending(t => t.CreatedAt).ToList();
        
        var listItems = tickets.Select(ticket => new ListItem(
            title: $"{ticket.TicketNumber} - {ticket.Subject}",
            subtitle: $"{ticket.CustomerName} - {ticket.Priority} priority",
            icon: Icons.Info,
            badge: ticket.Status.ToString(),
            onClick: _ => blades.Push(this, new TicketDetailBlade(ticket.Id, () => refreshToken.Refresh()), ticket.TicketNumber)
        ));
        
        var mainContent = BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(4)
                .Add(searchQuery.ToSearchInput().Placeholder("Search by ticket number, customer, subject, status, or priority..."))
                .Add(new Button("New Ticket")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => isNewTicketOpen.Set(true))),
            tickets.Count == 0 
                ? Text.Block("No tickets found. Try a different search or create your first ticket!")
                : new List(listItems)
        );

        return isNewTicketOpen.Value ? new Sheet(
            (Event<Sheet> _) => isNewTicketOpen.Set(false),
            new TicketFormSheet(null, () => {
                isNewTicketOpen.Set(false);
                refreshToken.Refresh();
            }),
            title: "New Ticket",
            description: "Create a new support ticket"
        ).Width(Size.Fraction(1/3f)) : mainContent;
    }
}

public class TicketDetailBlade(int ticketId, Action? onRefresh = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var initialTicket = context.Tickets.FirstOrDefault(t => t.Id == ticketId);
        
        if (initialTicket == null)
        {
            return Layout.Vertical()
                .Gap(4)
                .Add(Text.H3("Ticket Not Found"))
                .Add(new Button("Go Back", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary));
        }
        
        var ticketData = this.UseState(initialTicket);
        var isEditOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        this.UseEffect(() =>
        {
            var updatedTicket = context.Tickets.FirstOrDefault(t => t.Id == ticketId);
            if (updatedTicket != null)
            {
                ticketData.Set(updatedTicket);
            }
        }, [refreshToken.ToTrigger()]);
        
        var statusBadge = new Badge(ticketData.Value.Status.ToString())
            .Variant(ticketData.Value.Status == TicketStatus.Resolved || ticketData.Value.Status == TicketStatus.Closed ? BadgeVariant.Success :
                   ticketData.Value.Status == TicketStatus.InProgress ? BadgeVariant.Primary :
                   BadgeVariant.Secondary);
        
        var priorityBadge = new Badge(ticketData.Value.Priority.ToString())
            .Variant(ticketData.Value.Priority == TicketPriority.Critical || ticketData.Value.Priority == TicketPriority.High ? BadgeVariant.Destructive :
                   ticketData.Value.Priority == TicketPriority.Medium ? BadgeVariant.Warning :
                   BadgeVariant.Outline);

        var ticketDetails = new
        {
            TicketNumber = ticketData.Value.TicketNumber,
            CustomerName = ticketData.Value.CustomerName,
            CustomerEmail = ticketData.Value.CustomerEmail,
            Subject = ticketData.Value.Subject,
            Description = ticketData.Value.Description,
            Category = ticketData.Value.Category,
            AssignedTo = ticketData.Value.AssignedTo,
            Status = statusBadge,
            Priority = priorityBadge,
            CreatedAt = ticketData.Value.CreatedAt.ToString("MMM dd, yyyy HH:mm"),
            ResolvedAt = ticketData.Value.ResolvedAt?.ToString("MMM dd, yyyy HH:mm")
        };
        
        return Layout.Vertical()
            .Gap(4)
            .Add(Text.H3($"{ticketData.Value.TicketNumber} - {ticketData.Value.Subject}"))
            .Add(ticketDetails.ToDetails().RemoveEmpty().MultiLine(x => x.Description))
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(ticketData.Value.Status != TicketStatus.Resolved && ticketData.Value.Status != TicketStatus.Closed ? new Button("Resolve Ticket", _ => {
                    var dbTicket = context.Tickets.FirstOrDefault(t => t.Id == ticketId);
                    if (dbTicket != null)
                    {
                        dbTicket.Status = TicketStatus.Resolved;
                        dbTicket.ResolvedAt = DateTime.UtcNow;
                        dbTicket.UpdatedAt = DateTime.UtcNow;
                        context.SaveChanges();
                        client.Toast($"Ticket {ticketData.Value.TicketNumber} resolved!");
                        refreshToken.Refresh();
                        onRefresh?.Invoke();
                    }
                })
                    .Variant(ButtonVariant.Success)
                    .Icon(Icons.Check)
                    : null)
                .Add(ticketData.Value.Status == TicketStatus.Resolved ? new Button("Close Ticket", _ => {
                    var dbTicket = context.Tickets.FirstOrDefault(t => t.Id == ticketId);
                    if (dbTicket != null)
                    {
                        dbTicket.Status = TicketStatus.Closed;
                        dbTicket.UpdatedAt = DateTime.UtcNow;
                        context.SaveChanges();
                        client.Toast($"Ticket {ticketData.Value.TicketNumber} closed!");
                        refreshToken.Refresh();
                        onRefresh?.Invoke();
                    }
                })
                    .Variant(ButtonVariant.Primary)
                    .Icon(Icons.Lock)
                    : null)
                .Add(new Button("Edit Ticket")
                    .Variant(ButtonVariant.Outline)
                    .Icon(Icons.Pencil)
                    .HandleClick(_ => isEditOpen.Set(true)))
                .Add(new Button("Delete Ticket")
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.Trash)
                    .HandleClick(_ => {
                        try
                        {
                            var ticketToDelete = context.Tickets.FirstOrDefault(t => t.Id == ticketId);
                            if (ticketToDelete != null)
                            {
                                context.Tickets.Remove(ticketToDelete);
                                context.SaveChanges();
                                client.Toast($"Ticket {ticketData.Value.TicketNumber} deleted successfully!");
                                refreshToken.Refresh();
                                onRefresh?.Invoke();
                                blades.Pop();
                            }
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error deleting ticket: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary)))
            .Add(isEditOpen.Value ? new Sheet(
                (Event<Sheet> _) => isEditOpen.Set(false),
                new TicketFormSheet(ticketId, () => {
                    isEditOpen.Set(false);
                    refreshToken.Refresh();
                    onRefresh?.Invoke();
                }),
                title: "Edit Ticket",
                description: $"Edit ticket {ticketData.Value.TicketNumber}"
            ).Width(Size.Fraction(1/3f)) : null);
    }
}

public class TicketFormSheet(int? ticketId = null, Action? onClose = null) : ViewBase
{
    private Ticket CloneTicket(Ticket source) => new Ticket
    {
        Id = source.Id,
        TicketNumber = source.TicketNumber,
        CustomerName = source.CustomerName,
        CustomerEmail = source.CustomerEmail,
        Subject = source.Subject,
        Description = source.Description,
        Status = source.Status,
        Priority = source.Priority,
        Category = source.Category,
        AssignedTo = source.AssignedTo,
        CreatedAt = source.CreatedAt,
        UpdatedAt = source.UpdatedAt,
        ResolvedAt = source.ResolvedAt
    };
    
    private string? ValidateTicket(Ticket ticket, IClientProvider client)
    {
        if (string.IsNullOrWhiteSpace(ticket.TicketNumber))
            return "Ticket Number is required";
        
        if (string.IsNullOrWhiteSpace(ticket.CustomerName))
            return "Customer Name is required";
        
        if (string.IsNullOrWhiteSpace(ticket.Subject))
            return "Subject is required";
        
        if (string.IsNullOrWhiteSpace(ticket.Description))
            return "Description is required";
        
        return null;
    }
    
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var isEdit = ticketId.HasValue;
        var existingTicket = isEdit ? context.Tickets.FirstOrDefault(t => t.Id == ticketId!.Value) : null;
        
        var ticketForm = this.UseState(existingTicket ?? new Ticket
        {
            TicketNumber = $"HD-{DateTime.UtcNow:yyyyMMdd-HHmmss}",
            CustomerName = "",
            CustomerEmail = "",
            Subject = "",
            Description = "",
            Status = TicketStatus.Open,
            Priority = TicketPriority.Medium,
            Category = "",
            AssignedTo = ""
        });
        
        var statusOptions = typeof(TicketStatus).ToOptions();
        var priorityOptions = typeof(TicketPriority).ToOptions();
        var categoryOptions = new[] { "Technical", "Feature", "Bug", "Account", "General", "Other" };
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => {
                        try
                        {
                            var validationError = ValidateTicket(ticketForm.Value, client);
                            if (validationError != null)
                            {
                                client.Toast(validationError, "Validation Error");
                                return;
                            }
                            
                            if (isEdit && existingTicket != null)
                            {
                                existingTicket.TicketNumber = ticketForm.Value.TicketNumber;
                                existingTicket.CustomerName = ticketForm.Value.CustomerName;
                                existingTicket.CustomerEmail = ticketForm.Value.CustomerEmail;
                                existingTicket.Subject = ticketForm.Value.Subject;
                                existingTicket.Description = ticketForm.Value.Description;
                                existingTicket.Status = ticketForm.Value.Status;
                                existingTicket.Priority = ticketForm.Value.Priority;
                                existingTicket.Category = ticketForm.Value.Category;
                                existingTicket.AssignedTo = ticketForm.Value.AssignedTo;
                                existingTicket.UpdatedAt = DateTime.UtcNow;
                            }
                            else
                            {
                                var newTicket = new Ticket
                                {
                                    TicketNumber = ticketForm.Value.TicketNumber,
                                    CustomerName = ticketForm.Value.CustomerName,
                                    CustomerEmail = ticketForm.Value.CustomerEmail,
                                    Subject = ticketForm.Value.Subject,
                                    Description = ticketForm.Value.Description,
                                    Status = ticketForm.Value.Status,
                                    Priority = ticketForm.Value.Priority,
                                    Category = ticketForm.Value.Category,
                                    AssignedTo = ticketForm.Value.AssignedTo,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                context.Tickets.Add(newTicket);
                            }
                            
                            context.SaveChanges();
                            client.Toast(isEdit ? "Ticket updated successfully!" : "Ticket created successfully!");
                            onClose?.Invoke();
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel")
                    .Variant(ButtonVariant.Outline)
                    .HandleClick(_ => onClose?.Invoke())),
            
            Layout.Vertical().Gap(4)
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Ticket Information"))
                        .Add(Text.Small("Ticket Number"))
                        .Add(new TextInput(ticketForm.Value.TicketNumber, isEdit ? null : (e => {
                            var cloned = CloneTicket(ticketForm.Value);
                            cloned.TicketNumber = e.Value;
                            ticketForm.Set(cloned);
                        })).Placeholder("HD-001").Disabled(isEdit))
                        .Add(Text.Small("Subject"))
                        .Add(new TextInput(ticketForm.Value.Subject, async e => {
                            var cloned = CloneTicket(ticketForm.Value);
                            cloned.Subject = e.Value;
                            ticketForm.Set(cloned);
                        }).Placeholder("Issue subject"))
                        .Add(Text.Small("Description"))
                        .Add(new TextInput(ticketForm.Value.Description, async e => {
                            var cloned = CloneTicket(ticketForm.Value);
                            cloned.Description = e.Value;
                            ticketForm.Set(cloned);
                        }).Placeholder("Detailed description...").Variant(TextInputs.Textarea))
                        .Add(Text.Small("Category"))
                        .Add(new SelectInput<string>(ticketForm.Value.Category, async e => {
                            var cloned = CloneTicket(ticketForm.Value);
                            cloned.Category = e.Value;
                            ticketForm.Set(cloned);
                        }, categoryOptions.ToOptions()))
                ).Title("Ticket Details"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Customer Information"))
                        .Add(Text.Small("Customer Name"))
                        .Add(new TextInput(ticketForm.Value.CustomerName, async e => {
                            var cloned = CloneTicket(ticketForm.Value);
                            cloned.CustomerName = e.Value;
                            ticketForm.Set(cloned);
                        }).Placeholder("John Doe"))
                        .Add(Text.Small("Customer Email"))
                        .Add(new TextInput(ticketForm.Value.CustomerEmail, async e => {
                            var cloned = CloneTicket(ticketForm.Value);
                            cloned.CustomerEmail = e.Value;
                            ticketForm.Set(cloned);
                        }).Placeholder("customer@example.com"))
                ).Title("Customer"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Ticket Management"))
                        .Add(Text.Small("Status"))
                        .Add(new SelectInput<TicketStatus>(ticketForm.Value.Status, async e => {
                            var cloned = CloneTicket(ticketForm.Value);
                            cloned.Status = e.Value;
                            ticketForm.Set(cloned);
                        }, statusOptions))
                        .Add(Text.Small("Priority"))
                        .Add(new SelectInput<TicketPriority>(ticketForm.Value.Priority, async e => {
                            var cloned = CloneTicket(ticketForm.Value);
                            cloned.Priority = e.Value;
                            ticketForm.Set(cloned);
                        }, priorityOptions))
                        .Add(Text.Small("Assigned To"))
                        .Add(new TextInput(ticketForm.Value.AssignedTo, async e => {
                            var cloned = CloneTicket(ticketForm.Value);
                            cloned.AssignedTo = e.Value;
                            ticketForm.Set(cloned);
                        }).Placeholder("Support Agent"))
                ).Title("Management"))
        );
    }
}
