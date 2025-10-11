using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Manufacturing;

[App(icon: Icons.Settings, title: "Manufacturing", path: new[] { "Business Operations" })]
public class ManufacturingApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new ManufacturingRootBlade(), "Manufacturing", Size.Units(110));
    }
}

public class ManufacturingRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var searchTerm = this.UseState("");
        
        var workOrders = context.WorkOrders
            .Include(wo => wo.Product)
            .OrderByDescending(wo => wo.CreatedAt)
            .ToList();
        
        var filteredWorkOrders = workOrders
            .Where(wo => string.IsNullOrEmpty(searchTerm.Value) ||
                        wo.WorkOrderNumber.Contains(searchTerm.Value, StringComparison.OrdinalIgnoreCase) ||
                        wo.ProductName.Contains(searchTerm.Value, StringComparison.OrdinalIgnoreCase) ||
                        wo.Status.Contains(searchTerm.Value, StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        var listItems = filteredWorkOrders.Select(wo => new ListItem(
            title: $"{wo.WorkOrderNumber} - {wo.ProductName}",
            subtitle: $"Qty: {wo.Quantity} - {wo.Progress}% complete",
            icon: Icons.Settings,
            badge: wo.Status,
            onClick: _ => { blades.Push(this, new WorkOrderDetailBlade(wo.Id), wo.WorkOrderNumber); return default; }
        ));
        
        return BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(8)
                .Add(searchTerm.ToTextInput().Placeholder("Search work orders..."))
                .Add(new Button("New Work Order")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .WithSheet(
                        () => new CreateWorkOrderSheet(context, client),
                        title: "Create Work Order",
                        description: "Fill in the details to create a new manufacturing work order",
                        width: Size.Fraction(1/3f)
                    )),
            filteredWorkOrders.Count == 0 
                ? Text.Block("No work orders found. Try adjusting your search or create a new work order!")
                : new List(listItems)
        );
    }
}

public class CreateWorkOrderSheet : ViewBase
{
    private readonly ApplicationDbContext _context;
    private readonly IClientProvider _client;

    public CreateWorkOrderSheet(ApplicationDbContext context, IClientProvider client)
    {
        _context = context;
        _client = client;
    }

    public override object? Build()
    {
        var workOrderNumber = this.UseState($"WO-{DateTime.UtcNow:yyyyMMddHHmmss}");
        var productName = this.UseState("");
        var quantity = this.UseState("0");
        var priority = this.UseState("Normal");
        var plannedStartDate = this.UseState(DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"));
        var plannedEndDate = this.UseState(DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd"));
        var notes = this.UseState("");

        var priorities = new[] { "Low", "Normal", "High", "Urgent" };

        return new FooterLayout(
            Layout.Horizontal().Gap(8)
                .Add(new Button("Create", _ => {
                    if (string.IsNullOrWhiteSpace(productName.Value))
                    {
                        _client.Toast("Please enter product name");
                        return default;
                    }

                    if (!int.TryParse(quantity.Value, out var qty) || qty <= 0)
                    {
                        _client.Toast("Please enter a valid quantity");
                        return default;
                    }

                    var newWorkOrder = new WorkOrder
                    {
                        WorkOrderNumber = workOrderNumber.Value,
                        ProductName = productName.Value,
                        Quantity = qty,
                        Status = "Scheduled",
                        Progress = 0,
                        Priority = priority.Value,
                        PlannedStartDate = DateTime.Parse(plannedStartDate.Value),
                        PlannedEndDate = DateTime.Parse(plannedEndDate.Value),
                        Notes = string.IsNullOrWhiteSpace(notes.Value) ? null : notes.Value,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.WorkOrders.Add(newWorkOrder);
                    _context.SaveChanges();
                    
                    _client.Toast($"Work order {workOrderNumber.Value} created successfully!");
                    return default;
                })
                .Variant(ButtonVariant.Primary))
                .Add(new Button("Cancel")
                    .Variant(ButtonVariant.Outline)),
            Layout.Vertical()
                .Gap(16)
                .Add(new Card(
                    Layout.Vertical()
                        .Gap(12)
                        .Add(Text.Small("Work Order Number"))
                        .Add(workOrderNumber.ToTextInput().Placeholder("WO-001").Disabled(true))
                        .Add(Text.Small("Product Name *"))
                        .Add(productName.ToTextInput().Placeholder("Enter product name"))
                        .Add(Text.Small("Quantity *"))
                        .Add(quantity.ToTextInput().Placeholder("Enter quantity"))
                        .Add(Text.Small("Priority"))
                        .Add(priority.ToSelectInput(priorities.Select(p => new Option<string>(p, p)).ToArray()))
                ).Title("Basic Information"))
                .Add(new Card(
                    Layout.Vertical()
                        .Gap(12)
                        .Add(Text.Small("Planned Start Date"))
                        .Add(plannedStartDate.ToDateInput().Placeholder("YYYY-MM-DD"))
                        .Add(Text.Small("Planned End Date"))
                        .Add(plannedEndDate.ToDateInput().Placeholder("YYYY-MM-DD"))
                ).Title("Schedule"))
                .Add(new Card(
                    Layout.Vertical()
                        .Gap(12)
                        .Add(Text.Small("Notes (Optional)"))
                        .Add(notes.ToTextInput().Placeholder("Add any additional notes..."))
                ).Title("Additional Information"))
        );
    }
}

public class WorkOrderDetailBlade(int workOrderId) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var workOrder = context.WorkOrders
            .Include(wo => wo.Product)
            .FirstOrDefault(wo => wo.Id == workOrderId);
        
        if (workOrder == null)
            return "Work order not found";
        
        return Layout.Vertical()
            .Gap(16)
            .Add(new Card(
                Layout.Vertical()
                    .Gap(12)
                    .Add(Layout.Horizontal()
                        .Gap(8)
                        .Add(new Badge(workOrder.Status)
                            .Variant(workOrder.Status == "Completed" ? BadgeVariant.Success :
                                   workOrder.Status == "In Production" ? BadgeVariant.Primary :
                                   workOrder.Status == "Cancelled" ? BadgeVariant.Destructive :
                                   BadgeVariant.Secondary))
                        .Add(new Badge(workOrder.Priority)
                            .Variant(workOrder.Priority == "Urgent" ? BadgeVariant.Destructive :
                                   workOrder.Priority == "High" ? BadgeVariant.Warning :
                                   BadgeVariant.Outline)))
                    .Add(new Progress(workOrder.Progress))
            ).Title(workOrder.WorkOrderNumber))
            .Add(new Card(
                new {
                    WorkOrderNumber = workOrder.WorkOrderNumber,
                    ProductName = workOrder.ProductName,
                    Quantity = workOrder.Quantity,
                    Priority = workOrder.Priority,
                    Status = workOrder.Status,
                    Progress = $"{workOrder.Progress}%",
                    Created = workOrder.CreatedAt.ToString("MMM dd, yyyy"),
                    PlannedStart = workOrder.PlannedStartDate.ToString("MMM dd, yyyy"),
                    PlannedEnd = workOrder.PlannedEndDate.ToString("MMM dd, yyyy"),
                    ActualStart = workOrder.StartDate?.ToString("MMM dd, yyyy"),
                    CompletionDate = workOrder.CompletionDate?.ToString("MMM dd, yyyy"),
                    Notes = workOrder.Notes
                }
                .ToDetails()
                .Remove(x => x.WorkOrderNumber)
                .RemoveEmpty()
                .MultiLine(x => x.Notes)
                .Builder(x => x.WorkOrderNumber, b => b.CopyToClipboard())
            ).Title("Work Order Details"))
            .Add(new Card(
                Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Update Progress", _ => client.Toast("Update progress"))
                        .Variant(ButtonVariant.Primary))
                    .Add(workOrder.Status != "Completed" 
                        ? new Button("Complete", _ => {
                            workOrder.Status = "Completed";
                            workOrder.Progress = 100;
                            workOrder.CompletionDate = DateTime.UtcNow;
                            context.SaveChanges();
                            client.Toast("Work order completed");
                            return default;
                        })
                            .Variant(ButtonVariant.Success)
                        : null)
                    .Add(workOrder.Status == "Scheduled" 
                        ? new Button("Start Production", _ => {
                            workOrder.Status = "In Production";
                            workOrder.StartDate = DateTime.UtcNow;
                            context.SaveChanges();
                            client.Toast("Production started");
                            return default;
                        })
                            .Variant(ButtonVariant.Primary)
                        : null)
            ).Title("Actions"));
    }
}
