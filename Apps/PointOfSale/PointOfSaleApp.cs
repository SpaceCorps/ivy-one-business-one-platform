namespace IvyOneBusinessOnePlatform.Apps.PointOfSale;

[App(icon: Icons.ShoppingCart, title: "Point of Sale", path: new[] { "Commerce & Services" })]
public class PointOfSaleApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new POSRootBlade(), "Point of Sale", Size.Units(100));
    }
}

public class POSRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var blades = this.UseContext<IBladeController>();
        var cartTotal = this.UseState(0.00m);
        
        var products = new[]
        {
            new { Name = "Coffee", Price = 5.00m },
            new { Name = "Sandwich", Price = 8.00m },
            new { Name = "Salad", Price = 7.00m },
            new { Name = "Drink", Price = 3.00m }
        };
        
        var listItems = products.Select(prod => new ListItem(
            title: prod.Name,
            subtitle: $"${prod.Price:N2}",
            icon: Icons.ShoppingCart,
            badge: $"${prod.Price:N2}",
            onClick: _ => {
                cartTotal.Set(cartTotal.Value + prod.Price);
                client.Toast($"{prod.Name} added to cart");
            }
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Text.H3("Point of Sale"))
            .Add(new Card(
                Layout.Vertical()
                    .Gap(12)
                    .Padding(16)
                    .Add($"Cart Total: ${cartTotal.Value:N2}")
                    .Add(Layout.Horizontal()
                        .Gap(8)
                        .Add(new Button("Clear Cart", _ => {
                            cartTotal.Set(0);
                            client.Toast("Cart cleared");
                        })
                            .Variant(ButtonVariant.Secondary))
                        .Add(new Button("Checkout", _ => {
                            client.Toast($"Processing ${cartTotal.Value:N2}");
                            cartTotal.Set(0);
                        })
                            .Variant(ButtonVariant.Success)
                            .Icon(Icons.Check))
                        .Add(new Button("History", _ => blades.Push(this, new SalesHistoryBlade(), "History"))
                            .Variant(ButtonVariant.Outline)))))
            .Add(new List(listItems));
    }
}

public class SalesHistoryBlade : ViewBase
{
    public override object? Build()
    {
        var sales = new[]
        {
            new { Id = "TXN-001", Date = DateTime.Now.AddHours(-2), Amount = 45.50m, Items = 3 },
            new { Id = "TXN-002", Date = DateTime.Now.AddHours(-4), Amount = 12.00m, Items = 1 },
            new { Id = "TXN-003", Date = DateTime.Now.AddHours(-6), Amount = 28.75m, Items = 2 }
        };
        
        var listItems = sales.Select(sale => new ListItem(
            title: sale.Id,
            subtitle: $"{sale.Date:MMM dd, HH:mm} - {sale.Items} items",
            icon: Icons.Receipt,
            badge: $"${sale.Amount:N2}"
        ));
        
        return new List(listItems);
    }
}
