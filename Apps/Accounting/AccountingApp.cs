using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Accounting;

[App(icon: Icons.Percent, title: "Accounting", path: new[] { "Business Operations" })]
public class AccountingApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new AccountingRootBlade(), "Accounting", Size.Units(110));
    }
}

public class AccountingRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var invoices = context.Invoices.ToList();
        var accounts = context.Accounts.ToList();
        var transactions = context.Transactions.ToList();

        var totalRevenue = invoices.Where(i => i.Status == "Paid").Sum(i => i.TotalAmount);
        var pendingInvoices = invoices.Where(i => i.Status != "Paid").Sum(i => i.TotalAmount);
        
        var menuItems = new[]
        {
            new ListItem("Invoices", 
                subtitle: $"{invoices.Count} total, ${pendingInvoices:N2} pending",
                icon: Icons.FileText,
                badge: invoices.Count.ToString(),
                onClick: _ => blades.Push(this, new InvoicesBlade(), "Invoices")),
            new ListItem("Chart of Accounts",
                subtitle: $"{accounts.Count} accounts, ${accounts.Sum(a => a.Balance):N2} total",
                icon: Icons.Book,
                badge: accounts.Count.ToString(),
                onClick: _ => blades.Push(this, new AccountsBlade(), "Accounts")),
            new ListItem("Transactions",
                subtitle: $"{transactions.Count} entries",
                icon: Icons.ArrowLeftRight,
                badge: transactions.Count.ToString(),
                onClick: _ => blades.Push(this, new TransactionsBlade(), "Transactions"))
        };
        
        return Layout.Vertical()
            .Gap(24)
            .Padding(24)
            .Add(new Card(
                Layout.Vertical()
                    .Gap(16)
                    .Padding(16)
                    .Add(Text.H3("Financial Dashboard"))
                    .Add(Layout.Grid()
                        .Columns(2)
                        .Gap(12)
                        .Add(new Card(Layout.Vertical().Gap(4).Padding(12)
                            .Add("Total Revenue").Add($"${totalRevenue:N2}").Add("Paid invoices")))
                        .Add(new Card(Layout.Vertical().Gap(4).Padding(12)
                            .Add("Pending").Add($"${pendingInvoices:N2}").Add("Unpaid amount"))))))
            .Add(new List(menuItems));
    }
}

public class InvoicesBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var searchQuery = this.UseState("");
        var isNewInvoiceOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        var query = context.Invoices.AsQueryable();
        
        if (!string.IsNullOrEmpty(searchQuery.Value))
        {
            var searchPattern = $"%{searchQuery.Value}%";
            query = query.Where(i => 
                EF.Functions.Like(i.InvoiceNumber, searchPattern) ||
                EF.Functions.Like(i.CustomerName, searchPattern) ||
                EF.Functions.Like(i.CustomerEmail, searchPattern) ||
                EF.Functions.Like(i.Status, searchPattern));
        }
        
        var invoices = query.OrderByDescending(i => i.IssueDate).ToList();
        
        var listItems = invoices.Select(invoice => new ListItem(
            title: $"#{invoice.InvoiceNumber} - {invoice.CustomerName}",
            subtitle: $"${invoice.TotalAmount:N2} - Due: {invoice.DueDate:MMM dd, yyyy}",
            icon: Icons.FileText,
            badge: invoice.Status,
            onClick: _ => blades.Push(this, new InvoiceDetailBlade(invoice.Id, () => refreshToken.Refresh()), $"Invoice {invoice.InvoiceNumber}")
        ));
        
        var mainContent = BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(4)
                .Add(searchQuery.ToSearchInput().Placeholder("Search by invoice number, customer, email, or status..."))
                .Add(new Button("Create Invoice")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => isNewInvoiceOpen.Set(true))),
            invoices.Count == 0 
                ? Text.Block("No invoices found. Try a different search or create your first invoice!")
                : new List(listItems)
        );

        return isNewInvoiceOpen.Value ? new Sheet(
            (Event<Sheet> _) => isNewInvoiceOpen.Set(false),
            new InvoiceFormSheet(null, () => {
                isNewInvoiceOpen.Set(false);
                refreshToken.Refresh();
            }),
            title: "New Invoice",
            description: "Create a new invoice"
        ).Width(Size.Fraction(1/3f)) : mainContent;
    }
}

public class InvoiceDetailBlade(int invoiceId, Action? onRefresh = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var initialInvoice = context.Invoices.Include(i => i.Payments).FirstOrDefault(i => i.Id == invoiceId);
        
        if (initialInvoice == null)
        {
            return Layout.Vertical()
                .Gap(4)
                .Add(Text.H3("Invoice Not Found"))
                .Add(new Button("Go Back", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary));
        }
        
        var invoiceData = this.UseState(initialInvoice);
        var isEditOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        // Refresh invoice data when refresh token changes
        this.UseEffect(() =>
        {
            var updatedInvoice = context.Invoices.Include(i => i.Payments).FirstOrDefault(i => i.Id == invoiceId);
            if (updatedInvoice != null)
            {
                invoiceData.Set(updatedInvoice);
            }
        }, [refreshToken.ToTrigger()]);
        
        var statusBadge = new Badge(invoiceData.Value.Status)
            .Variant(invoiceData.Value.Status == "Paid" ? BadgeVariant.Success :
                   invoiceData.Value.Status == "Overdue" ? BadgeVariant.Destructive :
                   invoiceData.Value.Status == "Sent" ? BadgeVariant.Primary :
                   BadgeVariant.Secondary);

        var invoiceDetails = new
        {
            InvoiceNumber = $"#{invoiceData.Value.InvoiceNumber}",
            Customer = invoiceData.Value.CustomerName,
            Email = invoiceData.Value.CustomerEmail,
            IssueDate = invoiceData.Value.IssueDate.ToString("MMM dd, yyyy"),
            DueDate = invoiceData.Value.DueDate.ToString("MMM dd, yyyy"),
            Amount = $"${invoiceData.Value.Amount:N2}",
            TaxAmount = $"${invoiceData.Value.TaxAmount:N2}",
            TotalAmount = $"${invoiceData.Value.TotalAmount:N2}",
            Status = statusBadge,
            Description = invoiceData.Value.Description
        };
        
        return Layout.Vertical()
            .Gap(4)
            .Add(Text.H3($"Invoice {invoiceData.Value.InvoiceNumber}"))
            .Add(invoiceDetails.ToDetails().RemoveEmpty().MultiLine(x => x.Description))
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(invoiceData.Value.Status != "Paid" 
                    ? new Button("Mark as Paid", _ => {
                        var dbInvoice = context.Invoices.FirstOrDefault(i => i.Id == invoiceId);
                        if (dbInvoice != null)
                        {
                            dbInvoice.Status = "Paid";
                            dbInvoice.UpdatedAt = DateTime.UtcNow;
                            context.SaveChanges();
                            client.Toast($"Invoice {invoiceData.Value.InvoiceNumber} marked as paid!");
                            refreshToken.Refresh();
                            onRefresh?.Invoke();
                        }
                    })
                        .Variant(ButtonVariant.Success)
                        .Icon(Icons.Check)
                    : null)
                .Add(invoiceData.Value.Status == "Draft" 
                    ? new Button("Send Invoice", _ => {
                        var dbInvoice = context.Invoices.FirstOrDefault(i => i.Id == invoiceId);
                        if (dbInvoice != null)
                        {
                            dbInvoice.Status = "Sent";
                            dbInvoice.UpdatedAt = DateTime.UtcNow;
                            context.SaveChanges();
                            client.Toast($"Invoice {invoiceData.Value.InvoiceNumber} sent!");
                            refreshToken.Refresh();
                            onRefresh?.Invoke();
                        }
                    })
                        .Variant(ButtonVariant.Primary)
                        .Icon(Icons.Mail)
                    : null)
                .Add(new Button("Edit Invoice")
                    .Variant(ButtonVariant.Outline)
                    .Icon(Icons.Pencil)
                    .HandleClick(_ => isEditOpen.Set(true)))
                .Add(new Button("Delete Invoice")
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.Trash)
                    .HandleClick(_ => {
                        try
                        {
                            var invoiceToDelete = context.Invoices.FirstOrDefault(i => i.Id == invoiceId);
                            if (invoiceToDelete != null)
                            {
                                context.Invoices.Remove(invoiceToDelete);
                                context.SaveChanges();
                                client.Toast($"Invoice {invoiceData.Value.InvoiceNumber} deleted successfully!");
                                refreshToken.Refresh();
                                onRefresh?.Invoke();
                                blades.Pop();
                            }
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error deleting invoice: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary)))
            .Add(isEditOpen.Value ? new Sheet(
                (Event<Sheet> _) => isEditOpen.Set(false),
                new InvoiceFormSheet(invoiceId, () => {
                    isEditOpen.Set(false);
                    refreshToken.Refresh();
                    onRefresh?.Invoke();
                }),
                title: "Edit Invoice",
                description: $"Edit invoice {invoiceData.Value.InvoiceNumber}"
            ).Width(Size.Fraction(1/3f)) : null);
    }
}

public class AccountsBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var accounts = context.Accounts.OrderBy(a => a.AccountNumber).ToList();
        
        var listItems = accounts.Select(account => new ListItem(
            title: $"{account.AccountNumber} - {account.AccountName}",
            subtitle: $"{account.AccountType} - Balance: ${account.Balance:N2}",
            icon: Icons.Book,
            badge: account.IsActive ? "Active" : "Inactive",
            onClick: _ => blades.Push(this, new AccountDetailBlade(account.Id), account.AccountName)
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(Text.H3("Chart of Accounts"))
                .Add(new Button("Add Account", _ => client.Toast("Add account"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)))
            .Add(accounts.Count == 0 
                ? Text.Block("No accounts found. Add your first account!")
                : new List(listItems));
    }
}

public class AccountDetailBlade(int accountId) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var account = context.Accounts.FirstOrDefault(a => a.Id == accountId);
        
        if (account == null)
            return "Account not found";
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add($"{account.AccountNumber} - {account.AccountName}")
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add($"Type: {account.AccountType}")
                    .Add($"Balance: ${account.Balance:N2}")
                    .Add($"Description: {account.Description}")
                    .Add(new Badge(account.IsActive ? "Active" : "Inactive")
                        .Variant(account.IsActive ? BadgeVariant.Success : BadgeVariant.Secondary)))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Edit", _ => client.Toast("Edit account"))
                        .Variant(ButtonVariant.Primary))
                    .Add(new Button(account.IsActive ? "Deactivate" : "Activate", _ => client.Toast("Toggle status"))
                        .Variant(account.IsActive ? ButtonVariant.Destructive : ButtonVariant.Success)))
        );
    }
}

public class TransactionsBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var transactions = context.Transactions.OrderByDescending(t => t.TransactionDate).ToList();
        
        var listItems = transactions.Select(transaction => new ListItem(
            title: $"#{transaction.TransactionNumber}",
            subtitle: $"{transaction.Description} - {transaction.TransactionDate:MMM dd, yyyy}",
            icon: Icons.ArrowLeftRight,
            badge: transaction.DebitAmount > 0 ? $"${transaction.DebitAmount:N2} DR" : $"${transaction.CreditAmount:N2} CR",
            onClick: _ => blades.Push(this, new TransactionDetailBlade(transaction.Id), transaction.TransactionNumber)
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(Text.H3("Transactions"))
                .Add(new Button("Add Transaction", _ => client.Toast("Add transaction"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)))
            .Add(transactions.Count == 0 
                ? Text.Block("No transactions found. Add your first transaction!")
                : new List(listItems));
    }
}

public class TransactionDetailBlade(int transactionId) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var transaction = context.Transactions.FirstOrDefault(t => t.Id == transactionId);
        
        if (transaction == null)
            return "Transaction not found";
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add($"Transaction #{transaction.TransactionNumber}")
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add($"Date: {transaction.TransactionDate:MMM dd, yyyy}")
                    .Add($"Description: {transaction.Description}")
                    .Add($"Debit: ${transaction.DebitAmount:N2}")
                    .Add($"Credit: ${transaction.CreditAmount:N2}")
                    .Add($"Reference: {transaction.Reference}"))
                .Add(new Button("Edit", _ => client.Toast("Edit transaction"))
                    .Variant(ButtonVariant.Primary))
        );
    }
}

public class InvoiceFormSheet(int? invoiceId = null, Action? onClose = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var isEdit = invoiceId.HasValue;
        var existingInvoice = isEdit ? context.Invoices.FirstOrDefault(i => i.Id == invoiceId!.Value) : null;
        
        var invoiceForm = this.UseState(existingInvoice ?? new Invoice
        {
            InvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd-HHmmss}",
            CustomerName = "",
            CustomerEmail = "",
            Amount = 0.00m,
            TaxAmount = 0.00m,
            TotalAmount = 0.00m,
            Status = "Draft",
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Description = ""
        });
        
        var statusOptions = new[] { "Draft", "Sent", "Paid", "Overdue" };
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => {
                        try
                        {
                            // Client-side validation
                            if (string.IsNullOrWhiteSpace(invoiceForm.Value.InvoiceNumber))
                            {
                                client.Toast("Invoice Number is required", "Validation Error");
                                return;
                            }
                            
                            if (string.IsNullOrWhiteSpace(invoiceForm.Value.CustomerName))
                            {
                                client.Toast("Customer Name is required", "Validation Error");
                                return;
                            }
                            
                            if (string.IsNullOrWhiteSpace(invoiceForm.Value.CustomerEmail))
                            {
                                client.Toast("Customer Email is required", "Validation Error");
                                return;
                            }
                            
                            if (invoiceForm.Value.Amount <= 0)
                            {
                                client.Toast("Amount must be greater than zero", "Validation Error");
                                return;
                            }
                            
                            if (invoiceForm.Value.TaxAmount < 0)
                            {
                                client.Toast("Tax amount cannot be negative", "Validation Error");
                                return;
                            }
                            
                            if (invoiceForm.Value.InvoiceNumber.Length > 50)
                            {
                                client.Toast("Invoice Number cannot exceed 50 characters", "Validation Error");
                                return;
                            }
                            
                            if (invoiceForm.Value.CustomerName.Length > 200)
                            {
                                client.Toast("Customer Name cannot exceed 200 characters", "Validation Error");
                                return;
                            }
                            
                            if (isEdit && existingInvoice != null)
                            {
                                // Update existing invoice
                                existingInvoice.InvoiceNumber = invoiceForm.Value.InvoiceNumber;
                                existingInvoice.CustomerName = invoiceForm.Value.CustomerName;
                                existingInvoice.CustomerEmail = invoiceForm.Value.CustomerEmail;
                                existingInvoice.Amount = invoiceForm.Value.Amount;
                                existingInvoice.TaxAmount = invoiceForm.Value.TaxAmount;
                                existingInvoice.TotalAmount = invoiceForm.Value.Amount + invoiceForm.Value.TaxAmount;
                                existingInvoice.Status = invoiceForm.Value.Status;
                                existingInvoice.IssueDate = invoiceForm.Value.IssueDate;
                                existingInvoice.DueDate = invoiceForm.Value.DueDate;
                                existingInvoice.Description = invoiceForm.Value.Description;
                                existingInvoice.UpdatedAt = DateTime.UtcNow;
                            }
                            else
                            {
                                // Create new invoice
                                var newInvoice = new Invoice
                                {
                                    InvoiceNumber = invoiceForm.Value.InvoiceNumber,
                                    CustomerName = invoiceForm.Value.CustomerName,
                                    CustomerEmail = invoiceForm.Value.CustomerEmail,
                                    Amount = invoiceForm.Value.Amount,
                                    TaxAmount = invoiceForm.Value.TaxAmount,
                                    TotalAmount = invoiceForm.Value.Amount + invoiceForm.Value.TaxAmount,
                                    Status = invoiceForm.Value.Status,
                                    IssueDate = invoiceForm.Value.IssueDate,
                                    DueDate = invoiceForm.Value.DueDate,
                                    Description = invoiceForm.Value.Description,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                context.Invoices.Add(newInvoice);
                            }
                            
                            context.SaveChanges();
                            client.Toast(isEdit ? "Invoice updated successfully!" : "Invoice created successfully!");
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
                        .Add(Text.Small("Invoice Information"))
                        .Add(Text.Small("Invoice Number"))
                        .Add(new TextInput(invoiceForm.Value.InvoiceNumber, e => {
                            var updated = invoiceForm.Value;
                            updated.InvoiceNumber = e.Value;
                            invoiceForm.Set(updated);
                        }).Placeholder("INV-001").Disabled(isEdit))
                        .Add(Text.Small("Status"))
                        .Add(new SelectInput<string>(invoiceForm.Value.Status, e => {
                            var updated = invoiceForm.Value;
                            updated.Status = e.Value;
                            invoiceForm.Set(updated);
                        }, statusOptions.ToOptions()))
                        .Add(Text.Small("Issue Date"))
                        .Add(new DateTimeInput<DateTime>(invoiceForm.Value.IssueDate, e => {
                            var updated = invoiceForm.Value;
                            updated.IssueDate = e.Value;
                            invoiceForm.Set(updated);
                        }))
                        .Add(Text.Small("Due Date"))
                        .Add(new DateTimeInput<DateTime>(invoiceForm.Value.DueDate, e => {
                            var updated = invoiceForm.Value;
                            updated.DueDate = e.Value;
                            invoiceForm.Set(updated);
                        }))
                ).Title("Invoice Details"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Customer Information"))
                        .Add(Text.Small("Customer Name"))
                        .Add(new TextInput(invoiceForm.Value.CustomerName, e => {
                            var updated = invoiceForm.Value;
                            updated.CustomerName = e.Value;
                            invoiceForm.Set(updated);
                        }).Placeholder("Customer Name"))
                        .Add(Text.Small("Customer Email"))
                        .Add(new TextInput(invoiceForm.Value.CustomerEmail, e => {
                            var updated = invoiceForm.Value;
                            updated.CustomerEmail = e.Value;
                            invoiceForm.Set(updated);
                        }).Placeholder("customer@example.com"))
                ).Title("Customer"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Amount Information"))
                        .Add(Text.Small("Amount ($)"))
                        .Add(new NumberInput<decimal>(invoiceForm.Value.Amount, v => {
                            var updated = invoiceForm.Value;
                            updated.Amount = v;
                            updated.TotalAmount = v + updated.TaxAmount;
                            invoiceForm.Set(updated);
                        }).Placeholder("0.00"))
                        .Add(Text.Small("Tax Amount ($)"))
                        .Add(new NumberInput<decimal>(invoiceForm.Value.TaxAmount, v => {
                            var updated = invoiceForm.Value;
                            updated.TaxAmount = v;
                            updated.TotalAmount = updated.Amount + v;
                            invoiceForm.Set(updated);
                        }).Placeholder("0.00"))
                        .Add(Text.Small("Total Amount ($)"))
                        .Add(Text.Block($"${(invoiceForm.Value.Amount + invoiceForm.Value.TaxAmount):N2}").Variant(TextVariants.Large))
                ).Title("Amounts"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Additional Information"))
                        .Add(Text.Small("Description"))
                        .Add(new TextInput(invoiceForm.Value.Description, e => {
                            var updated = invoiceForm.Value;
                            updated.Description = e.Value;
                            invoiceForm.Set(updated);
                        }).Placeholder("Invoice description...").Variant(TextInputs.Textarea))
                ).Title("Description"))
        );
    }
}
