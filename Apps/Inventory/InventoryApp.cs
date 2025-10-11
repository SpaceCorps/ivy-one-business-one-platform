using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Inventory;

[App(icon: Icons.Box, title: "Inventory", path: new[] { "Business Operations" })]
public class InventoryApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var products = context.Products.OrderBy(p => p.Name).ToList();
        var lowStock = products.Count(p => p.QuantityInStock < p.MinimumStockLevel);
        var totalValue = products.Sum(p => p.QuantityInStock * p.Cost);
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Inventory Management")
                .Add("Track stock levels and manage products")
                .Add(Layout.Grid()
                    .Columns(3)
                    .Gap(12)
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Total Products").Add(products.Count.ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Low Stock").Add(lowStock.ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Inventory Value").Add($"${totalValue:N2}"))))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Add Product", _ => client.Toast("Adding new product"))
                        .Variant(ButtonVariant.Primary)
                        .Icon(Icons.Plus))
                    .Add(new Button("Stock Movement", _ => client.Toast("Record stock movement"))
                        .Variant(ButtonVariant.Secondary))
                    .Add(new Button("Low Stock Alert", _ => client.Toast($"{lowStock} items need restocking"))
                        .Variant(ButtonVariant.Warning)))
                .Add(BuildProductsList(products, client))
        );
    }

    private object BuildProductsList(List<Product> products, IClientProvider client)
    {
        if (products.Count == 0)
            return new Card(Layout.Vertical().Padding(16).Add("No products yet. Add your first product!"));
        
        var cards = products.Take(10).Select(prod => new Card(
            Layout.Horizontal()
                .Gap(12)
                .Padding(16)
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add($"{prod.ProductCode} - {prod.Name}")
                    .Add(prod.Description)
                    .Add(Layout.Horizontal()
                        .Gap(8)
                        .Add(new Badge($"Stock: {prod.QuantityInStock}")
                            .Variant(prod.QuantityInStock < prod.MinimumStockLevel ? BadgeVariant.Destructive : BadgeVariant.Success))
                        .Add(new Badge($"${prod.Price:N2}").Variant(BadgeVariant.Info))
                        .Add(new Badge(prod.Category).Variant(BadgeVariant.Outline))))
                .Add(Layout.Horizontal()
                    .Gap(4)
                    .Add(new Button("Edit", _ => client.Toast($"Editing: {prod.Name}"))
                        .Small()
                        .Variant(ButtonVariant.Primary))
                    .Add(new Button("Adjust", _ => client.Toast($"Adjust stock for: {prod.Name}"))
                        .Small()
                        .Variant(ButtonVariant.Outline)))
        ));
        
        return Layout.Vertical().Gap(8).Add(cards);
    }
}
