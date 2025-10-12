using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Subscriptions;

[App(icon: Icons.RefreshCw, title: "Subscriptions", path: new[] { "Commerce & Services" })]
public class SubscriptionsApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new SubscriptionsRootBlade(), "Subscriptions", Size.Units(110));
    }
}

public class SubscriptionsRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var searchQuery = this.UseState("");
        var isNewSubscriptionOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        var query = context.Subscriptions.AsQueryable();
        
        if (!string.IsNullOrEmpty(searchQuery.Value))
        {
            var searchPattern = $"%{searchQuery.Value}%";
            query = query.Where(s => 
                EF.Functions.Like(s.CustomerName, searchPattern) ||
                EF.Functions.Like(s.CustomerEmail, searchPattern) ||
                EF.Functions.Like(s.PlanName, searchPattern) ||
                EF.Functions.Like(s.Status, searchPattern));
        }
        
        var subscriptions = query.ToList();
        
        var listItems = subscriptions.Select(sub => new ListItem(
            title: $"{sub.CustomerName} - {sub.PlanName}",
            subtitle: $"${sub.MonthlyAmount:N2}/{sub.BillingCycle} - Next: {sub.NextBillingDate:MMM dd}",
            icon: Icons.RefreshCw,
            badge: sub.Status,
            onClick: _ => blades.Push(this, new SubscriptionDetailBlade(sub.Id, () => refreshToken.Refresh()), sub.CustomerName)
        ));
        
        var mainContent = BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(4)
                .Add(searchQuery.ToSearchInput().Placeholder("Search by customer, email, plan, or status..."))
                .Add(new Button("New Subscription")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => isNewSubscriptionOpen.Set(true))),
            subscriptions.Count == 0 
                ? Text.Block("No subscriptions found. Try a different search or create your first subscription!")
                : new List(listItems)
        );

        return isNewSubscriptionOpen.Value ? new Sheet(
            (Event<Sheet> _) => isNewSubscriptionOpen.Set(false),
            new SubscriptionFormSheet(null, () => {
                isNewSubscriptionOpen.Set(false);
                refreshToken.Refresh();
            }),
            title: "New Subscription",
            description: "Create a new subscription"
        ).Width(Size.Fraction(1/3f)) : mainContent;
    }
}

public class SubscriptionDetailBlade(int subscriptionId, Action? onRefresh = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var (alertView, showAlert) = this.UseAlert();
        
        var initialSubscription = context.Subscriptions.FirstOrDefault(s => s.Id == subscriptionId);
        
        if (initialSubscription == null)
        {
            return Layout.Vertical()
                .Gap(4)
                .Add(Text.H3("Subscription Not Found"))
                .Add(new Button("Go Back", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary));
        }
        
        var subscriptionData = this.UseState(initialSubscription);
        var isEditOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        this.UseEffect(() =>
        {
            var updatedSubscription = context.Subscriptions.FirstOrDefault(s => s.Id == subscriptionId);
            if (updatedSubscription != null)
            {
                subscriptionData.Set(updatedSubscription);
            }
        }, [refreshToken.ToTrigger()]);
        
        var statusBadge = new Badge(subscriptionData.Value.Status)
            .Variant(subscriptionData.Value.Status == "Active" ? BadgeVariant.Success :
                   subscriptionData.Value.Status == "Cancelled" ? BadgeVariant.Destructive :
                   BadgeVariant.Warning);

        var subscriptionDetails = new
        {
            CustomerName = subscriptionData.Value.CustomerName,
            CustomerEmail = subscriptionData.Value.CustomerEmail,
            PlanName = subscriptionData.Value.PlanName,
            MonthlyAmount = $"${subscriptionData.Value.MonthlyAmount:N2}",
            BillingCycle = subscriptionData.Value.BillingCycle,
            PaymentMethod = subscriptionData.Value.PaymentMethod,
            StartDate = subscriptionData.Value.StartDate.ToString("MMM dd, yyyy"),
            EndDate = subscriptionData.Value.EndDate?.ToString("MMM dd, yyyy"),
            NextBillingDate = subscriptionData.Value.NextBillingDate?.ToString("MMM dd, yyyy"),
            Status = statusBadge
        };
        
        return Layout.Vertical()
            .Gap(4)
            .Add(Text.H3($"{subscriptionData.Value.CustomerName} - {subscriptionData.Value.PlanName}"))
            .Add(subscriptionDetails.ToDetails().RemoveEmpty())
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(subscriptionData.Value.Status == "Active" ? new Button("Cancel Subscription", _ => {
                    var dbSubscription = context.Subscriptions.FirstOrDefault(s => s.Id == subscriptionId);
                    if (dbSubscription != null)
                    {
                        dbSubscription.Status = "Cancelled";
                        dbSubscription.EndDate = DateTime.UtcNow;
                        dbSubscription.UpdatedAt = DateTime.UtcNow;
                        context.SaveChanges();
                        client.Toast($"Subscription for {subscriptionData.Value.CustomerName} cancelled!");
                        refreshToken.Refresh();
                        onRefresh?.Invoke();
                    }
                })
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.X)
                    : null)
                .Add(subscriptionData.Value.Status == "Paused" ? new Button("Reactivate", _ => {
                    var dbSubscription = context.Subscriptions.FirstOrDefault(s => s.Id == subscriptionId);
                    if (dbSubscription != null)
                    {
                        dbSubscription.Status = "Active";
                        dbSubscription.UpdatedAt = DateTime.UtcNow;
                        context.SaveChanges();
                        client.Toast($"Subscription for {subscriptionData.Value.CustomerName} reactivated!");
                        refreshToken.Refresh();
                        onRefresh?.Invoke();
                    }
                })
                    .Variant(ButtonVariant.Success)
                    .Icon(Icons.Check)
                    : null)
                .Add(new Button("Edit Subscription")
                    .Variant(ButtonVariant.Outline)
                    .Icon(Icons.Pencil)
                    .HandleClick(_ => isEditOpen.Set(true)))
                .Add(new Button("Delete Subscription")
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.Trash)
                    .HandleClick(_ => {
                        showAlert(
                            $"Are you sure you want to permanently delete the subscription for {subscriptionData.Value.CustomerName}? This action cannot be undone.",
                            result => {
                                if (result == AlertResult.Ok)
                                {
                                    try
                                    {
                                        var subscriptionToDelete = context.Subscriptions.FirstOrDefault(s => s.Id == subscriptionId);
                                        if (subscriptionToDelete != null)
                                        {
                                            context.Subscriptions.Remove(subscriptionToDelete);
                                            context.SaveChanges();
                                            client.Toast($"Subscription for {subscriptionData.Value.CustomerName} deleted successfully!");
                                            refreshToken.Refresh();
                                            onRefresh?.Invoke();
                                            blades.Pop();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        client.Toast($"Error deleting subscription: {ex.Message}", "Error");
                                    }
                                }
                            },
                            "Confirm Deletion"
                        );
                    }))
                .Add(new Button("Cancel", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary)))
            .Add(alertView)
            .Add(isEditOpen.Value ? new Sheet(
                (Event<Sheet> _) => isEditOpen.Set(false),
                new SubscriptionFormSheet(subscriptionId, () => {
                    isEditOpen.Set(false);
                    refreshToken.Refresh();
                    onRefresh?.Invoke();
                }),
                title: "Edit Subscription",
                description: $"Edit subscription for {subscriptionData.Value.CustomerName}"
            ).Width(Size.Fraction(1/3f)) : null);
    }
}

public class SubscriptionFormSheet(int? subscriptionId = null, Action? onClose = null) : ViewBase
{
    private Subscription CloneSubscription(Subscription source) => new Subscription
    {
        Id = source.Id,
        CustomerName = source.CustomerName,
        CustomerEmail = source.CustomerEmail,
        PlanName = source.PlanName,
        MonthlyAmount = source.MonthlyAmount,
        BillingCycle = source.BillingCycle,
        Status = source.Status,
        StartDate = source.StartDate,
        EndDate = source.EndDate,
        NextBillingDate = source.NextBillingDate,
        PaymentMethod = source.PaymentMethod,
        CreatedAt = source.CreatedAt,
        UpdatedAt = source.UpdatedAt
    };
    
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var isEdit = subscriptionId.HasValue;
        var existingSubscription = isEdit ? context.Subscriptions.FirstOrDefault(s => s.Id == subscriptionId!.Value) : null;
        
        var subscriptionForm = this.UseState(existingSubscription ?? new Subscription
        {
            CustomerName = "",
            CustomerEmail = "",
            PlanName = "",
            MonthlyAmount = 0.00m,
            BillingCycle = "Monthly",
            Status = "Active",
            StartDate = DateTime.UtcNow,
            NextBillingDate = DateTime.UtcNow.AddMonths(1),
            PaymentMethod = ""
        });
        
        var statusOptions = new[] { "Active", "Cancelled", "Paused", "Expired" };
        var billingCycleOptions = new[] { "Monthly", "Quarterly", "Yearly" };
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(subscriptionForm.Value.CustomerName))
                            {
                                client.Toast("Customer Name is required", "Validation Error");
                                return;
                            }
                            
                            if (string.IsNullOrWhiteSpace(subscriptionForm.Value.CustomerEmail))
                            {
                                client.Toast("Customer Email is required", "Validation Error");
                                return;
                            }
                            
                            if (string.IsNullOrWhiteSpace(subscriptionForm.Value.PlanName))
                            {
                                client.Toast("Plan Name is required", "Validation Error");
                                return;
                            }
                            
                            if (subscriptionForm.Value.MonthlyAmount < 0)
                            {
                                client.Toast("Monthly Amount cannot be negative", "Validation Error");
                                return;
                            }
                            
                            if (isEdit && existingSubscription != null)
                            {
                                existingSubscription.CustomerName = subscriptionForm.Value.CustomerName;
                                existingSubscription.CustomerEmail = subscriptionForm.Value.CustomerEmail;
                                existingSubscription.PlanName = subscriptionForm.Value.PlanName;
                                existingSubscription.MonthlyAmount = subscriptionForm.Value.MonthlyAmount;
                                existingSubscription.BillingCycle = subscriptionForm.Value.BillingCycle;
                                existingSubscription.Status = subscriptionForm.Value.Status;
                                existingSubscription.StartDate = subscriptionForm.Value.StartDate;
                                existingSubscription.NextBillingDate = subscriptionForm.Value.NextBillingDate;
                                existingSubscription.PaymentMethod = subscriptionForm.Value.PaymentMethod;
                                existingSubscription.UpdatedAt = DateTime.UtcNow;
                            }
                            else
                            {
                                var newSubscription = new Subscription
                                {
                                    CustomerName = subscriptionForm.Value.CustomerName,
                                    CustomerEmail = subscriptionForm.Value.CustomerEmail,
                                    PlanName = subscriptionForm.Value.PlanName,
                                    MonthlyAmount = subscriptionForm.Value.MonthlyAmount,
                                    BillingCycle = subscriptionForm.Value.BillingCycle,
                                    Status = subscriptionForm.Value.Status,
                                    StartDate = subscriptionForm.Value.StartDate,
                                    NextBillingDate = subscriptionForm.Value.NextBillingDate,
                                    PaymentMethod = subscriptionForm.Value.PaymentMethod,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                context.Subscriptions.Add(newSubscription);
                            }
                            
                            context.SaveChanges();
                            client.Toast(isEdit ? "Subscription updated successfully!" : "Subscription created successfully!");
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
                        .Add(Text.Small("Customer Information"))
                        .Add(Text.Small("Customer Name"))
                        .Add(new TextInput(subscriptionForm.Value.CustomerName, e => {
                            var cloned = CloneSubscription(subscriptionForm.Value);
                            cloned.CustomerName = e.Value;
                            subscriptionForm.Set(cloned);
                        }).Placeholder("John Doe"))
                        .Add(Text.Small("Customer Email"))
                        .Add(new TextInput(subscriptionForm.Value.CustomerEmail, e => {
                            var cloned = CloneSubscription(subscriptionForm.Value);
                            cloned.CustomerEmail = e.Value;
                            subscriptionForm.Set(cloned);
                        }).Placeholder("customer@example.com"))
                ).Title("Customer"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Plan Information"))
                        .Add(Text.Small("Plan Name"))
                        .Add(new TextInput(subscriptionForm.Value.PlanName, e => {
                            var cloned = CloneSubscription(subscriptionForm.Value);
                            cloned.PlanName = e.Value;
                            subscriptionForm.Set(cloned);
                        }).Placeholder("Premium Plan"))
                        .Add(Text.Small("Monthly Amount ($)"))
                        .Add(new NumberInput<decimal>(subscriptionForm.Value.MonthlyAmount, v => {
                            var cloned = CloneSubscription(subscriptionForm.Value);
                            cloned.MonthlyAmount = v;
                            subscriptionForm.Set(cloned);
                        }).Placeholder("0.00"))
                        .Add(Text.Small("Billing Cycle"))
                        .Add(new SelectInput<string>(subscriptionForm.Value.BillingCycle, e => {
                            var cloned = CloneSubscription(subscriptionForm.Value);
                            cloned.BillingCycle = e.Value;
                            subscriptionForm.Set(cloned);
                        }, billingCycleOptions.ToOptions()))
                        .Add(Text.Small("Payment Method"))
                        .Add(new TextInput(subscriptionForm.Value.PaymentMethod, e => {
                            var cloned = CloneSubscription(subscriptionForm.Value);
                            cloned.PaymentMethod = e.Value;
                            subscriptionForm.Set(cloned);
                        }).Placeholder("Credit Card"))
                ).Title("Plan Details"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Subscription Timeline"))
                        .Add(Text.Small("Start Date"))
                        .Add(new DateTimeInput<DateTime>(subscriptionForm.Value.StartDate, e => {
                            var cloned = CloneSubscription(subscriptionForm.Value);
                            cloned.StartDate = e.Value;
                            subscriptionForm.Set(cloned);
                        }))
                        .Add(Text.Small("Next Billing Date"))
                        .Add(new DateTimeInput<DateTime?>(subscriptionForm.Value.NextBillingDate, e => {
                            var cloned = CloneSubscription(subscriptionForm.Value);
                            cloned.NextBillingDate = e.Value;
                            subscriptionForm.Set(cloned);
                        }))
                        .Add(Text.Small("Status"))
                        .Add(new SelectInput<string>(subscriptionForm.Value.Status, e => {
                            var cloned = CloneSubscription(subscriptionForm.Value);
                            cloned.Status = e.Value;
                            subscriptionForm.Set(cloned);
                        }, statusOptions.ToOptions()))
                ).Title("Timeline"))
        );
    }
}

