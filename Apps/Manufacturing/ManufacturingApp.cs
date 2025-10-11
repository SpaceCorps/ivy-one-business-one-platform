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
        
        var workOrders = context.WorkOrders
            .Include(wo => wo.Product)
            .OrderByDescending(wo => wo.CreatedAt)
            .ToList();
        
        var listItems = workOrders.Select(wo => new ListItem(
            title: $"{wo.WorkOrderNumber} - {wo.ProductName}",
            subtitle: $"Qty: {wo.Quantity} - {wo.Progress}% complete - Priority: {wo.Priority}",
            icon: Icons.Settings,
            badge: wo.Status,
            onClick: _ => { blades.Push(this, new WorkOrderDetailBlade(wo.Id), wo.WorkOrderNumber); return default; }
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(Text.H3("Manufacturing Orders"))
                .Add(new Button("New Work Order", _ => client.Toast("Create work order"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)))
            .Add(workOrders.Count == 0 
                ? Text.Block("No work orders yet. Create your first work order!")
                : new List(listItems));
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
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(Text.H4(workOrder.WorkOrderNumber))
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add($"Product: {workOrder.ProductName}")
                    .Add($"Quantity: {workOrder.Quantity}")
                    .Add($"Priority: {workOrder.Priority}")
                    .Add($"Created: {workOrder.CreatedAt:MMM dd, yyyy}")
                    .Add($"Planned Start: {workOrder.PlannedStartDate:MMM dd, yyyy}")
                    .Add($"Planned End: {workOrder.PlannedEndDate:MMM dd, yyyy}")
                    .Add(workOrder.StartDate.HasValue ? $"Actual Start: {workOrder.StartDate.Value:MMM dd, yyyy}" : "Not started yet")
                    .Add(workOrder.CompletionDate.HasValue ? $"Completed: {workOrder.CompletionDate.Value:MMM dd, yyyy}" : null)
                    .Add(new Badge(workOrder.Status)
                        .Variant(workOrder.Status == "Completed" ? BadgeVariant.Success :
                               workOrder.Status == "In Production" ? BadgeVariant.Primary :
                               workOrder.Status == "Cancelled" ? BadgeVariant.Destructive :
                               BadgeVariant.Secondary))
                    .Add(new Badge(workOrder.Priority)
                        .Variant(workOrder.Priority == "Urgent" ? BadgeVariant.Destructive :
                               workOrder.Priority == "High" ? BadgeVariant.Warning :
                               BadgeVariant.Outline))
                    .Add(new Progress(workOrder.Progress))
                    .Add(workOrder.Notes != null ? $"Notes: {workOrder.Notes}" : null))
                .Add(Layout.Horizontal()
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
                        : null))
        );
    }
}
