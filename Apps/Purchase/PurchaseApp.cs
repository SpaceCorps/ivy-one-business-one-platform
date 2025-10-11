namespace IvyOneBusinessOnePlatform.Apps.Purchase;

[App(icon: Icons.ShoppingBag, title: "Purchase", path: new[] { "Business Operations" })]
public class PurchaseApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        var orders = new[]
        {
            new { Id = "PO-001", Supplier = "Office Supplies Co", Amount = 1250.00m, Status = "Pending", Date = DateTime.Now.AddDays(-1) },
            new { Id = "PO-002", Supplier = "Tech Equipment Ltd", Amount = 5400.00m, Status = "Approved", Date = DateTime.Now.AddDays(-3) },
            new { Id = "PO-003", Supplier = "Furniture World", Amount = 3200.00m, Status = "Received", Date = DateTime.Now.AddDays(-7) }
        };
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Purchase Management")
                .Add("Manage purchase orders and vendor relationships")
                .Add(Layout.Grid()
                    .Columns(3)
                    .Gap(12)
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Total Orders").Add(orders.Length.ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Pending Approval").Add(orders.Count(o => o.Status == "Pending").ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Total Value").Add($"${orders.Sum(o => o.Amount):N2}"))))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("New Purchase Order", _ => client.Toast("Creating purchase order"))
                        .Variant(ButtonVariant.Primary)
                        .Icon(Icons.Plus))
                    .Add(new Button("Vendors", _ => client.Toast("Manage vendors"))
                        .Variant(ButtonVariant.Secondary))
                    .Add(new Button("Approvals", _ => client.Toast("View pending approvals"))
                        .Variant(ButtonVariant.Outline)))
                .Add(BuildOrdersList(orders, client))
        );
    }

    private object BuildOrdersList(dynamic[] orders, IClientProvider client)
    {
        var orderCards = orders.Select(order => new Card(
            Layout.Horizontal()
                .Gap(12)
                .Padding(16)
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add(order.Id)
                    .Add(order.Supplier)
                    .Add($"Date: {order.Date:MMM dd, yyyy}")
                    .Add(Layout.Horizontal()
                        .Gap(8)
                        .Add(new Badge(order.Status)
                            .Variant(order.Status == "Received" ? BadgeVariant.Success :
                                   order.Status == "Approved" ? BadgeVariant.Primary :
                                   BadgeVariant.Secondary))
                        .Add(new Badge($"${order.Amount:N2}").Variant(BadgeVariant.Info))))
                .Add(Layout.Horizontal()
                    .Gap(4)
                    .Add(new Button("View", _ => client.Toast($"Viewing: {order.Id}"))
                        .Small()
                        .Variant(ButtonVariant.Primary))
                    .Add(order.Status == "Pending" 
                        ? new Button("Approve", _ => client.Toast($"Approving: {order.Id}"))
                            .Small()
                            .Variant(ButtonVariant.Success)
                        : null))
        ));
        
        return Layout.Vertical().Gap(8).Add(orderCards);
    }
}
