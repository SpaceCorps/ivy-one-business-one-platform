using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.PointOfSale;

[App(icon: Icons.ShoppingCart, title: "Point of Sale", path: new[] { "Commerce & Services" })]
public class PointOfSaleApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var selectedView = this.UseState("sales");
        var cartTotal = this.UseState(0.00m);
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Point of Sale System")
                .Add("Process sales transactions and manage inventory")
                .Add(Layout.Grid()
                    .Columns(2)
                    .Gap(16)
                    .Add(new Card(
                        Layout.Vertical()
                            .Gap(12)
                            .Padding(16)
                            .Add("Current Sale")
                            .Add(Layout.Vertical()
                                .Gap(8)
                                .Add("Shopping Cart")
                                .Add(new Badge("Cart is empty")
                                    .Variant(BadgeVariant.Secondary)))
                            .Add(Layout.Horizontal()
                                .Gap(12)
                                .Add("Total:")
                                .Add($"${cartTotal.Value:N2}"))
                            .Add(Layout.Horizontal()
                                .Gap(8)
                                .Add(new Button("Add Item", _ => {
                                    cartTotal.Set(cartTotal.Value + 10.00m);
                                    client.Toast("Item added to cart");
                                })
                                    .Variant(ButtonVariant.Primary)
                                    .Icon(Icons.Plus))
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
                                    .Icon(Icons.Check)))))
                    .Add(new Card(
                        Layout.Vertical()
                            .Gap(12)
                            .Padding(16)
                            .Add("Quick Products")
                            .Add(Layout.Grid()
                                .Columns(2)
                                .Gap(8)
                                .Add(new Button("Coffee - $5", _ => {
                                    cartTotal.Set(cartTotal.Value + 5.00m);
                                    client.Toast("Coffee added");
                                })
                                    .Small())
                                .Add(new Button("Sandwich - $8", _ => {
                                    cartTotal.Set(cartTotal.Value + 8.00m);
                                    client.Toast("Sandwich added");
                                })
                                    .Small())
                                .Add(new Button("Salad - $7", _ => {
                                    cartTotal.Set(cartTotal.Value + 7.00m);
                                    client.Toast("Salad added");
                                })
                                    .Small())
                                .Add(new Button("Drink - $3", _ => {
                                    cartTotal.Set(cartTotal.Value + 3.00m);
                                    client.Toast("Drink added");
                                })
                                    .Small())))))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Sales", _ => selectedView.Set("sales"))
                        .Variant(selectedView.Value == "sales" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("History", _ => selectedView.Set("history"))
                        .Variant(selectedView.Value == "history" ? ButtonVariant.Primary : ButtonVariant.Secondary)))
                .Add(BuildView(selectedView.Value, client))
        );
    }

    private object BuildView(string view, IClientProvider client)
    {
        if (view == "history")
        {
            var sales = new[]
            {
                new { Id = "TXN-001", Date = DateTime.Now.AddHours(-2), Amount = 45.50m, Items = 3 },
                new { Id = "TXN-002", Date = DateTime.Now.AddHours(-4), Amount = 12.00m, Items = 1 },
                new { Id = "TXN-003", Date = DateTime.Now.AddHours(-6), Amount = 28.75m, Items = 2 }
            };

            return new Card(
                Layout.Vertical()
                    .Gap(12)
                    .Padding(16)
                    .Add("Sales History")
                    .Add(Layout.Vertical()
                        .Gap(8)
                        .Add(sales.Select(sale => new Card(
                            Layout.Horizontal()
                                .Gap(12)
                                .Padding(12)
                                .Add(Layout.Vertical()
                                    .Gap(4)
                                    .Add(sale.Id)
                                    .Add(sale.Date.ToString("MMM dd, HH:mm"))
                                    .Add(new Badge($"{sale.Items} items")
                                        .Variant(BadgeVariant.Outline)))
                                .Add(Layout.Vertical()
                                    .Gap(4)
                                    .Add($"${sale.Amount:N2}")
                                    .Add(new Button("View", _ => client.Toast($"Viewing {sale.Id}"))
                                        .Small()
                                        .Variant(ButtonVariant.Outline)))
                        ))))
            );
        }

        return new Card(
            Layout.Vertical()
                .Padding(16)
                .Add("Ready to process sales transactions")
        );
    }
}
