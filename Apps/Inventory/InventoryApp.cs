using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Inventory;

[App(icon: Icons.Box, title: "Inventory", path: new[] { "Business Operations" })]
public class InventoryApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new InventoryRootBlade(), "Inventory", Size.Units(100));
    }
}

public class InventoryRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var products = context.Products.OrderBy(p => p.Name).ToList();
        
        var listItems = products.Select(prod => new ListItem(
            title: $"{prod.ProductCode} - {prod.Name}",
            subtitle: $"Stock: {prod.QuantityInStock} - ${prod.Price:N2} - {prod.Category}",
            icon: Icons.Package,
            badge: prod.QuantityInStock < prod.MinimumStockLevel ? "Low Stock" : $"{prod.QuantityInStock}",
            onClick: _ => { blades.Push(this, new ProductDetailBlade(prod.Id), prod.Name); return default; }
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(Text.H3("Inventory"))
                .Add(new Button("Add Product", _ => client.Toast("Add product"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)))
            .Add(products.Count == 0 
                ? Text.Block("No products yet.")
                : new List(listItems));
    }
}

public class ProductDetailBlade(int productId) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var product = context.Products.FirstOrDefault(p => p.Id == productId);
        
        if (product == null)
            return "Product not found";
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add($"{product.ProductCode} - {product.Name}")
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add($"Description: {product.Description}")
                    .Add($"Category: {product.Category}")
                    .Add($"Price: ${product.Price:N2}")
                    .Add($"Cost: ${product.Cost:N2}")
                    .Add($"Stock: {product.QuantityInStock}")
                    .Add($"Min Stock Level: {product.MinimumStockLevel}")
                    .Add(new Badge(product.QuantityInStock < product.MinimumStockLevel ? "Low Stock" : "In Stock")
                        .Variant(product.QuantityInStock < product.MinimumStockLevel ? BadgeVariant.Destructive : BadgeVariant.Success)))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Edit", _ => client.Toast("Edit product"))
                        .Variant(ButtonVariant.Primary))
                    .Add(new Button("Adjust Stock", _ => client.Toast("Adjust stock"))
                        .Variant(ButtonVariant.Secondary)))
        );
    }
}
