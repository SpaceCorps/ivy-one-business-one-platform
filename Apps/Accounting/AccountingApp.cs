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
        var seeded = this.UseState(false);

        // Seed mock data if needed
        this.UseEffect(() =>
        {
            if (!seeded.Value)
            {
                seeded.Value = true;
                System.Threading.Tasks.Task.Run(async () =>
                {
                    // Wait a bit for the app to fully initialize
                    await System.Threading.Tasks.Task.Delay(1000);
                    await SeedAccountingDataAsync();
                });
            }
        }, []);

        return this.UseBlades(() => new AccountingMenuBlade(), "Accounting");
    }

    private static async System.Threading.Tasks.Task SeedAccountingDataAsync()
    {
        try
        {
            // Create a new DbContext directly
            var connectionString = "Data Source=business_platform.db";
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite(connectionString);

            using var db = new ApplicationDbContext(optionsBuilder.Options);

            // Only seed if no invoices exist
            if (await db.Invoices.AnyAsync())
                return;

            // Seed invoices
            var invoices = new[]
            {
            new Invoice
            {
                InvoiceNumber = "INV-2024-001",
                IssueDate = DateTime.UtcNow.AddDays(-30),
                DueDate = DateTime.UtcNow.AddDays(-15),
                Amount = 4500.00m,
                TaxAmount = 450.00m,
                TotalAmount = 4950.00m,
                Status = "Overdue",
                CustomerName = "Acme Corporation",
                CustomerEmail = "accounts@acme.com",
                Description = "Consulting services for Q4"
            },
            new Invoice
            {
                InvoiceNumber = "INV-2024-002",
                IssueDate = DateTime.UtcNow.AddDays(-25),
                DueDate = DateTime.UtcNow.AddDays(-10),
                Amount = 7500.00m,
                TaxAmount = 750.00m,
                TotalAmount = 8250.00m,
                Status = "Paid",
                CustomerName = "TechStart Inc",
                CustomerEmail = "billing@techstart.com",
                Description = "Software development - Phase 1"
            },
            new Invoice
            {
                InvoiceNumber = "INV-2024-003",
                IssueDate = DateTime.UtcNow.AddDays(-20),
                DueDate = DateTime.UtcNow.AddDays(-5),
                Amount = 3200.00m,
                TaxAmount = 320.00m,
                TotalAmount = 3520.00m,
                Status = "Paid",
                CustomerName = "Global Enterprises",
                CustomerEmail = "finance@global-ent.com",
                Description = "Monthly maintenance services"
            },
            new Invoice
            {
                InvoiceNumber = "INV-2024-004",
                IssueDate = DateTime.UtcNow.AddDays(-15),
                DueDate = DateTime.UtcNow,
                Amount = 5800.00m,
                TaxAmount = 580.00m,
                TotalAmount = 6380.00m,
                Status = "Sent",
                CustomerName = "Innovate Labs",
                CustomerEmail = "ap@innovatelabs.com",
                Description = "Custom integration services"
            },
            new Invoice
            {
                InvoiceNumber = "INV-2024-005",
                IssueDate = DateTime.UtcNow.AddDays(-10),
                DueDate = DateTime.UtcNow.AddDays(5),
                Amount = 2100.00m,
                TaxAmount = 210.00m,
                TotalAmount = 2310.00m,
                Status = "Sent",
                CustomerName = "SmartRetail Co",
                CustomerEmail = "accounting@smartretail.com",
                Description = "Training and support services"
            },
            new Invoice
            {
                InvoiceNumber = "INV-2024-006",
                IssueDate = DateTime.UtcNow.AddDays(-5),
                DueDate = DateTime.UtcNow.AddDays(10),
                Amount = 9500.00m,
                TaxAmount = 950.00m,
                TotalAmount = 10450.00m,
                Status = "Draft",
                CustomerName = "MegaCorp Industries",
                CustomerEmail = "payables@megacorp.com",
                Description = "Enterprise solution - Phase 2"
            },
            new Invoice
            {
                InvoiceNumber = "INV-2024-007",
                IssueDate = DateTime.UtcNow.AddDays(-3),
                DueDate = DateTime.UtcNow.AddDays(12),
                Amount = 1850.00m,
                TaxAmount = 185.00m,
                TotalAmount = 2035.00m,
                Status = "Sent",
                CustomerName = "BrightFuture LLC",
                CustomerEmail = "billing@brightfuture.com",
                Description = "UI/UX design consultation"
            },
            new Invoice
            {
                InvoiceNumber = "INV-2024-008",
                IssueDate = DateTime.UtcNow.AddDays(-1),
                DueDate = DateTime.UtcNow.AddDays(14),
                Amount = 6700.00m,
                TaxAmount = 670.00m,
                TotalAmount = 7370.00m,
                Status = "Draft",
                CustomerName = "DataDrive Systems",
                CustomerEmail = "accounts@datadrive.com",
                Description = "Cloud migration services"
            }
        };

            db.Invoices.AddRange(invoices);
            await db.SaveChangesAsync();

            // Seed payments
            var payments = new[]
            {
            new Payment
            {
                InvoiceId = invoices[1].Id,
                Amount = 8250.00m,
                PaymentDate = DateTime.UtcNow.AddDays(-10),
                PaymentMethod = "Bank Transfer",
                Reference = "PMT-2024-001",
                Notes = "Full payment via wire transfer"
            },
            new Payment
            {
                InvoiceId = invoices[2].Id,
                Amount = 3520.00m,
                PaymentDate = DateTime.UtcNow.AddDays(-5),
                PaymentMethod = "Credit Card",
                Reference = "PMT-2024-002",
                Notes = "Paid by Visa ending in 4532"
            },
            new Payment
            {
                InvoiceId = invoices[1].Id,
                Amount = 2000.00m,
                PaymentDate = DateTime.UtcNow.AddDays(-20),
                PaymentMethod = "Check",
                Reference = "PMT-2024-003",
                Notes = "Partial payment - Check #2456"
            }
        };

            db.Payments.AddRange(payments);
            await db.SaveChangesAsync();

            // Seed transactions
            var transactions = new[]
            {
            new Transaction
            {
                TransactionNumber = "TXN-2024-001",
                TransactionDate = DateTime.UtcNow.AddDays(-30),
                Description = "Revenue from INV-2024-002",
                DebitAmount = 8250.00m,
                CreditAmount = 0,
                Reference = "INV-2024-002"
            },
            new Transaction
            {
                TransactionNumber = "TXN-2024-002",
                TransactionDate = DateTime.UtcNow.AddDays(-28),
                Description = "Office rent payment",
                DebitAmount = 3500.00m,
                CreditAmount = 0,
                Reference = "RENT-OCT-2024"
            },
            new Transaction
            {
                TransactionNumber = "TXN-2024-003",
                TransactionDate = DateTime.UtcNow.AddDays(-25),
                Description = "Equipment purchase",
                DebitAmount = 5400.00m,
                CreditAmount = 0,
                Reference = "PO-002"
            },
            new Transaction
            {
                TransactionNumber = "TXN-2024-004",
                TransactionDate = DateTime.UtcNow.AddDays(-20),
                Description = "Revenue from INV-2024-003",
                DebitAmount = 3520.00m,
                CreditAmount = 0,
                Reference = "INV-2024-003"
            },
            new Transaction
            {
                TransactionNumber = "TXN-2024-005",
                TransactionDate = DateTime.UtcNow.AddDays(-18),
                Description = "Utility bills payment",
                DebitAmount = 850.00m,
                CreditAmount = 0,
                Reference = "UTIL-OCT-2024"
            },
            new Transaction
            {
                TransactionNumber = "TXN-2024-006",
                TransactionDate = DateTime.UtcNow.AddDays(-15),
                Description = "Payroll expenses",
                DebitAmount = 15600.00m,
                CreditAmount = 0,
                Reference = "PAYROLL-OCT-2024"
            },
            new Transaction
            {
                TransactionNumber = "TXN-2024-007",
                TransactionDate = DateTime.UtcNow.AddDays(-12),
                Description = "Software subscriptions",
                DebitAmount = 1250.00m,
                CreditAmount = 0,
                Reference = "SUBS-OCT-2024"
            },
            new Transaction
            {
                TransactionNumber = "TXN-2024-008",
                TransactionDate = DateTime.UtcNow.AddDays(-10),
                Description = "Marketing expenses",
                DebitAmount = 2800.00m,
                CreditAmount = 0,
                Reference = "MARKETING-OCT-2024"
            },
            new Transaction
            {
                TransactionNumber = "TXN-2024-009",
                TransactionDate = DateTime.UtcNow.AddDays(-8),
                Description = "Client refund",
                DebitAmount = 0,
                CreditAmount = 750.00m,
                Reference = "REFUND-2024-001"
            },
            new Transaction
            {
                TransactionNumber = "TXN-2024-010",
                TransactionDate = DateTime.UtcNow.AddDays(-5),
                Description = "Office supplies purchase",
                DebitAmount = 425.00m,
                CreditAmount = 0,
                Reference = "PO-001"
            },
            new Transaction
            {
                TransactionNumber = "TXN-2024-011",
                TransactionDate = DateTime.UtcNow.AddDays(-3),
                Description = "Travel expenses reimbursement",
                DebitAmount = 1340.00m,
                CreditAmount = 0,
                Reference = "EXP-2024-045"
            },
            new Transaction
            {
                TransactionNumber = "TXN-2024-012",
                TransactionDate = DateTime.UtcNow.AddDays(-1),
                Description = "Bank service charges",
                DebitAmount = 85.00m,
                CreditAmount = 0,
                Reference = "BANK-FEES-OCT"
            }
        };

            db.Transactions.AddRange(transactions);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log or ignore seeding errors
            Console.WriteLine($"Error seeding accounting data: {ex.Message}");
        }
    }
}

// Menu blade - shows list of accounting sections
public class AccountingMenuBlade : ViewBase
{
    public override object? Build()
    {
        var blades = this.UseContext<IBladeController>();
        var db = this.UseService<ApplicationDbContext>();

        var invoicesCount = db.Invoices.Count();
        var accountsCount = db.Accounts.Count();
        var transactionsCount = db.Transactions.Count();
        var paymentsCount = db.Payments.Count();

        Func<Event<ListItem>, ValueTask> onDashboardClick = e => { blades.Push(this, new DashboardBlade(), "Dashboard"); return ValueTask.CompletedTask; };
        Func<Event<ListItem>, ValueTask> onInvoicesClick = e => { blades.Push(this, new InvoicesBlade(), "Invoices"); return ValueTask.CompletedTask; };
        Func<Event<ListItem>, ValueTask> onPaymentsClick = e => { blades.Push(this, new PaymentsBlade(), "Payments"); return ValueTask.CompletedTask; };
        Func<Event<ListItem>, ValueTask> onAccountsClick = e => { blades.Push(this, new AccountsBlade(), "Accounts"); return ValueTask.CompletedTask; };
        Func<Event<ListItem>, ValueTask> onTransactionsClick = e => { blades.Push(this, new TransactionsBlade(), "Transactions"); return ValueTask.CompletedTask; };
        Func<Event<ListItem>, ValueTask> onReportsClick = e => { blades.Push(this, new ReportsBlade(), "Reports"); return ValueTask.CompletedTask; };

        var menuItems = new[]
        {
            new ListItem(
                icon: Icons.TrendingUp,
                title: "Dashboard",
                subtitle: "Overview of your finances",
                onClick: onDashboardClick
            ),
            new ListItem(
                icon: Icons.FileText,
                title: "Invoices",
                subtitle: $"{invoicesCount} invoices",
                badge: invoicesCount > 0 ? invoicesCount.ToString() : null,
                onClick: onInvoicesClick
            ),
            new ListItem(
                icon: Icons.Receipt,
                title: "Payments",
                subtitle: $"{paymentsCount} payments",
                badge: paymentsCount > 0 ? paymentsCount.ToString() : null,
                onClick: onPaymentsClick
            ),
            new ListItem(
                icon: Icons.Book,
                title: "Chart of Accounts",
                subtitle: $"{accountsCount} accounts",
                badge: accountsCount > 0 ? accountsCount.ToString() : null,
                onClick: onAccountsClick
            ),
            new ListItem(
                icon: Icons.List,
                title: "Transactions",
                subtitle: $"{transactionsCount} transactions",
                badge: transactionsCount > 0 ? transactionsCount.ToString() : null,
                onClick: onTransactionsClick
            ),
            new ListItem(
                icon: Icons.TrendingUp,
                title: "Reports",
                subtitle: "Financial reports and analytics",
                onClick: onReportsClick
            )
        };

        return new List(menuItems);
    }
}

// Dashboard blade - shows financial overview
public class DashboardBlade : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();

        var invoices = db.Invoices.ToList();
        var transactions = db.Transactions.ToList();

        var totalRevenue = invoices.Where(i => i.Status == "Paid").Sum(i => i.TotalAmount);
        var pendingInvoices = invoices.Where(i => i.Status != "Paid").Sum(i => i.TotalAmount);
        var overdueInvoices = invoices.Where(i => i.Status == "Overdue").Sum(i => i.TotalAmount);
        var totalExpenses = transactions.Where(t => t.DebitAmount > 0).Sum(t => t.DebitAmount);

        // Monthly revenue trend data
        var monthlyRevenue = invoices
            .Where(i => i.Status == "Paid" && i.IssueDate >= DateTime.UtcNow.AddMonths(-12))
            .GroupBy(i => new { i.IssueDate.Year, i.IssueDate.Month })
            .Select(g => new { 
                Month = $"{g.Key.Year}-{g.Key.Month:D2}", 
                Revenue = g.Sum(i => i.TotalAmount) 
            })
            .OrderBy(x => x.Month)
            .ToArray();

        // Invoice status distribution
        var invoiceStatusData = invoices
            .GroupBy(i => i.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToArray();

        // Monthly expense trend
        var monthlyExpenses = transactions
            .Where(t => t.DebitAmount > 0 && t.TransactionDate >= DateTime.UtcNow.AddMonths(-12))
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .Select(g => new { 
                Month = $"{g.Key.Year}-{g.Key.Month:D2}", 
                Expenses = g.Sum(t => t.DebitAmount) 
            })
            .OrderBy(x => x.Month)
            .ToArray();

        // Top customers by revenue
        var topCustomers = invoices
            .Where(i => i.Status == "Paid")
            .GroupBy(i => i.CustomerName)
            .Select(g => new { Customer = g.Key, Revenue = g.Sum(i => i.TotalAmount) })
            .OrderByDescending(x => x.Revenue)
            .Take(8)
            .ToArray();

        // Daily revenue (last 30 days)
        var dailyRevenue = invoices
            .Where(i => i.Status == "Paid" && i.IssueDate >= DateTime.UtcNow.AddDays(-30))
            .GroupBy(i => i.IssueDate.Date)
            .Select(g => new { 
                Date = g.Key.ToString("MMM dd"), 
                Revenue = g.Sum(i => i.TotalAmount) 
            })
            .OrderBy(x => x.Date)
            .ToArray();

        // Expense categories
        var expenseCategories = transactions
            .Where(t => t.DebitAmount > 0)
            .GroupBy(t => t.Description.Split(' ')[0]) // First word as category
            .Select(g => new { Category = g.Key, Amount = g.Sum(t => t.DebitAmount) })
            .OrderByDescending(x => x.Amount)
            .Take(10)
            .ToArray();

        return Layout.Vertical()
            .Gap(3)
            .Add(Layout.Grid()
                .Columns(2)
                .Gap(3)
                .Add(new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Add(Layout.Horizontal()
                        .Gap(2)
                        .Add(Icons.TrendingUp.ToIcon())
                        .Add(Text.Small("TOTAL REVENUE")))
                        .Add(Text.H2($"${totalRevenue:N0}"))
                        .Add(Text.Small("From paid invoices"))))
                .Add(new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Add(Layout.Horizontal()
                        .Gap(2)
                        .Add(Icons.Clock.ToIcon())
                            .Add(Text.Small("PENDING")))
                        .Add(Text.H2($"${pendingInvoices:N0}"))
                        .Add(Text.Small("Unpaid invoices"))))
                .Add(new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Add(Layout.Horizontal()
                        .Gap(2)
                        .Add(Icons.X.ToIcon())
                        .Add(Text.Small("OVERDUE")))
                        .Add(Text.H2($"${overdueInvoices:N0}"))
                        .Add(Text.Small("Late payments"))))
                .Add(new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Add(Layout.Horizontal()
                        .Gap(2)
                        .Add(Icons.CreditCard.ToIcon())
                            .Add(Text.Small("EXPENSES")))
                        .Add(Text.H2($"${totalExpenses:N0}"))
                        .Add(Text.Small("Business expenses")))))
            .Add(new Card(
                monthlyRevenue.Length > 0
                    ? monthlyRevenue.ToLineChart(style: LineChartStyles.Dashboard)
                        .Dimension("Month", e => e.Month)
                        .Measure("Revenue", e => e.Sum(f => f.Revenue))
                    : Text.Small("No revenue data")
            ).Title("Revenue Trend (12 months)"))
            .Add(Layout.Horizontal()
                .Gap(3)
                .Add(new Card(
                    invoiceStatusData.Length > 0
                        ? invoiceStatusData.ToPieChart(
                            e => e.Status,
                            e => e.Sum(f => f.Count),
                            PieChartStyles.Donut
                        )
                        : Text.Small("No invoice data")
                ).Title("Invoice Status Distribution"))
                .Add(new Card(
                    monthlyExpenses.Length > 0
                        ? monthlyExpenses.ToBarChart()
                            .Dimension("Month", e => e.Month)
                            .Measure("Expenses", e => e.Sum(f => f.Expenses))
                        : Text.Small("No expense data")
                ).Title("Monthly Expenses")))
            .Add(Layout.Horizontal()
                .Gap(3)
                .Add(new Card(
                    topCustomers.Length > 0
                        ? topCustomers.ToBarChart()
                            .Dimension("Customer", e => e.Customer)
                            .Measure("Revenue", e => e.Sum(f => f.Revenue))
                        : Text.Small("No customer data")
                ).Title("Top Customers by Revenue"))
                .Add(new Card(
                    dailyRevenue.Length > 0
                        ? dailyRevenue.ToLineChart(style: LineChartStyles.Dashboard)
                            .Dimension("Date", e => e.Date)
                            .Measure("Revenue", e => e.Sum(f => f.Revenue))
                        : Text.Small("No daily data")
                ).Title("Daily Revenue (30 days)")))
            .Add(new Card(
                expenseCategories.Length > 0
                    ? expenseCategories.ToBarChart()
                        .Dimension("Category", e => e.Category)
                        .Measure("Amount", e => e.Sum(f => f.Amount))
                    : Text.Small("No expense data")
            ).Title("Top Expense Categories"))
            .Add(new Card(
                invoices.Count > 0
                    ? new List(invoices.Take(10).Select(inv => new ListItem(
                        title: $"#{inv.InvoiceNumber} - {inv.CustomerName}",
                        subtitle: $"${inv.TotalAmount:N2} - Due: {inv.DueDate:MMM dd, yyyy}",
                        badge: inv.Status
                    )))
                    : Layout.Vertical()
                        .Gap(2)
                        .Padding(4)
                        .Add(Text.H4("No invoices yet"))
                        .Add(Text.P("Create your first invoice to get started"))
            ).Title("Recent Invoices"));
    }
}

// Invoices blade - shows list of invoices with search and create
public class InvoicesBlade : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var searchQuery = this.UseState("");

        // Refresh when returning from detail view
        this.UseEffect(() =>
        {
            if (refreshToken.ReturnValue != null)
            {
                blades.Pop(this, true);
            }
        }, [refreshToken]);

        var allInvoices = db.Invoices.ToList();
        var invoices = allInvoices
            .Where(i => searchQuery.Value == "" ||
                       i.InvoiceNumber.Contains(searchQuery.Value) ||
                       i.CustomerName.Contains(searchQuery.Value))
            .OrderByDescending(i => i.IssueDate)
            .ToList();

        var onInvoiceClick = new Action<Event<ListItem>>(e =>
        {
            var invoiceId = (int)e.Sender.Tag!;
            var invoiceNumber = e.Sender.Title;
            blades.Push(this, new InvoiceDetailBlade(invoiceId), invoiceNumber);
        });

        var items = invoices.Select(invoice => new ListItem(
            title: $"#{invoice.InvoiceNumber} - {invoice.CustomerName}",
            subtitle: $"${invoice.TotalAmount:N2} - Due: {invoice.DueDate:MMM dd, yyyy}",
            badge: invoice.Status,
            onClick: onInvoiceClick,
            tag: invoice.Id
        ));

        // Invoice status distribution
        var invoiceStatusData = allInvoices
            .GroupBy(i => i.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToArray();

        // Monthly invoice amounts
        var monthlyInvoices = allInvoices
            .Where(i => i.IssueDate >= DateTime.UtcNow.AddMonths(-12))
            .GroupBy(i => new { i.IssueDate.Year, i.IssueDate.Month })
            .Select(g => new { 
                Month = $"{g.Key.Year}-{g.Key.Month:D2}", 
                Amount = g.Sum(i => i.TotalAmount),
                Count = g.Count()
            })
            .OrderBy(x => x.Month)
            .ToArray();

        // Top customers by invoice amount
        var topCustomers = allInvoices
            .GroupBy(i => i.CustomerName)
            .Select(g => new { Customer = g.Key, Amount = g.Sum(i => i.TotalAmount) })
            .OrderByDescending(x => x.Amount)
            .Take(8)
            .ToArray();

        // Invoice amounts by status
        var invoiceAmountsByStatus = allInvoices
            .GroupBy(i => i.Status)
            .Select(g => new { Status = g.Key, Amount = g.Sum(i => i.TotalAmount) })
            .ToArray();

        // Average invoice amount by month
        var avgInvoiceAmounts = allInvoices
            .Where(i => i.IssueDate >= DateTime.UtcNow.AddMonths(-12))
            .GroupBy(i => new { i.IssueDate.Year, i.IssueDate.Month })
            .Select(g => new { 
                Month = $"{g.Key.Year}-{g.Key.Month:D2}", 
                AvgAmount = g.Average(i => i.TotalAmount)
            })
            .OrderBy(x => x.Month)
            .ToArray();

        // Payment timing analysis
        var paymentTiming = allInvoices
            .Where(i => i.Status == "Paid")
            .Select(i => new { 
                DaysToPay = (i.UpdatedAt - i.IssueDate).Days,
                Amount = i.TotalAmount 
            })
            .GroupBy(i => i.DaysToPay / 10 * 10) // Group by 10-day intervals
            .Select(g => new { 
                DaysRange = $"{g.Key}-{g.Key + 9} days", 
                Count = g.Count(),
                AvgAmount = g.Average(i => i.Amount)
            })
            .OrderBy(x => x.DaysRange)
            .Take(8)
            .ToArray();

        var createButton = new Button(icon: Icons.Plus, variant: ButtonVariant.Outline).WithSheet(
            () => new CreateInvoiceSheet(refreshToken),
            title: "Create New Invoice",
            description: "Fill in the details to create a new invoice",
            width: Size.Fraction(1 / 2f)
        );

        var createButtonFull = new Button("Create Invoice")
            .Icon(Icons.Plus)
            .Variant(ButtonVariant.Primary)
            .WithSheet(
                () => new CreateInvoiceSheet(refreshToken),
                title: "Create New Invoice",
                description: "Fill in the details to create a new invoice",
                width: Size.Fraction(1 / 2f)
            );

        object content;
        
        if (invoices.Count > 0)
        {
            content = Layout.Vertical()
                .Gap(3)
                .Add(Layout.Horizontal()
                    .Gap(3)
                    .Add(new Card(
                        invoiceStatusData.Length > 0
                            ? invoiceStatusData.ToPieChart(
                                e => e.Status,
                                e => e.Sum(f => f.Count),
                                PieChartStyles.Donut
                            )
                            : Text.Small("No data")
                    ).Title("Invoice Status Distribution"))
                    .Add(new Card(
                        monthlyInvoices.Length > 0
                            ? monthlyInvoices.ToLineChart(style: LineChartStyles.Dashboard)
                                .Dimension("Month", e => e.Month)
                                .Measure("Amount", e => e.Sum(f => f.Amount))
                            : Text.Small("No data")
                    ).Title("Monthly Invoice Amounts")))
                .Add(Layout.Horizontal()
                    .Gap(3)
                    .Add(new Card(
                        topCustomers.Length > 0
                            ? topCustomers.ToBarChart()
                                .Dimension("Customer", e => e.Customer)
                                .Measure("Amount", e => e.Sum(f => f.Amount))
                            : Text.Small("No data")
                    ).Title("Top Customers by Invoice Amount"))
                    .Add(new Card(
                        invoiceAmountsByStatus.Length > 0
                            ? invoiceAmountsByStatus.ToBarChart()
                                .Dimension("Status", e => e.Status)
                                .Measure("Amount", e => e.Sum(f => f.Amount))
                            : Text.Small("No data")
                    ).Title("Invoice Amounts by Status")))
                .Add(Layout.Horizontal()
                    .Gap(3)
                    .Add(new Card(
                        avgInvoiceAmounts.Length > 0
                            ? avgInvoiceAmounts.ToLineChart(style: LineChartStyles.Dashboard)
                                .Dimension("Month", e => e.Month)
                                .Measure("AvgAmount", e => e.Sum(f => f.AvgAmount))
                            : Text.Small("No data")
                    ).Title("Average Invoice Amount by Month"))
                    .Add(new Card(
                        paymentTiming.Length > 0
                            ? paymentTiming.ToBarChart()
                                .Dimension("DaysRange", e => e.DaysRange)
                                .Measure("Count", e => e.Sum(f => f.Count))
                            : Text.Small("No data")
                    ).Title("Payment Timing Analysis")))
                .Add(new List(items));
        }
        else
        {
            content = new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Padding(4)
                    .Add(Text.H4("No invoices found"))
                    .Add(Text.P("Create your first invoice to get started"))
                    .Add(createButtonFull));
        }

        return BladeHelper.WithHeader(
            Layout.Horizontal()
                        .Gap(2)
                .Add(searchQuery.ToTextInput().Placeholder("Search invoices..."))
                .Add(createButton),
            content
        );
    }
}

// Create invoice sheet
public class CreateInvoiceSheet : ViewBase
{
    private readonly RefreshToken _refreshToken;

    public CreateInvoiceSheet(RefreshToken refreshToken)
    {
        _refreshToken = refreshToken;
    }

    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
        var client = this.UseService<IClientProvider>();

        var customerName = this.UseState("");
        var customerEmail = this.UseState("");
        var description = this.UseState("");
        var amount = this.UseState(0m);
        var taxAmount = this.UseState(0m);
        var dueDate = this.UseState(DateTime.UtcNow.AddDays(30));

        var onSave = new Action<Event<Button>>(_ =>
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(customerName.Value))
                {
                    client.Toast("Customer name is required");
                    return;
                }

                if (amount.Value <= 0)
                {
                    client.Toast("Amount must be greater than zero");
                    return;
                }

                // Generate invoice number
                var lastInvoice = db.Invoices
                    .OrderByDescending(i => i.Id)
                    .FirstOrDefault();
                
                var invoiceNumber = lastInvoice != null 
                    ? $"INV-2024-{(int.Parse(lastInvoice.InvoiceNumber.Split('-')[2]) + 1):D3}"
                    : "INV-2024-001";

                var totalAmount = amount.Value + taxAmount.Value;

                var invoice = new Invoice
                {
                    InvoiceNumber = invoiceNumber,
                    IssueDate = DateTime.UtcNow,
                    DueDate = dueDate.Value,
                    Amount = amount.Value,
                    TaxAmount = taxAmount.Value,
                    TotalAmount = totalAmount,
                    Status = "Draft",
                    CustomerName = customerName.Value,
                    CustomerEmail = customerEmail.Value,
                    Description = description.Value
                };

                db.Invoices.Add(invoice);
                db.SaveChanges();

                _refreshToken.Refresh(invoice.Id);
                client.Toast($"Invoice {invoiceNumber} created successfully!");
            }
            catch (Exception ex)
            {
                client.Toast($"Error creating invoice: {ex.Message}");
            }
        });

        return Layout.Vertical()
            .Gap(3)
            .Add(new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Add(Text.Small("Customer Name *"))
                    .Add(customerName.ToTextInput().Placeholder("Enter customer name"))
                    .Add(Text.Small("Customer Email"))
                    .Add(customerEmail.ToTextInput().Placeholder("customer@example.com"))
                    .Add(Text.Small("Description"))
                    .Add(description.ToTextInput().Placeholder("Services provided..."))
            ).Title("Customer Information"))
            .Add(new Card(
                Layout.Vertical()
                        .Gap(2)
                    .Add(Text.Small("Amount *"))
                    .Add(amount.ToNumberInput().Placeholder("0.00"))
                    .Add(Text.Small("Tax Amount"))
                    .Add(taxAmount.ToNumberInput().Placeholder("0.00"))
                    .Add(Text.Small("Total"))
                    .Add(Text.H3($"${amount.Value + taxAmount.Value:N2}"))
            ).Title("Amounts"))
            .Add(new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Add(Text.Small("Due Date"))
                    .Add(dueDate.ToDateInput())
            ).Title("Payment Terms"))
                    .Add(Layout.Horizontal()
                        .Gap(2)
                .Add(new Button("Create Invoice")
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(onSave)
                    .Width(Size.Full())));
    }
}

// Invoice detail blade - shows single invoice with sheet for editing
public class InvoiceDetailBlade : ViewBase
{
    private readonly int _invoiceId;

    public InvoiceDetailBlade(int invoiceId)
    {
        _invoiceId = invoiceId;
    }

    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
        var client = this.UseService<IClientProvider>();
        var blades = this.UseContext<IBladeController>();

        var invoice = db.Invoices.FirstOrDefault(i => i.Id == _invoiceId);
        if (invoice == null)
        {
            return new Card("Invoice not found");
        }

        var onDelete = new Action<Event<Button>>(_ =>
        {
            try
            {
                var invoiceToDelete = db.Invoices.FirstOrDefault(i => i.Id == _invoiceId);
                if (invoiceToDelete != null)
                {
                    var invoiceNumber = invoiceToDelete.InvoiceNumber;
                    db.Invoices.Remove(invoiceToDelete);
                    db.SaveChanges();
                    
                    client.Toast($"Invoice {invoiceNumber} deleted successfully!");
                    blades.Pop(this, true); // Pop this blade and refresh the previous one
                }
            }
            catch (Exception ex)
            {
                client.Toast($"Error deleting invoice: {ex.Message}");
            }
        });

        return Layout.Vertical()
            .Gap(3)
                    .Add(Layout.Horizontal()
                        .Gap(2)
                .Add(new Button("Edit").Icon(Icons.Pencil).Variant(ButtonVariant.Outline).WithSheet(
                    () => new EditInvoiceSheet(invoice),
                    title: "Edit Invoice",
                    description: $"Editing invoice #{invoice.InvoiceNumber}",
                    width: Size.Fraction(1 / 2f)
                ))
                .Add(new Button("Delete").Icon(Icons.Trash).Variant(ButtonVariant.Destructive).HandleClick(onDelete)))
            .Add(new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Add(Layout.Horizontal().Gap(2).Add(Text.Small("Invoice Number:")).Add(Text.Block(invoice.InvoiceNumber)))
                    .Add(Layout.Horizontal().Gap(2).Add(Text.Small("Customer:")).Add(Text.Block(invoice.CustomerName)))
                    .Add(Layout.Horizontal().Gap(2).Add(Text.Small("Status:")).Add(new Badge(invoice.Status)))
            ).Title("Invoice Details"))
            .Add(new Card(
                Layout.Vertical()
                        .Gap(2)
                    .Add(Layout.Horizontal().Gap(2).Add(Text.Small("Amount:")).Add(Text.Block($"${invoice.Amount:N2}")))
                    .Add(Layout.Horizontal().Gap(2).Add(Text.Small("Tax:")).Add(Text.Block($"${invoice.TaxAmount:N2}")))
                    .Add(Layout.Horizontal().Gap(2).Add(Text.Small("Total:")).Add(Text.H3($"${invoice.TotalAmount:N2}")))
            ).Title("Amounts"))
            .Add(new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Add(Layout.Horizontal().Gap(2).Add(Text.Small("Issue Date:")).Add(Text.Block(invoice.IssueDate.ToString("MMM dd, yyyy"))))
                    .Add(Layout.Horizontal().Gap(2).Add(Text.Small("Due Date:")).Add(Text.Block(invoice.DueDate.ToString("MMM dd, yyyy"))))
            ).Title("Dates"));
    }
}

// Edit invoice sheet
public class EditInvoiceSheet : ViewBase
{
    private readonly Invoice _invoice;

    public EditInvoiceSheet(Invoice invoice)
    {
        _invoice = invoice;
    }

    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
        var client = this.UseService<IClientProvider>();
        
        var customerName = this.UseState(_invoice.CustomerName);
        var amount = this.UseState(_invoice.Amount);
        var tax = this.UseState(_invoice.TaxAmount);
        var status = this.UseState(_invoice.Status);

        var onSave = new Action<Event<Button>>(_ =>
        {
            try
            {
                // Find and update the invoice
                var invoice = db.Invoices.FirstOrDefault(i => i.Id == _invoice.Id);
                if (invoice != null)
                {
                    invoice.CustomerName = customerName.Value;
                    invoice.Amount = amount.Value;
                    invoice.TaxAmount = tax.Value;
                    invoice.TotalAmount = amount.Value + tax.Value;
                    invoice.Status = status.Value;
                    invoice.UpdatedAt = DateTime.UtcNow;
                    
                    db.SaveChanges();
                    client.Toast($"Invoice {invoice.InvoiceNumber} updated successfully!");
                }
                else
                {
                    client.Toast("Invoice not found");
                }
            }
            catch (Exception ex)
            {
                client.Toast($"Error updating invoice: {ex.Message}");
            }
        });

        return Layout.Vertical()
            .Gap(3)
            .Add(new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Add(Text.Small("Customer Name"))
                    .Add(customerName.ToTextInput())
                    .Add(Text.Small("Amount"))
                    .Add(amount.ToNumberInput())
                    .Add(Text.Small("Tax"))
                    .Add(tax.ToNumberInput())
                    .Add(Text.Small("Total"))
                    .Add(Text.H3($"${amount.Value + tax.Value:N2}"))
                    .Add(Text.Small("Status"))
                    .Add(new SelectInput<string>(
                        options: new[] { "Draft", "Sent", "Paid", "Overdue" }.ToOptions(),
                        value: status.Value,
                        onChange: e => { status.Value = e.Value; return ValueTask.CompletedTask; }
                    ))
            ).Title("Invoice Information"))
                    .Add(Layout.Horizontal()
                        .Gap(2)
                .Add(new Button("Save Changes").Variant(ButtonVariant.Primary).HandleClick(onSave)));
    }
}

// Payments blade
public class PaymentsBlade : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
        var searchQuery = this.UseState("");

        var allPayments = db.Payments.ToList();
        var payments = allPayments
            .Where(p => searchQuery.Value == "" ||
                       p.Reference.Contains(searchQuery.Value))
            .OrderByDescending(p => p.PaymentDate)
            .ToList();

        var items = payments.Select(payment => new ListItem(
            title: $"{payment.Reference} - {payment.PaymentMethod}",
            subtitle: $"${payment.Amount:N2} - {payment.PaymentDate:MMM dd, yyyy}"
        ));

        // Payment method distribution
        var paymentMethodData = allPayments
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new { Method = g.Key, Amount = g.Sum(p => p.Amount) })
            .ToArray();

        // Monthly payment amounts
        var monthlyPayments = allPayments
            .Where(p => p.PaymentDate >= DateTime.UtcNow.AddMonths(-12))
            .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
            .Select(g => new { 
                Month = $"{g.Key.Year}-{g.Key.Month:D2}", 
                Amount = g.Sum(p => p.Amount),
                Count = g.Count()
            })
            .OrderBy(x => x.Month)
            .ToArray();

        // Daily payment amounts (last 30 days)
        var dailyPayments = allPayments
            .Where(p => p.PaymentDate >= DateTime.UtcNow.AddDays(-30))
            .GroupBy(p => p.PaymentDate.Date)
            .Select(g => new { 
                Date = g.Key.ToString("MMM dd"), 
                Amount = g.Sum(p => p.Amount),
                Count = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToArray();

        // Payment amount ranges
        var paymentRanges = allPayments
            .Select(p => new { 
                Range = p.Amount switch {
                    < 100 => "< $100",
                    < 500 => "$100-$500",
                    < 1000 => "$500-$1000",
                    < 5000 => "$1000-$5000",
                    _ => ">$5000"
                },
                Amount = p.Amount
            })
            .GroupBy(p => p.Range)
            .Select(g => new { Range = g.Key, Count = g.Count(), Amount = g.Sum(p => p.Amount) })
            .OrderBy(x => x.Range)
            .ToArray();

        // Average payment amount by method
        var avgPaymentByMethod = allPayments
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new { 
                Method = g.Key, 
                AvgAmount = g.Average(p => p.Amount),
                Count = g.Count()
            })
            .OrderByDescending(x => x.AvgAmount)
            .ToArray();

        // Payment frequency by day of week
        var paymentByDayOfWeek = allPayments
            .GroupBy(p => p.PaymentDate.DayOfWeek)
            .Select(g => new { 
                Day = g.Key.ToString(), 
                Count = g.Count(),
                Amount = g.Sum(p => p.Amount)
            })
            .OrderBy(x => x.Day)
            .ToArray();

        object content;
        
        if (payments.Count > 0)
        {
            content = Layout.Vertical()
                .Gap(3)
                .Add(Layout.Horizontal()
                    .Gap(3)
                    .Add(new Card(
                        paymentMethodData.Length > 0
                            ? paymentMethodData.ToPieChart(
                                e => e.Method,
                                e => e.Sum(f => f.Amount),
                                PieChartStyles.Donut
                            )
                            : Text.Small("No data")
                    ).Title("Payment Methods Distribution"))
                    .Add(new Card(
                        monthlyPayments.Length > 0
                            ? monthlyPayments.ToLineChart(style: LineChartStyles.Dashboard)
                                .Dimension("Month", e => e.Month)
                                .Measure("Amount", e => e.Sum(f => f.Amount))
                            : Text.Small("No data")
                    ).Title("Monthly Payment Amounts")))
                .Add(Layout.Horizontal()
                    .Gap(3)
                    .Add(new Card(
                        dailyPayments.Length > 0
                            ? dailyPayments.ToBarChart()
                                .Dimension("Date", e => e.Date)
                                .Measure("Amount", e => e.Sum(f => f.Amount))
                            : Text.Small("No data")
                    ).Title("Daily Payments (30 days)"))
                    .Add(new Card(
                        paymentRanges.Length > 0
                            ? paymentRanges.ToPieChart(
                                e => e.Range,
                                e => e.Sum(f => f.Count),
                                PieChartStyles.Donut
                            )
                            : Text.Small("No data")
                    ).Title("Payment Amount Ranges")))
                .Add(Layout.Horizontal()
                    .Gap(3)
                    .Add(new Card(
                        avgPaymentByMethod.Length > 0
                            ? avgPaymentByMethod.ToBarChart()
                                .Dimension("Method", e => e.Method)
                                .Measure("AvgAmount", e => e.Sum(f => f.AvgAmount))
                            : Text.Small("No data")
                    ).Title("Average Payment by Method"))
                    .Add(new Card(
                        paymentByDayOfWeek.Length > 0
                            ? paymentByDayOfWeek.ToBarChart()
                                .Dimension("Day", e => e.Day)
                                .Measure("Count", e => e.Sum(f => f.Count))
                            : Text.Small("No data")
                    ).Title("Payment Frequency by Day of Week")))
                .Add(new List(items));
        }
        else
        {
            content = new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Padding(4)
                    .Add(Text.H4("No payments found"))
                    .Add(Text.P("Record your first payment to get started")));
        }

        return BladeHelper.WithHeader(
            Layout.Horizontal()
                        .Gap(2)
                .Add(searchQuery.ToTextInput().Placeholder("Search payments..."))
                .Add(new Button(icon: Icons.Plus, variant: ButtonVariant.Outline)),
            content
        );
    }
}

// Accounts blade
public class AccountsBlade : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var searchQuery = this.UseState("");

        var allAccounts = db.Accounts.ToList();
        var accounts = allAccounts
            .Where(a => searchQuery.Value == "" ||
                       a.AccountName.Contains(searchQuery.Value) ||
                       a.AccountNumber.Contains(searchQuery.Value))
            .OrderBy(a => a.AccountNumber)
            .ToList();

        var onAccountClick = new Action<Event<ListItem>>(e =>
        {
            var accountId = (int)e.Sender.Tag!;
            var accountName = e.Sender.Title;
            blades.Push(this, new AccountDetailBlade(accountId), accountName);
        });

        var items = accounts.Select(account => new ListItem(
            title: $"{account.AccountNumber} - {account.AccountName}",
            subtitle: $"{account.AccountType} - Balance: ${account.Balance:N2}",
            onClick: onAccountClick,
            tag: account.Id
        ));

        // Group accounts by type for charts
        var accountsByType = allAccounts
            .GroupBy(a => a.AccountType)
            .Select(g => new { Type = g.Key, Balance = g.Sum(a => a.Balance) })
            .Where(x => x.Balance > 0)
            .ToArray();

        // Top accounts by balance
        var topAccounts = allAccounts
            .Where(a => a.Balance > 0)
            .OrderByDescending(a => a.Balance)
            .Take(10)
            .Select(a => new { Account = a.AccountName, Balance = a.Balance })
            .ToArray();

        // Account balance ranges
        var balanceRanges = allAccounts
            .Select(a => new { 
                Range = a.Balance switch {
                    < 0 => "Negative",
                    < 1000 => "$0-$1K",
                    < 10000 => "$1K-$10K",
                    < 50000 => "$10K-$50K",
                    < 100000 => "$50K-$100K",
                    _ => ">$100K"
                },
                Balance = a.Balance
            })
            .GroupBy(a => a.Range)
            .Select(g => new { Range = g.Key, Count = g.Count(), Balance = g.Sum(a => a.Balance) })
            .OrderBy(x => x.Range)
            .ToArray();

        // Assets vs Liabilities vs Equity
        var balanceSheetData = allAccounts
            .GroupBy(a => a.AccountType)
            .Select(g => new { Type = g.Key, Balance = g.Sum(a => a.Balance) })
            .ToArray();

        // Account distribution by type
        var accountCountByType = allAccounts
            .GroupBy(a => a.AccountType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToArray();

        // Largest positive and negative balances
        var extremeBalances = new[]
        {
            allAccounts.Where(a => a.Balance > 0).OrderByDescending(a => a.Balance).FirstOrDefault(),
            allAccounts.Where(a => a.Balance < 0).OrderBy(a => a.Balance).FirstOrDefault()
        }
        .Where(a => a != null)
        .Select(a => new { Account = a!.AccountName, Balance = a.Balance })
        .ToArray();

        object content;
        
        if (accounts.Count > 0)
        {
            content = Layout.Vertical()
                .Gap(3)
                    .Add(Layout.Horizontal()
                    .Gap(3)
                    .Add(new Card(
                        accountsByType.Length > 0
                            ? accountsByType.ToPieChart(
                                e => e.Type,
                                e => e.Sum(f => f.Balance),
                                PieChartStyles.Donut
                            )
                            : Text.Small("No data")
                    ).Title("Balance by Account Type"))
                    .Add(new Card(
                        topAccounts.Length > 0
                            ? topAccounts.ToBarChart()
                                .Dimension("Account", e => e.Account)
                                .Measure("Balance", e => e.Sum(f => f.Balance))
                            : Text.Small("No data")
                    ).Title("Top Accounts by Balance")))
                .Add(Layout.Horizontal()
                    .Gap(3)
                    .Add(new Card(
                        balanceRanges.Length > 0
                            ? balanceRanges.ToPieChart(
                                e => e.Range,
                                e => e.Sum(f => f.Count),
                                PieChartStyles.Donut
                            )
                            : Text.Small("No data")
                    ).Title("Account Balance Ranges"))
                    .Add(new Card(
                        balanceSheetData.Length > 0
                            ? balanceSheetData.ToBarChart()
                                .Dimension("Type", e => e.Type)
                                .Measure("Balance", e => e.Sum(f => f.Balance))
                            : Text.Small("No data")
                    ).Title("Balance Sheet Overview")))
                .Add(Layout.Horizontal()
                    .Gap(3)
                    .Add(new Card(
                        accountCountByType.Length > 0
                            ? accountCountByType.ToBarChart()
                                .Dimension("Type", e => e.Type)
                                .Measure("Count", e => e.Sum(f => f.Count))
                            : Text.Small("No data")
                    ).Title("Account Count by Type"))
                    .Add(new Card(
                        extremeBalances.Length > 0
                            ? extremeBalances.ToBarChart()
                                .Dimension("Account", e => e.Account)
                                .Measure("Balance", e => e.Sum(f => f.Balance))
                            : Text.Small("No data")
                    ).Title("Extreme Balances")))
                .Add(new List(items));
        }
        else
        {
            content = new Card(
                Layout.Vertical()
                        .Gap(2)
                    .Padding(4)
                    .Add(Text.H4("No accounts found"))
                    .Add(Text.P("Create your chart of accounts to get started")));
        }

        return BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(2)
                .Add(searchQuery.ToTextInput().Placeholder("Search accounts..."))
                .Add(new Button(icon: Icons.Plus, variant: ButtonVariant.Outline)),
            content
        );
    }
}

// Account detail blade
public class AccountDetailBlade : ViewBase
{
    private readonly int _accountId;

    public AccountDetailBlade(int accountId)
    {
        _accountId = accountId;
    }

    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();

        var account = db.Accounts.FirstOrDefault(a => a.Id == _accountId);
        if (account == null)
        {
            return new Card("Account not found");
        }

        var transactions = db.Transactions
            .OrderByDescending(t => t.TransactionDate)
            .Take(20)
            .ToList();

        return Layout.Vertical()
            .Gap(3)
            .Add(new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Add(Layout.Horizontal().Gap(2).Add(Text.Small("Account Number:")).Add(Text.Block(account.AccountNumber)))
                    .Add(Layout.Horizontal().Gap(2).Add(Text.Small("Account Name:")).Add(Text.Block(account.AccountName)))
                    .Add(Layout.Horizontal().Gap(2).Add(Text.Small("Type:")).Add(Text.Block(account.AccountType)))
                    .Add(Layout.Horizontal().Gap(2).Add(Text.Small("Balance:")).Add(Text.H3($"${account.Balance:N2}")))
            ).Title("Account Details"))
            .Add(new Card(
                transactions.Count > 0
                    ? new List(transactions.Select(t => new ListItem(
                        title: t.Description,
                        subtitle: $"${t.DebitAmount:N2} / ${t.CreditAmount:N2} - {t.TransactionDate:MMM dd, yyyy}"
                    )))
                    : Layout.Vertical()
                        .Gap(2)
                        .Padding(4)
                        .Add(Text.H4("No transactions"))
                        .Add(Text.P("No transactions for this account yet"))
            ).Title("Recent Transactions"));
    }
}

// Transactions blade
public class TransactionsBlade : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
        var searchQuery = this.UseState("");

        var allTransactions = db.Transactions.ToList();
        var transactions = allTransactions
            .Where(t => searchQuery.Value == "" || t.Description.Contains(searchQuery.Value))
            .OrderByDescending(t => t.TransactionDate)
            .ToList();

        var items = transactions.Select(trans => new ListItem(
            title: trans.Description,
            subtitle: $"${trans.DebitAmount:N2} / ${trans.CreditAmount:N2} - {trans.TransactionDate:MMM dd, yyyy}"
        ));

        // Monthly debit vs credit
        var monthlyDebitCredit = allTransactions
            .Where(t => t.TransactionDate >= DateTime.UtcNow.AddMonths(-12))
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .Select(g => new { 
                Month = $"{g.Key.Year}-{g.Key.Month:D2}", 
                Debit = g.Sum(t => t.DebitAmount),
                Credit = g.Sum(t => t.CreditAmount)
            })
            .OrderBy(x => x.Month)
            .ToArray();

        // Transaction type distribution (debit vs credit)
        var transactionTypeData = new[]
        {
            new { Type = "Debits", Amount = allTransactions.Sum(t => t.DebitAmount) },
            new { Type = "Credits", Amount = allTransactions.Sum(t => t.CreditAmount) }
        };

        // Daily transaction volume (last 30 days)
        var dailyTransactions = allTransactions
            .Where(t => t.TransactionDate >= DateTime.UtcNow.AddDays(-30))
            .GroupBy(t => t.TransactionDate.Date)
            .Select(g => new { 
                Date = g.Key.ToString("MMM dd"), 
                Count = g.Count(),
                Amount = g.Sum(t => t.DebitAmount + t.CreditAmount)
            })
            .OrderBy(x => x.Date)
            .ToArray();

        // Top transaction amounts
        var topTransactions = allTransactions
            .Where(t => (t.DebitAmount + t.CreditAmount) > 0)
            .OrderByDescending(t => t.DebitAmount + t.CreditAmount)
            .Take(10)
            .Select(t => new { 
                Description = t.Description.Length > 30 ? t.Description.Substring(0, 30) + "..." : t.Description,
                Amount = t.DebitAmount + t.CreditAmount
            })
            .ToArray();

        // Transaction categories
        var transactionCategories = allTransactions
            .GroupBy(t => t.Description.Split(' ')[0]) // First word as category
            .Select(g => new { Category = g.Key, Count = g.Count(), Amount = g.Sum(t => t.DebitAmount + t.CreditAmount) })
            .OrderByDescending(x => x.Amount)
            .Take(10)
            .ToArray();

        // Net cash flow by month
        var monthlyCashFlow = allTransactions
            .Where(t => t.TransactionDate >= DateTime.UtcNow.AddMonths(-12))
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .Select(g => new { 
                Month = $"{g.Key.Year}-{g.Key.Month:D2}", 
                NetFlow = g.Sum(t => t.CreditAmount - t.DebitAmount)
            })
            .OrderBy(x => x.Month)
            .ToArray();

        // Transaction frequency by day of week
        var transactionByDayOfWeek = allTransactions
            .GroupBy(t => t.TransactionDate.DayOfWeek)
            .Select(g => new { 
                Day = g.Key.ToString(), 
                Count = g.Count(),
                Amount = g.Sum(t => t.DebitAmount + t.CreditAmount)
            })
            .OrderBy(x => x.Day)
            .ToArray();

        object content;
        
        if (transactions.Count > 0)
        {
            content = Layout.Vertical()
                .Gap(3)
                .Add(Layout.Horizontal()
                    .Gap(3)
                    .Add(new Card(
                        transactionTypeData.Length > 0
                            ? transactionTypeData.ToPieChart(
                                e => e.Type,
                                e => e.Sum(f => f.Amount),
                                PieChartStyles.Donut
                            )
                            : Text.Small("No data")
                    ).Title("Debit vs Credit Distribution"))
                    .Add(new Card(
                        monthlyDebitCredit.Length > 0
                            ? monthlyDebitCredit.ToLineChart(style: LineChartStyles.Dashboard)
                                .Dimension("Month", e => e.Month)
                                .Measure("Debit", e => e.Sum(f => f.Debit))
                                .Measure("Credit", e => e.Sum(f => f.Credit))
                            : Text.Small("No data")
                    ).Title("Monthly Debit vs Credit")))
                .Add(Layout.Horizontal()
                    .Gap(3)
                    .Add(new Card(
                        dailyTransactions.Length > 0
                            ? dailyTransactions.ToBarChart()
                                .Dimension("Date", e => e.Date)
                                .Measure("Count", e => e.Sum(f => f.Count))
                            : Text.Small("No data")
                    ).Title("Daily Transaction Count (30 days)"))
                    .Add(new Card(
                        topTransactions.Length > 0
                            ? topTransactions.ToBarChart()
                                .Dimension("Description", e => e.Description)
                                .Measure("Amount", e => e.Sum(f => f.Amount))
                            : Text.Small("No data")
                    ).Title("Top Transactions by Amount")))
                .Add(Layout.Horizontal()
                    .Gap(3)
                    .Add(new Card(
                        transactionCategories.Length > 0
                            ? transactionCategories.ToBarChart()
                                .Dimension("Category", e => e.Category)
                                .Measure("Amount", e => e.Sum(f => f.Amount))
                            : Text.Small("No data")
                    ).Title("Transaction Categories"))
                    .Add(new Card(
                        monthlyCashFlow.Length > 0
                            ? monthlyCashFlow.ToLineChart(style: LineChartStyles.Dashboard)
                                .Dimension("Month", e => e.Month)
                                .Measure("NetFlow", e => e.Sum(f => f.NetFlow))
                            : Text.Small("No data")
                    ).Title("Monthly Cash Flow")))
                .Add(new Card(
                    transactionByDayOfWeek.Length > 0
                        ? transactionByDayOfWeek.ToBarChart()
                            .Dimension("Day", e => e.Day)
                            .Measure("Count", e => e.Sum(f => f.Count))
                        : Text.Small("No data")
                ).Title("Transaction Frequency by Day of Week"))
                .Add(new List(items));
        }
        else
        {
            content = new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Padding(4)
                    .Add(Text.H4("No transactions found"))
                    .Add(Text.P("Your financial activity will appear here")));
        }

        return BladeHelper.WithHeader(
                Layout.Horizontal()
                    .Gap(2)
                .Add(searchQuery.ToTextInput().Placeholder("Search transactions..."))
                .Add(new Button(icon: Icons.Plus, variant: ButtonVariant.Outline)),
            content
        );
    }
}

// Reports blade
public class ReportsBlade : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();

        var invoices = db.Invoices.ToList();
        var transactions = db.Transactions.ToList();
        var accounts = db.Accounts.ToList();

        // Profit & Loss data
        var revenue = invoices.Where(i => i.Status == "Paid").Sum(i => i.TotalAmount);
        var expenses = transactions.Where(t => t.DebitAmount > 0).Sum(t => t.DebitAmount);
        var netProfit = revenue - expenses;

        // Balance Sheet data
        var totalAssets = accounts.Where(a => a.AccountType == "Asset").Sum(a => a.Balance);
        var totalLiabilities = accounts.Where(a => a.AccountType == "Liability").Sum(a => a.Balance);
        var totalEquity = accounts.Where(a => a.AccountType == "Equity").Sum(a => a.Balance);

        // Cash Flow data (simplified)
        var cashInflows = invoices.Where(i => i.Status == "Paid").Sum(i => i.TotalAmount);
        var cashOutflows = transactions.Where(t => t.DebitAmount > 0).Sum(t => t.DebitAmount);
        var netCashFlow = cashInflows - cashOutflows;

        // Aged receivables
        var agedReceivables = invoices
            .Where(i => i.Status != "Paid")
            .GroupBy(i => DateTime.UtcNow.Subtract(i.DueDate).Days switch
            {
                <= 0 => "Current",
                <= 30 => "1-30 days",
                <= 60 => "31-60 days",
                <= 90 => "61-90 days",
                _ => "Over 90 days"
            })
            .Select(g => new { Age = g.Key, Amount = g.Sum(i => i.TotalAmount) })
            .ToArray();

        // Monthly financial trends
        var monthlyFinancials = invoices
            .Where(i => i.IssueDate >= DateTime.UtcNow.AddMonths(-12))
            .GroupBy(i => new { i.IssueDate.Year, i.IssueDate.Month })
            .Select(g => new { 
                Month = $"{g.Key.Year}-{g.Key.Month:D2}", 
                Revenue = g.Where(i => i.Status == "Paid").Sum(i => i.TotalAmount),
                Invoiced = g.Sum(i => i.TotalAmount)
            })
            .OrderBy(x => x.Month)
            .ToArray();

        var monthlyExpenses = transactions
            .Where(t => t.DebitAmount > 0 && t.TransactionDate >= DateTime.UtcNow.AddMonths(-12))
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .Select(g => new { 
                Month = $"{g.Key.Year}-{g.Key.Month:D2}", 
                Expenses = g.Sum(t => t.DebitAmount)
            })
            .OrderBy(x => x.Month)
            .ToArray();

        // Profit margin by month
        var monthlyProfitMargins = monthlyFinancials
            .Join(monthlyExpenses, 
                f => f.Month, 
                e => e.Month, 
                (f, e) => new { 
                    Month = f.Month, 
                    Revenue = f.Revenue, 
                    Expenses = e.Expenses,
                    Profit = f.Revenue - e.Expenses,
                    Margin = f.Revenue > 0 ? ((f.Revenue - e.Expenses) / f.Revenue) * 100 : 0
                })
            .ToArray();

        // Top expense categories
        var expenseCategories = transactions
            .Where(t => t.DebitAmount > 0)
            .GroupBy(t => t.Description.Split(' ')[0])
            .Select(g => new { Category = g.Key, Amount = g.Sum(t => t.DebitAmount) })
            .OrderByDescending(x => x.Amount)
            .Take(8)
            .ToArray();

        // Revenue by customer
        var revenueByCustomer = invoices
            .Where(i => i.Status == "Paid")
            .GroupBy(i => i.CustomerName)
            .Select(g => new { Customer = g.Key, Revenue = g.Sum(i => i.TotalAmount) })
            .OrderByDescending(x => x.Revenue)
            .Take(8)
            .ToArray();

        return Layout.Vertical()
            .Gap(3)
            .Add(Layout.Grid()
                .Columns(3)
                .Gap(3)
                .Add(new Card(
                    Layout.Vertical()
                        .Gap(2)
                        .Add(Text.H3("Revenue"))
                        .Add(Text.H2($"${revenue:N0}"))
                        .Add(Text.Small("Total earned"))))
                .Add(new Card(
                    Layout.Vertical()
                        .Gap(2)
                        .Add(Text.H3("Expenses"))
                        .Add(Text.H2($"${expenses:N0}"))
                        .Add(Text.Small("Total spent"))))
                .Add(new Card(
                    Layout.Vertical()
                        .Gap(2)
                        .Add(Text.H3("Net Profit"))
                        .Add(Text.H2($"${netProfit:N0}"))
                        .Add(Text.Small(netProfit >= 0 ? "Profit" : "Loss")))))
            .Add(Layout.Horizontal()
                .Gap(3)
                .Add(new Card(
                    agedReceivables.Length > 0
                        ? agedReceivables.ToPieChart(
                            e => e.Age,
                            e => e.Sum(f => f.Amount),
                            PieChartStyles.Donut
                        )
                        : Text.Small("No aged receivables")
                ).Title("Aged Receivables"))
                .Add(new Card(
                    monthlyFinancials.Length > 0
                        ? monthlyFinancials.ToLineChart(style: LineChartStyles.Dashboard)
                            .Dimension("Month", e => e.Month)
                            .Measure("Revenue", e => e.Sum(f => f.Revenue))
                            .Measure("Invoiced", e => e.Sum(f => f.Invoiced))
                        : Text.Small("No financial data")
                ).Title("Monthly Revenue Trends")))
            .Add(Layout.Horizontal()
                .Gap(3)
                .Add(new Card(
                    accounts.Count > 0
                        ? new[]
                        {
                            new { Type = "Assets", Amount = totalAssets },
                            new { Type = "Liabilities", Amount = totalLiabilities },
                            new { Type = "Equity", Amount = totalEquity }
                        }.ToBarChart()
                            .Dimension("Type", e => e.Type)
                            .Measure("Amount", e => e.Sum(f => f.Amount))
                        : Text.Small("No account data")
                ).Title("Balance Sheet Overview"))
                .Add(new Card(
                    monthlyExpenses.Length > 0
                        ? monthlyExpenses.ToBarChart()
                            .Dimension("Month", e => e.Month)
                            .Measure("Expenses", e => e.Sum(f => f.Expenses))
                        : Text.Small("No expense data")
                ).Title("Monthly Expenses")))
            .Add(Layout.Horizontal()
                .Gap(3)
                .Add(new Card(
                    expenseCategories.Length > 0
                        ? expenseCategories.ToPieChart(
                            e => e.Category,
                            e => e.Sum(f => f.Amount),
                            PieChartStyles.Donut
                        )
                        : Text.Small("No expense data")
                ).Title("Expense Categories"))
                .Add(new Card(
                    revenueByCustomer.Length > 0
                        ? revenueByCustomer.ToBarChart()
                            .Dimension("Customer", e => e.Customer)
                            .Measure("Revenue", e => e.Sum(f => f.Revenue))
                        : Text.Small("No customer data")
                ).Title("Revenue by Customer")))
            .Add(new Card(
                monthlyProfitMargins.Length > 0
                    ? monthlyProfitMargins.ToLineChart(style: LineChartStyles.Dashboard)
                        .Dimension("Month", e => e.Month)
                        .Measure("Profit", e => e.Sum(f => f.Profit))
                        .Measure("Margin", e => e.Sum(f => f.Margin))
                    : Text.Small("No profit data")
            ).Title("Monthly Profit & Margins"))
            .Add(new Card(
                Layout.Vertical()
                    .Gap(2)
                    .Add(Text.H4("Financial Reports"))
                    .Add(new List(new[]
                    {
                        new ListItem(
                            icon: Icons.FileText,
                            title: "Profit & Loss Statement",
                            subtitle: $"Revenue: ${revenue:N0} | Expenses: ${expenses:N0} | Net: ${netProfit:N0}"
                        ),
                        new ListItem(
                            icon: Icons.Activity,
                            title: "Balance Sheet",
                            subtitle: $"Assets: ${totalAssets:N0} | Liabilities: ${totalLiabilities:N0} | Equity: ${totalEquity:N0}"
                        ),
                        new ListItem(
                            icon: Icons.TrendingUp,
                            title: "Cash Flow Statement",
                            subtitle: $"Inflows: ${cashInflows:N0} | Outflows: ${cashOutflows:N0} | Net: ${netCashFlow:N0}"
                        ),
                        new ListItem(
                            icon: Icons.Circle,
                            title: "Aged Receivables",
                            subtitle: $"Total Outstanding: ${agedReceivables.Sum(a => a.Amount):N0}"
                        )
                    }))
            ).Title("Quick Reports"));
    }
}