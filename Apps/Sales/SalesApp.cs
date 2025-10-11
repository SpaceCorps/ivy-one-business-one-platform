namespace IvyOneBusinessOnePlatform.Apps.Sales;

[App(icon: Icons.TrendingUp, title: "Sales", path: new[] { "Business Operations" })]
public class SalesApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new SalesRootBlade(), "Sales", Size.Units(80));
    }
}

public class SalesRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var blades = this.UseContext<IBladeController>();
        
        var orders = new[]
        {
            new { Id = "SO-001", Customer = "Tech Corp", Amount = 12500.00m, Status = "Confirmed" },
            new { Id = "SO-002", Customer = "Retail Inc", Amount = 8300.00m, Status = "Pending" },
            new { Id = "SO-003", Customer = "Global Ltd", Amount = 15600.00m, Status = "Shipped" }
        };
        
        var listItems = orders.Select(order => new ListItem(
            title: $"{order.Id} - {order.Customer}",
            subtitle: $"${order.Amount:N2}",
            icon: Icons.ShoppingCart,
            badge: order.Status,
            onClick: _ => { blades.Push(this, new SalesOrderDetailBlade(order.Id, order.Customer, order.Amount, order.Status), order.Id); return default; }
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(Text.H3("Sales Orders"))
                .Add(new Button("New Order", _ => client.Toast("Create sales order"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)))
            .Add(new List(listItems));
    }
}

public class SalesOrderDetailBlade(string id, string customer, decimal amount, string status) : ViewBase
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
                    .Add($"Customer: {customer}")
                    .Add($"Amount: ${amount:N2}")
                    .Add(new Badge(status)
                        .Variant(status == "Shipped" ? BadgeVariant.Success :
                               status == "Confirmed" ? BadgeVariant.Primary :
                               BadgeVariant.Secondary)))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("View Details", _ => client.Toast("View order"))
                        .Variant(ButtonVariant.Primary))
                    .Add(new Button("Invoice", _ => client.Toast("Generate invoice"))
                        .Variant(ButtonVariant.Secondary)))
        );
    }
}
