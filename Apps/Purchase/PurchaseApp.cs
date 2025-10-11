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
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(id)
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add($"Supplier: {supplier}")
                    .Add($"Amount: ${amount:N2}")
                    .Add(new Badge(status)
                        .Variant(status == "Received" ? BadgeVariant.Success :
                               status == "Approved" ? BadgeVariant.Primary :
                               BadgeVariant.Secondary)))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("View Details", _ => client.Toast("View PO"))
                        .Variant(ButtonVariant.Primary))
                    .Add(status == "Pending" 
                        ? new Button("Approve", _ => client.Toast("PO approved"))
                            .Variant(ButtonVariant.Success)
                        : null))
        );
    }
}
