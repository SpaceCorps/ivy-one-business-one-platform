using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Accounting;

[App(icon: Icons.Percent, title: "Accounting", path: new[] { "Business Operations" })]
public class AccountingApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var selectedView = this.UseState("dashboard");
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Accounting System")
                .Add("Manage your financial records, invoices, and reports")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Dashboard", _ => selectedView.Set("dashboard"))
                        .Variant(selectedView.Value == "dashboard" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("Invoices", _ => selectedView.Set("invoices"))
                        .Variant(selectedView.Value == "invoices" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("Accounts", _ => selectedView.Set("accounts"))
                        .Variant(selectedView.Value == "accounts" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("Transactions", _ => selectedView.Set("transactions"))
                        .Variant(selectedView.Value == "transactions" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                )
                .Add(BuildSelectedView(selectedView.Value, context, client))
        );
    }

    private object BuildSelectedView(string view, ApplicationDbContext context, IClientProvider client)
    {
        return view switch
        {
            "dashboard" => BuildDashboard(context, client),
            "invoices" => BuildInvoicesView(context, client),
            "accounts" => BuildAccountsView(context, client),
            "transactions" => BuildTransactionsView(context, client),
            _ => "Select a view"
        };
    }

    private object BuildDashboard(ApplicationDbContext context, IClientProvider client)
    {
        var invoices = context.Invoices.ToList();
        var accounts = context.Accounts.ToList();
        var transactions = context.Transactions.ToList();

        var totalRevenue = invoices.Where(i => i.Status == "Paid").Sum(i => i.TotalAmount);
        var pendingInvoices = invoices.Where(i => i.Status != "Paid").Sum(i => i.TotalAmount);
        var totalAccounts = accounts.Count;
        var totalTransactions = transactions.Count;

        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add("Financial Dashboard")
                .Add(Layout.Grid()
                    .Columns(4)
                    .Gap(16)
                    .Add(new Card(
                        Layout.Vertical()
                            .Gap(8)
                            .Padding(16)
                            .Add("Total Revenue")
                            .Add($"${totalRevenue:N2}")
                            .Add("Paid invoices")
                    ))
                    .Add(new Card(
                        Layout.Vertical()
                            .Gap(8)
                            .Padding(16)
                            .Add("Pending Invoices")
                            .Add($"${pendingInvoices:N2}")
                            .Add("Unpaid amount")
                    ))
                    .Add(new Card(
                        Layout.Vertical()
                            .Gap(8)
                            .Padding(16)
                            .Add("Chart of Accounts")
                            .Add(totalAccounts.ToString())
                            .Add("Total accounts")
                    ))
                    .Add(new Card(
                        Layout.Vertical()
                            .Gap(8)
                            .Padding(16)
                            .Add("Transactions")
                            .Add(totalTransactions.ToString())
                            .Add("Total transactions")
                    ))
                )
        );
    }

    private object BuildInvoicesView(ApplicationDbContext context, IClientProvider client)
    {
        var invoices = context.Invoices.OrderByDescending(i => i.IssueDate).ToList();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add("Invoices")
                    .Add(new Button("Create Invoice", _ => client.Toast("Invoice creation coming soon!"))
                        .Icon(Icons.Plus)
                        .Variant(ButtonVariant.Primary))
                )
                .Add(invoices.Count == 0 
                    ? "No invoices found. Create your first invoice!"
                    : BuildInvoicesList(invoices, client))
        );
    }

    private object BuildAccountsView(ApplicationDbContext context, IClientProvider client)
    {
        var accounts = context.Accounts.OrderBy(a => a.AccountNumber).ToList();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add("Chart of Accounts")
                    .Add(new Button("Add Account", _ => client.Toast("Account creation coming soon!"))
                        .Icon(Icons.Plus)
                        .Variant(ButtonVariant.Primary))
                )
                .Add(accounts.Count == 0 
                    ? "No accounts found. Add your first account!"
                    : BuildAccountsList(accounts, client))
        );
    }

    private object BuildTransactionsView(ApplicationDbContext context, IClientProvider client)
    {
        var transactions = context.Transactions.OrderByDescending(t => t.TransactionDate).ToList();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add("Transactions")
                    .Add(new Button("Add Transaction", _ => client.Toast("Transaction creation coming soon!"))
                        .Icon(Icons.Plus)
                        .Variant(ButtonVariant.Primary))
                )
                .Add(transactions.Count == 0 
                    ? "No transactions found. Add your first transaction!"
                    : BuildTransactionsList(transactions, client))
        );
    }

    private object BuildInvoicesList(List<Invoice> invoices, IClientProvider client)
    {
        var invoiceCards = invoices.Select(invoice => new Card(
            Layout.Horizontal()
                .Gap(12)
                .Padding(12)
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add($"#{invoice.InvoiceNumber}")
                    .Add(invoice.CustomerName)
                    .Add(invoice.IssueDate.ToString("MMM dd, yyyy")))
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add($"${invoice.TotalAmount:N2}")
                    .Add(new Badge(invoice.Status)
                        .Variant(invoice.Status == "Paid" ? BadgeVariant.Success : 
                               invoice.Status == "Overdue" ? BadgeVariant.Destructive : 
                               BadgeVariant.Secondary)))
                .Add(new Button("View", _ => client.Toast($"Viewing invoice {invoice.InvoiceNumber}"))
                    .Small()
                    .Variant(ButtonVariant.Outline))
        ));

        return Layout.Vertical().Gap(8).Add(invoiceCards);
    }

    private object BuildAccountsList(List<Account> accounts, IClientProvider client)
    {
        var accountCards = accounts.Select(account => new Card(
            Layout.Horizontal()
                .Gap(12)
                .Padding(12)
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add($"{account.AccountNumber} - {account.AccountName}")
                    .Add(account.AccountType)
                    .Add(account.Description))
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add($"${account.Balance:N2}")
                    .Add(new Badge(account.IsActive ? "Active" : "Inactive")
                        .Variant(account.IsActive ? BadgeVariant.Success : BadgeVariant.Secondary)))
                .Add(new Button("Edit", _ => client.Toast($"Editing account {account.AccountNumber}"))
                    .Small()
                    .Variant(ButtonVariant.Outline))
        ));

        return Layout.Vertical().Gap(8).Add(accountCards);
    }

    private object BuildTransactionsList(List<Transaction> transactions, IClientProvider client)
    {
        var transactionCards = transactions.Select(transaction => new Card(
            Layout.Horizontal()
                .Gap(12)
                .Padding(12)
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add($"#{transaction.TransactionNumber}")
                    .Add(transaction.Description)
                    .Add(transaction.TransactionDate.ToString("MMM dd, yyyy")))
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add(transaction.DebitAmount > 0 ? $"Debit: ${transaction.DebitAmount:N2}" : "")
                    .Add(transaction.CreditAmount > 0 ? $"Credit: ${transaction.CreditAmount:N2}" : ""))
                .Add(new Button("View", _ => client.Toast($"Viewing transaction {transaction.TransactionNumber}"))
                    .Small()
                    .Variant(ButtonVariant.Outline))
        ));

        return Layout.Vertical().Gap(8).Add(transactionCards);
    }
}
