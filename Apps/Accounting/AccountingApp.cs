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

        // Modern KPI Cards
        var kpiCards = new[]
        {
            new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Padding(4)
                    .Add(Text.H4("Total Revenue"))
                    .Add(Text.H2($"${totalRevenue:N0}"))
                    .Add(Text.Small("From paid invoices"))),

            new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Padding(4)
                    .Add(Text.H4("Pending Amount"))
                    .Add(Text.H2($"${pendingInvoices:N0}"))
                    .Add(Text.Small("Unpaid invoices"))),

            new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Padding(4)
                    .Add(Text.H4("Overdue"))
                    .Add(Text.H2($"${overdueInvoices:N0}"))
                    .Add(Text.Small("Late payments"))),

            new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Padding(4)
                    .Add(Text.H4("Total Expenses"))
                    .Add(Text.H2($"${totalExpenses:N0}"))
                    .Add(Text.Small("Business expenses")))
        };

        // Quick Actions as ListItems
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

        // Recent Activity
        var recentInvoices = invoices.Take(5).Select(inv => new ListItem(
            title: $"#{inv.InvoiceNumber} - {inv.CustomerName}",
            subtitle: $"${inv.TotalAmount:N2} - {inv.Status}",
            badge: inv.Status == "Paid" ? BadgeVariant.Success :
                   inv.Status == "Overdue" ? BadgeVariant.Destructive :
                   BadgeVariant.Secondary
        ));
        
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
                    .Padding(4)
                    .Add(Text.H3("Quick Actions"))
                    .Add(new List(quickActions))))
            .Add(Layout.Grid()
                .Columns(2)
                .Gap(4)
                .Add(new Card(
                    Layout.Vertical()
                        .Gap(2)
                        .Padding(4)
                        .Add(Text.H4("Recent Invoices"))
                        .Add(invoices.Count > 0 
                            ? new List(recentInvoices)
                            : Text.P("No recent invoices"))))
                .Add(new Card(
                    Layout.Vertical()
                        .Gap(2)
                        .Padding(4)
                        .Add(Text.H4("Bank Reconciliation"))
                        .Add(Text.P("🤖 AI-Powered Matching: 95% automated"))
                        .Add(Text.P("📱 Mobile receipt capture"))
                        .Add(Text.P("🔄 Real-time synchronization"))
                        .Add(new Button("Reconcile Now", _ => blades.Push(this, new BankReconciliationBlade(), "Reconcile"))
                            .Variant(ButtonVariant.Primary)
                            .Width(Size.Full())))));
    }
}

// Simplified blade implementations
public class InvoicesListBlade : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
        var invoices = db.Invoices.OrderByDescending(i => i.IssueDate).ToList();
        
        var listItems = invoices.Select(invoice => new ListItem(
            title: $"#{invoice.InvoiceNumber} - {invoice.CustomerName}",
            subtitle: $"${invoice.TotalAmount:N2} - Due: {invoice.DueDate:MMM dd, yyyy}",
            icon: Icons.FileText,
            badge: invoice.Status
        ));
        
        return Layout.Vertical()
            .Gap(4)
            .Padding(2)
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(Text.H3("Customer Invoices"))
                .Add(new Button("Create Invoice").Variant(ButtonVariant.Primary).Icon(Icons.Plus)))
            .Add(invoices.Count == 0 
                ? new Card(
                    Layout.Vertical()
                        .Gap(2)
                        .Padding(2)
                        .Add(Text.H4("No invoices yet"))
                        .Add(Text.P("Create your first customer invoice to get started.")))
                : new List(listItems));
    }
}

public class VendorBillsListBlade : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            .Gap(4)
            .Padding(2)
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(Text.H3("Vendor Bills"))
                .Add(new Button("Create Bill").Variant(ButtonVariant.Primary).Icon(Icons.Plus)))
            .Add(new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Padding(2)
                    .Add(Text.H4("No vendor bills yet"))
                    .Add(Text.P("Process your first supplier invoice."))));
    }
}

public class BankReconciliationBlade : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            .Gap(4)
            .Padding(2)
            .Add(Text.H3("Bank Reconciliation"))
            .Add(new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Padding(2)
                    .Add(Text.H4("🤖 AI-Powered Bank Reconciliation"))
                    .Add(Text.P("Automatically match bank transactions with your accounting records."))
                    .Add(new Button("Start Reconciliation").Variant(ButtonVariant.Primary).Width(Size.Full()))));
    }
}

public class ChartOfAccountsBlade : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
        var accounts = db.Accounts.ToList();
        
        var listItems = accounts.Select(account => new ListItem(
            title: $"{account.Id} - Account",
            subtitle: account.Description ?? "No description",
            icon: Icons.List,
            badge: account.IsActive ? "Active" : "Inactive"
        ));
        
        return Layout.Vertical()
            .Gap(4)
            .Padding(2)
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(Text.H3("Chart of Accounts"))
                .Add(new Button("Add Account").Variant(ButtonVariant.Primary).Icon(Icons.Plus)))
            .Add(accounts.Count == 0 
                ? new Card(
                    Layout.Vertical()
                        .Gap(2)
                        .Padding(2)
                        .Add(Text.H4("No accounts yet"))
                        .Add(Text.P("Set up your chart of accounts to organize your financial data.")))
                : new List(listItems));
    }
}

public class ExpensesListBlade : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            .Gap(4)
            .Padding(2)
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(Text.H3("Expenses"))
                .Add(new Button("Add Expense").Variant(ButtonVariant.Primary).Icon(Icons.Plus)))
            .Add(new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Padding(2)
                    .Add(Text.H4("No expenses yet"))
                    .Add(Text.P("Track your business expenses with AI-powered categorization."))));
    }
}

public class FinancialReportsBlade : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            .Gap(4)
            .Padding(2)
            .Add(Text.H3("Financial Reports"))
            .Add(new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Padding(2)
                    .Add(Text.H4("📊 Financial Analytics"))
                    .Add(Text.P("Generate comprehensive financial reports and analytics."))
                    .Add(new Button("Generate Report").Variant(ButtonVariant.Primary).Width(Size.Full()))));
    }
}

public class ReceiptScannerBlade : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            .Gap(4)
            .Padding(2)
            .Add(Text.H3("📷 Receipt Scanner"))
            .Add(new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Padding(2)
                    .Add(Text.H4("AI Receipt Processing"))
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