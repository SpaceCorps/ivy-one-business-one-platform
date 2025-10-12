using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Inventory;

public static class MovementType
{
    public const string In = "In";
    public const string Out = "Out";
    public const string Transfer = "Transfer";
    public const string Adjustment = "Adjustment";
}

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
        var searchQuery = this.UseState("");
        var refreshToken = this.UseRefreshToken();
        
        var query = context.Products.AsQueryable();
        
        if (!string.IsNullOrEmpty(searchQuery.Value))
        {
            var searchPattern = $"%{searchQuery.Value}%";
            query = query.Where(p => 
                EF.Functions.Like(p.ProductCode, searchPattern) ||
                EF.Functions.Like(p.Name, searchPattern) ||
                EF.Functions.Like(p.Category, searchPattern) ||
                EF.Functions.Like(p.Supplier, searchPattern));
        }
        
        var products = query.OrderBy(p => p.Name).ToList();
        
        var listItems = products.Select(prod => new ListItem(
            title: $"{prod.ProductCode} - {prod.Name}",
            subtitle: $"Stock: {prod.QuantityInStock} - ${prod.Price:N2} - {prod.Category}",
            icon: Icons.Package,
            badge: prod.QuantityInStock == 0 ? "Out of Stock" : 
                   prod.QuantityInStock < prod.MinimumStockLevel ? "Low Stock" : "In Stock",
            onClick: _ => { blades.Push(this, new ProductDetailBlade(prod.Id, () => refreshToken.Refresh()), prod.Name); return default; }
        ));
        
        return BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(8)
                .Add(searchQuery.ToSearchInput().Placeholder("Search by code, name, category, or supplier..."))
                .Add(new Button("Add Product")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .WithSheet(
                        () => new ProductFormSheet(null, () => refreshToken.Refresh()),
                        title: "New Product",
                        description: "Add a new product to inventory",
                        width: Size.Fraction(1/3f)
                    )),
            products.Count == 0 
                ? Text.Block("No products yet. Try a different search or add your first product!")
                : new List(listItems)
        );
    }
}

public class ProductDetailBlade(int productId, Action? onRefresh = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var initialProduct = context.Products.FirstOrDefault(p => p.Id == productId);
        
        if (initialProduct == null)
        {
            return Layout.Vertical()
                .Gap(4)
                .Add(Text.H3("Product Not Found"))
                .Add(new Button("Go Back", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary));
        }
        
        var productData = this.UseState(initialProduct);
        var refreshToken = this.UseRefreshToken();
        
        this.UseEffect(() =>
        {
            var updatedProduct = context.Products.FirstOrDefault(p => p.Id == productId);
            if (updatedProduct != null)
            {
                productData.Set(updatedProduct);
            }
        }, [refreshToken.ToTrigger()]);
        
        var stockBadge = new Badge(
            productData.Value.QuantityInStock == 0 ? "Out of Stock" : 
            productData.Value.QuantityInStock < productData.Value.MinimumStockLevel ? "Low Stock" : "In Stock")
            .Variant(
                productData.Value.QuantityInStock == 0 ? BadgeVariant.Destructive :
                productData.Value.QuantityInStock < productData.Value.MinimumStockLevel ? BadgeVariant.Warning : BadgeVariant.Success);

        var productDetails = new
        {
            ProductCode = productData.Value.ProductCode,
            Name = productData.Value.Name,
            Description = productData.Value.Description,
            Category = productData.Value.Category,
            Supplier = productData.Value.Supplier,
            Price = $"${productData.Value.Price:N2}",
            Cost = $"${productData.Value.Cost:N2}",
            QuantityInStock = productData.Value.QuantityInStock.ToString(),
            MinimumStockLevel = productData.Value.MinimumStockLevel.ToString(),
            StockStatus = stockBadge,
            IsActive = productData.Value.IsActive ? "Active" : "Inactive"
        };
        
        return Layout.Vertical()
            .Gap(4)
            .Add(Text.H3($"{productData.Value.ProductCode} - {productData.Value.Name}"))
            .Add(productDetails.ToDetails().RemoveEmpty().MultiLine(x => x.Description))
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(new Button("Adjust Stock")
                    .Variant(ButtonVariant.Primary)
                    .Icon(Icons.Package)
                    .WithSheet(
                        () => new StockAdjustmentSheet(productId, () => {
                            refreshToken.Refresh();
                            onRefresh?.Invoke();
                        }),
                        title: "Adjust Stock",
                        description: $"Adjust stock for {productData.Value.Name}",
                        width: Size.Fraction(1/3f)
                    ))
                .Add(new Button("Edit Product")
                    .Variant(ButtonVariant.Outline)
                    .Icon(Icons.Pencil)
                    .WithSheet(
                        () => new ProductFormSheet(productId, () => {
                            refreshToken.Refresh();
                            onRefresh?.Invoke();
                        }),
                        title: "Edit Product",
                        description: $"Edit product {productData.Value.Name}",
                        width: Size.Fraction(1/3f)
                    ))
                .Add(new Button("Delete Product")
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.Trash)
                    .HandleClick(_ => {
                        try
                        {
                            var productToDelete = context.Products.FirstOrDefault(p => p.Id == productId);
                            if (productToDelete != null)
                            {
                                context.Products.Remove(productToDelete);
                                context.SaveChanges();
                                client.Toast($"Product {productData.Value.Name} deleted successfully!");
                                refreshToken.Refresh();
                                onRefresh?.Invoke();
                                blades.Pop();
                            }
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error deleting product: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary)));
    }
}

public class ProductFormSheet(int? productId = null, Action? onClose = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var isEdit = productId.HasValue;
        var existingProduct = isEdit ? context.Products.FirstOrDefault(p => p.Id == productId!.Value) : null;
        
        var productForm = this.UseState(existingProduct ?? new Product
        {
            ProductCode = isEdit ? "" : $"PROD-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Name = "",
            Description = "",
            Price = 0.00m,
            Cost = 0.00m,
            QuantityInStock = 0,
            MinimumStockLevel = 10,
            Category = "",
            Supplier = "",
            IsActive = true
        });
        
        var formBuilder = productForm.ToForm(isEdit ? "Save Changes" : "Create Product")
            .Label(m => m.ProductCode, "Product Code")
            .Builder(m => m.ProductCode, s => s.ToTextInput().Disabled(isEdit))
            .Label(m => m.Name, "Product Name")
            .Label(m => m.Description, "Description")
            .Builder(m => m.Description, s => s.ToTextAreaInput())
            .Label(m => m.Category, "Category")
            .Label(m => m.Supplier, "Supplier")
            .Label(m => m.Price, "Sale Price ($)")
            .Label(m => m.Cost, "Cost ($)")
            .Label(m => m.QuantityInStock, "Quantity in Stock")
            .Label(m => m.MinimumStockLevel, "Minimum Stock Level")
            .Label(m => m.IsActive, "Active Status")
            .Remove(m => m.Id)
            .Remove(m => m.CreatedAt)
            .Remove(m => m.UpdatedAt)
            .Required(m => m.Name)
            .Validate<decimal>(m => m.Price, price =>
                (price >= 0, "Sale price cannot be negative"))
            .Validate<decimal>(m => m.Cost, cost =>
                (cost >= 0, "Cost cannot be negative"))
            .Validate<int>(m => m.QuantityInStock, quantity =>
                (quantity >= 0, "Quantity in stock cannot be negative"))
            .Validate<int>(m => m.MinimumStockLevel, minStock =>
                (minStock >= 0, "Minimum stock level cannot be negative"));
        
        var (onSubmit, formView, validationView, loading) = formBuilder.UseForm(this.Context);
        
        async ValueTask HandleSubmit()
        {
            if (await onSubmit())
            {
                try
                {
                    if (isEdit && existingProduct != null)
                    {
                        existingProduct.ProductCode = productForm.Value.ProductCode;
                        existingProduct.Name = productForm.Value.Name;
                        existingProduct.Description = productForm.Value.Description;
                        existingProduct.Price = productForm.Value.Price;
                        existingProduct.Cost = productForm.Value.Cost;
                        existingProduct.QuantityInStock = productForm.Value.QuantityInStock;
                        existingProduct.MinimumStockLevel = productForm.Value.MinimumStockLevel;
                        existingProduct.Category = productForm.Value.Category;
                        existingProduct.Supplier = productForm.Value.Supplier;
                        existingProduct.IsActive = productForm.Value.IsActive;
                        existingProduct.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        var productCode = string.IsNullOrWhiteSpace(productForm.Value.ProductCode) 
                            ? $"PROD-{DateTime.UtcNow:yyyyMMddHHmmss}" 
                            : productForm.Value.ProductCode;
                            
                        var newProduct = new Product
                        {
                            ProductCode = productCode,
                            Name = productForm.Value.Name,
                            Description = productForm.Value.Description,
                            Price = productForm.Value.Price,
                            Cost = productForm.Value.Cost,
                            QuantityInStock = productForm.Value.QuantityInStock,
                            MinimumStockLevel = productForm.Value.MinimumStockLevel,
                            Category = productForm.Value.Category,
                            Supplier = productForm.Value.Supplier,
                            IsActive = productForm.Value.IsActive,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        context.Products.Add(newProduct);
                    }
                    
                    context.SaveChanges();
                    client.Toast(isEdit ? "Product updated successfully!" : "Product created successfully!");
                    onClose?.Invoke();
                }
                catch (Exception ex)
                {
                    client.Toast($"Error: {ex.Message}", "Error");
                }
            }
        }
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button(isEdit ? "Save Changes" : "Create Product")
                    .Variant(ButtonVariant.Primary)
                    .Loading(loading)
                    .Disabled(loading)
                    .HandleClick(_ => HandleSubmit()))
                .Add(new Button("Cancel")
                    .Variant(ButtonVariant.Outline)
                    .Disabled(loading)
                    .HandleClick(_ => onClose?.Invoke()))
                .Add(validationView),
            formView
        );
    }
}

public class StockAdjustmentSheet(int productId, Action? onClose = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var product = context.Products.FirstOrDefault(p => p.Id == productId);
        
        if (product == null)
        {
            client.Toast("Product not found", "Error");
            onClose?.Invoke();
            return null;
        }
        
        var adjustmentForm = this.UseState(new StockMovement
        {
            ProductId = productId,
            MovementType = MovementType.In,
            Quantity = 1,
            Reference = "",
            Notes = "",
            MovementDate = DateTime.UtcNow
        });
        
        var movementTypeOptions = new[] { MovementType.In, MovementType.Out, MovementType.Transfer, MovementType.Adjustment };
        
        var formBuilder = adjustmentForm.ToForm("Adjust Stock")
            .Label(m => m.MovementType, "Movement Type")
            .Builder(m => m.MovementType, s => s.ToSelectInput(movementTypeOptions.ToOptions()))
            .Label(m => m.Quantity, "Quantity")
            .Label(m => m.Reference, "Reference")
            .Label(m => m.MovementDate, "Movement Date")
            .Builder(m => m.MovementDate, s => s.ToDateTimeInput())
            .Label(m => m.Notes, "Notes")
            .Builder(m => m.Notes, s => s.ToTextAreaInput())
            .Remove(m => m.Id)
            .Remove(m => m.ProductId)
            .Remove(m => m.CreatedAt)
            .Required(m => m.MovementType, m => m.Quantity, m => m.MovementDate)
            .Validate<int>(m => m.Quantity, quantity =>
                (quantity > 0, "Quantity must be at least 1"))
            .Validate<DateTime>(m => m.MovementDate, movementDate =>
                (movementDate <= DateTime.UtcNow.AddDays(1), "Movement date cannot be more than 1 day in the future"));
        
        var (onSubmit, formView, validationView, loading) = formBuilder.UseForm(this.Context);
        
        async ValueTask HandleSubmit()
        {
            if (await onSubmit())
            {
                try
                {
                    var dbProduct = context.Products.FirstOrDefault(p => p.Id == productId);
                    if (dbProduct != null)
                    {
                        // Check for insufficient stock on outgoing movements
                        if (adjustmentForm.Value.MovementType == MovementType.Out && dbProduct.QuantityInStock < adjustmentForm.Value.Quantity)
                        {
                            client.Toast("Insufficient stock quantity", "Validation Error");
                            return;
                        }
                        
                        // Adjust stock based on movement type
                        if (adjustmentForm.Value.MovementType == MovementType.In || adjustmentForm.Value.MovementType == MovementType.Adjustment)
                        {
                            dbProduct.QuantityInStock += adjustmentForm.Value.Quantity;
                        }
                        else if (adjustmentForm.Value.MovementType == MovementType.Out)
                        {
                            dbProduct.QuantityInStock -= adjustmentForm.Value.Quantity;
                        }
                        
                        // Record stock movement
                        var stockMovement = new StockMovement
                        {
                            ProductId = productId,
                            MovementType = adjustmentForm.Value.MovementType,
                            Quantity = adjustmentForm.Value.Quantity,
                            Reference = adjustmentForm.Value.Reference,
                            Notes = adjustmentForm.Value.Notes,
                            MovementDate = adjustmentForm.Value.MovementDate,
                            CreatedAt = DateTime.UtcNow
                        };
                        context.StockMovements.Add(stockMovement);
                        
                        dbProduct.UpdatedAt = DateTime.UtcNow;
                        context.SaveChanges();
                        
                        client.Toast($"Stock adjusted successfully! New quantity: {dbProduct.QuantityInStock}");
                        onClose?.Invoke();
                    }
                }
                catch (Exception ex)
                {
                    client.Toast($"Error: {ex.Message}", "Error");
                }
            }
        }
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button("Adjust Stock")
                    .Variant(ButtonVariant.Primary)
                    .Loading(loading)
                    .Disabled(loading)
                    .HandleClick(_ => HandleSubmit()))
                .Add(new Button("Cancel")
                    .Variant(ButtonVariant.Outline)
                    .Disabled(loading)
                    .HandleClick(_ => onClose?.Invoke()))
                .Add(validationView),
            
            Layout.Vertical().Gap(4)
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Current Stock Information"))
                        .Add(Text.Block($"Product: {product.Name}"))
                        .Add(Text.Block($"Current Stock: {product.QuantityInStock}"))
                        .Add(product.QuantityInStock == 0 
                            ? new Badge("Out of Stock").Variant(BadgeVariant.Destructive)
                            : product.QuantityInStock < product.MinimumStockLevel 
                            ? new Badge("Low Stock").Variant(BadgeVariant.Warning) 
                            : new Badge("In Stock").Variant(BadgeVariant.Success))
                ).Title("Current Status"))
                .Add(formView)
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("New Stock Level"))
                        .Add(Text.H3(adjustmentForm.Value.MovementType == MovementType.Out 
                            ? $"{product.QuantityInStock - adjustmentForm.Value.Quantity}" 
                            : $"{product.QuantityInStock + adjustmentForm.Value.Quantity}"))
                ).Title("Preview"))
        );
    }
}
