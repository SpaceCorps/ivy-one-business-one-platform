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
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var orders = context.PurchaseOrders.OrderByDescending(po => po.OrderDate).ToList();
        
        var listItems = orders.Select(order => new ListItem(
            title: $"{order.OrderNumber} - {order.Supplier}",
            subtitle: $"${order.Amount:N2}",
            icon: Icons.ShoppingBag,
            badge: order.Status.ToString(),
            onClick: _ => blades.Push(this, new PurchaseOrderDetailBlade(order.Id), order.OrderNumber)
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
            .Add(orders.Count == 0 
                ? Text.Block("No purchase orders yet. Create your first order!")
                : new List(listItems));
    }
}

public class PurchaseOrderDetailBlade(int orderId) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var order = context.PurchaseOrders.FirstOrDefault(po => po.Id == orderId);
        
        if (order == null)
        {
            return Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add(Text.H3("Purchase Order Not Found"))
                .Add(new Button("Go Back", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary));
        }
        
        var currentStatus = this.UseState(order.Status);
        
        this.UseEffect(() =>
        {
            // Update database when status changes
            var dbOrder = context.PurchaseOrders.FirstOrDefault(po => po.Id == orderId);
            if (dbOrder != null && dbOrder.Status != currentStatus.Value)
            {
                dbOrder.Status = currentStatus.Value;
                dbOrder.UpdatedAt = DateTime.UtcNow;
                context.SaveChanges();
            }
        }, [currentStatus.ToTrigger()]);
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Text.H3($"Purchase Order {order.OrderNumber}"))
            .Add(new Card(
                Layout.Vertical()
                    .Gap(12)
                    .Padding(16)
                    .Add(Layout.Vertical()
                        .Gap(8)
                        .Add(Text.Label("Supplier"))
                        .Add(Text.Block(order.Supplier)))
                    .Add(Layout.Vertical()
                        .Gap(8)
                        .Add(Text.Label("Amount"))
                        .Add(Text.Block($"${order.Amount:N2}")))
                    .Add(Layout.Vertical()
                        .Gap(8)
                        .Add(Text.Label("Status"))
                        .Add(new Badge(currentStatus.Value.ToString())
                            .Variant(currentStatus.Value == PurchaseOrderStatus.Received ? BadgeVariant.Success :
                                   currentStatus.Value == PurchaseOrderStatus.Approved ? BadgeVariant.Primary :
                                   currentStatus.Value == PurchaseOrderStatus.Cancelled ? BadgeVariant.Destructive :
                                   BadgeVariant.Secondary)))))
            .Add(new Card(
                Layout.Vertical()
                    .Gap(12)
                    .Padding(16)
                    .Add(Text.Label("Order Details"))
                    .Add($"Order Date: {order.OrderDate:MMM dd, yyyy}")
                    .Add($"Expected Delivery: {order.ExpectedDeliveryDate?.ToString("MMM dd, yyyy") ?? "Not set"}")
                    .Add($"Payment Terms: {order.PaymentTerms}")
                    .Add($"Department: {order.Department}")
                    .Add(string.IsNullOrEmpty(order.Notes) ? null : $"Notes: {order.Notes}")))
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(currentStatus.Value == PurchaseOrderStatus.Pending 
                    ? new Button("Approve Order", _ => {
                        currentStatus.Set(PurchaseOrderStatus.Approved);
                        client.Toast($"Purchase Order {order.OrderNumber} approved!");
                    })
                        .Variant(ButtonVariant.Success)
                        .Icon(Icons.Check)
                    : null)
                .Add(currentStatus.Value == PurchaseOrderStatus.Approved 
                    ? new Button("Mark as Received", _ => {
                        currentStatus.Set(PurchaseOrderStatus.Received);
                        client.Toast($"Purchase Order {order.OrderNumber} marked as received!");
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
