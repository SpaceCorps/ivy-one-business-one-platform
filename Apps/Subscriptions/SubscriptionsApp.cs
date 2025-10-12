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
        
        return BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(8)
                .Add(searchQuery.ToSearchInput().Placeholder("Search by customer, email, plan, or status..."))
                .Add(new Button("New Subscription")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .WithSheet(
                        () => new SubscriptionFormSheet(null, () => refreshToken.Refresh()),
                        title: "New Subscription",
                        description: "Create a new subscription",
                        width: Size.Fraction(1/3f)
                    )),
            subscriptions.Count == 0 
                ? Text.Block("No subscriptions found. Try a different search or create your first subscription!")
                : new List(listItems)
        );
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
        
        // Helper function to get current subscription from database
        Subscription? GetCurrentSubscription() => context.Subscriptions.FirstOrDefault(s => s.Id == subscriptionId);
        
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
                    var dbSubscription = GetCurrentSubscription();
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
                    var dbSubscription = GetCurrentSubscription();
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
                                        var subscriptionToDelete = GetCurrentSubscription();
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
        
        var formBuilder = subscriptionForm.ToForm(isEdit ? "Save Changes" : "Create Subscription")
            .Label(m => m.CustomerName, "Customer Name")
            .Label(m => m.CustomerEmail, "Customer Email")
            .Label(m => m.PlanName, "Plan Name")
            .Label(m => m.MonthlyAmount, "Monthly Amount ($)")
            .Label(m => m.BillingCycle, "Billing Cycle")
            .Builder(m => m.BillingCycle, s => s.ToSelectInput(billingCycleOptions.ToOptions()))
            .Label(m => m.PaymentMethod, "Payment Method")
            .Label(m => m.StartDate, "Start Date")
            .Builder(m => m.StartDate, s => s.ToDateTimeInput())
            .Label(m => m.NextBillingDate, "Next Billing Date")
            .Builder(m => m.NextBillingDate, s => s.ToDateTimeInput())
            .Label(m => m.Status, "Status")
            .Builder(m => m.Status, s => s.ToSelectInput(statusOptions.ToOptions()))
            .Remove(m => m.Id)
            .Remove(m => m.EndDate)
            .Remove(m => m.CreatedAt)
            .Remove(m => m.UpdatedAt)
            .Required(m => m.CustomerName, m => m.CustomerEmail, m => m.PlanName)
            .Validate<string>(m => m.CustomerName, name =>
                (name.Length >= 2, "Customer name must be at least 2 characters"))
            .Validate<string>(m => m.CustomerEmail, email =>
                (email.Contains("@") && email.Contains("."), "Please enter a valid email address"))
            .Validate<string>(m => m.PlanName, plan =>
                (plan.Length >= 3, "Plan name must be at least 3 characters"))
            .Validate<decimal>(m => m.MonthlyAmount, amount =>
                (amount >= 0, "Monthly amount cannot be negative"))
            .Validate<DateTime>(m => m.StartDate, startDate =>
                (startDate <= DateTime.UtcNow.AddYears(1), "Start date cannot be more than 1 year in the future"))
            .Validate<DateTime?>(m => m.NextBillingDate, nextBilling =>
                (nextBilling == null || nextBilling >= DateTime.UtcNow.AddDays(-1), "Next billing date cannot be in the past"));
        
        var (onSubmit, formView, validationView, loading) = formBuilder.UseForm(this.Context);
        
        async ValueTask HandleSubmit()
        {
            if (await onSubmit())
            {
                try
                {
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
            }
        }
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button(isEdit ? "Save Changes" : "Create Subscription")
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

