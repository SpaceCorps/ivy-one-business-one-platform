namespace IvyOneBusinessOnePlatform.Apps.Sales;

[App(icon: Icons.TrendingUp, title: "Sales", path: new[] { "Business Operations" })]
public class SalesApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        var orders = new[]
        {
            new { Id = "SO-001", Customer = "Tech Corp", Amount = 12500.00m, Status = "Confirmed", Date = DateTime.Now.AddDays(-1) },
            new { Id = "SO-002", Customer = "Retail Inc", Amount = 8300.00m, Status = "Pending", Date = DateTime.Now.AddHours(-3) },
            new { Id = "SO-003", Customer = "Global Ltd", Amount = 15600.00m, Status = "Shipped", Date = DateTime.Now.AddDays(-5) }
        };
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Sales Management")
                .Add("Manage sales orders and quotations")
                .Add(Layout.Grid()
                    .Columns(4)
                    .Gap(12)
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Total Orders").Add(orders.Length.ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Pending").Add(orders.Count(o => o.Status == "Pending").ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Revenue").Add($"${orders.Where(o => o.Status != "Cancelled").Sum(o => o.Amount):N2}")))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Avg Order").Add($"${orders.Average(o => o.Amount):N2}"))))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("New Order", _ => client.Toast("Creating sales order"))
                        .Variant(ButtonVariant.Primary)
                        .Icon(Icons.Plus))
                    .Add(new Button("Quotations", _ => client.Toast("View quotations"))
                        .Variant(ButtonVariant.Secondary))
                    .Add(new Button("Reports", _ => client.Toast("Sales reports"))
                        .Variant(ButtonVariant.Outline)))
                .Add(BuildOrdersList(orders, client))
        );
    }

    private object BuildOrdersList(dynamic[] orders, IClientProvider client)
    {
        var cards = orders.Select(order => new Card(
            Layout.Horizontal()
                .Gap(12)
                .Padding(16)
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add(order.Id)
                    .Add($"Customer: {order.Customer}")
                    .Add($"Date: {order.Date:MMM dd, yyyy}")
                    .Add(Layout.Horizontal()
                        .Gap(8)
                        .Add(new Badge(order.Status)
                            .Variant(order.Status == "Shipped" ? BadgeVariant.Success :
                                   order.Status == "Confirmed" ? BadgeVariant.Primary :
                                   BadgeVariant.Secondary))
                        .Add(new Badge($"${order.Amount:N2}").Variant(BadgeVariant.Info))))
                .Add(Layout.Horizontal()
                    .Gap(4)
                    .Add(new Button("View", _ => client.Toast($"Viewing: {order.Id}"))
                        .Small()
                        .Variant(ButtonVariant.Primary))
                    .Add(new Button("Invoice", _ => client.Toast($"Generate invoice for: {order.Id}"))
                        .Small()
                        .Variant(ButtonVariant.Outline)))
        ));
        
        return Layout.Vertical().Gap(8).Add(cards);
    }
}
