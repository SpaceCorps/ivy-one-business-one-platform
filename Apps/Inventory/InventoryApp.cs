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
        var searchQuery = this.UseState("");
        var isNewProductOpen = this.UseState(false);
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
            badge: prod.QuantityInStock < prod.MinimumStockLevel ? "Low Stock" : $"{prod.QuantityInStock}",
            onClick: _ => blades.Push(this, new ProductDetailBlade(prod.Id, () => refreshToken.Refresh()), prod.Name)
        ));
        
        var mainContent = BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(4)
                .Add(searchQuery.ToSearchInput().Placeholder("Search by code, name, category, or supplier..."))
                .Add(new Button("Add Product")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => isNewProductOpen.Set(true))),
            products.Count == 0 
                ? Text.Block("No products yet. Try a different search or add your first product!")
                : new List(listItems)
        );

        return isNewProductOpen.Value ? new Sheet(
            (Event<Sheet> _) => isNewProductOpen.Set(false),
            new ProductFormSheet(null, () => {
                isNewProductOpen.Set(false);
                refreshToken.Refresh();
            }),
            title: "New Product",
            description: "Add a new product to inventory"
        ).Width(Size.Fraction(1/3f)) : mainContent;
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
        var isEditOpen = this.UseState(false);
        var isStockAdjustOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        this.UseEffect(() =>
        {
            var updatedProduct = context.Products.FirstOrDefault(p => p.Id == productId);
            if (updatedProduct != null)
            {
                productData.Set(updatedProduct);
            }
        }, [refreshToken.ToTrigger()]);
        
        var stockBadge = new Badge(productData.Value.QuantityInStock < productData.Value.MinimumStockLevel ? "Low Stock" : "In Stock")
            .Variant(productData.Value.QuantityInStock < productData.Value.MinimumStockLevel ? BadgeVariant.Destructive : BadgeVariant.Success);

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
                    .HandleClick(_ => isStockAdjustOpen.Set(true)))
                .Add(new Button("Edit Product")
                    .Variant(ButtonVariant.Outline)
                    .Icon(Icons.Pencil)
                    .HandleClick(_ => isEditOpen.Set(true)))
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
                    .Variant(ButtonVariant.Secondary)))
            .Add(isEditOpen.Value ? new Sheet(
                (Event<Sheet> _) => isEditOpen.Set(false),
                new ProductFormSheet(productId, () => {
                    isEditOpen.Set(false);
                    refreshToken.Refresh();
                    onRefresh?.Invoke();
                }),
                title: "Edit Product",
                description: $"Edit product {productData.Value.Name}"
            ).Width(Size.Fraction(1/3f)) : null)
            .Add(isStockAdjustOpen.Value ? new Sheet(
                (Event<Sheet> _) => isStockAdjustOpen.Set(false),
                new StockAdjustmentSheet(productId, () => {
                    isStockAdjustOpen.Set(false);
                    refreshToken.Refresh();
                    onRefresh?.Invoke();
                }),
                title: "Adjust Stock",
                description: $"Adjust stock for {productData.Value.Name}"
            ).Width(Size.Fraction(1/3f)) : null);
    }
}

public class ProductFormSheet(int? productId = null, Action? onClose = null) : ViewBase
{
    private Product CloneProduct(Product source) => new Product
    {
        Id = source.Id,
        ProductCode = source.ProductCode,
        Name = source.Name,
        Description = source.Description,
        Price = source.Price,
        Cost = source.Cost,
        QuantityInStock = source.QuantityInStock,
        MinimumStockLevel = source.MinimumStockLevel,
        Category = source.Category,
        Supplier = source.Supplier,
        IsActive = source.IsActive,
        CreatedAt = source.CreatedAt,
        UpdatedAt = source.UpdatedAt
    };
    
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var isEdit = productId.HasValue;
        var existingProduct = isEdit ? context.Products.FirstOrDefault(p => p.Id == productId!.Value) : null;
        
        var productForm = this.UseState(existingProduct ?? new Product
        {
            ProductCode = "",
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
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(productForm.Value.ProductCode))
                            {
                                client.Toast("Product Code is required", "Validation Error");
                                return;
                            }
                            
                            if (string.IsNullOrWhiteSpace(productForm.Value.Name))
                            {
                                client.Toast("Product Name is required", "Validation Error");
                                return;
                            }
                            
                            if (productForm.Value.Price < 0 || productForm.Value.Cost < 0)
                            {
                                client.Toast("Price and Cost cannot be negative", "Validation Error");
                                return;
                            }
                            
                            if (productForm.Value.QuantityInStock < 0)
                            {
                                client.Toast("Quantity cannot be negative", "Validation Error");
                                return;
                            }
                            
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
                                var newProduct = new Product
                                {
                                    ProductCode = productForm.Value.ProductCode,
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
                    }))
                .Add(new Button("Cancel")
                    .Variant(ButtonVariant.Outline)
                    .HandleClick(_ => onClose?.Invoke())),
            
            Layout.Vertical().Gap(4)
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Product Information"))
                        .Add(Text.Small("Product Code"))
                        .Add(new TextInput(productForm.Value.ProductCode, e => {
                            var cloned = CloneProduct(productForm.Value);
                            cloned.ProductCode = e.Value;
                            productForm.Set(cloned);
                        }).Placeholder("PROD-001").Disabled(isEdit))
                        .Add(Text.Small("Product Name"))
                        .Add(new TextInput(productForm.Value.Name, e => {
                            var cloned = CloneProduct(productForm.Value);
                            cloned.Name = e.Value;
                            productForm.Set(cloned);
                        }).Placeholder("Product Name"))
                        .Add(Text.Small("Description"))
                        .Add(new TextInput(productForm.Value.Description, e => {
                            var cloned = CloneProduct(productForm.Value);
                            cloned.Description = e.Value;
                            productForm.Set(cloned);
                        }).Placeholder("Product description...").Variant(TextInputs.Textarea))
                        .Add(Text.Small("Category"))
                        .Add(new TextInput(productForm.Value.Category, e => {
                            var cloned = CloneProduct(productForm.Value);
                            cloned.Category = e.Value;
                            productForm.Set(cloned);
                        }).Placeholder("Electronics"))
                        .Add(Text.Small("Supplier"))
                        .Add(new TextInput(productForm.Value.Supplier, e => {
                            var cloned = CloneProduct(productForm.Value);
                            cloned.Supplier = e.Value;
                            productForm.Set(cloned);
                        }).Placeholder("Supplier Name"))
                ).Title("Product Details"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Pricing Information"))
                        .Add(Text.Small("Sale Price ($)"))
                        .Add(new NumberInput<decimal>(productForm.Value.Price, v => {
                            var cloned = CloneProduct(productForm.Value);
                            cloned.Price = v;
                            productForm.Set(cloned);
                        }).Placeholder("0.00"))
                        .Add(Text.Small("Cost ($)"))
                        .Add(new NumberInput<decimal>(productForm.Value.Cost, v => {
                            var cloned = CloneProduct(productForm.Value);
                            cloned.Cost = v;
                            productForm.Set(cloned);
                        }).Placeholder("0.00"))
                        .Add(Text.Small("Margin"))
                        .Add(Text.H3(productForm.Value.Price > 0 
                            ? $"{((productForm.Value.Price - productForm.Value.Cost) / productForm.Value.Price * 100):N2}%" 
                            : "0%"))
                ).Title("Pricing"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Inventory"))
                        .Add(Text.Small("Quantity in Stock"))
                        .Add(new NumberInput<int>(productForm.Value.QuantityInStock, v => {
                            var cloned = CloneProduct(productForm.Value);
                            cloned.QuantityInStock = v;
                            productForm.Set(cloned);
                        }).Placeholder("0"))
                        .Add(Text.Small("Minimum Stock Level"))
                        .Add(new NumberInput<int>(productForm.Value.MinimumStockLevel, v => {
                            var cloned = CloneProduct(productForm.Value);
                            cloned.MinimumStockLevel = v;
                            productForm.Set(cloned);
                        }).Placeholder("10"))
                        .Add(Text.Small("Status"))
                        .Add(new SelectInput<bool>(productForm.Value.IsActive, e => {
                            var cloned = CloneProduct(productForm.Value);
                            cloned.IsActive = e.Value;
                            productForm.Set(cloned);
                        }, new[] { (true, "Active"), (false, "Inactive") }.ToOptions()))
                ).Title("Stock"))
        );
    }
}

public class StockAdjustmentSheet(int productId, Action? onClose = null) : ViewBase
{
    private StockMovement CloneStockMovement(StockMovement source) => new StockMovement
    {
        Id = source.Id,
        ProductId = source.ProductId,
        MovementType = source.MovementType,
        Quantity = source.Quantity,
        Reference = source.Reference,
        Notes = source.Notes,
        MovementDate = source.MovementDate,
        CreatedAt = source.CreatedAt
    };
    
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
            MovementType = "In",
            Quantity = 1,
            Reference = "",
            Notes = "",
            MovementDate = DateTime.UtcNow
        });
        
        var movementTypeOptions = new[] { "In", "Out", "Transfer", "Adjustment" };
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => {
                        try
                        {
                            if (adjustmentForm.Value.Quantity <= 0)
                            {
                                client.Toast("Quantity must be at least 1", "Validation Error");
                                return;
                            }
                            
                            var dbProduct = context.Products.FirstOrDefault(p => p.Id == productId);
                            if (dbProduct != null)
                            {
                                // Adjust stock based on movement type
                                if (adjustmentForm.Value.MovementType == "In" || adjustmentForm.Value.MovementType == "Adjustment")
                                {
                                    dbProduct.QuantityInStock += adjustmentForm.Value.Quantity;
                                }
                                else if (adjustmentForm.Value.MovementType == "Out")
                                {
                                    if (dbProduct.QuantityInStock < adjustmentForm.Value.Quantity)
                                    {
                                        client.Toast("Insufficient stock quantity", "Validation Error");
                                        return;
                                    }
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
                    }))
                .Add(new Button("Cancel")
                    .Variant(ButtonVariant.Outline)
                    .HandleClick(_ => onClose?.Invoke())),
            
            Layout.Vertical().Gap(4)
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Current Stock Information"))
                        .Add(Text.Block($"Product: {product.Name}"))
                        .Add(Text.Block($"Current Stock: {product.QuantityInStock}"))
                        .Add(product.QuantityInStock < product.MinimumStockLevel 
                            ? new Badge("Low Stock").Variant(BadgeVariant.Destructive) 
                            : new Badge("In Stock").Variant(BadgeVariant.Success))
                ).Title("Current Status"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Stock Adjustment"))
                        .Add(Text.Small("Movement Type"))
                        .Add(new SelectInput<string>(adjustmentForm.Value.MovementType, e => {
                            var cloned = CloneStockMovement(adjustmentForm.Value);
                            cloned.MovementType = e.Value;
                            adjustmentForm.Set(cloned);
                        }, movementTypeOptions.ToOptions()))
                        .Add(Text.Small("Quantity"))
                        .Add(new NumberInput<int>(adjustmentForm.Value.Quantity, v => {
                            var cloned = CloneStockMovement(adjustmentForm.Value);
                            cloned.Quantity = v;
                            adjustmentForm.Set(cloned);
                        }).Placeholder("1"))
                        .Add(Text.Small("Reference"))
                        .Add(new TextInput(adjustmentForm.Value.Reference, e => {
                            var cloned = CloneStockMovement(adjustmentForm.Value);
                            cloned.Reference = e.Value;
                            adjustmentForm.Set(cloned);
                        }).Placeholder("PO-12345"))
                        .Add(Text.Small("Movement Date"))
                        .Add(new DateTimeInput<DateTime>(adjustmentForm.Value.MovementDate, e => {
                            var cloned = CloneStockMovement(adjustmentForm.Value);
                            cloned.MovementDate = e.Value;
                            adjustmentForm.Set(cloned);
                        }))
                        .Add(Text.Small("Notes"))
                        .Add(new TextInput(adjustmentForm.Value.Notes, e => {
                            var cloned = CloneStockMovement(adjustmentForm.Value);
                            cloned.Notes = e.Value;
                            adjustmentForm.Set(cloned);
                        }).Placeholder("Additional notes...").Variant(TextInputs.Textarea))
                        .Add(Text.Small("New Stock Level"))
                        .Add(Text.H3(adjustmentForm.Value.MovementType == "Out" 
                            ? $"{product.QuantityInStock - adjustmentForm.Value.Quantity}" 
                            : $"{product.QuantityInStock + adjustmentForm.Value.Quantity}"))
                ).Title("Adjustment Details"))
        );
    }
}
