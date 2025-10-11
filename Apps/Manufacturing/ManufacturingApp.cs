namespace IvyOneBusinessOnePlatform.Apps.Manufacturing;

[App(icon: Icons.Settings, title: "Manufacturing", path: new[] { "Business Operations" })]
public class ManufacturingApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        var workOrders = new[]
        {
            new { Id = "WO-001", Product = "Widget A", Quantity = 500, Status = "In Production", Progress = 75 },
            new { Id = "WO-002", Product = "Component B", Quantity = 1000, Status = "Scheduled", Progress = 0 },
            new { Id = "WO-003", Product = "Assembly C", Quantity = 250, Status = "Completed", Progress = 100 }
        };
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Manufacturing Operations")
                .Add("Manage production orders and workflows")
                .Add(Layout.Grid()
                    .Columns(3)
                    .Gap(12)
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Work Orders").Add(workOrders.Length.ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("In Production").Add(workOrders.Count(w => w.Status == "In Production").ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Completed").Add(workOrders.Count(w => w.Status == "Completed").ToString()))))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("New Work Order", _ => client.Toast("Creating work order"))
                        .Variant(ButtonVariant.Primary)
                        .Icon(Icons.Plus))
                    .Add(new Button("Bill of Materials", _ => client.Toast("View BOM"))
                        .Variant(ButtonVariant.Secondary))
                    .Add(new Button("Production Schedule", _ => client.Toast("View schedule"))
                        .Variant(ButtonVariant.Outline)))
                .Add(BuildWorkOrdersList(workOrders, client))
        );
    }

    private object BuildWorkOrdersList(dynamic[] orders, IClientProvider client)
    {
        var cards = orders.Select(wo => new Card(
            Layout.Horizontal()
                .Gap(12)
                .Padding(16)
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add(wo.Id)
                    .Add($"Product: {wo.Product}")
                    .Add($"Quantity: {wo.Quantity}")
                    .Add(Layout.Horizontal()
                        .Gap(8)
                        .Add(new Badge(wo.Status)
                            .Variant(wo.Status == "Completed" ? BadgeVariant.Success :
                                   wo.Status == "In Production" ? BadgeVariant.Primary :
                                   BadgeVariant.Secondary))
                        .Add(new Badge($"{wo.Progress}% complete").Variant(BadgeVariant.Info))))
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add(new Progress(wo.Progress / 100.0))
                    .Add(Layout.Horizontal()
                        .Gap(4)
                        .Add(new Button("View", _ => client.Toast($"Viewing: {wo.Id}"))
                            .Small()
                            .Variant(ButtonVariant.Primary))
                        .Add(wo.Status != "Completed" 
                            ? new Button("Update", _ => client.Toast($"Updating: {wo.Id}"))
                                .Small()
                                .Variant(ButtonVariant.Outline)
                            : null)))
        ));
        
        return Layout.Vertical().Gap(8).Add(cards);
    }
}
