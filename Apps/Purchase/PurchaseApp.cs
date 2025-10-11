namespace IvyOneBusinessOnePlatform.Apps.Purchase;

[App(icon: Icons.ShoppingBag, title: "Purchase")]
public class PurchaseApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Purchase Management")
                .Add("Manage procurement, purchase orders, and vendor relationships")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Purchase Orders", _ => client.Toast("PO management coming soon!")))
                    .Add(new Button("Vendor Portal", _ => client.Toast("Vendor management coming soon!")))
                    .Add(new Button("Approval Workflow", _ => client.Toast("Approval process coming soon!")))
                )
        );
    }
}
