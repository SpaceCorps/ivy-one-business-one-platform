namespace IvyOneBusinessOnePlatform.Apps.Purchase;

[App(icon: Icons.ShoppingBag, title: "Purchase", path: new[] { "Business Operations" })]
public class PurchaseApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new PurchaseRootBlade(), "Purchase", Size.Units(110));
    }
}

public class PurchaseRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var blades = this.UseContext<IBladeController>();
        
        var orders = new[]
        {
            new { Id = "PO-001", Supplier = "Office Supplies Co", Amount = 1250.00m, Status = "Pending" },
            new { Id = "PO-002", Supplier = "Tech Equipment Ltd", Amount = 5400.00m, Status = "Approved" },
            new { Id = "PO-003", Supplier = "Furniture World", Amount = 3200.00m, Status = "Received" }
        };
        
        var listItems = orders.Select(order => new ListItem(
            title: $"{order.Id} - {order.Supplier}",
            subtitle: $"${order.Amount:N2}",
            icon: Icons.ShoppingBag,
            badge: order.Status,
            onClick: _ => { blades.Push(this, new PurchaseOrderDetailBlade(order.Id, order.Supplier, order.Amount, order.Status), order.Id); return default; }
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(Text.H3("Purchase Orders"))
                .Add(new Button("New Purchase Order", _ => client.Toast("Create PO"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)))
            .Add(new List(listItems));
    }
}

public class PurchaseOrderDetailBlade(string id, string supplier, decimal amount, string status) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var blades = this.UseContext<IBladeController>();
        var currentStatus = this.UseState(status);
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Text.H3($"Purchase Order {id}"))
            .Add(new Card(
                Layout.Vertical()
                    .Gap(12)
                    .Padding(16)
                    .Add(Layout.Vertical()
                        .Gap(8)
                        .Add(Text.Label("Supplier"))
                        .Add(Text.Block(supplier)))
                    .Add(Layout.Vertical()
                        .Gap(8)
                        .Add(Text.Label("Amount"))
                        .Add(Text.Block($"${amount:N2}")))
                    .Add(Layout.Vertical()
                        .Gap(8)
                        .Add(Text.Label("Status"))
                        .Add(new Badge(currentStatus.Value)
                            .Variant(currentStatus.Value == "Received" ? BadgeVariant.Success :
                                   currentStatus.Value == "Approved" ? BadgeVariant.Primary :
                                   BadgeVariant.Secondary)))))
            .Add(new Card(
                Layout.Vertical()
                    .Gap(12)
                    .Padding(16)
                    .Add(Text.Label("Order Details"))
                    .Add($"Order Date: {DateTime.Now.AddDays(-7):MMM dd, yyyy}")
                    .Add($"Expected Delivery: {DateTime.Now.AddDays(14):MMM dd, yyyy}")
                    .Add($"Payment Terms: Net 30")
                    .Add($"Department: Operations")))
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(currentStatus.Value == "Pending" 
                    ? new Button("Approve Order", _ => {
                        currentStatus.Set("Approved");
                        client.Toast($"Purchase Order {id} approved!");
                    })
                        .Variant(ButtonVariant.Success)
                        .Icon(Icons.Check)
                    : null)
                .Add(currentStatus.Value == "Approved" 
                    ? new Button("Mark as Received", _ => {
                        currentStatus.Set("Received");
                        client.Toast($"Purchase Order {id} marked as received!");
                    })
                        .Variant(ButtonVariant.Primary)
                        .Icon(Icons.Package)
                    : null)
                .Add(new Button("Edit Order", _ => client.Toast("Edit functionality coming soon"))
                    .Variant(ButtonVariant.Outline)
                    .Icon(Icons.Pencil))
                .Add(new Button("Cancel", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary)));
    }
}
