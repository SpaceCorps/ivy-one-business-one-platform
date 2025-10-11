namespace IvyOneBusinessOnePlatform.Apps.Purchase;

[App(icon: Icons.ShoppingBag, title: "Purchase", path: new[] { "Business Operations" })]
public class PurchaseApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new PurchaseRootBlade(), "Purchase Orders", Size.Units(110));
    }
}

public class PurchaseRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var searchQuery = this.UseState("");
        
        var query = context.PurchaseOrders.AsQueryable();
        
        if (!string.IsNullOrEmpty(searchQuery.Value))
        {
            query = query.Where(po => 
                po.OrderNumber.Contains(searchQuery.Value) ||
                po.Supplier.Contains(searchQuery.Value) ||
                po.Department.Contains(searchQuery.Value));
        }
        
        var orders = query.OrderByDescending(po => po.OrderDate).ToList();
        
        var listItems = orders.Select(order => new ListItem(
            title: $"{order.OrderNumber} - {order.Supplier}",
            subtitle: $"${order.Amount:N2}",
            icon: Icons.ShoppingBag,
            badge: order.Status.ToString(),
            onClick: _ => blades.Push(this, new PurchaseOrderDetailBlade(order.Id), order.OrderNumber)
        ));
        
        return BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(4)
                .Add(searchQuery.ToSearchInput().Placeholder("Search orders by number, supplier, or department..."))
                .Add(new Button("New PO", _ => client.Toast("Create PO"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)),
            orders.Count == 0 
                ? Text.Block("No purchase orders found. Try a different search or create your first order!")
                : new List(listItems)
        );
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
                .Gap(4)
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
        
        var statusBadge = new Badge(currentStatus.Value.ToString())
            .Variant(currentStatus.Value == PurchaseOrderStatus.Received ? BadgeVariant.Success :
                   currentStatus.Value == PurchaseOrderStatus.Approved ? BadgeVariant.Primary :
                   currentStatus.Value == PurchaseOrderStatus.Cancelled ? BadgeVariant.Destructive :
                   BadgeVariant.Secondary);

        var orderDetails = new
        {
            OrderNumber = order.OrderNumber,
            Supplier = order.Supplier,
            Amount = $"${order.Amount:N2}",
            Status = statusBadge,
            OrderDate = order.OrderDate.ToString("MMM dd, yyyy"),
            ExpectedDelivery = order.ExpectedDeliveryDate?.ToString("MMM dd, yyyy") ?? "Not set",
            PaymentTerms = order.PaymentTerms,
            Department = order.Department,
            Notes = order.Notes
        };
        
        return Layout.Vertical()
            .Gap(4)
            .Add(Text.H3($"Purchase Order {order.OrderNumber}"))
            .Add(orderDetails.ToDetails().RemoveEmpty().MultiLine(x => x.Notes))
            .Add(Layout.Horizontal()
                .Gap(4)
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
