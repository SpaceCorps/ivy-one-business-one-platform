using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Views.Alerts;
using Ivy.Views.Blades;
using Ivy.Views.Builders;
using Ivy.Views.Forms;
using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Accounting;

[App(icon: Icons.Calculator, title: "Accounting", path: new[] { "Finance" })]
public class AccountingApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new AccountingDashboard(), "Dashboard", Size.Units(80));
    }
}

public class AccountingDashboard : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();

        var invoices = db.Invoices.ToList();
        var accounts = db.Accounts.ToList();
        var transactions = db.Transactions.ToList();

        var totalRevenue = invoices.Where(i => i.Status == "Paid").Sum(i => i.TotalAmount);
        var pendingInvoices = invoices.Where(i => i.Status != "Paid").Sum(i => i.TotalAmount);
        var overdueInvoices = invoices.Where(i => i.Status == "Overdue").Sum(i => i.TotalAmount);
        var totalExpenses = transactions.Where(t => t.DebitAmount > 0).Sum(t => t.DebitAmount);

        var kpiCards = new[]
        {
            new Card(
                Layout.Vertical()
                    .Gap(4)
                    .Padding(2)
                    .Add(Text.H4("Total Revenue"))
                    .Add(Text.H2($"${totalRevenue:N0}"))
                    .Add(Text.Small("From paid invoices"))),

            new Card(
                Layout.Vertical()
                    .Gap(4)
                    .Padding(2)
                    .Add(Text.H4("Pending Amount"))
                    .Add(Text.H2($"${pendingInvoices:N0}"))
                    .Add(Text.Small("Unpaid invoices"))),

            new Card(
                Layout.Vertical()
                    .Gap(4)
                    .Padding(2)
                    .Add(Text.H4("Overdue"))
                    .Add(Text.H2($"${overdueInvoices:N0}"))
                    .Add(Text.Small("Late payments"))),

            new Card(
                Layout.Vertical()
                    .Gap(4)
                    .Padding(2)
                    .Add(Text.H4("Total Expenses"))
                    .Add(Text.H2($"${totalExpenses:N0}"))
                    .Add(Text.Small("Business expenses")))
        };

        var quickActions = new[]
        {
            new ListItem("📄 Customer Invoices",
                subtitle: "Create and manage customer invoices",
                icon: Icons.FileText,
                badge: invoices.Count.ToString(),
                onClick: _ => blades.Push(this, new InvoicesListBlade(), "Customer Invoices")),

            new ListItem("🧾 Vendor Bills",
                subtitle: "Process supplier invoices and bills",
                icon: Icons.Receipt,
                badge: "New",
                onClick: _ => blades.Push(this, new VendorBillsListBlade(), "Vendor Bills")),

            new ListItem("🏦 Bank Reconciliation",
                subtitle: "Match bank transactions automatically",
                icon: Icons.Building,
                badge: "Smart",
                onClick: _ => blades.Push(this, new BankReconciliationBlade(), "Bank Reconciliation")),

            new ListItem("📊 Chart of Accounts",
                subtitle: "Manage your accounting structure",
                icon: Icons.Book,
                badge: accounts.Count.ToString(),
                onClick: _ => blades.Push(this, new ChartOfAccountsBlade(), "Chart of Accounts")),

            new ListItem("💰 Expenses",
                subtitle: "Track and reimburse expenses",
                icon: Icons.CreditCard,
                badge: "AI",
                onClick: _ => blades.Push(this, new ExpensesListBlade(), "Expenses")),

            new ListItem("📈 Financial Reports",
                subtitle: "Real-time financial performance",
                icon: Icons.TrendingUp,
                badge: "Live",
                onClick: _ => blades.Push(this, new FinancialReportsBlade(), "Financial Reports"))
        };
        
        return Layout.Vertical()
            .Gap(4)
            .Padding(2)
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(Text.H2("Accounting Dashboard"))
                .Add(new Button("📷 Scan Receipt", _ => blades.Push(this, new ReceiptScannerBlade(), "Scan Receipt"))
                    .Icon(Icons.Camera)
                    .Variant(ButtonVariant.Primary))
                .Add(new Button("⚡ Quick Invoice", _ => { })
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Outline)))
            .Add(Layout.Grid()
                .Columns(4)
                .Gap(4)
                .Add(kpiCards))
            .Add(new Card(
                Layout.Vertical()
                    .Gap(4)
                    .Padding(2)
                    .Add(Text.H3("Quick Actions"))
                    .Add(new List(quickActions))))
                    .Add(Layout.Grid()
                        .Columns(2)
                .Gap(4)
                .Add(new Card(
                    Layout.Vertical()
                        .Gap(2)
                        .Padding(2)
                        .Add(Text.H4("Recent Invoices"))
                        .Add(invoices.Take(5).Select(inv => new ListItem(
                            title: $"#{inv.InvoiceNumber} - {inv.CustomerName}",
                            subtitle: $"${inv.TotalAmount:N2} - {inv.Status}",
                            badge: inv.Status == "Paid" ? BadgeVariant.Success :
                                   inv.Status == "Overdue" ? BadgeVariant.Destructive :
                                   BadgeVariant.Secondary))))
                ))
                .Add(new Card(
                    Layout.Vertical()
                        .Gap(2)
                        .Padding(2)
                        .Add(Text.H4("Bank Transactions"))
                        .Add(Text.P("🤖 AI-Powered Matching: 95% automated"))
                        .Add(Text.P("📱 Mobile receipt capture"))
                        .Add(Text.P("🔄 Real-time synchronization"))
                        .Add(new Button("Reconcile Now", _ => blades.Push(this, new BankReconciliationBlade(), "Reconcile"))
                            .Variant(ButtonVariant.Primary)
                            .Width(Size.Full()))));
    }
}

public class InvoicesListBlade : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();

        var invoices = db.Invoices.OrderByDescending(i => i.IssueDate).ToList();
        
        var createBtn = new Button("Create Invoice", _ =>
        {
            blades.Pop(this);
        }).ToTrigger((isOpen) => new InvoiceCreateDialog(isOpen, refreshToken));
        
        var listItems = invoices.Select(invoice => new ListItem(
            title: $"#{invoice.InvoiceNumber} - {invoice.CustomerName}",
            subtitle: $"${invoice.TotalAmount:N2} - Due: {invoice.DueDate:MMM dd, yyyy}",
            icon: Icons.FileText,
            badge: invoice.Status,
            onClick: _ => blades.Push(this, new InvoiceDetailsBlade(invoice.Id), $"Invoice {invoice.InvoiceNumber}")
        ));
        
        return Layout.Vertical()
            .Gap(4)
            .Padding(2)
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(Text.H3("Customer Invoices"))
                .Add(createBtn))
            .Add(invoices.Count == 0 
                ? new Card(
                    Layout.Vertical()
                        .Gap(4)
                        .Padding(24)
                        .Add(Text.H4("No invoices yet"))
                        .Add(Text.P("Create your first customer invoice to get started with invoicing."))
                        .Add(createBtn))
                : new List(listItems));
    }
}

public class InvoiceDetailsBlade(int invoiceId) : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        
        var invoice = db.Invoices.Include(i => i.Payments).FirstOrDefault(i => i.Id == invoiceId);
        
        if (invoice == null)
            return new Error("Invoice not found");

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .ToTrigger((isOpen) => new InvoiceEditSheet(isOpen, invoiceId, refreshToken));

        var sendBtn = new Button("Send")
            .Variant(ButtonVariant.Secondary)
            .Icon(Icons.Send);

        var markPaidBtn = invoice.Status != "Paid"
            ? new Button("Mark Paid")
                .Variant(ButtonVariant.Success)
                .Icon(Icons.Check)
            : null;

        return Layout.Vertical()
            .Gap(4)
            .Padding(2)
            .Add(new Card(
            Layout.Vertical()
                .Gap(4)
                    .Padding(2)
                    .Add(Layout.Horizontal()
                        .Gap(4)
                        .Add(Text.H3($"Invoice #{invoice.InvoiceNumber}"))
                        .Add(new Badge(invoice.Status)
                            .Variant(invoice.Status == "Paid" ? BadgeVariant.Success :
                                   invoice.Status == "Overdue" ? BadgeVariant.Destructive :
                                   BadgeVariant.Secondary)))
                    .Add(Layout.Grid()
                        .Columns(2)
                        .Gap(4)
                .Add(Layout.Vertical()
                    .Gap(8)
                            .Add(Text.H4("Customer Information"))
                            .Add($"Name: {invoice.CustomerName}")
                    .Add($"Email: {invoice.CustomerEmail}")
                            .Add($"Address: {invoice.CustomerEmail ?? "Not provided"}"))
                        .Add(Layout.Vertical()
                            .Gap(2)
                            .Add(Text.H4("Invoice Details"))
                    .Add($"Issue Date: {invoice.IssueDate:MMM dd, yyyy}")
                    .Add($"Due Date: {invoice.DueDate:MMM dd, yyyy}")
                    .Add($"Amount: ${invoice.Amount:N2}")
                    .Add($"Tax: ${invoice.TaxAmount:N2}")
                            .Add(Text.H4($"Total: ${invoice.TotalAmount:N2}"))))
                    .Add(Layout.Vertical()
                        .Gap(2)
                        .Add(Text.H4("Description"))
                        .Add(Text.P(invoice.Description ?? "No description provided")))
                .Add(Layout.Horizontal()
                    .Gap(12)
                        .Add(editBtn)
                        .Add(sendBtn)
                        .Add(markPaidBtn))));
    }
}

public class ChartOfAccountsBlade : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();

        var accounts = db.Accounts.OrderBy(a => a.AccountNumber).ToList();
        
        var createBtn = new Button("Add Account", _ =>
        {
            blades.Pop(this);
        }).ToTrigger((isOpen) => new AccountCreateDialog(isOpen, refreshToken));
        
        var listItems = accounts.Select(account => new ListItem(
            title: $"{account.AccountNumber} - {account.AccountName}",
            subtitle: $"{account.AccountType} - Balance: ${account.Balance:N2}",
            icon: Icons.Book,
            badge: account.IsActive ? "Active" : "Inactive",
            onClick: _ => blades.Push(this, new AccountDetailsBlade(account.Id), account.AccountName)
        ));
        
        return Layout.Vertical()
            .Gap(4)
            .Padding(2)
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(Text.H3("Chart of Accounts"))
                .Add(createBtn))
            .Add(accounts.Count == 0 
                ? new Card(
                    Layout.Vertical()
                        .Gap(4)
                        .Padding(2)
                        .Add(Text.H4("No accounts yet"))
                        .Add(Text.P("Set up your chart of accounts to organize your financial data."))
                        .Add(createBtn))
                : new List(listItems));
    }
}

public class AccountDetailsBlade(int accountId) : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
        var refreshToken = this.UseRefreshToken();
        
        var account = db.Accounts.FirstOrDefault(a => a.Id == accountId);
        
        if (account == null)
            return new Error("Account not found");

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .ToTrigger((isOpen) => new AccountEditSheet(isOpen, accountId, refreshToken));

        var toggleBtn = new Button(account.IsActive ? "Deactivate" : "Activate")
            .Variant(account.IsActive ? ButtonVariant.Destructive : ButtonVariant.Success)
            .Icon(account.IsActive ? Icons.X : Icons.Check);

        return Layout.Vertical()
            .Gap(4)
            .Padding(2)
            .Add(new Card(
            Layout.Vertical()
                .Gap(4)
                    .Padding(2)
                    .Add(Layout.Horizontal()
                        .Gap(4)
                        .Add(Text.H3($"{account.AccountNumber} - {account.AccountName}"))
                        .Add(new Badge(account.IsActive ? "Active" : "Inactive")
                            .Variant(account.IsActive ? BadgeVariant.Success : BadgeVariant.Secondary)))
                    .Add(Layout.Grid()
                        .Columns(2)
                        .Gap(4)
                .Add(Layout.Vertical()
                    .Gap(8)
                            .Add(Text.H4("Account Information"))
                    .Add($"Type: {account.AccountType}")
                            .Add($"Number: {account.AccountNumber}")
                            .Add($"Balance: ${account.Balance:N2}"))
                        .Add(Layout.Vertical()
                            .Gap(2)
                            .Add(Text.H4("Description"))
                            .Add(Text.P(account.Description ?? "No description provided"))))
                .Add(Layout.Horizontal()
                    .Gap(2)
                        .Add(editBtn)
                        .Add(toggleBtn))));
    }
}

public class TransactionsListBlade : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();

        var transactions = db.Transactions.OrderByDescending(t => t.TransactionDate).ToList();
        
        var createBtn = new Button("Add Transaction", _ =>
        {
            blades.Pop(this);
        }).ToTrigger((isOpen) => new TransactionCreateDialog(isOpen, refreshToken));
        
        var listItems = transactions.Select(transaction => new ListItem(
            title: $"#{transaction.TransactionNumber}",
            subtitle: $"{transaction.Description} - {transaction.TransactionDate:MMM dd, yyyy}",
            icon: Icons.ArrowLeftRight,
            badge: transaction.DebitAmount > 0 ? $"${transaction.DebitAmount:N2} DR" : $"${transaction.CreditAmount:N2} CR",
            onClick: _ => blades.Push(this, new TransactionDetailsBlade(transaction.Id), transaction.TransactionNumber)
        ));
        
        return Layout.Vertical()
            .Gap(4)
            .Padding(2)
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(Text.H3("Transactions"))
                .Add(createBtn))
            .Add(transactions.Count == 0 
                ? new Card(
                    Layout.Vertical()
                        .Gap(4)
                        .Padding(2)
                        .Add(Text.H4("No transactions yet"))
                        .Add(Text.P("Create your first transaction to start tracking your financial data."))
                        .Add(createBtn))
                : new List(listItems));
    }
}

public class TransactionDetailsBlade(int transactionId) : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
        var refreshToken = this.UseRefreshToken();
        
        var transaction = db.Transactions.FirstOrDefault(t => t.Id == transactionId);
        
        if (transaction == null)
            return new Error("Transaction not found");

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .ToTrigger((isOpen) => new TransactionEditSheet(isOpen, transactionId, refreshToken));

        return Layout.Vertical()
            .Gap(4)
            .Padding(2)
            .Add(new Card(
            Layout.Vertical()
                    .Gap(4)
                    .Padding(2)
                    .Add(Text.H3($"Transaction #{transaction.TransactionNumber}"))
                    .Add(Layout.Grid()
                        .Columns(2)
                        .Gap(4)
                .Add(Layout.Vertical()
                    .Gap(8)
                            .Add(Text.H4("Transaction Details"))
                    .Add($"Date: {transaction.TransactionDate:MMM dd, yyyy}")
                    .Add($"Description: {transaction.Description}")
                            .Add($"Reference: {transaction.Reference ?? "N/A"}"))
                        .Add(Layout.Vertical()
                            .Gap(2)
                            .Add(Text.H4("Amounts"))
                    .Add($"Debit: ${transaction.DebitAmount:N2}")
                    .Add($"Credit: ${transaction.CreditAmount:N2}")
                            .Add(Text.H4($"Net: ${(transaction.DebitAmount - transaction.CreditAmount):N2}"))))
                    .Add(editBtn)));
    }
}

// New Odoo-style components
public class VendorBillsListBlade : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();

        var bills = db.Invoices.Where(i => i.Status == "VendorBill").ToList();

        var createBtn = new Button("Create Vendor Bill", _ =>
        {
            blades.Pop(this);
        }).ToTrigger((isOpen) => new VendorBillCreateDialog(isOpen, refreshToken));

        return Layout.Vertical()
            .Gap(4)
            .Padding(2)
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(Text.H3("Vendor Bills"))
                .Add(createBtn))
            .Add(new Card(
                Layout.Vertical()
                    .Gap(4)
                    .Padding(2)
                    .Add(Text.H4("🤖 AI-Powered Bill Processing"))
                    .Add(Text.P("• 98% recognition rate for invoice data capture"))
                    .Add(Text.P("• Automatic vendor matching"))
                    .Add(Text.P("• Smart tax calculation"))
                    .Add(Text.P("• One-click approval workflow"))
                    .Add(createBtn)));
    }
}

public class BankReconciliationBlade : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();

        return Layout.Vertical()
            .Gap(4)
            .Padding(2)
            .Add(new Card(
                Layout.Vertical()
                    .Gap(4)
                    .Padding(2)
                    .Add(Text.H3("🏦 Bank Reconciliation"))
                    .Add(Layout.Grid()
                        .Columns(2)
                        .Gap(4)
                        .Add(Layout.Vertical()
                            .Gap(2)
                            .Add(Text.H4("📱 Smart Features"))
                            .Add(Text.P("• 28,000+ banks supported"))
                            .Add(Text.P("• Real-time synchronization"))
                            .Add(Text.P("• 95% automated matching"))
                            .Add(Text.P("• Mobile receipt capture")))
                        .Add(Layout.Vertical()
                            .Gap(2)
                            .Add(Text.H4("⚡ Quick Actions"))
                            .Add(new Button("Sync Bank Data")
                                .Variant(ButtonVariant.Primary)
                                .Width(Size.Full()))
                            .Add(new Button("Manual Match")
                                .Variant(ButtonVariant.Outline)
                                .Width(Size.Full()))
                            .Add(new Button("Review Exceptions")
                                .Variant(ButtonVariant.Secondary)
                                .Width(Size.Full()))))));
    }
}

public class ExpensesListBlade : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            .Gap(4)
            .Padding(2)
            .Add(new Card(
                Layout.Vertical()
                    .Gap(4)
                    .Padding(2)
                    .Add(Text.H3("💰 Expense Management"))
                    .Add(Layout.Grid()
                        .Columns(2)
                        .Gap(4)
                        .Add(Layout.Vertical()
                            .Gap(2)
                            .Add(Text.H4("📱 Mobile Experience"))
                            .Add(Text.P("• Take pictures of receipts"))
                            .Add(Text.P("• AI extracts data automatically"))
                            .Add(Text.P("• Instant expense categorization"))
                            .Add(Text.P("• GPS location tracking")))
                        .Add(Layout.Vertical()
                            .Gap(2)
                            .Add(Text.H4("🚀 Quick Actions"))
                            .Add(new Button("📷 Scan Receipt")
                                .Variant(ButtonVariant.Primary)
                                .Width(Size.Full()))
                            .Add(new Button("Add Manual Expense")
                                .Variant(ButtonVariant.Outline)
                                .Width(Size.Full()))
                            .Add(new Button("View Reports")
                                .Variant(ButtonVariant.Secondary)
                                .Width(Size.Full()))))));
    }
}

public class FinancialReportsBlade : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            .Gap(4)
            .Padding(2)
            .Add(new Card(
                Layout.Vertical()
                    .Gap(4)
                    .Padding(2)
                    .Add(Text.H3("📈 Financial Reports"))
                    .Add(Text.P("Real-time financial performance reports to make informed business decisions."))
                    .Add(Layout.Grid()
                        .Columns(2)
                        .Gap(2)
                        .Add(new Button("📊 Profit & Loss")
                            .Variant(ButtonVariant.Outline)
                            .Width(Size.Full()))
                        .Add(new Button("💰 Balance Sheet")
                            .Variant(ButtonVariant.Outline)
                            .Width(Size.Full()))
                        .Add(new Button("💸 Cash Flow")
                            .Variant(ButtonVariant.Outline)
                            .Width(Size.Full()))
                        .Add(new Button("📋 Trial Balance")
                            .Variant(ButtonVariant.Outline)
                            .Width(Size.Full()))
                        .Add(new Button("🏦 Bank Reconciliation")
                            .Variant(ButtonVariant.Outline)
                            .Width(Size.Full()))
                        .Add(new Button("📤 Export Data")
                            .Variant(ButtonVariant.Outline)
                            .Width(Size.Full())))));
    }
}

public class ReceiptScannerBlade : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            .Gap(4)
            .Padding(2)
            .Add(new Card(
                Layout.Vertical()
                    .Gap(4)
                    .Padding(2)
                    .Add(Text.H3("📷 Receipt Scanner"))
                    .Add(Text.P("Take a picture of your receipt and let AI do the rest!"))
                    .Add(Layout.Vertical()
                        .Gap(2)
                        .Add(new Button("📷 Take Photo")
                            .Variant(ButtonVariant.Primary)
                            .Width(Size.Full())
                            .Height(Size.Units(60)))
                        .Add(new Button("📁 Upload from Gallery")
                            .Variant(ButtonVariant.Outline)
                            .Width(Size.Full()))
                        .Add(Text.P("🤖 AI will automatically extract:"))
                        .Add(Text.P("• Vendor name and address"))
                        .Add(Text.P("• Amount and tax"))
                        .Add(Text.P("• Date and category")))));
    }
}

// Dialog and Sheet components (simplified for now)
public class QuickInvoiceDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    public override object? Build()
    {
        return new Card(
            Layout.Vertical()
                .Gap(4)
                .Padding(2)
                .Add(Text.H3("⚡ Quick Invoice"))
                .Add(Text.P("Create a simple invoice in seconds"))
                .Add(new Button("Create Invoice")
                    .Variant(ButtonVariant.Primary)
                    .Width(Size.Full()))
                .Add(new Button("Cancel")
                    .Variant(ButtonVariant.Outline)
                    .Width(Size.Full())));
    }
}

public class InvoiceCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    public override object? Build()
    {
        return new Card(
            Layout.Vertical()
                .Gap(4)
                .Padding(2)
                .Add(Text.H3("Create Invoice"))
                .Add(Text.P("Full invoice creation form"))
                .Add(new Button("Create")
                    .Variant(ButtonVariant.Primary)
                    .Width(Size.Full()))
                .Add(new Button("Cancel")
                    .Variant(ButtonVariant.Outline)
                    .Width(Size.Full())));
    }
}

public class InvoiceEditSheet(IState<bool> isOpen, int id, RefreshToken refreshToken) : ViewBase
{
    public override object? Build()
    {
        return new Card(
            Layout.Vertical()
                .Gap(4)
                .Padding(2)
                .Add(Text.H3("Edit Invoice"))
                .Add(Text.P("Edit invoice form"))
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .Width(Size.Full()))
                .Add(new Button("Cancel")
                    .Variant(ButtonVariant.Outline)
                    .Width(Size.Full())));
    }
}

public class AccountCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    public override object? Build()
    {
        return new Card(
            Layout.Vertical()
                .Gap(4)
                .Padding(2)
                .Add(Text.H3("Add Account"))
                .Add(Text.P("Create new account form"))
                .Add(new Button("Create")
                    .Variant(ButtonVariant.Primary)
                    .Width(Size.Full()))
                .Add(new Button("Cancel")
                    .Variant(ButtonVariant.Outline)
                    .Width(Size.Full())));
    }
}

public class AccountEditSheet(IState<bool> isOpen, int id, RefreshToken refreshToken) : ViewBase
{
    public override object? Build()
    {
        return new Card(
            Layout.Vertical()
                .Gap(4)
                .Padding(2)
                .Add(Text.H3("Edit Account"))
                .Add(Text.P("Edit account form"))
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .Width(Size.Full()))
                .Add(new Button("Cancel")
                    .Variant(ButtonVariant.Outline)
                    .Width(Size.Full())));
    }
}

public class TransactionCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    public override object? Build()
    {
        return new Card(
            Layout.Vertical()
                .Gap(4)
                .Padding(2)
                .Add(Text.H3("Add Transaction"))
                .Add(Text.P("Create new transaction form"))
                .Add(new Button("Create")
                    .Variant(ButtonVariant.Primary)
                    .Width(Size.Full()))
                .Add(new Button("Cancel")
                    .Variant(ButtonVariant.Outline)
                    .Width(Size.Full())));
    }
}

public class TransactionEditSheet(IState<bool> isOpen, int id, RefreshToken refreshToken) : ViewBase
{
    public override object? Build()
    {
        return new Card(
            Layout.Vertical()
                .Gap(4)
                .Padding(2)
                .Add(Text.H3("Edit Transaction"))
                .Add(Text.P("Edit transaction form"))
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .Width(Size.Full()))
                .Add(new Button("Cancel")
                    .Variant(ButtonVariant.Outline)
                    .Width(Size.Full())));
    }
}

public class VendorBillCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    public override object? Build()
    {
        return new Card(
            Layout.Vertical()
                .Gap(4)
                .Padding(2)
                .Add(Text.H3("Create Vendor Bill"))
                .Add(Text.P("Create vendor bill form"))
                .Add(new Button("Create")
                    .Variant(ButtonVariant.Primary)
                    .Width(Size.Full()))
                .Add(new Button("Cancel")
                    .Variant(ButtonVariant.Outline)
                    .Width(Size.Full())));
    }
}
