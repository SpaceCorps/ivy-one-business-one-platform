namespace IvyOneBusinessOnePlatform.Apps.Accounting;

[App(icon: Icons.Percent, title: "Accounting", path: new[] { "Business Operations" })]
public class AccountingApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Accounting System")
                .Add("Manage your financial records, invoices, and reports")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("View Reports", _ => client.Toast("Reports coming soon!")))
                    .Add(new Button("Manage Invoices", _ => client.Toast("Invoice management coming soon!")))
                    .Add(new Button("Financial Dashboard", _ => client.Toast("Financial dashboard coming soon!")))
                )
        );
    }
}
