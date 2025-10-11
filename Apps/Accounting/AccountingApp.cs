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
        return this.UseBlades(() => new AccountingMenuBlade(), "Accounting");
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

        var invoices = db.Invoices
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

        return BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(2)
                .Add(searchQuery.ToTextInput().Placeholder("Search invoices..."))
                .Add(new Button(icon: Icons.Plus, onClick: _ =>
                {
                    // Show create invoice sheet
                }, variant: ButtonVariant.Outline)),
            invoices.Count > 0 
                ? new List(items)
                : new Card(
                    Layout.Vertical()
                        .Gap(2)
                        .Padding(4)
                        .Add(Text.H4("No invoices found"))
                        .Add(Text.P("Create your first invoice to get started"))
                        .Add(new Button("Create Invoice").Icon(Icons.Plus).Variant(ButtonVariant.Primary)))
        );
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
        
        var invoice = db.Invoices.FirstOrDefault(i => i.Id == _invoiceId);
        if (invoice == null)
        {
            return new Card("Invoice not found");
        }

        return Layout.Vertical()
            .Gap(3)
            .Add(Layout.Horizontal()
                .Gap(2)
                .Add(new Button("Edit").Icon(Icons.Pencil).Variant(ButtonVariant.Outline).WithSheet(
                    () => new EditInvoiceSheet(invoice),
                    title: "Edit Invoice",
                    description: $"Editing invoice #{invoice.InvoiceNumber}",
                    width: Size.Fraction(1/2f)
                ))
                .Add(new Button("Delete").Icon(Icons.Trash).Variant(ButtonVariant.Destructive)))
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
        var customerName = this.UseState(_invoice.CustomerName);
        var amount = this.UseState(_invoice.Amount);
        var tax = this.UseState(_invoice.TaxAmount);
        var status = this.UseState(_invoice.Status);
        var client = this.UseService<IClientProvider>();

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
                    .Add(Text.Small("Status"))
                    .Add(new SelectInput<string>(
                        options: new[] { "Draft", "Sent", "Paid", "Overdue" }.ToOptions(),
                        value: status.Value,
                        onChange: e => { status.Value = e.Value; return ValueTask.CompletedTask; }
                    ))
            ).Title("Invoice Information"))
            .Add(Layout.Horizontal()
                .Gap(2)
                .Add(new Button("Save").Variant(ButtonVariant.Primary).HandleClick(_ =>
                {
                    client.Toast("Invoice updated successfully!");
                }))
                .Add(new Button("Cancel").Variant(ButtonVariant.Outline)));
    }
}

// Payments blade
public class PaymentsBlade : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
        var searchQuery = this.UseState("");

        var payments = db.Payments
            .Where(p => searchQuery.Value == "" || 
                       p.Reference.Contains(searchQuery.Value))
            .OrderByDescending(p => p.PaymentDate)
            .ToList();

        var items = payments.Select(payment => new ListItem(
            title: $"{payment.Reference} - {payment.PaymentMethod}",
            subtitle: $"${payment.Amount:N2} - {payment.PaymentDate:MMM dd, yyyy}"
        ));

        return BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(2)
                .Add(searchQuery.ToTextInput().Placeholder("Search payments..."))
                .Add(new Button(icon: Icons.Plus, variant: ButtonVariant.Outline)),
            payments.Count > 0 
                ? new List(items)
                : new Card(
                    Layout.Vertical()
                        .Gap(2)
                        .Padding(4)
                        .Add(Text.H4("No payments found"))
                        .Add(Text.P("Record your first payment to get started")))
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

        var accounts = db.Accounts
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

        return BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(2)
                .Add(searchQuery.ToTextInput().Placeholder("Search accounts..."))
                .Add(new Button(icon: Icons.Plus, variant: ButtonVariant.Outline)),
            accounts.Count > 0 
                ? new List(items)
                : new Card(
                    Layout.Vertical()
                        .Gap(2)
                        .Padding(4)
                        .Add(Text.H4("No accounts found"))
                        .Add(Text.P("Create your chart of accounts to get started")))
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

        var transactions = db.Transactions
            .Where(t => searchQuery.Value == "" || t.Description.Contains(searchQuery.Value))
            .OrderByDescending(t => t.TransactionDate)
            .ToList();

        var items = transactions.Select(trans => new ListItem(
            title: trans.Description,
            subtitle: $"${trans.DebitAmount:N2} / ${trans.CreditAmount:N2} - {trans.TransactionDate:MMM dd, yyyy}"
        ));

        return BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(2)
                .Add(searchQuery.ToTextInput().Placeholder("Search transactions..."))
                .Add(new Button(icon: Icons.Plus, variant: ButtonVariant.Outline)),
            transactions.Count > 0 
                ? new List(items)
                : new Card(
                    Layout.Vertical()
                        .Gap(2)
                        .Padding(4)
                        .Add(Text.H4("No transactions found"))
                        .Add(Text.P("Your financial activity will appear here")))
        );
    }
}

// Reports blade
public class ReportsBlade : ViewBase
{
    public override object? Build()
    {
        var reportTypes = new[]
        {
            new ListItem(
                icon: Icons.FileText,
                title: "Profit & Loss",
                subtitle: "Income statement for the period"
            ),
            new ListItem(
                icon: Icons.Activity,
                title: "Balance Sheet",
                subtitle: "Assets, liabilities, and equity"
            ),
            new ListItem(
                icon: Icons.TrendingUp,
                title: "Cash Flow",
                subtitle: "Cash movement analysis"
            ),
            new ListItem(
                icon: Icons.Circle,
                title: "Aged Receivables",
                subtitle: "Outstanding customer invoices"
            ),
            new ListItem(
                icon: Icons.Activity,
                title: "Aged Payables",
                subtitle: "Outstanding vendor bills"
            )
        };

        return new List(reportTypes);
    }
}