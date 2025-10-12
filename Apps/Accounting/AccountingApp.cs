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
    private readonly RefreshToken _refreshToken;

    public CreateInvoiceSheet(RefreshToken refreshToken)
    {
        _refreshToken = refreshToken;
    }

    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
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
        var db = this.UseService<ApplicationDbContext>();
        var client = this.UseService<IClientProvider>();
        var blades = this.UseContext<IBladeController>();
        var searchQuery = this.UseState("");
        var isNewAccountOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        var query = context.Accounts.AsQueryable();
        
        if (!string.IsNullOrEmpty(searchQuery.Value))
        {
            var searchPattern = $"%{searchQuery.Value}%";
            query = query.Where(a => 
                EF.Functions.Like(a.AccountNumber, searchPattern) ||
                EF.Functions.Like(a.AccountName, searchPattern) ||
                EF.Functions.Like(a.AccountType, searchPattern) ||
                EF.Functions.Like(a.Description, searchPattern));
        }
        
        var accounts = query.OrderBy(a => a.AccountNumber).ToList();
        
        var listItems = accounts.Select(account => new ListItem(
            title: $"{account.AccountNumber} - {account.AccountName}",
            subtitle: $"{account.AccountType} - Balance: ${account.Balance:N2}",
            icon: Icons.Book,
            badge: account.IsActive ? "Active" : "Inactive",
            onClick: _ => blades.Push(this, new AccountDetailBlade(account.Id, () => refreshToken.Refresh()), account.AccountName)
        ));
        
        var mainContent = BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(4)
                .Add(searchQuery.ToSearchInput().Placeholder("Search by account number, name, type, or description..."))
                .Add(new Button("Add Account")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => isNewAccountOpen.Set(true))),
            accounts.Count == 0 
                ? Text.Block("No accounts found. Try a different search or add your first account!")
                : new List(listItems)
        );

        return isNewAccountOpen.Value ? new Sheet(
            (Event<Sheet> _) => isNewAccountOpen.Set(false),
            new AccountFormSheet(null, () => {
                isNewAccountOpen.Set(false);
                refreshToken.Refresh();
            }),
            title: "New Account",
            description: "Add a new account to the chart of accounts"
        ).Width(Size.Fraction(1/3f)) : mainContent;
    }
}

public class AccountDetailBlade(int accountId, Action? onRefresh = null) : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var initialAccount = context.Accounts.FirstOrDefault(a => a.Id == accountId);
        
        if (initialAccount == null)
        {
            return Layout.Vertical()
                .Gap(4)
                .Add(Text.H3("Account Not Found"))
                .Add(new Button("Go Back", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary));
        }
        
        var accountData = this.UseState(initialAccount);
        var isEditOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        // Refresh account data when refresh token changes
        this.UseEffect(() =>
        {
            var updatedAccount = context.Accounts.FirstOrDefault(a => a.Id == accountId);
            if (updatedAccount != null)
            {
                accountData.Set(updatedAccount);
            }
        }, [refreshToken.ToTrigger()]);
        
        var statusBadge = new Badge(accountData.Value.IsActive ? "Active" : "Inactive")
            .Variant(accountData.Value.IsActive ? BadgeVariant.Success : BadgeVariant.Secondary);

        var accountDetails = new
        {
            AccountNumber = accountData.Value.AccountNumber,
            AccountName = accountData.Value.AccountName,
            AccountType = accountData.Value.AccountType,
            Balance = $"${accountData.Value.Balance:N2}",
            Description = accountData.Value.Description,
            Status = statusBadge
        };
        
        return Layout.Vertical()
            .Gap(4)
            .Add(Text.H3($"{accountData.Value.AccountNumber} - {accountData.Value.AccountName}"))
            .Add(accountDetails.ToDetails().RemoveEmpty().MultiLine(x => x.Description))
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(new Button(accountData.Value.IsActive ? "Deactivate" : "Activate", _ => {
                    var dbAccount = context.Accounts.FirstOrDefault(a => a.Id == accountId);
                    if (dbAccount != null)
                    {
                        dbAccount.IsActive = !dbAccount.IsActive;
                        dbAccount.UpdatedAt = DateTime.UtcNow;
                        context.SaveChanges();
                        client.Toast($"Account {accountData.Value.AccountNumber} {(dbAccount.IsActive ? "activated" : "deactivated")}!");
                        refreshToken.Refresh();
                        onRefresh?.Invoke();
                    }
                })
                    .Variant(accountData.Value.IsActive ? ButtonVariant.Destructive : ButtonVariant.Success)
                    .Icon(accountData.Value.IsActive ? Icons.X : Icons.Check))
                .Add(new Button("Edit Account")
                    .Variant(ButtonVariant.Outline)
                    .Icon(Icons.Pencil)
                    .HandleClick(_ => isEditOpen.Set(true)))
                .Add(new Button("Delete Account")
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.Trash)
                    .HandleClick(_ => {
                        try
                        {
                            var accountToDelete = context.Accounts.FirstOrDefault(a => a.Id == accountId);
                            if (accountToDelete != null)
                            {
                                context.Accounts.Remove(accountToDelete);
                                context.SaveChanges();
                                client.Toast($"Account {accountData.Value.AccountNumber} deleted successfully!");
                                refreshToken.Refresh();
                                onRefresh?.Invoke();
                                blades.Pop();
                            }
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error deleting account: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary)))
            .Add(isEditOpen.Value ? new Sheet(
                (Event<Sheet> _) => isEditOpen.Set(false),
                new AccountFormSheet(accountId, () => {
                    isEditOpen.Set(false);
                    refreshToken.Refresh();
                    onRefresh?.Invoke();
                }),
                title: "Edit Account",
                description: $"Edit account {accountData.Value.AccountNumber}"
            ).Width(Size.Fraction(1/3f)) : null);
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
        var isNewTransactionOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        var query = context.Transactions.AsQueryable();
        
        if (!string.IsNullOrEmpty(searchQuery.Value))
        {
            var searchPattern = $"%{searchQuery.Value}%";
            query = query.Where(t => 
                EF.Functions.Like(t.TransactionNumber, searchPattern) ||
                EF.Functions.Like(t.Description, searchPattern) ||
                EF.Functions.Like(t.Reference, searchPattern));
        }
        
        var transactions = query.OrderByDescending(t => t.TransactionDate).ToList();
        
        var listItems = transactions.Select(transaction => new ListItem(
            title: $"#{transaction.TransactionNumber}",
            subtitle: $"{transaction.Description} - {transaction.TransactionDate:MMM dd, yyyy}",
            icon: Icons.ArrowLeftRight,
            badge: transaction.DebitAmount > 0 ? $"${transaction.DebitAmount:N2} DR" : $"${transaction.CreditAmount:N2} CR",
            onClick: _ => blades.Push(this, new TransactionDetailBlade(transaction.Id, () => refreshToken.Refresh()), transaction.TransactionNumber)
        ));
        
        var mainContent = BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(4)
                .Add(searchQuery.ToSearchInput().Placeholder("Search by transaction number, description, or reference..."))
                .Add(new Button("Add Transaction")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => isNewTransactionOpen.Set(true))),
            transactions.Count == 0 
                ? Text.Block("No transactions found. Try a different search or add your first transaction!")
                : new List(listItems)
        );

        return isNewTransactionOpen.Value ? new Sheet(
            (Event<Sheet> _) => isNewTransactionOpen.Set(false),
            new TransactionFormSheet(null, () => {
                isNewTransactionOpen.Set(false);
                refreshToken.Refresh();
            }),
            title: "New Transaction",
            description: "Add a new accounting transaction"
        ).Width(Size.Fraction(1/3f)) : mainContent;
    }
}

public class TransactionDetailBlade(int transactionId, Action? onRefresh = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var initialTransaction = context.Transactions.FirstOrDefault(t => t.Id == transactionId);
        
        if (initialTransaction == null)
        {
            return Layout.Vertical()
                .Gap(4)
                .Add(Text.H3("Transaction Not Found"))
                .Add(new Button("Go Back", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary));
        }
        
        var transactionData = this.UseState(initialTransaction);
        var isEditOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        // Refresh transaction data when refresh token changes
        this.UseEffect(() =>
        {
            var updatedTransaction = context.Transactions.FirstOrDefault(t => t.Id == transactionId);
            if (updatedTransaction != null)
            {
                transactionData.Set(updatedTransaction);
            }
        }, [refreshToken.ToTrigger()]);
        
        var transactionDetails = new
        {
            TransactionNumber = $"#{transactionData.Value.TransactionNumber}",
            Date = transactionData.Value.TransactionDate.ToString("MMM dd, yyyy"),
            Description = transactionData.Value.Description,
            DebitAmount = $"${transactionData.Value.DebitAmount:N2}",
            CreditAmount = $"${transactionData.Value.CreditAmount:N2}",
            Reference = transactionData.Value.Reference
        };
        
        return Layout.Vertical()
            .Gap(4)
            .Add(Text.H3($"Transaction {transactionData.Value.TransactionNumber}"))
            .Add(transactionDetails.ToDetails().RemoveEmpty().MultiLine(x => x.Description))
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(new Button("Edit Transaction")
                    .Variant(ButtonVariant.Outline)
                    .Icon(Icons.Pencil)
                    .HandleClick(_ => isEditOpen.Set(true)))
                .Add(new Button("Delete Transaction")
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.Trash)
                    .HandleClick(_ => {
                        try
                        {
                            var transactionToDelete = context.Transactions.FirstOrDefault(t => t.Id == transactionId);
                            if (transactionToDelete != null)
                            {
                                context.Transactions.Remove(transactionToDelete);
                                context.SaveChanges();
                                client.Toast($"Transaction {transactionData.Value.TransactionNumber} deleted successfully!");
                                refreshToken.Refresh();
                                onRefresh?.Invoke();
                                blades.Pop();
                            }
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error deleting transaction: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary)))
            .Add(isEditOpen.Value ? new Sheet(
                (Event<Sheet> _) => isEditOpen.Set(false),
                new TransactionFormSheet(transactionId, () => {
                    isEditOpen.Set(false);
                    refreshToken.Refresh();
                    onRefresh?.Invoke();
                }),
                title: "Edit Transaction",
                description: $"Edit transaction {transactionData.Value.TransactionNumber}"
            ).Width(Size.Fraction(1/3f)) : null);
    }
}

public class InvoiceFormSheet(int? invoiceId = null, Action? onClose = null) : ViewBase
{
    private Invoice CloneInvoice(Invoice source) => new Invoice
    {
        Id = source.Id,
        InvoiceNumber = source.InvoiceNumber,
        CustomerName = source.CustomerName,
        CustomerEmail = source.CustomerEmail,
        Amount = source.Amount,
        TaxAmount = source.TaxAmount,
        TotalAmount = source.TotalAmount,
        Status = source.Status,
        IssueDate = source.IssueDate,
        DueDate = source.DueDate,
        Description = source.Description,
        CreatedAt = source.CreatedAt,
        UpdatedAt = source.UpdatedAt
    };
    
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
                            if (!isEdit) {
                                var cloned = CloneInvoice(invoiceForm.Value);
                                cloned.InvoiceNumber = e.Value;
                                invoiceForm.Set(cloned);
                            }
                        }).Placeholder("INV-001").Disabled(isEdit))
                        .Add(Text.Small("Status"))
                        .Add(new SelectInput<string>(invoiceForm.Value.Status, e => {
                            var cloned = CloneInvoice(invoiceForm.Value);
                            cloned.Status = e.Value;
                            invoiceForm.Set(cloned);
                        }, statusOptions.ToOptions()))
                        .Add(Text.Small("Issue Date"))
                        .Add(new DateTimeInput<DateTime>(invoiceForm.Value.IssueDate, e => {
                            var cloned = CloneInvoice(invoiceForm.Value);
                            cloned.IssueDate = e.Value;
                            invoiceForm.Set(cloned);
                        }))
                        .Add(Text.Small("Due Date"))
                        .Add(new DateTimeInput<DateTime>(invoiceForm.Value.DueDate, e => {
                            var cloned = CloneInvoice(invoiceForm.Value);
                            cloned.DueDate = e.Value;
                            invoiceForm.Set(cloned);
                        }))
                ).Title("Invoice Details"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Customer Information"))
                        .Add(Text.Small("Customer Name"))
                        .Add(new TextInput(invoiceForm.Value.CustomerName, e => {
                            var cloned = CloneInvoice(invoiceForm.Value);
                            cloned.CustomerName = e.Value;
                            invoiceForm.Set(cloned);
                        }).Placeholder("Customer Name"))
                        .Add(Text.Small("Customer Email"))
                        .Add(new TextInput(invoiceForm.Value.CustomerEmail, e => {
                            var cloned = CloneInvoice(invoiceForm.Value);
                            cloned.CustomerEmail = e.Value;
                            invoiceForm.Set(cloned);
                        }).Placeholder("customer@example.com"))
                ).Title("Customer"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Amount Information"))
                        .Add(Text.Small("Amount ($)"))
                        .Add(new NumberInput<decimal>(invoiceForm.Value.Amount, v => {
                            var cloned = CloneInvoice(invoiceForm.Value);
                            cloned.Amount = v;
                            cloned.TotalAmount = v + cloned.TaxAmount;
                            invoiceForm.Set(cloned);
                        }).Placeholder("0.00"))
                        .Add(Text.Small("Tax Amount ($)"))
                        .Add(new NumberInput<decimal>(invoiceForm.Value.TaxAmount, v => {
                            var cloned = CloneInvoice(invoiceForm.Value);
                            cloned.TaxAmount = v;
                            cloned.TotalAmount = cloned.Amount + v;
                            invoiceForm.Set(cloned);
                        }).Placeholder("0.00"))
                        .Add(Text.Small("Total Amount ($)"))
                        .Add(Text.H3($"${(invoiceForm.Value.Amount + invoiceForm.Value.TaxAmount):N2}"))
                ).Title("Amounts"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Additional Information"))
                        .Add(Text.Small("Description"))
                        .Add(new TextInput(invoiceForm.Value.Description, e => {
                            var cloned = CloneInvoice(invoiceForm.Value);
                            cloned.Description = e.Value;
                            invoiceForm.Set(cloned);
                        }).Placeholder("Invoice description...").Variant(TextInputs.Textarea))
                ).Title("Description"))
        );
    }
}

public class AccountFormSheet(int? accountId = null, Action? onClose = null) : ViewBase
{
    private Account CloneAccount(Account source) => new Account
    {
        Id = source.Id,
        AccountNumber = source.AccountNumber,
        AccountName = source.AccountName,
        AccountType = source.AccountType,
        Balance = source.Balance,
        IsActive = source.IsActive,
        Description = source.Description,
        CreatedAt = source.CreatedAt,
        UpdatedAt = source.UpdatedAt
    };
    
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var isEdit = accountId.HasValue;
        var existingAccount = isEdit ? context.Accounts.FirstOrDefault(a => a.Id == accountId!.Value) : null;
        
        var accountForm = this.UseState(existingAccount ?? new Account
        {
            AccountNumber = "",
            AccountName = "",
            AccountType = "Asset",
            Balance = 0.00m,
            Description = "",
            IsActive = true
        });
        
        var accountTypeOptions = new[] { "Asset", "Liability", "Equity", "Revenue", "Expense" };
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => {
                        try
                        {
                            // Client-side validation
                            if (string.IsNullOrWhiteSpace(accountForm.Value.AccountNumber))
                            {
                                client.Toast("Account Number is required", "Validation Error");
                                return;
                            }
                            
                            if (string.IsNullOrWhiteSpace(accountForm.Value.AccountName))
                            {
                                client.Toast("Account Name is required", "Validation Error");
                                return;
                            }
                            
                            if (string.IsNullOrWhiteSpace(accountForm.Value.AccountType))
                            {
                                client.Toast("Account Type is required", "Validation Error");
                                return;
                            }
                            
                            if (accountForm.Value.Balance < 0)
                            {
                                client.Toast("Balance cannot be negative", "Validation Error");
                                return;
                            }
                            
                            if (accountForm.Value.AccountNumber.Length > 20)
                            {
                                client.Toast("Account Number cannot exceed 20 characters", "Validation Error");
                                return;
                            }
                            
                            if (accountForm.Value.AccountName.Length > 200)
                            {
                                client.Toast("Account Name cannot exceed 200 characters", "Validation Error");
                                return;
                            }
                            
                            if (isEdit && existingAccount != null)
                            {
                                // Update existing account
                                existingAccount.AccountNumber = accountForm.Value.AccountNumber;
                                existingAccount.AccountName = accountForm.Value.AccountName;
                                existingAccount.AccountType = accountForm.Value.AccountType;
                                existingAccount.Balance = accountForm.Value.Balance;
                                existingAccount.Description = accountForm.Value.Description;
                                existingAccount.IsActive = accountForm.Value.IsActive;
                                existingAccount.UpdatedAt = DateTime.UtcNow;
                            }
                            else
                            {
                                // Create new account
                                var newAccount = new Account
                                {
                                    AccountNumber = accountForm.Value.AccountNumber,
                                    AccountName = accountForm.Value.AccountName,
                                    AccountType = accountForm.Value.AccountType,
                                    Balance = accountForm.Value.Balance,
                                    Description = accountForm.Value.Description,
                                    IsActive = accountForm.Value.IsActive,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                context.Accounts.Add(newAccount);
                            }
                            
                            context.SaveChanges();
                            client.Toast(isEdit ? "Account updated successfully!" : "Account created successfully!");
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
                        .Add(Text.Small("Account Information"))
                        .Add(Text.Small("Account Number"))
                        .Add(new TextInput(accountForm.Value.AccountNumber, e => {
                            var cloned = CloneAccount(accountForm.Value);
                            cloned.AccountNumber = e.Value;
                            accountForm.Set(cloned);
                        }).Placeholder("1000"))
                        .Add(Text.Small("Account Name"))
                        .Add(new TextInput(accountForm.Value.AccountName, e => {
                            var cloned = CloneAccount(accountForm.Value);
                            cloned.AccountName = e.Value;
                            accountForm.Set(cloned);
                        }).Placeholder("Cash"))
                        .Add(Text.Small("Account Type"))
                        .Add(new SelectInput<string>(accountForm.Value.AccountType, e => {
                            var cloned = CloneAccount(accountForm.Value);
                            cloned.AccountType = e.Value;
                            accountForm.Set(cloned);
                        }, accountTypeOptions.ToOptions()))
                        .Add(Text.Small("Balance ($)"))
                        .Add(new NumberInput<decimal>(accountForm.Value.Balance, v => {
                            var cloned = CloneAccount(accountForm.Value);
                            cloned.Balance = v;
                            accountForm.Set(cloned);
                        }).Placeholder("0.00"))
                        .Add(Text.Small("Status"))
                        .Add(new SelectInput<bool>(accountForm.Value.IsActive, e => {
                            var cloned = CloneAccount(accountForm.Value);
                            cloned.IsActive = e.Value;
                            accountForm.Set(cloned);
                        }, new[] { (true, "Active"), (false, "Inactive") }.ToOptions()))
                ).Title("Account Details"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Additional Information"))
                        .Add(Text.Small("Description"))
                        .Add(new TextInput(accountForm.Value.Description, e => {
                            var cloned = CloneAccount(accountForm.Value);
                            cloned.Description = e.Value;
                            accountForm.Set(cloned);
                        }).Placeholder("Account description...").Variant(TextInputs.Textarea))
                ).Title("Description"))
        );
    }
}

public class TransactionFormSheet(int? transactionId = null, Action? onClose = null) : ViewBase
{
    private Transaction CloneTransaction(Transaction source) => new Transaction
    {
        Id = source.Id,
        TransactionNumber = source.TransactionNumber,
        TransactionDate = source.TransactionDate,
        Description = source.Description,
        DebitAmount = source.DebitAmount,
        CreditAmount = source.CreditAmount,
        Reference = source.Reference,
        CreatedAt = source.CreatedAt,
        UpdatedAt = source.UpdatedAt
    };
    
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var isEdit = transactionId.HasValue;
        var existingTransaction = isEdit ? context.Transactions.FirstOrDefault(t => t.Id == transactionId!.Value) : null;
        
        var transactionForm = this.UseState(existingTransaction ?? new Transaction
        {
            TransactionNumber = $"TXN-{DateTime.Now:yyyyMMdd-HHmmss}",
            TransactionDate = DateTime.UtcNow,
            Description = "",
            DebitAmount = 0.00m,
            CreditAmount = 0.00m,
            Reference = ""
        });
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => {
                        try
                        {
                            // Client-side validation
                            if (string.IsNullOrWhiteSpace(transactionForm.Value.TransactionNumber))
                            {
                                client.Toast("Transaction Number is required", "Validation Error");
                                return;
                            }
                            
                            if (string.IsNullOrWhiteSpace(transactionForm.Value.Description))
                            {
                                client.Toast("Description is required", "Validation Error");
                                return;
                            }
                            
                            if (transactionForm.Value.DebitAmount < 0)
                            {
                                client.Toast("Debit amount cannot be negative", "Validation Error");
                                return;
                            }
                            
                            if (transactionForm.Value.CreditAmount < 0)
                            {
                                client.Toast("Credit amount cannot be negative", "Validation Error");
                                return;
                            }
                            
                            if (transactionForm.Value.DebitAmount == 0 && transactionForm.Value.CreditAmount == 0)
                            {
                                client.Toast("Either debit or credit amount must be greater than zero", "Validation Error");
                                return;
                            }
                            
                            if (transactionForm.Value.DebitAmount > 0 && transactionForm.Value.CreditAmount > 0)
                            {
                                client.Toast("Cannot have both debit and credit amounts", "Validation Error");
                                return;
                            }
                            
                            if (transactionForm.Value.TransactionNumber.Length > 50)
                            {
                                client.Toast("Transaction Number cannot exceed 50 characters", "Validation Error");
                                return;
                            }
                            
                            if (transactionForm.Value.Description.Length > 500)
                            {
                                client.Toast("Description cannot exceed 500 characters", "Validation Error");
                                return;
                            }
                            
                            if (isEdit && existingTransaction != null)
                            {
                                // Update existing transaction
                                existingTransaction.TransactionNumber = transactionForm.Value.TransactionNumber;
                                existingTransaction.TransactionDate = transactionForm.Value.TransactionDate;
                                existingTransaction.Description = transactionForm.Value.Description;
                                existingTransaction.DebitAmount = transactionForm.Value.DebitAmount;
                                existingTransaction.CreditAmount = transactionForm.Value.CreditAmount;
                                existingTransaction.Reference = transactionForm.Value.Reference;
                                existingTransaction.UpdatedAt = DateTime.UtcNow;
                            }
                            else
                            {
                                // Create new transaction
                                var newTransaction = new Transaction
                                {
                                    TransactionNumber = transactionForm.Value.TransactionNumber,
                                    TransactionDate = transactionForm.Value.TransactionDate,
                                    Description = transactionForm.Value.Description,
                                    DebitAmount = transactionForm.Value.DebitAmount,
                                    CreditAmount = transactionForm.Value.CreditAmount,
                                    Reference = transactionForm.Value.Reference,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                context.Transactions.Add(newTransaction);
                            }
                            
                            context.SaveChanges();
                            client.Toast(isEdit ? "Transaction updated successfully!" : "Transaction created successfully!");
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
                        .Add(Text.Small("Transaction Information"))
                        .Add(Text.Small("Transaction Number"))
                        .Add(new TextInput(transactionForm.Value.TransactionNumber, e => {
                            var cloned = CloneTransaction(transactionForm.Value);
                            cloned.TransactionNumber = e.Value;
                            transactionForm.Set(cloned);
                        }).Placeholder("TXN-001").Disabled(isEdit))
                        .Add(Text.Small("Transaction Date"))
                        .Add(new DateTimeInput<DateTime>(transactionForm.Value.TransactionDate, e => {
                            var cloned = CloneTransaction(transactionForm.Value);
                            cloned.TransactionDate = e.Value;
                            transactionForm.Set(cloned);
                        }))
                        .Add(Text.Small("Description"))
                        .Add(new TextInput(transactionForm.Value.Description, e => {
                            var cloned = CloneTransaction(transactionForm.Value);
                            cloned.Description = e.Value;
                            transactionForm.Set(cloned);
                        }).Placeholder("Transaction description...").Variant(TextInputs.Textarea))
                        .Add(Text.Small("Reference"))
                        .Add(new TextInput(transactionForm.Value.Reference, e => {
                            var cloned = CloneTransaction(transactionForm.Value);
                            cloned.Reference = e.Value;
                            transactionForm.Set(cloned);
                        }).Placeholder("Reference number or code"))
                ).Title("Transaction Details"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Amount Information"))
                        .Add(Text.Small("Debit Amount ($)"))
                        .Add(new NumberInput<decimal>(transactionForm.Value.DebitAmount, v => {
                            var cloned = CloneTransaction(transactionForm.Value);
                            cloned.DebitAmount = v;
                            if (v > 0)
                            {
                                cloned.CreditAmount = 0;
                            }
                            transactionForm.Set(cloned);
                        }).Placeholder("0.00"))
                        .Add(Text.Small("Credit Amount ($)"))
                        .Add(new NumberInput<decimal>(transactionForm.Value.CreditAmount, v => {
                            var cloned = CloneTransaction(transactionForm.Value);
                            cloned.CreditAmount = v;
                            if (v > 0)
                            {
                                cloned.DebitAmount = 0;
                            }
                            transactionForm.Set(cloned);
                        }).Placeholder("0.00"))
                        .Add(Text.Small("Transaction Type"))
                        .Add(Text.Block(transactionForm.Value.DebitAmount > 0 ? "Debit Transaction" : 
                                      transactionForm.Value.CreditAmount > 0 ? "Credit Transaction" : 
                                      "No amount entered"))
                ).Title("Amounts"))
        );
    }
}
