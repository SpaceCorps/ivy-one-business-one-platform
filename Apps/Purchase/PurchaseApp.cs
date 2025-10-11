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
        var isNewPOOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
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
            onClick: _ => blades.Push(this, new PurchaseOrderDetailBlade(order.Id, () => refreshToken.Refresh()), order.OrderNumber)
        ));
        
        var mainContent = BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(4)
                .Add(searchQuery.ToSearchInput().Placeholder("Search orders by number, supplier, or department..."))
                .Add(new Button("New PO")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => isNewPOOpen.Set(true))),
            orders.Count == 0 
                ? Text.Block("No purchase orders found. Try a different search or create your first order!")
                : new List(listItems)
        );

        return isNewPOOpen.Value ? new Sheet(
            (Event<Sheet> _) => isNewPOOpen.Set(false),
            new PurchaseOrderFormSheet(null, () => {
                isNewPOOpen.Set(false);
                refreshToken.Refresh();
            }),
            title: "New Purchase Order",
            description: "Create a new purchase order"
            ).Width(Size.Fraction(1/3f)) : mainContent;
    }
}

public class PurchaseOrderDetailBlade(int orderId, Action? onRefresh = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var initialOrder = context.PurchaseOrders.FirstOrDefault(po => po.Id == orderId);
        
        if (initialOrder == null)
        {
            return Layout.Vertical()
                .Gap(4)
                .Add(Text.H3("Purchase Order Not Found"))
                .Add(new Button("Go Back", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary));
        }
        
        var orderData = this.UseState(initialOrder);
        var isEditOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        // Refresh order data when refresh token changes
        this.UseEffect(() =>
        {
            var updatedOrder = context.PurchaseOrders.FirstOrDefault(po => po.Id == orderId);
            if (updatedOrder != null)
            {
                orderData.Set(updatedOrder);
            }
        }, [refreshToken.ToTrigger()]);
        
        var statusBadge = new Badge(orderData.Value.Status.ToString())
            .Variant(orderData.Value.Status == PurchaseOrderStatus.Received ? BadgeVariant.Success :
                   orderData.Value.Status == PurchaseOrderStatus.Approved ? BadgeVariant.Primary :
                   orderData.Value.Status == PurchaseOrderStatus.Cancelled ? BadgeVariant.Destructive :
                   BadgeVariant.Secondary);

        var orderDetails = new
        {
            OrderNumber = orderData.Value.OrderNumber,
            Supplier = orderData.Value.Supplier,
            Amount = $"${orderData.Value.Amount:N2}",
            Status = statusBadge,
            OrderDate = orderData.Value.OrderDate.ToString("MMM dd, yyyy"),
            ExpectedDelivery = orderData.Value.ExpectedDeliveryDate?.ToString("MMM dd, yyyy") ?? "Not set",
            PaymentTerms = orderData.Value.PaymentTerms,
            Department = orderData.Value.Department,
            Notes = orderData.Value.Notes
        };
        
        return Layout.Vertical()
            .Gap(4)
            .Add(Text.H3($"Purchase Order {orderData.Value.OrderNumber}"))
            .Add(orderDetails.ToDetails().RemoveEmpty().MultiLine(x => x.Notes))
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(orderData.Value.Status == PurchaseOrderStatus.Pending 
                    ? new Button("Approve Order", _ => {
                        var dbOrder = context.PurchaseOrders.FirstOrDefault(po => po.Id == orderId);
                        if (dbOrder != null)
                        {
                            dbOrder.Status = PurchaseOrderStatus.Approved;
                            dbOrder.UpdatedAt = DateTime.UtcNow;
                            context.SaveChanges();
                            client.Toast($"Purchase Order {orderData.Value.OrderNumber} approved!");
                            refreshToken.Refresh();
                            onRefresh?.Invoke();
                        }
                    })
                        .Variant(ButtonVariant.Success)
                        .Icon(Icons.Check)
                    : null)
                .Add(orderData.Value.Status == PurchaseOrderStatus.Approved 
                    ? new Button("Mark as Received", _ => {
                        var dbOrder = context.PurchaseOrders.FirstOrDefault(po => po.Id == orderId);
                        if (dbOrder != null)
                        {
                            dbOrder.Status = PurchaseOrderStatus.Received;
                            dbOrder.UpdatedAt = DateTime.UtcNow;
                            context.SaveChanges();
                            client.Toast($"Purchase Order {orderData.Value.OrderNumber} marked as received!");
                            refreshToken.Refresh();
                            onRefresh?.Invoke();
                        }
                    })
                        .Variant(ButtonVariant.Primary)
                        .Icon(Icons.Package)
                    : null)
                .Add(new Button("Edit Order")
                    .Variant(ButtonVariant.Outline)
                    .Icon(Icons.Pencil)
                    .HandleClick(_ => isEditOpen.Set(true)))
                .Add(new Button("Delete Order")
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.Trash)
                    .HandleClick(_ => {
                        try
                        {
                            var orderToDelete = context.PurchaseOrders.FirstOrDefault(po => po.Id == orderId);
                            if (orderToDelete != null)
                            {
                                context.PurchaseOrders.Remove(orderToDelete);
                                context.SaveChanges();
                                client.Toast($"Purchase Order {orderData.Value.OrderNumber} deleted successfully!");
                                refreshToken.Refresh();
                                onRefresh?.Invoke();
                                blades.Pop();
                            }
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error deleting order: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary)))
            .Add(isEditOpen.Value ? new Sheet(
                (Event<Sheet> _) => isEditOpen.Set(false),
                new PurchaseOrderFormSheet(orderId, () => {
                    isEditOpen.Set(false);
                    refreshToken.Refresh();
                    onRefresh?.Invoke();
                }),
                title: "Edit Purchase Order",
                description: $"Edit purchase order {orderData.Value.OrderNumber}"
            ).Width(Size.Fraction(1/3f)) : null);
    }
}

public class PurchaseOrderFormSheet(int? orderId = null, Action? onClose = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var isEdit = orderId.HasValue;
        var existingOrder = isEdit ? context.PurchaseOrders.FirstOrDefault(po => po.Id == orderId!.Value) : null;
        
        var orderForm = this.UseState(existingOrder ?? new PurchaseOrder
        {
            OrderNumber = $"PO-{DateTime.Now:yyyyMMdd-HHmmss}",
            Supplier = "",
            Amount = 0.00m,
            Status = PurchaseOrderStatus.Pending,
            OrderDate = DateTime.UtcNow,
            ExpectedDeliveryDate = DateTime.UtcNow.AddDays(30),
            PaymentTerms = "Net 30",
            Department = "Operations",
            Notes = ""
        });
        
        var statusOptions = new[]
        {
            PurchaseOrderStatus.Pending,
            PurchaseOrderStatus.Approved,
            PurchaseOrderStatus.Received,
            PurchaseOrderStatus.Cancelled
        };
        
        var departmentOptions = new[]
        {
            "Operations", "IT", "Marketing", "Sales", "HR", "Finance", "Legal"
        };
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => {
                        try
                        {
                            if (isEdit && existingOrder != null)
                            {
                                // Update existing order
                                existingOrder.OrderNumber = orderForm.Value.OrderNumber;
                                existingOrder.Supplier = orderForm.Value.Supplier;
                                existingOrder.Amount = orderForm.Value.Amount;
                                existingOrder.Status = orderForm.Value.Status;
                                existingOrder.OrderDate = orderForm.Value.OrderDate;
                                existingOrder.ExpectedDeliveryDate = orderForm.Value.ExpectedDeliveryDate;
                                existingOrder.PaymentTerms = orderForm.Value.PaymentTerms;
                                existingOrder.Department = orderForm.Value.Department;
                                existingOrder.Notes = orderForm.Value.Notes;
                                existingOrder.UpdatedAt = DateTime.UtcNow;
                            }
                            else
                            {
                                // Create new order
                                var newOrder = new PurchaseOrder
                                {
                                    OrderNumber = orderForm.Value.OrderNumber,
                                    Supplier = orderForm.Value.Supplier,
                                    Amount = orderForm.Value.Amount,
                                    Status = orderForm.Value.Status,
                                    OrderDate = orderForm.Value.OrderDate,
                                    ExpectedDeliveryDate = orderForm.Value.ExpectedDeliveryDate,
                                    PaymentTerms = orderForm.Value.PaymentTerms,
                                    Department = orderForm.Value.Department,
                                    Notes = orderForm.Value.Notes,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                context.PurchaseOrders.Add(newOrder);
                            }
                            
                            context.SaveChanges();
                            client.Toast(isEdit ? "Purchase order updated successfully!" : "Purchase order created successfully!");
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
                        .Add(Text.Small("Basic Information"))
                        .Add(Text.Small("Order Number"))
                        .Add(new TextInput(orderForm.Value.OrderNumber, e => {
                            var updated = orderForm.Value;
                            updated.OrderNumber = e.Value;
                            orderForm.Set(updated);
                        }).Placeholder("Order Number"))
                        .Add(Text.Small("Supplier"))
                        .Add(new TextInput(orderForm.Value.Supplier, e => {
                            var updated = orderForm.Value;
                            updated.Supplier = e.Value;
                            orderForm.Set(updated);
                        }).Placeholder("Supplier Name"))
                        .Add(Text.Small("Amount ($)"))
                        .Add(new NumberInput<decimal>(orderForm.Value.Amount, v => {
                            var updated = orderForm.Value;
                            updated.Amount = v;
                            orderForm.Set(updated);
                        }).Placeholder("0.00"))
                        .Add(Text.Small("Status"))
                        .Add(new SelectInput<PurchaseOrderStatus>(orderForm.Value.Status, e => {
                            var updated = orderForm.Value;
                            updated.Status = e.Value;
                            orderForm.Set(updated);
                        }, statusOptions.ToOptions()))
                        .Add(Text.Small("Department"))
                        .Add(new SelectInput<string>(orderForm.Value.Department, e => {
                            var updated = orderForm.Value;
                            updated.Department = e.Value;
                            orderForm.Set(updated);
                        }, departmentOptions.ToOptions()))
                ).Title("Order Details"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Dates & Terms"))
                        .Add(Text.Small("Order Date"))
                        .Add(new DateTimeInput<DateTime>(orderForm.Value.OrderDate, e => {
                            var updated = orderForm.Value;
                            updated.OrderDate = e.Value;
                            orderForm.Set(updated);
                        }))
                        .Add(Text.Small("Expected Delivery"))
                        .Add(new DateTimeInput<DateTime>(orderForm.Value.ExpectedDeliveryDate ?? DateTime.UtcNow, e => {
                            var updated = orderForm.Value;
                            updated.ExpectedDeliveryDate = e.Value;
                            orderForm.Set(updated);
                        }))
                        .Add(Text.Small("Payment Terms"))
                        .Add(new TextInput(orderForm.Value.PaymentTerms, e => {
                            var updated = orderForm.Value;
                            updated.PaymentTerms = e.Value;
                            orderForm.Set(updated);
                        }).Placeholder("Net 30"))
                ).Title("Timeline"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Additional Information"))
                        .Add(Text.Small("Notes"))
                        .Add(new TextInput(orderForm.Value.Notes, e => {
                            var updated = orderForm.Value;
                            updated.Notes = e.Value;
                            orderForm.Set(updated);
                        }).Placeholder("Additional notes...").Variant(TextInputs.Textarea))
                ).Title("Notes"))
        );
    }
}
