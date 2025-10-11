namespace IvyOneBusinessOnePlatform.Apps.Manufacturing;

[App(icon: Icons.Settings, title: "Manufacturing", path: new[] { "Business Operations" })]
public class ManufacturingApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new ManufacturingRootBlade(), "Manufacturing");
    }
}

public class ManufacturingRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var blades = this.UseContext<IBladeController>();
        
        var workOrders = new[]
        {
            new { Id = "WO-001", Product = "Widget A", Quantity = 500, Status = "In Production", Progress = 75 },
            new { Id = "WO-002", Product = "Component B", Quantity = 1000, Status = "Scheduled", Progress = 0 },
            new { Id = "WO-003", Product = "Assembly C", Quantity = 250, Status = "Completed", Progress = 100 }
        };
        
        var listItems = workOrders.Select(wo => new ListItem(
            title: $"{wo.Id} - {wo.Product}",
            subtitle: $"Qty: {wo.Quantity} - {wo.Progress}% complete",
            icon: Icons.Settings,
            badge: wo.Status,
            onClick: _ => { blades.Push(this, new WorkOrderDetailBlade(wo.Id, wo.Product, wo.Quantity, wo.Status, wo.Progress), wo.Id); return default; }
        ));
        
        return BladeHelper.WithHeader(
            new Button("New Work Order", _ => client.Toast("Create work order"))
                .Icon(Icons.Plus)
                .Variant(ButtonVariant.Primary),
            new List(listItems)
        );
    }
}

public class WorkOrderDetailBlade(string id, string product, int quantity, string status, int progress) : ViewBase
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
                    .Add($"Product: {product}")
                    .Add($"Quantity: {quantity}")
                    .Add(new Badge(status)
                        .Variant(status == "Completed" ? BadgeVariant.Success :
                               status == "In Production" ? BadgeVariant.Primary :
                               BadgeVariant.Secondary))
                    .Add(new Progress(progress)))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Update Progress", _ => client.Toast("Update progress"))
                        .Variant(ButtonVariant.Primary))
                    .Add(status != "Completed" 
                        ? new Button("Complete", _ => client.Toast("Mark complete"))
                            .Variant(ButtonVariant.Success)
                        : null))
        );
    }
}
