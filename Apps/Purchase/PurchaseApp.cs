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
            onClick: _ => blades.Push(this, new PurchaseOrderDetailBlade(order.Id), order.OrderNumber)
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

public class PurchaseOrderDetailBlade(int orderId) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var order = context.PurchaseOrders.FirstOrDefault(po => po.Id == orderId);
        
        if (order == null)
        {
            return Layout.Vertical()
                .Gap(4)
                .Add(Text.H3("Purchase Order Not Found"))
                .Add(new Button("Go Back", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary));
        }
        
        var currentStatus = this.UseState(order.Status);
        var isEditOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        this.UseEffect(() =>
        {
            // Update database when status changes
            var dbOrder = context.PurchaseOrders.FirstOrDefault(po => po.Id == orderId);
            if (dbOrder != null && dbOrder.Status != currentStatus.Value)
            {
                dbOrder.Status = currentStatus.Value;
                dbOrder.UpdatedAt = DateTime.UtcNow;
                context.SaveChanges();
                refreshToken.Refresh();
            }
        }, [currentStatus.ToTrigger()]);
        
        var statusBadge = new Badge(currentStatus.Value.ToString())
            .Variant(currentStatus.Value == PurchaseOrderStatus.Received ? BadgeVariant.Success :
                   currentStatus.Value == PurchaseOrderStatus.Approved ? BadgeVariant.Primary :
                   currentStatus.Value == PurchaseOrderStatus.Cancelled ? BadgeVariant.Destructive :
                   BadgeVariant.Secondary);

        var orderDetails = new
        {
            OrderNumber = order.OrderNumber,
            Supplier = order.Supplier,
            Amount = $"${order.Amount:N2}",
            Status = statusBadge,
            OrderDate = order.OrderDate.ToString("MMM dd, yyyy"),
            ExpectedDelivery = order.ExpectedDeliveryDate?.ToString("MMM dd, yyyy") ?? "Not set",
            PaymentTerms = order.PaymentTerms,
            Department = order.Department,
            Notes = order.Notes
        };
        
        return Layout.Vertical()
            .Gap(4)
            .Add(Text.H3($"Purchase Order {order.OrderNumber}"))
            .Add(orderDetails.ToDetails().RemoveEmpty().MultiLine(x => x.Notes))
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(currentStatus.Value == PurchaseOrderStatus.Pending 
                    ? new Button("Approve Order", _ => {
                        currentStatus.Set(PurchaseOrderStatus.Approved);
                        client.Toast($"Purchase Order {order.OrderNumber} approved!");
                    })
                        .Variant(ButtonVariant.Success)
                        .Icon(Icons.Check)
                    : null)
                .Add(currentStatus.Value == PurchaseOrderStatus.Approved 
                    ? new Button("Mark as Received", _ => {
                        currentStatus.Set(PurchaseOrderStatus.Received);
                        client.Toast($"Purchase Order {order.OrderNumber} marked as received!");
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
                                client.Toast($"Purchase Order {order.OrderNumber} deleted successfully!");
                                refreshToken.Refresh();
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
                }),
                title: "Edit Purchase Order",
                description: $"Edit purchase order {order.OrderNumber}"
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
        
        var orderNumber = this.UseState(existingOrder?.OrderNumber ?? $"PO-{DateTime.Now:yyyyMMdd-HHmmss}");
        var supplier = this.UseState(existingOrder?.Supplier ?? "");
        var amount = this.UseState(existingOrder?.Amount.ToString() ?? "0.00");
        var status = this.UseState(existingOrder?.Status ?? PurchaseOrderStatus.Pending);
        var orderDate = this.UseState(existingOrder?.OrderDate ?? DateTime.UtcNow);
        var expectedDelivery = this.UseState(existingOrder?.ExpectedDeliveryDate ?? DateTime.UtcNow.AddDays(30));
        var paymentTerms = this.UseState(existingOrder?.PaymentTerms ?? "Net 30");
        var department = this.UseState(existingOrder?.Department ?? "Operations");
        var notes = this.UseState(existingOrder?.Notes ?? "");
        
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
                                existingOrder.OrderNumber = orderNumber.Value;
                                existingOrder.Supplier = supplier.Value;
                                existingOrder.Amount = decimal.Parse(amount.Value);
                                existingOrder.Status = status.Value;
                                existingOrder.OrderDate = orderDate.Value;
                                existingOrder.ExpectedDeliveryDate = expectedDelivery.Value;
                                existingOrder.PaymentTerms = paymentTerms.Value;
                                existingOrder.Department = department.Value;
                                existingOrder.Notes = notes.Value;
                                existingOrder.UpdatedAt = DateTime.UtcNow;
                            }
                            else
                            {
                                // Create new order
                                var newOrder = new PurchaseOrder
                                {
                                    OrderNumber = orderNumber.Value,
                                    Supplier = supplier.Value,
                                    Amount = decimal.Parse(amount.Value),
                                    Status = status.Value,
                                    OrderDate = orderDate.Value,
                                    ExpectedDeliveryDate = expectedDelivery.Value,
                                    PaymentTerms = paymentTerms.Value,
                                    Department = department.Value,
                                    Notes = notes.Value,
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
                        .Add(orderNumber.ToTextInput().Placeholder("Order Number"))
                        .Add(Text.Small("Supplier"))
                        .Add(supplier.ToTextInput().Placeholder("Supplier Name"))
                        .Add(Text.Small("Amount ($)"))
                        .Add(amount.ToTextInput().Placeholder("0.00"))
                        .Add(Text.Small("Status"))
                        .Add(status.ToSelectInput(statusOptions.ToOptions()))
                        .Add(Text.Small("Department"))
                        .Add(department.ToSelectInput(departmentOptions.ToOptions()))
                ).Title("Order Details"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Dates & Terms"))
                        .Add(Text.Small("Order Date"))
                        .Add(orderDate.ToDateTimeInput())
                        .Add(Text.Small("Expected Delivery"))
                        .Add(expectedDelivery.ToDateTimeInput())
                        .Add(Text.Small("Payment Terms"))
                        .Add(paymentTerms.ToTextInput().Placeholder("Net 30"))
                ).Title("Timeline"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Additional Information"))
                        .Add(Text.Small("Notes"))
                        .Add(notes.ToTextInput().Placeholder("Additional notes...").Variant(TextInputs.Textarea))
                ).Title("Notes"))
        );
    }
}
