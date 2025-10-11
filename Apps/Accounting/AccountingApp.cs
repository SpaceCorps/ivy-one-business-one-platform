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
        return new AccountingDashboard();
    }
}

public class AccountingDashboard : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();

        var invoices = db.Invoices.ToList();
        var accounts = db.Accounts.ToList();
        var transactions = db.Transactions.ToList();

        var totalRevenue = invoices.Where(i => i.Status == "Paid").Sum(i => i.TotalAmount);
        var pendingInvoices = invoices.Where(i => i.Status != "Paid").Sum(i => i.TotalAmount);
        var overdueInvoices = invoices.Where(i => i.Status == "Overdue").Sum(i => i.TotalAmount);
        var totalExpenses = transactions.Where(t => t.DebitAmount > 0).Sum(t => t.DebitAmount);

        // Financial Overview KPI Cards
        var kpiCards = new[]
        {
            new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Padding(4)
                    .Add(Layout.Horizontal()
                        .Gap(2)
                        .Add(Icons.TrendingUp.ToIcon())
                        .Add(Text.Small("TOTAL REVENUE")))
                    .Add(Text.H1($"${totalRevenue:N0}"))
                    .Add(Text.Small("From paid invoices"))),

            new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Padding(4)
                    .Add(Layout.Horizontal()
                        .Gap(2)
                        .Add(Icons.Clock.ToIcon())
                        .Add(Text.Small("PENDING AMOUNT")))
                    .Add(Text.H1($"${pendingInvoices:N0}"))
                    .Add(Text.Small("Unpaid invoices"))),

            new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Padding(4)
                    .Add(Layout.Horizontal()
                        .Gap(2)
                        .Add(Icons.X.ToIcon())
                        .Add(Text.Small("OVERDUE")))
                    .Add(Text.H1($"${overdueInvoices:N0}"))
                    .Add(Text.Small("Late payments"))),

            new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Padding(4)
                    .Add(Layout.Horizontal()
                        .Gap(2)
                        .Add(Icons.CreditCard.ToIcon())
                        .Add(Text.Small("TOTAL EXPENSES")))
                    .Add(Text.H1($"${totalExpenses:N0}"))
                    .Add(Text.Small("Business expenses")))
        };

        // Quick Actions Section
        var quickActions = new[]
        {
            new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Padding(4)
                    .Add(Layout.Horizontal()
                        .Gap(2)
                        .Add(Icons.FileText.ToIcon())
                        .Add(Text.H4("Customer Invoices"))
                        .Add(new Badge(invoices.Count.ToString())))
                    .Add(Text.Small("Create and manage customer invoices"))
                    .Add(Layout.Horizontal()
                        .Gap(2)
                        .Add(new Button("View All").Variant(ButtonVariant.Outline))
                        .Add(new Button("Create New").Variant(ButtonVariant.Primary)))),

            new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Padding(4)
                    .Add(Layout.Horizontal()
                        .Gap(2)
                        .Add(Icons.Receipt.ToIcon())
                        .Add(Text.H4("Vendor Bills"))
                        .Add(new Badge("New").Variant(BadgeVariant.Success)))
                    .Add(Text.Small("Process supplier invoices and bills"))
                    .Add(Layout.Horizontal()
                        .Gap(2)
                        .Add(new Button("View All").Variant(ButtonVariant.Outline))
                        .Add(new Button("Create New").Variant(ButtonVariant.Primary)))),

            new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Padding(4)
                    .Add(Layout.Horizontal()
                        .Gap(2)
                        .Add(Icons.Building.ToIcon())
                        .Add(Text.H4("Bank Reconciliation"))
                        .Add(new Badge("Smart").Variant(BadgeVariant.Success)))
                    .Add(Text.Small("Match bank transactions automatically"))
                    .Add(new Button("Reconcile Now").Variant(ButtonVariant.Primary).Width(Size.Full()))),

            new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Padding(4)
                    .Add(Layout.Horizontal()
                        .Gap(2)
                        .Add(Icons.Book.ToIcon())
                        .Add(Text.H4("Chart of Accounts"))
                        .Add(new Badge(accounts.Count.ToString())))
                    .Add(Text.Small("Manage your accounting structure"))
                    .Add(Layout.Horizontal()
                        .Gap(2)
                        .Add(new Button("View All").Variant(ButtonVariant.Outline))
                        .Add(new Button("Add Account").Variant(ButtonVariant.Primary)))),

            new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Padding(4)
                    .Add(Layout.Horizontal()
                        .Gap(2)
                        .Add(Icons.CreditCard.ToIcon())
                        .Add(Text.H4("Expenses"))
                        .Add(new Badge("AI").Variant(BadgeVariant.Success)))
                    .Add(Text.Small("Track and reimburse expenses"))
                    .Add(Layout.Horizontal()
                        .Gap(2)
                        .Add(new Button("View All").Variant(ButtonVariant.Outline))
                        .Add(new Button("Add Expense").Variant(ButtonVariant.Primary)))),

            new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Padding(4)
                    .Add(Layout.Horizontal()
                        .Gap(2)
                        .Add(Icons.TrendingUp.ToIcon())
                        .Add(Text.H4("Financial Reports"))
                        .Add(new Badge("Live").Variant(BadgeVariant.Success)))
                    .Add(Text.Small("Real-time financial performance"))
                    .Add(new Button("Generate Report").Variant(ButtonVariant.Primary).Width(Size.Full())))
        };

        // Recent Activity Section
        var recentInvoices = invoices.Take(5).Select(inv => new ListItem(
            title: $"#{inv.InvoiceNumber} - {inv.CustomerName}",
            subtitle: $"${inv.TotalAmount:N2} - Due: {inv.DueDate:MMM dd, yyyy}",
            badge: inv.Status == "Paid" ? BadgeVariant.Success :
                   inv.Status == "Overdue" ? BadgeVariant.Destructive :
                   BadgeVariant.Secondary
        ));

        var recentTransactions = transactions.Take(5).Select(trans => new ListItem(
            title: trans.Description,
            subtitle: $"${trans.DebitAmount:N2} / ${trans.CreditAmount:N2} - {trans.TransactionDate:MMM dd, yyyy}",
            badge: "Transaction"
        ));

        // AI Features Section
        var aiFeatures = new[]
        {
            new Card(
                Layout.Horizontal()
                    .Gap(2)
                    .Padding(4)
                    .Add(Icons.Bot.ToIcon())
                    .Add(Layout.Vertical()
                        .Gap(1)
                        .Add(Text.Small("AI-Powered Matching"))
                        .Add(Text.Small("95% automated bank reconciliation")))),
            
            new Card(
                Layout.Horizontal()
                    .Gap(2)
                    .Padding(4)
                    .Add(Icons.Smartphone.ToIcon())
                    .Add(Layout.Vertical()
                        .Gap(1)
                        .Add(Text.Small("Mobile Receipt Capture"))
                        .Add(Text.Small("Scan receipts with your phone")))),
            
            new Card(
                Layout.Horizontal()
                    .Gap(2)
                    .Padding(4)
                    .Add(Icons.RefreshCw.ToIcon())
                    .Add(Layout.Vertical()
                        .Gap(1)
                        .Add(Text.Small("Real-time Sync"))
                        .Add(Text.Small("Always up-to-date data"))))
        };

        return Layout.Vertical()
            .Gap(4)
            .Padding(4)
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(Text.H2("Accounting Dashboard"))
                .Add(new Button("📷 Scan Receipt")
                    .Icon(Icons.Camera)
                    .Variant(ButtonVariant.Primary))
                .Add(new Button("⚡ Quick Invoice")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Outline)))
            .Add(Layout.Grid()
                .Columns(4)
                .Gap(4)
                .Add(kpiCards))
            .Add(Layout.Vertical()
                .Gap(4)
                .Add(Text.H3("Quick Actions"))
                .Add(Layout.Grid()
                    .Columns(3)
                    .Gap(4)
                    .Add(quickActions)))
            .Add(Layout.Grid()
                .Columns(2)
                .Gap(4)
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add(Text.H3("Recent Invoices"))
                    .Add(invoices.Count > 0 
                        ? new List(recentInvoices)
                        : new Card(
                            Layout.Vertical()
                                .Gap(2)
                                .Padding(4)
                                .Add(Text.H4("No invoices yet"))
                                .Add(Text.P("Create your first invoice to get started")))))
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add(Text.H3("Recent Transactions"))
                    .Add(transactions.Count > 0 
                        ? new List(recentTransactions)
                        : new Card(
                            Layout.Vertical()
                                .Gap(2)
                                .Padding(4)
                                .Add(Text.H4("No transactions yet"))
                                .Add(Text.P("Your financial activity will appear here"))))))
            .Add(Layout.Vertical()
                .Gap(4)
                .Add(Text.H3("AI-Powered Features"))
                .Add(Layout.Grid()
                    .Columns(3)
                    .Gap(4)
                    .Add(aiFeatures)));
    }
}