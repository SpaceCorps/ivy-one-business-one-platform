using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Accounting;

[App(icon: Icons.Percent, title: "Accounting", path: new[] { "Business Operations" })]
public class AccountingApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new AccountingRootBlade(), "Accounting");
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
        
        return BladeHelper.WithHeader(
            Layout.Vertical()
                .Gap(12)
                .Add("Financial Dashboard")
                .Add(Layout.Grid()
                    .Columns(2)
                    .Gap(12)
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12)
                        .Add("Total Revenue").Add($"${totalRevenue:N2}").Add("Paid invoices")))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12)
                        .Add("Pending").Add($"${pendingInvoices:N2}").Add("Unpaid amount")))),
            new List(menuItems)
        );
    }
}

public class InvoicesBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var invoices = context.Invoices.OrderByDescending(i => i.IssueDate).ToList();
        
        var listItems = invoices.Select(invoice => new ListItem(
            title: $"#{invoice.InvoiceNumber} - {invoice.CustomerName}",
            subtitle: $"${invoice.TotalAmount:N2} - Due: {invoice.DueDate:MMM dd, yyyy}",
            icon: Icons.FileText,
            badge: invoice.Status,
            onClick: _ => blades.Push(this, new InvoiceDetailBlade(invoice.Id), $"Invoice {invoice.InvoiceNumber}")
        ));
        
        return BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(12)
                .Add(new Button("Create Invoice", _ => client.Toast("Create invoice"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)),
            invoices.Count == 0 
                ? "No invoices found. Create your first invoice!"
                : new List(listItems)
        );
    }
}

public class InvoiceDetailBlade(int invoiceId) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var invoice = context.Invoices.Include(i => i.Payments).FirstOrDefault(i => i.Id == invoiceId);
        
        if (invoice == null)
            return "Invoice not found";
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add($"Invoice #{invoice.InvoiceNumber}")
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add($"Customer: {invoice.CustomerName}")
                    .Add($"Email: {invoice.CustomerEmail}")
                    .Add($"Issue Date: {invoice.IssueDate:MMM dd, yyyy}")
                    .Add($"Due Date: {invoice.DueDate:MMM dd, yyyy}")
                    .Add($"Amount: ${invoice.Amount:N2}")
                    .Add($"Tax: ${invoice.TaxAmount:N2}")
                    .Add($"Total: ${invoice.TotalAmount:N2}")
                    .Add(new Badge(invoice.Status)
                        .Variant(invoice.Status == "Paid" ? BadgeVariant.Success :
                               invoice.Status == "Overdue" ? BadgeVariant.Destructive :
                               BadgeVariant.Secondary)))
                .Add("Description")
                .Add(invoice.Description)
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Edit", _ => client.Toast("Edit invoice"))
                        .Variant(ButtonVariant.Primary))
                    .Add(new Button("Send", _ => client.Toast("Send invoice"))
                        .Variant(ButtonVariant.Secondary))
                    .Add(invoice.Status != "Paid" 
                        ? new Button("Mark Paid", _ => client.Toast("Mark as paid"))
                            .Variant(ButtonVariant.Success)
                        : null))
        );
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
        
        return BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(12)
                .Add(new Button("Add Account", _ => client.Toast("Add account"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)),
            accounts.Count == 0 
                ? "No accounts found. Add your first account!"
                : new List(listItems)
        );
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
        
        return BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(12)
                .Add(new Button("Add Transaction", _ => client.Toast("Add transaction"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)),
            transactions.Count == 0 
                ? "No transactions found. Add your first transaction!"
                : new List(listItems)
        );
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
